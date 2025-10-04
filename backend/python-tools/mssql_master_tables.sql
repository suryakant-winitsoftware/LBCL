-- MSSQL Master Tables Migration
-- Database: LBSSFADev
-- Tables structure only (no data)

USE [LBSSFADev];
GO

-- Table: address
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[address]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[address] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [type] NVARCHAR(50) NULL,
        [name] NVARCHAR(200) NULL,
        [line1] NVARCHAR(200) NULL,
        [line2] NVARCHAR(200) NULL,
        [line3] NVARCHAR(200) NULL,
        [landmark] NVARCHAR(200) NULL,
        [area] NVARCHAR(100) NULL,
        [sub_area] NVARCHAR(100) NULL,
        [zip_code] NVARCHAR(10) NULL,
        [city] NVARCHAR(100) NULL,
        [country_code] NVARCHAR(50) NULL,
        [region_code] NVARCHAR(50) NULL,
        [phone] NVARCHAR(50) NULL,
        [phone_extension] NVARCHAR(50) NULL,
        [mobile1] NVARCHAR(50) NULL,
        [mobile2] NVARCHAR(50) NULL,
        [email] NVARCHAR(100) NULL,
        [fax] NVARCHAR(50) NULL,
        [latitude] NVARCHAR(50) NULL,
        [longitude] NVARCHAR(50) NULL,
        [altitude] DECIMAL(10,5) NULL,
        [linked_item_uid] NVARCHAR(250) NULL,
        [linked_item_type] NVARCHAR(250) NULL,
        [status] NVARCHAR(50) NULL,
        [state_code] NVARCHAR(50) NULL,
        [territory_code] NVARCHAR(50) NULL,
        [pan] NVARCHAR(20) NULL,
        [aadhar] NVARCHAR(20) NULL,
        [ssn] NVARCHAR(20) NULL,
        [is_editable] BIT NULL,
        [is_default] BIT NULL,
        [line4] NVARCHAR(200) NULL,
        [info] NVARCHAR(250) NULL,
        [depot] NVARCHAR(250) NULL,
        [location_uid] NVARCHAR(250) NULL,
        [custom_field1] NVARCHAR(250) NULL,
        [custom_field2] NVARCHAR(250) NULL,
        [custom_field3] NVARCHAR(250) NULL,
        [custom_field4] NVARCHAR(250) NULL,
        [custom_field5] NVARCHAR(250) NULL,
        [custom_field6] NVARCHAR(250) NULL,
        [is_multiple_asm_allowed] NVARCHAR(MAX) NULL,
        [branch_uid] NVARCHAR(250) NULL,
        [asm_emp_uid] NVARCHAR(250) NULL,
        [sales_office_uid] NVARCHAR(250) NULL,
        [state] NVARCHAR(250) NULL,
        [locality] NVARCHAR(250) NULL,
        [org_unit_uid] NVARCHAR(250) NULL,
        [ss] INT NULL,
        CONSTRAINT [PK_address] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: bank
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[bank]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[bank] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [company_uid] NVARCHAR(250) NULL,
        [bank_name] NVARCHAR(250) NULL,
        [country_uid] NVARCHAR(250) NULL,
        [cheque_fee] DECIMAL(18,4) NULL,
        [ss] INT NULL,
        [created_time] DATETIME2 NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL DEFAULT now(),
        [server_modified_time] DATETIME2 NULL DEFAULT now(),
        [bank_code] NVARCHAR(50) NULL,
        CONSTRAINT [PK_bank] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: country
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[country]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[country] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [code] NVARCHAR(250) NULL,
        [name] NVARCHAR(100) NULL,
        [calling_code] NVARCHAR(100) NULL,
        [tld] NVARCHAR(100) NULL,
        CONSTRAINT [PK_country] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: currency
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[currency]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[currency] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [name] NVARCHAR(50) NULL,
        [symbol] NVARCHAR(10) NULL,
        [digits] INT NULL,
        [code] NVARCHAR(50) NULL,
        [fraction_name] NVARCHAR(50) NULL,
        [ss] INT NULL,
        [created_time] DATETIME2 NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL DEFAULT CURRENT_TIMESTAMP,
        [server_modified_time] DATETIME2 NULL DEFAULT CURRENT_TIMESTAMP,
        [round_off_max_limit] DECIMAL(18,4) NULL,
        [round_off_min_limit] DECIMAL(18,4) NULL,
        CONSTRAINT [PK_currency] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: sku
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sku]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[sku] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [company_uid] NVARCHAR(250) NULL,
        [org_uid] NVARCHAR(250) NOT NULL,
        [code] NVARCHAR(250) NOT NULL,
        [name] NVARCHAR(250) NULL,
        [arabic_name] NVARCHAR(250) NULL,
        [alias_name] NVARCHAR(250) NULL,
        [long_name] NVARCHAR(250) NULL,
        [base_uom] NVARCHAR(50) NULL,
        [outer_uom] NVARCHAR(50) NULL,
        [from_date] DATE NULL,
        [to_date] DATE NULL,
        [is_stockable] BIT NULL,
        [parent_uid] NVARCHAR(250) NULL,
        [is_active] BIT NULL,
        [is_third_party] BIT NULL,
        [supplier_org_uid] NVARCHAR(250) NULL,
        [ss] INT NULL DEFAULT 0,
        CONSTRAINT [PK_sku] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: sku_group
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sku_group]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[sku_group] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [sku_group_type_uid] NVARCHAR(250) NULL,
        [code] NVARCHAR(100) NULL,
        [name] NVARCHAR(250) NULL,
        [parent_uid] NVARCHAR(250) NULL,
        [item_level] INT NULL,
        [supplier_org_uid] NVARCHAR(250) NULL
    );
