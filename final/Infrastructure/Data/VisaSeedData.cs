using final.Entities;
using Microsoft.EntityFrameworkCore;

namespace final.Infrastructure.Data
{
    public static class VisaSeedData
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Banks.AnyAsync()) return;

            var banks = new List<Bank>
            {
                new Bank { Name = "البنك الأهلي المصري", Code = "NBE", LogoUrl = "/logos/nbe.png", IsActive = true },
                new Bank { Name = "بنك مصر", Code = "BOM", LogoUrl = "/logos/bom.png", IsActive = true },
                new Bank { Name = "بنك CIB", Code = "CIB", LogoUrl = "/logos/cib.png", IsActive = true },
                new Bank { Name = "بنك QNB", Code = "QNB", LogoUrl = "/logos/qnb.png", IsActive = true },
                new Bank { Name = "بنك ALEX", Code = "ALEX", LogoUrl = "/logos/alex.png", IsActive = true },
            };

            await context.Banks.AddRangeAsync(banks);
            await context.SaveChangesAsync();

            var nbe = banks.First(b => b.Code == "NBE");
            var cib = banks.First(b => b.Code == "CIB");
            var qnb = banks.First(b => b.Code == "QNB");

            var visaCards = new List<VisaCard>
            {
                // فيزا 1 - NBE
                new VisaCard
                {
                    CardNumber = "4111111111111111",
                    Pin = "1234",
                    UserId = null,  // ✅ مش مرتبطة بأي user
                    BankId = nbe.Id,
                    IsActive = true,
                    ExpiryDate = new DateTime(2027, 12, 31),
                    Accounts = new List<BankAccount>
                    {
                        new BankAccount { AccountNumber = "NBE-ACC-001", AccountType = "Savings", Balance = 15000, Currency = "EGP", IsActive = true },
                        new BankAccount { AccountNumber = "NBE-ACC-002", AccountType = "Current", Balance = 32000, Currency = "EGP", IsActive = true }
                    }
                },

                // فيزا 2 - CIB
                new VisaCard
                {
                    CardNumber = "4222222222222222",
                    Pin = "5678",
                    UserId = null,  // ✅ مش مرتبطة بأي user
                    BankId = cib.Id,
                    IsActive = true,
                    ExpiryDate = new DateTime(2026, 6, 30),
                    Accounts = new List<BankAccount>
                    {
                        new BankAccount { AccountNumber = "CIB-ACC-001", AccountType = "Savings", Balance = 8500, Currency = "EGP", IsActive = true }
                    }
                },

                // فيزا 3 - QNB
                new VisaCard
                {
                    CardNumber = "4333333333333333",
                    Pin = "9999",
                    UserId = null,  // ✅ مش مرتبطة بأي user
                    BankId = qnb.Id,
                    IsActive = true,
                    ExpiryDate = new DateTime(2028, 3, 31),
                    Accounts = new List<BankAccount>
                    {
                        new BankAccount { AccountNumber = "QNB-ACC-001", AccountType = "Current", Balance = 50000, Currency = "EGP", IsActive = true },
                        new BankAccount { AccountNumber = "QNB-ACC-002", AccountType = "Savings", Balance = 120000, Currency = "EGP", IsActive = true },
                        new BankAccount { AccountNumber = "QNB-ACC-003", AccountType = "Business", Balance = 250000, Currency = "EGP", IsActive = true }
                    }
                }
            };

            await context.VisaCards.AddRangeAsync(visaCards);
            await context.SaveChangesAsync();
        }
    }
}