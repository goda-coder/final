using final.Enums;
using Microsoft.AspNetCore.Identity;

namespace final.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.None;
        public bool IsFingerprintEnabled { get; set; } = false;
        public string? FingerprintTemplate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public decimal Balance { get; set; } = 0;

        // Profile
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // Merchant
        public string? MerchantName { get; set; }
        public string? CommercialRegistration { get; set; }
        public string? TaxNumber { get; set; }
        public MerchantStatus? MerchantStatus { get; set; }

        // Refresh Token
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
        public ICollection<FingerprintLog> FingerprintLogs { get; set; } = new List<FingerprintLog>();
    }
}