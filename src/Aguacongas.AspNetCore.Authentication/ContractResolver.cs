// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Reflection;

namespace Aguacongas.AspNetCore.Authentication
{
    /// <summary>
    /// Ignore delegate, interface and read-only property ContractResolver
    /// </summary>
    public class ContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty"/> for the given <see cref="T:System.Reflection.MemberInfo"/>.
        /// </summary>
        /// <param name="member">
        /// The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty"/> for.
        /// </param>
        /// <param name="memberSerialization">
        /// The member's parent <see cref="T:Newtonsoft.Json.MemberSerialization"/>.
        /// </param>
        /// <returns>
        /// A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty"/> for the given <see cref="T:System.Reflection.MemberInfo"/>.
        /// </returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            var propertyInfo = member as PropertyInfo;
            var propertyType = propertyInfo?.PropertyType;
            property.ShouldSerialize = instance => propertyType != null && !propertyType.IsInterface && propertyInfo.CanWrite && !propertyType.IsSubclassOf(typeof(Delegate));

            return property;
        }        
    }
}
