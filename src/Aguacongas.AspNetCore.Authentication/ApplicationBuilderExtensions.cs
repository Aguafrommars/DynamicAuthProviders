using Aguacongas.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder LoadDynamicAuthenticationConfiguration(this IApplicationBuilder builder)
        {
            return LoadDynamicAuthenticationConfiguration<ProviderDefinition>(builder);
        }

        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <typeparam name="TDefinition">The type of the definition.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder LoadDynamicAuthenticationConfiguration<TDefinition>(this IApplicationBuilder builder)
            where TDefinition: ProviderDefinition, new()
        {
            var manager = builder.ApplicationServices.GetRequiredService<DynamicManager<TDefinition>>();
            manager.Load();
            return builder;
        }
    }
}
