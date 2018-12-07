using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public class DynamicProviderStore : IDynamicProviderStore
    {
        private readonly ProviderDbContext _context;

        public virtual IQueryable<ProviderDefinition> ProviderDefinitions => _context.Set<ProviderDefinition>();

        public DynamicProviderStore(ProviderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async Task Add(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();

            _context.Add(definition);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task Remove(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Remove(definition);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task Update(ProviderDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Attach(definition);
            definition.ConcurrencyStamp = Guid.NewGuid().ToString();
            _context.Update(definition);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task<ProviderDefinition> FindByScheme(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.FindAsync<ProviderDefinition>(new[] { scheme }, cancellationToken);
        }
    }
}
