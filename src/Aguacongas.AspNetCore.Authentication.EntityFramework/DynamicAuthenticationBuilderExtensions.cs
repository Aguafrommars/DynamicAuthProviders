using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
        {
            return AddEntityFrameworkStore<ProviderDbContext>(builder, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext>(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
        {
            return AddEntityFrameworkStore<TContext, ProviderDefinition>(builder, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <typeparam name="TProviderDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext, TProviderDefinition>(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : DbContext
            where TProviderDefinition: ProviderDefinition, new()
        {
            builder.Services.AddDynamicAuthenticationEntityFrameworkStore<TContext, TProviderDefinition>(optionsAction);
            return builder;
        }

        /// <summary>
        /// Adds the dynamic authentication entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <typeparam name="TProviderDefinition">The type of the provider definition.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddDynamicAuthenticationEntityFrameworkStore<TContext, TProviderDefinition>(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null)
                    where TContext : DbContext
                    where TProviderDefinition : ProviderDefinition, new()
        {
            return serviceCollection.AddDbContext<TContext>(optionsAction)
                .AddTransient<IDynamicProviderStore<TProviderDefinition>, DynamicProviderStore<TProviderDefinition>>();
        }
    }
}
