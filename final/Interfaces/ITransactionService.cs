using final.Entities;

namespace final.Interfaces
{
    

   

    public interface ITransactionService
    {
        Task<Transaction> CreateTransferAsync(string senderId, string receiverPhone, decimal amount, string? description, bool useFingerprint = false, string? deviceId = null);
        Task<Transaction> CreateMerchantPaymentAsync(string userId, string merchantId, decimal amount, bool useFingerprint = false, string? deviceId = null);
        Task<Transaction?> GetTransactionByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId);
        Task<bool> ConfirmTransactionAsync(Guid transactionId);
        Task<bool> CancelTransactionAsync(Guid transactionId);
    }
}
