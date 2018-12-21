// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.EntityFramework;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// IApplicationBuilder extensions
    /// </summary>
    public static  class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Loads the dynamic authentication configuration.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder LoadDynamicAuthenticationConfiguration(this IApplicationBuilder builder)
        {
            builder.ApplicationServices.LoadDynamicAuthenticationConfiguration<SchemeDefinition>();
            return builder;
        }
    }
}
