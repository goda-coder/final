using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using final.Entities;
using final.Enums;
using final.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using AppDTOs = final.Application.DTOs;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AppDTOs.RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.FullName = request.FullName.Trim();

            if (string.IsNullOrWhiteSpace(request.Email))
            {
                request.Email = null;
            }
            else
            {
                request.Email = request.Email.Trim();
                if (!new EmailAddressAttribute().IsValid(request.Email))
                    return BadRequest(new { Message = "Invalid email format" });
            }

            var phone = request.PhoneNumber.Trim();

            var user = new ApplicationUser
            {
                UserName = phone,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = phone
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var response = await GenerateAuthResponse(user);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AppDTOs.LoginRequest request)
        {
            var phone = request.PhoneNumber.Trim();
            var user = await _userManager.Users
                .FirstOrDefaultAsync(x => x.PhoneNumber == phone);

            if (user == null || !user.IsActive)
                return Unauthorized(new { Message = "Invalid credentials" });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                return Unauthorized(new { Message = "Invalid credentials" });

            var response = await GenerateAuthResponse(user);
            return Ok(response);
        }

        [HttpPost("set-role")]
        [Authorize]
        public async Task<IActionResult> SetRole([FromBody] AppDTOs.SetRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                return NotFound(new { Message = "User not found" });

            if (request.Role == UserRole.None)
                return BadRequest(new { Message = "Invalid role" });

            user.Role = request.Role;

            if (request.Role == UserRole.Merchant)
                user.MerchantStatus = MerchantStatus.Pending;
            else
                user.MerchantStatus = null;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, request.Role.ToString());

            await _userManager.UpdateAsync(user);

            var response = await GenerateAuthResponse(user);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] AppDTOs.RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return Unauthorized(new { Message = "Invalid or expired refresh token" });

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            _dbContext.Entry(user).Property(x => x.RefreshToken).IsModified = true;
            _dbContext.Entry(user).Property(x => x.RefreshTokenExpiryTime).IsModified = true;
            await _dbContext.SaveChangesAsync();

            var response = await GenerateAuthResponse(user);
            return Ok(response);
        }

        private async Task<AppDTOs.AuthResponse> GenerateAuthResponse(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim("PhoneNumber", user.PhoneNumber ?? "")
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim(ClaimTypes.Email, user.Email));

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                Convert.ToDouble(_configuration["Jwt:RefreshTokenDurationInDays"])
            );

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiry;
            _dbContext.Entry(user).Property(x => x.RefreshToken).IsModified = true;
            _dbContext.Entry(user).Property(x => x.RefreshTokenExpiryTime).IsModified = true;
            await _dbContext.SaveChangesAsync();

            return new AppDTOs.AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expires,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = refreshTokenExpiry,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber ?? "",
                FullName = user.FullName ?? "",
                Role = user.Role
            };
        }
    }
}
