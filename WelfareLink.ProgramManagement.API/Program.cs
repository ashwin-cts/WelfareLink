using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using WelfareLink.ProgramManagement.API.Data;
using WelfareLink.ProgramManagement.API.Interfaces;
using WelfareLink.ProgramManagement.API.Repositories;
using WelfareLink.ProgramManagement.API.Services;
// ADDED THIS USING STATEMENT for your GlobalExceptionHandler
using WelfareLink.ProgramManagement.API.Middleware;

namespace WelfareLink.ProgramManagement.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/microservice-.txt", rollingInterval: RollingInterval.Day)
            .CreateBootstrapLogger();
            try
            {
                Log.Information("Starting the ProgramManagement API");
                var builder = WebApplication.CreateBuilder(args);

                // 1. ADDED THIS: Route all internal ASP.NET logs (including your exception handler) to Serilog
                builder.Host.UseSerilog();

                builder.Services.AddHttpContextAccessor();
                // Add services to the container.

                builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                    })
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        options.InvalidModelStateResponseFactory = ctx =>
                            new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
                            {
                                Error = string.Join("; ",
                                    ctx.ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage))
                            });
                    });

                // 2. ADDED THIS: Register your global exception handler in the DI container
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                // Explicitly configure the Host logger to filter out the noise and write to File/Console
                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .MinimumLevel.Information()
                    // Hide standard ASP.NET request logs
                    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                    // Hide standard Entity Framework SQL queries
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                    .WriteTo.Console()
                    .WriteTo.File("logs/microservice-.txt", rollingInterval: RollingInterval.Day)
                );

                //Db reg
                builder.Services.AddDbContext<WelfareLinkDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                //DI Container
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<ICitizenRepository, CitizenRepository>();
                builder.Services.AddScoped<ICitizenDocumentRepository, CitizenDocumentRepository>();
                builder.Services.AddScoped<IWelfareApplicationRepository, WelfareApplicationRepository>();
                builder.Services.AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>();
                builder.Services.AddScoped<IBenefitRepository, BenefitRepository>();
                builder.Services.AddScoped<IDisbursementRepository, DisbursementRepository>();
                builder.Services.AddScoped<IWelfareProgramRepository, WelfareProgramRespository>();
                builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
                builder.Services.AddScoped<IComplainceRecordRepository, ComplainceRecordRepository>();
                builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();

                // Service registrations
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IComplianceCheckService, ComplianceCheckService>();
                builder.Services.AddScoped<ICitizenService, CitizenService>();
                builder.Services.AddScoped<ICitizenDocumentService, CitizenDocumentService>();
                builder.Services.AddScoped<IWelfareApplicationService, WelfareApplicationService>();
                builder.Services.AddScoped<IEligibilityCheckService, EligibilityCheckService>();
                builder.Services.AddScoped<IBenefitService, BenefitService>();
                builder.Services.AddScoped<IDisbursementService, DisbursementService>();
                builder.Services.AddScoped<IWelfareProgramService, WelfareProgramService>();
                builder.Services.AddScoped<IResourceService, ResourceService>();
                builder.Services.AddScoped<IComplainceRecordService, ComplainceRecordService>();
                builder.Services.AddScoped<IBenefitAnalyticsService, BenefitAnalyticsService>();
                builder.Services.AddScoped<IWelfareApplicationAnalyticsService, WelfareApplicationAnalyticsService>();
                builder.Services.AddScoped<IWelfareApplicationDocumentService, WelfareApplicationDocumentService>();
                builder.Services.AddScoped<IAuditLogService, AuditLogService>();

                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddProblemDetails();
                builder.Services.AddOpenApi();
                builder.Services.AddSwaggerGen();

                // JWT Configuration
                var jwtSettings = builder.Configuration.GetSection("JwtSettings");
                var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
                var key = Encoding.ASCII.GetBytes(secret);

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                // Configure Authorization with global [Authorize] policy
                builder.Services.AddAuthorization(options =>
                {
                    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                });

                // Add CORS to allow requests from WelfareLink (MVC) and API clients with JWT tokens
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowWelfareLinkMvc", policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyMethod()
                              .AllowAnyHeader();
                    });
                });

                var app = builder.Build();
                app.UseExceptionHandler();

                // 3. ADDED THIS: Add the exception handler to the HTTP pipeline so it actually catches errors!
                app.UseExceptionHandler();

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseStaticFiles();

                app.UseRouting();

                // Enable CORS before authentication
                app.UseCors("AllowWelfareLinkMvc");

                // Apply JWT Authentication & Authorization middleware
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application ProgramManagement API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}