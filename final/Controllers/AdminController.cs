using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using final.Entities;
using final.Enums;
using final.Infrastructure.Data;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;
namespace final.Controllers
{
    

    

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            return Ok(users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.Role,
                u.Balance,
                u.IsActive,
                u.CreatedAt
            }));
        }

        [HttpGet("merchants/pending")]
        public async Task<IActionResult> GetPendingMerchants()
        {
            var merchants = await _userManager.Users
                .Where(u => u.Role == UserRole.Merchant && u.MerchantStatus == MerchantStatus.Pending)
                .ToListAsync();
            return Ok(merchants);
        }

        [HttpPost("merchants/{merchantId}/approve")]
        public async Task<IActionResult> ApproveMerchant(string merchantId)
        {
            var merchant = await _userManager.FindByIdAsync(merchantId);
            if (merchant == null || merchant.Role != UserRole.Merchant)
                return NotFound();

            merchant.MerchantStatus = MerchantStatus.Approved;
            await _userManager.UpdateAsync(merchant);
            return Ok(new { Message = "Merchant approved successfully" });
        }

        [HttpPost("users/{userId}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);
            return Ok(new { Message = $"User {(user.IsActive ? "activated" : "deactivated")}" });
        }

        [HttpGet("transactions/all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Sender)
                .Include(t => t.Receiver)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(transactions);
        }

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalUsers = await _userManager.Users.CountAsync();
            var totalMerchants = await _userManager.Users.CountAsync(u => u.Role == UserRole.Merchant);
            var totalTransactions = await _context.Transactions.CountAsync();
            var totalAmount = await _context.Transactions
                .Where(t => t.Status == TransactionStatus.Completed)
                .SumAsync(t => t.Amount);

            return Ok(new
            {
                TotalUsers = totalUsers,
                TotalMerchants = totalMerchants,
                TotalTransactions = totalTransactions,
                TotalTransactionAmount = totalAmount
            });
        }
    }
}
