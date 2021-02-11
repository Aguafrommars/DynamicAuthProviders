// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication;
using Aguacongas.AspNetCore.Authentication.RavenDb;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
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
        /// <param name="builder">The builder.</param>
        /// <param name="dataBase">The data base.</param>
        /// <returns>
        /// The <see cref="DynamicAuthenticationBuilder" />
        /// </returns>
        public static DynamicAuthenticationBuilder AddRavenDbStorekStore(this DynamicAuthenticationBuilder builder, string dataBase = null)
        {
            AddStore(builder.Services, builder.DefinitionType, dataBase);
            return builder;
        }

        private static void AddStore(IServiceCollection service, Type definitionType, string dataBase)
        {
            var storeType = typeof(DynamicProviderStore<>).MakeGenericType(definitionType);
            var loggerType = typeof(ILogger<>).MakeGenericType(storeType);

            service.TryAddTransient(typeof(IDynamicProviderStore<>).MakeGenericType(definitionType), p =>
            {
                var session = p.GetRequiredService<IDocumentStore>().OpenAsyncSession(new SessionOptions
                {
                    Database = dataBase
                });
                session.Advanced.UseOptimisticConcurrency = true;
                var constructor = storeType.GetConstructor(new[] { typeof(IAsyncDocumentSession), typeof(IAuthenticationSchemeOptionsSerializer), loggerType });
                return constructor.Invoke(new[] { session, p.GetRequiredService<IAuthenticationSchemeOptionsSerializer>(), p.GetRequiredService(loggerType) });
            });
            service.AddTransient<IAuthenticationSchemeOptionsSerializer, AuthenticationSchemeOptionsSerializer>();
        }
    }
}
