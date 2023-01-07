using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Microsoft.AspNetCore.Builder;
public static class WebApplicationExtensions
{
    public static WebApplication UseSample(this WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            using var scope = app.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<SchemeDbContext>().Database.EnsureCreated();
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

        return app;
    }
}

