// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using System;

namespace Aguacongas.AspNetCore.Authentication.RavenDb
{
    /// <summary>
    /// Scheme definition for entity framework store
    /// </summary>
    /// <seealso cref="SchemeDefinitionBase" />
    public class SchemeDefinition: SchemeDefinitionBase, ICloneable
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
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
