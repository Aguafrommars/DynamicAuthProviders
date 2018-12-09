// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.AspNetCore.Authentication
{
    public class AuthenticationSchemeOptionsSerializer : IAuthenticationSchemeOptionsSerializer
    {
        /// <summary>
        /// Gets or sets the json serializer settings.
        /// </summary>
        /// <value>
        /// The json serializer settings.
        /// </value>
        public static JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ContractResolver = new ContractResolver()
        };

        public string Serialize(AuthenticationSchemeOptions options, Type optionsType)
        {
            return JsonConvert.SerializeObject(options, optionsType, JsonSerializerSettings);
        }

        public AuthenticationSchemeOptions Deserialize(string value, Type optionsType)
        {
            return JsonConvert.DeserializeObject(value, optionsType) as AuthenticationSchemeOptions;
        }

    }
}
