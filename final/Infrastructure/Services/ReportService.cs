using final.Application.DTOs;
using final.Entities;
using final.Enums;
using final.Infrastructure.Data;
using final.Interfaces;

using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;

using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace final.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<UserReportSummaryDto> GetUserReportAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");

            var transactions = await _context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .Where(t => t.SenderId == userId || t.ReceiverId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            var items = transactions.Select(t => MapToItem(t, userId)).ToList();

            return new UserReportSummaryDto
            {
                UserId = userId,
                UserName = user.FullName ?? user.UserName ?? "Unknown",
                PhoneNumber = user.PhoneNumber ?? "-",
                CurrentBalance = user.Balance,
                GeneratedAt = DateTime.UtcNow,
                TotalTransactions = transactions.Count,
                TotalSent = transactions
                                        .Where(t => t.SenderId == userId && t.Status == TransactionStatus.Completed)
                                        .Sum(t => t.Amount),
                TotalReceived = transactions
                                        .Where(t => t.ReceiverId == userId && t.SenderId != userId && t.Status == TransactionStatus.Completed)
                                        .Sum(t => t.Amount),
                TotalDeposited = transactions
                                        .Where(t => t.Type == TransactionType.Deposit && t.Status == TransactionStatus.Completed)
                                        .Sum(t => t.Amount),
                CompletedCount = transactions.Count(t => t.Status == TransactionStatus.Completed),
                FailedCount = transactions.Count(t => t.Status == TransactionStatus.Failed),
                CancelledCount = transactions.Count(t => t.Status == TransactionStatus.Cancelled),
                FingerprintPaymentsCount = transactions.Count(t => t.IsFingerprintPayment),
                Transactions = items
            };
        }

        public async Task<byte[]> GenerateUserReportPdfAsync(string userId)
        {
            var report = await GetUserReportAsync(userId);

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Header
            document.Add(new Paragraph("Transaction Report")
                .SetFont(boldFont).SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(30, 80, 160)));

            document.Add(new Paragraph($"Generated: {report.GeneratedAt:dd MMM yyyy  HH:mm} UTC")
                .SetFont(normalFont).SetFontSize(9)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY));

            document.Add(new Paragraph("\n"));

            // User Info
            document.Add(new Paragraph("User Information")
                .SetFont(boldFont).SetFontSize(13)
                .SetFontColor(new DeviceRgb(30, 80, 160)));

            var infoTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 3 }))
                .UseAllAvailableWidth();
            AddInfoRow(infoTable, "Name", report.UserName, boldFont, normalFont);
            AddInfoRow(infoTable, "Phone", report.PhoneNumber, boldFont, normalFont);
            AddInfoRow(infoTable, "Balance", $"{report.CurrentBalance:N2} EGP", boldFont, normalFont);
            document.Add(infoTable);

            document.Add(new Paragraph("\n"));

            // Summary
            document.Add(new Paragraph("Summary")
                .SetFont(boldFont).SetFontSize(13)
                .SetFontColor(new DeviceRgb(30, 80, 160)));

            var summaryTable = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 3, 2 }))
                .UseAllAvailableWidth();
            AddSummaryRow(summaryTable, "Total Transactions", report.TotalTransactions.ToString(),
                                        "Fingerprint Payments", report.FingerprintPaymentsCount.ToString(), boldFont, normalFont);
            AddSummaryRow(summaryTable, "Total Sent", $"{report.TotalSent:N2} EGP",
                                        "Total Received", $"{report.TotalReceived:N2} EGP", boldFont, normalFont);
            AddSummaryRow(summaryTable, "Total Deposited", $"{report.TotalDeposited:N2} EGP",
                                        "Completed", report.CompletedCount.ToString(), boldFont, normalFont);
            AddSummaryRow(summaryTable, "Failed", report.FailedCount.ToString(),
                                        "Cancelled", report.CancelledCount.ToString(), boldFont, normalFont);
            document.Add(summaryTable);

            document.Add(new Paragraph("\n"));

            // Transactions Table
            document.Add(new Paragraph("Transactions")
                .SetFont(boldFont).SetFontSize(13)
                .SetFontColor(new DeviceRgb(30, 80, 160)));

            var txTable = new Table(UnitValue.CreatePercentArray(new float[] { 1.5f, 1.5f, 1, 1.5f, 2, 2.5f }))
                .UseAllAvailableWidth();

            foreach (var h in new[] { "Date", "Type", "Direction", "Amount (EGP)", "Status", "Counterparty" })
            {
                txTable.AddHeaderCell(new Cell()
                    .Add(new Paragraph(h).SetFont(boldFont).SetFontSize(9))
                    .SetBackgroundColor(new DeviceRgb(30, 80, 160))
                    .SetFontColor(ColorConstants.WHITE));
            }

            bool alternate = false;
            foreach (var tx in report.Transactions)
            {
                var bg = alternate ? new DeviceRgb(240, 245, 255) : (Color)ColorConstants.WHITE;

                txTable.AddCell(StyledCell($"{tx.CreatedAt:dd/MM/yy HH:mm}", normalFont, 8, bg));
                txTable.AddCell(StyledCell(tx.Type, normalFont, 8, bg));
                txTable.AddCell(StyledCell(tx.Direction, normalFont, 8,
                    tx.Direction == "Sent" ? new DeviceRgb(255, 220, 220) : new DeviceRgb(210, 255, 210)));
                txTable.AddCell(StyledCell($"{tx.Amount:N2}", normalFont, 8, bg, TextAlignment.RIGHT));
                txTable.AddCell(StatusCell(tx.Status, boldFont, bg));
                txTable.AddCell(StyledCell(tx.CounterpartyName, normalFont, 8, bg));

                alternate = !alternate;
            }

            document.Add(txTable);

            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("This report is system-generated and does not require a signature.")
                .SetFont(normalFont).SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY));

            document.Close();
            return ms.ToArray();
        }

        private static TransactionReportItemDto MapToItem(Transaction t, string userId)
        {
            bool isSender = t.SenderId == userId;
            string counterparty = t.Type == TransactionType.Deposit
                ? "Self (Deposit)"
                : isSender
                    ? (t.Receiver?.FullName ?? t.Receiver?.UserName ?? t.ReceiverId)
                    : (t.Sender?.FullName ?? t.Sender?.UserName ?? t.SenderId);

            return new TransactionReportItemDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                Amount = t.Amount,
                Direction = t.Type == TransactionType.Deposit ? "Deposit"
                                     : isSender ? "Sent" : "Received",
                CounterpartyName = counterparty,
                Description = t.Description,
                IsFingerprintPayment = t.IsFingerprintPayment,
                CreatedAt = t.CreatedAt,
                CompletedAt = t.CompletedAt,
                FailureReason = t.FailureReason
            };
        }

        private static void AddInfoRow(Table table, string label, string value, PdfFont bold, PdfFont normal)
        {
            table.AddCell(new Cell().Add(new Paragraph(label).SetFont(bold).SetFontSize(10))
                .SetBackgroundColor(new DeviceRgb(235, 240, 255)));
            table.AddCell(new Cell().Add(new Paragraph(value).SetFont(normal).SetFontSize(10)));
        }

        private static void AddSummaryRow(Table table, string l1, string v1, string l2, string v2, PdfFont bold, PdfFont normal)
        {
            table.AddCell(new Cell().Add(new Paragraph(l1).SetFont(bold).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(235, 240, 255)));
            table.AddCell(new Cell().Add(new Paragraph(v1).SetFont(normal).SetFontSize(9)));
            table.AddCell(new Cell().Add(new Paragraph(l2).SetFont(bold).SetFontSize(9))
                .SetBackgroundColor(new DeviceRgb(235, 240, 255)));
            table.AddCell(new Cell().Add(new Paragraph(v2).SetFont(normal).SetFontSize(9)));
        }

        private static Cell StyledCell(string text, PdfFont font, float size, Color bg,
            TextAlignment align = TextAlignment.LEFT)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontSize(size).SetTextAlignment(align))
                .SetBackgroundColor(bg);
        }

        private static Cell StatusCell(string status, PdfFont font, Color defaultBg)
        {
            Color color = status switch
            {
                "Completed" => new DeviceRgb(0, 150, 0),
                "Failed" => new DeviceRgb(200, 0, 0),
                "Cancelled" => new DeviceRgb(180, 90, 0),
                _ => new DeviceRgb(80, 80, 80)
            };
            return new Cell()
                .Add(new Paragraph(status).SetFont(font).SetFontSize(8).SetFontColor(color))
                .SetBackgroundColor(defaultBg);
        }
    }
}