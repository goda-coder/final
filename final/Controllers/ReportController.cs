using final.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace final.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyReport()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var report = await _reportService.GetUserReportAsync(userId);
                return Ok(report);
            }
            catch (InvalidOperationException ex) { return NotFound(new { Message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Message = ex.Message }); }
        }

        [HttpGet("me/pdf")]
        public async Task<IActionResult> GetMyReportPdf()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                var pdfBytes = await _reportService.GenerateUserReportPdfAsync(userId);
                var fileName = $"Report_{DateTime.UtcNow:yyyyMMdd_HHmm}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (InvalidOperationException ex) { return NotFound(new { Message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { Message = ex.Message }); }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserReport(string userId)
        {
            try
            {
                var report = await _reportService.GetUserReportAsync(userId);
                return Ok(report);
            }
            catch (InvalidOperationException ex) { return NotFound(new { Message = ex.Message }); }
        }

        [HttpGet("user/{userId}/pdf")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserReportPdf(string userId)
        {
            try
            {
                var pdfBytes = await _reportService.GenerateUserReportPdfAsync(userId);
                return File(pdfBytes, "application/pdf", $"Report_{userId}_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (InvalidOperationException ex) { return NotFound(new { Message = ex.Message }); }
        }
    }
}