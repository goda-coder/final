using final.Application.DTOs;
using final.Application.Interfaces;
using final.Entities;
using final.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace final.Infrastructure.Services
{
    public class TrustedContactService : ITrustedContactService
    {
        private readonly ApplicationDbContext _context;

        public TrustedContactService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> AddTrustedContactAsync(AddTrustedContactDto dto)
        {
            var exists = await _context.TrustedContacts
                .AnyAsync(x =>
                    x.UserId == dto.UserId &&
                    x.TrustedUserId == dto.TrustedUserId);

            if (exists)
                return "Trusted contact already exists";

            var contact = new TrustedContact
            {
                UserId = dto.UserId,
                TrustedUserId = dto.TrustedUserId,
                NickName = dto.NickName,
                TrustedPhone = dto.TrustedPhone,
                DailyLimit = dto.DailyLimit,
                TotalLimit = dto.TotalLimit
            };

            _context.TrustedContacts.Add(contact);

            await _context.SaveChangesAsync();

            return "Trusted contact added successfully";
        }

        public async Task<string> QuickTransferAsync(QuickTransferDto dto)
        {
            var contact = await _context.TrustedContacts
                .FirstOrDefaultAsync(x =>
                    x.UserId == dto.UserId &&
                    x.NickName == dto.NickName);

            if (contact == null)
                return "Trusted contact not found";

            if (contact.UsedToday + dto.Amount > contact.DailyLimit)
                return "Daily limit exceeded";

            if (contact.UsedTotal + dto.Amount > contact.TotalLimit)
                return "Total limit exceeded";

            // هنا تعمل التحويل الحقيقي
            // خصم رصيد + إضافة رصيد

            contact.UsedToday += dto.Amount;
            contact.UsedTotal += dto.Amount;

            await _context.SaveChangesAsync();

            return "Transfer completed successfully";
        }

        public async Task<List<TrustedContact>> GetTrustedContactsAsync(int userId)
        {
            return await _context.TrustedContacts
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }
    }
}