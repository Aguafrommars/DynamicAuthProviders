// Project: DymamicAuthProviders
// Copyright (c) 2018 @Olivier Lefebvre
using System;

namespace Aguacongas.AspNetCore.Authentication.Sample.Models
{
    public class ErrorViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}