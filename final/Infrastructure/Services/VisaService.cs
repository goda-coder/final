using final.Application.DTOs;
using final.Entities;
using final.Infrastructure.Data;
using final.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace final.Infrastructure.Services
{
    public class VisaService : IVisaService
    {
        private readonly ApplicationDbContext _context;

        public VisaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<VisaLookupResponse> LookupVisaAsync(string userId, string bankCode, string cardNumber, string pin)
        {
            var bank = await _context.Banks
                .FirstOrDefaultAsync(b => b.Code == bankCode && b.IsActive);

            if (bank == null)
                throw new InvalidOperationException("البنك مش موجود");

            var visa = await _context.VisaCards
                .Include(v => v.Accounts)
                .FirstOrDefaultAsync(v =>
                    v.CardNumber == cardNumber &&
                    v.BankId == bank.Id &&
                    v.IsActive &&
                    v.ExpiryDate > DateTime.UtcNow);

            if (visa == null)
                throw new InvalidOperationException("رقم الفيزا غلط أو منتهية");

            if (visa.Pin != pin)
                throw new InvalidOperationException("PIN غلط");

            // ✅ لو الفيزا مش مرتبطة بأي user اربطها بالـ user الحالي
            if (visa.UserId == null)
            {
                visa.UserId = userId;
                await _context.SaveChangesAsync();
            }
            // ✅ لو مرتبطة بـ user تاني ارفض
            else if (visa.UserId != userId)
            {
                throw new InvalidOperationException("الفيزا دي مش بتاعتك");
            }

            return new VisaLookupResponse
            {
                BankName = bank.Name,
                BankCode = bank.Code,
                Accounts = visa.Accounts.Where(a => a.IsActive).Select(a => new BankAccountDto
                {
                    AccountId = a.Id,
                    AccountNumber = a.AccountNumber,
                    AccountType = a.AccountType,
                    Balance = a.Balance,
                    Currency = a.Currency
                }).ToList()
            };
        }

        public async Task<SelectAccountResponse> SelectAccountAsync(string userId, int accountId)
        {
            var account = await _context.BankAccounts
                .Include(a => a.VisaCard)
                .FirstOrDefaultAsync(a => a.Id == accountId && a.VisaCard.UserId == userId && a.IsActive);

            if (account == null)
                throw new InvalidOperationException("الحساب مش موجود");

            // بنمسح أي Token قديم للـ user ده
            var oldTokens = await _context.BankTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();
            _context.BankTokens.RemoveRange(oldTokens);

            var token = new BankToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = userId,
                BankAccountId = accountId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };

            _context.BankTokens.Add(token);
            await _context.SaveChangesAsync();

            return new SelectAccountResponse
            {
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            };
        }

        public async Task<bool> TransferByTokenAsync(string userId, string token, decimal amount, string receiverPhone, string? description)
        {
            var bankToken = await _context.BankTokens
                .Include(t => t.BankAccount)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    t.UserId == userId &&
                    !t.IsUsed &&
                    t.ExpiresAt > DateTime.UtcNow);

            if (bankToken == null)
                throw new InvalidOperationException("Token غلط أو منتهي");

            if (bankToken.BankAccount.Balance < amount)
                throw new InvalidOperationException("الرصيد مش كافي");

            var receiver = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == receiverPhone);

            if (receiver == null)
                throw new InvalidOperationException("المستقبل مش موجود");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bankToken.BankAccount.Balance -= amount;
                receiver.Balance += amount;
                bankToken.IsUsed = true;

                _context.Transactions.Add(new Transaction
                {
                    SenderId = userId,
                    ReceiverId = receiver.Id,
                    Amount = amount,
                    Description = description ?? "تحويل من حساب بنكي",
                    Type = TransactionType.Transfer,
                    Status = TransactionStatus.Completed,
                    CompletedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}