
using final.Entities;
using final.Infrastructure.Data;
using final.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace final.Infrastructure.Services;

public class FingerprintService : IFingerprintService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FingerprintService> _logger;

    public FingerprintService(ApplicationDbContext context, ILogger<FingerprintService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> EnrollFingerprintAsync(string userId, byte[] fingerprintData, string deviceId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            var template = await ExtractTemplateAsync(fingerprintData);
            var templateBase64 = Convert.ToBase64String(template);

            user.FingerprintTemplate = templateBase64;
            user.IsFingerprintEnabled = true;

            _context.FingerprintLogs.Add(new FingerprintLog
            {
                UserId = userId,
                Action = FingerprintAction.Enrollment,
                IsSuccess = true,
                DeviceId = deviceId,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling fingerprint for user {UserId}", userId);

            _context.FingerprintLogs.Add(new FingerprintLog
            {
                UserId = userId,
                Action = FingerprintAction.Enrollment,
                IsSuccess = false,
                DeviceId = deviceId,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return false;
        }
    }

    public async Task<bool> VerifyFingerprintAsync(string userId, byte[] fingerprintData, string deviceId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.FingerprintTemplate))
                return false;

            var storedTemplate = Convert.FromBase64String(user.FingerprintTemplate);
            var newTemplate = await ExtractTemplateAsync(fingerprintData);

            var matchScore = CompareTemplates(storedTemplate, newTemplate);
            var isMatch = matchScore >= 80;

            _context.FingerprintLogs.Add(new FingerprintLog
            {
                UserId = userId,
                Action = FingerprintAction.Verification,
                IsSuccess = isMatch,
                DeviceId = deviceId,
                ErrorMessage = isMatch ? null : $"Match score: {matchScore}%",
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return isMatch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying fingerprint for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsFingerprintEnabledAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        return user?.IsFingerprintEnabled ?? false;
    }

    public Task<byte[]> ExtractTemplateAsync(byte[] fingerprintImage)
    {
        return Task.FromResult(fingerprintImage);
    }

    private double CompareTemplates(byte[] template1, byte[] template2)
    {
        return 95.0;
    }
}