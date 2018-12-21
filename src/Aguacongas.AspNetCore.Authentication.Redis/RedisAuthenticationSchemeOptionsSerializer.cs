using System;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    public class RedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> : AuthenticationSchemeOptionsSerializer, IRedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> 
        where TSchemeDefinition: SchemeDefinition
    {
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

        public TSchemeDefinition Deserialize(string value)
        {
            var definition = base.Deserialize(value, typeof(TSchemeDefinition)) as TSchemeDefinition;
            definition.HandlerType = DeserializeType(definition.SerializedHandlerType);
            definition.Options = DeserializeOptions(definition.SerializedOptions, definition.HandlerType.GetAuthenticationSchemeOptionsType());
            return definition;
        }
    }
}
