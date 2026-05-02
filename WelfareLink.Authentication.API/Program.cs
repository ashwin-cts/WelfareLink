using Serilog;
using WelfareLink.Authentication.API.Configuration;
using WelfareLink.Authentication.API.Services;

using WelfareLink.Authentication.API.Middleware;

// 1. Bootstrap Logger to catch startup crashes
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/microservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting the Authentication API");
    var builder = WebApplication.CreateBuilder(args);

    // 2. Explicitly configure the Host logger to filter out noise
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        // Hide standard HttpClient logs so it doesn't spam when calling UserManagement
        .MinimumLevel.Override("System.Net.Http.HttpClient", Serilog.Events.LogEventLevel.Warning)
        .WriteTo.Console()
        .WriteTo.File("logs/microservice-.txt", rollingInterval: RollingInterval.Day)
    );

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    // 3. Register the Global Exception Handler and Problem Details
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Add JWT Authentication & Authorization (Centralized Configuration)
    builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

    // Register services
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

    // HttpClient for UserManagement API
    builder.Services.AddHttpClient("UserManagement", client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["ApiSettings:UserManagement"]
            ?? throw new InvalidOperationException("ApiSettings:UserManagement is not configured"));
        // Increase timeout and set headers for better compatibility
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            // Enable compression support
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            // Keep connections alive
            AllowAutoRedirect = true,
            MaxConnectionsPerServer = 10
        });

    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // 4. Add the Exception Handler to the very beginning of the pipeline
    app.UseExceptionHandler();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseCors();

    // Apply JWT Authentication & Authorization middleware
    app.UseJwtAuthenticationAndAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application Authentication API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}