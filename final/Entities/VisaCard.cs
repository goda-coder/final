namespace final.Entities
{
    public class VisaCard
    {
        public int Id { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string? UserId { get; set; }  // ✅ Nullable - مش مرتبطة بأي user في البداية
        public virtual ApplicationUser? User { get; set; }
        public int BankId { get; set; }
        public virtual Bank Bank { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime ExpiryDate { get; set; }
        public ICollection<BankAccount> Accounts { get; set; } = new List<BankAccount>();
    }
}