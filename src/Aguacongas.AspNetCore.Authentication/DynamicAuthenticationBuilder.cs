using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicAuthenticationBuilder : AuthenticationBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicAuthenticationBuilder"/> class.
        /// </summary>
        /// <param name="services">The services being configured.</param>
        public DynamicAuthenticationBuilder(IServiceCollection services): base(services)
        { }

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
            Services.AddSingleton(provider => new OptionsMonitorCacheWrapper<TOptions>(
                provider.GetRequiredService<IOptionsMonitorCache<TOptions>>(),
                configure => configureOptions?.Invoke((TOptions)configure)));
            base.AddScheme<TOptions, THandler>(authenticationScheme, displayName, configureOptions);
            return this;
        }
    }
}
