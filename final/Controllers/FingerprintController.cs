using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using final.Application.DTOs;
using final.Interfaces;


using Microsoft.AspNetCore.Authorization;

namespace final.Controllers
{
  

    

    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FingerprintController : ControllerBase
    {
        private readonly IFingerprintService _fingerprintService;
        private readonly ITransactionService _transactionService;
        private readonly ILogger<FingerprintController> _logger;

        public FingerprintController(
            IFingerprintService fingerprintService,
            ITransactionService transactionService,
            ILogger<FingerprintController> logger)
        {
            _fingerprintService = fingerprintService;
            _transactionService = transactionService;
            _logger = logger;
        }

        // 1. تسجيل البصمة (Enrollment)
        [HttpPost("enroll")]
        public async Task<IActionResult> EnrollFingerprint([FromBody] FingerprintPaymentRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                var success = await _fingerprintService.EnrollFingerprintAsync(
                    userId!, request.FingerprintData, request.DeviceId);

                if (success)
                    return Ok(new { Message = "Fingerprint enrolled successfully" });

                return BadRequest(new { Message = "Failed to enroll fingerprint" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling fingerprint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // 2. دفع بالبصمة
        [HttpPost("pay")]
        public async Task<IActionResult> FingerprintPayment([FromBody] FingerprintPaymentRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // 1. Verify البصمة الأول
                var isVerified = await _fingerprintService.VerifyFingerprintAsync(
                    userId!, request.FingerprintData, request.DeviceId);

                if (!isVerified)
                    return Unauthorized(new { Message = "Fingerprint verification failed" });

                // 2. لو نجحت البصمة، نعمل التحويل
                final.Entities.Transaction transaction;
                if (!string.IsNullOrEmpty(request.MerchantId))
                {
                    transaction = await _transactionService.CreateMerchantPaymentAsync(
                        userId!, request.MerchantId, request.Amount, true, request.DeviceId);
                }
                else
                {
                    transaction = await _transactionService.CreateTransferAsync(
                        userId!, request.ReceiverPhone!, request.Amount,
                        request.Description, true, request.DeviceId);
                }

                return Ok(new
                {
                    Message = "Payment successful via fingerprint",
                    TransactionId = transaction.Id,
                    Amount = request.Amount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing fingerprint payment");
                return BadRequest(new { Message = ex.Message });
            }
        }

        // 3. تفعيل/تعطيل الدفع بالبصمة
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleFingerprint()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isEnabled = await _fingerprintService.IsFingerprintEnabledAsync(userId!);

            return Ok(new { IsFingerprintEnabled = isEnabled });
        }
    }
}
