using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
using WelfareLink.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WelfareLink
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add MVC
            builder.Services.AddControllersWithViews();

            // JSON options
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // DB context (only for auth/admin)
            builder.Services.AddDbContext<WelfareLinkDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // -----------------------------
            // HttpClients
            // -----------------------------
            //Typed HttpClient for WelfareApiClient
            builder.Services.AddHttpClient<WelfareApiClient>(client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:WApplicationSystem"]
                    ?? throw new InvalidOperationException("ApiSettings:WApplicationSystem is not configured."));
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("UserManagement", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:UserManagement"]
                    ?? throw new InvalidOperationException("ApiSettings:UserManagement is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("BenefitsAndEligibility", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:BenefitsAndEligibility"]
                    ?? throw new InvalidOperationException("ApiSettings:BenefitsAndEligibility is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("WApplicationSystem", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:WApplicationSystem"]
                    ?? throw new InvalidOperationException("ApiSettings:WApplicationSystem is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("ComplianceAndAuditLog", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:ComplianceAndAuditLog"]
                    ?? throw new InvalidOperationException("ApiSettings:ComplianceAndAuditLog is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("Operations", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:Operations"]
                    ?? throw new InvalidOperationException("ApiSettings:Operations is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            builder.Services.AddHttpClient("AnalyticsAndReporting", client =>
            {
                client.BaseAddress = new Uri(
                    builder.Configuration["ApiSettings:AnalyticsAndReporting"]
                    ?? throw new InvalidOperationException("ApiSettings:AnalyticsAndReporting is not configured."));
            }).ConfigurePrimaryHttpMessageHandler(() =>
                new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });

            var app = builder.Build();

            // Middleware
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}