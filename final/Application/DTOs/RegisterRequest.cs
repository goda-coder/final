using System.ComponentModel.DataAnnotations;
using final.Enums;

namespace final.Application.DTOs
{
    // ✅ أضفنا RegisterRequest
    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? NationalId { get; set; }

        public UserRole Role { get; set; } = UserRole.User;

        // Merchant fields (اختيارية)
        public string? MerchantName { get; set; }
        public string? CommercialRegistration { get; set; }
        public string? TaxNumber { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class FingerprintPaymentRequest
    {
        [Required]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public byte[] FingerprintData { get; set; } = Array.Empty<byte>();

        [Required]
        public decimal Amount { get; set; }

        public string? ReceiverPhone { get; set; }
        public string? MerchantId { get; set; }
        public string? Description { get; set; }
    }
}