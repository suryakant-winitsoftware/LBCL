import {
  Module,
  SubModule,
  SubSubModule,
  ModulePermission,
} from "@/types/permission.types";
import { authService } from "./auth-service";

interface ModulesMasterView {
  modules: Module[];
  subModules: SubModule[];
  subSubModules: SubSubModule[];
}

interface PermissionApiResponse {
  Data?: ModulePermission[];
  data?: ModulePermission[];
  StatusCode?: number;
  IsSuccess?: boolean;
  ErrorMessage?: string;
}

interface ModulesApiResponse {
  Data?: ModulesMasterView;
  data?: ModulesMasterView;
  StatusCode?: number;
  IsSuccess?: boolean;
  ErrorMessage?: string;
}

class ProductionPermissionService {
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
  private permissionsCache: Map<string, ModulePermission[]> = new Map();
  private modulesCache: Map<string, ModulesMasterView> = new Map();

  /**
   * Get all modules hierarchy for a platform using production endpoints
   */
  async getModulesMaster(
    platform: "Web" | "Mobile"
  ): Promise<ModulesMasterView> {
    const cacheKey = `modules_${platform}`;

    // TEMPORARILY DISABLED CACHE FOR DEBUGGING
    // Check cache first
    // if (this.modulesCache.has(cacheKey)) {
    //   return this.modulesCache.get(cacheKey)!
    // }

    try {
      const token = authService.getToken();
      if (!token) {
        // No authentication token available
        return { modules: [], subModules: [], subSubModules: [] };
      }

      // Try multiple potential endpoints for modules (ordered by best to worst)
      const endpoints = [
        `/Role/GetModulesMasterByPlatForm?Platform=${platform}`, // Best - has proper relationship fields
        `/Role/GetAllModulesMaster?Platform=${platform}`,
        `/Module/GetModulesByPlatform?platform=${platform}`,
        `/Auth/GetModules?platform=${platform}`,
      ];

      for (const endpoint of endpoints) {
        try {
          const response = await fetch(`${this.API_BASE_URL}${endpoint}`, {
            method: "GET",
            headers: {
              Authorization: `Bearer ${token}`,
              Accept: "application/json",
            },
          });

          if (response.ok) {
            const data: ModulesApiResponse = await response.json();

            // Modules API Response received

            if (data.IsSuccess || data.Data || data.data) {
              let modulesMaster: ModulesMasterView;

              if (data.Data) {
                modulesMaster = data.Data;
              } else if (data.data) {
                modulesMaster = data.data;
              } else {
                // Handle direct response
                modulesMaster = data as unknown as ModulesMasterView;
              }

              // Ensure all arrays exist and have correct structure
              modulesMaster = this.normalizeModulesData(modulesMaster);

              // Cache successful result
              this.modulesCache.set(cacheKey, modulesMaster);

              // Successfully loaded modules
              return modulesMaster;
            }
          } else {
            // Failed to load from endpoint

            // Handle authentication errors
            if (response.status === 401 || response.status === 403) {
              // Import and use the centralized auth clear function
              const { clearAuthAndRedirect } = await import(
                "./api-interceptor"
              );
              clearAuthAndRedirect("unauthorized");
              return { modules: [], subModules: [], subSubModules: [] };
            }
          }
        } catch {
          // Failed to load from endpoint
          continue;
        }
      }

      // All module endpoints failed
      return { modules: [], subModules: [], subSubModules: [] };
    } catch {
      // Error fetching modules
      return { modules: [], subModules: [], subSubModules: [] };
    }
  }

