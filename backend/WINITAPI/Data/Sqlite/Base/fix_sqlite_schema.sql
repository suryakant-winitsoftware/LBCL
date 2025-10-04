-- SQLite Database Schema Fix Script
-- This script fixes the missing tables and columns that are causing synchronization errors

-- 1. Add missing columns to existing tables

-- Add created_by column to tax table
ALTER TABLE tax ADD COLUMN created_by VARCHAR(250);

-- Add created_by column to sku table  
ALTER TABLE sku ADD COLUMN created_by VARCHAR(250);

-- Add created_by column to vehicle table
ALTER TABLE vehicle ADD COLUMN created_by VARCHAR(250);

-- Add code column to currency table
ALTER TABLE currency ADD COLUMN code VARCHAR(50);

-- 2. Create missing tables

-- Create store_group_data table
CREATE TABLE IF NOT EXISTS store_group_data (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UID VARCHAR(250) UNIQUE NOT NULL,
    CreatedBy VARCHAR(250),
    CreatedTime DATETIME,
    ModifiedBy VARCHAR(250),
    ModifiedTime DATETIME,
    ServerAddTime DATETIME,
    ServerModifiedTime DATETIME,
    CompanyUID VARCHAR(250),
    OrgUID VARCHAR(250),
    StoreGroupUID VARCHAR(250),
    StoreUID VARCHAR(250),
    IsActive INTEGER DEFAULT 1
);

-- Create org_hierarchy table
CREATE TABLE IF NOT EXISTS org_hierarchy (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UID VARCHAR(250) UNIQUE NOT NULL,
    CreatedBy VARCHAR(250),
    CreatedTime DATETIME,
    ModifiedBy VARCHAR(250),
    ModifiedTime DATETIME,
    ServerAddTime DATETIME,
    ServerModifiedTime DATETIME,
    CompanyUID VARCHAR(250),
    OrgUID VARCHAR(250),
    ParentOrgUID VARCHAR(250),
    HierarchyLevel INTEGER,
    IsActive INTEGER DEFAULT 1
);

-- Create list_item table
CREATE TABLE IF NOT EXISTS list_item (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UID VARCHAR(250) UNIQUE NOT NULL,
    CreatedBy VARCHAR(250),
    CreatedTime DATETIME,
    ModifiedBy VARCHAR(250),
    ModifiedTime DATETIME,
    ServerAddTime DATETIME,
    ServerModifiedTime DATETIME,
    CompanyUID VARCHAR(250),
    OrgUID VARCHAR(250),
    ListHeaderUID VARCHAR(250),
    Code VARCHAR(100),
    Name VARCHAR(200),
    Description TEXT,
    SortOrder INTEGER,
    IsActive INTEGER DEFAULT 1
);

-- Create org_currency table
CREATE TABLE IF NOT EXISTS org_currency (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UID VARCHAR(250) UNIQUE NOT NULL,
    CreatedBy VARCHAR(250),
    CreatedTime DATETIME,
    ModifiedBy VARCHAR(250),
    ModifiedTime DATETIME,
    ServerAddTime DATETIME,
    ServerModifiedTime DATETIME,
    CompanyUID VARCHAR(250),
    OrgUID VARCHAR(250),
    CurrencyUID VARCHAR(250),
    IsDefault INTEGER DEFAULT 0,
    IsActive INTEGER DEFAULT 1
);

-- Create tax_group_taxes table
CREATE TABLE IF NOT EXISTS tax_group_taxes (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UID VARCHAR(250) UNIQUE NOT NULL,
    CreatedBy VARCHAR(250),
    CreatedTime DATETIME,
    ModifiedBy VARCHAR(250),
    ModifiedTime DATETIME,
    ServerAddTime DATETIME,
    ServerModifiedTime DATETIME,
    CompanyUID VARCHAR(250),
    OrgUID VARCHAR(250),
    TaxGroupUID VARCHAR(250),
    TaxUID VARCHAR(250),
    SortOrder INTEGER,
    IsActive INTEGER DEFAULT 1
);

-- 3. Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_store_group_data_store_group_uid ON store_group_data(StoreGroupUID);
CREATE INDEX IF NOT EXISTS idx_store_group_data_store_uid ON store_group_data(StoreUID);
CREATE INDEX IF NOT EXISTS idx_org_hierarchy_org_uid ON org_hierarchy(OrgUID);
CREATE INDEX IF NOT EXISTS idx_org_hierarchy_parent_org_uid ON org_hierarchy(ParentOrgUID);
CREATE INDEX IF NOT EXISTS idx_list_item_list_header_uid ON list_item(ListHeaderUID);
CREATE INDEX IF NOT EXISTS idx_org_currency_org_uid ON org_currency(OrgUID);
CREATE INDEX IF NOT EXISTS idx_org_currency_currency_uid ON org_currency(CurrencyUID);
CREATE INDEX IF NOT EXISTS idx_tax_group_taxes_tax_group_uid ON tax_group_taxes(TaxGroupUID);
CREATE INDEX IF NOT EXISTS idx_tax_group_taxes_tax_uid ON tax_group_taxes(TaxUID);

-- 4. Update existing records to set default values for new columns
UPDATE tax SET created_by = 'system' WHERE created_by IS NULL;
UPDATE sku SET created_by = 'system' WHERE created_by IS NULL;
UPDATE vehicle SET created_by = 'system' WHERE created_by IS NULL;
UPDATE currency SET code = UID WHERE code IS NULL;

-- 5. Verify the changes
SELECT 'Schema fix completed successfully' as status; 