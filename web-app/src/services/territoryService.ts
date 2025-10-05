import {
  ApiResponse,
  FilterCriteria,
  SortCriteria,
} from "@/types/common.types";

export interface Territory {
  UID: string;
  OrgUID: string;
  TerritoryCode: string;
  TerritoryName: string;
  ManagerEmpUID?: string | null;
  ClusterCode?: string | null;
  ParentUID?: string | null;
  ItemLevel?: number;
  HasChild?: boolean;
  IsImport?: boolean;
  IsLocal?: boolean;
  IsNonLicense?: number;
  Status?: string;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class TerritoryService {
  private async apiCall<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    try {
      const token =
        typeof window !== "undefined"
          ? localStorage.getItem("auth_token")
          : null;

      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers: {
          "Content-Type": "application/json",
          Authorization: token ? `Bearer ${token}` : "",
          ...options.headers,
        },
      });

      if (!response.ok) {
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorText = await response.text();
          if (errorText) {
            console.error("API Error:", errorText);
            errorMessage += ` - ${errorText}`;
          }
        } catch (e) {
          // Ignore if we can't read error text
        }
        throw new Error(errorMessage);
      }

      const data = await response.json();
      return {
        success: data.IsSuccess !== false,
        data: data.Data || data,
        message: data.Message || data.ErrorMessage || data.message,
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

  // Get all territories with pagination and filters
  async getTerritories(
    pageNumber: number = 1,
    pageSize: number = 20,
    sortCriterias: SortCriteria[] = [],
    filterCriterias: FilterCriteria[] = [],
    isCountRequired: boolean = true
  ): Promise<{ data: Territory[]; totalCount: number }> {
    const pagingRequest = {
      pageNumber,
      pageSize,
      sortCriterias,
      filterCriterias,
      isCountRequired,
    };

    const response = await this.apiCall<any>(
      "/Territory/SelectAllTerritories",
      {
        method: "POST",
        body: JSON.stringify(pagingRequest),
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch territories");
    }

    return {
      data: response.data.PagedData || [],
      totalCount: response.data.TotalCount || 0,
    };
  }

  // Get territory by UID
  async getTerritoryByUID(uid: string): Promise<Territory> {
    const response = await this.apiCall<Territory>(
      `/Territory/GetTerritoryByUID?UID=${uid}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch territory");
    }

    return response.data;
  }

  // Get territory by code
  async getTerritoryByCode(
    territoryCode: string,
    orgUID: string
  ): Promise<Territory> {
    const response = await this.apiCall<Territory>(
      `/Territory/GetTerritoryByCode?territoryCode=${territoryCode}&orgUID=${orgUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch territory");
    }

    return response.data;
  }

  // Get territories by organization
  async getTerritoriesByOrg(orgUID: string): Promise<Territory[]> {
    const response = await this.apiCall<Territory[]>(
      `/Territory/GetTerritoriesByOrg?orgUID=${orgUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(
        response.message || "Failed to fetch territories by organization"
      );
    }

    return response.data;
  }

  // Get territories by manager
  async getTerritoriesByManager(managerEmpUID: string): Promise<Territory[]> {
    const response = await this.apiCall<Territory[]>(
      `/Territory/GetTerritoriesByManager?managerEmpUID=${managerEmpUID}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(
        response.message || "Failed to fetch territories by manager"
      );
    }

    return response.data;
  }

  // Get territories by cluster
  async getTerritoriesByCluster(clusterCode: string): Promise<Territory[]> {
    const response = await this.apiCall<Territory[]>(
      `/Territory/GetTerritoriesByCluster?clusterCode=${clusterCode}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(
        response.message || "Failed to fetch territories by cluster"
      );
    }

    return response.data;
  }

  // Create territory
  async createTerritory(territory: Partial<Territory>): Promise<Territory> {
    const response = await this.apiCall<any>("/Territory/CreateTerritory", {
      method: "POST",
      body: JSON.stringify(territory),
    });

    if (!response.success) {
      throw new Error(response.message || "Failed to create territory");
    }

    return response.data;
  }

  // Update territory
  async updateTerritory(territory: Territory): Promise<Territory> {
    const response = await this.apiCall<any>("/Territory/UpdateTerritory", {
      method: "PUT",
      body: JSON.stringify(territory),
    });

    if (!response.success) {
      throw new Error(response.message || "Failed to update territory");
    }

    return response.data;
  }

  // Delete territory
  async deleteTerritory(uid: string): Promise<void> {
    const response = await this.apiCall<any>(
      `/Territory/DeleteTerritory?UID=${uid}`,
      {
        method: "DELETE",
      }
    );

    if (!response.success) {
      throw new Error(response.message || "Failed to delete territory");
    }
  }
}

export const territoryService = new TerritoryService();
