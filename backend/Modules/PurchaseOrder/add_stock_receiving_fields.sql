-- Add missing fields to StockReceivingTracking table
-- These fields track additional employee and time data for stock receiving

-- PostgreSQL version
ALTER TABLE public."StockReceivingTracking"
ADD COLUMN IF NOT EXISTS "ForkLiftOperatorUID" varchar(250),
ADD COLUMN IF NOT EXISTS "LoadEmptyStockEmployeeUID" varchar(250),
ADD COLUMN IF NOT EXISTS "GetpassEmployeeUID" varchar(250),
ADD COLUMN IF NOT EXISTS "LoadEmptyStockTime" timestamp,
ADD COLUMN IF NOT EXISTS "GetpassTime" timestamp;

-- Add comments
COMMENT ON COLUMN public."StockReceivingTracking"."ForkLiftOperatorUID" IS 'Fork lift operator employee UID';
COMMENT ON COLUMN public."StockReceivingTracking"."LoadEmptyStockEmployeeUID" IS 'Employee who loaded empty stock UID';
COMMENT ON COLUMN public."StockReceivingTracking"."GetpassEmployeeUID" IS 'Employee who processed getpass UID';
COMMENT ON COLUMN public."StockReceivingTracking"."LoadEmptyStockTime" IS 'Time when empty stock loading started';
COMMENT ON COLUMN public."StockReceivingTracking"."GetpassTime" IS 'Time when getpass was processed';
