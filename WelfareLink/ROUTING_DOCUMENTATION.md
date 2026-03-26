# WelfareLink URL Routing Documentation

## Fixed Issues
1. ✅ Added `WelfareLink.Controllers` namespace to `_ViewImports.cshtml`
2. ✅ All CSHTML files now correctly reference `WelfareLink.Controllers` namespace
3. ✅ All controllers are in the `WelfareLink.Controllers` namespace

## Available Routes

### Citizen Controller Routes

#### Dashboard
- **URL**: `/Citizen/Dashboard`
- **Method**: GET
- **Description**: Displays citizen's dashboard with profile and document statistics
- **View Model**: `CitizenDashboardViewModel`
- **Redirects to CreateProfile if**: No citizen profile exists

#### Create Profile
- **URL (GET)**: `/Citizen/CreateProfile`
- **URL (POST)**: `/Citizen/CreateProfile`
- **Method**: GET, POST
- **Description**: Create a new citizen profile
- **View Model**: `CreateCitizenViewModel`
- **Required Fields**:
  - Name (max 50 chars)
  - Date of Birth
  - Address (max 300 chars)
  - Contact Information (max 50 chars)

#### Edit Profile
- **URL (GET)**: `/Citizen/EditProfile`
- **URL (POST)**: `/Citizen/EditProfile`
- **Method**: GET, POST
- **Description**: Edit existing citizen profile
- **View Model**: `EditCitizenViewModel`
- **Redirects to CreateProfile if**: No citizen profile exists

### CitizenDocument Controller Routes

#### Upload Document
- **URL (GET)**: `/CitizenDocument/UploadDocument`
- **URL (POST)**: `/CitizenDocument/UploadDocument`
- **Method**: GET, POST
- **Description**: Upload a document for verification
- **View Model**: `DocumentUploadViewModel`
- **Allowed File Types**: PDF, JPG, JPEG, PNG, DOC, DOCX
- **Max File Size**: 10 MB
- **Document Types**:
  - ID Proof (Aadhaar/PAN/Voter ID)
  - Residence Proof (Electricity Bill/Rent Agreement)
  - Income Certificate
  - Bank Statement
  - Other
- **Redirects to CreateProfile if**: No citizen profile exists

#### Document Status
- **URL**: `/CitizenDocument/DocumentStatus`
- **URL with filter**: `/CitizenDocument/DocumentStatus?status={Pending|Approved|Rejected}`
- **Method**: GET
- **Description**: View all uploaded documents and their verification status
- **View Model**: `DocumentStatusViewModel`
- **Filter Options**:
  - All (no filter)
  - Pending
  - Approved
  - Rejected
- **Redirects to CreateProfile if**: No citizen profile exists

#### Delete Document
- **URL**: `/CitizenDocument/Delete`
- **Method**: POST
- **Parameters**: `documentId` (int)
- **Description**: Delete a document (only Pending or Rejected documents can be deleted)
- **Returns**: Redirects to DocumentStatus

## Navigation Flow

```
Home (/Home/Index)
  │
  ├─→ Create Citizen Profile (/Citizen/CreateProfile)
  │     └─→ Dashboard (/Citizen/Dashboard)
  │           ├─→ Edit Profile (/Citizen/EditProfile)
  │           │     └─→ Dashboard (after save)
  │           │
  │           └─→ Upload Document (/CitizenDocument/UploadDocument)
  │                 └─→ Document Status (/CitizenDocument/DocumentStatus)
  │                       ├─→ Upload New Document (/CitizenDocument/UploadDocument)
  │                       └─→ Delete Document (POST /CitizenDocument/Delete)
  │
  └─→ Dashboard redirects to CreateProfile if profile doesn't exist
```

## Testing URLs

After running the application, you can test these URLs:

1. **Start**: `https://localhost:{port}/Citizen/Dashboard`
   - Will redirect to CreateProfile if no profile exists

2. **Create Profile**: `https://localhost:{port}/Citizen/CreateProfile`
   - Fill the form and submit to create a profile

3. **Upload Document**: `https://localhost:{port}/CitizenDocument/UploadDocument`
   - Requires a citizen profile to exist

4. **View Documents**: `https://localhost:{port}/CitizenDocument/DocumentStatus`
   - Filter by status: `https://localhost:{port}/CitizenDocument/DocumentStatus?status=Pending`

## Important Notes

- All routes use the default UserId = 1 for demo purposes
- In production, this should be replaced with actual authentication
- All POST actions use AntiForgeryToken validation
- File uploads are validated for type and size
- Documents can only be deleted if status is "Pending" or "Rejected"
