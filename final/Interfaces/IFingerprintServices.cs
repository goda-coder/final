namespace final.Interfaces
{
    

    public interface IFingerprintService
    {
        Task<bool> EnrollFingerprintAsync(string userId, byte[] fingerprintData, string deviceId);
        Task<bool> VerifyFingerprintAsync(string userId, byte[] fingerprintData, string deviceId);
        Task<bool> IsFingerprintEnabledAsync(string userId);
        Task<byte[]> ExtractTemplateAsync(byte[] fingerprintImage);
    }
}
