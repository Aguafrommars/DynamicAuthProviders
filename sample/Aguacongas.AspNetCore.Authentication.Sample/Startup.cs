// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                .AddOpenIdConnect()
                // The order is important, 1st add dynamic, then store, then providers you want to manage dynamically.
                // OpenIdConnect should appears in managed handlers list, however, if you move it after AddDynamic, it will.
                .AddDynamic(options =>
                {
                    options.UseInMemoryDatabase("sample");
                })
                .AddGoogle(options =>
                {
                    // You can defined default configuration for managed handlers.
                    options.Events.OnTicketReceived = context =>
                    {
                        var provider = context.HttpContext.RequestServices;
                        var logger = provider.GetRequiredService<ILogger<TicketReceivedContext>>();
                        logger.LogInformation($"Ticket received for scheme {context.Scheme}");
                        return Task.CompletedTask;
                    };
                })
                .AddFacebook()
                .AddTwitter()
                .AddMicrosoftAccount();

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
