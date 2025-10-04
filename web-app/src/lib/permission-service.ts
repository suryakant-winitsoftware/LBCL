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
}

interface ModulesApiResponse {
  Data?: ModulesMasterView;
  data?: ModulesMasterView;
  StatusCode?: number;
  IsSuccess?: boolean;
}

class PermissionService {
  private readonly API_BASE_URL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  private permissionsCache: Map<string, ModulePermission[]> = new Map();
  private modulesCache: Map<string, ModulesMasterView> = new Map();

  /**
   * Get all modules hierarchy for a platform
   */
  async getModulesMaster(
    platform: "Web" | "Mobile"
  ): Promise<ModulesMasterView> {
    const cacheKey = `modules_${platform}`;

    // Check cache first
    if (this.modulesCache.has(cacheKey)) {
      return this.modulesCache.get(cacheKey)!;
    }

    try {
      const token = authService.getToken();
      if (!token) throw new Error("No authentication token");

      const response = await fetch(
        `${this.API_BASE_URL}/Role/GetModulesMasterByPlatForm?Platform=${platform}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch modules: ${response.statusText}`);
      }

      const data: ModulesApiResponse = await response.json();

      // Handle the response structure properly
      let modulesMaster: ModulesMasterView;

      if (data.Data) {
        modulesMaster = data.Data;
      } else if (data.data) {
        modulesMaster = data.data;
      } else if (
        "modules" in data ||
        "subModules" in data ||
        "subSubModules" in data
      ) {
        // Direct response without wrapper
        modulesMaster = data as unknown as ModulesMasterView;
      } else {
        // Fallback
        modulesMaster = { modules: [], subModules: [], subSubModules: [] };
      }

      // Ensure all arrays exist
      modulesMaster = {
        modules: modulesMaster.modules || [],
        subModules: modulesMaster.subModules || [],
        subSubModules: modulesMaster.subSubModules || [],
      };

      // Cache the result
      this.modulesCache.set(cacheKey, modulesMaster);

      return modulesMaster;
    } catch (error) {
      console.error("Error fetching modules:", error);
      return { modules: [], subModules: [], subSubModules: [] };
    }
  }

  /**
   * Get permissions for a specific role
   */
  async getPermissionsByRole(
    roleUID: string,
    platform: "Web" | "Mobile",
    isPrincipalRole: boolean
  ): Promise<ModulePermission[]> {
    const cacheKey = `permissions_${roleUID}_${platform}_${isPrincipalRole}`;

    // Check cache first
    if (this.permissionsCache.has(cacheKey)) {
      return this.permissionsCache.get(cacheKey)!;
    }

    try {
      const token = authService.getToken();
      if (!token) throw new Error("No authentication token");

      const response = await fetch(
        `${this.API_BASE_URL}/Role/SelectAllPermissionsByRoleUID?` +
          `roleUID=${roleUID}&platform=${platform}&isPrincipalTypePermission=${isPrincipalRole}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch permissions: ${response.statusText}`);
      }

      const data: PermissionApiResponse = await response.json();
      const permissions = data.Data || data.data || [];

      // Cache the result
      this.permissionsCache.set(cacheKey, permissions);

      return permissions;
    } catch (error) {
      console.error("Error fetching permissions:", error);
      return [];
    }
  }

  /**
   * Get permission for specific role and page
   */
  async getPermissionByRoleAndPage(
    roleUID: string,
    relativePath: string,
    isPrincipalRole: boolean
  ): Promise<ModulePermission | null> {
    try {
      const token = authService.getToken();
      if (!token) throw new Error("No authentication token");

      const response = await fetch(
        `${this.API_BASE_URL}/Role/GetPermissionByRoleAndPage?` +
          `roleUID=${roleUID}&relativePath=${relativePath}&isPrincipleRole=${isPrincipalRole}`,
        {
          method: "GET",
          headers: {
            Authorization: `Bearer ${token}`,
            Accept: "application/json",
          },
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch permission: ${response.statusText}`);
      }

      const data = await response.json();
      return data.Data || data.data || null;
    } catch (error) {
      console.error("Error fetching permission by page:", error);
      return null;
    }
  }

  /**
   * Save or update permissions
   */
  async savePermissions(permissions: ModulePermission[]): Promise<boolean> {
    try {
      const token = authService.getToken();
      if (!token) throw new Error("No authentication token");

      const response = await fetch(
        `${this.API_BASE_URL}/Role/CUDPermissionMaster`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${token}`,
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify(permissions),
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to save permissions: ${response.statusText}`);
      }

      // Clear cache after update
      this.clearCache();

      return true;
    } catch (error) {
      console.error("Error saving permissions:", error);
      return false;
    }
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
   * Build hierarchical menu from modules and permissions
   */
  buildMenuHierarchy(
    modulesMaster: ModulesMasterView,
    permissions: ModulePermission[]
  ): Module[] {
    const {
      modules = [],
      subModules = [],
      subSubModules = [],
    } = modulesMaster || {};

    // Return empty array if no modules
    if (!modules || modules.length === 0) {
      return [];
    }

    // Create a map for quick permission lookup
    const permissionMap = new Map(
      permissions.map((p) => [p.subSubModuleUid, p])
    );

    // Build hierarchy
    const menuModules: Module[] = modules
      .filter((m) => m.showInMenu)
      .sort((a, b) => a.serialNo - b.serialNo)
      .map((module) => {
        // Find sub-modules for this module
        const moduleSubModules = subModules
          .filter((sm) => sm.moduleUID === module.uid && sm.showInMenu)
          .sort((a, b) => a.serialNo - b.serialNo)
          .map((subModule) => {
            // Find sub-sub-modules for this sub-module
            const subModulePages = subSubModules
              .filter(
                (ssm) => ssm.subModuleUID === subModule.uid && ssm.showInMenu
              )
              .sort((a, b) => a.serialNo - b.serialNo)
              .filter((ssm) => {
                // Check if user has permission to view this page
                const permission = permissionMap.get(ssm.uid);
                return (
                  permission &&
                  (permission.fullAccess || permission.viewAccess) &&
                  permission.showInMenu
                );
              });

            // Only include sub-module if it has accessible pages
            return subModulePages.length > 0
              ? {
                  ...subModule,
                  children: subModulePages,
                }
              : null;
          })
          .filter(Boolean);

        // Only include module if it has accessible sub-modules
        return moduleSubModules.length > 0
          ? {
              ...module,
              children: moduleSubModules,
            }
          : null;
      })
      .filter(Boolean) as Module[];

    return menuModules;
  }

  /**
   * Clear all caches
   */
  clearCache(): void {
    this.permissionsCache.clear();
    this.modulesCache.clear();
  }
}

export const permissionService = new PermissionService();
