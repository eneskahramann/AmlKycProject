using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AmlKycProject.Api.Data;

public class AmlKycDbContextFactory : IDesignTimeDbContextFactory<AmlKycDbContext>
{
    public AmlKycDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AmlKycDbContext>();
        
        // docker-compose dosyamızdaki PostgreSQL bağlantı bilgilerini doğrudan buraya veriyoruz
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=aml_kyc_db;Username=aml_user;Password=aml_password");

        return new AmlKycDbContext(optionsBuilder.Options);
    }
}