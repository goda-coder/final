using final.Infrastructure.Data;
using final.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PaymentSystem.Application.DTOs;
using PaymentSystem.Entities;
using PaymentSystem.Enums;
using PaymentSystem.Infrastructure.Interfaces;

namespace PaymentSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ApplicationDbContext _db;

        public NotificationService(IHubContext<NotificationHub> hubContext, ApplicationDbContext db)
        {
            _hubContext = hubContext;
            _db = db;
        }

        public async Task SendDebitNotificationAsync(string userId, decimal amount, decimal newBalance)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.Debit,
                Title = "تم الخصم",
                Message = $"تم خصم {amount:F2} جنيه من حسابك. رصيدك الحالي: {newBalance:F2} جنيه",
                Amount = amount,
                NewBalance = newBalance
            };
            await SaveAndSendAsync(userId, notification);
        }

        public async Task SendTransferNotificationAsync(string userId, decimal amount, decimal newBalance, string senderPhone)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.Transfer,
                Title = "تم استقبال تحويل",
                Message = $"تم تحويل {amount:F2} جنيه إلى حسابك من {senderPhone}. رصيدك الحالي: {newBalance:F2} جنيه",
                Amount = amount,
                NewBalance = newBalance,
                SenderPhone = senderPhone
            };
            await SaveAndSendAsync(userId, notification);
        }

        public async Task SendFraudAlertAsync(string userId, decimal amount, string reason)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = NotificationType.FraudAlert,
                Title = "⚠️ تحذير: نشاط مشبوه",
                Message = $"تم رصد نشاط مشبوه على حسابك بمبلغ {amount:F2} جنيه. السبب: {reason}",
                Amount = amount
            };
            await SaveAndSendAsync(userId, notification);
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(string userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => MapToDto(n))
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }

        private async Task SaveAndSendAsync(string userId, Notification notification)
        {
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            var dto = MapToDto(notification);
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", dto);
        }

        private static NotificationDto MapToDto(Notification n) => new()
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Title = n.Title,
            Message = n.Message,
            Amount = n.Amount,
            NewBalance = n.NewBalance,
            SenderPhone = n.SenderPhone,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}