
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 07/31/2017 14:24:19
-- Generated from EDMX file: C:\Users\BugSkan\Source\Workspaces\Workspace\BugShield\slnBugShield\BUGSKAN.Portal\Models\BugSkan.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BugSkanDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Preferences_Site]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Preferences] DROP CONSTRAINT [FK_Preferences_Site];
GO
IF OBJECT_ID(N'[dbo].[FK_ShieldLog_ToTable]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[ShieldLog] DROP CONSTRAINT [FK_ShieldLog_ToTable];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[AccessLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[AccessLog];
GO
IF OBJECT_ID(N'[dbo].[BugDB]', 'U') IS NOT NULL
    DROP TABLE [dbo].[BugDB];
GO
IF OBJECT_ID(N'[dbo].[Customer]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Customer];
GO
IF OBJECT_ID(N'[dbo].[Preferences]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Preferences];
GO
IF OBJECT_ID(N'[dbo].[ShieldLog]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ShieldLog];
GO
IF OBJECT_ID(N'[dbo].[Site]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Site];
GO
IF OBJECT_ID(N'[dbo].[SkanResults]', 'U') IS NOT NULL
    DROP TABLE [dbo].[SkanResults];
GO
IF OBJECT_ID(N'[dbo].[Subscriptions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Subscriptions];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[__MigrationHistory]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[__MigrationHistory];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[App]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[App];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[AspNetRoles]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[AspNetRoles];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[AspNetUserClaims]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[AspNetUserClaims];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[AspNetUserLogins]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[AspNetUserLogins];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[AspNetUserRoles]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[AspNetUserRoles];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[AspNetUsers]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[AspNetUsers];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[Bugs]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[Bugs];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[DomainRequest]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[DomainRequest];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[EMailRequest]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[EMailRequest];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[MailJobs]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[MailJobs];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[ResultItems]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[ResultItems];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[SkanJobs]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[SkanJobs];
GO
IF OBJECT_ID(N'[BugSkanDBModelStoreContainer].[sysdiagrams]', 'U') IS NOT NULL
    DROP TABLE [BugSkanDBModelStoreContainer].[sysdiagrams];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'AccessLogs'
CREATE TABLE [dbo].[AccessLogs] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [LogDetails] nvarchar(max)  NULL,
    [Url] nvarchar(50)  NULL,
    [TimeStamp] datetime  NULL
);
GO

-- Creating table 'BugDBs'
CREATE TABLE [dbo].[BugDBs] (
    [Id] int  NOT NULL,
    [Title] varchar(max)  NULL,
    [Description] varchar(max)  NULL,
    [Links] varchar(max)  NULL,
    [VideoUrl] varchar(max)  NULL,
    [TimeStamp] datetime  NULL,
    [ShieldLog_id] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'Customers'
CREATE TABLE [dbo].[Customers] (
    [Id] uniqueidentifier  NOT NULL,
    [CompanyName] varchar(max)  NULL,
    [Address1] varchar(max)  NULL,
    [Address2] varchar(max)  NULL,
    [Address3] varchar(max)  NULL,
    [City] varchar(max)  NULL,
    [State] varchar(max)  NULL,
    [Country] varchar(max)  NULL,
    [ZipCode] varchar(20)  NULL,
    [Website] varchar(max)  NULL,
    [Phone] varchar(max)  NULL,
    [Email] varchar(max)  NULL,
    [Notes] varchar(max)  NULL,
    [Created] datetime  NULL,
    [UserId] nvarchar(128)  NULL
);
GO

-- Creating table 'Preferences'
CREATE TABLE [dbo].[Preferences] (
    [Id] int  NOT NULL,
    [DefaultSite] uniqueidentifier  NULL,
    [NotificationEmail] varchar(max)  NULL,
    [UserId] nvarchar(128)  NULL
);
GO

-- Creating table 'Sites'
CREATE TABLE [dbo].[Sites] (
    [SiteId] uniqueidentifier  NOT NULL,
    [Name] varchar(max)  NULL,
    [Url] varchar(max)  NOT NULL,
    [AddedDate] datetime  NULL,
    [LastScan] datetime  NULL,
    [Status] varchar(max)  NULL,
    [Notes] varchar(max)  NULL,
    [LoginUser] varchar(max)  NULL,
    [LoginPass] varchar(max)  NULL,
    [Technology] varchar(max)  NULL,
    [UserId] nvarchar(max)  NULL,
    [LoginUrl] varchar(max)  NULL,
    [LoginSuccess] nchar(10)  NULL,
    [IPAddress] varchar(max)  NULL,
    [Server] varchar(max)  NULL,
    [PlanName] varchar(max)  NULL
);
GO

-- Creating table 'SkanResults'
CREATE TABLE [dbo].[SkanResults] (
    [resultId] int IDENTITY(1,1) NOT NULL,
    [href] varchar(max)  NULL,
    [id] varchar(50)  NULL,
    [name] varchar(max)  NULL,
    [url] varchar(max)  NULL,
    [description] varchar(max)  NULL,
    [skanid] int  NULL
);
GO

-- Creating table 'Subscriptions'
CREATE TABLE [dbo].[Subscriptions] (
    [Id] nvarchar(128)  NOT NULL,
    [PlanName] varchar(50)  NULL,
    [StartDate] datetime  NULL,
    [EndDate] datetime  NULL,
    [PaymentMade] int  NULL,
    [PaymentPending] int  NULL,
    [PaymentDate] datetime  NULL,
    [PaymentDueDate] datetime  NULL,
    [UserId] nvarchar(max)  NULL
);
GO

-- Creating table 'C__MigrationHistory'
CREATE TABLE [dbo].[C__MigrationHistory] (
    [MigrationId] nvarchar(150)  NOT NULL,
    [ContextKey] nvarchar(300)  NOT NULL,
    [Model] varbinary(max)  NOT NULL,
    [ProductVersion] nvarchar(32)  NOT NULL
);
GO

-- Creating table 'Apps'
CREATE TABLE [dbo].[Apps] (
    [Id] uniqueidentifier  NOT NULL,
    [AppName] varchar(max)  NULL,
    [AppTitle] varchar(max)  NULL,
    [AppType] varchar(50)  NULL,
    [Url] varchar(max)  NULL,
    [SubmittedDate] datetime  NULL,
    [LastScan] datetime  NULL,
    [UserId] nvarchar(128)  NULL,
    [ReportUrl] varchar(max)  NULL,
    [Critical] int  NULL,
    [Warning] int  NULL,
    [Info] int  NULL
);
GO

-- Creating table 'AspNetRoles'
CREATE TABLE [dbo].[AspNetRoles] (
    [Id] nvarchar(128)  NOT NULL,
    [Name] nvarchar(256)  NOT NULL
);
GO

-- Creating table 'AspNetUserClaims'
CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id] int  NOT NULL,
    [UserId] nvarchar(128)  NOT NULL,
    [ClaimType] nvarchar(max)  NULL,
    [ClaimValue] nvarchar(max)  NULL
);
GO

-- Creating table 'AspNetUserLogins'
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider] nvarchar(128)  NOT NULL,
    [ProviderKey] nvarchar(128)  NOT NULL,
    [UserId] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'AspNetUserRoles'
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] nvarchar(128)  NOT NULL,
    [RoleId] nvarchar(128)  NOT NULL
);
GO

-- Creating table 'AspNetUsers'
CREATE TABLE [dbo].[AspNetUsers] (
    [Id] nvarchar(128)  NOT NULL,
    [Email] nvarchar(256)  NULL,
    [EmailConfirmed] bit  NOT NULL,
    [PasswordHash] nvarchar(max)  NULL,
    [SecurityStamp] nvarchar(max)  NULL,
    [PhoneNumber] nvarchar(max)  NULL,
    [PhoneNumberConfirmed] bit  NOT NULL,
    [TwoFactorEnabled] bit  NOT NULL,
    [LockoutEndDateUtc] datetime  NULL,
    [LockoutEnabled] bit  NOT NULL,
    [AccessFailedCount] int  NOT NULL,
    [UserName] nvarchar(256)  NOT NULL,
    [TempConfCode] nvarchar(50)  NULL
);
GO

-- Creating table 'Bugs'
CREATE TABLE [dbo].[Bugs] (
    [Id] int  NOT NULL,
    [Title] varchar(max)  NULL,
    [Description] varchar(max)  NULL,
    [Links] varchar(max)  NULL,
    [VideoUrl] varchar(max)  NULL,
    [TimeStamp] datetime  NULL
);
GO

-- Creating table 'DomainRequests'
CREATE TABLE [dbo].[DomainRequests] (
    [Id] int  NOT NULL,
    [domain] varchar(max)  NULL,
    [requestip] varchar(50)  NULL,
    [status] varchar(50)  NULL,
    [verified] bit  NULL
);
GO

-- Creating table 'EMailRequests'
CREATE TABLE [dbo].[EMailRequests] (
    [Id] int  NOT NULL,
    [EmailId] varchar(max)  NULL,
    [requestip] varchar(50)  NULL,
    [status] varchar(50)  NULL,
    [verified] bit  NULL
);
GO

-- Creating table 'MailJobs'
CREATE TABLE [dbo].[MailJobs] (
    [Id] int  NOT NULL,
    [SubcriberId] varchar(max)  NULL,
    [JobId] varchar(50)  NULL
);
GO

-- Creating table 'ResultItems'
CREATE TABLE [dbo].[ResultItems] (
    [ResultId] int  NOT NULL,
    [AppId] uniqueidentifier  NULL,
    [ResultTitle] varchar(max)  NULL,
    [ResultCount] int  NULL,
    [ResultSummary] varchar(max)  NULL,
    [VectorDetails] varchar(max)  NULL,
    [CveNumber] varchar(max)  NULL,
    [ThreatLevel] varchar(50)  NULL,
    [Tags] varchar(max)  NULL
);
GO

-- Creating table 'ShieldLogs'
CREATE TABLE [dbo].[ShieldLogs] (
    [id] nvarchar(50)  NOT NULL,
    [Date] datetime  NULL,
    [Time] datetime  NULL,
    [SourceIp] varchar(50)  NULL,
    [SourcePort] varchar(50)  NULL,
    [DestIp] varchar(50)  NULL,
    [DestPort] varchar(50)  NULL,
    [Method] varchar(50)  NULL,
    [Url] varchar(max)  NULL,
    [Host] varchar(50)  NULL,
    [UserAgent] varchar(max)  NULL,
    [PostData] varchar(max)  NULL,
    [ResponseCode] varchar(50)  NULL,
    [AttackId] int  NULL,
    [IpInfo] varchar(max)  NULL
);
GO

-- Creating table 'SkanJobs'
CREATE TABLE [dbo].[SkanJobs] (
    [Id] varchar(50)  NOT NULL,
    [SkanId] varchar(50)  NULL,
    [SubscriberId] varchar(max)  NULL,
    [Url] varchar(max)  NULL,
    [PutTime] datetime  NULL,
    [CurrentStatus] varchar(50)  NULL,
    [StartTime] datetime  NULL,
    [EndTime] datetime  NULL,
    [RefId] int  NULL,
    [Notes] varchar(max)  NULL
);
GO

-- Creating table 'sysdiagrams'
CREATE TABLE [dbo].[sysdiagrams] (
    [name] nvarchar(128)  NOT NULL,
    [principal_id] int  NOT NULL,
    [diagram_id] int  NOT NULL,
    [version] int  NULL,
    [definition] varbinary(max)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'AccessLogs'
ALTER TABLE [dbo].[AccessLogs]
ADD CONSTRAINT [PK_AccessLogs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'BugDBs'
ALTER TABLE [dbo].[BugDBs]
ADD CONSTRAINT [PK_BugDBs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Customers'
ALTER TABLE [dbo].[Customers]
ADD CONSTRAINT [PK_Customers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Preferences'
ALTER TABLE [dbo].[Preferences]
ADD CONSTRAINT [PK_Preferences]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [SiteId] in table 'Sites'
ALTER TABLE [dbo].[Sites]
ADD CONSTRAINT [PK_Sites]
    PRIMARY KEY CLUSTERED ([SiteId] ASC);
GO

-- Creating primary key on [resultId] in table 'SkanResults'
ALTER TABLE [dbo].[SkanResults]
ADD CONSTRAINT [PK_SkanResults]
    PRIMARY KEY CLUSTERED ([resultId] ASC);
GO

-- Creating primary key on [Id] in table 'Subscriptions'
ALTER TABLE [dbo].[Subscriptions]
ADD CONSTRAINT [PK_Subscriptions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [MigrationId], [ContextKey], [Model], [ProductVersion] in table 'C__MigrationHistory'
ALTER TABLE [dbo].[C__MigrationHistory]
ADD CONSTRAINT [PK_C__MigrationHistory]
    PRIMARY KEY CLUSTERED ([MigrationId], [ContextKey], [Model], [ProductVersion] ASC);
GO

-- Creating primary key on [Id] in table 'Apps'
ALTER TABLE [dbo].[Apps]
ADD CONSTRAINT [PK_Apps]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id], [Name] in table 'AspNetRoles'
ALTER TABLE [dbo].[AspNetRoles]
ADD CONSTRAINT [PK_AspNetRoles]
    PRIMARY KEY CLUSTERED ([Id], [Name] ASC);
GO

-- Creating primary key on [Id], [UserId] in table 'AspNetUserClaims'
ALTER TABLE [dbo].[AspNetUserClaims]
ADD CONSTRAINT [PK_AspNetUserClaims]
    PRIMARY KEY CLUSTERED ([Id], [UserId] ASC);
GO

-- Creating primary key on [LoginProvider], [ProviderKey], [UserId] in table 'AspNetUserLogins'
ALTER TABLE [dbo].[AspNetUserLogins]
ADD CONSTRAINT [PK_AspNetUserLogins]
    PRIMARY KEY CLUSTERED ([LoginProvider], [ProviderKey], [UserId] ASC);
GO

-- Creating primary key on [UserId], [RoleId] in table 'AspNetUserRoles'
ALTER TABLE [dbo].[AspNetUserRoles]
ADD CONSTRAINT [PK_AspNetUserRoles]
    PRIMARY KEY CLUSTERED ([UserId], [RoleId] ASC);
GO

-- Creating primary key on [Id], [EmailConfirmed], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], [UserName] in table 'AspNetUsers'
ALTER TABLE [dbo].[AspNetUsers]
ADD CONSTRAINT [PK_AspNetUsers]
    PRIMARY KEY CLUSTERED ([Id], [EmailConfirmed], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnabled], [AccessFailedCount], [UserName] ASC);
GO

-- Creating primary key on [Id] in table 'Bugs'
ALTER TABLE [dbo].[Bugs]
ADD CONSTRAINT [PK_Bugs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'DomainRequests'
ALTER TABLE [dbo].[DomainRequests]
ADD CONSTRAINT [PK_DomainRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'EMailRequests'
ALTER TABLE [dbo].[EMailRequests]
ADD CONSTRAINT [PK_EMailRequests]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'MailJobs'
ALTER TABLE [dbo].[MailJobs]
ADD CONSTRAINT [PK_MailJobs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [ResultId] in table 'ResultItems'
ALTER TABLE [dbo].[ResultItems]
ADD CONSTRAINT [PK_ResultItems]
    PRIMARY KEY CLUSTERED ([ResultId] ASC);
GO

-- Creating primary key on [id] in table 'ShieldLogs'
ALTER TABLE [dbo].[ShieldLogs]
ADD CONSTRAINT [PK_ShieldLogs]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'SkanJobs'
ALTER TABLE [dbo].[SkanJobs]
ADD CONSTRAINT [PK_SkanJobs]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [name], [principal_id], [diagram_id] in table 'sysdiagrams'
ALTER TABLE [dbo].[sysdiagrams]
ADD CONSTRAINT [PK_sysdiagrams]
    PRIMARY KEY CLUSTERED ([name], [principal_id], [diagram_id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [DefaultSite] in table 'Preferences'
ALTER TABLE [dbo].[Preferences]
ADD CONSTRAINT [FK_Preferences_Site]
    FOREIGN KEY ([DefaultSite])
    REFERENCES [dbo].[Sites]
        ([SiteId])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Preferences_Site'
CREATE INDEX [IX_FK_Preferences_Site]
ON [dbo].[Preferences]
    ([DefaultSite]);
GO

-- Creating foreign key on [AttackId] in table 'ShieldLogs'
ALTER TABLE [dbo].[ShieldLogs]
ADD CONSTRAINT [FK_ShieldLog_ToTable]
    FOREIGN KEY ([AttackId])
    REFERENCES [dbo].[BugDBs]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_ShieldLog_ToTable'
CREATE INDEX [IX_FK_ShieldLog_ToTable]
ON [dbo].[ShieldLogs]
    ([AttackId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------