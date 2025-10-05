-- Migration: Add territory_uid column to org table (SQL Server)
-- Purpose: Store sales territory assignment for organizations (distributors)
-- Date: 2025-01-05

USE [LBSSFADev];
GO

-- Add territory_uid column if it doesn't exist
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[dbo].[org]')
    AND name = 'territory_uid'
)
BEGIN
    ALTER TABLE [dbo].[org]
    ADD [territory_uid] NVARCHAR(250) NULL;

    PRINT 'Column territory_uid added to org table successfully';
END
ELSE
BEGIN
    PRINT 'Column territory_uid already exists in org table';
END
GO

-- Add extended property for documentation
IF NOT EXISTS (
    SELECT 1
    FROM sys.extended_properties
    WHERE major_id = OBJECT_ID(N'[dbo].[org]')
    AND minor_id = (SELECT column_id FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[org]') AND name = 'territory_uid')
    AND name = 'MS_Description'
)
BEGIN
    EXEC sys.sp_addextendedproperty
        @name = N'MS_Description',
        @value = N'Foreign key reference to territory table - stores sales territory assignment',
        @level0type = N'SCHEMA', @level0name = N'dbo',
        @level1type = N'TABLE',  @level1name = N'org',
        @level2type = N'COLUMN', @level2name = N'territory_uid';
END
GO

-- Create index for better query performance
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'idx_org_territory_uid'
    AND object_id = OBJECT_ID(N'[dbo].[org]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [idx_org_territory_uid]
    ON [dbo].[org] ([territory_uid]);

    PRINT 'Index idx_org_territory_uid created successfully';
END
ELSE
BEGIN
    PRINT 'Index idx_org_territory_uid already exists';
END
GO

-- Optional: Add foreign key constraint if territory table exists
-- Uncomment the following if you want to enforce referential integrity
/*
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_org_territory'
    AND parent_object_id = OBJECT_ID(N'[dbo].[org]')
)
BEGIN
    ALTER TABLE [dbo].[org]
    ADD CONSTRAINT [FK_org_territory]
    FOREIGN KEY ([territory_uid]) REFERENCES [dbo].[territory]([uid]);

    PRINT 'Foreign key constraint FK_org_territory created successfully';
END
GO
*/

-- Verify the column was added
SELECT
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[dbo].[org]')
AND c.name = 'territory_uid';
GO
