namespace final.Entities
{
    public class BankToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        public int BankAccountId { get; set; }
        public virtual BankAccount BankAccount { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(15);
        public bool IsUsed { get; set; } = false;
    }
}