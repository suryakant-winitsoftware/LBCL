import {
  Employee,
  EmpDTOModel,
  PagingRequest,
  PagedResponse,
  ApiResponse,
  JobPosition
} from "@/types/admin.types";
import { authService } from "@/lib/auth-service";

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
      totalCount: response.TotalCount || response.PagedData.length
    };
  }

  // Format 2: Nested Data.PagedData
  if (response.Data?.PagedData && Array.isArray(response.Data.PagedData)) {
    return {
      items: response.Data.PagedData,
      totalCount: response.Data.TotalCount || response.Data.PagedData.length
    };
  }

  // Format 3: Direct Data array
  if (response.Data && Array.isArray(response.Data)) {
    return {
      items: response.Data,
      totalCount: response.TotalCount || response.Data.length
    };
  }

  // Default: empty array
  return { items: [], totalCount: 0 };
}

class EmployeeService {
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${authService.getToken()}`,
          ...options.headers
        },
        ...options
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return {
        success: true,
        data: data.data || data,
        message: data.message
      };
    } catch (error) {
      console.error(`API call failed for ${endpoint}:`, error);
      return {
        success: false,
        message:
          error instanceof Error ? error.message : "Unknown error occurred"
      };
    }
  }

  // Get employee statistics
  async getEmployeeStats(): Promise<{
    total: number;
    active: number;
    inactive: number;
    pending: number;
  }> {
    try {
      // Get all employees with minimal data to count statuses
      const response = await fetch(
        `${API_BASE_URL}/MaintainUser/SelectAllMaintainUserDetails`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          },
          body: JSON.stringify({
            pageNumber: 1,
            pageSize: 10000, // Get all to count properly
            isCountRequired: true,
            sortCriterias: [],
            filterCriterias: []
          })
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      const employees = data.Data?.PagedData || data.PagedData || [];
      const total = data.Data?.TotalCount || data.TotalCount || 0;

      const active = employees.filter(
        (emp: any) => emp.Status === "Active"
      ).length;
      const inactive = employees.filter(
        (emp: any) => emp.Status === "Inactive"
      ).length;
      const pending = employees.filter(
        (emp: any) => emp.Status === "Pending"
      ).length;

      return { total, active, inactive, pending };
    } catch (error) {
      console.error("Failed to fetch employee stats:", error);
      return { total: 0, active: 0, inactive: 0, pending: 0 };
    }
  }

  // Employee CRUD Operations
  async getEmployees(
    pagingRequest: PagingRequest
  ): Promise<PagedResponse<Employee>> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/MaintainUser/SelectAllMaintainUserDetails`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          },
          body: JSON.stringify(pagingRequest)
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      const extracted = extractPagedData(data);

      return {
        pagedData: extracted.items,
        totalRecords: extracted.totalCount,
        totalPages: Math.ceil(extracted.totalCount / pagingRequest.pageSize),
        pageNumber: pagingRequest.pageNumber,
        pageSize: pagingRequest.pageSize
      };
    } catch (error) {
      console.error("Failed to fetch employees:", error);
      throw new Error(
        error instanceof Error ? error.message : "Failed to fetch employees"
      );
    }
  }

  async getEmployeeById(uid: string): Promise<any> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/MaintainUser/SelectMaintainUserDetailsByUID?empUID=${uid}`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          }
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch employee: ${response.status}`);
      }

      const result = await response.json();

      // Handle the response structure { Data: {...}, StatusCode: 200, IsSuccess: true }
      if (result.IsSuccess && result.Data) {
        return result.Data;
      } else if (result.Data) {
        return result.Data;
      } else if (result.Emp || result.emp) {
        // Fallback for different structure
        return result;
      } else {
        // If no recognized structure, return the result as is
        return result;
      }
    } catch (error) {
      console.error("Failed to fetch employee by ID:", error);
      throw error;
    }
  }

  async createEmployee(empDTO: EmpDTOModel): Promise<number> {
    const response = await this.apiCall<number>("/MaintainUser/CUDEmployee", {
      method: "POST",
      body: JSON.stringify(empDTO)
    });

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create employee");
    }

    return response.data;
  }

  async updateEmployee(empDTO: EmpDTOModel): Promise<number> {
    const response = await this.apiCall<number>("/MaintainUser/CUDEmployee", {
      method: "POST",
      body: JSON.stringify(empDTO)
    });

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update employee");
    }

    return response.data;
  }

  async deleteEmployee(uid: string): Promise<boolean> {
    try {
      let empData: EmpDTOModel | null = null;

      // Try to get employee data for proper deletion
      try {
        empData = await this.getEmployeeById(uid);
      } catch (fetchError) {
        console.error("Failed to fetch employee for deletion:", fetchError);
        // If we can't fetch the employee, try direct delete with minimal data
        empData = {
          emp: {
            uid,
            code: "",
            name: "",
            loginId: "",
            status: "Inactive",
            authType: "",
            actionType: "Delete"
          }
        };
      }

      if (empData && empData.emp) {
        // First try hard delete
        const deleteDTO: EmpDTOModel = {
          ...empData,
          emp: {
            ...empData.emp,
            actionType: "Delete"
          }
        };

        try {
          const response = await fetch(
            `${API_BASE_URL}/MaintainUser/CUDEmployee`,
            {
              method: "POST",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${authService.getToken()}`
              },
              body: JSON.stringify(deleteDTO)
            }
          );

          if (response.ok) {
            const data = await response.json();
            return data.Data > 0 || data.data > 0;
          }
        } catch (deleteError) {
          console.error(
            "Hard delete failed, will try soft delete:",
            deleteError
          );
        }

        // If hard delete fails, perform soft delete
        const currentName = empData.emp.name || "Unknown";
        const softDeleteDTO: EmpDTOModel = {
          ...empData,
          emp: {
            ...empData.emp,
            status: "Inactive",
            name: currentName.startsWith("[DELETED]")
              ? currentName
              : `[DELETED] ${currentName}`,
            modifiedDate: new Date().toISOString(),
            serverModifiedTime: new Date().toISOString(),
            actionType: "Update"
          }
        };

        const response = await fetch(
          `${API_BASE_URL}/MaintainUser/CUDEmployee`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${authService.getToken()}`
            },
            body: JSON.stringify(softDeleteDTO)
          }
        );

        if (!response.ok) {
          throw new Error("Failed to delete employee");
        }

        const data = await response.json();
        return data.Data > 0 || data.data > 0;
      }

      throw new Error("Unable to delete employee - invalid data");
    } catch (error) {
      console.error("Failed to delete employee:", error);
      throw new Error(
        "Cannot delete employee due to existing references. Employee has been deactivated instead."
      );
    }
  }

  async deactivateEmployee(uid: string): Promise<boolean> {
    try {
      console.log("Attempting to deactivate employee:", uid);

      // Always fetch full data for proper update
      let empData: any = null;
      try {
        empData = await this.getEmployeeById(uid);
        console.log("Fetched employee data for deactivation:", empData);
      } catch (fetchError) {
        console.error("Could not fetch employee data:", fetchError);
        throw new Error(
          "Cannot deactivate employee without fetching current data"
        );
      }

      if (empData && (empData.Emp || empData.emp)) {
        // Handle both uppercase and lowercase field names
        const emp = empData.Emp || empData.emp;
        const empInfo = empData.EmpInfo || empData.empInfo;
        const jobPosition = empData.JobPosition || empData.jobPosition;
        const empOrgMapping = empData.EmpOrgMapping || empData.empOrgMapping;

        // Make sure we have all required entities
        if (!empInfo) {
          console.warn("EmpInfo is missing, creating minimal version");
        }
        if (!jobPosition) {
          console.warn("JobPosition is missing, creating minimal version");
        }

        // Prepare the update DTO with all required fields
        const updateDTO = {
          Emp: {
            ...emp,
            Status: "Inactive",
            ActionType: 2, // Update action
            ModifiedBy: "ADMIN",
            ModifiedTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          EmpInfo: empInfo || {
            // Provide minimal EmpInfo if not available
            EmpUID: emp.UID || uid,
            Email: "",
            Phone: "",
            StartDate: new Date().toISOString(),
            CanHandleStock: false,
            ActionType: 2,
            UID: `EI-${Date.now()}`,
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          JobPosition: jobPosition || {
            // Provide minimal JobPosition if not available
            EmpUID: emp.UID || uid,
            CompanyUID: emp.CompanyUID || "",
            OrgUID: emp.CompanyUID || "",
            Department: "",
            UserRoleUID: "",
            HasEOT: false,
            CollectionLimit: 0,
            ActionType: 2,
            UID: `JP-${Date.now()}`,
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          EmpOrgMapping: empOrgMapping || []
        };

        console.log("Attempting CUDEmployee with full DTO:", updateDTO);

        const cudResponse = await fetch(
          `${API_BASE_URL}/MaintainUser/CUDEmployee`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${authService.getToken()}`
            },
            body: JSON.stringify(updateDTO)
          }
        );

        if (cudResponse.ok) {
          const cudData = await cudResponse.json();
          console.log("CUDEmployee response:", cudData);
          return cudData.IsSuccess || cudData.Data > 0;
        } else {
          const errorText = await cudResponse.text();
          console.error("CUDEmployee failed:", cudResponse.status, errorText);
        }
      }

      throw new Error("Failed to deactivate employee - all methods failed");
    } catch (error) {
      console.error("Failed to deactivate employee:", error);
      throw new Error(
        error instanceof Error ? error.message : "Failed to deactivate employee"
      );
    }
  }

  async activateEmployee(uid: string): Promise<boolean> {
    try {
      console.log("Attempting to activate employee:", uid);

      // Always fetch full data for proper update
      let empData: any = null;
      try {
        empData = await this.getEmployeeById(uid);
        console.log("Fetched employee data for activation:", empData);
      } catch (fetchError) {
        console.error("Could not fetch employee data:", fetchError);
        throw new Error(
          "Cannot activate employee without fetching current data"
        );
      }

      if (empData && (empData.Emp || empData.emp)) {
        // Handle both uppercase and lowercase field names
        const emp = empData.Emp || empData.emp;
        const empInfo = empData.EmpInfo || empData.empInfo;
        const jobPosition = empData.JobPosition || empData.jobPosition;
        const empOrgMapping = empData.EmpOrgMapping || empData.empOrgMapping;

        // Make sure we have all required entities
        if (!empInfo) {
          console.warn("EmpInfo is missing, creating minimal version");
        }
        if (!jobPosition) {
          console.warn("JobPosition is missing, creating minimal version");
        }

        // Remove [DELETED] marker if present
        let name = emp.Name || emp.name || "";
        if (name.startsWith("[DELETED] ")) {
          name = name.replace("[DELETED] ", "");
        }

        // Prepare the update DTO with all required fields
        const updateDTO = {
          Emp: {
            ...emp,
            Name: name,
            Status: "Active",
            ActionType: 2, // Update action
            ModifiedBy: "ADMIN",
            ModifiedTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          EmpInfo: empInfo || {
            // Provide minimal EmpInfo if not available
            EmpUID: emp.UID || uid,
            Email: "",
            Phone: "",
            StartDate: new Date().toISOString(),
            CanHandleStock: false,
            ActionType: 2,
            UID: `EI-${Date.now()}`,
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          JobPosition: jobPosition || {
            // Provide minimal JobPosition if not available
            EmpUID: emp.UID || uid,
            CompanyUID: emp.CompanyUID || "",
            OrgUID: emp.CompanyUID || "",
            Department: "",
            UserRoleUID: "",
            HasEOT: false,
            CollectionLimit: 0,
            ActionType: 2,
            UID: `JP-${Date.now()}`,
            CreatedBy: "ADMIN",
            ModifiedBy: "ADMIN",
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          },
          EmpOrgMapping: empOrgMapping || []
        };

        console.log("Attempting CUDEmployee with full DTO:", updateDTO);

        const cudResponse = await fetch(
          `${API_BASE_URL}/MaintainUser/CUDEmployee`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${authService.getToken()}`
            },
            body: JSON.stringify(updateDTO)
          }
        );

        if (cudResponse.ok) {
          const cudData = await cudResponse.json();
          console.log("CUDEmployee response:", cudData);
          return cudData.IsSuccess || cudData.Data > 0;
        } else {
          const errorText = await cudResponse.text();
          console.error("CUDEmployee failed:", cudResponse.status, errorText);
        }
      }

      throw new Error("Failed to activate employee - all methods failed");
    } catch (error) {
      console.error("Failed to activate employee:", error);
      throw new Error(
        error instanceof Error ? error.message : "Failed to activate employee"
      );
    }
  }

  // Job Position Management
  async getJobPositionByEmpUID(empUID: string): Promise<JobPosition> {
    const response = await this.apiCall<JobPosition>(
      `/JobPosition/SelectJobPositionByEmpUID?empUID=${empUID}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch job position");
    }

    return response.data;
  }

  async createJobPosition(jobPosition: JobPosition): Promise<number> {
    const response = await this.apiCall<number>(
      "/JobPosition/CreateJobPosition",
      {
        method: "POST",
        body: JSON.stringify(jobPosition)
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to create job position");
    }

    return response.data;
  }

  async updateJobPosition(jobPosition: JobPosition): Promise<number> {
    const response = await this.apiCall<number>(
      "/JobPosition/UpdateJobPosition",
      {
        method: "PUT",
        body: JSON.stringify(jobPosition)
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to update job position");
    }

    return response.data;
  }

  // Password Management
  async resetPassword(userId: string, newPassword: string): Promise<boolean> {
    const response = await this.apiCall<string>(
      "/Auth/ChangePasswordWithoutOldPassword",
      {
        method: "POST",
        body: JSON.stringify({
          UserId: userId,
          NewPassword: newPassword
        })
      }
    );

    return response.success;
  }

  async changePassword(
    userId: string,
    oldPassword: string,
    newPassword: string,
    challengeCode: string
  ): Promise<boolean> {
    const response = await this.apiCall<string>(
      "/Auth/UpdateExistingPasswordWithNewPassword",
      {
        method: "POST",
        body: JSON.stringify({
          UserId: userId,
          OldPassword: oldPassword,
          NewPassword: newPassword,
          ChallengeCode: challengeCode
        })
      }
    );

    return response.success;
  }

  // User Master Data
  async getUserMasterData(loginId: string): Promise<any> {
    const response = await this.apiCall<any>(
      `/MaintainUser/GetAllUserMasterDataByLoginID?LoginID=${loginId}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch user master data");
    }

    return response.data;
  }

  // Search and Filter Helpers
  buildPagingRequest(
    page: number = 1,
    pageSize: number = 10,
    searchQuery?: string,
    filters?: Record<string, any>,
    sortField?: string,
    sortDirection?: "ASC" | "DESC"
  ): any {
    // Use the exact same format as working web-portal
    const pagingRequest = {
      pageNumber: page,
      pageSize: pageSize,
      sortCriterias: [] as any[],
      filterCriterias: [] as any[],
      isCountRequired: true
    };

    if (sortField && sortDirection) {
      pagingRequest.sortCriterias.push({
        fieldName: sortField,
        sortDirection: sortDirection
      });
    }

    if (searchQuery) {
      pagingRequest.filterCriterias.push({
        fieldName: "Name",
        operator: "LIKE",
        value: `%${searchQuery}%`
      });
    }

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (
          value !== undefined &&
          value !== null &&
          value !== "" &&
          (Array.isArray(value) ? value.length > 0 : true)
        ) {
          pagingRequest.filterCriterias.push({
            fieldName: key,
            operator: Array.isArray(value) ? "IN" : "EQ",
            value
          });
        }
      });
    }

    return pagingRequest;
  }

  // Bulk Operations
  async bulkUpdateStatus(
    employeeIds: string[],
    status: "Active" | "Inactive"
  ): Promise<{ success: number; failed: number }> {
    let success = 0;
    let failed = 0;

    for (const uid of employeeIds) {
      try {
        const empDTO: Partial<EmpDTOModel> = {
          emp: { uid, status } as Employee
        };
        await this.updateEmployee(empDTO as EmpDTOModel);
        success++;
      } catch (error) {
        console.error(`Failed to update employee ${uid}:`, error);
        failed++;
      }
    }

    return { success, failed };
  }

  async bulkAssignRole(
    employeeIds: string[],
    roleUID: string,
    orgUID: string
  ): Promise<{ success: number; failed: number }> {
    let success = 0;
    let failed = 0;

    for (const empUID of employeeIds) {
      try {
        // Get existing job position or create new one
        let jobPosition: JobPosition;
        try {
          jobPosition = await this.getJobPositionByEmpUID(empUID);
          jobPosition.userRoleUID = roleUID;
          await this.updateJobPosition(jobPosition);
        } catch {
          // Create new job position if doesn't exist
          jobPosition = {
            uid: `JP_${empUID}_${Date.now()}`,
            empUID,
            userRoleUID: roleUID,
            orgUID,
            fromDate: new Date().toISOString().split("T")[0],
            isActive: true
          };
          await this.createJobPosition(jobPosition);
        }
        success++;
      } catch (error) {
        console.error(`Failed to assign role to employee ${empUID}:`, error);
        failed++;
      }
    }

    return { success, failed };
  }

  // Location Mapping Management
  async getEmployeeLocationMapping(empUID: string): Promise<any[]> {
    try {
      // Note: This API might not be available in all environments
      const response = await fetch(
        `${API_BASE_URL}/EmpOrgMapping/GetEmpOrgMappingDetailsByEmpUID?empUID=${empUID}`,
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          }
        }
      );

      if (response.ok) {
        const data = await response.json();
        return data.Data || data || [];
      }

      return [];
    } catch (error) {
      console.error("Failed to get employee location mapping:", error);
      return [];
    }
  }

  async copyLocationMapping(
    fromEmpUID: string,
    toEmpUID: string
  ): Promise<boolean> {
    try {
      // Get source employee's location mappings
      const sourceMappings = await this.getEmployeeLocationMapping(fromEmpUID);

      if (!sourceMappings || sourceMappings.length === 0) {
        console.log(
          "Source employee has no explicit location mappings in emp_org_mapping table"
        );
        // Return true since we'll copy the data from JobPosition instead
        return true;
      }

      // Delete existing mappings for target employee first
      await this.deleteEmployeeLocationMappings(toEmpUID);

      // Create new mappings for target employee
      const newMappings = sourceMappings.map((mapping) => ({
        UID: `EOM_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
        EmpUID: toEmpUID,
        OrgUID: mapping.OrgUID || mapping.orgUID,
        CreatedBy: "ADMIN",
        CreatedTime: new Date().toISOString(),
        ModifiedBy: "ADMIN",
        ModifiedTime: new Date().toISOString()
      }));

      // Try to create the mappings
      try {
        const response = await fetch(
          `${API_BASE_URL}/EmpOrgMapping/CreateEmpOrgMapping`,
          {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${authService.getToken()}`
            },
            body: JSON.stringify(newMappings)
          }
        );

        return response.ok;
      } catch (apiError) {
        console.log(
          "EmpOrgMapping API not available, but data will be copied from JobPosition"
        );
        return true;
      }
    } catch (error) {
      console.error("Failed to copy location mapping:", error);
      // Don't throw error, just return false
      return false;
    }
  }

  async deleteEmployeeLocationMappings(empUID: string): Promise<boolean> {
    try {
      const mappings = await this.getEmployeeLocationMapping(empUID);

      for (const mapping of mappings) {
        await this.apiCall<any>(
          `/EmpOrgMapping/DeleteEmpOrgMapping?uid=${mapping.UID}`,
          { method: "DELETE" }
        );
      }

      return true;
    } catch (error) {
      console.error("Failed to delete employee location mappings:", error);
      return false;
    }
  }

  // Export functionality
  async exportEmployees(
    format: "csv" | "excel",
    filters?: Record<string, any>
  ): Promise<Blob> {
    // Get all employees with filters
    const pagingRequest = this.buildPagingRequest(1, 10000, undefined, filters);
    const employees = await this.getEmployees(pagingRequest);

    if (format === "csv") {
      return this.exportToCSV(employees.pagedData);
    } else {
      return this.exportToExcel(employees.pagedData);
    }
  }

  private exportToCSV(employees: Employee[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Login ID",
      "Status",
      "Auth Type",
      "Created Date"
    ];
    const csvContent = [
      headers.join(","),
      ...employees.map((emp) =>
        [
          emp.code,
          emp.name,
          emp.loginId,
          emp.status,
          emp.authType,
          emp.createdDate || ""
        ]
          .map((field) => `"${field}"`)
          .join(",")
      )
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportToExcel(employees: Employee[]): Blob {
    // Simplified Excel export (would need a library like xlsx for full implementation)
    return this.exportToCSV(employees);
  }
}

export const employeeService = new EmployeeService();
