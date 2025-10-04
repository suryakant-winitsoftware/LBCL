import {
  Role,
  PagingRequest,
  PagedResponse,
  ApiResponse,
  Module,
  ModuleHierarchy,
} from "@/types/admin.types";
import { authService } from "@/lib/auth-service";
import { apiService } from "../api";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

// Helper function to extract paged data from various response formats
function extractPagedData(response: any): { items: any[]; totalCount: number } {
  if (!response) {
    return { items: [], totalCount: 0 };
  }

  // Format 1: Direct PagedData property
  if (response.PagedData && Array.isArray(response.PagedData)) {
    return {
      items: response.PagedData,
      totalCount: response.TotalCount || response.PagedData.length,
    };
  }

  // Format 2: Nested Data.PagedData
  if (response.Data?.PagedData && Array.isArray(response.Data.PagedData)) {
    return {
      items: response.Data.PagedData,
      totalCount: response.Data.TotalCount || response.Data.PagedData.length,
    };
  }

  // Format 3: Direct Data array
  if (response.Data && Array.isArray(response.Data)) {
    return {
      items: response.Data,
      totalCount: response.TotalCount || response.Data.length,
    };
  }

  // Default: empty array
  return { items: [], totalCount: 0 };
}

class RoleService {
  // Helper function to format module selections for backend
  formatModuleData(selectedModuleUIDs: string[]): string {
    if (!selectedModuleUIDs || selectedModuleUIDs.length === 0) {
      return "";
    }

    // Create the structure expected by backend
    const moduleData = selectedModuleUIDs.map((uid) => ({
      Module: { UID: uid },
      SubModuleHierarchies: [],
    }));

    return JSON.stringify(moduleData);
  }
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
          ...options.headers,
        },
        ...options,
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return {
        success: true,
        data: data.data || data,
        message: data.message,
      };
    } catch (error) {
      console.error(`API call failed for ${endpoint}:`, error);
      return {
        success: false,
        message:
          error instanceof Error ? error.message : "Unknown error occurred",
      };
    }
  }

  // Role CRUD Operations
  async getRoles(pagingRequest: PagingRequest): Promise<PagedResponse<Role>> {
    try {
      console.log("🔄 Fetching roles with request:", pagingRequest);

      const data = await apiService.post("Role/SelectAllRoles", pagingRequest);
      console.log("✅ Roles API response:", data);

      const extracted = extractPagedData(data);
      console.log("📊 Extracted roles data:", extracted);

      // Client-side filtering as fallback if backend doesn't filter properly
      let filteredItems = extracted.items;

      // Check if we have filters in the request
      if (
        pagingRequest.filterCriterias &&
        pagingRequest.filterCriterias.length > 0
      ) {
        console.log("Applying client-side filters as fallback...");

        pagingRequest.filterCriterias.forEach((filter: any) => {
          if (filter.Name === "RoleNameEn" && filter.Type === 6) {
            // Contains filter
            // Filter by role name (case-insensitive)
            const searchValue = (filter.Value || "").toLowerCase();
            filteredItems = filteredItems.filter((role: any) => {
              const roleName = (
                role.RoleNameEn ||
                role.roleNameEn ||
                ""
              ).toLowerCase();
              return roleName.includes(searchValue);
            });
            console.log(
              `Filtered by RoleNameEn containing "${filter.Value}": ${filteredItems.length} results`
            );
          } else if (filter.Type === 0) {
            // Equals filter
            // Filter by exact match
            filteredItems = filteredItems.filter((role: any) => {
              const fieldValue = role[filter.Name];
              return fieldValue === filter.Value;
            });
          }
        });
      }

      // Apply pagination on filtered results
      const startIndex =
        (pagingRequest.pageNumber - 1) * pagingRequest.pageSize;
      const endIndex = startIndex + pagingRequest.pageSize;
      const paginatedItems = filteredItems.slice(startIndex, endIndex);

      return {
        pagedData: paginatedItems,
        totalRecords: filteredItems.length, // Use filtered count
        totalPages: Math.ceil(filteredItems.length / pagingRequest.pageSize),
        pageNumber: pagingRequest.pageNumber,
        pageSize: pagingRequest.pageSize,
      };
    } catch (error) {
      console.error("❌ Failed to fetch roles:", error);
      throw new Error(
        error instanceof Error ? error.message : "Failed to fetch roles"
      );
    }
  }

  async getRoleById(uid: string): Promise<Role> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/Role/SelectRolesByUID?uid=${uid}`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`,
          },
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      if (!result.IsSuccess || !result.Data) {
        throw new Error(result.Message || "Failed to fetch role");
      }

      // Return the role data with proper mapping
      return result.Data;
    } catch (error) {
      console.error(`Failed to fetch role by ID:`, error);
      throw error;
    }
  }

  async createRole(role: Partial<Role>): Promise<Role> {
    try {
      // Format the role data to match backend expectations - include all backend fields
      const formattedRole = {
        UID: role.uid || role.code || `ROLE_${Date.now()}`,
        Code: role.code,
        RoleNameEn: role.roleNameEn,
        RoleNameOther: role.roleNameOther || "", // Backend field for description/alias
        IsWebUser: role.isWebUser || false,
        IsAppUser: role.isAppUser || false,
        IsPrincipalRole: role.isPrincipalRole || false,
        IsDistributorRole: role.isDistributorRole || false,
        IsAdmin: role.isAdmin || false,
        IsActive: role.isActive !== false,
        WebMenuData: role.webMenuData || "",
        MobileMenuData: role.mobileMenuData || "",
        ParentRoleUid:
          role.parentRoleUid === "none" || !role.parentRoleUid
            ? null
            : role.parentRoleUid,
        HaveWarehouse: role.haveWarehouse || false,
        HaveVehicle: role.haveVehicle || false,
        IsForReportsTo: role.isForReportsTo || false,
        CreatedBy: "ADMIN",
        CreatedTime: new Date().toISOString(),
        OrgUid: role.orgUid,
      };

      const response = await fetch(`${API_BASE_URL}/Role/CreateRole`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
        },
        body: JSON.stringify(formattedRole),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      if (!result.IsSuccess) {
        throw new Error(result.Message || "Failed to create role");
      }

      // Return the created role by fetching it
      return await this.getRoleById(formattedRole.UID);
    } catch (error) {
      console.error(`Failed to create role:`, error);
      throw error;
    }
  }

  async updateRole(role: Role): Promise<Role> {
    try {
      // Ensure all fields are included when updating
      const formattedRole = {
        ...role,
        RoleNameOther: role.roleNameOther || role.RoleNameOther || "", // Use roleNameOther from form or existing value
        ParentRoleUid:
          role.ParentRoleUid === "none" || !role.ParentRoleUid
            ? null
            : role.ParentRoleUid,
        IsForReportsTo:
          role.IsForReportsTo !== undefined
            ? role.IsForReportsTo
            : role.isForReportsTo || false,
        HaveWarehouse:
          role.HaveWarehouse !== undefined
            ? role.HaveWarehouse
            : role.haveWarehouse || false,
        HaveVehicle:
          role.HaveVehicle !== undefined
            ? role.HaveVehicle
            : role.haveVehicle || false,
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date().toISOString(),
      };

      const response = await fetch(`${API_BASE_URL}/Role/UpdateRoles`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
        },
        body: JSON.stringify(formattedRole),
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      if (!result.IsSuccess) {
        throw new Error(result.Message || "Failed to update role");
      }

      // Return the updated role (fetch it again to get the latest data)
      return await this.getRoleById(role.UID);
    } catch (error) {
      console.error(`Failed to update role:`, error);
      throw error;
    }
  }

  async deleteRole(uid: string): Promise<boolean> {
    // Soft delete by setting isActive to false
    const role = await this.getRoleById(uid);
    const updatedRole = {
      ...role,
      isActive: false,
      modifiedDate: new Date().toISOString(),
    };

    const response = await this.updateRole(updatedRole);
    return !!response;
  }

  // Role by Organization
  async getRolesByOrg(
    orgUID: string,
    isAppUser: boolean = false
  ): Promise<Role[]> {
    const response = await this.apiCall<Role[]>(
      `/Role/SelectRolesByOrgUID?orgUID=${orgUID}&IsAppUser=${isAppUser}`
    );

    if (!response.success || !response.data) {
      throw new Error(
        response.message || "Failed to fetch roles by organization"
      );
    }

    return response.data;
  }

  // Module Management
  async getAllModules(platform?: string): Promise<Module[]> {
    const endpoint = platform
      ? `/Role/GetModulesMasterByPlatForm?Platform=${platform}`
      : "/Role/GetAllModulesMaster";

    const response = await this.apiCall<Module[]>(endpoint);

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch modules");
    }

    return response.data;
  }

  async getModuleHierarchy(platform: string): Promise<ModuleHierarchy[]> {
    const modules = await this.getAllModules(platform);
    // Transform flat modules into hierarchy (would need additional API calls for sub-modules)
    return modules.map((module) => ({
      ...module,
      children: [], // This would be populated from sub-modules API
    }));
  }

  // Menu Management
  async updateMenuByPlatform(platform: string): Promise<boolean> {
    const response = await this.apiCall<any>(
      `/Role/UpdateMenuByPlatForm?platForm=${platform}`,
      {
        method: "PUT",
      }
    );

    return response.success;
  }

  // Role Validation and Utilities
  async validateRoleCode(code: string, excludeUID?: string): Promise<boolean> {
    const pagingRequest: PagingRequest = {
      pageNumber: 0,
      pageSize: 1,
      isCountRequired: true,
      filterCriterias: [
        {
          fieldName: "code",
          operator: "EQ",
          value: code,
        },
      ],
    };

    if (excludeUID) {
      pagingRequest.filterCriterias?.push({
        fieldName: "uid",
        operator: "NE",
        value: excludeUID,
      });
    }

    const result = await this.getRoles(pagingRequest);
    return result.totalRecords === 0;
  }

  // Search and Filter Helpers
  buildRolePagingRequest(
    page: number = 1,
    pageSize: number = 10,
    searchQuery?: string,
    filters?: {
      isPrincipalRole?: boolean;
      isDistributorRole?: boolean;
      isAdmin?: boolean;
      isWebUser?: boolean;
      isAppUser?: boolean;
      isActive?: boolean;
    },
    sortField?: string,
    sortDirection?: "ASC" | "DESC"
  ): any {
    // Build sort criteria array using the correct backend format
    const sortCriterias: any[] = [];
    if (sortField && sortDirection) {
      // Backend expects: Name and Direction (0 = Asc, 1 = Desc)
      sortCriterias.push({
        Name: sortField,
        Direction: sortDirection === "ASC" ? 0 : 1,
      });
    }

    // Build filter criteria array using the correct backend format
    const filterCriterias: any[] = [];

    // Only add search filter if searchQuery is not empty
    if (searchQuery && searchQuery.trim() !== "") {
      console.log("Adding search filter for:", searchQuery);
      // Backend expects: Name, Value, Type (as enum number)
      // FilterType enum: Equal = 0, NotEqual = 1, GreaterThan = 2, LessThan = 3,
      // GreaterThanOrEqual = 4, LessThanOrEqual = 5, Contains = 6, NotContains = 7,
      // StartsWith = 8, EndsWith = 9, In = 10, NotIn = 11
      // IMPORTANT: Use the aliased column name from the subquery
      filterCriterias.push({
        Name: "RoleNameEn", // Aliased column name from subquery (PascalCase)
        Value: searchQuery, // Don't add % wildcards - backend handles this for Contains type
        Type: 6, // Contains = 6 (equivalent to LIKE)
        DataType: "System.String",
      });
    }

    // Only add filters that have actual values
    if (filters) {
      // Map frontend field names to aliased column names from subquery (PascalCase)
      const fieldNameMapping: Record<string, string> = {
        isPrincipalRole: "IsPrincipalRole",
        isDistributorRole: "IsDistributorRole",
        isAdmin: "IsAdmin",
        isWebUser: "IsWebUser",
        isAppUser: "IsAppUser",
        isActive: "IsActive",
      };

      Object.entries(filters).forEach(([key, value]) => {
        // Skip undefined and null values
        // For booleans, include them (even if false)
        // For strings, skip empty ones
        if (value !== undefined && value !== null) {
          // If it's a string, check if it's not empty
          if (typeof value === "string" && value.trim() === "") {
            return; // Skip empty strings
          }
          const backendFieldName = fieldNameMapping[key] || key;
          console.log(`Adding filter: ${backendFieldName} = ${value}`);
          filterCriterias.push({
            Name: backendFieldName,
            Value: value,
            Type: 0, // Equal = 0
            DataType:
              typeof value === "boolean" ? "System.Boolean" : "System.String",
          });
        }
      });
    }

    console.log("Final filterCriterias:", filterCriterias);

    // Use the exact same format as working web-portal
    // IMPORTANT: Backend expects arrays, not null - passing null causes "Object reference not set" error
    const pagingRequest = {
      pageNumber: page || 1,
      pageSize: pageSize || 10,
      sortCriterias: sortCriterias, // Always pass array (even if empty)
      filterCriterias: filterCriterias, // Always pass array (even if empty)
      isCountRequired: true,
    };

    console.log("Building paging request:", pagingRequest);
    return pagingRequest;
  }

  // Bulk Operations
  async bulkUpdateRoleStatus(
    roleIds: string[],
    isActive: boolean
  ): Promise<{ success: number; failed: number }> {
    let success = 0;
    let failed = 0;

    for (const uid of roleIds) {
      try {
        const role = await this.getRoleById(uid);
        await this.updateRole({
          ...role,
          isActive,
          modifiedDate: new Date().toISOString(),
        });
        success++;
      } catch (error) {
        console.error(`Failed to update role ${uid}:`, error);
        failed++;
      }
    }

    return { success, failed };
  }

  async bulkUpdatePlatformAccess(
    roleIds: string[],
    isWebUser?: boolean,
    isAppUser?: boolean
  ): Promise<{ success: number; failed: number }> {
    let success = 0;
    let failed = 0;

    for (const uid of roleIds) {
      try {
        const role = await this.getRoleById(uid);
        const updates: Partial<Role> = {
          modifiedDate: new Date().toISOString(),
        };

        if (isWebUser !== undefined) updates.isWebUser = isWebUser;
        if (isAppUser !== undefined) updates.isAppUser = isAppUser;

        await this.updateRole({ ...role, ...updates });
        success++;
      } catch (error) {
        console.error(`Failed to update role ${uid}:`, error);
        failed++;
      }
    }

    return { success, failed };
  }

  // Role Templates and Cloning
  async cloneRole(
    sourceRoleUID: string,
    newRoleName: string,
    newRoleCode: string
  ): Promise<Role> {
    const sourceRole = await this.getRoleById(sourceRoleUID);

    const newRole: Partial<Role> = {
      code: newRoleCode,
      roleNameEn: newRoleName,
      isPrincipalRole: sourceRole.isPrincipalRole,
      isDistributorRole: sourceRole.isDistributorRole,
      isAdmin: false, // Never clone admin permissions
      isWebUser: sourceRole.isWebUser,
      isAppUser: sourceRole.isAppUser,
      webMenuData: sourceRole.webMenuData,
      mobileMenuData: sourceRole.mobileMenuData,
    };

    return await this.createRole(newRole);
  }

  // Analytics and Statistics
  async getRoleStats(): Promise<{
    totalRoles: number;
    activeRoles: number;
    principalRoles: number;
    distributorRoles: number;
    adminRoles: number;
    webRoles: number;
    appRoles: number;
  }> {
    try {
      const allRoles = await this.getRoles({
        pageNumber: 0,
        pageSize: 10000,
        isCountRequired: true,
      });

      const roles = allRoles?.pagedData || [];

      return {
        totalRoles: roles.length,
        activeRoles: roles.filter((r) => r.isActive).length,
        principalRoles: roles.filter((r) => r.isPrincipalRole).length,
        distributorRoles: roles.filter((r) => r.isDistributorRole).length,
        adminRoles: roles.filter((r) => r.isAdmin).length,
        webRoles: roles.filter((r) => r.isWebUser).length,
        appRoles: roles.filter((r) => r.isAppUser).length,
      };
    } catch (error) {
      console.error("Failed to get role stats:", error);
      return {
        totalRoles: 0,
        activeRoles: 0,
        principalRoles: 0,
        distributorRoles: 0,
        adminRoles: 0,
        webRoles: 0,
        appRoles: 0,
      };
    }
  }

  // Export functionality
  async exportRoles(
    format: "csv" | "excel",
    filters?: Record<string, any>
  ): Promise<Blob> {
    const pagingRequest = this.buildRolePagingRequest(
      1,
      10000,
      undefined,
      filters
    );
    const roles = await this.getRoles(pagingRequest);

    if (format === "csv") {
      return this.exportToCSV(roles.pagedData);
    } else {
      return this.exportToExcel(roles.pagedData);
    }
  }

  private exportToCSV(roles: Role[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Principal Role",
      "Distributor Role",
      "Admin",
      "Web User",
      "App User",
      "Status",
      "Created Date",
      "Created By",
      "Modified Date",
      "Modified By",
    ];

    const csvContent = [
      headers.join(","),
      ...roles.map((role) =>
        [
          role.Code || "",
          role.RoleNameEn || "",
          role.IsPrincipalRole ? "Yes" : "No",
          role.IsDistributorRole ? "Yes" : "No",
          role.IsAdmin ? "Yes" : "No",
          role.IsWebUser ? "Yes" : "No",
          role.IsAppUser ? "Yes" : "No",
          role.IsActive ? "Active" : "Inactive",
          role.CreatedTime
            ? new Date(role.CreatedTime).toLocaleDateString()
            : "",
          role.CreatedBy || "",
          role.ModifiedTime
            ? new Date(role.ModifiedTime).toLocaleDateString()
            : "",
          role.ModifiedBy || "",
        ]
          .map((field) => `"${field}"`)
          .join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportToExcel(roles: Role[]): Blob {
    // For now, return CSV format with Excel MIME type
    // In the future, could use libraries like xlsx for proper Excel export
    const csvContent = this.exportToCSV(roles);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const roleService = new RoleService();
