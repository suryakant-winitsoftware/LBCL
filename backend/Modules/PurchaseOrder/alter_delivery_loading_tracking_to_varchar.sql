-- PostgreSQL Script to alter DeliveryLoadingTracking columns from uuid to varchar
-- This allows storing vehicle numbers and employee codes instead of GUIDs

-- Change VehicleUID from uuid to varchar
ALTER TABLE public."DeliveryLoadingTracking"
    ALTER COLUMN "VehicleUID" TYPE varchar(100);

-- Change DriverEmployeeUID from uuid to varchar
ALTER TABLE public."DeliveryLoadingTracking"
    ALTER COLUMN "DriverEmployeeUID" TYPE varchar(100);

-- Change ForkLiftOperatorUID from uuid to varchar
ALTER TABLE public."DeliveryLoadingTracking"
    ALTER COLUMN "ForkLiftOperatorUID" TYPE varchar(100);

-- Change SecurityOfficerUID from uuid to varchar
ALTER TABLE public."DeliveryLoadingTracking"
    ALTER COLUMN "SecurityOfficerUID" TYPE varchar(100);

-- Verify the changes
SELECT column_name, data_type, character_maximum_length
FROM information_schema.columns
WHERE table_name = 'DeliveryLoadingTracking'
  AND column_name IN ('VehicleUID', 'DriverEmployeeUID', 'ForkLiftOperatorUID', 'SecurityOfficerUID');
