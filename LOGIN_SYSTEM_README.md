# WelfareLink Login System

## Overview
A comprehensive login system with role-based access control has been implemented for the WelfareLink application.

## User Roles

### 1. **Citizen**
- **Access**: Personal dashboard, apply for programs, track applications
- **Navigation**: Dashboard only
- **Registration**: Public registration available through "New Registration" link on login page

### 2. **Welfare Officer**
- **Access**: Application review, benefit management, disbursement
- **Navigation**: Applications, Benefits, Disbursement tabs
- **Purpose**: Approve/reject citizen applications

### 3. **Program Manager**
- **Access**: Program creation/management, resource allocation
- **Navigation**: Program, Resource tabs
- **Purpose**: Create programs and allocate resources

### 4. **Admin**
- **Access**: User management, system-wide access
- **Navigation**: User Management, Programs, Benefits
- **Purpose**: Create officers, admins, block users, manage permissions

## How to Set Up

### 1. Apply Database Migration
Run the following command in the Package Manager Console or terminal:
```bash
dotnet ef database update --project WelfareLink
```

Or in Package Manager Console:
```powershell
Update-Database
```

### 2. Create Initial Admin Account
After running the migration, you'll need to create an initial admin account manually in the database:

```sql
INSERT INTO Users (Username, Password, Role, FullName, Email, IsActive, CreatedAt)
VALUES ('admin', 'admin123', 'Admin', 'System Administrator', 'admin@welfarelink.com', 1, GETUTCDATE());
```

### 3. Login
Navigate to the application. You'll be redirected to the login page by default.

**Default Admin Credentials** (after running SQL above):
- **Username**: admin
- **Password**: admin123
- **Role**: Select "Admin"

## Features Implemented

### 🔐 Login System
- Multi-role login page with visual role selection
- Session-based authentication
- Automatic redirection based on role

### 👥 User Management (Admin)
- Create Welfare Officers
- Create Program Managers
- Create additional Admins
- Block/Unblock users
- View all users with status

### 🎯 Role-Based Navigation
- Navigation menu changes dynamically based on logged-in user role
- Citizens only see Citizen Dashboard
- Officers see Applications, Benefits, Disbursement
- Program Managers see Programs and Resources
- Admins see User Management and overview of all sections

### 📝 Citizen Registration
- Public registration form available from login page
- Creates both User account and Citizen profile
- Automatic role assignment as "Citizen"
- Redirects to login after successful registration

### 🔒 Session Management
- 30-minute session timeout
- Logout functionality
- User information stored in session (UserId, Username, UserRole, FullName, CitizenId)

### 🚪 Access Control
- All Citizen actions now check for logged-in user session
- Redirects to login if not authenticated
- Role verification on sensitive pages

## User Flow

### For Citizens:
1. Click "New Registration" on login page
2. Fill registration form (username, password, personal info)
3. Login with credentials
4. Redirected to Citizen Dashboard
5. Can apply for programs, track applications

### For Officers/Managers:
1. Admin creates officer account
2. Officer receives username and password
3. Officer logs in selecting appropriate role
4. Redirected to their specific dashboard
5. Can perform role-specific tasks

### For Admins:
1. Login with admin credentials
2. Redirected to User Management
3. Can create officers, admins
4. Can block/unblock users
5. Can access all system areas

## Security Notes

⚠️ **Important**: This is a simple authentication system for demonstration purposes:
- Passwords are stored in plain text (NOT production-ready)
- No password hashing
- No email verification
- No password reset functionality
- Simple session-based auth (not JWT)

### For Production Deployment:
1. Implement password hashing (use `BCrypt` or `PasswordHasher`)
2. Add email verification
3. Implement password reset functionality
4. Use proper authentication middleware (ASP.NET Core Identity or JWT)
5. Add HTTPS enforcement
6. Implement CSRF protection
7. Add rate limiting for login attempts
8. Implement two-factor authentication

## Role-Based Dashboards

### Citizen Dashboard Route
`/Citizen/Dashboard`
- View profile
- View documents
- Track applications

### Welfare Officer Dashboard Route
`/Benefit/Index`
- Manage benefits
- Review applications
- Process disbursements

### Program Manager Dashboard Route
`/WelfareProgram/Dashboard`
- View program overview
- Manage programs
- Allocate resources

### Admin Dashboard Route
`/Admin/Index`
- User management
- System overview

## API Endpoints

### Account Controller
- `GET /Account/Login` - Login page
- `POST /Account/Login` - Login submission
- `GET /Account/Logout` - Logout

### Admin Controller
- `GET /Admin/Index` - User list
- `GET /Admin/CreateOfficer` - Officer creation form
- `POST /Admin/CreateOfficer` - Create officer
- `GET /Admin/CreateAdmin` - Admin creation form
- `POST /Admin/CreateAdmin` - Create admin
- `POST /Admin/BlockUser/{userId}` - Block user
- `POST /Admin/UnblockUser/{userId}` - Unblock user

### Citizen Controller
- `GET /Citizen/CreateProfile` - Public registration
- `POST /Citizen/CreateProfile` - Submit registration
- `GET /Citizen/Dashboard` - Citizen dashboard (auth required)
- All other citizen endpoints now require authentication

## Testing the System

### Create Test Users

After creating the initial admin, you can use the admin panel to create test users:

1. **Create a Welfare Officer**:
   - Login as admin
   - Go to "Create Officer"
   - Fill form with role "Welfare Officer"

2. **Create a Program Manager**:
   - Login as admin
   - Go to "Create Officer"
   - Fill form with role "Program Manager"

3. **Create a Citizen**:
   - Logout from admin
   - Click "New Registration" on login page
   - Fill citizen registration form

### Test Login Flow
1. Try logging in with different roles
2. Verify navigation changes based on role
3. Test that users can only access their authorized pages
4. Test logout functionality

## Troubleshooting

### Issue: Can't login
- Verify user exists in database
- Check role is spelled correctly ("Citizen", "WelfareOfficer", "ProgramManager", "Admin")
- Ensure `IsActive` is set to `1` (true)

### Issue: Redirected to login after login
- Check session is enabled in Program.cs
- Verify `app.UseSession()` is called before `app.UseAuthorization()`

### Issue: Navigation not showing
- Check session contains "UserRole"
- Verify Layout.cshtml has role checks
- Clear browser cache and cookies

### Issue: Database error
- Ensure migrations are applied: `dotnet ef database update`
- Check connection string in appsettings.json

## Next Steps

To enhance the system further, consider:
1. Add password hashing
2. Implement email verification
3. Add password reset via email
4. Create audit logging for user actions
5. Add profile pictures
6. Implement remember me functionality
7. Add multi-factor authentication
8. Create password strength requirements
9. Add account lockout after failed attempts
10. Implement proper authorization filters/middleware

## Support

For issues or questions:
1. Check browser console for JavaScript errors
2. Check application logs
3. Verify database schema matches migrations
4. Check session configuration in Program.cs
