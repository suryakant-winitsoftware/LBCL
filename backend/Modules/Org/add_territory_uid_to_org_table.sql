-- Migration: Add territory_uid column to org table
-- Purpose: Store sales territory assignment for organizations (distributors)
-- Date: 2025-01-05

-- PostgreSQL Migration
-- Add territory_uid column if it doesn't exist
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_name = 'org'
        AND column_name = 'territory_uid'
    ) THEN
        ALTER TABLE org ADD COLUMN territory_uid VARCHAR(250);

        -- Add comment for documentation
        COMMENT ON COLUMN org.territory_uid IS 'Foreign key reference to territory table - stores sales territory assignment';

        -- Optional: Add foreign key constraint if territory table exists
        -- ALTER TABLE org ADD CONSTRAINT fk_org_territory
        -- FOREIGN KEY (territory_uid) REFERENCES territory(uid);

        RAISE NOTICE 'Column territory_uid added to org table successfully';
    ELSE
        RAISE NOTICE 'Column territory_uid already exists in org table';
    END IF;
END $$;

-- Create index for better query performance
CREATE INDEX IF NOT EXISTS idx_org_territory_uid ON org(territory_uid);

-- Verify the column was added
SELECT column_name, data_type, character_maximum_length, is_nullable
FROM information_schema.columns
WHERE table_name = 'org' AND column_name = 'territory_uid';
