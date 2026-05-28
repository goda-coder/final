using final.Application.DTOs;
using final.Entities;

namespace final.Application.Interfaces
{
    public interface ITrustedContactService
    {
        Task<string> AddTrustedContactAsync(AddTrustedContactDto dto);

        Task<string> QuickTransferAsync(QuickTransferDto dto);

        Task<List<TrustedContact>> GetTrustedContactsAsync(int userId);
    }
}