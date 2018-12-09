using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
        {
            return builder.AddDynamic<SchemeDefinition>()
                .AddEntityFrameworkStore(optionsAction);
        }

        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TContext>(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : ProviderDbContext<SchemeDefinition>
        {
            return builder.AddDynamic<SchemeDefinition>()
                .AddEntityFrameworkStore<TContext>(optionsAction);
        }

        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TContext, TSchemeDefinition>(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TSchemeDefinition: SchemeDefinition, new()
            where TContext : ProviderDbContext<TSchemeDefinition>
        {
            return builder.AddDynamic<TSchemeDefinition>()
                .AddEntityFrameworkStore<TContext, TSchemeDefinition>(optionsAction);
        }

        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
        {
            return AddEntityFrameworkStore<ProviderDbContext<SchemeDefinition>>(builder, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext>(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : ProviderDbContext<SchemeDefinition>
        {
            return AddEntityFrameworkStore<TContext, SchemeDefinition>(builder, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext, TSchemeDefinition>(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : ProviderDbContext<TSchemeDefinition>
            where TSchemeDefinition: SchemeDefinition, new()
        {
            builder.Services.AddDynamicAuthenticationEntityFrameworkStore<TContext, TSchemeDefinition>(optionsAction);
            return builder;
        }

        /// <summary>
        /// Adds the dynamic authentication entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <typeparam name="TProviderDefinition">The type of the scheme definition.</typeparam>
        /// <param name="serviceCollection">The service collection.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static IServiceCollection AddDynamicAuthenticationEntityFrameworkStore<TContext, TSchemeDefinition>(this IServiceCollection serviceCollection, Action<DbContextOptionsBuilder> optionsAction = null)
                    where TContext : ProviderDbContext<TSchemeDefinition>
                    where TSchemeDefinition : SchemeDefinition, new()
        {
            return serviceCollection.AddDbContext<TContext>(optionsAction)
                .AddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>()
                .AddTransient<IDynamicProviderStore<TSchemeDefinition>, DynamicProviderStore<TSchemeDefinition>>();
        }
    }
}
