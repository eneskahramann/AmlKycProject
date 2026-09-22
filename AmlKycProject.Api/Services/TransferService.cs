using Microsoft.EntityFrameworkCore;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Entities;
using System.Text.Json;

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

        // Gönderici ve alıcı hesapları veritabanından çekiyoruz.
        var senderAccount = await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == senderAccountId);

        // Alıcı hesabı veritabanından çekiyoruz.
        var receiverAccount = await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == receiverAccountId);

        if (senderAccount == null || receiverAccount == null)
            return (false, "Hesap bulunamadı.", null);

        var isSenderSanctioned = await _context.Sanctions.AnyAsync(s => s.IdentityNumber == senderAccount.Customer.IdentityNumber);
        var isReceiverSanctioned = await _context.Sanctions.AnyAsync(s => s.IdentityNumber == receiverAccount.Customer.IdentityNumber);
    
    if (isSenderSanctioned || isReceiverSanctioned){
            var blockedTransfer = new Transfer
            {
                SenderAccountId = senderAccountId,
                ReceiverAccountId = receiverAccountId,
                Amount = amount,
                IsSuccessful = false, // Transfer başaşrısız
                TransferDate = DateTime.UtcNow
            };

            _context.Transfers.Add(blockedTransfer);
            await _context.SaveChangesAsync();// id oluşmaması için kaydetme işlemi burada yapılır

            
            var ruleList = new List<string>{"Sanction Eşleşmesi - İşlem Bloke Edildi"};

            var riskLog = new RiskLog
            {
                TransferId = blockedTransfer.Id,
                RiskScore = 100, // Maksimum risk skoru
                // C#'ın bu listeyi bir JSON formatına çevirmesini sağlıyoruz
                TriggeredRules = JsonSerializer.Serialize(ruleList),
                CreatedAt = DateTime.UtcNow
            };
            _context.RiskLogs.Add(riskLog);

            var alert = new Alert
            {
                TransferId = blockedTransfer.Id,
                RiskLog = riskLog,
                Status = "Bloke Edildi", // Vue.js tarafında Analist onayına düşecek
                CreatedAt = DateTime.UtcNow
            };
            _context.Alerts.Add(alert);

            await _context.SaveChangesAsync();

            return (false, "İşlem Yaptırım politikaları gereği BLOKE EDİLMİŞTİR", null);
        }



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
            
            // Transactionı onayla 
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