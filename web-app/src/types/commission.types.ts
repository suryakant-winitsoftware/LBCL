/**
 * Commission Module Types
 * Production-ready type definitions for commission management
 * Compatible with WINITAPI PostgreSQL backend
 */

// Core Commission Configuration
export interface Commission {
  commission_id: string
  org_code: string
  commission_name: string
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// KPI-based Commission Structure
export interface CommissionKPI {
  commission_kpi_id: string
  commission_id: string
  org_code: string
  commission_name: string
  kpi_type_id: string
  kpi_name: string
  kpi_structure_type: string
  kpi_weight: number
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// Customer-KPI Mapping
export interface CommissionKPICustomerMapping {
  commission_kpi_customer_mapping_id: string
  commission_kpi_id: string
  linked_customer_code: string
  linked_customer_type: string
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// Product-KPI Mapping
export interface CommissionKPIProductMapping {
  commission_kpi_product_mapping_id: string
  commission_kpi_id: string
  linked_product_code: string
  linked_product_type: string
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// Commission Slab Structure
export interface CommissionKPISlab {
  commission_kpi_slab_id: string
  commission_kpi_id: string
  commission_slab_from: number
  commission_slab_to: number
  commission_kpi_slab_payout: number
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// User-Commission Mapping
export interface CommissionUserMapping {
  commission_user_mapping_id: string
  commission_id: string
  linked_user_type: string
  linked_user_code: string
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// User KPI Performance Tracking
export interface CommissionUserKPIPerformance {
  commission_user_kpi_performance_id: string
  commission_id: string
  commission_kpi_id: string
  user_code: string
  actual_sales?: number
  target_sales?: number
  sales_percent?: number
  commission_payout: number
  performance_period?: string
  calculated_date?: Date
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// User Commission Payout Summary
export interface CommissionUserPayout {
  commission_user_payout_id: string
  commission_id: string
  user_code: string
  user_name?: string
  total_commission_payout: number
  payout_period?: string
  payout_status: 'Pending' | 'Approved' | 'Paid' | 'Cancelled'
  approved_by?: string
  approved_date?: Date
  paid_date?: Date
  is_active: boolean
  created_by?: string
  created_time?: Date
  modified_by?: string
  modified_time?: Date
}

// Commission Calculation Request
export interface CommissionCalculationRequest {
  commission_id?: string
  org_code?: string
  calculation_period?: {
    start_date: Date
    end_date: Date
  }
  user_codes?: string[]
  recalculate?: boolean
}

// Commission Calculation Response
export interface CommissionCalculationResponse {
  success: boolean
  message: string
  calculation_date: Date
  processed_users: number
  total_commission_amount: number
  summary: {
    commission_id: string
    commission_name: string
    total_users: number
    total_payout: number
    performance_period: string
  }[]
}

// Commission Analytics
export interface CommissionAnalytics {
  period: string
  total_commission_paid: number
  average_commission_per_user: number
  top_performers: {
    user_code: string
    user_name: string
    commission_amount: number
    achievement_percent: number
  }[]
  kpi_performance: {
    kpi_name: string
    average_achievement: number
    total_payout: number
  }[]
}

// Commission Report Filters
export interface CommissionReportFilters {
  commission_id?: string
  user_code?: string
  date_from?: Date
  date_to?: Date
  payout_status?: string
  org_code?: string
  kpi_type?: string
}

// Commission Dashboard Data
export interface CommissionDashboard {
  total_active_commissions: number
  total_users_enrolled: number
  current_period_payout: number
  pending_approvals: number
  recent_calculations: {
    calculation_date: Date
    commission_name: string
    users_processed: number
    total_amount: number
  }[]
  performance_trends: {
    period: string
    total_payout: number
    users_count: number
    average_achievement: number
  }[]
}

// Commission Setup Wizard
export interface CommissionSetupWizard {
  step: 'basic' | 'kpis' | 'mappings' | 'slabs' | 'users' | 'review'
  commission: Partial<Commission>
  kpis: Partial<CommissionKPI>[]
  customer_mappings: Partial<CommissionKPICustomerMapping>[]
  product_mappings: Partial<CommissionKPIProductMapping>[]
  slabs: Partial<CommissionKPISlab>[]
  user_mappings: Partial<CommissionUserMapping>[]
}

// API Response Types
export interface CommissionApiResponse<T> {
  success: boolean
  message: string
  data: T
  total_count?: number
  page_number?: number
  page_size?: number
}

export interface CommissionPagingRequest {
  page_number: number
  page_size: number
  sort_by?: string
  sort_direction?: 'asc' | 'desc'
  filters?: CommissionReportFilters
}

export interface CommissionPagingResponse<T> {
  data: T[]
  total_count: number
  page_number: number
  page_size: number
  total_pages: number
}

// Enums for better type safety
export type CommissionStatus = 'Active' | 'Inactive' | 'Draft' | 'Archived'
export type KPIStructureType = 'Sales' | 'Revenue' | 'Units' | 'Custom'
export type UserType = 'Employee' | 'Manager' | 'Team' | 'Region'
export type CustomerType = 'Individual' | 'Corporate' | 'Distributor' | 'All'
export type ProductType = 'Category' | 'Brand' | 'SKU' | 'All'
export type PayoutStatus = 'Pending' | 'Approved' | 'Paid' | 'Cancelled'