// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
namespace Aguacongas.AspNetCore.Authentication.Redis
{
    /// <summary>
    /// Scheme definition for entity framework store
    /// </summary>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.SchemeDefinitionBase" />
    public class SchemeDefinition : SchemeDefinitionBase
    {
        /// <summary>
        /// Gets or sets the serialized handler type.
        /// </summary>
        /// <value>
        /// The name of the serialized handler type.
        /// </value>
        public string SerializedHandlerType { get; set; }

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
        public long ConcurrencyStamp { get; set; }
    }
}
