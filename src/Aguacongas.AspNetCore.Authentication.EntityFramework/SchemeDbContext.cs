// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.EntityFrameworkCore;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    /// <summary>
    /// Scheme definition db context of <see cref="SchemeDefinition"/>
    /// </summary>
    /// <seealso cref="SchemeDbContext{SchemeDefinition}" />
    /// <seealso cref="SchemeDefinition" />
    public class SchemeDbContext : SchemeDbContext<SchemeDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SchemeDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public SchemeDbContext(DbContextOptions options) : base(options)
        {
        }
    }

    /// <summary>
    /// Scheme definition db context
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme otptions.</typeparam>
    public class SchemeDbContext<TSchemeDefinition> : DbContext
        where TSchemeDefinition : SchemeDefinition
    {
        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        /// <value>
        /// The providers.
        /// </value>
        public DbSet<TSchemeDefinition> Providers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemeDbContext{TSchemeOtptions}"/> class.
        /// </summary>
        /// <param name="options">The options for this context.</param>
        public SchemeDbContext(DbContextOptions options): base(options)
        { }

        /// <summary>
        /// Override this method to further configure the model that was discovered by convention from the entity types
        /// exposed in <see cref="T:Microsoft.EntityFrameworkCore.DbSet`1" /> properties on your derived context. The resulting model may be cached
        /// and re-used for subsequent instances of your derived context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
        /// define extension methods on this object that allow you to configure aspects of the model that are specific
        /// to a given database.</param>
        /// <remarks>
        /// If a model is explicitly set on the options for this context (via <see cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)" />)
        /// then this method will not be run.
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TSchemeDefinition>(b =>
            {
                b.Ignore(p => p.Options)
                  .Ignore(p => p.HandlerType)
                  .HasKey(p => p.Scheme);
                b.Property(p => p.ConcurrencyStamp).IsConcurrencyToken();
            });
                
        }
    }
}
