using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // --- TEST VERİSİ OLUŞTURMA METODU --- DATA SEED
    // Bu metod, test amaçlı olarak veritabanına örnek müşteri ve hesap verilerini ekler. Analist ve kullanıcılar bu verilerle transfer işlemlerini test edebilir.
    [HttpPost("add-customer")]
public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerRequestDto request, [FromServices] AmlKycDbContext context)
{
    // 1. Aynı kimlik numarasıyla daha önce kayıt olunmuş mu kontrol et
    if (context.Customers.Any(c => c.IdentityNumber == request.IdentityNumber))
    {
        return BadRequest(new { Message = "Bu kimlik numarasına sahip bir müşteri zaten var!" });
    }

    // 2. Yeni Müşteriyi Oluştur
    var newCustomer = new Customer 
    { 
        FirstName = request.FirstName, 
        LastName = request.LastName, 
        IdentityNumber = request.IdentityNumber 
    };
    
    context.Customers.Add(newCustomer);
    await context.SaveChangesAsync(); // Veritabanına kaydet ki ID'si oluşsun

    // 3. Müşteriye Hesap Aç ve Bakiyesini Yükle
    var newAccount = new Account 
    { 
        CustomerId = newCustomer.Id, 
        Balance = request.InitialBalance, 
        Currency = "TRY" 
    };

    context.Accounts.Add(newAccount);
    await context.SaveChangesAsync(); // Hesabı da kaydet

    // 4. Başarı Mesajı Dön
    return Ok(new 
    { 
        Message = $"{newCustomer.FirstName} {newCustomer.LastName} sisteme başarıyla eklendi!", 
        MusteriId = newCustomer.Id,
        HesapId = newAccount.Id,
        Bakiye = newAccount.Balance
    });
}

    // --- TRANSFER METODU ---
    // Bu metod, transfer işlemini başlatır ve risk değerlendirmesi yapar.
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
    // Bu metod, veritabanındaki tüm hesapları listeler. Hesap ID, bakiye ve müşteri ID'sini döndürür.
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
    // Bu metod veritabanındaki tüm alarmları risk loglarıyla birlikte listeler.Analist bu alarmları inceleyebilir.
    [HttpGet("alerts")]
public async Task<IActionResult> GetAlerts([FromServices] AmlKycDbContext context)
{
    // Veritabanındaki tüm alarmları risk loglarıyla birlikte çekiyoruz.
    var alerts = await context.Alerts
        .Include(a => a.RiskLog)// Risk log detayları 
        .Include(a => a.Transfer) // Transfer detayları
        .OrderByDescending(a => a.CreatedAt)
        .ToListAsync();

    return Ok(alerts);
}

// Analistin dışarıdan göndereceği durumu yakalayacağımız model
public class UpdateAlertStatusDto
{
    public string Status { get; set; }
}

// Alarmın durumunu güncelleme (Endpoint)
[HttpPut("alerts/{id}/status")]
public async Task<IActionResult> UpdateAlertStatus(int id, [FromBody] UpdateAlertStatusDto request, [FromServices] AmlKycDbContext context)
{
    // Veritabanından ilgili alarmı bul
    var alert = await context.Alerts.FindAsync(id);
    
    if (alert == null) 
        return NotFound(new { message = "Alarm bulunamadı." });

    // Durumu güncelle ("Approved" veya "Suspicious" olarak)
    alert.Status = request.Status;
    
    // Değişikliği PostgreSQL'e kaydet
    await context.SaveChangesAsync();

    return Ok(new { message = "Durum başarıyla güncellendi.", newStatus = alert.Status });
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
public class CreateCustomerRequestDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string IdentityNumber { get; set; }
    public decimal InitialBalance { get; set; }
}