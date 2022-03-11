// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class AuthenticationSchemeOptionsSerializerTest
    {
        [Fact]
        public void SerializeOptions_should_serialize_OAuthOptionsScope()
        {
            var oAuthOptions = new OAuthOptions
            {
                ClientId = "test",
            };

            oAuthOptions.Scope.Add("test");

            var sut = new AuthenticationSchemeOptionsSerializer();

            var result = sut.SerializeOptions(oAuthOptions, typeof(OAuthOptions));

            var expected = sut.DeserializeOptions(result, typeof(OAuthOptions)) as OAuthOptions;

            Assert.Contains(expected.Scope, value => value == "test");
        }

        [Fact]
        public void DeserializeOptions_should_deserialize_WsFederationOptions()
        {
            var serialized = "{\"RemoteSignOutPath\":\"/signin-wsfed\",\"AllowUnsolicitedLogins\":false,\"RequireHttpsMetadata\":false,\"UseTokenLifetime\":true,\"Wtrealm\":\"urn:aspnetcorerp\",\"SignOutWreply\":null,\"Wreply\":null,\"SkipUnrecognizedRequests\":false,\"RefreshOnIssuerKeyNotFound\":true,\"MetadataAddress\":\"http://localhost:5001/wsfederation\",\"SignOutScheme\":null,\"SaveTokens\":false}";

            var sut = new AuthenticationSchemeOptionsSerializer();

            var result = sut.DeserializeOptions(serialized, typeof(WsFederationOptions)) as WsFederationOptions;

            Assert.False(result.RequireHttpsMetadata);
        }

        [Fact]
        public void SeserializeOptions_should_seserialize_default_value()
        {
            var sut = new AuthenticationSchemeOptionsSerializer();
            var result = sut.SerializeOptions(new WsFederationOptions
            {
                RequireHttpsMetadata = false
            }, typeof(WsFederationOptions));

            Assert.Contains(nameof(WsFederationOptions.RequireHttpsMetadata), result);

            var deserialized = sut.DeserializeOptions(result, typeof(WsFederationOptions)) as WsFederationOptions;

            Assert.False(deserialized.RequireHttpsMetadata);
        }

        [Fact]
        public void SerializationOptions_should_serialize_x509_certificate()
        {
            var holder = new CertificateHolder();
            holder.Certificate = CertificateHolder.CreateCertificate();
            var str = JsonConvert.SerializeObject(holder);
            var holderRestored = JsonConvert.DeserializeObject<CertificateHolder>(str);
            var holderStr = holder.Certificate.ToString();
            var holderRestoredStr = holderRestored.Certificate.ToString();
            Assert.Equal(holderStr, holderRestoredStr);
            holder.Dispose();
            holderRestored.Dispose();
        }
    }

    public class CertificateHolder : IDisposable
    {
        [Newtonsoft.Json.JsonConverter(typeof(X509Certificate2JsonConverter))]
        public X509Certificate2 Certificate { get; set; }

        public void Dispose() => Certificate?.Dispose();

        public static X509Certificate2 CreateCertificate()
        {
            var ecdsa = ECDsa.Create();
            var req = new CertificateRequest("cn=foobar", ecdsa, HashAlgorithmName.SHA256);
            var c = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(5));
            return c;
        }
    }

}
