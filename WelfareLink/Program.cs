using Microsoft.EntityFrameworkCore;
using WelfareLink.Data;
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
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // DB context kept only for AccountController and AdminController (login/auth)
            builder.Services.AddDbContext<WelfareLinkDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register typed HttpClient pointing at the API
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                             ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

            builder.Services.AddHttpClient<WelfareApiClient>(client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                // Accept self-signed certs in development
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            // Register HttpClient for dashboard controllers
            builder.Services.AddHttpClient("DashboardClient", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });


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
