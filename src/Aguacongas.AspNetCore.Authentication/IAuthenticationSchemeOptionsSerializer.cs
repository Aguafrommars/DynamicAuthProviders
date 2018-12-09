using System;
using Microsoft.AspNetCore.Authentication;

namespace Aguacongas.AspNetCore.Authentication
{
    public interface IAuthenticationSchemeOptionsSerializer
    {
        AuthenticationSchemeOptions Deserialize(string value, Type optionsType);
        string Serialize(AuthenticationSchemeOptions options, Type optionsType);
    }
}