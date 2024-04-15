using Azure.Monitor.OpenTelemetry.Exporter;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Host.UseLamar(registry => { registry.IncludeRegistry<UiServiceRegistry>(); });

var resource = ResourceBuilder.CreateDefault()
    .AddService("ChurchBulletin");

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddAzureMonitorLogExporter(config => config.ConnectionString = builder.Configuration["OpenTelemetry:ConnectionString"]);
    options.SetResourceBuilder(resource);
});

using var meterProvider = Sdk.CreateMeterProviderBuilder()
        .AddAzureMonitorMetricExporter(config => config.ConnectionString = builder.Configuration["OpenTelemetry:ConnectionString"])
        .SetResourceBuilder(resource)
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .Build();

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddAzureMonitorTraceExporter(config => config.ConnectionString = builder.Configuration["OpenTelemetry:ConnectionString"])
    .SetResourceBuilder(resource)
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .Build();

var app = builder.Build();
//Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");
app.MapHealthChecks("_healthcheck");

await app.Services.GetRequiredService<HealthCheckService>().CheckHealthAsync();

app.Run();