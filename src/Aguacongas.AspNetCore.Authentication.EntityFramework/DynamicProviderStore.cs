using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Aguacongas.AspNetCore.Authentication.EntityFramework
{
    public class DynamicProviderStore<TDefinition> : IDynamicProviderStore<TDefinition>
        where TDefinition: SchemeDefinition, new()
    {
        private readonly ProviderDbContext<TDefinition> _context;
        private readonly IAuthenticationSchemeOptionsSerializer _authenticationSchemeOptionsSerializer;
        private readonly ILogger<DynamicProviderStore<TDefinition>> _logger;

        public virtual IQueryable<TDefinition> ProviderDefinitions => _context.Providers
            .ToList()
            .Select(definition =>
            {
                Deserialize(definition);
                return definition;
            })
            .AsQueryable();

        public DynamicProviderStore(ProviderDbContext<TDefinition> context, IAuthenticationSchemeOptionsSerializer authenticationSchemeOptionsSerializer, ILogger<DynamicProviderStore<TDefinition>> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _authenticationSchemeOptionsSerializer = authenticationSchemeOptionsSerializer ?? throw new ArgumentNullException(nameof(authenticationSchemeOptionsSerializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual async Task AddAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();

            Serialize(definition);

            _context.Add(definition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheme {scheme} added for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
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

            _logger.LogInformation("Scheme {scheme} removed", definition.Scheme);
        }

        public virtual async Task UpdateAsync(TDefinition definition, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _context.Attach(definition);

            Serialize(definition);
            definition.ConcurrencyStamp = Guid.NewGuid().ToString();

            _context.Update(definition);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Scheme {scheme} updated for {handlerType} with options: {options}", definition.Scheme, definition.HandlerType, definition.SerializedOptions);
        }

        public virtual async Task<TDefinition> FindBySchemeAsync(string scheme, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(scheme))
            {
                throw new ArgumentException($"Parameter {nameof(scheme)} cannor be null or empty");
            }

            cancellationToken.ThrowIfCancellationRequested();
            var definition = await _context.FindAsync<TDefinition>(new[] { scheme }, cancellationToken);

            Deserialize(definition);

            return definition;
        }


        private void Serialize(TDefinition definition)
        {
            definition.HandlerTypeName = definition.HandlerType.FullName;
            definition.SerializedOptions = _authenticationSchemeOptionsSerializer.Serialize(definition.Options, definition.HandlerType.GetAuthenticationSchemeOptionsType());
        }

        private void Deserialize(TDefinition definition)
        {
            var handlerType = GetHandlerType(definition.HandlerTypeName);
            definition.HandlerType = handlerType;
            definition.Options = _authenticationSchemeOptionsSerializer.Deserialize(definition.SerializedOptions, handlerType.GetAuthenticationSchemeOptionsType());
        }

        private Type GetHandlerType(string handlerTypeName)
        {
            var platform = Environment.OSVersion.Platform.ToString();
            var runtimeAssemblyNames = DependencyContext.Default.GetRuntimeAssemblyNames(platform);

            return runtimeAssemblyNames
                    .Select(Assembly.Load)
                    .SelectMany(a => a.ExportedTypes)
                    .First(t => t.FullName == handlerTypeName);
        }
    }
}