END
GO

-- Table: store
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[store]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[store] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [company_uid] NVARCHAR(250) NULL,
        [code] NVARCHAR(50) NULL,
        [number] NVARCHAR(50) NULL,
        [name] NVARCHAR(250) NULL,
        [alias_name] NVARCHAR(500) NULL,
        [legal_name] NVARCHAR(500) NULL,
        [type] NVARCHAR(50) NULL,
        [bill_to_store_uid] NVARCHAR(250) NULL,
        [ship_to_store_uid] NVARCHAR(250) NULL,
        [sold_to_store_uid] NVARCHAR(250) NULL,
        [status] NVARCHAR(50) NULL,
        [is_active] BIT NULL,
        [store_class] NVARCHAR(50) NULL,
        [store_rating] NVARCHAR(50) NULL,
        [is_blocked] BIT NULL,
        [blocked_reason_code] NVARCHAR(250) NULL,
        [blocked_reason_description] NVARCHAR(250) NULL,
        [created_by_emp_uid] NVARCHAR(250) NULL,
        [created_by_job_position_uid] NVARCHAR(250) NULL,
        [country_uid] NVARCHAR(250) NULL,
        [region_uid] NVARCHAR(250) NULL,
        [city_uid] NVARCHAR(250) NULL,
        [source] NVARCHAR(50) NULL,
        [arabic_name] NVARCHAR(250) NULL,
        [outlet_name] NVARCHAR(250) NULL,
        [blocked_by_emp_uid] NVARCHAR(250) NULL,
        [is_tax_applicable] BIT NULL,
        [tax_doc_number] NVARCHAR(50) NULL,
        [school_warehouse] NVARCHAR(50) NULL,
        [day_type] NVARCHAR(50) NULL,
        [special_day] DATE NULL,
        [is_tax_doc_verified] BIT NULL,
        [store_size] DECIMAL(9,0) NULL,
        [prospect_emp_uid] NVARCHAR(250) NULL,
        [tax_key_field] NVARCHAR(250) NULL,
        [store_image] NVARCHAR(250) NULL,
        [is_vat_qr_capture_mandatory] BIT NULL,
        [tax_type] NVARCHAR(50) NULL,
        [franchisee_org_uid] NVARCHAR(250) NULL,
        [state_uid] NVARCHAR(255) NULL,
        [route_type] NVARCHAR(255) NULL,
        [price_type] NVARCHAR(255) NULL,
        [location_uid] NVARCHAR(250) NULL,
        [broad_classification] NVARCHAR(50) NULL,
        [classfication_type] NVARCHAR(50) NULL,
        [tab_type] NVARCHAR(250) NULL,
        [reporting_emp_uid] NVARCHAR(250) NULL,
        [is_asm_mapped_by_customer] NVARCHAR(MAX) NULL,
        [is_available_to_use] BIT NULL,
        [latitude] DECIMAL(10,8) NULL,
        [longitude] DECIMAL(11,8) NULL,
        CONSTRAINT [PK_store] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: tax
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tax]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tax] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [name] NVARCHAR(200) NULL,
        [legal_name] NVARCHAR(50) NULL,
        [applicable_at] NVARCHAR(100) NULL,
        [tax_calculation_type] NVARCHAR(50) NULL,
        [base_tax_rate] DECIMAL(18,4) NULL,
        [status] NVARCHAR(50) NULL,
        [valid_from] DATE NULL,
        [valid_upto] DATE NULL,
        [code] NVARCHAR(50) NULL,
        [is_tax_on_tax_applicable] BIT NOT NULL DEFAULT false,
        CONSTRAINT [PK_tax] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: vehicle
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[vehicle]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[vehicle] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [company_uid] NVARCHAR(250) NULL,
        [org_uid] NVARCHAR(250) NULL,
        [vehicle_no] NVARCHAR(250) NULL,
        [registration_no] NVARCHAR(250) NULL,
        [model] NVARCHAR(250) NULL,
        [type] NVARCHAR(250) NULL,
        [is_active] BIT NULL,
        [truck_si_date] DATE NULL,
        [road_tax_expiry_date] DATE NULL,
        [inspection_date] DATE NULL,
        CONSTRAINT [PK_vehicle] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: route
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[route]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[route] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [created_by] NVARCHAR(250) NOT NULL,
        [created_time] DATETIME2 NULL,
        [modified_by] NVARCHAR(250) NOT NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [company_uid] NVARCHAR(250) NULL,
        [code] NVARCHAR(50) NULL,
        [name] NVARCHAR(200) NULL,
        [role_uid] NVARCHAR(250) NOT NULL,
        [org_uid] NVARCHAR(250) NULL,
        [wh_org_uid] NVARCHAR(250) NULL,
        [vehicle_uid] NVARCHAR(250) NULL,
        [job_position_uid] NVARCHAR(250) NULL,
        [location_uid] NVARCHAR(250) NULL,
        [is_active] BIT NULL,
        [status] NVARCHAR(50) NULL,
        [valid_from] DATE NULL,
        [valid_upto] DATE NULL,
        [print_standing] BIT NULL,
        [print_topup] BIT NULL,
        [print_order_summary] BIT NULL,
        [auto_freeze_jp] BIT NULL,
        [add_to_run] BIT NULL,
        [auto_freeze_run_time] NVARCHAR(250) NULL,
        [ss] INT NULL,
        [total_customers] INT NULL,
        [print_forward] BIT NULL,
        [visit_time] NVARCHAR(250) NULL,
        [end_time] NVARCHAR(250) NULL,
        [visit_duration] INT NULL,
        [travel_time] INT NULL,
        [is_customer_with_time] BIT NULL,
        CONSTRAINT [PK_route] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

