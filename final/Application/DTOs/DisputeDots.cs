// Application/DTOs/DisputeDtos.cs
namespace final.Application.DTOs
{
    // المستخدم بيفتح نزاع
    public class CreateDisputeRequest
    {
        public Guid TransactionId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    // الأدمن بيحل النزاع
    public class ResolveDisputeRequest
    {
        public bool Approve { get; set; }      // true = Resolved, false = Rejected
        public bool IssueRefund { get; set; }  // true = رد الفلوس
        public string? AdminNote { get; set; }
    }

    // الـ Response
    public class DisputeResponse
    {
        public Guid Id { get; set; }
        public Guid TransactionId { get; set; }
        public decimal TransactionAmount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public bool RefundIssued { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}