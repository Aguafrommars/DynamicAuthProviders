using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.AspNetCore.Authentication.Redis
{
    public class RedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> : AuthenticationSchemeOptionsSerializer, IRedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition> 
        where TSchemeDefinition: SchemeDefinition
    {
        public string Serialize(TSchemeDefinition definition)
        {
            definition.SerializedHandlerType = SerializeType(definition.HandlerType);
            return Serialize(definition, typeof(TSchemeDefinition));
        }

        public TSchemeDefinition Deserialize(string value)
        {
            var definition = base.Deserialize(value, typeof(TSchemeDefinition)) as TSchemeDefinition;
            definition.HandlerType = DeserializeType(definition.SerializedHandlerType);
            return definition;
        }
    }
}
