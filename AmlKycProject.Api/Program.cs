using Microsoft.EntityFrameworkCore;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Services; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<AmlKycDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servislerimizi sisteme (Dependency Injection) tanıtıyoruz
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IRiskService, RiskService>(); // <-- İŞTE SİSTEMİ AYAĞA KALDIRACAK O SATIR

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();