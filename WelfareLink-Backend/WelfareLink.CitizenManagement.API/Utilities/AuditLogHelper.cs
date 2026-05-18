using System;

namespace WelfareLink.CitizenManagement.API.Utilities
{
    public static class AuditLogHelper
    {
        public static string GetClientIpAddress(HttpContext context)
        {
            if (context == null) return string.Empty;

            var ipAddress = context.Connection?.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
                return ipAddress;

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',');
                if (ips.Length > 0)
                    return ips[0].Trim();
            }

            return string.Empty;
        }

        public static string GetUserAgent(HttpContext context)
        {
            if (context == null) return string.Empty;

            if (context.Request.Headers.TryGetValue("User-Agent", out var userAgent))
                return userAgent.ToString();

            return string.Empty;
        }
    }
}
