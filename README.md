# DynamicAuthProviders

Store and manage Microsoft.AspNetCore.Authentication providers.

[![Build status](https://ci.appveyor.com/api/projects/status/c92qtam52ughrtv1?svg=true)](https://ci.appveyor.com/project/aguacongas/dymamicauthproviders)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=aguacongas_DymamicAuthProviders&metric=alert_status)](https://sonarcloud.io/dashboard?id=aguacongas_DymamicAuthProviders)
[![CII Best Practices](https://bestpractices.coreinfrastructure.org/projects/3160/badge)](https://bestpractices.coreinfrastructure.org/projects/3160)

Nuget packages
--------------
|Dynamic Provider|EntityFramework Store|Redis Store|RavenDB Store|Tests Suite|
|:------:|:------:|:------:|:------:|:------:|
|[![][Aguacongas.AspNetCore.Authentication-badge]][Aguacongas.AspNetCore.Authentication-nuget]|[![][Aguacongas.AspNetCore.Authentication.EntityFramework-badge]][Aguacongas.AspNetCore.Authentication.EntityFramework-nuget]|[![][Aguacongas.AspNetCore.Authentication.Redis-badge]][Aguacongas.AspNetCore.Authentication.Redis-nuget]|[![][Aguacongas.AspNetCore.Authentication.RavenDB-badge]][Aguacongas.AspNetCore.Authentication.RavenDB-nuget]|[![][Aguacongas.AspNetCore.Authentication.TestBase-badge]][Aguacongas.AspNetCore.Authentication.TestBase-nuget]|
|[![][Aguacongas.AspNetCore.Authentication-downloadbadge]][Aguacongas.AspNetCore.Authentication-nuget]|[![][Aguacongas.AspNetCore.Authentication.EntityFramework-downloadbadge]][Aguacongas.AspNetCore.Authentication.EntityFramework-nuget]|[![][Aguacongas.AspNetCore.Authentication.Redis-downloadbadge]][Aguacongas.AspNetCore.Authentication.Redis-nuget]|[![][Aguacongas.AspNetCore.Authentication.RavenDB-downloadbadge]][Aguacongas.AspNetCore.Authentication.RavenDB-nuget]|[![][Aguacongas.AspNetCore.Authentication.TestBase-downloadbadge]][Aguacongas.AspNetCore.Authentication.TestBase-nuget]|  

[Aguacongas.AspNetCore.Authentication-badge]: https://img.shields.io/nuget/v/Aguacongas.AspNetCore.Authentication.svg
[Aguacongas.AspNetCore.Authentication-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.AspNetCore.Authentication.svg
[Aguacongas.AspNetCore.Authentication-nuget]: https://www.nuget.org/packages/Aguacongas.AspNetCore.Authentication/

[Aguacongas.AspNetCore.Authentication.EntityFramework-badge]: https://img.shields.io/nuget/v/Aguacongas.AspNetCore.Authentication.EntityFramework.svg
[Aguacongas.AspNetCore.Authentication.EntityFramework-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.AspNetCore.Authentication.EntityFramework.svg
[Aguacongas.AspNetCore.Authentication.EntityFramework-nuget]: https://www.nuget.org/packages/Aguacongas.AspNetCore.Authentication.EntityFramework/

[Aguacongas.AspNetCore.Authentication.Redis-badge]: https://img.shields.io/nuget/v/Aguacongas.AspNetCore.Authentication.Redis.svg
[Aguacongas.AspNetCore.Authentication.Redis-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.AspNetCore.Authentication.Redis.svg
[Aguacongas.AspNetCore.Authentication.Redis-nuget]: https://www.nuget.org/packages/Aguacongas.AspNetCore.Authentication.Redis/

[Aguacongas.AspNetCore.Authentication.RavenDB-badge]: https://img.shields.io/nuget/v/Aguacongas.AspNetCore.Authentication.RavenDB.svg
[Aguacongas.AspNetCore.Authentication.RavenDB-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.AspNetCore.Authentication.RavenDB.svg
[Aguacongas.AspNetCore.Authentication.RavenDB-nuget]: https://www.nuget.org/packages/Aguacongas.AspNetCore.Authentication.RavenDB/

[Aguacongas.AspNetCore.Authentication.TestBase-badge]: https://img.shields.io/nuget/v/Aguacongas.AspNetCore.Authentication.TestBase.svg
[Aguacongas.AspNetCore.Authentication.TestBase-downloadbadge]: https://img.shields.io/nuget/dt/Aguacongas.AspNetCore.Authentication.TestBase.svg
[Aguacongas.AspNetCore.Authentication.TestBase-nuget]: https://www.nuget.org/packages/Aguacongas.AspNetCore.Authentication.TestBase/

## Setup

In your Startup `ConfigureServices` method, add the following:

``` csharp
/** Add dynamic management **/

// Add the context to store schemes configurations.
// The context can be any kind of DbContext having a DbSet<TSchemeDefinition>
// where TSchemeDefinition is of type SchemeDefinition or derived.
services.AddDbContext<SchemeDbContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("Default"));
}); 

// Add authentication
var authBuilder = services
    .AddAuthentication();

// Add the magic
var dynamicBuilder = authBuilder
    .AddDynamic<SchemeDefinition>()
    .AddEntityFrameworkStore<SchemeDbContext>();

// Add providers managed dynamically
dynamicBuilder.AddGoogle()
    .AddOAuth("Github", "Github", options =>
    {
        // You can define default configuration for managed handlers.
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

```

And in the `Configure` method load the configuration with `LoadDynamicAuthenticationConfiguration` method:

``` csharp

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

```

Read the [wiki](../../wiki) for more information.
