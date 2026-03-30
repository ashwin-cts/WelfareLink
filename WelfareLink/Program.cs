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

            // Configure web root path to fix static files (HTML/CSS/JS)
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
            if (projectRoot != null)
            {
                builder.Environment.ContentRootPath = projectRoot;
                builder.Environment.WebRootPath = Path.Combine(projectRoot, "wwwroot");
            }

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllersWithViews();

            //Db reg
            builder.Services.AddDbContext<WelfareLinkDbContext>(options => 
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Dependency injections
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
            builder.Services.AddScoped<IComplianceRecordRepository, ComplianceRecordRepository>();
            builder.Services.AddScoped<IAuditRepository, AuditRepository>();
            builder.Services.AddScoped<IReportRepository, ReportRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

            // Service registrations
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
            builder.Services.AddScoped<IComplianceRecordService, ComplianceRecordService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();

            var app = builder.Build();

            // Auto-apply database migrations on startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<WelfareLinkDbContext>();
                try
                {
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
