namespace Aguacongas.AspNetCore.Authentication.Redis
{
    public interface IRedisAuthenticationSchemeOptionsSerializer<TSchemeDefinition>
        where TSchemeDefinition : SchemeDefinition
    {
        TSchemeDefinition Deserialize(string value);
        string Serialize(TSchemeDefinition definition);
    }
}