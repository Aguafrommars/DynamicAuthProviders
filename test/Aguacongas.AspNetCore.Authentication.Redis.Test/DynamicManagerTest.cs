// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    public class DynamicManagerTest: DynamicManagerTestBase<SchemeDefinition>, IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        public DynamicManagerTest(ITestOutputHelper output, TestFixture fixture): base(output)
        {
            _fixture = fixture;
        }

        protected override DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder)
        {
            return builder.AddRedisStore(provider => _fixture.Database);
        }
    }
}
