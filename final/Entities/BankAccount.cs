namespace final.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty; // Savings, Current, etc
        public decimal Balance { get; set; }
        public string Currency { get; set; } = "EGP";
        public int VisaCardId { get; set; }
        public virtual VisaCard VisaCard { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}