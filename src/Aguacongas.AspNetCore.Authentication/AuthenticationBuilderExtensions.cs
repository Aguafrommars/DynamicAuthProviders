using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Aguacongas.AspNetCore.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddDynamic(this AuthenticationBuilder builder)
        {
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient<DynamicManager>();
            return new DynamicAuthenticationBuilder(builder.Services);
        }
    }
}
