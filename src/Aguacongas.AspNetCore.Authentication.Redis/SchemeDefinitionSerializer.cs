// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using System;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    /// <summary>
    /// <see cref="SchemeDefinition"/> serializer.
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.AuthenticationSchemeOptionsSerializer" />
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.Redis.ISchemeDefinitionSerializer{TSchemeDefinition}" />
    public class SchemeDefinitionSerializer<TSchemeDefinition> : AuthenticationSchemeOptionsSerializer, ISchemeDefinitionSerializer<TSchemeDefinition> 
        where TSchemeDefinition: SchemeDefinition
    {
        /// <summary>
        /// Serializes the specified definition.
        /// </summary>
        /// <param name="definition">The definition.</param>
        /// <returns></returns>
        public string Serialize(TSchemeDefinition definition)
        {
            var options = definition.Options;
            var type = definition.HandlerType;
            definition.HandlerType = null;
            definition.Options = null;

            definition.SerializedHandlerType = SerializeType(type);
            definition.SerializedOptions = SerializeOptions(options, type.GetAuthenticationSchemeOptionsType());
            
            var result = Serialize(definition, typeof(TSchemeDefinition));

            definition.HandlerType = type;
            definition.Options = options;

            return result;
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public TSchemeDefinition Deserialize(string value)
        {
            var definition = base.Deserialize(value, typeof(TSchemeDefinition)) as TSchemeDefinition;
            definition.HandlerType = DeserializeType(definition.SerializedHandlerType);
            definition.Options = DeserializeOptions(definition.SerializedOptions, definition.HandlerType.GetAuthenticationSchemeOptionsType());
            return definition;
        }
    }
}
