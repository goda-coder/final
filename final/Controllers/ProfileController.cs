using final.Entities;
using final.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AppDTOs = final.Application.DTOs;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // ================= GET PROFILE =================
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                return NotFound(new { Message = "User not found" });

            var profile = new AppDTOs.UserProfileDto
            {
                Name = user.FullName,
                Phone = user.PhoneNumber ?? "",
                Email = user.Email,
                Address = user.Address,
                Occupation = user.Occupation,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth
            };

            return Ok(profile);
        }

        // ================= UPDATE PROFILE =================
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] AppDTOs.UpdateProfileDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                return NotFound(new { Message = "User not found" });

            // ✅ التحقق إن الفون unique (مش موجود عند حد تاني)
            if (user.PhoneNumber != request.Phone)
            {
                var phoneExists = await _userManager.Users
                    .AnyAsync(u => u.PhoneNumber == request.Phone && u.Id != userId);

                if (phoneExists)
                    return BadRequest(new { Message = "Phone number already in use" });
            }

            // ✅ التحقق إن الإيميل unique (مش موجود عند حد تاني)
            if (!string.IsNullOrEmpty(request.Email) && user.Email != request.Email)
            {
                var emailExists = await _userManager.Users
                    .AnyAsync(u => u.Email == request.Email && u.Id != userId);

                if (emailExists)
                    return BadRequest(new { Message = "Email already in use" });

                user.Email = request.Email;
                user.UserName = request.Email;
            }

            // ✅ تحديث البيانات
            user.FullName = request.Name;
            user.PhoneNumber = request.Phone;
            user.Address = request.Address;
            user.Occupation = request.Occupation;
            user.Gender = request.Gender;
            user.DateOfBirth = request.DateOfBirth;

            // ✅ تغيير الباسورد لو اتبعت
            if (!string.IsNullOrEmpty(request.Password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, token, request.Password);

                if (!passwordResult.Succeeded)
                    return BadRequest(passwordResult.Errors);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}