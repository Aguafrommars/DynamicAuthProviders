# Aguacongas.AspNetCore.Authentication.RavenDb

## Setup

### By registring a document store in DI


```cs
// Add authentication
var authBuilder = services
    .AddAuthentication();

// Add the magic
var dynamicBuilder = authBuilder
    .AddDynamic<SchemeDefinition>()
    .AddRavenDbStorekStore();

// Add the ravendb document store
service.AddSingleton(p => new DocumentStore()
{
    // Define the cluster node URLs (required)
    Urls = new[] { "http://your_RavenDB_cluster_node", 
                    /*some additional nodes of this cluster*/ },

    // Define a client certificate (optional)
    Certificate = new X509Certificate2("C:\\path_to_your_pfx_file\\cert.pfx"),
    Database = "SchemeDefinition"

// Initialize the Document Store
}.Initialize());
```

### By providing the get document store function


```cs
// Add authentication
var authBuilder = services
    .AddAuthentication();

// Add the magic
var dynamicBuilder = authBuilder
    .AddDynamic<SchemeDefinition>()
    .AddRavenDbStorekStore(p => new DocumentStore()
    {
        // Define the cluster node URLs (required)
        Urls = new[] { "http://your_RavenDB_cluster_node", 
                        /*some additional nodes of this cluster*/ },

        // Define a client certificate (optional)
        Certificate = new X509Certificate2("C:\\path_to_your_pfx_file\\cert.pfx"),
        Database = "SchemeDefinition"

    // Initialize the Document Store
    }.Initialize());
```