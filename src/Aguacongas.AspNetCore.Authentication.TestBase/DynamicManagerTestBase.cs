// Project: aguacongas/DymamicAuthProviders
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
using System.Text.Encodings.Web;
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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.ManagedHandlerType);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(FacebookHandler), sut.ManagedHandlerType);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(GoogleHandler), sut.ManagedHandlerType);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(JwtBearerHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<JwtBearerOptions>("test", provider);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(MicrosoftAccountHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<MicrosoftAccountOptions>("test", provider);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(OpenIdConnectHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<OpenIdConnectOptions>("test", provider);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(TwitterHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<TwitterOptions>("test", provider);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(WsFederationHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<WsFederationOptions>("test", provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as WsFederationOptions,
                new AuthenticationTicket(new ClaimsPrincipal(), "test")));

            Assert.True(eventCalled);
        }
        
        [Fact]
        public async Task AddAsync_should_add_generic_handler()
        {
            var eventCalled = false;
            Task onTicketReceived(TicketReceivedContext context)
            {
                eventCalled = true;
                return Task.CompletedTask;
            }

            var provider = CreateServiceProvider(options =>
            {
                options.AddScheme<OAuthOptions, FakeGenericHandler<string, OAuthOptions>>("test", configure =>
                {
                    configure.Events.OnTicketReceived = onTicketReceived;
                });
            });

            var oAuthOptions = new OAuthOptions
            {
                ClientId = "test"                
            };

            var definition = new TSchemeDefinition
            {
                Scheme = "test",
                DisplayName = "test",
                HandlerType = typeof(FakeGenericHandler<string, OAuthOptions>),
                Options = oAuthOptions
            };

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(FakeGenericHandler<string, OAuthOptions>), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            var state = await VerifyAddedAsync<OAuthOptions>("test", provider);

            var httpContext = new Mock<HttpContext>().Object;
            state.options.Events.OnTicketReceived(new TicketReceivedContext(
                httpContext,
                state.scheme as AuthenticationScheme,
                state.options as OAuthOptions,
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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.ManagedHandlerType);
            Assert.Contains(typeof(WsFederationHandler), sut.ManagedHandlerType);

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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();
            Assert.Contains(typeof(CookieAuthenticationHandler), sut.ManagedHandlerType);

            await sut.AddAsync(definition);
            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);

            await sut.RemoveAsync("test");
            await Assert.ThrowsAsync<NullReferenceException>(() => VerifyAddedAsync<WsFederationOptions>("test", provider));
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

            var sut = provider.GetRequiredService<PersistentDynamicManager<TSchemeDefinition>>();

            sut.Load();

            await VerifyAddedAsync<CookieAuthenticationOptions>("test", provider);
        }

        protected abstract DynamicAuthenticationBuilder AddStore(DynamicAuthenticationBuilder builder);

        private async Task<dynamic> VerifyAddedAsync<TOptions>(string schemeName, IServiceProvider provider) where TOptions : AuthenticationSchemeOptions
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
                .AddDynamic<TSchemeDefinition>();

            AddStore(builder);

            addHandlers?.Invoke(builder);

            return services.BuildServiceProvider();
        }

        class FakeGenericHandler<TFakeOptions, TOptions> : AuthenticationHandler<TOptions>
            where TOptions : AuthenticationSchemeOptions, new()
        {
            public FakeGenericHandler(IOptionsMonitor<TOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                return Task.FromResult(new FakeAuthenticateResult() as AuthenticateResult);
            }

            public class FakeAuthenticateResult: AuthenticateResult
            {
                public FakeAuthenticateResult() : base()
                {
                }
            }
        }
    }
}
