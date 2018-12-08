using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEntityFrameworkStore<TContext>(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null) where TContext : DbContext
        {
            return serviceCollection.AddDbContext<TContext>(optionsAction)
                .AddTransient<IDynamicProviderStore, DynamicProviderStore>();
        }
    }
}
