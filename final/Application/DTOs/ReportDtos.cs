namespace final.Application.DTOs
{
    public class UserReportSummaryDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int TotalTransactions { get; set; }
        public decimal TotalSent { get; set; }
        public decimal TotalReceived { get; set; }
        public decimal TotalDeposited { get; set; }
        public int CompletedCount { get; set; }
        public int FailedCount { get; set; }
        public int CancelledCount { get; set; }
        public int FingerprintPaymentsCount { get; set; }
        public List<TransactionReportItemDto> Transactions { get; set; } = new();
    }

    public class TransactionReportItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Direction { get; set; } = string.Empty;
        public string CounterpartyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsFingerprintPayment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? FailureReason { get; set; }
    }
}