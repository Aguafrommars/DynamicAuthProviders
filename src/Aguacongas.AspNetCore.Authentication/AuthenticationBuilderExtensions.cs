using Aguacongas.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.Extensions.DependencyInjection
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
        public static DynamicAuthenticationBuilder AddDynamic(this AuthenticationBuilder builder)
        {
            return AddDynamic<ProviderDefinition>(builder);
        }

        /// <summary>
        /// Adds the dynamic.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the provider definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static DynamicAuthenticationBuilder AddDynamic<TDefinition>(this AuthenticationBuilder builder)
            where TDefinition: ProviderDefinition, new()
        {
            builder.Services
                .AddSingleton<OptionsMonitorCacheWrapperFactory>()
                .AddTransient<DynamicManager<TDefinition>>();
            return new DynamicAuthenticationBuilder(builder.Services);
        }
    }
}