  /**
   * Get permissions for a specific role using production endpoints
   */
  async getPermissionsByRole(
    roleUID: string,
    platform: "Web" | "Mobile",
    isPrincipalRole: boolean
  ): Promise<ModulePermission[]> {
    const cacheKey = `permissions_${roleUID}_${platform}_${isPrincipalRole}`;

    // TEMPORARILY DISABLED CACHE FOR DEBUGGING
    // Check cache first
    // if (this.permissionsCache.has(cacheKey)) {
    //   return this.permissionsCache.get(cacheKey)!
    // }

    try {
      const token = authService.getToken();
      if (!token) {
        // No authentication token available
        return [];
      }

      // Try multiple potential endpoints for permissions
      const endpoints = [
        `/Role/SelectAllPermissionsByRoleUID?roleUID=${roleUID}&platform=${platform}&isPrincipalTypePermission=${isPrincipalRole}`,
        `/Permission/GetByRole?roleId=${roleUID}&platform=${platform}&isPrincipal=${isPrincipalRole}`,
        `/Auth/GetPermissions?roleUID=${roleUID}&platform=${platform}`,
        `/User/GetUserPermissions?roleUID=${roleUID}&platform=${platform}`,
      ];

      for (const endpoint of endpoints) {
        try {
          const response = await fetch(`${this.API_BASE_URL}${endpoint}`, {
            method: "GET",
            headers: {
              Authorization: `Bearer ${token}`,
              Accept: "application/json",
            },
          });

          if (response.ok) {
            const data: PermissionApiResponse = await response.json();

            if (data.IsSuccess || data.Data || data.data) {
              const rawPermissions = data.Data || data.data || [];

              const permissions = this.normalizePermissionsData(rawPermissions);

              // Cache successful result
              this.permissionsCache.set(cacheKey, permissions);

              // Successfully loaded permissions
              return permissions;
            }
          } else {
            // Failed to load permissions from endpoint

            // Handle authentication errors
            if (response.status === 401 || response.status === 403) {
              // Import and use the centralized auth clear function
              const { clearAuthAndRedirect } = await import(
                "./api-interceptor"
              );
              clearAuthAndRedirect("unauthorized");
              return [];
            }
          }
        } catch {
          // Failed to load permissions from endpoint
          continue;
        }
      }

      // All permission endpoints failed
      return [];
    } catch {
      // Error fetching permissions
      return [];
    }
  }

  /**
   * Normalize modules data to ensure consistent structure
   */
  private normalizeModulesData(data: unknown): ModulesMasterView {
    const dataObj = data as Record<string, unknown>;

    // Handle backend's PascalCase response structure (actual API returns Data.Modules, Data.SubModules, Data.SubSubModules)
    const modules = Array.isArray(dataObj.modules)
      ? dataObj.modules
      : Array.isArray(dataObj.Modules)
      ? dataObj.Modules
      : Array.isArray(data)
      ? data
      : [];

    const subModules = Array.isArray(dataObj.subModules)
      ? dataObj.subModules
      : Array.isArray(dataObj.SubModules)
      ? dataObj.SubModules
      : Array.isArray(dataObj.sub_modules)
      ? dataObj.sub_modules
      : [];

    const subSubModules = Array.isArray(dataObj.subSubModules)
      ? dataObj.subSubModules
      : Array.isArray(dataObj.SubSubModules)
      ? dataObj.SubSubModules
      : Array.isArray(dataObj.sub_sub_modules)
      ? dataObj.sub_sub_modules
      : [];

    return {
      modules: modules.map(this.normalizeModule),
      subModules: subModules.map(this.normalizeSubModule),
      subSubModules: subSubModules.map(this.normalizeSubSubModule),
    };
  }

  /**
   * Normalize module object to ensure consistent field names
   */
  private normalizeModule = (module: unknown): Module => {
    const mod = module as Record<string, unknown>;
    return {
      uid: String(mod.uid || mod.UID || mod.Uid || ""),
      moduleNameEn: String(
        mod.moduleNameEn ||
          mod.ModuleNameEn ||
          mod.module_name_en ||
          mod.name ||
          mod.Name ||
          mod.UID ||
          ""
      ),
      moduleNameOther: (mod.moduleNameOther ||
        mod.ModuleNameOther ||
        mod.module_name_other) as string | undefined,
      platform: (mod.platform || mod.Platform || "Web") as "Web" | "Mobile",
      // Use the actual showInMenu value from the database
      showInMenu: Boolean(
        mod.showInMenu !== undefined
          ? mod.showInMenu
          : mod.ShowInMenu !== undefined
          ? mod.ShowInMenu
          : mod.show_in_menu !== undefined
          ? mod.show_in_menu
          : true
      ),
      serialNo:
        Number(mod.serialNo || mod.SerialNo || mod.serial_no || mod.Id) || 0,
      relativePath: (mod.relativePath ||
        mod.RelativePath ||
        mod.relative_path) as string | undefined,
      isForDistributor: Boolean(
        mod.isForDistributor !== undefined
          ? mod.isForDistributor
          : mod.IsForDistributor !== undefined
          ? mod.IsForDistributor
          : true
      ),
      isForPrincipal: Boolean(
        mod.isForPrincipal !== undefined
          ? mod.isForPrincipal
          : mod.IsForPrincipal !== undefined
          ? mod.IsForPrincipal
          : true
      ),
    };
  };

