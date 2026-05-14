namespace final.Application.DTOs
{
    public class VisaLookupRequest
    {
        public string BankCode { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
    }

    public class BankAccountDto
    {
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class VisaLookupResponse
    {
        public string BankName { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public List<BankAccountDto> Accounts { get; set; } = new();
    }

    public class SelectAccountRequest
    {
        public int AccountId { get; set; }
    }

    public class SelectAccountResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    public class VisaTransferRequest
    {
        public string Token { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string ReceiverPhone { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}