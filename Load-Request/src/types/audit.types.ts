/**
 * Audit Trail Types
 * Production-ready type definitions for audit trail functionality
 * Compatible with AuditTrailAPI MongoDB backend
 */

export interface ChangeLog {
  field: string
  oldValue: any
  newValue: any
}

export interface AuditTrailEntry {
  id?: number // PostgreSQL auto-increment ID
  uid?: string // Unique identifier (GUID)
  linkedItemType: string // E.g., "Employee", "Role", "Store"
  linkedItemUID: string // UID of the item being audited
  commandType: 'Insert' | 'Update' | 'Delete' | 'View' | 'Export' | 'Login' | 'Logout'
  commandDate: Date
  docNo?: string // Document number if applicable
  jobPositionUID?: string
  empUID: string
  empName: string
  newData: Record<string, any> // Current state as JSON
  originalDataId?: string // Reference to previous audit entry UID
  hasChanges: boolean
  changeData?: ChangeLog[] // List of changes (for updates)
  
  // Standard WINIT fields
  createdBy?: string
  createdTime?: Date
  modifiedBy?: string
  modifiedTime?: Date
  serverAddTime?: Date
  serverModifiedTime?: Date
  companyUID?: string
}

export interface AuditTrailFilter {
  linkedItemType?: string
  linkedItemUID?: string
  commandType?: string
  empUID?: string
  startDate?: Date
  endDate?: Date
  searchText?: string
}

export interface AuditTrailPagingRequest {
  pageNumber: number
  pageSize: number
  sortCriterias?: Array<{
    field: string
    direction: 'asc' | 'desc'
  }>
  filterCriterias?: Array<{
    field: string
    operator: 'eq' | 'ne' | 'gt' | 'lt' | 'gte' | 'lte' | 'contains' | 'startsWith' | 'endsWith'
    value: any
  }>
  isCountRequired?: boolean
}

export interface AuditTrailPagingResponse {
  data: AuditTrailEntry[]
  totalCount?: number
  pageNumber: number
  pageSize: number
  totalPages?: number
}

// Standard WINIT API Response structure
export interface AuditTrailApiResponse<T = any> {
  Data?: T
  StatusCode?: number
  IsSuccess?: boolean
  ErrorMessage?: string
  Message?: string
}

// Audit context for automatic tracking
export interface AuditContext {
  linkedItemType: string
  linkedItemUID: string
  docNo?: string
  metadata?: Record<string, any>
}

// Audit hook options
export interface UseAuditOptions {
  enabled?: boolean
  autoTrack?: boolean
  context?: AuditContext
}

// Audit log levels for different operations
export enum AuditLogLevel {
  INFO = 'INFO',
  WARNING = 'WARNING',
  ERROR = 'ERROR',
  CRITICAL = 'CRITICAL'
}

// Security-related audit events
export interface SecurityAuditEvent {
  eventType: 'LOGIN' | 'LOGOUT' | 'FAILED_LOGIN' | 'PASSWORD_CHANGE' | 'PERMISSION_CHANGE' | 'ROLE_CHANGE'
  ipAddress?: string
  userAgent?: string
  sessionId?: string
  details?: Record<string, any>
}