  /**
   * Normalize submodule object to ensure consistent field names
   */
  private normalizeSubModule = (subModule: unknown): SubModule => {
    const sm = subModule as Record<string, unknown>;
    // Extract module UID from backend field or derive from compound UID
    const uid = String(sm.uid || sm.UID || sm.Uid || "");
    const backendModuleUID = String(
      sm.moduleUID ||
        sm.ModuleUID ||
        sm.module_uid ||
        sm.ModuleUid ||
        sm.moduleUid ||
        ""
    );

    // Backend provides ModuleUid field in GetModulesMasterByPlatForm endpoint
    // Fallback to extraction from compound UID if field is missing
    const moduleUID =
      backendModuleUID || (uid.includes("_") ? uid.split("_")[0] : uid);

    // SubModule normalization completed

    return {
      uid: uid,
      submoduleNameEn: String(
        sm.submoduleNameEn ||
          sm.SubModuleNameEn ||
          sm.submodule_name_en ||
          sm.name ||
          sm.Name ||
          (uid.includes("_") ? uid.split("_")[1] : uid)
      ),
      submoduleNameOther: (sm.submoduleNameOther ||
        sm.SubModuleNameOther ||
        sm.submodule_name_other) as string | undefined,
      moduleUID: moduleUID,
      relativePath: (sm.relativePath || sm.RelativePath || sm.relative_path) as
        | string
        | undefined,
      serialNo:
        Number(sm.serialNo || sm.SerialNo || sm.serial_no || sm.Id) || 0,
      // Use the actual showInMenu value from the database
      showInMenu: Boolean(
        sm.showInMenu !== undefined
          ? sm.showInMenu
          : sm.ShowInMenu !== undefined
          ? sm.ShowInMenu
          : sm.show_in_menu !== undefined
          ? sm.show_in_menu
          : true
      ),
      isForDistributor: Boolean(
        sm.isForDistributor !== undefined
          ? sm.isForDistributor
          : sm.IsForDistributor !== undefined
          ? sm.IsForDistributor
          : true
      ),
      isForPrincipal: Boolean(
        sm.isForPrincipal !== undefined
          ? sm.isForPrincipal
          : sm.IsForPrincipal !== undefined
          ? sm.IsForPrincipal
          : true
      ),
    };
  };

  /**
   * Normalize sub-submodule object to ensure consistent field names
   */
  private normalizeSubSubModule = (subSubModule: unknown): SubSubModule => {
    const ssm = subSubModule as Record<string, unknown>;
    // Extract subModule UID from compound UID (e.g., "SystemAdministration_UserManagement_MaintainUsers" -> "SystemAdministration_UserManagement")
    const uid = String(ssm.uid || ssm.UID || ssm.Uid || "");
    const parts = uid.split("_");
    // Backend provides SubModuleUid field in GetModulesMasterByPlatForm endpoint
    // Fallback to extraction from compound UID if field is missing
    const backendSubModuleUID = String(
      ssm.subModuleUID ||
        ssm.SubModuleUID ||
        ssm.sub_module_uid ||
        ssm.SubModuleUid ||
        ssm.subModuleUid ||
        ""
    );
    const subModuleUID =
      backendSubModuleUID ||
      (parts.length >= 2 ? parts.slice(0, 2).join("_") : "");

    return {
      uid: uid,
      subSubModuleNameEn: String(
        ssm.subSubModuleNameEn ||
          ssm.SubSubModuleNameEn ||
          ssm.sub_sub_module_name_en ||
          ssm.name ||
          ssm.Name ||
          (parts.length >= 3 ? parts[2] : uid)
      ),
      subSubModuleNameOther: (ssm.subSubModuleNameOther ||
        ssm.SubSubModuleNameOther ||
        ssm.sub_sub_module_name_other) as string | undefined,
      subModuleUID: subModuleUID,
      relativePath: String(
        ssm.relativePath || ssm.RelativePath || ssm.relative_path || ""
      ),
      serialNo:
        Number(ssm.serialNo || ssm.SerialNo || ssm.serial_no || ssm.Id) || 0,
      // For subSubModules, keep original showInMenu but will be filtered by permissions
      showInMenu: Boolean(
        ssm.showInMenu !== undefined
          ? ssm.showInMenu
          : ssm.ShowInMenu !== undefined
          ? ssm.ShowInMenu
          : ssm.show_in_menu !== undefined
          ? ssm.show_in_menu
          : false
      ),
      isForDistributor: Boolean(
        ssm.isForDistributor !== undefined
          ? ssm.isForDistributor
          : ssm.IsForDistributor !== undefined
          ? ssm.IsForDistributor
          : true
      ),
      isForPrincipal: Boolean(
        ssm.isForPrincipal !== undefined
          ? ssm.isForPrincipal
          : ssm.IsForPrincipal !== undefined
          ? ssm.IsForPrincipal
          : true
      ),
      checkInPermission:
        Number(ssm.checkInPermission || ssm.CheckInPermission) || 0,
    };
  };

