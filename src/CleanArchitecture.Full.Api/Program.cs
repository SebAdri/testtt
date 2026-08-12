using CleanArchitecture.Full.Api.Endpoints;
using CleanArchitecture.Full.Api.Middleware;
using CleanArchitecture.Full.Application;
using CleanArchitecture.Full.Infrastructure;
using CleanArchitecture.Full.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.Console())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, loggerConfiguration) =>
    {
        var applicationName = context.Configuration["APPLICATION_NAME"] ?? "CleanArchitecture.Full.Api";
        var seqUrl = context.Configuration["Seq:ServerUrl"] ?? "http://localhost:5341";

        loggerConfiguration
            // ReadFrom.Configuration aplica niveles, enrichers y propiedades de appsettings.json.
            .ReadFrom.Configuration(context.Configuration)
            .Enrich.WithProperty("Application", applicationName)
            // Los sinks corren en un buffer en background (Serilog.Sinks.Async):
            // el hilo que escribe el log no espera a que la escritura a consola/Seq termine.
            .WriteTo.Async(a => a.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Application} {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Async(a => a.Seq(seqUrl));
    });

    builder.Services.AddControllers();
    builder.Services.AddOpenApi();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>(
            name: "postgresql",
            failureStatus: HealthStatus.Unhealthy,
            tags: ["ready"]);

    var app = builder.Build();
    
    app.UseSerilogRequestLogging(options =>
    {
        // Las probes son frecuentes: se conservan a nivel Debug sin bombardear a Seq.
        options.GetLevel = (httpContext, _, exception) =>
            httpContext.Request.Path.StartsWithSegments("/health")
                ? LogEventLevel.Debug
                : exception is not null || httpContext.Response.StatusCode >= 500
                    ? LogEventLevel.Error
                    : LogEventLevel.Information;

        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestId", httpContext.TraceIdentifier);
            diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "unknown");
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    app.UseValidationExceptionHandling();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //db.Database.Migrate();
        //DbSeeder.Seed(db);
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference(options =>
        {
            options.Title = "CleanArchitecture.Full API";
        });
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();

    app.MapControllers();
    app.MapAccountEndpoints();
    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false
    });
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = healthCheck => healthCheck.Tags.Contains("ready")
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CleanArchitecture.Full.Api termin� de forma inesperada durante el arranque");
}
finally
{
    Log.CloseAndFlush();
}
