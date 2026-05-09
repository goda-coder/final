


using final.Enums;

using Microsoft.AspNetCore.Identity;
using System.Transactions;
namespace final.Entities
{
    

    

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? NationalId { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsFingerprintEnabled { get; set; } = false;
        public string? FingerprintTemplate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public decimal Balance { get; set; } = 0;

        // للتجار فقط
        public string? MerchantName { get; set; }
        public string? CommercialRegistration { get; set; }
        public string? TaxNumber { get; set; }
        public MerchantStatus? MerchantStatus { get; set; }

        public virtual ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
        public virtual ICollection<FingerprintLog> FingerprintLogs { get; set; } = new List<FingerprintLog>();
    }

    public enum MerchantStatus
    {
        Pending,
        Approved,
        Rejected,
        Suspended
    }
}
