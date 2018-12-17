using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.Sample
{
    /// <summary>
    /// Subsribe to scheme change notifications
    /// </summary>
    public class SchemeChangeSubscriber
    {
        private readonly NoPersistentDynamicManager<SchemeDefinition> _manager;
        private readonly IDynamicProviderStore<SchemeDefinition> _store;

        public SchemeChangeSubscriber(NoPersistentDynamicManager<SchemeDefinition> manager, IDynamicProviderStore<SchemeDefinition> store)
        {
            _manager = manager;
            _store = store;
        }

    /// <summary>
    /// Called when scheme change.
    /// </summary>
    /// <param name="change">The change.</param>
    /// <returns></returns>
        public async Task OnSchemeChange(SchemeChange change)
        {
            var definition = await  _store.FindBySchemeAsync(change.Scheme);
            switch(change.Kind)
            {
                case SchemeChangeKind.Added:
                    await (definition != null ? _manager.AddAsync(definition) : _manager.RemoveAsync(change.Scheme));
                    break;
                case SchemeChangeKind.Updated:
                    await (definition != null ? _manager.UpdateAsync(definition) : _manager.RemoveAsync(change.Scheme));
                    break;
                case SchemeChangeKind.Deleted:
                    await _manager.RemoveAsync(change.Scheme);
                    break;
            }
        }
    }

    public enum SchemeChangeKind
    {
        Added,
        Updated,
        Deleted
    }

    public class SchemeChange
    {
        public string Scheme { get; set; }
        public SchemeChangeKind Kind { get; set; }
    }
}
