/**
 * Store Management Types
 * Based on actual database structure from 'store' table
 */

// Base Store Interface matching database columns
// Supporting both PascalCase (from API) and camelCase (standard)
export interface IStore {
  id?: number;
  uid?: string;
  UID?: string;
  created_by?: string;
  created_time?: Date | string;
  modified_by?: string;
  modified_time?: Date | string;
  server_add_time?: Date | string;
  server_modified_time?: Date | string;
  
  // Company Information
  company_uid?: string;
  franchisee_org_uid?: string;
  
  // Basic Store Information (API uses PascalCase)
  code?: string;
  Code?: string;
  number?: string;
  Number?: string;
  name?: string;
  Name?: string;
  alias_name?: string;
  AliasName?: string;
  legal_name?: string;
  LegalName?: string;
  arabic_name?: string;
  ArabicName?: string;
  outlet_name?: string;
  OutletName?: string;
  
  // Store Type and Classification
  type?: string; // FRC, etc.
  Type?: string;
  broad_classification?: string; // GT, More, Mother Dairy, etc.
  BroadClassification?: string;
  classfication_type?: string;
  ClassificationType?: string;
  store_class?: string;
  StoreClass?: string;
  store_rating?: string;
  StoreRating?: string;
  
  // Store References (Self-referencing)
  bill_to_store_uid?: string;
  ship_to_store_uid?: string;
  sold_to_store_uid?: string;
  
  // Status and Flags
  status?: string; // Approved, Pending, etc.
  Status?: string;
  is_active?: boolean;
  IsActive?: boolean;
  is_blocked?: boolean;
  IsBlocked?: boolean;
  blocked_reason_code?: string;
  BlockedReasonCode?: string;
  blocked_reason_description?: string;
  BlockedReasonDescription?: string;
  blocked_by_emp_uid?: string;
  BlockedByEmpUID?: string;
  is_available_to_use?: boolean;
  IsAvailableToUse?: boolean;
  
  // Location Information
  country_uid?: string;
  CountryUID?: string;
  region_uid?: string;
  RegionUID?: string;
  state_uid?: string;
  StateUID?: string;
  city_uid?: string;
  CityUID?: string;
  location_uid?: string;
  LocationUID?: string;
  
  // Employee Information
  created_by_emp_uid?: string;
  created_by_job_position_uid?: string;
  prospect_emp_uid?: string;
  reporting_emp_uid?: string;
  
  // Tax Information
  is_tax_applicable?: boolean;
  tax_doc_number?: string;
  is_tax_doc_verified?: boolean;
  tax_key_field?: string;
  tax_type?: string;
  is_vat_qr_capture_mandatory?: boolean;
  
  // Additional Fields
  source?: string;
  school_warehouse?: string;
  day_type?: string;
  special_day?: Date | string;
  store_size?: number;
  store_image?: string;
  route_type?: string;
  price_type?: string;
  tab_type?: string;
  is_asm_mapped_by_customer?: boolean;
  
  // Related data (not in main table, joined from other tables)
  email?: string;
  mobile?: string;
  total_outstandings?: number;
}

// Store Additional Info Interface
export interface IStoreAdditionalInfo {
  id?: number;
  uid: string;
  store_uid: string;
  order_type?: string;
  is_promotions_block?: boolean;
  customer_start_date?: Date | string;
  customer_end_date?: Date | string;
  purchase_order_number?: string;
  delivery_docket_is_purchase_order_required?: number;
  is_with_printed_invoices?: boolean;
  is_capture_signature_required?: boolean;
  is_always_printed?: number;
  building_delivery_code?: string;
  delivery_information?: string;
  bank_uid?: string;
}

// Store Credit Interface
export interface IStoreCredit {
  id?: number;
  uid: string;
  store_uid: string;
  org_uid?: string;
  distribution_channel_uid?: string;
  credit_limit?: number;
  credit_days?: number;
  outstanding_invoices?: number;
  credit_utilized?: number;
  payment_terms?: string;
  is_credit_blocked?: boolean;
  blocked_reason?: string;
}

// Store Contact Interface
export interface IContact {
  id?: number;
  uid: string;
  linked_item_uid: string; // Store UID
  name: string;
  mobile?: string;
  email?: string;
  designation?: string;
  is_primary?: boolean;
  contact_type?: string;
}

