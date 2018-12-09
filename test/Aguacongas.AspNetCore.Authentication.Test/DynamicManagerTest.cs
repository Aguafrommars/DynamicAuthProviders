// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class DynamicManagerTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicManager<FakeSchemeDefinition>(null, null, null));
            var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicManager<FakeSchemeDefinition>(schemeProviderMock, null, null));
            var serviceProviderMock = new Mock<IServiceProvider>().Object;
            var factory = new OptionsMonitorCacheWrapperFactory(serviceProviderMock);
            Assert.Throws<ArgumentNullException>(() => new DynamicManager<FakeSchemeDefinition>(schemeProviderMock, factory, null));
            var storeMock = new Mock<IDynamicProviderStore<FakeSchemeDefinition>>().Object;
            var manager = new DynamicManager<FakeSchemeDefinition>(schemeProviderMock, factory, storeMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => manager.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync("  "));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => manager.FindBySchemeAsync("  "));
        }
    }
    public class FakeSchemeDefinition : SchemeDefinitionBase
    { }

}
