-- Territory Master Table Schema
-- Created for LBCL Territory Management
-- Maps territories to organizations, employees (managers), and clusters

USE [LBSSFADev];
GO

-- Table: territory
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[territory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[territory] (
        [id] INT IDENTITY(1,1) NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,

        -- Core Territory Fields
        [org_uid] NVARCHAR(250) NOT NULL,               -- FK to org table (BusinessUnit like 'LBCL')
        [territory_code] NVARCHAR(50) NOT NULL,         -- Territory code (BORA, NEGO, CM15, etc.)
        [territory_name] NVARCHAR(250) NOT NULL,        -- Territory name (Boralesgamuwa, Negombo, etc.)
        [manager_emp_uid] NVARCHAR(250) NULL,           -- FK to emp table (Territory Manager)
        [cluster_code] NVARCHAR(50) NULL,               -- Cluster code for grouping territories

        -- Hierarchy Fields (like Location)
        [parent_uid] NVARCHAR(250) NULL,                -- Parent territory UID for hierarchy
        [item_level] INT NULL DEFAULT 0,                -- Hierarchy level (0=root, 1=child, etc.)
        [has_child] BIT NULL DEFAULT 0,                 -- Whether this territory has child territories

        -- Territory Type Flags
        [is_import] BIT NOT NULL DEFAULT 0,             -- Import territory flag (0 or 1)
        [is_local] BIT NOT NULL DEFAULT 0,              -- Local territory flag (0 or 1)
        [is_non_license] INT NOT NULL DEFAULT 0,        -- NonLicense type (0, 1, or 2)

        -- Status Fields
        [status] NVARCHAR(50) NULL,                     -- Status (Active/Inactive)
        [is_active] BIT NULL DEFAULT 1,                 -- Active flag

        CONSTRAINT [PK_territory] PRIMARY KEY CLUSTERED ([uid]),
        CONSTRAINT [UK_territory_org_code] UNIQUE ([org_uid], [territory_code])
    );

    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX [IX_territory_org_uid] ON [dbo].[territory] ([org_uid]);
    CREATE NONCLUSTERED INDEX [IX_territory_code] ON [dbo].[territory] ([territory_code]);
    CREATE NONCLUSTERED INDEX [IX_territory_manager] ON [dbo].[territory] ([manager_emp_uid]);
    CREATE NONCLUSTERED INDEX [IX_territory_cluster] ON [dbo].[territory] ([cluster_code]);
    CREATE NONCLUSTERED INDEX [IX_territory_parent] ON [dbo].[territory] ([parent_uid]);
END
GO

-- Insert sample data for LBCL territories
IF NOT EXISTS (SELECT 1 FROM territory WHERE org_uid = 'LBCL_ORG_UID')
BEGIN
    -- Note: Replace 'LBCL_ORG_UID' with actual org.uid for LBCL
    -- Sample insert statements based on the provided data

    /*
    INSERT INTO territory (uid, created_by, created_time, modified_by, modified_time, server_add_time, server_modified_time,
                          org_uid, territory_code, territory_name, manager_emp_uid, cluster_code, is_import, is_local, is_non_license, status, is_active)
    VALUES
    (NEWID(), 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), GETDATE(), GETDATE(),
     'LBCL_ORG_UID', 'BORA', 'Boralesgamuwa', NULL, NULL, 0, 1, 1, 'Active', 1),
    (NEWID(), 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), GETDATE(), GETDATE(),
     'LBCL_ORG_UID', 'NEGO', 'Negombo', NULL, NULL, 1, 1, 0, 'Active', 1),
    (NEWID(), 'SYSTEM', GETDATE(), 'SYSTEM', GETDATE(), GETDATE(), GETDATE(),
     'LBCL_ORG_UID', 'CM15', 'Colombo-15', NULL, NULL, 1, 1, 0, 'Active', 1);
    -- Add more territories as needed
    */
END
GO

PRINT 'Territory table created successfully';
