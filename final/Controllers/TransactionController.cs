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
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost("transfer")]
        [Authorize(Roles = "User,Merchant")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var transaction = await _transactionService.CreateTransferAsync(
                    userId!, request.ReceiverPhone, request.Amount, request.Description);

                return Ok(new { Message = "Transfer successful", TransactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("pay-merchant")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> PayMerchant([FromBody] MerchantPaymentRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var transaction = await _transactionService.CreateMerchantPaymentAsync(
                    userId!, request.MerchantId, request.Amount);

                return Ok(new { Message = "Payment successful", TransactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var transactions = await _transactionService.GetUserTransactionsAsync(userId!);
            return Ok(transactions);
        }
    }
}
