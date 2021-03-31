// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.WsFederation;
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
    }
}
