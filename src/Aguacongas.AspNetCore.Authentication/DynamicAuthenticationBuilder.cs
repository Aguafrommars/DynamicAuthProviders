using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.AspNetCore.Authentication
{
    public class DynamicAuthenticationBuilder : AuthenticationBuilder
    {
        public DynamicAuthenticationBuilder(IServiceCollection services): base(services)
        { }

        public override AuthenticationBuilder AddScheme<TOptions, THandler>(string authenticationScheme, string displayName, Action<TOptions> configureOptions)
        {
            Services.AddTransient<THandler>();
            return this;
        }
    }
}
