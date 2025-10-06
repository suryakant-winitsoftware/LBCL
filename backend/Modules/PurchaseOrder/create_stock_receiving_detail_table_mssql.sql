-- Stock Receiving Detail Table (MSSQL version)
-- This table stores the line-level physical count details during stock receiving

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockReceivingDetail')
BEGIN
    CREATE TABLE [dbo].[StockReceivingDetail] (
        [UID] uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
        [PurchaseOrderUID] nvarchar(250) NOT NULL,
        [PurchaseOrderLineUID] uniqueidentifier NOT NULL,
        [SKUCode] nvarchar(100),
        [SKUName] nvarchar(500),
        [OrderedQty] decimal(18,2) DEFAULT 0,
        [ReceivedQty] decimal(18,2) DEFAULT 0,
        [AdjustmentReason] nvarchar(500),
        [AdjustmentQty] decimal(18,2) DEFAULT 0,
        [ImageURL] nvarchar(max),
        [IsActive] bit DEFAULT 1,
        [CreatedDate] datetime DEFAULT GETDATE(),
        [CreatedBy] nvarchar(250),
        [ModifiedDate] datetime,
        [ModifiedBy] nvarchar(250),
        CONSTRAINT [FK_StockReceivingDetail_PO]
            FOREIGN KEY ([PurchaseOrderUID])
            REFERENCES [dbo].[purchase_order_header]([uid]),
        CONSTRAINT [FK_StockReceivingDetail_Line]
            FOREIGN KEY ([PurchaseOrderLineUID])
            REFERENCES [dbo].[purchase_order_line]([uid])
    );

    -- Create indexes
    CREATE INDEX [IX_StockReceivingDetail_POUID]
        ON [dbo].[StockReceivingDetail]([PurchaseOrderUID]);

    CREATE INDEX [IX_StockReceivingDetail_LineUID]
        ON [dbo].[StockReceivingDetail]([PurchaseOrderLineUID]);

    CREATE INDEX [IX_StockReceivingDetail_Active]
        ON [dbo].[StockReceivingDetail]([IsActive]);
END
