// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Aguacongas.AspNetCore.Authentication.Sample.Models
{
    public class AuthenticationViewModel
    {
        [Required]
        public string Scheme { get; set; }

        [Required]
        public string DisplayName { get; set; }

        [Required]
        public string ClientId { get; set; }

        [Required]
        public string ClientSecret { get; set; }

        public string HandlerType { get; set; }
        public PathString CallbackPath { get; internal set; }
    }
}
