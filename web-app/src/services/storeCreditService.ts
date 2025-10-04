/**
 * Store Credit Service
 * Handles all Store Credit related API calls
 * Uses existing authentication from api.ts
 */

import { apiService } from "./api";
import {
  IStoreCredit,
  StoreCreditListRequest,
  PagedResponse,
  ApiResponse,
  StoreCreditLimitRequest,
  IStoreCreditLimit,
} from "@/types/storeCredit.types";

class StoreCreditService {
  private getBaseUrl(): string {
    return process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  }

  /**
   * Get all store credits with pagination and filtering
   */
  async getAllStoreCredits(
    request: StoreCreditListRequest
  ): Promise<PagedResponse<IStoreCredit>> {
    try {
      // Convert to lowercase property names for API
      const apiRequest = {
        pageNumber: request.PageNumber || request.pageNumber || 1,
        pageSize: request.PageSize || request.pageSize || 10,
        filterCriterias:
          request.FilterCriterias || request.filterCriterias || [],
        sortCriterias: request.SortCriterias || request.sortCriterias || [],
        isCountRequired:
          request.IsCountRequired ?? request.isCountRequired ?? true,
      };

      console.log(
        "📤 Store Credit API Request:",
        JSON.stringify(apiRequest, null, 2)
      );

      const response = await apiService.post(
        "/StoreCredit/SelectAllStoreCredit",
        apiRequest
      );

      console.log("📥 Store Credit Raw API Response:", response);

      if (response && typeof response === "object") {
        // Handle Data wrapper from .NET API
        if (response.Data !== undefined) {
          const data = response.Data;
          console.log("📦 Extracting from Data wrapper:", data);
          return {
            pagedData: data.PagedData || data.pagedData || [],
            PagedData: data.PagedData || data.pagedData || [],
            totalCount: data.TotalCount ?? data.totalCount ?? -1,
            TotalCount: data.TotalCount ?? data.totalCount ?? -1,
          };
        }
        // Handle PagedData from .NET API
        if (response.PagedData !== undefined) {
          console.log("📦 Direct PagedData response");
          return {
            pagedData: response.PagedData,
            PagedData: response.PagedData,
            totalCount: response.TotalCount ?? -1,
            TotalCount: response.TotalCount ?? -1,
          };
        }
        // Handle pagedData (camelCase)
        if (response.pagedData !== undefined) {
          console.log("📦 Direct pagedData response");
          return {
            pagedData: response.pagedData,
            PagedData: response.pagedData,
            totalCount: response.totalCount ?? -1,
            TotalCount: response.totalCount ?? -1,
          };
        }
      }

      console.log("⚠️ No valid response structure found, returning empty");
      return {
        pagedData: [],
        PagedData: [],
        totalCount: 0,
        TotalCount: 0,
      };
    } catch (error) {
      console.error("Error fetching store credits:", error);
      throw error;
    }
  }

  /**
   * Get store credit by UID
   */
  async getStoreCreditByUID(uid: string): Promise<IStoreCredit> {
    try {
      const response = await apiService.get(
        `/StoreCredit/SelectStoreCreditByUID?UID=${uid}`
      );

      // Handle Data wrapper from .NET API
      if (
        response &&
        typeof response === "object" &&
        response.Data !== undefined
      ) {
        return response.Data;
      }

      return response;
    } catch (error) {
      console.error("Error fetching store credit:", error);
      throw error;
    }
  }

  /**
   * Create new store credit
   */
  async createStoreCredit(storeCredit: Partial<IStoreCredit>): Promise<number> {
    try {
      const response = await apiService.post(
        "/StoreCredit/CreateStoreCredit",
        storeCredit
      );
      return response;
    } catch (error) {
      console.error("Error creating store credit:", error);
      throw error;
    }
  }

  /**
   * Update existing store credit
   */
  async updateStoreCredit(storeCredit: IStoreCredit): Promise<number> {
    try {
      const response = await apiService.put(
        "/StoreCredit/UpdateStoreCredit",
        storeCredit
      );
      return response;
    } catch (error) {
      console.error("Error updating store credit:", error);
      throw error;
    }
  }

  /**
   * Delete store credit by UID
   */
  async deleteStoreCredit(uid: string): Promise<number> {
    try {
      const response = await apiService.delete(
        `/StoreCredit/DeleteStoreCredit?UID=${uid}`
      );
      return response;
    } catch (error) {
      console.error("Error deleting store credit:", error);
      throw error;
    }
  }

  /**
   * Get current credit limit by store and division
   */
  async getCurrentLimitByStoreAndDivision(
    request: StoreCreditLimitRequest
  ): Promise<IStoreCreditLimit[]> {
    try {
      const response = await apiService.post(
        "/StoreCredit/GetCurrentLimitByStoreAndDivision",
        request
      );
      return response || [];
    } catch (error) {
      console.error("Error fetching credit limit:", error);
      throw error;
    }
  }

  /**
   * Export store credits to CSV
   */
  async exportStoreCredits(params: {
    searchText?: string;
    creditType?: string;
  }): Promise<Blob> {
    try {
      const queryParams = new URLSearchParams();
      if (params.searchText)
        queryParams.append("searchText", params.searchText);
      if (params.creditType)
        queryParams.append("creditType", params.creditType);

      const response = await apiService.get(
        `/StoreCredit/Export?${queryParams.toString()}`,
        { responseType: "blob" }
      );
      return response;
    } catch (error) {
      console.error("Error exporting store credits:", error);
      throw error;
    }
  }
}

export const storeCreditService = new StoreCreditService();
