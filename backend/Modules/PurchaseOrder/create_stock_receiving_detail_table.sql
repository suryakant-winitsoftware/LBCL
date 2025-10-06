-- Stock Receiving Detail Table
-- This table stores the line-level physical count details during stock receiving

CREATE TABLE IF NOT EXISTS public."StockReceivingDetail" (
    "UID" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PurchaseOrderUID" varchar(250) NOT NULL,
    "PurchaseOrderLineUID" varchar(250) NOT NULL,
    "SKUCode" varchar(100),
    "SKUName" varchar(500),
    "OrderedQty" decimal(18,2) DEFAULT 0,
    "ReceivedQty" decimal(18,2) DEFAULT 0,
    "AdjustmentReason" varchar(500),
    "AdjustmentQty" decimal(18,2) DEFAULT 0,
    "ImageURL" text,
    "IsActive" boolean DEFAULT true,
    "CreatedDate" timestamp DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" varchar(250),
    "ModifiedDate" timestamp,
    "ModifiedBy" varchar(250),
    CONSTRAINT fk_stock_receiving_detail_po
        FOREIGN KEY ("PurchaseOrderUID")
        REFERENCES public.purchase_order_header(uid),
    CONSTRAINT fk_stock_receiving_detail_line
        FOREIGN KEY ("PurchaseOrderLineUID")
        REFERENCES public.purchase_order_line(uid)
);

-- Create indexes for faster lookups
CREATE INDEX IF NOT EXISTS idx_stock_receiving_detail_po_uid
    ON public."StockReceivingDetail"("PurchaseOrderUID");

CREATE INDEX IF NOT EXISTS idx_stock_receiving_detail_line_uid
    ON public."StockReceivingDetail"("PurchaseOrderLineUID");

CREATE INDEX IF NOT EXISTS idx_stock_receiving_detail_active
    ON public."StockReceivingDetail"("IsActive");

COMMENT ON TABLE public."StockReceivingDetail" IS 'Stores line-level physical count details during stock receiving';
COMMENT ON COLUMN public."StockReceivingDetail"."PurchaseOrderUID" IS 'Reference to the purchase order';
COMMENT ON COLUMN public."StockReceivingDetail"."PurchaseOrderLineUID" IS 'Reference to the specific purchase order line item';
COMMENT ON COLUMN public."StockReceivingDetail"."SKUCode" IS 'SKU code for reference';
COMMENT ON COLUMN public."StockReceivingDetail"."SKUName" IS 'SKU name for reference';
COMMENT ON COLUMN public."StockReceivingDetail"."OrderedQty" IS 'Quantity ordered';
COMMENT ON COLUMN public."StockReceivingDetail"."ReceivedQty" IS 'Quantity physically received and counted';
COMMENT ON COLUMN public."StockReceivingDetail"."AdjustmentReason" IS 'Reason for any adjustment (shortage/damage/etc)';
COMMENT ON COLUMN public."StockReceivingDetail"."AdjustmentQty" IS 'Adjustment quantity (positive or negative)';
COMMENT ON COLUMN public."StockReceivingDetail"."ImageURL" IS 'URL or path to uploaded image for this line item';
