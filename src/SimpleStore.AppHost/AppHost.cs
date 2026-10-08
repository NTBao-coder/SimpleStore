var builder = DistributedApplication.CreateBuilder(args);

// No containers or service-to-service dependencies until their teaching chapter.
builder.AddProject<Projects.SimpleStore_Identity_API>("identity-api");
builder.AddProject<Projects.SimpleStore_Catalog_API>("catalog-api");
builder.AddProject<Projects.SimpleStore_Order_API>("order-api");
builder.AddProject<Projects.SimpleStore_Cart_API>("cart-api");
builder.AddProject<Projects.SimpleStore_Inventory_API>("inventory-api");
builder.AddProject<Projects.SimpleStore_Payment_API>("payment-api");
builder.AddProject<Projects.SimpleStore_Checkout_API>("checkout-api");
builder.AddProject<Projects.SimpleStore_Gateway>("gateway");
builder.AddProject<Projects.SimpleStore_Web>("web");
builder.AddProject<Projects.SimpleStore_Admin>("admin");

builder.Build().Run();
