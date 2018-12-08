using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public class DynamicProviderStore<TDefinition> : IDynamicProviderStore<TDefinition>
        where TDefinition: ProviderDefinition, new()
    {
        private readonly ProviderDbContext _context;

        public virtual IQueryable<TDefinition> ProviderDefinitions => _context.Set<TDefinition>();

        public DynamicProviderStore(ProviderDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual async Task AddAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();

            _context.Add(definition);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task RemoveAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Remove(definition);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task UpdateAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
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

        public virtual async Task<TDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }
            cancellationToken.ThrowIfCancellationRequested();
            return await _context.FindAsync<TDefinition>(new[] { scheme }, cancellationToken);
        }
    }
}
