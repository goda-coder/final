// Infrastructure/Services/DisputeService.cs
using final.Application.DTOs;
using final.Entities;
using final.Infrastructure.Data;
using final.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace final.Infrastructure.Services
{
    public class DisputeService : IDisputeService
    {
        private readonly ApplicationDbContext _context;

        public DisputeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DisputeResponse> OpenDisputeAsync(string userId, CreateDisputeRequest request)
        {
            // تأكد إن العملية موجودة وتخص المستخدم
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Id == request.TransactionId
                                       && (t.SenderId == userId || t.ReceiverId == userId))
                ?? throw new InvalidOperationException("Transaction not found or not yours");

            // منع فتح نزاع مكرر على نفس العملية
            var exists = await _context.Disputes
                .AnyAsync(d => d.TransactionId == request.TransactionId
                            && d.UserId == userId
                            && d.Status != DisputeStatus.Rejected);
            if (exists)
                throw new InvalidOperationException("A dispute already exists for this transaction");

            var dispute = new Dispute
            {
                UserId = userId,
                TransactionId = request.TransactionId,
                Reason = request.Reason,
                Status = DisputeStatus.Open
            };

            _context.Disputes.Add(dispute);
            await _context.SaveChangesAsync();

            return await MapToResponse(dispute.Id);
        }

        public async Task<IEnumerable<DisputeResponse>> GetMyDisputesAsync(string userId)
        {
            var disputes = await _context.Disputes
                .Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.Id)
                .ToListAsync();

            var result = new List<DisputeResponse>();
            foreach (var id in disputes)
                result.Add(await MapToResponse(id));

            return result;
        }

        public async Task<IEnumerable<DisputeResponse>> GetAllDisputesAsync()
        {
            var disputes = await _context.Disputes
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => d.Id)
                .ToListAsync();

            var result = new List<DisputeResponse>();
            foreach (var id in disputes)
                result.Add(await MapToResponse(id));

            return result;
        }

        public async Task<DisputeResponse> SetInReviewAsync(Guid disputeId, string adminId)
        {
            var dispute = await _context.Disputes.FindAsync(disputeId)
                ?? throw new InvalidOperationException("Dispute not found");

            if (dispute.Status != DisputeStatus.Open)
                throw new InvalidOperationException("Dispute is not Open");

            dispute.Status = DisputeStatus.InReview;
            dispute.ReviewedByAdminId = adminId;

            await _context.SaveChangesAsync();
            return await MapToResponse(disputeId);
        }

        public async Task<DisputeResponse> ResolveDisputeAsync(Guid disputeId, string adminId, ResolveDisputeRequest request)
        {
            var dispute = await _context.Disputes
                .Include(d => d.Transaction)
                .FirstOrDefaultAsync(d => d.Id == disputeId)
                ?? throw new InvalidOperationException("Dispute not found");

            if (dispute.Status != DisputeStatus.InReview)
                throw new InvalidOperationException("Dispute must be InReview before resolving");

            // لو Approve وعايز Refund
            if (request.Approve && request.IssueRefund)
            {
                var transaction = dispute.Transaction;

                // رجّع الفلوس للـ Sender
                var sender = await _context.Users.FindAsync(transaction.SenderId)
                    ?? throw new InvalidOperationException("Sender not found");

                var receiver = await _context.Users.FindAsync(transaction.ReceiverId)
                    ?? throw new InvalidOperationException("Receiver not found");

                if (receiver.Balance < transaction.Amount)
                    throw new InvalidOperationException("Receiver has insufficient balance for refund");

                sender.Balance += transaction.Amount;
                receiver.Balance -= transaction.Amount;

                // سجّل Refund transaction
                _context.Transactions.Add(new Transaction
                {
                    SenderId = transaction.ReceiverId,
                    ReceiverId = transaction.SenderId,
                    Amount = transaction.Amount,
                    Description = $"Refund for dispute {dispute.Id}",
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                });

                dispute.RefundIssued = true;
            }

            dispute.Status = request.Approve ? DisputeStatus.Resolved : DisputeStatus.Rejected;
            dispute.AdminNote = request.AdminNote;
            dispute.ReviewedByAdminId = adminId;
            dispute.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await MapToResponse(disputeId);
        }

        // ── Helper ──────────────────────────────────────
        private async Task<DisputeResponse> MapToResponse(Guid disputeId)
        {
            var d = await _context.Disputes
                .Include(d => d.User)
                .Include(d => d.Transaction)
                .FirstOrDefaultAsync(d => d.Id == disputeId)
                ?? throw new InvalidOperationException("Dispute not found");

            return new DisputeResponse
            {
                Id = d.Id,
                TransactionId = d.TransactionId,
                TransactionAmount = d.Transaction.Amount,
                Reason = d.Reason,
                Status = d.Status.ToString(),
                AdminNote = d.AdminNote,
                RefundIssued = d.RefundIssued,
                UserName = d.User?.FullName ?? d.User?.UserName ?? d.UserId,
                CreatedAt = d.CreatedAt,
                ResolvedAt = d.ResolvedAt
            };
        }
    }
}