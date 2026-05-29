using System.Security.Claims;
using final.Entities;
using final.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AppDTOs = final.Application.DTOs;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MerchantController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public MerchantController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] AppDTOs.MerchantSetupRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
                return NotFound(new { Message = "User not found" });

            var isMerchant = await _userManager.IsInRoleAsync(user, "Merchant");
            if (!isMerchant)
                return Forbid();

            user.MerchantName = request.MerchantName;
            user.CommercialRegistration = request.CommercialRegistration;
            user.TaxNumber = request.TaxNumber;

            if (user.MerchantStatus == MerchantStatus.Pending || user.MerchantStatus == null)
                user.MerchantStatus = MerchantStatus.Pending;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Merchant profile updated successfully" });
        }
    }
}
