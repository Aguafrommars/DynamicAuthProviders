// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using System;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class OptionsMonitorCacheWrapperTest
    {
        [Fact]
        public void Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new OptionsMonitorCacheWrapper<AuthenticationSchemeOptions>(null, null));
            var cacheMock = new Mock<IOptionsMonitorCache<AuthenticationSchemeOptions>>().Object;
            Assert.Throws<ArgumentNullException>(() => new OptionsMonitorCacheWrapper<AuthenticationSchemeOptions>(cacheMock, null));
            Assert.Throws<ArgumentNullException>(() => new OptionsMonitorCacheWrapperFactory(null));
        }
    }
}
