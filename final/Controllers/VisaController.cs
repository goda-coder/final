using final.Application.DTOs;
using final.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VisaController : ControllerBase
    {
        private readonly IVisaService _visaService;

        public VisaController(IVisaService visaService)
        {
            _visaService = visaService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        [HttpPost("lookup")]
        public async Task<IActionResult> Lookup([FromBody] VisaLookupRequest request)
        {
            try
            {
                var result = await _visaService.LookupVisaAsync(
                    GetUserId(),
                    request.BankCode,
                    request.CardNumber,
                    request.Pin);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("select-account")]
        public async Task<IActionResult> SelectAccount([FromBody] SelectAccountRequest request)
        {
            try
            {
                var result = await _visaService.SelectAccountAsync(GetUserId(), request.AccountId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] VisaTransferRequest request)
        {
            try
            {
                await _visaService.TransferByTokenAsync(
                    GetUserId(),
                    request.Token,
                    request.Amount,
                    request.ReceiverPhone,
                    request.Description);
                return Ok(new { message = "تم التحويل بنجاح" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}