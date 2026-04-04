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
            builder.Services.AddControllersWithViews();

            // Add Session support
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            //Db reg
            builder.Services.AddDbContext<WelfareLinkDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
            builder.Services.AddScoped<IWelfareProgramRepository, WelfareProgramRespository>();
            builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
            builder.Services.AddScoped<IComplainceRecordRepository, ComplainceRecordRepository>();
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
            builder.Services.AddScoped<IComplainceRecordService, ComplainceRecordService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IBenefitAnalyticsService, BenefitAnalyticsService>();
            builder.Services.AddScoped<IWelfareApplicationAnalyticsService, WelfareApplicationAnalyticsService>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