  /**
   * Normalize permissions data to ensure consistent structure
   */
  private normalizePermissionsData(permissions: unknown[]): ModulePermission[] {
    if (!Array.isArray(permissions)) {
      return [];
    }

    return permissions.map((permission) => {
      const p = permission as Record<string, unknown>;
      return {
        id: p.id || p.Id || p.ID || "",
        uid: p.uid || p.UID || p.Uid || "",
        roleUid: p.roleUid || p.RoleUid || p.role_uid || "",
        subSubModuleUid:
          p.subSubModuleUid || p.SubSubModuleUid || p.sub_sub_module_uid || "",
        fullAccess: Boolean(p.fullAccess || p.FullAccess || p.full_access),
        viewAccess: Boolean(p.viewAccess || p.ViewAccess || p.view_access),
        addAccess: Boolean(p.addAccess || p.AddAccess || p.add_access),
        editAccess: Boolean(p.editAccess || p.EditAccess || p.edit_access),
        deleteAccess: Boolean(
          p.deleteAccess || p.DeleteAccess || p.delete_access
        ),
        downloadAccess: Boolean(
          p.downloadAccess || p.DownloadAccess || p.download_access
        ),
        approvalAccess: Boolean(
          p.approvalAccess || p.ApprovalAccess || p.approval_access
        ),
        // showInMenu doesn't exist in the permissions table, it's only on modules/submodules/pages
        platform: String(p.platform || p.Platform || "Web"),
      } as ModulePermission;
    });
  }

  /**
   * Check if user has specific permission
   */
  hasPermission(
    permissions: ModulePermission[],
    subSubModuleUid: string,
    action: "view" | "add" | "edit" | "delete" | "download" | "approval"
  ): boolean {
    const permission = permissions.find(
      (p) => p.subSubModuleUid === subSubModuleUid
    );
    if (!permission) return false;

    // Full access overrides individual permissions
    if (permission.fullAccess) return true;

    switch (action) {
      case "view":
        return permission.viewAccess;
      case "add":
        return permission.addAccess;
      case "edit":
        return permission.editAccess;
      case "delete":
        return permission.deleteAccess;
      case "download":
        return permission.downloadAccess;
      case "approval":
        return permission.approvalAccess;
      default:
        return false;
    }
  }

