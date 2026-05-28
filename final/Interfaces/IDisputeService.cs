// Interfaces/IDisputeService.cs
using final.Application.DTOs;

namespace final.Interfaces
{
    public interface IDisputeService
    {
        Task<DisputeResponse> OpenDisputeAsync(string userId, CreateDisputeRequest request);
        Task<IEnumerable<DisputeResponse>> GetMyDisputesAsync(string userId);
        Task<IEnumerable<DisputeResponse>> GetAllDisputesAsync(); // Admin
        Task<DisputeResponse> ResolveDisputeAsync(Guid disputeId, string adminId, ResolveDisputeRequest request);
        Task<DisputeResponse> SetInReviewAsync(Guid disputeId, string adminId); // Admin يبدأ المراجعة
    }
}