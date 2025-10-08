-- Migration script to change StockReceivingTracking from PurchaseOrderUID to WHStockRequestUID
-- This aligns with the DeliveryLoadingTracking migration

-- Step 1: Drop the old foreign key constraint
ALTER TABLE public.stock_receiving_tracking
DROP CONSTRAINT IF EXISTS fk_stock_receiving_purchase_order;

-- Step 2: Rename the column from PurchaseOrderUID to WHStockRequestUID
ALTER TABLE public.stock_receiving_tracking
RENAME COLUMN "PurchaseOrderUID" TO "WHStockRequestUID";

-- Step 3: Change the data type from UUID to VARCHAR(255) to match WH Stock Request UIDs
ALTER TABLE public.stock_receiving_tracking
ALTER COLUMN "WHStockRequestUID" TYPE VARCHAR(255);

-- Note: We don't add a foreign key constraint because wh_stock_request has a composite
-- unique key (uid, org_uid, warehouse_uid, year_month) and we only store uid.
-- Application logic ensures referential integrity.
