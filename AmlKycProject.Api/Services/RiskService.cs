using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Services;

public class RiskService : IRiskService
{
    private readonly AmlKycDbContext _context;

    public RiskService(AmlKycDbContext context)
    {
        _context = context;
    }

    public async Task EvaluateTransferRiskAsync(Transfer transfer)
    {
        int riskScore = 0;
        var triggeredRules = new List<string>();

        // İşlemi yapan gönderici ve alıcının müşteri bilgilerini (TC Kimlik vb.) veritabanından çekiyoruz
        var senderAccount = await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == transfer.SenderAccountId);
        var receiverAccount = await _context.Accounts.Include(a => a.Customer).FirstOrDefaultAsync(a => a.Id == transfer.ReceiverAccountId);

        if (senderAccount == null || receiverAccount == null) return;

        // Gönderici ve alıcı hesaplarının yaşını hesaplıyoruz
        var senderYasi = (DateTime.UtcNow - senderAccount.CreatedAt).TotalDays;
        var receiverYasi = (DateTime.UtcNow - receiverAccount.CreatedAt).TotalDays;

        // KURAL 1: 100.000 TL üzeri transfer (+40 Puan)
        if (transfer.Amount > 100000)
        {
            riskScore += 40;
            triggeredRules.Add("Yüksek Tutar (100.000 TL Üzeri)");
        }

        // KURAL 2: Gece İşlemi (+20 Puan)
        var currentHour = DateTime.UtcNow.AddHours(3).Hour; 
        if (currentHour >= 22 || currentHour < 6)
        {
            riskScore += 20;
            triggeredRules.Add("Gece İşlemi (22:00 - 06:00)");
        }

        // KURAL 4: Çifte Yeni Hesap Şüphesi (+30 Puan)
        if ((senderYasi <= 3 || receiverYasi <= 3) && transfer.Amount >= 20000)
        {
            riskScore += 30;
            triggeredRules.Add("Çifte Yeni Hesap: Yeni açılan iki hesap arasında şüpheli transfer.");
        }
        
        // KURAL 5: Sınır Altı İşlem Şüphesi (+10 Puan)
        if (transfer.Amount >= 95000 && transfer.Amount < 100000)
        {
            riskScore += 10;
            triggeredRules.Add("Sınır Altı İşlem Şüphesi (95.000 TL - 100.000 TL)");
        }

        // KURAL 6: Doğal Olmayan Küsuratsız İşlem (+10 Puan)
        if (transfer.Amount >= 50000 && transfer.Amount % 1000 == 0)
        {
            riskScore += 10;
            triggeredRules.Add("Doğal olmayan küsuratsız işlem");
        }

        // KURAL 7: Uyuyan Hesap Hareketi (+30 Puan)
        var lastTransfer = await _context.Transfers
            .Where(t => t.SenderAccountId == senderAccount.Id && t.IsSuccessful == true)
            .OrderByDescending(t => t.TransferDate)
            .FirstOrDefaultAsync();

        if (lastTransfer != null)
        {
            var daysSinceLastTransfer = (DateTime.UtcNow - lastTransfer.TransferDate).TotalDays;

            if (daysSinceLastTransfer > 90 && transfer.Amount > 75000)
            {
                riskScore += 20;
                triggeredRules.Add("Uyuyan Hesap: Uzun süre pasif olan hesaptan yüklü çıkış.");
            }
        }

        // Skor 100'ü geçmeyecek şekilde sabitlenir
        riskScore = Math.Min(100, riskScore);

        // Esnek yapı (JSONB) için tetiklenen kuralları JSON string formatına çeviriyoruz
        string rulesJson = JsonSerializer.Serialize(triggeredRules);

        // 1. Adım: Her işlemin risk skorunu Risklog tablosuna kaydet
        var riskLog = new RiskLog
        {
            TransferId = transfer.Id,
            RiskScore = riskScore,
            TriggeredRules = rulesJson,
            CreatedAt = DateTime.UtcNow
        };

        _context.RiskLogs.Add(riskLog);

        // 2. Adım: Risk skoru 70 ve üzeriyse Alert (Alarm) tablosuna kayıt at
        if (riskScore >= 60) 
        {
            var alert = new Alert
            {
                TransferId = transfer.Id,
                RiskLog = riskLog,              
                Status = "Açık", 
                CreatedAt = DateTime.UtcNow
            };
            _context.Alerts.Add(alert);
        }

        await _context.SaveChangesAsync();
    }
}