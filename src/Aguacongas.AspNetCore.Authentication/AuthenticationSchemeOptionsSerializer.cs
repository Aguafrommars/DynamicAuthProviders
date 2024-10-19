// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Converter For x509 Data
    /// </summary>
    public class X509Certificate2JsonConverter : JsonConverter
    {
        /// <summary>
        /// Is the object fed to this a match for our converter type.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(X509Certificate2);
        }

        /// <summary>
        /// Read method. 
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="objectType"></param>
        /// <param name="existingValue"></param>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            var deserializedRaw = serializer.Deserialize<byte[]>(reader);
            var deserialized = new X509Certificate2(deserializedRaw);
            return deserialized;
        }

        /// <summary>
        /// Write method.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            byte[] certData = ((X509Certificate2)value).Export(X509ContentType.Pfx);
            serializer.Serialize(writer, certData);
        }
    }

    /// <summary>
    /// Manage <see cref="AuthenticationSchemeOptions"/> serialization.
    /// </summary>
    /// <seealso cref="Aguacongas.AspNetCore.Authentication.IAuthenticationSchemeOptionsSerializer" />
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
            DefaultValueHandling = DefaultValueHandling.Include,
            ContractResolver = new ContractResolver(),
            Converters =
            {
                new X509Certificate2JsonConverter()
            }
        };

        /// <summary>
        /// Serializes the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>
        /// The serialized result.
        /// </returns>
        public virtual string SerializeOptions(AuthenticationSchemeOptions options, Type optionsType)
        {
            return Serialize(options, optionsType);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="optionsType">Type of the options.</param>
        /// <returns>
        /// An AuthenticationSchemeOptions instance.
        /// </returns>
        public virtual AuthenticationSchemeOptions DeserializeOptions(string value, Type optionsType)
        {
            return Deserialize(value, optionsType) as AuthenticationSchemeOptions;
        }

        /// <summary>
        /// Deserializes the type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual Type DeserializeType(string value)
        {
            return GetType(Deserialize(value, typeof(TypeDefinition)) as TypeDefinition);
        }

        /// <summary>
        /// Serializes the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual string SerializeType(Type type)
        {
            return Serialize(CreateTypeDefinition(type), typeof(Type));
        }

        /// <summary>
        /// Serializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected virtual string Serialize(object value, Type type)
        {
            return JsonConvert.SerializeObject(value, type, JsonSerializerSettings);
        }

        /// <summary>
        /// Deserializes the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        protected virtual object Deserialize(string value, Type type)
        {
            var result = JsonConvert.DeserializeObject(value, type, JsonSerializerSettings);
            var requireHttpsMetaDataProperty = type.GetProperty("RequireHttpsMetadata");
            if (requireHttpsMetaDataProperty != null && value.Contains("\"RequireHttpsMetadata\":false"))
            {
                requireHttpsMetaDataProperty.SetValue(result, false);
            }

            return result;
        }

        private static Type GetType(string typeName)
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            return runtimeAssemblyNames
                    .Select(Assembly.Load)
                    .Select(a => a.GetType(typeName))
                    .First(t => t != null);
        }

        private Type GetType(TypeDefinition typeDefinition)
        {
            if (typeDefinition.ArgsTypeDefinition != null)
            {
                var type = GetType(typeDefinition.Name);
                var argsTypes = typeDefinition.ArgsTypeDefinition.Select(GetType).ToArray();

                return type.MakeGenericType(argsTypes);
            }
            return GetType(typeDefinition.Name);
        }

        private TypeDefinition CreateTypeDefinition(Type type)
        {
            if (type.IsGenericType)
            {
                var genericType = type.GetGenericTypeDefinition();
                return new TypeDefinition
                {
                    Name = genericType.FullName,
                    ArgsTypeDefinition = type
                       .GetGenericArguments().Select(CreateTypeDefinition).ToArray()
                };
            }

            return new TypeDefinition
            {
                Name = type.FullName
            };
        }

        class TypeDefinition
        {
            public string Name { get; set; }

            public TypeDefinition[] ArgsTypeDefinition { get; set; }
        }
    }
}
