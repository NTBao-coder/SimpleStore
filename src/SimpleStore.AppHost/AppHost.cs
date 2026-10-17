var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb();

var catalogDb = postgres.AddDatabase("catalogdb");
var orderDb = postgres.AddDatabase("orderdb");
var identityDb = postgres.AddDatabase("identitydb");

var catalog = builder.AddProject<Projects.SimpleStore_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

var web = builder.AddProject<Projects.SimpleStore_Web>("web")
    .WithReference(catalog)
    .WithReference(orderDb)
    .WithReference(identityDb)
    .WaitFor(catalog)
    .WaitFor(orderDb)
    .WaitFor(identityDb);

var admin = builder.AddProject<Projects.SimpleStore_Admin>("admin")
    .WithReference(catalog)
    .WithReference(orderDb)
    .WithReference(identityDb)
    .WaitFor(catalog)
    .WaitFor(orderDb)
    .WaitFor(identityDb);

builder.Build().Run();
