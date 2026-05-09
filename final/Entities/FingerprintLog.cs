using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace final.Entities
{
   

   

    public class FingerprintLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public FingerprintAction Action { get; set; }
        public bool IsSuccess { get; set; }
        public string? DeviceId { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? IpAddress { get; set; }
    }

    public enum FingerprintAction
    {
        Enrollment,
        Verification,
        Payment,
        Login
    }
}
