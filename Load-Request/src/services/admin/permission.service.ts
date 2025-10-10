import {
  Permission,
  PermissionMaster,
  ApiResponse,
  ModuleHierarchy,
  PermissionMatrix,
  Role
} from "@/types/admin.types";
import { authService } from "@/lib/auth-service";
import {
  fixPermissionConsistency,
  fixAllPermissions
} from "@/utils/fix-permissions";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class PermissionService {
  private generateGUID(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(
      /[xy]/g,
      function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c == "x" ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      }
    );
  }
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const fullUrl = `${API_BASE_URL}${endpoint}`;
      const token = authService.getToken();

      const response = await fetch(fullUrl, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
          ...options.headers
        },
        ...options
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(
          `HTTP error! status: ${response.status} - ${errorText}`
        );
      }

      const data = await response.json();

      // Handle backend response format based on working sidebar implementation
      let responseData: any;
      let success = true;

      if (
        data.IsSuccess === false ||
        (data.StatusCode && data.StatusCode !== 200)
      ) {
        success = false;
      }

      // Extract data based on backend response structure
      if (data.Data !== undefined) {
        responseData = data.Data;
      } else if (data.data !== undefined) {
        responseData = data.data;
      } else {
        responseData = data;
      }

      return {
        success,
        data: responseData,
        message: data.message || data.Message || data.ErrorMessage
      };
    } catch (error) {
      return {
        success: false,
        message:
          error instanceof Error ? error.message : "Unknown error occurred"
      };
    }
  }

  // Permission CRUD Operations
  async getPermissionsByRole(
    roleUID: string,
    platform: string,
    isPrincipalRole: boolean
  ): Promise<Permission[]> {
    const url = `/Role/SelectAllPermissionsByRoleUID?roleUID=${roleUID}&platform=${platform}&isPrincipalTypePermission=${isPrincipalRole}`;

    const response = await this.apiCall<Permission[]>(url);

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch permissions");
    }

    // Normalize permission data to handle backend PascalCase properties and preserve all fields
    const normalizedPermissions = response.data.map((permission) => ({
      // Required fields for frontend
      roleUID:
        permission.RoleUid || permission.roleUID || permission.RoleUID || "",
      subSubModuleUID:
        permission.SubSubModuleUid ||
        permission.subSubModuleUID ||
        permission.SubSubModuleUID ||
        "",
      fullAccess:
        permission.FullAccess !== undefined
          ? permission.FullAccess
          : permission.fullAccess || false,
      viewAccess:
        permission.ViewAccess !== undefined
          ? permission.ViewAccess
          : permission.viewAccess || false,
      addAccess:
        permission.AddAccess !== undefined
          ? permission.AddAccess
          : permission.addAccess || false,
      editAccess:
        permission.EditAccess !== undefined
          ? permission.EditAccess
          : permission.editAccess || false,
      deleteAccess:
        permission.DeleteAccess !== undefined
          ? permission.DeleteAccess
          : permission.deleteAccess || false,
      downloadAccess:
        permission.DownloadAccess !== undefined
          ? permission.DownloadAccess
          : permission.downloadAccess || false,
      approvalAccess:
        permission.ApprovalAccess !== undefined
          ? permission.ApprovalAccess
          : permission.approvalAccess || false,
      platform: permission.Platform || permission.platform || platform,

      // Additional backend fields required for updates
      uid: permission.UID || permission.uid || null,
      id: permission.Id || permission.id || 0,
      isModified:
        permission.IsModified !== undefined ? permission.IsModified : true,
      showInMenu:
        permission.ShowInMenu !== undefined ? permission.ShowInMenu : false,
      createdBy: permission.CreatedBy || permission.createdBy || "ADMIN",
      createdTime:
        permission.CreatedTime ||
        permission.createdTime ||
        new Date().toISOString(),
      modifiedBy: permission.ModifiedBy || permission.modifiedBy || "ADMIN",
      modifiedTime:
        permission.ModifiedTime ||
        permission.modifiedTime ||
        new Date().toISOString()
    }));

    // Fix permission consistency - if fullAccess is true, all other flags should be true
    const fixedPermissions = normalizedPermissions.map((permission) => {
      if (permission.fullAccess) {
        return {
          ...permission,
          viewAccess: true,
          addAccess: true,
          editAccess: true,
          deleteAccess: true,
          downloadAccess: true,
          approvalAccess: true,
          showInMenu: true
        };
      }
      // If any access is granted, view access should be true
      if (
        permission.addAccess ||
        permission.editAccess ||
        permission.deleteAccess ||
        permission.downloadAccess ||
        permission.approvalAccess
      ) {
        return {
          ...permission,
          viewAccess: true
        };
      }
      return permission;
    });

    return fixedPermissions;
  }

  async getPermissionByRoleAndPage(
    roleUID: string,
    relativePath: string,
    isPrincipalRole: boolean
  ): Promise<Permission | null> {
    const response = await this.apiCall<Permission>(
      `/Role/GetPermissionByRoleAndPage?roleUID=${roleUID}&relativePath=${relativePath}&isPrincipleRole=${isPrincipalRole}`
    );

    if (!response.success) {
      return null;
    }

    return response.data || null;
  }

  async updatePermissions(
    permissionMaster: PermissionMaster
  ): Promise<boolean> {
    // Validate that all permissions have the correct roleUID
    const invalidPermissions = permissionMaster.permissions.filter(
      (p) => !p.roleUID
    );
    if (invalidPermissions.length > 0) {
      // Fix permissions without roleUID
      permissionMaster.permissions = permissionMaster.permissions.map((p) => ({
        ...p,
        roleUID: p.roleUID || permissionMaster.roleUID
      }));
    }

    // First fix permission consistency before sending to backend
    const fixedPermissions = fixAllPermissions(permissionMaster.permissions);

    // Transform permissions to backend format (PascalCase)
    const backendPermissions = fixedPermissions
      .map((permission) => {
        // Debug missing UIDs
        if (!permission.uid) {
        }

        // CRITICAL: Ensure RoleUid is ALWAYS set correctly
        // For updates, we MUST use the roleUID from permissionMaster, not from individual permissions
        // This prevents foreign key constraint errors when the permission's roleUID is incorrect
        const roleUid = permissionMaster.roleUID; // Always use the master roleUID for consistency

        if (!roleUid) {
          // Skip this permission to avoid foreign key constraint error
          return null;
        }

        // Fix if roleUID is a boolean or invalid
        if (
          typeof permission.roleUID === "boolean" ||
          !permission.roleUID ||
          typeof permission.roleUID !== "string"
        ) {
          permission.roleUID = roleUid; // Fix it to use the master roleUID
        }

        // Warn if permission has a different roleUID
        if (permission.roleUID && permission.roleUID !== roleUid) {
        }

        // CRITICAL: Validate SubSubModuleUid doesn't have role prefix
        let subSubModuleUid = permission.subSubModuleUID;

        // Ensure subSubModuleUid is a string
        if (typeof subSubModuleUid !== "string") {
          // Skip this permission if subSubModuleUid is invalid
          return null;
        }

        // If the subSubModuleUID starts with roleUID prefix, remove it
        // This handles cases where the UID might be "Admin_UpdatedFeatures_..."
        // but the sub_sub_module table only has "UpdatedFeatures_..."
        if (subSubModuleUid && subSubModuleUid.startsWith(roleUid + "_")) {
          subSubModuleUid = subSubModuleUid.substring(roleUid.length + 1);
        }

        // Additional validation: if subSubModuleUid contains "Admin_", it's wrong
        if (subSubModuleUid && subSubModuleUid.includes("Admin_")) {
          // Try to fix by removing Admin_ prefix
          subSubModuleUid = subSubModuleUid.replace(/^Admin_/, "");
        }

        return {
          UID: permission.uid || null,
          Id: permission.id || 0,
          RoleUid: roleUid, // ALWAYS use the master roleUID to avoid FK constraint errors
          SubSubModuleUid: subSubModuleUid,
          Platform: permission.platform || permissionMaster.platform,
          FullAccess: permission.fullAccess,
          ViewAccess: permission.viewAccess,
          AddAccess: permission.addAccess,
          EditAccess: permission.editAccess,
          DeleteAccess: permission.deleteAccess,
          DownloadAccess: permission.downloadAccess,
          ApprovalAccess: permission.approvalAccess,
          IsModified:
            permission.isModified !== undefined ? permission.isModified : true,
          ShowInMenu:
            permission.showInMenu !== undefined ? permission.showInMenu : false,
          CreatedBy: permission.createdBy || "ADMIN",
          CreatedTime: permission.createdTime || new Date().toISOString(),
          ModifiedBy: permission.modifiedBy || "ADMIN",
          ModifiedTime: new Date().toISOString()
        };
      })
      .filter((p) => p !== null); // Filter out any null permissions

    const backendPayload = {
      RoleUID: permissionMaster.roleUID,
      Platform: permissionMaster.platform,
      IsPrincipalRole: permissionMaster.isPrincipalRole,
      IsPrincipalPermission: permissionMaster.isPrincipalRole, // Backend expects this name
      Permissions: backendPermissions
    };

    // Validate all permissions have correct RoleUid and SubSubModuleUid
    const invalidRoleUidPermissions = backendPermissions.filter(
      (p) => !p.RoleUid || p.RoleUid !== permissionMaster.roleUID
    );
    if (invalidRoleUidPermissions.length > 0) {
    }

    // Check for permissions with Admin_ in SubSubModuleUid
    const invalidSubSubModulePermissions = backendPermissions.filter(
      (p) => p.SubSubModuleUid && p.SubSubModuleUid.includes("Admin_")
    );
    if (invalidSubSubModulePermissions.length > 0) {
    }

    const response = await this.apiCall<any>("/Role/CUDPermissionMaster", {
      method: "POST",
      body: JSON.stringify(backendPayload)
    });

    if (response.success) {
    } else {
    }

    return response.success;
  }

  // Permission Matrix Operations
  async getPermissionMatrix(
    roleUID: string,
    platform: "Web" | "Mobile"
  ): Promise<PermissionMatrix> {
    // Get role information
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const role = roleResponse.data;

    // Get permissions for this role
    const permissions = await this.getPermissionsByRole(
      roleUID,
      platform,
      role.IsPrincipalRole
    );

    // Get module hierarchy
    const moduleHierarchy = await this.getModuleHierarchy(platform);

    // Check for Updated Features module
    const updatedFeaturesModule = moduleHierarchy.find(
      (m) => m.UID === "UpdatedFeatures"
    );
    if (updatedFeaturesModule) {
    } else {
    }

    // Build permission matrix
    const matrix: PermissionMatrix = {
      roleUID,
      roleName: role.RoleNameEn,
      platform,
      modules: moduleHierarchy.map((module) => ({
        module,
        subModules: (module.children || []).map((subModule) => ({
          subModule,
          pages: (subModule.children || []).map((page) => {
            const permission = permissions.find(
              (p) => p.subSubModuleUID === page.UID
            );

            if (!permission) {
              // Permission not found for this page
            }

            return {
              page,
              permissions: permission
                ? {
                    roleUID,
                    subSubModuleUID: page.UID,
                    fullAccess: permission.fullAccess || false,
                    viewAccess: permission.viewAccess || false,
                    addAccess: permission.addAccess || false,
                    editAccess: permission.editAccess || false,
                    deleteAccess: permission.deleteAccess || false,
                    downloadAccess: permission.downloadAccess || false,
                    approvalAccess: permission.approvalAccess || false,
                    platform,
                    // Preserve backend fields for updates
                    uid: permission.uid,
                    id: permission.id,
                    isModified: false, // Only mark as modified when user changes something
                    showInMenu: permission.showInMenu,
                    createdBy: permission.createdBy,
                    createdTime: permission.createdTime,
                    modifiedBy: permission.modifiedBy,
                    modifiedTime: permission.modifiedTime
                  }
                : {
                    roleUID,
                    subSubModuleUID: page.UID,
                    fullAccess: false,
                    viewAccess: false,
                    addAccess: false,
                    editAccess: false,
                    deleteAccess: false,
                    downloadAccess: false,
                    approvalAccess: false,
                    platform,
                    // New permission defaults
                    uid: this.generateGUID(),
                    id: 0,
                    isModified: true,
                    showInMenu: false,
                    createdBy: "ADMIN",
                    createdTime: new Date().toISOString(),
                    modifiedBy: "ADMIN",
                    modifiedTime: new Date().toISOString()
                  }
            };
          })
        }))
      }))
    };

    return matrix;
  }

  async updatePermissionMatrix(matrix: PermissionMatrix): Promise<boolean> {
    // Get role information to determine isPrincipalRole
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${matrix.roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const role = roleResponse.data;

    // Extract all permissions from the matrix
    const permissions: Permission[] = [];

    matrix.modules.forEach((moduleGroup) => {
      moduleGroup.subModules.forEach((subModuleGroup) => {
        subModuleGroup.pages.forEach((pageGroup) => {
          permissions.push(pageGroup.permissions);
        });
      });
    });

    const permissionMaster: PermissionMaster = {
      roleUID: matrix.roleUID,
      platform: matrix.platform,
      isPrincipalRole: role.IsPrincipalRole, // Use actual value from role
      permissions
    };

    return await this.updatePermissions(permissionMaster);
  }

  // Bulk Permission Operations
  async bulkUpdatePermissions(
    roleUID: string,
    platform: "Web" | "Mobile",
    pageUIDs: string[],
    permissionUpdates: Partial<Permission>
  ): Promise<boolean> {
    // Get role information first
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const role = roleResponse.data;

    const currentPermissions = await this.getPermissionsByRole(
      roleUID,
      platform,
      role.IsPrincipalRole
    );

    const updatedPermissions = pageUIDs.map((pageUID) => {
      const existing = currentPermissions.find(
        (p) => p.subSubModuleUID === pageUID
      );
      return {
        roleUID,
        subSubModuleUID: pageUID,
        platform,
        fullAccess: false,
        viewAccess: false,
        addAccess: false,
        editAccess: false,
        deleteAccess: false,
        downloadAccess: false,
        approvalAccess: false,
        ...existing,
        ...permissionUpdates
      } as Permission;
    });

    const permissionMaster: PermissionMaster = {
      roleUID,
      platform,
      isPrincipalRole: role.IsPrincipalRole,
      permissions: updatedPermissions
    };

    return await this.updatePermissions(permissionMaster);
  }

  async copyPermissions(
    sourceRoleUID: string,
    targetRoleUID: string,
    platform: "Web" | "Mobile"
  ): Promise<boolean> {
    // Get source role information
    const sourceRoleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${sourceRoleUID}`
    );

    if (!sourceRoleResponse.success || !sourceRoleResponse.data) {
      throw new Error("Failed to fetch source role information");
    }

    // Get target role information
    const targetRoleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${targetRoleUID}`
    );

    if (!targetRoleResponse.success || !targetRoleResponse.data) {
      throw new Error("Failed to fetch target role information");
    }

    const targetRole = targetRoleResponse.data;

    // Get source role permissions
    const sourcePermissions = await this.getPermissionsByRole(
      sourceRoleUID,
      platform,
      sourceRoleResponse.data.IsPrincipalRole
    );

    // Create new permissions for target role
    const targetPermissions = sourcePermissions.map((permission) => ({
      ...permission,
      roleUID: targetRoleUID
    }));

    const permissionMaster: PermissionMaster = {
      roleUID: targetRoleUID,
      platform,
      isPrincipalRole: targetRole.IsPrincipalRole,
      permissions: targetPermissions
    };

    return await this.updatePermissions(permissionMaster);
  }

  async clearAllPermissions(
    roleUID: string,
    platform: "Web" | "Mobile"
  ): Promise<boolean> {
    // Get role information
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const permissionMaster: PermissionMaster = {
      roleUID,
      platform,
      isPrincipalRole: roleResponse.data.IsPrincipalRole,
      permissions: []
    };

    return await this.updatePermissions(permissionMaster);
  }

  // Permission Templates
  async createPermissionTemplate(
    templateName: string,
    roleUID: string,
    platform: "Web" | "Mobile"
  ): Promise<boolean> {
    // Get role information
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const permissions = await this.getPermissionsByRole(
      roleUID,
      platform,
      roleResponse.data.IsPrincipalRole
    );

    // Store template in localStorage or send to backend
    const template = {
      name: templateName,
      platform,
      permissions: permissions.map((p) => ({
        subSubModuleUID: p.subSubModuleUID,
        fullAccess: p.fullAccess,
        viewAccess: p.viewAccess,
        addAccess: p.addAccess,
        editAccess: p.editAccess,
        deleteAccess: p.deleteAccess,
        downloadAccess: p.downloadAccess,
        approvalAccess: p.approvalAccess
      }))
    };

    localStorage.setItem(
      `permission_template_${templateName}`,
      JSON.stringify(template)
    );
    return true;
  }

  async applyPermissionTemplate(
    templateName: string,
    roleUID: string,
    platform: "Web" | "Mobile"
  ): Promise<boolean> {
    const templateData = localStorage.getItem(
      `permission_template_${templateName}`
    );
    if (!templateData) {
      throw new Error("Template not found");
    }

    // Get role information
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const template = JSON.parse(templateData);
    const permissions = template.permissions.map((p: any) => ({
      ...p,
      roleUID,
      platform
    }));

    const permissionMaster: PermissionMaster = {
      roleUID,
      platform,
      isPrincipalRole: roleResponse.data.IsPrincipalRole,
      permissions
    };

    return await this.updatePermissions(permissionMaster);
  }

  // Permission Analysis
  async analyzeRolePermissions(roleUID: string): Promise<{
    webPermissions: number;
    mobilePermissions: number;
    totalPages: number;
    fullAccessPages: number;
    viewOnlyPages: number;
    noAccessPages: number;
  }> {
    // Get role information
    const roleResponse = await this.apiCall<Role>(
      `/Role/SelectRolesByUID?uid=${roleUID}`
    );

    if (!roleResponse.success || !roleResponse.data) {
      throw new Error("Failed to fetch role information");
    }

    const isPrincipalRole = roleResponse.data.IsPrincipalRole;

    const [webPermissions, mobilePermissions] = await Promise.all([
      this.getPermissionsByRole(roleUID, "Web", isPrincipalRole),
      this.getPermissionsByRole(roleUID, "Mobile", isPrincipalRole)
    ]);

    const webModules = await this.getModuleHierarchy("Web");
    const mobileModules = await this.getModuleHierarchy("Mobile");

    const totalWebPages = this.countTotalPages(webModules);
    const totalMobilePages = this.countTotalPages(mobileModules);
    const totalPages = totalWebPages + totalMobilePages;

    const allPermissions = [...webPermissions, ...mobilePermissions];
    const fullAccessPages = allPermissions.filter((p) => p.fullAccess).length;
    const viewOnlyPages = allPermissions.filter(
      (p) => p.viewAccess && !p.fullAccess
    ).length;
    const noAccessPages = totalPages - allPermissions.length;

    return {
      webPermissions: webPermissions.length,
      mobilePermissions: mobilePermissions.length,
      totalPages,
      fullAccessPages,
      viewOnlyPages,
      noAccessPages
    };
  }

  private countTotalPages(modules: ModuleHierarchy[]): number {
    return modules.reduce((total, module) => {
      return (
        total +
        (module.children?.reduce((subTotal, subModule) => {
          return subTotal + (subModule.children?.length || 0);
        }, 0) || 0)
      );
    }, 0);
  }

  // Helper Methods
  private async getModuleHierarchy(
    platform: string
  ): Promise<ModuleHierarchy[]> {
    // Use the same approach as the working production service
    const response = await this.apiCall<any>(
      `/Role/GetModulesMasterByPlatForm?Platform=${platform}`
    );

    if (!response.success || !response.data) {
      // Return empty hierarchy instead of throwing error to match production service
      return [];
    }

    const hierarchy = this.buildHierarchy(response.data);

    return hierarchy;
  }

  private buildHierarchy(data: any): ModuleHierarchy[] {
    // Normalize data using the same approach as production service
    const normalizedData = this.normalizeModulesData(data);
    const {
      modules = [],
      subModules = [],
      subSubModules = []
    } = normalizedData;

    if (modules.length === 0) {
      return [];
    }

    // Build hierarchy following production service pattern
    const hierarchy = modules
      .filter((module: any) => module.showInMenu)
      .sort((a: any, b: any) => (a.serialNo || 0) - (b.serialNo || 0))
      .map((module: any) => {
        const matchingSubModules = subModules.filter(
          (sub: any) => sub.moduleUID === module.uid && sub.showInMenu
        );

        const processedSubModules = matchingSubModules
          .sort((a: any, b: any) => (a.serialNo || 0) - (b.serialNo || 0))
          .map((subModule: any) => {
            const matchingPages = subSubModules.filter(
              (subSub: any) =>
                subSub.subModuleUID === subModule.uid && subSub.showInMenu
            );

            return {
              UID: subModule.uid,
              SubModuleNameEn: subModule.submoduleNameEn,
              ModuleUid: subModule.moduleUID,
              RelativePath: subModule.relativePath,
              SerialNo: subModule.serialNo,
              ShowInMenu: subModule.showInMenu,
              children: matchingPages
                .sort((a: any, b: any) => (a.serialNo || 0) - (b.serialNo || 0))
                .map((subSubModule: any) => ({
                  UID: subSubModule.uid,
                  SubSubModuleNameEn: subSubModule.subSubModuleNameEn,
                  SubModuleUid: subSubModule.subModuleUID,
                  RelativePath: subSubModule.relativePath,
                  SerialNo: subSubModule.serialNo,
                  ShowInMenu: subSubModule.showInMenu
                }))
            };
          });

        return {
          UID: module.uid,
          ModuleNameEn: module.moduleNameEn,
          Platform: module.platform,
          SerialNo: module.serialNo,
          ShowInMenu: module.showInMenu,
          IsForDistributor: module.isForDistributor,
          IsForPrincipal: module.isForPrincipal,
          children: processedSubModules
        };
      });

    return hierarchy;
  }

  // Add normalization methods from production service
  private normalizeModulesData(data: unknown): any {
    const dataObj = data as Record<string, unknown>;

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
      subSubModules: subSubModules.map(this.normalizeSubSubModule)
    };
  }

  private normalizeModule = (module: unknown): any => {
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
      platform: (mod.platform || mod.Platform || "Web") as "Web" | "Mobile",
      showInMenu: Boolean(
        mod.showInMenu !== undefined
          ? mod.showInMenu
          : mod.ShowInMenu !== undefined
          ? mod.ShowInMenu
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
      )
    };
  };

  private normalizeSubModule = (subModule: unknown): any => {
    const sm = subModule as Record<string, unknown>;
    const uid = String(sm.uid || sm.UID || sm.Uid || "");
    const backendModuleUID = String(
      sm.moduleUID ||
        sm.ModuleUID ||
        sm.module_uid ||
        sm.ModuleUid ||
        sm.moduleUid ||
        ""
    );
    const moduleUID =
      backendModuleUID || (uid.includes("_") ? uid.split("_")[0] : uid);

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
      moduleUID: moduleUID,
      relativePath: (sm.relativePath || sm.RelativePath || sm.relative_path) as
        | string
        | undefined,
      serialNo:
        Number(sm.serialNo || sm.SerialNo || sm.serial_no || sm.Id) || 0,
      showInMenu: Boolean(
        sm.showInMenu !== undefined
          ? sm.showInMenu
          : sm.ShowInMenu !== undefined
          ? sm.ShowInMenu
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
      )
    };
  };

  private normalizeSubSubModule = (subSubModule: unknown): any => {
    const ssm = subSubModule as Record<string, unknown>;
    const uid = String(ssm.uid || ssm.UID || ssm.Uid || "");
    const parts = uid.split("_");
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
      subModuleUID: subModuleUID,
      relativePath: String(
        ssm.relativePath || ssm.RelativePath || ssm.relative_path || ""
      ),
      serialNo:
        Number(ssm.serialNo || ssm.SerialNo || ssm.serial_no || ssm.Id) || 0,
      showInMenu: Boolean(
        ssm.showInMenu !== undefined
          ? ssm.showInMenu
          : ssm.ShowInMenu !== undefined
          ? ssm.ShowInMenu
          : ssm.show_in_menu !== undefined
          ? ssm.show_in_menu
          : true
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
      )
    };
  };

  // Permission Validation
  validatePermissionLogic(permission: Permission): {
    valid: boolean;
    errors: string[];
  } {
    const errors: string[] = [];

    // If fullAccess is true, all other permissions should be true
    if (permission.fullAccess) {
      if (!permission.viewAccess)
        errors.push("Full access requires view access");
      if (!permission.addAccess) errors.push("Full access requires add access");
      if (!permission.editAccess)
        errors.push("Full access requires edit access");
      if (!permission.deleteAccess)
        errors.push("Full access requires delete access");
    }

    // Edit access usually requires view access
    if (permission.editAccess && !permission.viewAccess) {
      errors.push("Edit access requires view access");
    }

    // Delete access usually requires view access
    if (permission.deleteAccess && !permission.viewAccess) {
      errors.push("Delete access requires view access");
    }

    // Approval access usually requires view access
    if (permission.approvalAccess && !permission.viewAccess) {
      errors.push("Approval access requires view access");
    }

    return {
      valid: errors.length === 0,
      errors
    };
  }

  // Export functionality
  async exportPermissionMatrix(
    roleUID: string,
    platform: "Web" | "Mobile",
    format: "csv" | "excel"
  ): Promise<Blob> {
    const matrix = await this.getPermissionMatrix(roleUID, platform);

    if (format === "csv") {
      return this.exportMatrixToCSV(matrix);
    } else {
      return this.exportMatrixToExcel(matrix);
    }
  }

  private exportMatrixToCSV(matrix: PermissionMatrix): Blob {
    const headers = [
      "Module",
      "Sub Module",
      "Page",
      "Path",
      "Full Access",
      "View",
      "Add",
      "Edit",
      "Delete",
      "Download",
      "Approval"
    ];

    const rows: string[] = [headers.join(",")];

    matrix.modules.forEach((moduleGroup) => {
      moduleGroup.subModules.forEach((subModuleGroup) => {
        subModuleGroup.pages.forEach((pageGroup) => {
          const { page, permissions } = pageGroup;
          const row = [
            moduleGroup.module.ModuleNameEn,
            subModuleGroup.subModule.SubModuleNameEn,
            page.SubSubModuleNameEn,
            page.RelativePath,
            permissions.fullAccess ? "Yes" : "No",
            permissions.viewAccess ? "Yes" : "No",
            permissions.addAccess ? "Yes" : "No",
            permissions.editAccess ? "Yes" : "No",
            permissions.deleteAccess ? "Yes" : "No",
            permissions.downloadAccess ? "Yes" : "No",
            permissions.approvalAccess ? "Yes" : "No"
          ]
            .map((field) => `"${field}"`)
            .join(",");

          rows.push(row);
        });
      });
    });

    const csvContent = rows.join("\n");
    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportMatrixToExcel(matrix: PermissionMatrix): Blob {
    // Simplified Excel export (would need a library like xlsx for full implementation)
    return this.exportMatrixToCSV(matrix);
  }
}

export const permissionService = new PermissionService();
