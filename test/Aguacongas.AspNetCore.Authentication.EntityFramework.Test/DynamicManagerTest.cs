// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit.Abstractions;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework.Test
{
    public class DynamicManagerTest: DynamicManagerTestBase<SchemeDefinition>
    {
        private readonly string dbName = Guid.NewGuid().ToString();
        public DynamicManagerTest(ITestOutputHelper output): base(output)
        {
        }

        protected override DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder)
        {
            builder.Services.AddDbContext<SchemeDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
            });
            return builder.AddEntityFrameworkStore<SchemeDbContext>();
        }
    }
}
