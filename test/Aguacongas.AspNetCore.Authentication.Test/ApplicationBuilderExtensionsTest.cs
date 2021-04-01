using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class ApplicationBuilderExtensionsTest
    {
        [Fact]
        public void LoadDynamicAuthenticationConfiguration_should_load_WsFederationOptions()
        {
            var serializer = new AuthenticationSchemeOptionsSerializer();
            var serialized = serializer.SerializeOptions(new WsFederationOptions
            {
                RequireHttpsMetadata = false,
                MetadataAddress = "http://localhost:5000/wsfederation",
                Wtrealm = "urm:aspnetcorerp"
            }, typeof(WsFederationOptions));
            var storeMock = new Mock<IDynamicProviderStore<SchemeDefinition>>();
            storeMock.Setup(m => m.SchemeDefinitions).Returns(new List<string>
            {
                serialized
            }.Select(s => new SchemeDefinition
            {
                DisplayName = "test",
                HandlerType = typeof(WsFederationHandler),
                Options = serializer.DeserializeOptions(s, typeof(WsFederationOptions)),
                Scheme = "test"
            }).AsQueryable());
            var services = new ServiceCollection().AddTransient(p => storeMock.Object);

            services.AddAuthentication()
                .AddDynamic<SchemeDefinition>()
                .AddWsFederation();

            var provider = services.BuildServiceProvider();
            var applicationBuilderMock = new Mock<IApplicationBuilder>();
            applicationBuilderMock.SetupGet(m => m.ApplicationServices).Returns(provider);

            applicationBuilderMock.Object.LoadDynamicAuthenticationConfiguration<SchemeDefinition>();

            Assert.True(true);
        }

        public class SchemeDefinition : SchemeDefinitionBase
        {

        }
    }
}
