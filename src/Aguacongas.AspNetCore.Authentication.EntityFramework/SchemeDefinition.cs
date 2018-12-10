// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Scheme definition for entity framework store
    /// </summary>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.SchemeDefinitionBase" />
    public class SchemeDefinition: SchemeDefinitionBase
    {
        /// <summary>
        /// Gets or sets the name of the handler type.
        /// </summary>
        /// <value>
        /// The name of the handler type.
        /// </value>
        public string HandlerTypeName { get; set; }

        /// <summary>
        /// Gets or sets the serialized options.
        /// </summary>
        /// <value>
        /// The serialized options.
        /// </value>
        public string SerializedOptions { get; set; }

        /// <summary>
        /// Gets or sets the concurrency stamp.
        /// </summary>
        /// <value>
        /// The concurrency stamp.
        /// </value>
        public string ConcurrencyStamp { get; set; }
    }
}
