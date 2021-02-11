// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2021 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.RavenDb;
using Aguacongas.AspNetCore.Authentication.TestBase;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.TestDriver;
using System.Runtime.CompilerServices;
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
            builder.Services.AddSingleton(p => new RavenDbTestDriverWrapper().GetDocumentStore());
            return builder.AddRavenDbStorekStore();
        }
    }

    class RavenDbTestDriverWrapper : RavenTestDriver
    {
        public new IDocumentStore GetDocumentStore(GetDocumentStoreOptions options = null, [CallerMemberName] string database = null)
            => base.GetDocumentStore(options, database);
    }
}
