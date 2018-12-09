// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="DynamicAuthenticationBuilder"/> extensions.
    /// </summary>
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Configures the DI for dynamic scheme management.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <param name="notify">The action to call on scheme added or removed.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null, Action<string, SchemeAction> notify = null)
        {
            return builder.AddDynamic<SchemeDefinition>(notify)
                .AddEntityFrameworkStore(optionsAction);
        }

        /// <summary>
        /// Configures the DI for dynamic scheme management.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The db context options builder action.</param>
        /// <param name="notify">The action to call on scheme added or removed.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TContext>(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null, Action<string, SchemeAction> notify = null)
            where TContext : SchemeDbContext<SchemeDefinition>
        {
            return builder.AddDynamic<SchemeDefinition>(notify)
                .AddEntityFrameworkStore<TContext>(optionsAction);
        }

        /// <summary>
        /// Configures the DI for dynamic scheme management.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TContext, TSchemeDefinition>(this AuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null, Action<string, SchemeAction> notify = null)
            where TSchemeDefinition: SchemeDefinition, new()
            where TContext : SchemeDbContext<TSchemeDefinition>
        {
            return builder.AddDynamic<TSchemeDefinition>(notify)
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
            return AddEntityFrameworkStore<SchemeDbContext<SchemeDefinition>>(builder, optionsAction);
        }
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="optionsAction">The options action.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext>(this DynamicAuthenticationBuilder builder, Action<DbContextOptionsBuilder> optionsAction = null)
            where TContext : SchemeDbContext<SchemeDefinition>
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
            where TContext : SchemeDbContext<TSchemeDefinition>
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
                    where TContext : SchemeDbContext<TSchemeDefinition>
                    where TSchemeDefinition : SchemeDefinition, new()
        {
            return serviceCollection.AddDbContext<TContext>(optionsAction)
                .AddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>()
                .AddTransient<IDynamicProviderStore<TSchemeDefinition>, DynamicProviderStore<TSchemeDefinition>>();
        }
    }
}
