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
using Xunit.Sdk;

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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            await sut.AddAsync(CookieAuthenticationDefaults.AuthenticationScheme, "test", typeof(CookieAuthenticationHandler), cookieOptions);
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();
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
        [Fact]
        public async Task UpdateAsync_should_update_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie()
                    .AddWsFederation(configure =>
                    {
                        configure.Events.OnTicketReceived = onTicketReceived;
                    });
            });

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            await sut.AddAsync<CookieAuthenticationHandler, CookieAuthenticationOptions>("test", "test", cookieOptions);            
            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);

            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            await sut.UpdateAsync<WsFederationHandler, WsFederationOptions>("test", "new name", wsFederationOptions);
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

        [Fact]
        public async Task RemoveAsync_should_remove_handler()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            await sut.AddAsync<CookieAuthenticationHandler, CookieAuthenticationOptions>("test", "test", cookieOptions);
            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);

            await sut.RemoveAsync("test");
            await Assert.ThrowsAsync<NotNullException>(() => VerifyAddedAsync<WsFederationOptions>("test", provider));
        }

        [Fact]
        public async Task Load_should_load_configuration()
        {
            var provider = CreateServiceProvider(options =>
            {
                options.AddCookie();
            });

            var store = provider.GetRequiredService<IDynamicProviderStore<ProviderDefinition>>();

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var definition = new ProviderDefinition
            {
                Id = "test",
                HandlerTypeName = typeof(CookieAuthenticationHandler).FullName,
                SerializedOptions = JsonConvert.SerializeObject(cookieOptions, DynamicManager<ProviderDefinition>.JsonSerializerSettings)
            };

            await store.AddAsync(definition);

            var sut = provider.GetRequiredService<DynamicManager<ProviderDefinition>>();

            sut.Load();

            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);
        }
        protected virtual DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder)
        {
            return builder.AddEntityFrameworkStore(options =>
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

            var optionsMonitorCache = provider.GetRequiredService<IOptionsMonitorCache<TOptions>>();
            var options = optionsMonitorCache.GetOrAdd(schemeName, () => default(TOptions));
            Assert.NotNull(options);
            return new { definition, scheme, options };
        }

        private IServiceProvider CreateServiceProvider(Action<AuthenticationBuilder> addHandlers = null)
        {
            var services = new ServiceCollection();
            var builder = services.AddLogging(configure =>
                {
                    configure.AddConsole()
                        .AddDebug();
                })
                .AddAuthentication()
                .AddDynamic();

            AddStore(builder);

            addHandlers?.Invoke(builder);

            return services.BuildServiceProvider();
        }       
    }
}
