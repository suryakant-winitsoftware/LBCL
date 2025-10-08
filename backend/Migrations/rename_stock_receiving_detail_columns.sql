-- Migration: Rename columns in stock_receiving_detail table and remove foreign keys
-- Date: 2025-01-07

-- Step 1: Drop the old foreign key constraints
ALTER TABLE public.stock_receiving_detail
DROP CONSTRAINT IF EXISTS fk_stock_receiving_detail_po;

ALTER TABLE public.stock_receiving_detail
DROP CONSTRAINT IF EXISTS fk_stock_receiving_detail_line;

-- Step 2: Rename PurchaseOrderUID to WHStockRequestUID
ALTER TABLE public.stock_receiving_detail
RENAME COLUMN "PurchaseOrderUID" TO "WHStockRequestUID";

-- Step 3: Rename PurchaseOrderLineUID to WHStockRequestLineUID
ALTER TABLE public.stock_receiving_detail
RENAME COLUMN "PurchaseOrderLineUID" TO "WHStockRequestLineUID";

-- Note: We don't add new foreign key constraints because wh_stock_request has a composite
-- unique key (uid, org_uid, warehouse_uid, year_month) and we only store uid.
-- Application logic ensures referential integrity.

-- Verify the changes
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'stock_receiving_detail'
AND table_schema = 'public'
ORDER BY ordinal_position;
