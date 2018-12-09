using Aguacongas.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TDefinition>(this AuthenticationBuilder builder)
            where TDefinition: SchemeDefinitionBase, new()
        {
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient<DynamicManager<TDefinition>>();
            return new DynamicAuthenticationBuilder(builder.Services);
        }
    }
}
