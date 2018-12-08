using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class DynamicManagerTest
    {
        [Fact]
        public async Task AddAsync_should_add_cookie_handler()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            await sut.AddAsync(CookieAuthenticationDefaults.AuthenticationScheme, "test", typeof(CookieAuthenticationHandler), (AuthenticationSchemeOptions)cookieOptions);
            var definition = await VerifyAddedAsync<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, provider);

            var storedOptions = JsonConvert.DeserializeObject<CookieAuthenticationOptions>(definition.SerializedOptions);
            
            Assert.Equal(cookieOptions.Cookie.Domain, storedOptions.Cookie.Domain);
        }


        protected virtual IServiceCollection AddStore(IServiceCollection services)
        {
            return services.AddEntityFrameworkStore<ProviderDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        }

        private static async Task<ProviderDefinition> VerifyAddedAsync<TOptions>(string schemeName, IServiceProvider provider) where TOptions : AuthenticationSchemeOptions
        {
            var store = provider.GetRequiredService<IDynamicProviderStore>();
            var definition = await store.FindBySchemeAsync(schemeName);
            Assert.NotNull(definition);
            var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(schemeName);
            Assert.NotNull(scheme);
            Assert.Equal("test", scheme.DisplayName);

            var optionsMonitorCache = provider.GetRequiredService<IOptionsMonitorCache<TOptions>>();
            Assert.NotNull(optionsMonitorCache.GetOrAdd(schemeName, () => default(TOptions)));
            return definition;
        }

        private IServiceProvider CreateServiceProvider(Action<AuthenticationBuilder> addHandlers = null)
        {
            var services = new ServiceCollection();
            var provider = AddStore(services)
                .AddLogging(configure =>
                {
                    configure.AddConsole()
                        .AddDebug();
                })
                .AddAuthentication();

            provider.AddDynamic();

            addHandlers?.Invoke(provider);

            return services.BuildServiceProvider();
        }       
    }
}
