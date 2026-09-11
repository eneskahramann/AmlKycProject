using Microsoft.EntityFrameworkCore;
using AmlKycProject.Api.Data;
using AmlKycProject.Api.Services; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // Vue'nun çalıştığı adres
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<AmlKycDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Servislerimizi sisteme tanıtıyoruz
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IRiskService, RiskService>(); 

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowVueApp");
app.UseAuthorization();
app.MapControllers();
app.Run();