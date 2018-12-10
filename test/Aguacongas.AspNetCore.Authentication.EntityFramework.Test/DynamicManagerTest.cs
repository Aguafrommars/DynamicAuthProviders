// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework.Test
{
    public class DynamicManagerTest: DynamicManagerTestBase<SchemeDefinition>
    {
        public DynamicManagerTest(ITestOutputHelper output): base(output)
        {
        }

        protected override DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder)
        {
            return builder.AddEntityFrameworkStore(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        }
    }
}