// Store Address Interface
export interface IAddress {
  id?: number;
  uid: string;
  linked_item_uid: string; // Store UID
  address_type: string; // Billing, Shipping, etc.
  address_line1: string;
  address_line2?: string;
  city: string;
  state?: string;
  postal_code?: string;
  country: string;
  latitude?: number;
  longitude?: number;
  is_default?: boolean;
}

// Store Document Interface
export interface IStoreDocument {
  id?: number;
  uid: string;
  store_uid: string;
  document_type: string;
  document_name: string;
  document_path?: string;
  document_number?: string;
  issue_date?: Date | string;
  expiry_date?: Date | string;
  is_verified?: boolean;
  verified_by?: string;
  verification_date?: Date | string;
}

// Store Master - Complete Store with all related data
export interface IStoreMaster {
  store: IStore;
  storeAdditionalInfo?: IStoreAdditionalInfo;
  storeCredits?: IStoreCredit[];
  contacts?: IContact[];
  addresses?: IAddress[];
  storeDocuments?: IStoreDocument[];
}

// Store Customer for Route Mapping
export interface IStoreCustomer {
  uid: string;
  code: string;
  label: string;
  address?: string;
  routeCustomerUID?: string;
  isDeleted?: boolean;
  seqNo?: number;
  isSelected?: boolean;
}

// Route Customer Mapping
export interface IRouteCustomer {
  id?: number;
  uid: string;
  route_uid: string;
  store_uid: string;
  seq_no: number;
  visit_time?: string;
  visit_duration?: number;
  end_time?: string;
  is_deleted: boolean;
  travel_time?: number;
}

// Request/Response Types (using camelCase for JSON serialization)
export interface StoreListRequest {
  pageNumber: number;
  pageSize: number;
  filterCriterias?: FilterCriteria[];
  sortCriterias?: SortCriteria[];
  isCountRequired?: boolean;
}

export interface FilterCriteria {
  name: string;
  value: any;
  operator?: string; // equals, contains, greater_than, etc.
  type?: number; // FilterType enum value
}

export interface SortCriteria {
  sortParameter: string;
  direction: 0 | 1; // 0 = ASC, 1 = DESC
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string;
  data: T;
  currentServerTime?: Date | string;
}

// Store Type Options (from database)
export const STORE_TYPES = {
  FRC: 'Franchise',
  RETAIL: 'Retail',
  WHOLESALE: 'Wholesale',
  DISTRIBUTOR: 'Distributor'
} as const;

// Store Classifications (from actual data)
export const STORE_CLASSIFICATIONS = [
  'GT', // General Trade
  'More',
  'Mother Dairy',
  'Apna Mart',
  'Reliance',
  'Wellness Forever',
  'KPN Fresh',
  'Spencers',
  '7 Eleven',
  'Metro CNC'
] as const;

// Store Status Options
export const STORE_STATUS = {
  PENDING: 'Pending',
  APPROVED: 'Approved',
  ACTIVE: 'Active',
  INACTIVE: 'Inactive',
  BLOCKED: 'Blocked'
} as const;

// Form Data Types
export interface StoreFormData {
  // Basic Information
  code: string;
  name: string;
  type: string;
  broad_classification?: string;
  status: string;
  is_active: boolean;
  
  // Company & Organization
  company_uid?: string;
  franchisee_org_uid?: string;
  
  // Location
  country_uid?: string;
  region_uid?: string;
  state_uid?: string;
  city_uid?: string;
  
  // Contact Information
  email?: string;
  mobile?: string;
  
  // Tax Information
  is_tax_applicable?: boolean;
  tax_doc_number?: string;
  tax_type?: string;
  
  // Additional Info
  store_size?: number;
  store_class?: string;
  store_rating?: string;
}

// Search and Filter Types
export interface StoreSearchParams {
  searchText?: string;
  type?: string;
  classification?: string;
  status?: string;
  isActive?: boolean;
  country?: string;
  region?: string;
  city?: string;
}

// Export types for use in components
export type StoreType = keyof typeof STORE_TYPES;
export type StoreClassification = typeof STORE_CLASSIFICATIONS[number];
export type StoreStatus = keyof typeof STORE_STATUS;