-- Table: promotion
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[promotion]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[promotion] (
        [id] INT NOT NULL,
        [uid] NVARCHAR(250) NOT NULL,
        [company_uid] NVARCHAR(250) NULL,
        [org_uid] NVARCHAR(250) NULL,
        [code] NVARCHAR(50) NULL,
        [name] NVARCHAR(250) NULL,
        [remarks] NVARCHAR(MAX) NULL,
        [category] NVARCHAR(50) NULL,
        [has_slabs] BIT NULL,
        [created_by_emp_uid] NVARCHAR(250) NULL,
        [valid_from] DATETIME2 NULL,
        [valid_upto] DATETIME2 NULL,
        [type] NVARCHAR(50) NULL,
        [promo_format] NVARCHAR(MAX) NULL,
        [is_active] BIT NULL,
        [promo_title] NVARCHAR(200) NULL,
        [promo_message] NVARCHAR(500) NULL,
        [has_fact_sheet] BIT NULL,
        [priority] INT NULL,
        [ss] INT NULL,
        [created_time] DATETIME2 NULL,
        [modified_time] DATETIME2 NULL,
        [server_add_time] DATETIME2 NULL,
        [server_modified_time] DATETIME2 NULL,
        [created_by] NVARCHAR(250) NULL,
        [modified_by] NVARCHAR(250) NULL,
        [contribution_level3] DECIMAL(18,3) NULL,
        [contribution_level1] DECIMAL(18,3) NULL,
        [contribution_level2] DECIMAL(18,3) NULL,
        CONSTRAINT [PK_promotion] PRIMARY KEY CLUSTERED ([uid])
    );
END
GO

