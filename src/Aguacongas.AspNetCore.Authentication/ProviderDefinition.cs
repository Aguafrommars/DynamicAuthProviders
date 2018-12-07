using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aguacongas.AspNetCore.Authentication
{
    public class ProviderDefinition
    {
        public string Scheme { get; set; }

        public string DisplayName { get; set; }

        public string HandlerTypeName { get; set; }

        public string SerializedOptions { get; set; }

        public string ConcurrencyStamp { get; set; }
    }
}
