using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace WelfareLink.UserManagement.API.Configuration
{
    /// <summary>
    /// Centralized JWT configuration for global authorization.
    /// </summary>
    public static class JwtConfiguration
    {
        /// <summary>
        /// Configures JWT authentication and authorization for the API.
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

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new 
                        { 
                            error = "Token validation failed",
                            details = context.Exception.Message 
                        });
                    },
                    OnChallenge = async context =>
                    {
                        // Check if endpoint allows anonymous access
                        var endpoint = context.HttpContext.GetEndpoint();
                        var allowAnonymous = endpoint?.Metadata.GetOrderedMetadata<IAllowAnonymousMetadata>().FirstOrDefault();

                        if (allowAnonymous != null)
                        {
                            // This endpoint allows anonymous access, don't challenge
                            context.Succeed(null);
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new 
                        { 
                            error = "Unauthorized - Valid JWT token required" 
                        });
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }

        public static WebApplication UseJwtAuthenticationAndAuthorization(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }
}
