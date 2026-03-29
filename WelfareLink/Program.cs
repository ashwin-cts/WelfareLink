using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Interfaces;
using WelfareLink.Repositories;
using WelfareLink.Services;

namespace WelfareLink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            //Db reg
            builder.Services.AddDbContext<WelfareLinkDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add session services
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //Dependency injections
            // Core services
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Repository registrations
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
            builder.Services.AddScoped<ICitizenRepository, CitizenRepository>();
            builder.Services.AddScoped<ICitizenDocumentRepository, CitizenDocumentRepository>();
            builder.Services.AddScoped<IWelfareApplicationRepository, WelfareApplicationRepository>();
            builder.Services.AddScoped<IEligibilityCheckRepository, EligibilityCheckRepository>();
            builder.Services.AddScoped<IBenefitRepository, BenefitRepository>();
            builder.Services.AddScoped<IDisbursementRepository, DisbursementRepository>();
            builder.Services.AddScoped<IWelfareProgramRepository, WelfareProgramRepository>();
            builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
            builder.Services.AddScoped<IComplainceRecordRepository, ComplainceRecordRepository>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Service registrations
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IAuditLogService, AuditLogService>();
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



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}



/*
 using Welfare_FinalProject.Extensions;
using Welfare_FinalProject.Data;

namespace Welfare_FinalProject
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add IAM Services
            builder.Services.AddRepositories(builder.Configuration);
            builder.Services.AddIAMServices();

            var app = builder.Build();

            // Seed the database
            await DbSeeder.SeedDataAsync(app.Services);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.MapRazorPages();

            app.Run();
        }
    }
}
*/