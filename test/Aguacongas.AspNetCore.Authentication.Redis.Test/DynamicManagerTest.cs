// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.Redis.Test
{
    public class DynamicManagerTest: DynamicManagerTestBase<SchemeDefinition>
    {
        public DynamicManagerTest(ITestOutputHelper output): base(output)
        {
        }

        protected override DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder)
        {            
            return builder.AddRedisStore("localhost:6379");
        }
    }
}
