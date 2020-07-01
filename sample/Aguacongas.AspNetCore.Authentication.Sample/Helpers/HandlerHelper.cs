// Project: aguacongas/DymamicAuthProviders
// Copyright (c) 2020 @Olivier Lefebvre
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Facebook;

namespace Aguacongas.AspNetCore.Authentication.Sample.Helpers
{
    public static class HandlerHelper
    {
        public static string GetProviderName(string handlerType)
        {
            if (handlerType == typeof(FacebookHandler).Name)
            {
                return "Facebook";
            }
            if (handlerType == typeof(GoogleHandler).Name)
            {
                return "Google";
            }
            if (handlerType == typeof(OAuthHandler<OAuthOptions>).Name)
            {
                return "Github";
            }

            throw new InvalidOperationException("Unknown handler type");
        }
    }
}
