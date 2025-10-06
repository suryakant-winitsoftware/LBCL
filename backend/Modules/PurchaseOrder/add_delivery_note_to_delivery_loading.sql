-- Add DeliveryNoteFilePath and DeliveryNoteNumber columns to DeliveryLoadingTracking table

ALTER TABLE public."DeliveryLoadingTracking"
ADD COLUMN IF NOT EXISTS "DeliveryNoteFilePath" varchar(500);

ALTER TABLE public."DeliveryLoadingTracking"
ADD COLUMN IF NOT EXISTS "DeliveryNoteNumber" varchar(100);

COMMENT ON COLUMN public."DeliveryLoadingTracking"."DeliveryNoteFilePath" IS 'Path to the uploaded delivery note PDF file';
COMMENT ON COLUMN public."DeliveryLoadingTracking"."DeliveryNoteNumber" IS 'SAP Delivery Note Number';
