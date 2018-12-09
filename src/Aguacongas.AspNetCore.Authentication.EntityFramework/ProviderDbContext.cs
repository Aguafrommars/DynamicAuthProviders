using Microsoft.EntityFrameworkCore;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public class ProviderDbContext: ProviderDbContext<SchemeDefinition>
    {
        public ProviderDbContext(DbContextOptions options) : base(options)
        { }
    }

    public class ProviderDbContext<TSchemeOtptions>: DbContext
        where TSchemeOtptions: SchemeDefinition
    {
        public DbSet<TSchemeOtptions> Providers { get; set; }
        public ProviderDbContext(DbContextOptions options): base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TSchemeOtptions>()
                .Ignore(p => p.Options)
                .Ignore(p => p.HandlerType)
                .HasKey(p => p.Scheme);
        }
    }
}
