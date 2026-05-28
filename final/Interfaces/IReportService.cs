using final.Application.DTOs;

namespace final.Interfaces
{
    public interface IReportService
    {
        Task<UserReportSummaryDto> GetUserReportAsync(string userId);
        Task<byte[]> GenerateUserReportPdfAsync(string userId);
    }
}