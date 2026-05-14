using final.Application.DTOs;

namespace final.Interfaces
{
    public interface IVisaService
    {
        Task<VisaLookupResponse> LookupVisaAsync(string userId, string bankCode, string cardNumber, string pin);
        Task<SelectAccountResponse> SelectAccountAsync(string userId, int accountId);
        Task<bool> TransferByTokenAsync(string userId, string token, decimal amount, string receiverPhone, string? description);
    }
}