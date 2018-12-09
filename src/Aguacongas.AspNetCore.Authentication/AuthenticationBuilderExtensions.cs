﻿// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="notify">The action to call on scheme added or removed.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TDefinition>(this AuthenticationBuilder builder, Action<string, SchemeAction> notify = null)
            where TDefinition: SchemeDefinitionBase, new()
        {
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient<DynamicManager<TDefinition>>();
            return new DynamicAuthenticationBuilder(builder.Services, notify);
        }
    }
}