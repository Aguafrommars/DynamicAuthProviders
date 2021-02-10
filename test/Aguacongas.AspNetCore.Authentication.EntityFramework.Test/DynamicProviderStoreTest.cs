// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework.Test
{
    public class DynamicProviderStoreTest
    {
        [Fact]
        public async Task Assertions()
        {
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(null, null, null));
            var contextMock = new SchemeDbContext<SchemeDefinition>(new FakeDbContextOptions());
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(contextMock, null, null));
            Assert.Throws<ArgumentNullException>(() => new DynamicProviderStore<SchemeDefinition>(contextMock, new AuthenticationSchemeOptionsSerializer(), null));
            var loggerMock = new Mock<ILogger<DynamicProviderStore<SchemeDefinition>>>().Object;
            var store = new DynamicProviderStore<SchemeDefinition>(contextMock, new AuthenticationSchemeOptionsSerializer(), loggerMock);
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.AddAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.UpdateAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => store.RemoveAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(null));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => store.FindBySchemeAsync("  "));
        }
    }

    class FakeDbContextOptions : DbContextOptions
    {
        public FakeDbContextOptions() : base(new Mock<IReadOnlyDictionary<Type, IDbContextOptionsExtension>>().Object)
        {
        }

        public override Type ContextType => typeof(SchemeDbContext<SchemeDefinition>);

        public override DbContextOptions WithExtension<TExtension>(TExtension extension)
        {
            throw new NotImplementedException();
        }
    }
}
