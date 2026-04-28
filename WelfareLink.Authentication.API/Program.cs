using WelfareLink.Authentication.API.Configuration;
using WelfareLink.Authentication.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
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
