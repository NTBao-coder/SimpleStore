var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgWeb();

var catalogDb = postgres.AddDatabase("catalogdb");
var orderDb = postgres.AddDatabase("orderdb");
var identityDb = postgres.AddDatabase("identitydb");
var jwtKey = builder.AddParameter("jwt-key", secret: true);
var jwtIssuer = builder.AddParameter("jwt-issuer", value: "simple-store");
var jwtAudience = builder.AddParameter("jwt-audience", value: "simple-store");

var identity = builder.AddProject<Projects.SimpleStore_Identity_API>("identity-api")
    .WithReference(identityDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(identityDb);

var catalog = builder.AddProject<Projects.SimpleStore_Catalog_API>("catalog-api")
    .WithReference(catalogDb)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(catalogDb);

var web = builder.AddProject<Projects.SimpleStore_Web>("web")
    .WithReference(catalog)
    .WithReference(orderDb)
    .WithReference(identity)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(catalog)
    .WaitFor(orderDb)
    .WaitFor(identity);

var admin = builder.AddProject<Projects.SimpleStore_Admin>("admin")
    .WithReference(catalog)
    .WithReference(orderDb)
    .WithReference(identity)
    .WithEnvironment("Jwt__Key", jwtKey)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WaitFor(catalog)
    .WaitFor(orderDb)
    .WaitFor(identity);

builder.Build().Run();
