// Entities/Dispute.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace final.Entities
{
    public class Dispute
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // المستخدم اللي فتح النزاع
        [Required]
        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        // العملية المشتكي عليها
        [Required]
        public Guid TransactionId { get; set; }
        [ForeignKey("TransactionId")]
        public virtual Transaction Transaction { get; set; } = null!;

        [Required]
        public string Reason { get; set; } = string.Empty; // سبب الشكوى

        public DisputeStatus Status { get; set; } = DisputeStatus.Open;

        // Admin اللي بيراجع
        public string? ReviewedByAdminId { get; set; }
        [ForeignKey("ReviewedByAdminId")]
        public virtual ApplicationUser? ReviewedByAdmin { get; set; }

        public string? AdminNote { get; set; } // تعليق الأدمن

        public bool RefundIssued { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
    }

    public enum DisputeStatus
    {
        Open,
        InReview,
        Resolved,
        Rejected
    }
}