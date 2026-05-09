
using final.Entities;
using final.Enums;
using final.Infrastructure.Data;
using final.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace final.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly ApplicationDbContext _context;
    private readonly IFingerprintService _fingerprintService;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ApplicationDbContext context,
        IFingerprintService fingerprintService,
        ILogger<TransactionService> logger)
    {
        _context = context;
        _fingerprintService = fingerprintService;
        _logger = logger;
    }

    public async Task<Transaction> CreateTransferAsync(
        string senderId,
        string receiverPhone,
        decimal amount,
        string? description,
        bool useFingerprint = false,
        string? deviceId = null)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be greater than 0");
        if (amount > 50_000) throw new ArgumentException("Maximum transfer amount is 50,000 EGP");

        var sender = await _context.Users.FindAsync(senderId);
        if (sender == null) throw new InvalidOperationException("Sender not found");
        if (sender.Balance < amount) throw new InvalidOperationException("Insufficient balance");

        if (useFingerprint)
        {
            if (!await _fingerprintService.IsFingerprintEnabledAsync(senderId))
                throw new InvalidOperationException("Fingerprint not enabled");
        }

        var receiver = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == receiverPhone);
        if (receiver == null) throw new InvalidOperationException("Receiver not found");

        var transaction = new Transaction
        {
            SenderId = senderId,
            ReceiverId = receiver.Id,
            Amount = amount,
            Description = description,
            Type = TransactionType.Transfer,
            Status = TransactionStatus.Pending,
            IsFingerprintPayment = useFingerprint,
            FingerprintDeviceId = deviceId
        };

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            sender.Balance -= amount;
            receiver.Balance += amount;
            transaction.Status = TransactionStatus.Completed;
            transaction.CompletedAt = DateTime.UtcNow;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            await dbTransaction.CommitAsync();

            return transaction;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            transaction.Status = TransactionStatus.Failed;
            transaction.FailureReason = "Transaction failed";
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            throw;
        }
    }

    public async Task<Transaction> CreateMerchantPaymentAsync(
        string userId,
        string merchantId,
        decimal amount,
        bool useFingerprint = false,
        string? deviceId = null)
    {
        var merchant = await _context.Users.FindAsync(merchantId);
        if (merchant == null || merchant.Role != UserRole.Merchant)
            throw new InvalidOperationException("Invalid merchant");
        if (merchant.MerchantStatus != MerchantStatus.Approved)
            throw new InvalidOperationException("Merchant not approved");

        var transaction = await CreateTransferAsync(userId, merchant.PhoneNumber!, amount,
            $"Payment to {merchant.MerchantName}", useFingerprint, deviceId);

        transaction.Type = TransactionType.MerchantPayment;
        await _context.SaveChangesAsync();

        return transaction;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId)
    {
        return await _context.Transactions
            .Include(t => t.Sender)
            .Include(t => t.Receiver)
            .Where(t => t.SenderId == userId || t.ReceiverId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ConfirmTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null || transaction.Status != TransactionStatus.Pending) return false;

        transaction.Status = TransactionStatus.Completed;
        transaction.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelTransactionAsync(Guid transactionId)
    {
        var transaction = await _context.Transactions.FindAsync(transactionId);
        if (transaction == null || transaction.Status != TransactionStatus.Pending) return false;

        var sender = await _context.Users.FindAsync(transaction.SenderId);
        if (sender != null) sender.Balance += transaction.Amount;

        transaction.Status = TransactionStatus.Cancelled;
        await _context.SaveChangesAsync();
        return true;
    }
}