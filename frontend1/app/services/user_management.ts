import {
  Employee,
  EmpDTOModel,
  PagingRequest,
  PagedResponse,
  ApiResponse,
  JobPosition
} from "./admin.types";
import { authService } from "@/lib/auth-service";
import { 
  API_CONFIG, 
  AUTH_ENDPOINTS,
  USER_ENDPOINTS, 
  EMPLOYEE_ENDPOINTS, 
  JOB_POSITION_ENDPOINTS, 
  EMP_ORG_MAPPING_ENDPOINTS,
  buildApiUrl, 
  apiRequest,
  buildPagingRequest,
  extractPagedData
} from "../../lib/api-config";


class EmployeeService {
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const token = authService.getToken();
      
      const response = await fetch(buildApiUrl(endpoint), {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
          ...options.headers
        },
        ...options
      });

      if (!response.ok) {
        console.error(`API call failed for ${endpoint}: ${response.status} ${response.statusText}`);
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
      const token = authService.getToken();
      
      // Create a basic paging request to get all data
      const pagingRequest = {
        pageNumber: 1,
        pageSize: 10000, // Large page size to get all data
        sortCriterias: [],
        filterCriterias: [],
        isCountRequired: true
      };

      const response = await fetch(
        buildApiUrl(USER_ENDPOINTS.selectAll),
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify(pagingRequest) // Send proper paging request object
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
      // Add timeout to prevent hanging
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 10000); // 10 second timeout

      const token = authService.getToken();

      const response = await fetch(
        buildApiUrl(USER_ENDPOINTS.selectAll),
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`
          },
          body: JSON.stringify(pagingRequest), // Send proper paging request object
          signal: controller.signal
        }
      );

      clearTimeout(timeoutId);

      if (!response.ok) {
        console.error(`API request failed: ${response.status} ${response.statusText}`);
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
    } catch (error: any) {
      console.error("Failed to fetch employees:", error);

      if (error.name === "AbortError") {
        throw new Error(
          "Request timed out. Please check your connection and try again."
        );
      }

      throw new Error(
        error instanceof Error ? error.message : "Failed to fetch employees"
      );
    }
  }

  async getEmployeeById(uid: string): Promise<any> {
    try {
      const response = await fetch(
        buildApiUrl(`${USER_ENDPOINTS.selectByUID}?empUID=${uid}`),
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

  async createEmployee(empDTO: any): Promise<number> {
    try {
      const token = authService.getToken();
      
      // Log the payload being sent for debugging
      console.log('Creating employee with payload:', JSON.stringify(empDTO, null, 2));
      
      const response = await fetch(buildApiUrl(USER_ENDPOINTS.cudEmployee), {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify(empDTO)
      });

      if (!response.ok) {
        let errorDetails;
        try {
          errorDetails = await response.json();
          console.error('Create employee failed - JSON response:', response.status, errorDetails);
        } catch (jsonError) {
          errorDetails = await response.text();
          console.error('Create employee failed - Text response:', response.status, errorDetails);
        }
        throw new Error(`HTTP error! status: ${response.status} - ${JSON.stringify(errorDetails)}`);
      }

      const data = await response.json();
      
      // Handle different response formats
      if (data.IsSuccess || data.success) {
        return data.Data || data.data || 1;
      } else {
        throw new Error(data.Message || data.message || "Failed to create employee");
      }
    } catch (error) {
      console.error("Failed to create employee:", error);
      throw error;
    }
  }

  async updateEmployee(empDTO: EmpDTOModel): Promise<number> {
    const response = await this.apiCall<number>(USER_ENDPOINTS.cudEmployee, {
      method: "PUT",
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
        // First try hard delete by UID

        try {
          const response = await fetch(
            buildApiUrl(`${USER_ENDPOINTS.cudEmployee}/${uid}`),
            {
              method: "DELETE",
              headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${authService.getToken()}`
              }
            }
          );

          if (response.ok) {
            const data = await response.json();
            return data.Data > 0 || data.data > 0 || data.IsSuccess;
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
          buildApiUrl(USER_ENDPOINTS.cudEmployee),
          {
            method: "PUT",
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
          buildApiUrl(USER_ENDPOINTS.cudEmployee),
          {
            method: "PUT",
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
          buildApiUrl(USER_ENDPOINTS.cudEmployee),
          {
            method: "PUT",
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
      `${JOB_POSITION_ENDPOINTS.selectByEmp}?empUID=${empUID}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch job position");
    }

    return response.data;
  }

  async createJobPosition(jobPosition: JobPosition): Promise<number> {
    const response = await this.apiCall<number>(
      JOB_POSITION_ENDPOINTS.create,
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
      JOB_POSITION_ENDPOINTS.update,
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
  async resetPassword(empUID: string, newPassword: string): Promise<boolean> {
    try {
      // The backend will encrypt the password using RSA encryption
      const response = await this.apiCall<string>(AUTH_ENDPOINTS.updateNewPassword, {
        method: "POST",
        body: JSON.stringify({
          EmpUID: empUID,
          NewPassword: newPassword,
          // These fields are part of ChangePassword model but not needed for UpdateNewPassword
          UserId: null,
          OldPassword: null,
          ChallengeCode: null
        })
      });

      return response.success;
    } catch (error) {
      console.error("Reset password error:", error);
      // Try with alternate format if the first fails
      try {
        const alternateResponse = await this.apiCall<string>(
          AUTH_ENDPOINTS.updateNewPassword,
          {
            method: "POST",
            body: JSON.stringify({
              empUID: empUID,
              newPassword: newPassword
            })
          }
        );
        return alternateResponse.success;
      } catch (altError) {
        console.error("Alternate reset password error:", altError);
        return false;
      }
    }
  }

  async changePassword(
    userId: string,
    oldPassword: string,
    newPassword: string,
    challengeCode: string
  ): Promise<boolean> {
    const response = await this.apiCall<string>(
      AUTH_ENDPOINTS.updateExistingPassword,
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
      `${USER_ENDPOINTS.getMasterData}?LoginID=${loginId}`
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch user master data");
    }

    return response.data;
  }

  // Get employees that can be report-to based on role - CORRECT API FOUND!
  async getReportsToEmployeesByRoleUID(roleUID: string): Promise<any[]> {
    try {
      const response = await fetch(
        buildApiUrl(`${EMPLOYEE_ENDPOINTS.getReportsTo}?roleUID=${roleUID}`),
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      // Handle the response structure
      if (result.IsSuccess && result.Data) {
        return Array.isArray(result.Data) ? result.Data : [result.Data];
      } else if (result.Data) {
        return Array.isArray(result.Data) ? result.Data : [result.Data];
      } else {
        return [];
      }
    } catch (error) {
      console.error("Failed to fetch report-to employees by role:", error);
      return [];
    }
  }

  // Get employees as selection items by role (alternative format)
  async getEmployeesSelectionItemByRoleUID(
    orgUID: string,
    roleUID: string
  ): Promise<any[]> {
    try {
      const response = await fetch(
        buildApiUrl(`${EMPLOYEE_ENDPOINTS.getSelectionItems}?orgUID=${orgUID}&roleUID=${roleUID}`),
        {
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          }
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const result = await response.json();

      // Handle the response structure
      if (result.IsSuccess && result.Data) {
        return Array.isArray(result.Data) ? result.Data : [result.Data];
      } else if (result.Data) {
        return Array.isArray(result.Data) ? result.Data : [result.Data];
      } else {
        return [];
      }
    } catch (error) {
      console.error(
        "Failed to fetch employees selection items by role:",
        error
      );
      return [];
    }
  }

  // Search and Filter Helpers - using centralized implementation

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

  async updateJobPositionLocation(
    jobPositionUID: string,
    locationType: string,
    locationValue: string
  ): Promise<boolean> {
    try {
      const response = await fetch(
        buildApiUrl(JOB_POSITION_ENDPOINTS.updateLocation),
        {
          method: "PUT",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${authService.getToken()}`
          },
          body: JSON.stringify({
            UID: jobPositionUID,
            LocationType: locationType,
            LocationValue: locationValue
          })
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data = await response.json();
      return data.IsSuccess || data.Data > 0;
    } catch (error) {
      console.error("Failed to update job position location:", error);
      return false;
    }
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
        buildApiUrl(`${EMP_ORG_MAPPING_ENDPOINTS.getByEmp}?empUID=${empUID}`),
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
        UID: `EOM_${Date.now()}_${Math.random().toString(36).substring(2, 11)}`,
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
          buildApiUrl(EMP_ORG_MAPPING_ENDPOINTS.create),
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
          `${EMP_ORG_MAPPING_ENDPOINTS.delete}?uid=${mapping.UID}`,
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
    const pagingRequest = buildPagingRequest(1, 10000, undefined, filters);
    const employees = await this.getEmployees(pagingRequest);

    if (format === "csv") {
      return this.exportToCSV(employees.pagedData);
    } else {
      return this.exportToExcel(employees.pagedData);
    }
  }

  private exportToCSV(employees: any[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Login ID",
      "Status",
      "Auth Type",
      "Email",
      "Created Date",
      "Created By",
      "Modified Date",
      "Modified By"
    ];

    const csvContent = [
      headers.join(","),
      ...employees.map((emp) =>
        [
          emp.Code || emp.code || "",
          emp.Name || emp.name || "",
          emp.LoginId || emp.loginId || "",
          emp.Status || emp.status || "",
          emp.AuthType || emp.authType || "",
          emp.Email || emp.email || "",
          emp.CreatedDate || emp.createdDate
            ? new Date(emp.CreatedDate || emp.createdDate).toLocaleDateString()
            : "",
          emp.CreatedBy || emp.createdBy || "",
          emp.ModifiedDate || emp.modifiedDate
            ? new Date(
                emp.ModifiedDate || emp.modifiedDate
              ).toLocaleDateString()
            : "",
          emp.ModifiedBy || emp.modifiedBy || ""
        ]
          .map((field) => `"${field}"`)
          .join(",")
      )
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportToExcel(employees: any[]): Blob {
    // For now, return CSV format with Excel MIME type
    // In the future, could use libraries like xlsx for proper Excel export
    const csvContent = this.exportToCSV(employees);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;"
    });
  }
}

export const employeeService = new EmployeeService();
