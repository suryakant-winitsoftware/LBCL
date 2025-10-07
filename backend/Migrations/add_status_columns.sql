-- Migration: Add Status columns to DeliveryLoadingTracking and StockReceivingTracking tables
-- Date: 2025-01-06
-- Description: Add workflow status tracking to separate concerns from purchase_order_header

-- Add Status column to DeliveryLoadingTracking table
ALTER TABLE public."DeliveryLoadingTracking"
ADD COLUMN IF NOT EXISTS "Status" VARCHAR(50) DEFAULT 'PENDING';

-- Add comment to describe the column
COMMENT ON COLUMN public."DeliveryLoadingTracking"."Status" IS 'Delivery loading workflow status: PENDING, APPROVED_FOR_SHIPMENT, SHIPPED';

-- Update existing records to have default status
UPDATE public."DeliveryLoadingTracking"
SET "Status" = 'PENDING'
WHERE "Status" IS NULL;

-- Add Status column to StockReceivingTracking table
ALTER TABLE public."StockReceivingTracking"
ADD COLUMN IF NOT EXISTS "Status" VARCHAR(50) DEFAULT 'PENDING';

-- Add comment to describe the column
COMMENT ON COLUMN public."StockReceivingTracking"."Status" IS 'Stock receiving workflow status: PENDING, GATE_ENTRY, UNLOADING, LOAD_EMPTY, COMPLETED';

-- Update existing records to have default status
UPDATE public."StockReceivingTracking"
SET "Status" = 'PENDING'
WHERE "Status" IS NULL;

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_delivery_loading_tracking_status
ON public."DeliveryLoadingTracking"("Status");

CREATE INDEX IF NOT EXISTS idx_stock_receiving_tracking_status
ON public."StockReceivingTracking"("Status");

-- Success message
SELECT 'Migration completed successfully: Status columns added to DeliveryLoadingTracking and StockReceivingTracking tables' AS message;
