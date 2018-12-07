using Microsoft.AspNetCore.Authentication;

namespace Aguacongas.AspNetCore.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddDynamic(this AuthenticationBuilder builder)
        {
            return new DynamicAuthenticationBuilder(builder.Services);
        }
    }
}
