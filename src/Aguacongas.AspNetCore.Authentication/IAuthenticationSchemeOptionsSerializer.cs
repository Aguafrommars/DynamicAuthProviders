// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using System;
using Microsoft.AspNetCore.Authentication;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// <see cref="AuthenticationSchemeOptions"/> serializer interface
    /// </summary>
    public interface IAuthenticationSchemeOptionsSerializer
    {
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>An AuthenticationSchemeOptions instance.</returns>
        AuthenticationSchemeOptions Deserialize(string value, Type optionsType);
        /// <summary>
        /// Serializes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>The serialized result.</returns>
        string Serialize(AuthenticationSchemeOptions options, Type optionsType);
    }
}