// Admin-related TypeScript interfaces and types

export interface Employee {
  uid: string
  code: string
  name: string
  loginId: string
  email?: string
  phone?: string
  status: 'Active' | 'Inactive'
  authType: string
  createdBy?: string
  createdDate?: string
  modifiedBy?: string
  modifiedDate?: string
  serverModifiedTime?: string
  actionType?: 'Add' | 'Update' | 'Delete'
}

export interface EmployeeInfo {
  uid: string
  empUID: string
  address?: string
  city?: string
  state?: string
  country?: string
  pincode?: string
  dateOfBirth?: string
  joiningDate?: string
  departmentUID?: string
  designationUID?: string
  actionType?: 'Add' | 'Update' | 'Delete'
}

export interface JobPosition {
  uid: string
  empUID: string
  userRoleUID: string
  orgUID: string
  branchUID?: string
  fromDate: string
  toDate?: string
  isActive: boolean
  locationType?: string
  locationValue?: string
  actionType?: 'Add' | 'Update' | 'Delete'
}

export interface EmpDTOModel {
  emp: Employee
  empInfo?: EmployeeInfo
  jobPosition?: JobPosition
}

export interface Role {
  UID: string
  Code: string
  RoleNameEn: string
  IsPrincipalRole: boolean
  IsDistributorRole: boolean
  IsAdmin: boolean
  IsWebUser: boolean
  IsAppUser: boolean
  WebMenuData?: string // JSON string
  MobileMenuData?: string // JSON string
  IsActive: boolean
  CreatedBy?: string
  CreatedTime?: string
  ModifiedBy?: string
  ModifiedTime?: string
}

export interface Permission {
  roleUID: string
  subSubModuleUID: string
  fullAccess: boolean
  viewAccess: boolean
  addAccess: boolean
  editAccess: boolean
  deleteAccess: boolean
  downloadAccess: boolean
  approvalAccess: boolean
  platform: 'Web' | 'Mobile'
  
  // Additional backend fields required for CRUD operations
  uid?: string | null
  id?: number
  isModified?: boolean
  showInMenu?: boolean
  createdBy?: string
  createdTime?: string
  modifiedBy?: string
  modifiedTime?: string
}

export interface PermissionMaster {
  roleUID: string
  platform: 'Web' | 'Mobile'
  isPrincipalRole: boolean
  permissions: Permission[]
}

export interface Module {
  UID: string
  ModuleNameEn: string
  Platform: 'Web' | 'Mobile'
  ShowInMenu: boolean
  IsForDistributor: boolean
  IsForPrincipal: boolean
  SerialNo?: number
  IconName?: string
}

export interface SubModule {
  UID: string
  SubModuleNameEn: string
  ModuleUid: string
  RelativePath?: string
  ShowInMenu: boolean
  SerialNo?: number
  IconName?: string
}

export interface SubSubModule {
  UID: string
  SubSubModuleNameEn: string
  SubModuleUid: string
  RelativePath: string
  ShowInMenu: boolean
  SerialNo?: number
  Description?: string
}

export interface ModuleHierarchy extends Module {
  children?: SubModuleHierarchy[]
}

export interface SubModuleHierarchy extends SubModule {
  children?: SubSubModule[]
}

// API Request/Response types
export interface PagingRequest {
  pageNumber: number
  pageSize: number
  sortCriterias?: SortCriteria[]
  filterCriterias?: FilterCriteria[]
  isCountRequired: boolean
}

export interface SortCriteria {
  fieldName: string
  sortDirection: 'ASC' | 'DESC'
}

export interface FilterCriteria {
  fieldName: string
  operator: 'EQ' | 'NE' | 'GT' | 'LT' | 'GTE' | 'LTE' | 'LIKE' | 'IN'
  value: any
}

export interface PagedResponse<T> {
  pagedData: T[]
  totalRecords: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

// Form state types
export interface EmployeeFormData {
  emp: Partial<Employee>
  empInfo?: Partial<EmployeeInfo>
  jobPosition?: Partial<JobPosition>
  password?: string
}

export interface RoleFormData extends Partial<Role> {
  selectedModules?: string[]
}

export interface PermissionFormData {
  roleUID: string
  platform: 'Web' | 'Mobile'
  permissions: Record<string, Partial<Permission>>
}

// UI State types
export interface TableState {
  loading: boolean
  data: any[]
  totalRecords: number
  currentPage: number
  pageSize: number
  sortField?: string
  sortDirection?: 'ASC' | 'DESC'
  filters: Record<string, any>
  selectedRows: string[]
}

export interface DialogState {
  open: boolean
  mode: 'create' | 'edit' | 'view'
  selectedId?: string
}

// Dashboard types
export interface AdminStats {
  totalEmployees: number
  activeEmployees: number
  totalRoles: number
  activeRoles: number
  totalPermissions: number
  recentActivities: number
}

export interface ActivityLog {
  id: string
  action: string
  entityType: 'Employee' | 'Role' | 'Permission'
  entityId: string
  entityName: string
  performedBy: string
  performedAt: string
  details?: string
}

// Search and filter types
export interface SearchOptions {
  query: string
  filters: {
    status?: string[]
    roles?: string[]
    organizations?: string[]
    platform?: string[]
  }
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// Bulk operation types
export interface BulkOperation {
  action: 'activate' | 'deactivate' | 'delete' | 'assign-role' | 'update-permission'
  targetIds: string[]
  data?: any
}

export interface BulkOperationResult {
  success: boolean
  processed: number
  failed: number
  errors?: string[]
}

// Permission matrix types
export interface PermissionMatrix {
  roleUID: string
  roleName: string
  platform: 'Web' | 'Mobile'
  modules: ModulePermissions[]
}

export interface ModulePermissions {
  module: Module
  subModules: SubModulePermissions[]
}

export interface SubModulePermissions {
  subModule: SubModule
  pages: PagePermissions[]
}

export interface PagePermissions {
  page: SubSubModule
  permissions: Permission
}

// Export/Import types
export interface ExportOptions {
  format: 'csv' | 'excel' | 'pdf'
  includeFields: string[]
  filters?: FilterCriteria[]
}

export interface ImportResult {
  success: boolean
  totalRecords: number
  imported: number
  failed: number
  errors?: string[]
}