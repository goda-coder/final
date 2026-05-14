using PaymentSystem.Application.DTOs;

namespace PaymentSystem.Infrastructure.Interfaces
{
    public interface INotificationService
    {
        Task SendDebitNotificationAsync(string userId, decimal amount, decimal newBalance);
        Task SendTransferNotificationAsync(string userId, decimal amount, decimal newBalance, string senderPhone);
        Task SendFraudAlertAsync(string userId, decimal amount, string reason);
        Task<List<NotificationDto>> GetUserNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
    }
}