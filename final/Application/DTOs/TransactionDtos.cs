namespace final.Application.DTOs
{
   

    public class TransferRequest
    {
        public string ReceiverPhone { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class MerchantPaymentRequest
    {
        public string MerchantId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsFingerprintPayment { get; set; }
    }
}
