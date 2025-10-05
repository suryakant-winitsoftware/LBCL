-- SQL Server Script to create DeliveryLoadingTracking table

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DeliveryLoadingTracking]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DeliveryLoadingTracking](
        [UID] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
        [PurchaseOrderUID] [uniqueidentifier] NOT NULL,
        [VehicleUID] [uniqueidentifier] NULL,
        [DriverEmployeeUID] [uniqueidentifier] NULL,
        [ForkLiftOperatorUID] [uniqueidentifier] NULL,
        [SecurityOfficerUID] [uniqueidentifier] NULL,
        [ArrivalTime] [datetime] NULL,
        [LoadingStartTime] [datetime] NULL,
        [LoadingEndTime] [datetime] NULL,
        [DepartureTime] [datetime] NULL,
        [LogisticsSignature] [nvarchar](MAX) NULL,
        [DriverSignature] [nvarchar](MAX) NULL,
        [Notes] [nvarchar](MAX) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
        [CreatedBy] [uniqueidentifier] NULL,
        [ModifiedDate] [datetime] NULL,
        [ModifiedBy] [uniqueidentifier] NULL,
        CONSTRAINT [PK_DeliveryLoadingTracking] PRIMARY KEY CLUSTERED ([UID] ASC),
        CONSTRAINT [FK_DeliveryLoadingTracking_PurchaseOrderHeader] FOREIGN KEY([PurchaseOrderUID])
            REFERENCES [dbo].[PurchaseOrderHeader] ([UID])
            ON DELETE CASCADE
    )
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DeliveryLoadingTracking_PurchaseOrderUID' AND object_id = OBJECT_ID('DeliveryLoadingTracking'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DeliveryLoadingTracking_PurchaseOrderUID]
    ON [dbo].[DeliveryLoadingTracking] ([PurchaseOrderUID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DeliveryLoadingTracking_VehicleUID' AND object_id = OBJECT_ID('DeliveryLoadingTracking'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DeliveryLoadingTracking_VehicleUID]
    ON [dbo].[DeliveryLoadingTracking] ([VehicleUID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DeliveryLoadingTracking_CreatedDate' AND object_id = OBJECT_ID('DeliveryLoadingTracking'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_DeliveryLoadingTracking_CreatedDate]
    ON [dbo].[DeliveryLoadingTracking] ([CreatedDate] DESC)
END
GO

EXEC sys.sp_addextendedproperty
    @name=N'MS_Description',
    @value=N'Tracks delivery loading information including vehicle, personnel, timings, and signatures for Step 1.1.7 of delivery process',
    @level0type=N'SCHEMA', @level0name=N'dbo',
    @level1type=N'TABLE', @level1name=N'DeliveryLoadingTracking'
GO
