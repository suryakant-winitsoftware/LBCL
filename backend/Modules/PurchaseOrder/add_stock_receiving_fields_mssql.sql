-- Add missing fields to StockReceivingTracking table
-- These fields track additional employee and time data for stock receiving

-- MSSQL version
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StockReceivingTracking]') AND name = 'ForkLiftOperatorUID')
BEGIN
    ALTER TABLE [dbo].[StockReceivingTracking]
    ADD [ForkLiftOperatorUID] VARCHAR(250) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StockReceivingTracking]') AND name = 'LoadEmptyStockEmployeeUID')
BEGIN
    ALTER TABLE [dbo].[StockReceivingTracking]
    ADD [LoadEmptyStockEmployeeUID] VARCHAR(250) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StockReceivingTracking]') AND name = 'GetpassEmployeeUID')
BEGIN
    ALTER TABLE [dbo].[StockReceivingTracking]
    ADD [GetpassEmployeeUID] VARCHAR(250) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StockReceivingTracking]') AND name = 'LoadEmptyStockTime')
BEGIN
    ALTER TABLE [dbo].[StockReceivingTracking]
    ADD [LoadEmptyStockTime] DATETIME NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StockReceivingTracking]') AND name = 'GetpassTime')
BEGIN
    ALTER TABLE [dbo].[StockReceivingTracking]
    ADD [GetpassTime] DATETIME NULL;
END

-- Add extended properties for documentation
EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = 'Fork lift operator employee UID',
    @level0type = N'Schema', @level0name = 'dbo',
    @level1type = N'Table', @level1name = 'StockReceivingTracking',
    @level2type = N'Column', @level2name = 'ForkLiftOperatorUID';

EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = 'Employee who loaded empty stock UID',
    @level0type = N'Schema', @level0name = 'dbo',
    @level1type = N'Table', @level1name = 'StockReceivingTracking',
    @level2type = N'Column', @level2name = 'LoadEmptyStockEmployeeUID';

EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = 'Employee who processed getpass UID',
    @level0type = N'Schema', @level0name = 'dbo',
    @level1type = N'Table', @level1name = 'StockReceivingTracking',
    @level2type = N'Column', @level2name = 'GetpassEmployeeUID';

EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = 'Time when empty stock loading started',
    @level0type = N'Schema', @level0name = 'dbo',
    @level1type = N'Table', @level1name = 'StockReceivingTracking',
    @level2type = N'Column', @level2name = 'LoadEmptyStockTime';

EXEC sp_addextendedproperty
    @name = N'MS_Description', @value = 'Time when getpass was processed',
    @level0type = N'Schema', @level0name = 'dbo',
    @level1type = N'Table', @level1name = 'StockReceivingTracking',
    @level2type = N'Column', @level2name = 'GetpassTime';
