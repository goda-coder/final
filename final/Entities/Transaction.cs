using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace final.Entities
{
   

    

    public class Transaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [ForeignKey("SenderId")]
        public virtual ApplicationUser Sender { get; set; } = null!;

        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [ForeignKey("ReceiverId")]
        public virtual ApplicationUser Receiver { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public string? Description { get; set; }

        public TransactionType Type { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        public bool IsFingerprintPayment { get; set; } = false;
        public string? FingerprintDeviceId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public string? FailureReason { get; set; }
    }

    public enum TransactionType
    {
        Transfer,
        Payment,
        Deposit,
        Withdrawal,
        MerchantPayment
    }

    public enum TransactionStatus
    {
        Pending,
        Completed,
        Failed,
        Cancelled,
        Refunded
    }
}
