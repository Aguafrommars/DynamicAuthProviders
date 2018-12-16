// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

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


            /** add dynamic management **/

            // add authentication
            var authBuilder = services
                .AddAuthentication()
                // You must first create an app with Facebook and add its ID and Secret to your user-secrets.
                // https://developers.facebook.com/apps/
                // https://developers.facebook.com/docs/facebook-login/manually-build-a-login-flow#login
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["facebook:appid"];
                    options.AppSecret = Configuration["facebook:appsecret"];
                }); // this handler cannot be managed dynamically

            // add the context to store schemes configuration

            services.AddDbContext<SchemeDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Default"));
            }); 

            // add magic
            var dynamicBuilder = authBuilder
                .AddDynamic<SchemeDefinition>()
                .AddEntityFrameworkStore<SchemeDbContext>();

            // add providers managed dynamically
            dynamicBuilder.AddGoogle()
                .AddOAuth("Github", "Github", options =>
                {
                    // You can defined default configuration for managed handlers.

                    options.CallbackPath = new PathString("/signin-github");
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";
                    options.ClaimsIssuer = "OAuth2-Github";


                    // Retrieving user information is unique to each provider.
                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            // Get the GitHub user
                            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                            response.EnsureSuccessStatusCode();

                            var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                            var identifier = user.Value<string>("id");
                            if (!string.IsNullOrEmpty(identifier))
                            {
                                context.Identity.AddClaim(new Claim(
                                    ClaimTypes.NameIdentifier, identifier,
                                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
                            }

                            var userName = user.Value<string>("login");
                            if (!string.IsNullOrEmpty(userName))
                            {
                                context.Identity.AddClaim(new Claim(
                                    ClaimsIdentity.DefaultNameClaimType, userName,
                                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
                            }

                            var name = user.Value<string>("name");
                            if (!string.IsNullOrEmpty(name))
                            {
                                context.Identity.AddClaim(new Claim(
                                    "urn:github:name", name,
                                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
                            }

                            var email = user.Value<string>("email");
                            if (!string.IsNullOrEmpty(email))
                            {
                                context.Identity.AddClaim(new Claim(
                                    ClaimTypes.Email, email,
                                    ClaimValueTypes.Email, context.Options.ClaimsIssuer));
                            }

                            var link = user.Value<string>("url");
                            if (!string.IsNullOrEmpty(link))
                            {
                                context.Identity.AddClaim(new Claim(
                                    "urn:github:url", link,
                                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
                            }
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
                .LoadDynamicAuthenticationConfiguration();
        }
    }
}
