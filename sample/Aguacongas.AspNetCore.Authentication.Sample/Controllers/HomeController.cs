// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Sample.Helpers;
using Aguacongas.AspNetCore.Authentication.Sample.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Sample.Controllers
{
    public class HomeController : Controller
    {
        private readonly PersistentDynamicManager<SchemeDefinition> _manager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public HomeController(PersistentDynamicManager<SchemeDefinition> manager, SignInManager<IdentityUser> signInManager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View(_manager.ManagedHandlerType.Select(t => t.Name));
        }

        // Returns an empty details view to create a scheme for a type of handler
        [Route("Create/{type}")]
        public IActionResult Create(string type)
        {
            return View(new AuthenticationViewModel
            {
                HandlerType = type
            });
        }

        // Creates a new scheme
        [HttpPost]
        [Route("Create/{type}")]
        public async Task<IActionResult> Create(AuthenticationViewModel model)
        {
            if (ModelState.IsValid)
            {
                OAuthOptions oAuthOptions;
                if (HandlerHelper.GetProviderName(model.HandlerType) == "Google")
                {
                    oAuthOptions = new GoogleOptions();
                }
                else
                {
                    oAuthOptions = new OAuthOptions();
                }

                oAuthOptions.ClientId = model.ClientId;
                oAuthOptions.ClientSecret = model.ClientSecret;
                oAuthOptions.CallbackPath = "/signin-" + model.Scheme;

                await _manager.AddAsync(new SchemeDefinition
                {
                    Scheme = model.Scheme,
                    DisplayName = model.DisplayName,
                    HandlerType = _manager.ManagedHandlerType.First(t => t.Name == model.HandlerType),
                    Options = oAuthOptions
                });
                return RedirectToAction("List");
            }

            return View(model);
        }

        // Returns a scheme details view to update id
        [Route("Update/{scheme}")]
        public async Task<IActionResult> Update(string scheme)
        {
            AuthenticationViewModel model;
            var definition = await _manager.FindBySchemeAsync(scheme);
            if (definition == null)
            {
                return NotFound();
            }
            else
            {
                model = new AuthenticationViewModel
                {
                    Scheme = definition.Scheme,
                    DisplayName = definition.DisplayName,
                    HandlerType = definition.HandlerType.Name
                };

                var oAuthOptions = definition.Options as OAuthOptions; // GoogleOptions is OAuthOptions
                model.ClientId = oAuthOptions.ClientId;
                model.ClientSecret = oAuthOptions.ClientSecret;
            }

            return View(model);
        }

        // Updates a scheme
        [HttpPost]
        [Route("Update/{scheme}")]
        public async Task<IActionResult> Update(AuthenticationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var definition = await _manager.FindBySchemeAsync(model.Scheme);
                if (definition == null)
                {
                    return NotFound();
                }

                if (definition.Options is OAuthOptions oAuthOptions) // GoogleOptions is OAuthOptions
                {
                    oAuthOptions.ClientId = model.ClientId;
                    oAuthOptions.ClientSecret = model.ClientSecret;
                }

                definition.DisplayName = model.DisplayName;

                await _manager.UpdateAsync(definition);
            }

            return View(model);
        }

        // Lists all schemes we can manage
        [Route("List")]
        public async Task<IActionResult> List()
        {
            var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

            var managedSchemes = schemes.Where(s => _manager.ManagedHandlerType.Any(h => s.HandlerType == h))
                .Select(s => s.Name);

            var definitions = managedSchemes.Select(name => _manager.FindBySchemeAsync(name).GetAwaiter().GetResult());
            return View(definitions.Select(definition => new AuthenticationViewModel
            {
                Scheme = definition.Scheme,
                DisplayName = definition.DisplayName,
                CallbackPath = definition.Options is RemoteAuthenticationOptions remote ? remote.CallbackPath : null
            }));
        }

        // Deletes a scheme
        [Route("Delete/{scheme}")]
        public async Task<IActionResult> Delete(string scheme)
        {
            var definition = await _manager.FindBySchemeAsync(scheme);
            if (definition == null)
            {
                return NotFound();
            }

            await _manager.RemoveAsync(scheme);
            return RedirectToAction("List");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
