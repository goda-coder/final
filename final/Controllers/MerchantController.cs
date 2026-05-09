using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using final.Entities;
using final.Infrastructure.Data;

using final.Enums;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
namespace final.Controllers
{
   

    

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class MerchantController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public MerchantController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var merchant = await _userManager.FindByIdAsync(userId!);

            return Ok(new
            {
                merchant!.MerchantName,
                merchant.CommercialRegistration,
                merchant.TaxNumber,
                merchant.MerchantStatus,
                merchant.Balance,
                merchant.PhoneNumber
            });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetMerchantTransactions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var transactions = await _context.Transactions
                .Where(t => t.ReceiverId == userId && t.Type == TransactionType.MerchantPayment)
                .Include(t => t.Sender)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(transactions.Select(t => new
            {
                t.Id,
                t.Amount,
                CustomerName = t.Sender?.FullName,
                t.CreatedAt,
                t.Status
            }));
        }

        [HttpGet("qr-code")]
        public async Task<IActionResult> GetQrCodeData()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var merchant = await _userManager.FindByIdAsync(userId!);

            // بيانات الـ QR Code اللي العميل هيscanها
            var qrData = new
            {
                MerchantId = merchant!.Id,
                MerchantName = merchant.MerchantName,
                MerchantPhone = merchant.PhoneNumber
            };

            return Ok(qrData);
        }
    }
}
