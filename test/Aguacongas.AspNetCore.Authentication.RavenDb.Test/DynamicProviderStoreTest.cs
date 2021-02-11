// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Microsoft.Extensions.Logging;
using Moq;
using Raven.Client.Documents.Session;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.RavenDb.Test
{
    public class DynamicProviderStoreTest
    {

        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(null, null, null));
            var sessionsMock = new Mock<IAsyncDocumentSession>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(sessionsMock, null, null));
            var serializerMock = new Mock<IAuthenticationSchemeOptionsSerializer>().Object;
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(sessionsMock, serializerMock, null));
            var loggerMock = new Mock<ILogger<DynamicProviderStore<SchemeDefinition>>>().Object;
            var store = new DynamicProviderStore<SchemeDefinition>(sessionsMock, serializerMock, loggerMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync("  "));
        }
    }
}
