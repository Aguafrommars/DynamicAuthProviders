// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
namespace Aguacongas.AspNetCore.Authentication.Redis
{
    /// <summary>
    /// <see cref="SchemeDefinition"/> serializer interface.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public interface ISchemeDefinitionSerializer<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition
    {
        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        TSchemeDefinition Deserialize(string value);
        /// <summary>
        /// Serializes the specified definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns></returns>
        string Serialize(TSchemeDefinition definition);
    }
}