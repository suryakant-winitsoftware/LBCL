/**
 * Store Credit Management Types
 * Based on actual database structure from 'store_credit' table
 */

// Store Credit Interface matching database columns
export interface IStoreCredit {
  id?: number;
  UID?: string;
  uid?: string;
  CreatedBy?: string;
  created_by?: string;
  CreatedTime?: Date | string;
  created_time?: Date | string;
  ModifiedBy?: string;
  modified_by?: string;
  ModifiedTime?: Date | string;
  modified_time?: Date | string;
  ServerAddTime?: Date | string;
  server_add_time?: Date | string;
  ServerModifiedTime?: Date | string;
  server_modified_time?: Date | string;

  // Store and Organization Info
  StoreUID?: string;
  store_uid?: string;
  OrgUID?: string;
  org_uid?: string;
  OrgLabel?: string;
  org_label?: string;
  DistributionChannelUID?: string;
  distribution_channel_uid?: string;
  DCLabel?: string;
  DivisionOrgUID?: string;
  division_org_uid?: string;
  DivisionName?: string;
  division_name?: string;

  // Payment Terms
  PaymentTermUID?: string;
  payment_term_uid?: string;
  PaymentTermLabel?: string;
  payment_term_label?: string;
  PreferredPaymentMode?: string;
  preferred_payment_mode?: string;
  PreferredPaymentMethod?: string;
  preferred_payment_method?: string;
  PaymentType?: string;
  payment_type?: string;
  PaymentTypeLabel?: string;
  payment_type_label?: string;

  // Credit Information
  CreditType?: string;
  credit_type?: string;
  CreditLimit?: number;
  credit_limit?: number;
  TemporaryCredit?: number;
  temporary_credit?: number;
  CreditDays?: number;
  credit_days?: number;
  TemporaryCreditDays?: number;
  temporary_credit_days?: number;
  TemporaryCreditApprovalDate?: Date | string;
  temporary_credit_approval_date?: Date | string;

  // Status and Flags
  IsActive?: boolean;
  is_active?: boolean;
  IsBlocked?: boolean;
  is_blocked?: boolean;
  BlockingReasonCode?: string;
  blocking_reason_code?: string;
  BlockingReasonDescription?: string;
  blocking_reason_description?: string;
  Disabled?: boolean;
  disabled?: boolean;

  // Price and Authorization
  PriceList?: string;
  price_list?: string;
  AuthorizedItemGRPKey?: string;
  authorized_item_grp_key?: string;
  MessageKey?: string;
  message_key?: string;
  TaxKeyField?: string;
  tax_key_field?: string;
  PromotionKey?: string;
  promotion_key?: string;

  // Address Information
  BillToAddressUID?: string;
  bill_to_address_uid?: string;
  ShipToAddressUID?: string;
  ship_to_address_uid?: string;
  StoreGroupDataUID?: string;
  store_group_data_uid?: string;

  // Invoice and Outstanding
  OutstandingInvoices?: number;
  outstanding_invoices?: number;
  InvoiceAdminFeePerBillingCycle?: number;
  invoice_admin_fee_per_billing_cycle?: number;
  InvoiceAdminFeePerDelivery?: number;
  invoice_admin_fee_per_delivery?: number;
  InvoiceLatePaymentFee?: number;
  invoice_late_payment_fee?: number;
  IsCancellationOfInvoiceAllowed?: boolean;
  is_cancellation_of_invoice_allowed?: boolean;
  IsAllowCashOnCreditExceed?: boolean;
  is_allow_cash_on_credit_exceed?: boolean;
  IsOutstandingBillControl?: boolean;
  is_outstanding_bill_control?: boolean;
  IsNegativeInvoiceAllowed?: boolean;
  is_negative_invoice_allowed?: boolean;

  // Calculated Fields (not in database)
  TotalBalance?: number;
  total_balance?: number;
  OverdueBalance?: number;
  overdue_balance?: number;
  AvailableBalance?: number;
  available_balance?: number;
  AsmEmpName?: string;
  asm_emp_name?: string;

  // For selection in UI
  IsSelected?: boolean;
}

// Store Credit Limit Interface
export interface IStoreCreditLimit {
  StoreUID?: string;
  store_uid?: string;
  Division?: string;
  division?: string;
  DivisionOrgUID?: string;
  division_org_uid?: string;
  CreditLimit?: number;
  credit_limit?: number;
  TemporaryCreditLimit?: number;
  temporary_credit_limit?: number;
  CurrentOutstanding?: number;
  current_outstanding?: number;
  CreditDays?: number;
  credit_days?: number;
  MaxAgingDays?: number;
  max_aging_days?: number;
  TemporaryCreditDays?: number;
  temporary_credit_days?: number;
  DueDate?: Date | string;
  due_date?: Date | string;
  TemporaryCreditApprovalDate?: Date | string;
  temporary_credit_approval_date?: Date | string;
  BlockedLimit?: number;
  blocked_limit?: number;
}

// Store Credit Limit Request
export interface StoreCreditLimitRequest {
  StoreUids: string[];
  DivisionUID: string;
}

// Request types
export interface StoreCreditListRequest {
  PageNumber?: number;
  pageNumber?: number;
  PageSize?: number;
  pageSize?: number;
  FilterCriterias?: FilterCriteria[];
  filterCriterias?: FilterCriteria[];
  SortCriterias?: SortCriteria[];
  sortCriterias?: SortCriteria[];
  IsCountRequired?: boolean;
  isCountRequired?: boolean;
}

export interface FilterCriteria {
  Name?: string;
  name?: string;
  Value?: any;
  value?: any;
  Type?: number;
  type?: number;
  FilterMode?: number;
  filterMode?: number;
}

export interface SortCriteria {
  SortParameter?: string;
  sortParameter?: string;
  Direction?: number;
  direction?: number;
}

// Response types
export interface PagedResponse<T> {
  PagedData?: T[];
  pagedData?: T[];
  TotalCount?: number;
  totalCount?: number;
}

export interface ApiResponse<T> {
  Data?: T;
  data?: T;
  StatusCode?: number;
  statusCode?: number;
  IsSuccess?: boolean;
  isSuccess?: boolean;
  ErrorMessage?: string;
  errorMessage?: string;
}

// Credit type options
export enum CreditType {
  CASH = 'CASH',
  CREDIT = 'CREDIT',
  MIXED = 'MIXED'
}

// Payment mode options
export enum PaymentMode {
  CASH = 'CASH',
  CREDIT = 'CREDIT',
  CHEQUE = 'CHEQUE',
  BANK_TRANSFER = 'BANK_TRANSFER',
  MOBILE_MONEY = 'MOBILE_MONEY'
}

// Store Credit Search Parameters
export interface StoreCreditSearchParams {
  searchText?: string;
  creditType?: string;
  isActive?: boolean;
  isBlocked?: boolean;
  storeUID?: string;
  orgUID?: string;
}