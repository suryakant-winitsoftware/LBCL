-- Migration script to change DeliveryLoadingTracking from PurchaseOrderUID to WHStockRequestUID
-- This changes the column to reference WH Stock Requests instead of Purchase Orders

-- Step 1: Drop the foreign key constraint
ALTER TABLE public."DeliveryLoadingTracking"
DROP CONSTRAINT IF EXISTS "FK_DeliveryLoadingTracking_PurchaseOrder";

-- Step 2: Drop the existing index
DROP INDEX IF EXISTS "IX_DeliveryLoadingTracking_PurchaseOrderUID";

-- Step 3: Change column type from UUID to VARCHAR and rename
ALTER TABLE public."DeliveryLoadingTracking"
ALTER COLUMN "PurchaseOrderUID" TYPE VARCHAR(255);

-- Step 4: Rename the column
ALTER TABLE public."DeliveryLoadingTracking"
RENAME COLUMN "PurchaseOrderUID" TO "WHStockRequestUID";

-- Step 5: Create new index on the renamed column
CREATE INDEX IF NOT EXISTS "IX_DeliveryLoadingTracking_WHStockRequestUID"
    ON public."DeliveryLoadingTracking" ("WHStockRequestUID");

-- Step 6: Update comment
COMMENT ON TABLE public."DeliveryLoadingTracking"
    IS 'Tracks delivery loading information for WH Stock Requests including vehicle, personnel, timings, and signatures';

-- Note: Foreign key constraint is not added because wh_stock_request table uses varchar UIDs
-- and may not have a primary key constraint that can be referenced
