// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    public class DynamicProviderStoreTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(null, null, null));
            var databaseMock = new Mock<IDatabase>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(databaseMock, null, null));
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(databaseMock, new RedisAuthenticationSchemeOptionsSerializer<SchemeDefinition>(), null));
            var loggerMock = new Mock<ILogger<DynamicProviderStore<SchemeDefinition>>>().Object;
            var store = new DynamicProviderStore<SchemeDefinition>(databaseMock, new RedisAuthenticationSchemeOptionsSerializer<SchemeDefinition>(), loggerMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync("  "));
        }
    }
}
