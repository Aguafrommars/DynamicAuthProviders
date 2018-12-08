using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicAuthenticationBuilder : AuthenticationBuilder
    {
        public DynamicAuthenticationBuilder(IServiceCollection services): base(services)
        { }

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
