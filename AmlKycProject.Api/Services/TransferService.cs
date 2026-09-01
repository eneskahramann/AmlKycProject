using Microsoft.EntityFrameworkCore;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Services;

public class TransferService : ITransferService
{
    private readonly AmlKycDbContext _context;
    private readonly IRiskService _riskService;

    public TransferService(AmlKycDbContext context, IRiskService riskService) 
    {
        _context = context;
        _riskService = riskService;
    }

    public async Task<(bool IsSuccess, string Message, Transfer? TransferRecord)> ExecuteTransferAsync(int senderAccountId, int receiverAccountId, decimal amount)
    {
        // 1. Temel Kontroller
        if (amount <= 0) return (false, "Transfer tutarı 0'dan büyük olmalıdır.", null);
        if (senderAccountId == receiverAccountId) return (false, "Gönderici ve alıcı hesap aynı olamaz.", null);

        var senderAccount = await _context.Accounts.FindAsync(senderAccountId);
        var receiverAccount = await _context.Accounts.FindAsync(receiverAccountId);

        if (senderAccount == null || receiverAccount == null)
            return (false, "Hesap bulunamadı.", null);

        if (senderAccount.Balance < amount)
            return (false, "Yetersiz bakiye.", null);

        // 2. ACID Transaction Başlatılıyor
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Bakiyeleri güncelle
            senderAccount.Balance -= amount;
            receiverAccount.Balance += amount;

            // Transfer kaydını oluştur
            var transfer = new Transfer
            {
                SenderAccountId = senderAccountId,
                ReceiverAccountId = receiverAccountId,
                Amount = amount,
                IsSuccessful = true,
                TransferDate = DateTime.UtcNow
            };

            _context.Transfers.Add(transfer);
            
            // Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // TRANSFER BAŞARILI, RİSK MOTORUNU ÇALIŞTIR
            await _riskService.EvaluateTransferRiskAsync(transfer);
            
            // Transaction'ı onayla (Commit)
            await transaction.CommitAsync();

            

            

            return (true, "Transfer başarıyla gerçekleşti.", transfer);
        }
        catch (Exception ex)
        {
            // Herhangi bir hata oluşması durumunda tüm işlemler geri alınır
            await transaction.RollbackAsync();
            return (false, $"Transfer sırasında hata oluştu: {ex.Message}", null);
        }
    }
}