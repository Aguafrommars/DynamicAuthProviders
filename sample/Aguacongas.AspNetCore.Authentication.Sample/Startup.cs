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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
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

            services
                .AddAuthentication()
                .AddFacebook()
                // The order is important, 1st add dynamic, then store, then providers you want to manage dynamically.
                // Facebook should appears in managed handlers list, however, if you move it after AddDynamic, it will.
                .AddDynamic(options =>
                {
                    options.UseInMemoryDatabase("sample");
                })
                .AddGoogle()
                .AddOAuth("Github", options =>
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
