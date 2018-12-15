// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Sample.Helpers;
using Aguacongas.AspNetCore.Authentication.Sample.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
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

        public HomeController(PersistentDynamicManager<SchemeDefinition> manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public IActionResult Index()
        {
            return View(_manager.ManagedHandlerType.Select(t => t.Name));
        }

        [Route("Create/{type}")]
        public IActionResult Create(string type)
        {
            return View(new AuthenticationViewModel
            {
                HandlerType = type
            });
        }

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

        [Route("Update/{scheme}")]
        public async Task<IActionResult> Update(string scheme)
        {
            var definition = await _manager.FindBySchemeAsync(scheme);
            if (definition == null)
            {
                return NotFound();
            }

            var vm = new AuthenticationViewModel
            {
                Scheme = definition.Scheme,
                DisplayName = definition.DisplayName,
                HandlerType = definition.HandlerType.Name
            };

            if(definition.Options is OAuthOptions oAuthOptions) // GoogleOptions is OAuthOptions
            {
                vm.ClientId = oAuthOptions.ClientId;
                vm.ClientSecret = oAuthOptions.ClientSecret;
            }
            else
            {
                return Error();
            }

            return View(vm);
        }

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

        [Route("List")]
        public IActionResult List()
        {
            return View(_manager.SchemeDefinitions.Select(s => s.Scheme));
        }

        [Route("Delete/{scheme}")]
        public async Task<IActionResult> Delete(string scheme)
        {
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
