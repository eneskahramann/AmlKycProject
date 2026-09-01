using Microsoft.AspNetCore.Mvc;
using AmlKycProject.Api.Services;
using AmlKycProject.Api.DTOs;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Entities;

namespace AmlKycProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransferController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransferController(ITransferService transferService)
    {
        _transferService = transferService;
    }

    // --- TEST VERİSİ OLUŞTURMA METODU ---
    [HttpPost("seed")]
    public async Task<IActionResult> SeedData([FromServices] AmlKycDbContext context)
    {
        if (context.Accounts.Any()) return Ok("Veritabanında zaten hesaplar var.");

        // İki örnek müşteri oluşturuyoruz
        var customer1 = new Customer { FirstName = "Ahmet", LastName = "Yılmaz", IdentityNumber = "11111111111" };
        var customer2 = new Customer { FirstName = "Ayşe", LastName = "Kaya", IdentityNumber = "22222222222" };
        
        context.Customers.AddRange(customer1, customer2);
        await context.SaveChangesAsync();

        // Ahmet'in 100.000 TL'si var, Ayşe'nin hesabı boş
        var account1 = new Account { CustomerId = customer1.Id, Balance = 100000m, Currency = "TRY" };
        var account2 = new Account { CustomerId = customer2.Id, Balance = 0m, Currency = "TRY" };

        context.Accounts.AddRange(account1, account2);
        await context.SaveChangesAsync();

        return Ok(new 
        { 
            Message = "Test verileri başarıyla oluşturuldu!", 
            GondericiHesapId = account1.Id, 
            AliciHesapId = account2.Id,
            GondericiBakiye = account1.Balance
        });
    }

    // --- ASIL TRANSFER METODU ---
    [HttpPost]
    public async Task<IActionResult> MakeTransfer([FromBody] TransferRequestDto request)
    {
        var result = await _transferService.ExecuteTransferAsync(
            request.SenderAccountId, 
            request.ReceiverAccountId, 
            request.Amount
        );

        if (!result.IsSuccess)
        {
            return BadRequest(new { Message = result.Message });
        }

        return Ok(new { Message = result.Message, TransferDetails = result.TransferRecord });
    }

    // --- VERİTABANINDAKİ HESAPLARI GÖRME METODU ---
    [HttpGet("accounts")]
    public IActionResult GetAccounts([FromServices] AmlKycDbContext context)
    {
        var accounts = context.Accounts
            .Select(a => new { HesapId = a.Id, Bakiye = a.Balance, MusteriId = a.CustomerId })
            .ToList();
            
        return Ok(accounts);
    }

    // --- AHMET'İ KARA LİSTEYE (SANCTION) EKLEME METODU ---
    [HttpPost("add-sanction")]
    public async Task<IActionResult> AddSanction([FromServices] AmlKycDbContext context)
    {
        if (context.Sanctions.Any(s => s.IdentityNumber == "11111111111"))
            return Ok("Ahmet zaten kara listede!");

        var sanction = new Sanction
        {
            FullName = "Ahmet Yılmaz",
            IdentityNumber = "11111111111",
            Country = "Türkiye"
        };
        
        context.Sanctions.Add(sanction);
        await context.SaveChangesAsync();
        
        return Ok("Uyarı: Ahmet kara listeye (Sanction) eklendi!");
    }

    // --- OLUŞAN ALARMLARI GÖRME METODU
    [HttpGet("alerts")]
    public IActionResult GetAlerts([FromServices] AmlKycDbContext context)
    {
        var alerts = context.Alerts
            .Select(a => new 
            { 
                AlarmId = a.Id, 
                Durum = a.Status, 
                RiskSkoru = a.RiskLog.RiskScore,
                TetiklenenKurallar = a.RiskLog.TriggeredRules
            })
            .ToList();
            
        return Ok(alerts);
    }

    // --- HESABA PARA EKLEME METODU (TEST İÇİN) ---
    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoney([FromServices] AmlKycDbContext context, int accountId, decimal amount)
    {
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null) return NotFound("Hesap bulunamadı.");
        
        account.Balance += amount;
        await context.SaveChangesAsync();
        
        return Ok($"İşlem başarılı. Hesap ID {accountId} için yeni bakiye: {account.Balance} TL");
    }
}