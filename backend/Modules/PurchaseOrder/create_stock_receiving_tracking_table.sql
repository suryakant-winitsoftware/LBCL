-- Stock Receiving Tracking Table
-- This table tracks when distributors/agents receive stock deliveries

CREATE TABLE IF NOT EXISTS public.stock_receiving_tracking (
    "UID" uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    "PurchaseOrderUID" varchar(250) NOT NULL,
    "ReceiverName" varchar(200),
    "ReceiverEmployeeCode" varchar(100),
    "ArrivalTime" timestamp,
    "UnloadingStartTime" timestamp,
    "UnloadingEndTime" timestamp,
    "PhysicalCountStartTime" timestamp,
    "PhysicalCountEndTime" timestamp,
    "ReceiverSignature" text,
    "Notes" text,
    "IsActive" boolean DEFAULT true,
    "CreatedDate" timestamp DEFAULT CURRENT_TIMESTAMP,
    "CreatedBy" varchar(250),
    "ModifiedDate" timestamp,
    "ModifiedBy" varchar(250),
    CONSTRAINT fk_stock_receiving_purchase_order
        FOREIGN KEY ("PurchaseOrderUID")
        REFERENCES public.purchase_order_header(uid)
);

-- Create index for faster lookups
CREATE INDEX IF NOT EXISTS idx_stock_receiving_po_uid
    ON public.stock_receiving_tracking("PurchaseOrderUID");

CREATE INDEX IF NOT EXISTS idx_stock_receiving_active
    ON public.stock_receiving_tracking("IsActive");

COMMENT ON TABLE public.stock_receiving_tracking IS 'Tracks stock receiving activities at distributor/agent locations';
COMMENT ON COLUMN public.stock_receiving_tracking."PurchaseOrderUID" IS 'Reference to the purchase order being received';
COMMENT ON COLUMN public.stock_receiving_tracking."ReceiverName" IS 'Name of person receiving the stock';
COMMENT ON COLUMN public.stock_receiving_tracking."ReceiverEmployeeCode" IS 'Employee code of receiver (if applicable)';
COMMENT ON COLUMN public.stock_receiving_tracking."ArrivalTime" IS 'When truck arrived at destination';
COMMENT ON COLUMN public.stock_receiving_tracking."UnloadingStartTime" IS 'When unloading started';
COMMENT ON COLUMN public.stock_receiving_tracking."UnloadingEndTime" IS 'When unloading completed';
COMMENT ON COLUMN public.stock_receiving_tracking."PhysicalCountStartTime" IS 'When physical count verification started';
COMMENT ON COLUMN public.stock_receiving_tracking."PhysicalCountEndTime" IS 'When physical count verification completed';
COMMENT ON COLUMN public.stock_receiving_tracking."ReceiverSignature" IS 'Base64 encoded signature of receiver';
