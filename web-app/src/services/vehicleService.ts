import {
  ApiResponse,
  FilterCriteria,
  SortCriteria,
} from "@/types/common.types";

export interface Vehicle {
  UID: string
  CompanyUID?: string | null
  OrgUID: string
  VehicleNo: string
  RegistrationNo: string
  Model: string
  Type: string
  IsActive: boolean
  TruckSIDate: string
  RoadTaxExpiryDate: string
  InspectionDate: string
  WeightLimit?: number | null
  Capacity?: number | null
  LoadingCapacity?: number | null
  WarehouseCode?: string | null
  LocationCode?: string | null
  TerritoryUID?: string | null
  CreatedBy?: string
  CreatedTime?: string
  ModifiedBy?: string
  ModifiedTime?: string
  ServerAddTime?: string
  ServerModifiedTime?: string
}

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

class VehicleService {
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

  // Get all vehicles with pagination and filters
  async getVehicles(
    pageNumber: number = 1,
    pageSize: number = 10,
    orgUID: string = 'LBCL_ORG_UID',
    filterCriterias: FilterCriteria[] = [],
    sortCriterias: SortCriteria[] = []
  ): Promise<{ data: Vehicle[]; totalCount: number }> {
    const pagingRequest = {
      pageNumber,
      pageSize,
      sortCriterias,
      filterCriterias,
      isCountRequired: true,
    };

    const response = await this.apiCall<any>(
      `/Vehicle/SelectAllVehicleDetails?OrgUID=${orgUID}`,
      {
        method: "POST",
        body: JSON.stringify(pagingRequest),
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch vehicles");
    }

    return {
      data: response.data.PagedData || [],
      totalCount: response.data.TotalCount || 0,
    };
  }

  // Get vehicle by UID
  async getVehicleByUID(uid: string): Promise<Vehicle> {
    const response = await this.apiCall<Vehicle>(
      `/Vehicle/GetVehicleByUID?UID=${uid}`,
      {
        method: "GET",
      }
    );

    if (!response.success || !response.data) {
      throw new Error(response.message || "Failed to fetch vehicle");
    }

    return response.data;
  }

  // Create vehicle
  async createVehicle(vehicle: Partial<Vehicle>): Promise<number> {
    const response = await this.apiCall<any>("/Vehicle/CreateVehicle", {
      method: "POST",
      body: JSON.stringify(vehicle),
    });

    if (!response.success) {
      throw new Error(response.message || "Failed to create vehicle");
    }

    return response.data;
  }

  // Update vehicle
  async updateVehicle(vehicle: Vehicle): Promise<number> {
    const response = await this.apiCall<any>("/Vehicle/UpdateVehicleDetails", {
      method: "PUT",
      body: JSON.stringify(vehicle),
    });

    if (!response.success) {
      throw new Error(response.message || "Failed to update vehicle");
    }

    return response.data;
  }

  // Delete vehicle
  async deleteVehicle(uid: string): Promise<number> {
    const response = await this.apiCall<any>(
      `/Vehicle/DeleteVehicleDetails?UID=${uid}`,
      {
        method: "DELETE",
      }
    );

    if (!response.success) {
      throw new Error(response.message || "Failed to delete vehicle");
    }

    return response.data;
  }
}

export const vehicleService = new VehicleService()
