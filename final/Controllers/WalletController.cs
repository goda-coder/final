using final.Entities;
using final.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace final.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public WalletController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);
        return Ok(new { Balance = user?.Balance ?? 0 });
    }

    [HttpPost("deposit")]
    [Authorize(Roles = "User,Merchant")]
    public async Task<IActionResult> Deposit([FromBody] decimal amount)
    {
        if (amount <= 0) return BadRequest("Invalid amount");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var user = await _userManager.FindByIdAsync(userId!);

        user!.Balance += amount;
        await _userManager.UpdateAsync(user);

        _context.Transactions.Add(new Transaction
        {
            SenderId = userId!,
            ReceiverId = userId!,
            Amount = amount,
            Type = TransactionType.Deposit,
            Status = TransactionStatus.Completed,
            CompletedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Deposit successful", NewBalance = user.Balance });
    }
}