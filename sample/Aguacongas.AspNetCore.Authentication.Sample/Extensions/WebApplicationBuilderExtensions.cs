using Aguacongas.AspNetCore.Authentication.EntityFramework;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Microsoft.AspNetCore.Builder;

public static class WebApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSample(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });


        /** Add dynamic management **/

        // Add the context to store schemes configuration
        services.AddDbContext<SchemeDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("Default"), configure =>
            {
                configure.MigrationsAssembly(typeof(WebApplicationBuilderExtensions).Assembly.FullName);
            });
        });

        // Add authentication
        var authBuilder = services
            .AddAuthentication();

        // Add the magic
        var dynamicBuilder = authBuilder
            .AddDynamic<SchemeDefinition>()
            .AddEntityFrameworkStore<SchemeDbContext>();

        // Add providers handlers managed dynamically
        dynamicBuilder
            .AddFacebook()
            .AddGoogle()
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

                        using var doc = JsonDocument.Parse(content);

                        context.RunClaimActions(doc.RootElement);
                    }
                };
            });

        services.AddMvc(options => options.EnableEndpointRouting = false);

        return builder;
    }
}
