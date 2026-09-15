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

        // KURAL 1: 100.000 TL üzeri transfer (+40 Puan)
        if (transfer.Amount > 100000)
        {
            riskScore += 40;
            triggeredRules.Add("Yüksek Tutar (100.000 TL Üzeri)");
        }

        // KURAL 2: Gece İşlemi (+20 Puan)
        // Örneğin akşam 22:00 ile sabah 06:00 arası gece kabul edilir.
        var currentHour = DateTime.UtcNow.AddHours(3).Hour; 
        if (currentHour >= 22 || currentHour < 6)
        {
            riskScore += 20;
            triggeredRules.Add("Gece İşlemi (22:00 - 06:00)");
            
        }

        // KURAL 3: Yaptırım Listesi Eşleşmesi (+60 Puan)
        var isSenderSanctioned = await _context.Sanctions.AnyAsync(s => s.IdentityNumber == senderAccount.Customer.IdentityNumber);
        var isReceiverSanctioned = await _context.Sanctions.AnyAsync(s => s.IdentityNumber == receiverAccount.Customer.IdentityNumber);

        if (isSenderSanctioned || isReceiverSanctioned)
        {
            riskScore += 60;
            triggeredRules.Add("Sanction Eşleşmesi");
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
        if (riskScore >= 70 || triggeredRules.Contains("Sanction Eşleşmesi")) // Yaptırım listesi eşleşmesi de alarm tetiklemeli
        {
            var alert = new Alert
            {
                TransferId = transfer.Id,
                RiskLog = riskLog,              
                // Vue.js tarafında Analist onayına düşecek
                Status = "Açık", 
                CreatedAt = DateTime.UtcNow
            };
            _context.Alerts.Add(alert);
        }

        await _context.SaveChangesAsync();
    }
}