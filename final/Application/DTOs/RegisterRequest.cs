using System.ComponentModel.DataAnnotations;
using final.Enums;

namespace final.Application.DTOs
{
    public class RegisterRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
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
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiresAt { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class SetRoleRequest
    {
        [Required]
        public UserRole Role { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class MerchantSetupRequest
    {
        [Required]
        public string MerchantName { get; set; } = string.Empty;

        [Required]
        public string CommercialRegistration { get; set; } = string.Empty;

        [Required]
        public string TaxNumber { get; set; } = string.Empty;
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