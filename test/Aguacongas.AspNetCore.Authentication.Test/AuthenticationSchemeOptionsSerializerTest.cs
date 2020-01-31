// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication.OAuth;
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
    }
}
