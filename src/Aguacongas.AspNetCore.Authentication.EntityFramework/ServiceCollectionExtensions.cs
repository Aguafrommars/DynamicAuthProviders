using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <param name="serviceCollection">
        /// The service collection.
        /// </param>
        /// <param name="optionsAction">
        /// The options action.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFrameworkStore(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null)
        {
            return AddEntityFrameworkStore<ProviderDbContext>(serviceCollection, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of the context.
        /// </typeparam>
        /// <param name="serviceCollection">
        /// The service collection.
        /// </param>
        /// <param name="optionsAction">
        /// The options action.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFrameworkStore<TContext>(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            return AddEntityFrameworkStore<TContext, ProviderDefinition>(serviceCollection, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">
        /// The type of the context.
        /// </typeparam>
        /// <typeparam name="TDefinition">
        /// The type of the provider definition.
        /// </typeparam>
        /// <param name="serviceCollection">
        /// The service collection.
        /// </param>
        /// <param name="optionsAction">
        /// The options action.
        /// </param>
        /// <returns></returns>
        public static IServiceCollection AddEntityFrameworkStore<TContext, TDefinition>(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
            where TDefinition: ProviderDefinition, new()
        {
            return serviceCollection.AddDbContext<TContext>(optionsAction)
                .AddTransient<IDynamicProviderStore<TDefinition>, DynamicProviderStore<TDefinition>>();
        }
    }
}
