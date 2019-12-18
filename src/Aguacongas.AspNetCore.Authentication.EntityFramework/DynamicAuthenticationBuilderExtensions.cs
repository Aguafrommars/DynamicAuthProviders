// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// <see cref="DynamicAuthenticationBuilder"/> extensions.
    /// </summary>
    public static class DynamicAuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the entity framework store.
        /// </summary>
        /// <typeparam name="TContext">The type of the context.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns>The <see cref="DynamicAuthenticationBuilder"/></returns>
        public static DynamicAuthenticationBuilder AddEntityFrameworkStore<TContext>(this DynamicAuthenticationBuilder builder)
            where TContext : DbContext
        {
            AddStore(builder.Services, builder.DefinitionType, typeof(TContext));
            return builder;
        }

        private static void AddStore(IServiceCollection service, Type definitionType, Type contextType)
        {
            var storeType = typeof(DynamicProviderStore<,>).MakeGenericType(definitionType, contextType);

            service.TryAddTransient(typeof(IDynamicProviderStore<>).MakeGenericType(definitionType), storeType);
            service.AddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>();

        }
    }
}
