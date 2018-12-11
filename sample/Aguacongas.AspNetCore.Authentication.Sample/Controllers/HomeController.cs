// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using Aguacongas.AspNetCore.Authentication.Sample.Models;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Google;
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
            return View(new AuthenticationViewModel());
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
                HandlerType = definition.HandlerType.Name
            };

            if (definition.Options is GoogleOptions googleOptions)
            {
                vm.ClientId = googleOptions.ClientId;
                vm.ClientSecret = googleOptions.ClientSecret;
            }
            else if(definition.Options is OAuthOptions oAuthOptions)
            {
                vm.ClientId = oAuthOptions.ClientId;
                vm.ClientSecret = oAuthOptions.ClientSecret;
            }
            else
            {
                return Error();
            }

            return View(definition);
        }

        [Route("List")]
        public IActionResult List()
        {
            return View(_manager.SchemeDefinitions.Select(s => s.Scheme));
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
