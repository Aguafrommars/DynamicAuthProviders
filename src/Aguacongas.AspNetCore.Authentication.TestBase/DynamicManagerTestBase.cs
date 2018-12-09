// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Moq;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Aguacongas.AspNetCore.Authentication.TestBase
{
    /// <summary>
    /// Base test suite to verify if the store implementation work as expecter
    /// </summary>
    /// <typeparam name="TSchemeDefinition">The type of the scheme definition.</typeparam>
    public abstract class DynamicManagerTestBase<TSchemeDefinition>
        where TSchemeDefinition: SchemeDefinitionBase, new()
    {
        private readonly ITestOutputHelper _output;
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicManagerTestBase{TSchemeDefinition}"/> class.
        /// </summary>
        /// <param name="output">The output.</param>
        public DynamicManagerTestBase(ITestOutputHelper output)
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

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var definition = new TSchemeDefinition
            {
                Scheme = CookieAuthenticationDefaults.AuthenticationScheme,
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };
            await sut.AddAsync(definition);
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

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            var facebookOptions = new FacebookOptions
            {
                AppId = "test",
                AppSecret = "test"
            };
            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(FacebookHandler),
                Options = facebookOptions
            };

            await sut.AddAsync(definition);
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

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            var googleOptions = new GoogleOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(GoogleHandler),
                Options = googleOptions
            };
            await sut.AddAsync(definition);
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

            var jwtBearerOptions = new JwtBearerOptions
            {
                Authority = "test"
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(JwtBearerHandler),
                Options = jwtBearerOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
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

            var msAccountOptions = new MicrosoftAccountOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(MicrosoftAccountHandler),
                Options = msAccountOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
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

            var oidcptions = new OpenIdConnectOptions
            {
                ClientId = "test",
                ClientSecret = "test"
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(OpenIdConnectHandler),
                Options = oidcptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<OpenIdConnectOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<OpenIdConnectOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(oidcptions.ClientId, storedOptions.ClientId);
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

            var twittertOptions = new TwitterOptions
            {
                ConsumerKey = "test",
                ConsumerSecret = "test"
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(TwitterHandler),
                Options = twittertOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
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

            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(WsFederationHandler),
                Options = wsFederationOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
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

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.UpdateAsync(definition));

            await sut.AddAsync(definition);
            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);

            var wsFederationOptions = new WsFederationOptions
            {
                Configuration = new WsFederationConfiguration
                {
                    Issuer = "test"
                }
            };

            definition.Options = wsFederationOptions;
            definition.DisplayName = "new name";
            definition.HandlerType = typeof(WsFederationHandler);

            await sut.UpdateAsync(definition);
            var state = await VerifyAddedAsync<WsFederationOptions>("test", provider);

            var storedOptions = JsonConvert.DeserializeObject<WsFederationOptions>(state.definition.SerializedOptions);
            _output.WriteLine(state.definition.SerializedOptions);

            Assert.Equal(wsFederationOptions.Configuration.Issuer, storedOptions.Configuration.Issuer);
            Assert.Equal(state.scheme.DisplayName, definition.DisplayName);

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

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();
            await sut.AddAsync(definition);
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

            var store = provider.GetRequiredService<IDynamicProviderStore<TSchemeDefinition>>();

            var cookieOptions = new CookieAuthenticationOptions
            {
                Cookie = new CookieBuilder
                {
                    Domain = "test"
                }
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                HandlerType = typeof(CookieAuthenticationHandler),
                Options = cookieOptions
            };

            await store.AddAsync(definition);

            var sut = provider.GetRequiredService<DynamicManager<TSchemeDefinition>>();

            sut.Load();

            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);
        }
        protected abstract DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder);

        private static async Task<dynamic> VerifyAddedAsync<TOptions>(string schemeName, IServiceProvider provider) where TOptions : AuthenticationSchemeOptions
        {
            var store = provider.GetRequiredService<IDynamicProviderStore<TSchemeDefinition>>();
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
                .AddDynamic<TSchemeDefinition>(null);

            AddStore(builder);

            addHandlers?.Invoke(builder);

            return services.BuildServiceProvider();
        }
    }
}
