using System;
using Aguacongas.AspNetCore.Authentication.Sample.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Aguacongas.AspNetCore.Authentication.Sample.Areas.Identity.IdentityHostingStartup))]
namespace Aguacongas.AspNetCore.Authentication.Sample.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<IdentityDataContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("IdentityDataContextConnection")));

                services.AddDefaultIdentity<IdentityUser>()
                    .AddEntityFrameworkStores<IdentityDataContext>();
            });
        }
    }
}