namespace final.Entities
{
    public class TrustedContact
    {
        public int Id { get; set; }

        // صاحب الحساب
        public int UserId { get; set; }

        // الشخص الموثوق
        public int TrustedUserId { get; set; }

        public string NickName { get; set; } = string.Empty;

        public string TrustedPhone { get; set; } = string.Empty;

        public decimal DailyLimit { get; set; }

        public decimal TotalLimit { get; set; }

        public decimal UsedToday { get; set; } = 0;

        public decimal UsedTotal { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}