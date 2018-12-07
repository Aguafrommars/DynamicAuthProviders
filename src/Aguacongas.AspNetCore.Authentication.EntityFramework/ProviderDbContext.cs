using Microsoft.EntityFrameworkCore;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public class ProviderDbContext: DbContext
    {
        public DbSet<ProviderDefinition> Providers { get; set; }
        public ProviderDbContext(DbContextOptions options): base(options)
        { }
    }
}
