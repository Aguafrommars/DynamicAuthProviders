using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Sample.Helpers
{
    public static class HandlerHelper
    {
        public static string GetProviderName(string handlerType)
        {
            if (handlerType == typeof(GoogleHandler).Name)
            {
                return "Google";
            }
            if (handlerType == typeof(OAuthHandler<OAuthOptions>).Name)
            {
                return "Github";
            }

            throw new InvalidOperationException("Unknow hanlder type");
        }
    }
}
