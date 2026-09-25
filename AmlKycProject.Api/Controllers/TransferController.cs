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
    private readonly AmlKycDbContext _context;

    public TransferController(ITransferService transferService, AmlKycDbContext context)
    {
        _transferService = transferService;
        _context = context; 
    }

    // --- TEST VERİSİ OLUŞTURMA METODU --- DATA SEED
    [HttpPost("add-customer")]
    public async Task<IActionResult> AddCustomer([FromBody] CreateCustomerRequestDto request)
    {
        if (_context.Customers.Any(c => c.IdentityNumber == request.IdentityNumber))
            return BadRequest(new { Message = "Bu kimlik numarasına sahip bir müşteri zaten var!" });

        var newCustomer = new Customer 
        { 
            FirstName = request.FirstName, 
            LastName = request.LastName, 
            IdentityNumber = request.IdentityNumber 
        };
        
        _context.Customers.Add(newCustomer);
        await _context.SaveChangesAsync();

        var newAccount = new Account 
        { 
            CustomerId = newCustomer.Id, 
            FirstName = newCustomer.FirstName, // İsimler artık hesaba da ekleniyor
            LastName = newCustomer.LastName,   
            Balance = request.InitialBalance, 
            Currency = "TRY" 
        };

        _context.Accounts.Add(newAccount);
        await _context.SaveChangesAsync();

        return Ok(new 
        { 
            Message = $"{newCustomer.FirstName} {newCustomer.LastName} sisteme başarıyla eklendi!", 
            MusteriId = newCustomer.Id,
            HesapId = newAccount.Id,
            Bakiye = newAccount.Balance
        });
    }

    // --- TRANSFER METODU ---
    [HttpPost] // Vue.js'den gelen POST isteğini yakalıyoruz.
    public async Task<IActionResult> MakeTransfer([FromBody] TransferRequestDto request)// Vue.js'in gönderdiği JSON verisi (Gönderen, Alıcı, Tutar) 'request' nesnesine gelir.
    {

        // Bu gelen veriyi alıp ve asıl işi yapması için TransferService'e yolluyoruz.
        
        var result = await _transferService.ExecuteTransferAsync(
            request.SenderAccountId, 
            request.ReceiverAccountId, 
            request.Amount
        );

        if (!result.IsSuccess) return BadRequest(new { Message = result.Message });

        return Ok(new { Message = result.Message, TransferDetails = result.TransferRecord });
    }

    // --- VERİTABANINDAKİ HESAPLARI GÖRME METODU ---
    [HttpGet("accounts")]
    public IActionResult GetAccounts()
    {
        var accounts = _context.Accounts
            .Select(a => new 
            { 
                HesapId = a.Id, 
                Ad = a.FirstName,       
                Soyad = a.LastName,     
                Bakiye = a.Balance, 
                MusteriId = a.CustomerId 
            })
            .ToList();
            
        return Ok(accounts);
    }

    // --- AHMET'İ KARA LİSTEYE (SANCTION) EKLEME METODU ---
    [HttpPost("add-sanction")]
    public async Task<IActionResult> AddSanction()
    {
        if (_context.Sanctions.Any(s => s.IdentityNumber == "11111113422"))
            return Ok("Ahmet zaten kara listede!");

        var sanction = new Sanction
        {
            FullName = "Mehmet Yıllar",
            IdentityNumber = "11111111112",
            Country = "Türkiye"
        };
        
        _context.Sanctions.Add(sanction);
        await _context.SaveChangesAsync();
        
        return Ok("Uyarı: Ahmet kara listeye (Sanction) eklendi!");
    }

    // --- OLUŞAN ALARMLARI GÖRME METODU ---
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _context.Alerts
            .Include(a => a.RiskLog)
            .Include(a => a.Transfer)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                id = a.Id,
                createdAt = a.CreatedAt,
                status = a.Status,
                riskLog = a.RiskLog,
                transfer = new
                {
                    id = a.Transfer.Id,
                    amount = a.Transfer.Amount,
                    senderAccountId = a.Transfer.SenderAccountId,
                    receiverAccountId = a.Transfer.ReceiverAccountId,
                    // Hesap ID'lerinden isimleri çekiyoruz
                    senderName = _context.Accounts.Where(acc => acc.Id == a.Transfer.SenderAccountId).Select(acc => acc.FirstName + " " + acc.LastName).FirstOrDefault(),
                    receiverName = _context.Accounts.Where(acc => acc.Id == a.Transfer.ReceiverAccountId).Select(acc => acc.FirstName + " " + acc.LastName).FirstOrDefault()
                }
            })
            .ToListAsync();

        return Ok(alerts);
    }

    // --- ALARM DURUMUNU GÜNCELLEME ---
    [HttpPut("alerts/{id}/status")]
    public async Task<IActionResult> UpdateAlertStatus(int id, [FromBody] UpdateAlertStatusDto request)
    {
        var alert = await _context.Alerts.FindAsync(id);
        if (alert == null) return NotFound(new { message = "Alarm bulunamadı." });

        alert.Status = request.Status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Durum başarıyla güncellendi.", newStatus = alert.Status });
    }

    // --- HESABA PARA EKLEME METODU (TEST İÇİN) ---
    [HttpPost("add-money")]
    public async Task<IActionResult> AddMoney(int accountId, decimal amount)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null) return NotFound("Hesap bulunamadı.");
        
        account.Balance += amount;
        await _context.SaveChangesAsync();
        
        return Ok($"İşlem başarılı. Hesap ID {accountId} için yeni bakiye: {account.Balance} TL");
    }
}