  /**
   * Build hierarchical menu from modules and permissions with robust error handling
   */
  buildMenuHierarchy(
    modulesMaster: ModulesMasterView,
    permissions: ModulePermission[]
  ): Module[] {
    try {
      const {
        modules = [],
        subModules = [],
        subSubModules = [],
      } = modulesMaster || {};

      if (!modules || modules.length === 0) {
        return [];
      }

      // Create permission lookup map
      const permissionMap = new Map(
        permissions.map((p) => [p.subSubModuleUid, p])
      );

      // Check which modules have permissions
      const modulePermissionCounts = new Map<string, number>();
      permissions.forEach((p) => {
        const moduleUid = p.subSubModuleUid.split("_")[0];
        modulePermissionCounts.set(
          moduleUid,
          (modulePermissionCounts.get(moduleUid) || 0) + 1
        );
      });

      // Debug: Check for UpdatedFeatures permissions specifically
      const updatedFeaturesPerms = permissions.filter((p) =>
        p.subSubModuleUid.startsWith("UpdatedFeatures")
      );
      if (updatedFeaturesPerms.length > 0) {
      }

      // Build hierarchy with proper error handling
      const menuModules: Module[] = modules
        .filter((m) => {
          const show = m && m.showInMenu;
          return show;
        })
        .sort((a, b) => (a.serialNo || 0) - (b.serialNo || 0))
        .map((module) => {
          try {
            // Processing module

            // Find sub-modules for this module
            const moduleSubModules = subModules
              .filter((sm) => {
                const include =
                  sm && sm.moduleUID === module.uid && sm.showInMenu;
                if (!include && sm?.moduleUID === module.uid) {
                }
                return include;
              })
              .sort((a, b) => (a.serialNo || 0) - (b.serialNo || 0))
              .map((subModule) => {
                try {
                  // Find sub-sub-modules for this sub-module
                  const filteredPages = subSubModules
                    .filter(
                      (ssm) =>
                        ssm &&
                        ssm.subModuleUID === subModule.uid &&
                        ssm.showInMenu &&
                        // Filter out problematic routes
                        !ssm.relativePath
                          ?.toLowerCase()
                          .includes("loadrequestemplate") &&
                        !ssm.relativePath
                          ?.toLowerCase()
                          .includes("loadtemplate") &&
                        !ssm.subSubModuleNameEn
                          ?.toLowerCase()
                          .includes("loadtemplate")
                    )
                    .sort((a, b) => (a.serialNo || 0) - (b.serialNo || 0));

                  const subModulePages = filteredPages.filter((ssm) => {
                    // Check if user has permission to view this page
                    const permission = permissionMap.get(ssm.uid);

                    if (!permission) {
                      // No permission found = no access, hide the page
                      return false;
                    }

                    // IMPORTANT: Only show pages where user has EXPLICIT permissions
                    // Check for truthy values, not just strict true
                    const hasAccess =
                      permission.fullAccess ||
                      permission.viewAccess ||
                      permission.addAccess ||
                      permission.editAccess ||
                      permission.deleteAccess ||
                      permission.downloadAccess ||
                      permission.approvalAccess;

                    // Don't check permission.showInMenu as it doesn't exist in the database
                    // Only check if user has actual access permissions

                    // Only show page if user has access
                    return hasAccess;
                  });

                  // Only include sub-module if it has accessible pages
                  return subModulePages.length > 0
                    ? {
                        ...subModule,
                        children: subModulePages,
                      }
                    : null;
                } catch {
                  // Error processing submodule
                  return null;
                }
              })
              .filter(Boolean);

            // Check if this module has any VALID permissions
            const modulePermissions = permissions.filter((p) => {
              // Check if permission belongs to this module
              if (!p.subSubModuleUid.startsWith(module.uid)) {
                return false;
              }
              // Check if permission grants any access (truthy values)
              return (
                p.fullAccess ||
                p.viewAccess ||
                p.addAccess ||
                p.editAccess ||
                p.deleteAccess ||
                p.downloadAccess ||
                p.approvalAccess
              );
            });

            // Only show module if it has accessible sub-modules with permissions
            if (moduleSubModules.length > 0) {
              return {
                ...module,
                children: moduleSubModules,
              };
            } else if (modulePermissions.length > 0 && module.relativePath) {
              // Module with direct permissions but no sub-modules (like Dashboard)
              // Only show if it has a direct route and valid permissions
              return {
                ...module,
                children: [],
                hasDirectAccess: true,
                relativePath: module.relativePath,
              };
            } else {
              // Module with no accessible content - hide it completely
              return null;
            }
          } catch {
            // Error processing module
            return null;
          }
        })
        .filter(Boolean) as Module[];

      // Built menu hierarchy successfully
      return menuModules;
    } catch {
      // Error building menu hierarchy
      return [];
    }
  }

  /**
   * Clear all caches
   */
  clearCache(): void {
    this.permissionsCache.clear();
    this.modulesCache.clear();
    // Permission caches cleared
  }

  /**
   * Clear modules cache only
   */
  clearModulesCache(): void {
    this.modulesCache.clear();
  }

  /**
   * Clear permissions cache only
   */
  clearPermissionsCache(): void {
    this.permissionsCache.clear();
  }

  /**
   * Refresh menu cache from backend
   */
  async refreshMenuCache(platform: "Web" | "Mobile" = "Web"): Promise<boolean> {
    try {
      const token = authService.getToken();
      if (!token) return false;

      // Call backend to refresh menu cache
      const response = await fetch(
        `${this.API_BASE_URL}/Role/UpdateMenuByPlatForm?platForm=${platform}`,
        {
          method: "PUT",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        }
      );

      if (response.ok) {
        // Clear local caches after backend refresh
        this.clearCache();
        return true;
      }

      return false;
    } catch {
      // Failed to refresh menu cache
      return false;
    }
  }
}

export const productionPermissionService = new ProductionPermissionService();
