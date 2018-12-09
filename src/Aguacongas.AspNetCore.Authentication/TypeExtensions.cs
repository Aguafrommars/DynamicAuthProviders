// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;

namespace System
{
    public static class TypeExtensions
    {
        public static Type GetAuthenticationSchemeOptionsType(this Type handlerType)
        {
            if (handlerType.GetInterface(nameof(IAuthenticationHandler)) == null)
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            var genericTypeArguments = GetGenericTypeArguments(handlerType);
            var optionsType = genericTypeArguments[0];
            return optionsType;
        }

        private static Type[] GetGenericTypeArguments(Type handlerType)
        {
            if (handlerType.GenericTypeArguments.Length == 1)
            {
                return handlerType.GenericTypeArguments;
            }

            if (handlerType.BaseType == null || handlerType.BaseType == typeof(object))
            {
                throw new ArgumentException($"Parameter {nameof(handlerType)} should be a {nameof(AuthenticationHandler<AuthenticationSchemeOptions>)}");
            }

            return GetGenericTypeArguments(handlerType.BaseType);
        }
    }
}
