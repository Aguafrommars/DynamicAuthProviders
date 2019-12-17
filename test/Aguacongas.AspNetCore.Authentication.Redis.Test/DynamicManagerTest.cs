// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    [Collection("Redis")]
    public class DynamicManagerTest: DynamicManagerTestBase<SchemeDefinition>
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
