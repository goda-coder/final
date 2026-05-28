// Controllers/DisputeController.cs
using final.Application.DTOs;
using final.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DisputeController : ControllerBase
    {
        private readonly IDisputeService _disputeService;

        public DisputeController(IDisputeService disputeService)
        {
            _disputeService = disputeService;
        }

        private string UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value;

        // ── User Endpoints ───────────────────────────────

        // POST api/dispute  → فتح نزاع
        [HttpPost]
        [Authorize(Roles = "User,Merchant")]
        public async Task<IActionResult> OpenDispute([FromBody] CreateDisputeRequest request)
        {
            try
            {
                var result = await _disputeService.OpenDisputeAsync(UserId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { Message = ex.Message }); }
        }

        // GET api/dispute/my  → نزاعاتي
        [HttpGet("my")]
        public async Task<IActionResult> GetMyDisputes()
        {
            var result = await _disputeService.GetMyDisputesAsync(UserId);
            return Ok(result);
        }

        // ── Admin Endpoints ──────────────────────────────

        // GET api/dispute  → كل النزاعات
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllDisputes()
        {
            var result = await _disputeService.GetAllDisputesAsync();
            return Ok(result);
        }

        // PUT api/dispute/{id}/review  → الأدمن يبدأ المراجعة
        [HttpPut("{id}/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetInReview(Guid id)
        {
            try
            {
                var result = await _disputeService.SetInReviewAsync(id, UserId);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { Message = ex.Message }); }
        }

        // PUT api/dispute/{id}/resolve  → الأدمن يحل النزاع
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveDispute(Guid id, [FromBody] ResolveDisputeRequest request)
        {
            try
            {
                var result = await _disputeService.ResolveDisputeAsync(id, UserId, request);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { Message = ex.Message }); }
        }
    }
}