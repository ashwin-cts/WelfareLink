using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace WelfareLink.Authentication.API.Configuration
{
    /// <summary>
    /// Centralized JWT configuration for global authorization across all API projects.
    /// </summary>
    public static class JwtConfiguration
    {
        /// <summary>
        /// Configures JWT authentication and authorization for the API.
        /// This method should be called in Program.cs for each API project.
        /// </summary>
        public static IServiceCollection AddJwtAuthenticationAndAuthorization(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection("JwtSettings");
            var secret = jwtSettings["Secret"]
                ?? throw new InvalidOperationException("JwtSettings:Secret is not configured");
            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured");
            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("JwtSettings:Audience is not configured");

            var key = Encoding.ASCII.GetBytes(secret);

            // Add Authentication
            services.AddAuthentication(options =>
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
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Handle authorization failure 
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // 1. Tell the framework we are handling the response manually
                        context.HandleResponse();

                        // Check if endpoint allows anonymous access
                        var endpoint = context.HttpContext.GetEndpoint();
                        var allowAnonymous = endpoint?.Metadata.GetOrderedMetadata<IAllowAnonymous>().FirstOrDefault();

                        if (allowAnonymous != null)
                        {
                            // This endpoint allows anonymous access, skip sending challenge
                            return;
                        }

                        // 2. Set the Status Code and Headers FIRST
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        // 3. Write the body LAST
                        var result = JsonSerializer.Serialize(new { Error = "Unauthorized - Valid JWT token required to access this resource." });
                        await context.Response.WriteAsync(result);
                    },
                    OnForbidden = async context =>
                    {
                        // 1. Set the Status Code and Headers FIRST
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        // 2. Write the body LAST
                        var result = JsonSerializer.Serialize(new { Error = "Forbidden - You do not have the required permissions to perform this action." });
                        await context.Response.WriteAsync(result);
                    }
                };
            });

            
            services.AddAuthorization();

            return services;
        }

        /// <summary>
        /// Applies JWT middleware to the HTTP request pipeline.
        /// This method should be called in Program.cs after app.Build() for each API project.
        /// </summary>
        public static WebApplication UseJwtAuthenticationAndAuthorization(this WebApplication app)
        {
            //after app.build insert this method for HTTP pipeline
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}