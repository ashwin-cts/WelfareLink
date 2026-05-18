IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Citizens] (
        [CitizenId] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [DateOfBirth] datetime2 NOT NULL,
        [Address] nvarchar(300) NULL,
        [ContactInfo] nvarchar(50) NULL,
        [Status] nvarchar(50) NULL,
        [Gender] nvarchar(20) NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_Citizens] PRIMARY KEY ([CitizenId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Programs] (
        [ProgramID] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [Budget] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NULL,
        [EligibleGender] nvarchar(20) NULL,
        [RequiredDocuments] nvarchar(500) NULL,
        CONSTRAINT [PK_Programs] PRIMARY KEY ([ProgramID])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [CitizenDocuments] (
        [DocumentID] int NOT NULL IDENTITY,
        [CitizenId] int NOT NULL,
        [DocType] nvarchar(30) NOT NULL,
        [DocumentName] nvarchar(100) NULL,
        [FileURI] nvarchar(500) NOT NULL,
        [UploadedDate] datetime2 NOT NULL,
        [VerificationStatus] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_CitizenDocuments] PRIMARY KEY ([DocumentID]),
        CONSTRAINT [FK_CitizenDocuments_Citizens_CitizenId] FOREIGN KEY ([CitizenId]) REFERENCES [Citizens] ([CitizenId]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Users] (
        [UserId] int NOT NULL IDENTITY,
        [Username] nvarchar(100) NOT NULL,
        [Password] nvarchar(100) NOT NULL,
        [Role] nvarchar(50) NOT NULL,
        [FullName] nvarchar(100) NULL,
        [Email] nvarchar(100) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CitizenId] int NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
        CONSTRAINT [FK_Users_Citizens_CitizenId] FOREIGN KEY ([CitizenId]) REFERENCES [Citizens] ([CitizenId])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Resources] (
        [ResourceID] int NOT NULL IDENTITY,
        [ProgramID] int NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [Quantity] decimal(18,2) NOT NULL,
        [Status] nvarchar(50) NULL,
        CONSTRAINT [PK_Resources] PRIMARY KEY ([ResourceID]),
        CONSTRAINT [FK_Resources_Programs_ProgramID] FOREIGN KEY ([ProgramID]) REFERENCES [Programs] ([ProgramID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [WelfareApplications] (
        [ApplicationID] int NOT NULL IDENTITY,
        [CitizenID] int NOT NULL,
        [ProgramID] int NOT NULL,
        [SubmittedDate] date NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_WelfareApplications] PRIMARY KEY ([ApplicationID]),
        CONSTRAINT [FK_WelfareApplications_Citizens_CitizenID] FOREIGN KEY ([CitizenID]) REFERENCES [Citizens] ([CitizenId]) ON DELETE CASCADE,
        CONSTRAINT [FK_WelfareApplications_Programs_ProgramID] FOREIGN KEY ([ProgramID]) REFERENCES [Programs] ([ProgramID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [AuditLogs] (
        [LogID] int NOT NULL IDENTITY,
        [UserId] int NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] int NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Timestamp] datetime2 NOT NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([LogID]),
        CONSTRAINT [FK_AuditLogs_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Audits] (
        [AuditID] int NOT NULL IDENTITY,
        [ProgramID] int NULL,
        [AuditedByUserId] int NOT NULL,
        [AuditDate] datetime2 NOT NULL,
        [FindingType] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [ResolvedDate] datetime2 NULL,
        CONSTRAINT [PK_Audits] PRIMARY KEY ([AuditID]),
        CONSTRAINT [FK_Audits_Programs_ProgramID] FOREIGN KEY ([ProgramID]) REFERENCES [Programs] ([ProgramID]) ON DELETE SET NULL,
        CONSTRAINT [FK_Audits_Users_AuditedByUserId] FOREIGN KEY ([AuditedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [ComplianceRecords] (
        [RecordID] int NOT NULL IDENTITY,
        [RaisedByUserId] int NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] int NOT NULL,
        [ViolationType] nvarchar(100) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ResolvedDate] datetime2 NULL,
        [ResolvedByUserId] int NULL,
        [Notes] nvarchar(max) NULL,
        CONSTRAINT [PK_ComplianceRecords] PRIMARY KEY ([RecordID]),
        CONSTRAINT [FK_ComplianceRecords_Users_RaisedByUserId] FOREIGN KEY ([RaisedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL,
        CONSTRAINT [FK_ComplianceRecords_Users_ResolvedByUserId] FOREIGN KEY ([ResolvedByUserId]) REFERENCES [Users] ([UserId]) ON DELETE SET NULL
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Benefits] (
        [BenefitID] int NOT NULL IDENTITY,
        [ApplicationID] int NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Amount] float NOT NULL,
        [Date] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Benefits] PRIMARY KEY ([BenefitID]),
        CONSTRAINT [FK_Benefits_WelfareApplications_ApplicationID] FOREIGN KEY ([ApplicationID]) REFERENCES [WelfareApplications] ([ApplicationID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [EligibilityChecks] (
        [CheckID] int NOT NULL IDENTITY,
        [ApplicationID] int NOT NULL,
        [OfficerID] int NOT NULL,
        [Result] nvarchar(max) NOT NULL,
        [ResultCode] nvarchar(max) NOT NULL,
        [Date] date NOT NULL,
        [Notes] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_EligibilityChecks] PRIMARY KEY ([CheckID]),
        CONSTRAINT [FK_EligibilityChecks_WelfareApplications_ApplicationID] FOREIGN KEY ([ApplicationID]) REFERENCES [WelfareApplications] ([ApplicationID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [WelfareApplicationDocuments] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationID] int NOT NULL,
        [DocumentID] int NOT NULL,
        CONSTRAINT [PK_WelfareApplicationDocuments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WelfareApplicationDocuments_CitizenDocuments_DocumentID] FOREIGN KEY ([DocumentID]) REFERENCES [CitizenDocuments] ([DocumentID]) ON DELETE NO ACTION,
        CONSTRAINT [FK_WelfareApplicationDocuments_WelfareApplications_ApplicationID] FOREIGN KEY ([ApplicationID]) REFERENCES [WelfareApplications] ([ApplicationID]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE TABLE [Disbursements] (
        [DisbursementID] int NOT NULL IDENTITY,
        [BenefitID] int NOT NULL,
        [CitizenID] int NOT NULL,
        [OfficerID] int NOT NULL,
        [Amount] float NOT NULL,
        [Date] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Disbursements] PRIMARY KEY ([DisbursementID]),
        CONSTRAINT [FK_Disbursements_Benefits_BenefitID] FOREIGN KEY ([BenefitID]) REFERENCES [Benefits] ([BenefitID]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Audits_AuditedByUserId] ON [Audits] ([AuditedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Audits_ProgramID] ON [Audits] ([ProgramID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Benefits_ApplicationID] ON [Benefits] ([ApplicationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_CitizenDocuments_CitizenId] ON [CitizenDocuments] ([CitizenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_ComplianceRecords_RaisedByUserId] ON [ComplianceRecords] ([RaisedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_ComplianceRecords_ResolvedByUserId] ON [ComplianceRecords] ([ResolvedByUserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Disbursements_BenefitID] ON [Disbursements] ([BenefitID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_EligibilityChecks_ApplicationID] ON [EligibilityChecks] ([ApplicationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Resources_ProgramID] ON [Resources] ([ProgramID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_Users_CitizenId] ON [Users] ([CitizenId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_WelfareApplicationDocuments_ApplicationID] ON [WelfareApplicationDocuments] ([ApplicationID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_WelfareApplicationDocuments_DocumentID] ON [WelfareApplicationDocuments] ([DocumentID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_WelfareApplications_CitizenID] ON [WelfareApplications] ([CitizenID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    CREATE INDEX [IX_WelfareApplications_ProgramID] ON [WelfareApplications] ([ProgramID]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260413170324_AddAuditComplianceTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260413170324_AddAuditComplianceTables', N'10.0.5');
END;

COMMIT;
GO

