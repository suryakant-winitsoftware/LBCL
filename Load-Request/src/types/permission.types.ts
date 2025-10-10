import { UserRole } from './auth.types';

// Backend-aligned permission structure
export interface ModulePermission {
  id?: string;
  uid: string;
  roleUid: string;
  subSubModuleUid: string;
  platform: 'Web' | 'Mobile';
  fullAccess: boolean;
  addAccess: boolean;
  editAccess: boolean;
  viewAccess: boolean;
  deleteAccess: boolean;
  downloadAccess: boolean;
  approvalAccess: boolean;
  showInMenu: boolean;
  createdBy?: string;
  modifiedBy?: string;
  createdTime?: Date;
  modifiedTime?: Date;
}

export interface Module {
  uid: string;
  moduleNameEn: string;
  moduleNameOther?: string;
  serialNo: number;
  platform: 'Web' | 'Mobile';
  showInMenu: boolean;
  isForDistributor: boolean;
  isForPrincipal: boolean;
  icon?: string;
  relativePath?: string;
  children?: SubModule[];
}

export interface SubModule {
  uid: string;
  submoduleNameEn: string;
  submoduleNameOther?: string;
  moduleUID: string;
  serialNo: number;
  showInMenu: boolean;
  isForDistributor: boolean;
  isForPrincipal: boolean;
  relativePath?: string;
  children?: SubSubModule[];
}

export interface SubSubModule {
  uid: string;
  subSubModuleNameEn: string;
  subSubModuleNameOther?: string;
  subModuleUID: string;
  serialNo: number;
  showInMenu: boolean;
  isForDistributor: boolean;
  isForPrincipal: boolean;
  relativePath: string;
  checkInPermission: number;
}

export interface MenuItem {
  key: string;
  label: string;
  icon?: React.ComponentType<React.SVGProps<SVGSVGElement>>;
  path: string;
  moduleUID: string;
  requiredPermission: keyof ModulePermission;
  children?: MenuItem[];
  badge?: number;
  visible?: boolean;
  disabled?: boolean;
}

export interface PermissionCheck {
  moduleUID: string;
  permission: keyof ModulePermission;
}

export interface BulkPermissionResponse {
  [key: string]: boolean;
}

export interface PermissionContext {
  permissions: ModulePermission[];
  modules: Module[];
  userRoles: UserRole[];
  currentRole: UserRole | null;
  hasPermission: (moduleUID: string, permission: keyof ModulePermission) => boolean;
  hasPermissionAsync: (moduleUID: string, permission: keyof ModulePermission) => Promise<boolean>;
  bulkCheckPermissions: (checks: PermissionCheck[]) => Promise<BulkPermissionResponse>;
  refreshPermissions: () => Promise<void>;
  switchRole: (roleUID: string) => Promise<void>;
  loading: boolean;
  error: string | null;
}

export interface PermissionWrapperProps {
  children: React.ReactNode;
  moduleUID: string;
  permission: keyof ModulePermission;
  fallback?: React.ReactNode;
  showFallback?: boolean;
  loading?: React.ReactNode;
}

export interface ProtectedRouteProps {
  children: React.ReactNode;
  requiredModule?: string;
  requiredPermission?: keyof ModulePermission;
  fallback?: React.ComponentType;
  redirectTo?: string;
}

export interface RoleHierarchy {
  roleUID: string;
  parentRoleUID?: string;
  level: number;
  children: RoleHierarchy[];
}

export interface AccessLog {
  id: string;
  userId: string;
  moduleUID: string;
  action: string;
  permission: string;
  granted: boolean;
  timestamp: Date;
  ipAddress: string;
  userAgent: string;
  metadata?: Record<string, unknown>;
}