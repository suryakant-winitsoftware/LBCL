-- Migration: Rename columns in StockReceivingDetail table
-- Date: 2025-01-07

-- Rename PurchaseOrderUID to WHStockRequestUID
ALTER TABLE public."StockReceivingDetail"
RENAME COLUMN "PurchaseOrderUID" TO "WHStockRequestUID";

-- Rename PurchaseOrderLineUID to WHStockRequestLineUID
ALTER TABLE public."StockReceivingDetail"
RENAME COLUMN "PurchaseOrderLineUID" TO "WHStockRequestLineUID";

-- Verify the changes
SELECT column_name, data_type
FROM information_schema.columns
WHERE table_name = 'StockReceivingDetail'
AND table_schema = 'public'
ORDER BY ordinal_position;
