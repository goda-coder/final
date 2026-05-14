using System.ComponentModel.DataAnnotations;
using final.Enums;

namespace final.Application.DTOs
{
    public class UserProfileDto
    {
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class UpdateProfileDto
    {
        [Required]
        [MinLength(3)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public Gender? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }

        // تغيير الباسورد (اختياري)
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}