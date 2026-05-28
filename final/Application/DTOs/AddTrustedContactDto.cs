namespace final.Application.DTOs
{
    public class AddTrustedContactDto
    {
        public int UserId { get; set; }

        public int TrustedUserId { get; set; }

        public string NickName { get; set; } = string.Empty;

        public string TrustedPhone { get; set; } = string.Empty;

        public decimal DailyLimit { get; set; }

        public decimal TotalLimit { get; set; }
    }

    public class QuickTransferDto
    {
        public int UserId { get; set; }

        public string NickName { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}