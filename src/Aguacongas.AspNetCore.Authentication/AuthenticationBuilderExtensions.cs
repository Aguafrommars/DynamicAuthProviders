// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// AuthenticationBuilder extensions
    /// </summary>
    public static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Configures the DI for dynamic scheme management.
        /// </summary>
        /// <typeparam name="TSchemeDefinition">The type of the definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="notify">The action to call on scheme added or removed.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TSchemeDefinition>(this AuthenticationBuilder builder)
            where TSchemeDefinition: SchemeDefinitionBase, new()
        {
            var dynamicBuilder = new DynamicAuthenticationBuilder(builder.Services, typeof(TSchemeDefinition));
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient(provider => new PersistentDynamicManager<TSchemeDefinition>
                (
                    provider.GetRequiredService<IAuthenticationSchemeProvider>(),
                    provider.GetRequiredService<OptionsMonitorCacheWrapperFactory>(),
                    provider.GetRequiredService<IDynamicProviderStore<TSchemeDefinition>>(),
                    dynamicBuilder.HandlerTypes
                ))
                .AddTransient(provider => new NoPersistentDynamicManager<TSchemeDefinition>
                (
                    provider.GetRequiredService<IAuthenticationSchemeProvider>(),
                    provider.GetRequiredService<OptionsMonitorCacheWrapperFactory>(),
                    dynamicBuilder.HandlerTypes
                ));
            return dynamicBuilder;
        }
    }
}
