﻿// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IApplicationBuilder extensions to load configuration
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder LoadDynamicAuthenticationConfiguration<TDefinition>(this IApplicationBuilder builder)
            where TDefinition: SchemeDefinitionBase, new()
        {
            builder.ApplicationServices.LoadDynamicAuthenticationConfiguration<TDefinition>();
            return builder;
        }

        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the definition.</typeparam>
        /// <param name="provider">The provider.</param>
        /// <returns></returns>
        public static IServiceProvider LoadDynamicAuthenticationConfiguration<TDefinition>(this IServiceProvider provider)
            where TDefinition : SchemeDefinitionBase, new()
        {
            using (var scope = provider.CreateScope())
            {
                var manager = scope.ServiceProvider.GetRequiredService<PersistentDynamicManager<TDefinition>>();
                manager.Load();
            }
            return provider;
        }
    }
}
