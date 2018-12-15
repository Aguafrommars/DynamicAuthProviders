// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Configure the DI for dynamic scheme management.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationBuilder" />
    public class DynamicAuthenticationBuilder : AuthenticationBuilder
    {
        private readonly List<Type> _handlerTypes = new List<Type>();

        /// <summary>
        /// Gets the handler types managed by this instance.
        /// </summary>
        /// <value>
        /// The handler types.
        /// </value>
        public IEnumerable<Type> HandlerTypes { get; }

        public Type DefinitionType { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAuthenticationBuilder"/> class.
        /// </summary>
        /// <param name="services">The services.</param>
        public DynamicAuthenticationBuilder(IServiceCollection services, Type definitionType): base(services)
        {
            HandlerTypes = _handlerTypes;
            DefinitionType = definitionType;
        }

        /// <summary>
        /// Adds a <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationScheme" /> which can be used by <see cref="T:Microsoft.AspNetCore.Authentication.IAuthenticationService" />.
        /// </summary>
        /// <typeparam name="TOptions">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" /> type to configure the handler."/&gt;.</typeparam>
        /// <typeparam name="THandler">The <see cref="T:Microsoft.AspNetCore.Authentication.AuthenticationHandler`1" /> used to handle this scheme.</typeparam>
        /// <param name="authenticationScheme">The name of this scheme.</param>
        /// <param name="displayName">The display name of this scheme.</param>
        /// <param name="configureOptions">Used to configure the scheme options.</param>
        /// <returns>
        /// The builder.
        /// </returns>
        public override AuthenticationBuilder AddScheme<TOptions, THandler>(string authenticationScheme, string displayName, Action<TOptions> configureOptions)
        {
            _handlerTypes.Add(typeof(THandler));
            Services.AddSingleton(provider => 
                new OptionsMonitorCacheWrapper<TOptions>
                (
                    provider.GetRequiredService<IOptionsMonitorCache<TOptions>>(),
                    (name, configure) =>
                    {
                        configureOptions?.Invoke((TOptions)configure);
                    }
                )
            );
            base.AddScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
            return this;
        }
    }
}
