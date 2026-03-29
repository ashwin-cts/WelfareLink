namespace WelfareLink.Services.Helpers
{
    public static class ServiceHelpers
    {
        public static class UserRoles
        {
            public const string Citizen = "Citizen";
            public const string Officer = "Officer";
            public const string Manager = "Manager";
            public const string Admin = "Admin";
            public const string Compliance = "Compliance";
            public const string Auditor = "Auditor";

            public static readonly string[] AllRoles =
            {
                Citizen, Officer, Manager, Admin, Compliance, Auditor
            };

            public static bool IsValidRole(string role)
            {
                return AllRoles.Contains(role);
            }
        }

        public static class UserStatus
        {
            public const string Active = "Active";
            public const string Inactive = "Inactive";
            public const string Suspended = "Suspended";

            public static readonly string[] AllStatuses =
            {
                Active, Inactive, Suspended
            };

            public static bool IsValidStatus(string status)
            {
                return AllStatuses.Contains(status);
            }
        }

        public static class AuditActions
        {
            public const string Login = "Login";
            public const string Logout = "Logout";
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
            public const string View = "View";
            public const string Approve = "Approve";
            public const string Reject = "Reject";
            public const string Suspend = "Suspend";
            public const string Activate = "Activate";
            public const string PasswordChange = "Password Change";
            public const string PasswordReset = "Password Reset";
            public const string FailedLogin = "Failed Login";

            public static readonly string[] AllActions =
            {
                Login, Logout, Create, Update, Delete, View,
                Approve, Reject, Suspend, Activate, PasswordChange,
                PasswordReset, FailedLogin
            };
        }

        public static class NotificationCategories
        {
            public const string AccountCreated = "AccountCreated";
            public const string LoginFailure = "LoginFailure";
            public const string AccountSuspended = "AccountSuspended";
            public const string AccountDeactivated = "AccountDeactivated";
            public const string PasswordReset = "PasswordReset";
            public const string SystemAlert = "SystemAlert";
            public const string Reminder = "Reminder";

            public static readonly string[] AllCategories =
            {
                AccountCreated, LoginFailure, AccountSuspended,
                AccountDeactivated, PasswordReset, SystemAlert, Reminder
            };
        }

        public static class NotificationStatus
        {
            public const string Unread = "Unread";
            public const string Read = "Read";
            public const string Archived = "Archived";

            public static readonly string[] AllStatuses =
            {
                Unread, Read, Archived
            };
        }

        public static string GenerateUserID()
        {
            return Guid.NewGuid().ToString();
        }

        public static string GenerateTemporaryPassword(int length = 12)
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@#$%^&*";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true; // Phone is optional

            // Basic phone validation (adjust as needed)
            var cleanPhone = new string(phone.Where(char.IsDigit).ToArray());
            return cleanPhone.Length >= 10 && cleanPhone.Length <= 15;
        }
    }
}