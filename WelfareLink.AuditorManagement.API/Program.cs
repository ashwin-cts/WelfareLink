using Microsoft.EntityFrameworkCore;
using Serilog;
using WelfareLink.AuditorManagement.API.Configuration;
using WelfareLink.AuditorManagement.API.Data;
using WelfareLink.AuditorManagement.API.Interfaces;
using WelfareLink.AuditorManagement.API.Repositories;
using WelfareLink.AuditorManagement.API.Services;

namespace WelfareLink.AuditorManagement.API
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
                Log.Information("Starting the AuditorManagement API");
                var builder = WebApplication.CreateBuilder(args);
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
                builder.Services.AddScoped<IReportRepository, ReportRepository>();
                builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
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
                builder.Services.AddScoped<IReportService, ReportService>();
                builder.Services.AddScoped<INotificationService, NotificationService>();
                builder.Services.AddScoped<IBenefitAnalyticsService, BenefitAnalyticsService>();
                builder.Services.AddScoped<IWelfareApplicationAnalyticsService, WelfareApplicationAnalyticsService>();
                builder.Services.AddScoped<IWelfareApplicationDocumentService, WelfareApplicationDocumentService>();
                builder.Services.AddScoped<IAuditLogService, AuditLogService>();
                // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
                builder.Services.AddProblemDetails();
                builder.Services.AddOpenApi();
                builder.Services.AddSwaggerGen();

                // Add JWT Authentication & Authorization (Centralized Configuration)
                builder.Services.AddJwtAuthenticationAndAuthorization(builder.Configuration);

                // Add CORS to allow requests from WelfareLink (MVC) to WelfareLinkApi
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowWelfareLinkMvc", policy =>
                    {
                        policy.WithOrigins("https://localhost:7100", "http://localhost:5000")
                              .AllowAnyMethod()
                              .AllowAnyHeader()
                              .AllowCredentials();
                    });
                });

                var app = builder.Build();
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
                app.UseJwtAuthenticationAndAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application AuditorMangement API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
