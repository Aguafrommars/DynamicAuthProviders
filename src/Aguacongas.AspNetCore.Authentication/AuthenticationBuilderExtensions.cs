using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Aguacongas.AspNetCore.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <param name="builder">
        /// The builder.
        /// </param>
        /// <returns></returns>
        public static AuthenticationBuilder AddDynamic(this AuthenticationBuilder builder)
        {
            return AddDynamic<ProviderDefinition>(builder);
        }

        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddDynamic<TDefinition>(this AuthenticationBuilder builder)
            where TDefinition: ProviderDefinition, new()
        {
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient<DynamicManager<TDefinition>>();
            return new DynamicAuthenticationBuilder(builder.Services);
        }
    }
}
