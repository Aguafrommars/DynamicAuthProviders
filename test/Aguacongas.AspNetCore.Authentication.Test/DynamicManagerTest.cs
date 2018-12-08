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
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.Google;
using Xunit.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.IdentityModel.Protocols.WsFederation;

namespace Aguacongas.AspNetCore.Authentication.Test
{
    public class DynamicManagerTest
    {
        private readonly ITestOutputHelper _output;
        public DynamicManagerTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task AddAsync_should_add_cookie_handler()
        {
            var eventCalled = false;
            Task onSignId(CookieSignedInContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie(configure =>
                {
                    configure.Events.OnSignedIn = onSignId;
                });
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
            var state = await VerifyAddedAsync<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, provider);

            var storedOptions = JsonConvert.DeserializeObject<CookieAuthenticationOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(cookieOptions.Cookie.Domain, storedOptions.Cookie.Domain);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnSignedIn(new CookieSignedInContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                state.options as CookieAuthenticationOptions));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_facebook_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddFacebook(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var facebookOptions = new FacebookOptions
            {
                AppId = "test",
                AppSecret = "test"
            };

            await sut.AddAsync<FacebookHandler, FacebookOptions>("test", "test", facebookOptions);
            var state = await VerifyAddedAsync<FacebookOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<FacebookOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(facebookOptions.AppId, storedOptions.AppId);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(), 
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as FacebookOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception())));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_google_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddGoogle(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var googleOptions = new GoogleOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            await sut.AddAsync<GoogleHandler, GoogleOptions>("test", "test", googleOptions);
            var state = await VerifyAddedAsync<GoogleOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<GoogleOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(googleOptions.ClientId, storedOptions.ClientId);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as GoogleOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception())));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_jwtbearer_handler()
        {
            var eventCalled = false;
            Task onMessageReceived(Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddJwtBearer(configure =>
                {
                    configure.Events = new JwtBearerEvents();
                    configure.Events.OnMessageReceived = onMessageReceived;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var jwtBearerOptions = new JwtBearerOptions
            {
                Authority = "test"
            };

            await sut.AddAsync<JwtBearerHandler, JwtBearerOptions>("test", "test", jwtBearerOptions);
            var state = await VerifyAddedAsync<JwtBearerOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<JwtBearerOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(jwtBearerOptions.Authority, storedOptions.Authority);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnMessageReceived(new Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as JwtBearerOptions));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_ms_account_handler()
        {
            var eventCalled = false;
            Task onCreatingTicket(OAuthCreatingTicketContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddMicrosoftAccount(configure =>
                {
                    configure.Events.OnCreatingTicket = onCreatingTicket;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var msAccountOptions = new MicrosoftAccountOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            await sut.AddAsync<MicrosoftAccountHandler, MicrosoftAccountOptions>("test", "test", msAccountOptions);
            var state = await VerifyAddedAsync<MicrosoftAccountOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<MicrosoftAccountOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(msAccountOptions.ClientId, storedOptions.ClientId);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnCreatingTicket(new OAuthCreatingTicketContext(
                new ClaimsPrincipal(),
                new AuthenticationProperties(),
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as MicrosoftAccountOptions,
                new HttpClient(),
                OAuthTokenResponse.Failed(new Exception())));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_oidc_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddOpenIdConnect(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var msAccountOptions = new OpenIdConnectOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            await sut.AddAsync<OpenIdConnectHandler, OpenIdConnectOptions>("test", "test", msAccountOptions);
            var state = await VerifyAddedAsync<OpenIdConnectOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<OpenIdConnectOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(msAccountOptions.ClientId, storedOptions.ClientId);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,                
                state.scheme as AuthenticationScheme,
                state.options as OpenIdConnectOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_twitter_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddTwitter(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var twittertOptions = new TwitterOptions
            {
                ConsumerKey = "test",
                ConsumerSecret = "test"
            };

            await sut.AddAsync<TwitterHandler, TwitterOptions>("test", "test", twittertOptions);
            var state = await VerifyAddedAsync<TwitterOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<TwitterOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(twittertOptions.ConsumerKey, storedOptions.ConsumerKey);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as TwitterOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }

        [Fact]
        public async Task AddAsync_should_add_ws_federation_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddWsFederation(configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var sut = provider.GetRequiredService<DynamicManager>();
            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            await sut.AddAsync<WsFederationHandler, WsFederationOptions>("test", "test", wsFederationOptions);
            var state = await VerifyAddedAsync<WsFederationOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<WsFederationOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(wsFederationOptions.Configuration.Issuer, storedOptions.Configuration.Issuer);
            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as WsFederationOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }

        protected virtual IServiceCollection AddStore(IServiceCollection services)
        {
            return services.AddEntityFrameworkStore<ProviderDbContext>(options =>
            {
                options.UseInMemoryDatabase(Guid.NewGuid().ToString());
            });
        }

        private static async Task<dynamic> VerifyAddedAsync<TOptions>(string schemeName, IServiceProvider provider) where TOptions : AuthenticationSchemeOptions
        {
            var store = provider.GetRequiredService<IDynamicProviderStore<ProviderDefinition>>();
            var definition = await store.FindBySchemeAsync(schemeName);
            Assert.NotNull(definition);
            var schemeProvider = provider.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = await schemeProvider.GetSchemeAsync(schemeName);
            Assert.NotNull(scheme);
            Assert.Equal("test", scheme.DisplayName);

            var optionsMonitorCache = provider.GetRequiredService<IOptionsMonitorCache<TOptions>>();
            var options = optionsMonitorCache.GetOrAdd(schemeName, () => default(TOptions));
            Assert.NotNull(options);
            return new { definition, scheme, options };
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
                .AddAuthentication()
                .AddDynamic();

            addHandlers?.Invoke(provider);

            return services.BuildServiceProvider();
        }       
    }
}
