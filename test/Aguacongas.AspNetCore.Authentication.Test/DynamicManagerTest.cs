// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class PersistentDynamicManagerTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new PersistentDynamicManager<FakeSchemeDefinition>(null, null, null, null));
            var schemeProviderMock = new Mock<IAuthenticationSchemeProvider>().Object;
            Assert.Throws<ArgumentNullException>(() => new PersistentDynamicManager<FakeSchemeDefinition>(schemeProviderMock, null, null, null));
            var serviceProviderMock = new Mock<IServiceProvider>().Object;
            var factory = new OptionsMonitorCacheWrapperFactory(serviceProviderMock);
            Assert.Throws<ArgumentNullException>(() => new PersistentDynamicManager<FakeSchemeDefinition>(schemeProviderMock, factory, null, null));
            var storeMock = new Mock<IDynamicProviderStore<FakeSchemeDefinition>>().Object;
            var manager = new PersistentDynamicManager<FakeSchemeDefinition>(schemeProviderMock, factory, storeMock, new List<Type>());
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
            var manager = new PersistentDynamicManager<FakeSchemeDefinition>(schemeProviderMock, factory, storeMock, new List<Type>());
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
