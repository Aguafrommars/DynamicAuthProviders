// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Aguacongas.AspNetCore.Authentication.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            /** Add dynamic management **/

            // Add authentication
            var authBuilder = services
                .AddAuthentication()
                // You must first create an app with Facebook and add its ID and Secret to your user-secrets.
                // https://developers.facebook.com/apps/
                // https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow#login
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["facebook:appid"] ?? "not set";
                    options.AppSecret = Configuration["facebook:appsecret"] ?? "not set";
                }); // this handler cannot be managed dynamically

            // Add the context to store schemes configuration
            services.AddDbContext<SchemeDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Default"), configure =>
                {
                    configure.MigrationsAssembly(GetType().Assembly.FullName);
                });
            }); 

            // Add the magic
            var dynamicBuilder = authBuilder
                .AddDynamic<SchemeDefinition>()
                .AddEntityFrameworkStore<SchemeDbContext>();

            // Add providers managed dynamically
            dynamicBuilder.AddGoogle()
                .AddOAuth("Github", "Github", options =>
                {
                    // You can defined default configuration for managed handlers.
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";
                    options.ClaimsIssuer = "OAuth2-Github";
                    // Retrieving user information is unique to each provider.
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
                    options.ClaimActions.MapJsonKey("urn:github:name", "name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
                    options.ClaimActions.MapJsonKey("urn:github:url", "url");
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            // Get the GitHub user
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            // A user-agent header is required by GitHub. See (https://developer.github.com/v3/#user-agent-required)
                            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("DynamicAuthProviders-sample", "1.0.0"));

                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            var content = await response.Content.ReadAsStringAsync();
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(content);

                            context.RunClaimActions(user);
                        }
                    };
                }); 

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection()
                .UseStaticFiles()
                .UseCookiePolicy()
                .UseAuthentication()
                .UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                })
                // load dynamyc authentication configuration from store
                .LoadDynamicAuthenticationConfiguration<SchemeDefinition>();
        }
    }
}
