using Inventory.Grpc.Extensions;
using Inventory.Grpc.Services;
using Serilog;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddAppConfigurations();

Log.Information($"Start {builder.Environment.ApplicationName} up");

try
{
    builder.Host.AddAppConfigurations();
    builder.Services.AddConfigurationSettings(builder.Configuration);
    builder.Services.ConfigureMongoDbClient();
    builder.Services.AddInfrastructureServices();
    builder.Services.AddGrpc();

    builder.WebHost.ConfigureKestrel(options =>
    {
        if (builder.Environment.IsDevelopment())
        {
            options.ListenAnyIP(5007);
            options.ListenAnyIP(5107, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        }
        if (builder.Environment.IsProduction())
        {
            options.ListenAnyIP(8080);
            options.ListenAnyIP(8585, listenOptions =>
            {
                listenOptions.Protocols = HttpProtocols.Http2;
            });
        }
    });

    var app = builder.Build();
    app.UseRouting();
    // app.UseHttpsRedirection();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapGrpcService<InventoryService>();
        endpoints.MapGet("/", async context =>
        {
            await context.Response.WriteAsync("Inventory.GRPC - Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
        });
    });

    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, $"Unhandled exception: {ex.Message}");
}
finally
{
    Log.Information($"Shut down {builder.Environment.ApplicationName} complete");
    Log.CloseAndFlush();
}