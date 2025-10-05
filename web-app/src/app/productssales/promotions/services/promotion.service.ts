// Using native fetch API instead of axios
import {
  IPromoMasterView,
  IPromoOrder,
  IPromoOffer,
  IPromoOrderItemView,
  IPromoOfferItemView,
  IItemPromotionMap,
  ISchemeBranch,
  ISchemeOrg,
  ISchemeBroadClassification,
  IApiResponse
} from "../types/promotion.types";
import {
  transformApiResponse,
  transformPagedResponse
} from "../utils/apiResponseTransformer";

// Define ActionType since it's not exported from types
type ActionType = "Add" | "Update" | "Delete";

// Legacy types for backward compatibility
export interface Promotion {
  uid: string;
  promotionCode: string;
  promotionName: string;
  promotionType: string;
  discountType: string;
  discountValue: number;
  minAmount: number;
  maxDiscountAmount: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export interface PromotionResponse {
  success: boolean;
  data: Promotion[];
}

export interface OrderItem {
  skuUID: string;
  quantity: number;
  unitPrice: number;
}

export interface CheckPromotionRequest {
  storeUID: string;
  orderDate: string;
  items: OrderItem[];
  orderTotal: number;
}

export interface ApplicablePromotion {
  promotionUID: string;
  promotionCode: string;
  discountAmount: number;
  message: string;
}

export interface ApplicablePromotionResponse {
  success: boolean;
  data: {
    applicablePromotions: ApplicablePromotion[];
    totalDiscount: number;
  };
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias: any[];
  filterCriterias: any[];
  isCountRequired: boolean;
}

// API Configuration
const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
// Pagination configuration
const PAGINATION_CONFIG = {
  PAGE_SIZES: {
    PROMOTIONS: 20,
    PRODUCTS: 50,
    STORES: 25,
    CUSTOMERS: 30,
    EMPLOYEES: 25,
    ORGANIZATIONS: 25,
    ROLES: 20,
    BRANCHES: 25,
    SKU_GROUPS: 30,
    SKU_GROUP_TYPES: 20
  }
};

// Helper function to create paging request
const createPagingRequest = (
  pageNumber: number = 1,
  pageSize?: number,
  entityType: keyof typeof PAGINATION_CONFIG.PAGE_SIZES = "PROMOTIONS"
): PagingRequest => {
  return {
    pageNumber,
    pageSize: pageSize || PAGINATION_CONFIG.PAGE_SIZES[entityType],
    sortCriterias: [],
    filterCriterias: [],
    isCountRequired: true
  };
};

// Utility function to generate GUID (UUID v4)
const generateGUID = (): string => {
  if (typeof crypto !== "undefined" && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  // Fallback for browsers that don't support crypto.randomUUID()
  return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
    const r = (Math.random() * 16) | 0;
    const v = c === "x" ? r : (r & 0x3) | 0x8;
    return v.toString(16);
  });
};

class PromotionService {
  private cache: Map<string, { data: any; timestamp: number }> = new Map();
  private cacheTimeout = 5 * 60 * 1000; // 5 minutes cache

  private createPagingRequest(
    pageNumber?: number,
    pageSize?: number,
    entityType?: keyof typeof PAGINATION_CONFIG.PAGE_SIZES
  ): PagingRequest {
    return createPagingRequest(pageNumber, pageSize, entityType);
  }

  private getCacheKey(method: string, url: string, data?: any): string {
    return `${method}_${url}_${JSON.stringify(data || {})}`;
  }

  private getCachedData(key: string): any | null {
    const cached = this.cache.get(key);
    if (cached) {
      const isExpired = Date.now() - cached.timestamp > this.cacheTimeout;
      if (!isExpired) {
        return cached.data;
      }
      this.cache.delete(key);
    }
    return null;
  }

  private setCachedData(key: string, data: any): void {
    this.cache.set(key, { data, timestamp: Date.now() });
  }

  public clearCache(): void {
    this.cache.clear();
  }

  // Get auth headers from localStorage
  private getAuthHeaders(): Record<string, string> {
    const token = localStorage.getItem("auth_token");
    return {
      "Content-Type": "application/json",
      ...(token && { Authorization: `Bearer ${token}` })
    };
  }

  private async makeRequest(
    method: "GET" | "POST" | "PUT" | "DELETE",
    url: string,
    data?: any,
    params?: any,
    useCache: boolean = true
  ): Promise<any> {
    try {
      // Check cache for GET requests
      if (method === "GET" && useCache) {
        const cacheKey = this.getCacheKey(method, url, params);
        const cachedData = this.getCachedData(cacheKey);
        if (cachedData) {
          return cachedData;
        }
      }

      // Check cache for POST requests to specific endpoints
      if (
        method === "POST" &&
        useCache &&
        (url.includes("/GetPromotionDetails") ||
          url.includes("/SelectAllSKUDetails") ||
          url.includes("/SelectAllStore"))
      ) {
        const cacheKey = this.getCacheKey(method, url, data);
        const cachedData = this.getCachedData(cacheKey);
        if (cachedData) {
          return cachedData;
        }
      }

      let fullUrl = `${API_BASE_URL}${url}`;

      // Add query parameters for GET and DELETE requests
      if (params && (method === "GET" || method === "DELETE")) {
        const searchParams = new URLSearchParams();
        Object.keys(params).forEach((key) => {
          if (params[key] !== undefined && params[key] !== null) {
            searchParams.append(key, params[key].toString());
          }
        });
        if (searchParams.toString()) {
          fullUrl += `?${searchParams.toString()}`;
        }
      }

      const config: RequestInit = {
        method,
        headers: this.getAuthHeaders()
      };

      if (data && method !== "GET") {
        config.body = JSON.stringify(data);
      }

      const response = await fetch(fullUrl, config);

      if (response.status === 401) {
        if (typeof window !== "undefined") {
          localStorage.removeItem("auth_token");
          window.location.href = "/login";
        }
        throw new Error("Unauthorized");
      }

      let responseData;
      try {
        responseData = await response.json();
      } catch {
        responseData = null;
      }

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      // Handle API response format
      let result;
      if (responseData && typeof responseData === "object") {
        // If response has standard API format with IsSuccess flag
        if (
          ("IsSuccess" in responseData || "isSuccess" in responseData) &&
          ("Data" in responseData || "data" in responseData)
        ) {
          // This is a standard WINIT API response
          const extractedData = responseData.Data || responseData.data;

          // Check if the extracted data itself has PagedData
          if (
            extractedData &&
            typeof extractedData === "object" &&
            ("PagedData" in extractedData || "pagedData" in extractedData)
          ) {
            // Return the PagedData directly as the data
            result = {
              success:
                responseData.IsSuccess !== false &&
                responseData.isSuccess !== false,
              data: extractedData, // Keep the structure with PagedData
              message: responseData.Message || responseData.message,
              status: response.status
            };
          } else {
            // Return the extracted data as is
            result = {
              success:
                responseData.IsSuccess !== false &&
                responseData.isSuccess !== false,
              data: extractedData,
              message: responseData.Message || responseData.message,
              status: response.status
            };
          }
        } else if ("Data" in responseData || "data" in responseData) {
          // Legacy format without IsSuccess
          result = {
            success: true,
            data: responseData.Data || responseData.data,
            message: responseData.Message || responseData.message,
            status: response.status
          };
        } else {
          // If response is direct data
          result = {
            success: true,
            data: responseData,
            status: response.status
          };
        }
      } else {
        result = {
          success: true,
          data: responseData,
          status: response.status
        };
      }

      // Cache successful GET requests
      if (method === "GET" && useCache && result.success) {
        const cacheKey = this.getCacheKey(method, url, params);
        this.setCachedData(cacheKey, result);
      }

      // Cache successful POST requests for specific endpoints
      if (
        method === "POST" &&
        useCache &&
        result.success &&
        (url.includes("/GetPromotionDetails") ||
          url.includes("/SelectAllSKUDetails") ||
          url.includes("/SelectAllStore"))
      ) {
        const cacheKey = this.getCacheKey(method, url, data);
        this.setCachedData(cacheKey, result);
      }

      // Clear cache on mutations
      if ((method === "PUT" || method === "DELETE") && result.success) {
        // Clear all promotion-related cache entries
        if (url.includes("/Promotion")) {
          const keysToDelete: string[] = [];
          this.cache.forEach((value, key) => {
            if (key.includes("/Promotion")) {
              keysToDelete.push(key);
            }
          });
          keysToDelete.forEach((key) => this.cache.delete(key));
        }
      }

      // Transform the response to handle case sensitivity
      // Transform result.data, not the raw responseData
      const transformedResult = {
        ...result,
        data: transformApiResponse(result.data)
      };

      return transformedResult;
    } catch (error: any) {
      return {
        success: false,
        error: error.message || "Network error",
        status: 0,
        details: error
      };
    }
  }

  // Core Promotion APIs
  async getPromotionDetails(
    pageNumber: number = 1,
    pageSize?: number
  ): Promise<any> {
    const data = this.createPagingRequest(pageNumber, pageSize, "PROMOTIONS");
    const response = await this.makeRequest(
      "POST",
      "/Promotion/GetPromotionDetails",
      data
    );

    // The transformApiResponse in makeRequest will handle case sensitivity
    // No need for additional transformation here
    return response;
  }

  async getPromotionByUID(promotionUID: string): Promise<any> {
    const result = await this.makeRequest(
      "GET",
      "/Promotion/GetPromotionDetailsByUID",
      null,
      { promotionUID }
    );

    // Check if the API returned incomplete data (common backend issue)
    if (result.success && result.data) {
      // Handle case where backend returns the promotion directly without PromotionView wrapper
      // Check both uppercase and lowercase field names
      if (!result.data.PromotionView && !result.data.promotionView) {
        // Check if the data itself is the promotion (has promotion fields)
        if (
          result.data.uid ||
          result.data.UID ||
          result.data.code ||
          result.data.Code ||
          result.data.name ||
          result.data.Name
        ) {
          // The data is the promotion itself, wrap it properly
          return {
            ...result,
            data: {
              IsNew: false,
              PromotionView: result.data,
              PromoOrderViewList:
                result.data.PromoOrderViewList ||
                result.data.promoOrderViewList ||
                [],
              PromoOrderItemViewList:
                result.data.PromoOrderItemViewList ||
                result.data.promoOrderItemViewList ||
                [],
              PromoOfferViewList:
                result.data.PromoOfferViewList ||
                result.data.promoOfferViewList ||
                [],
              PromoOfferItemViewList:
                result.data.PromoOfferItemViewList ||
                result.data.promoOfferItemViewList ||
                [],
              PromoConditionViewList:
                result.data.PromoConditionViewList ||
                result.data.promoConditionViewList ||
                [],
              ItemPromotionMapViewList:
                result.data.ItemPromotionMapViewList ||
                result.data.itemPromotionMapViewList ||
                [],
              SchemeBranches:
                result.data.SchemeBranches || result.data.schemeBranches || [],
              SchemeOrgs:
                result.data.SchemeOrgs || result.data.schemeOrgs || [],
              SchemeBroadClassifications:
                result.data.SchemeBroadClassifications ||
                result.data.schemeBroadClassifications ||
                []
            }
          };
        }

        // If not, try the fallback
        try {
          // Fallback: Use the list API to get basic promotion data
          const listResult = await this.getPromotionDetails(1, 100);
          if (
            listResult.success &&
            (listResult.data?.PagedData || listResult.data?.pagedData)
          ) {
            const pagedData =
              listResult.data.PagedData || listResult.data.pagedData;
            const promotionFromList = pagedData.find(
              (p: any) =>
                (p.UID || p.uid) === promotionUID ||
                (p.Code || p.code) === promotionUID
            );
            if (promotionFromList) {
              // Construct a minimal but complete data structure
              const promoOfferUID = generateGUID();
              const fallbackData = {
                IsNew: false,
                PromotionView: promotionFromList,
                PromoOrderViewList: [],
                PromoOrderItemViewList: [],
                PromoOfferViewList: [
                  {
                    UID: promoOfferUID,
                    PromotionUID: promotionUID,
                    OfferNo: 1,
                    OfferType: "DISCOUNT"
                  }
                ],
                PromoOfferItemViewList: [
                  {
                    UID: generateGUID(),
                    PromoOfferUID: promoOfferUID,
                    ItemCriteriaType: "ALL",
                    ItemCriteriaSelected: "",
                    ItemQty: 0,
                    ItemAmount: 0,
                    ItemUOM: "PCS",
                    DiscountType: "PERCENTAGE",
                    DiscountValue: 10 // Default value
                  }
                ],
                PromoConditionViewList: [],
                ItemPromotionMapViewList: [],
                SchemeBranches: [],
                SchemeOrgs: [],
                SchemeBroadClassifications: []
              };

              return {
                ...result,
                data: fallbackData,
                fallbackUsed: true
              };
            }
          }
        } catch (fallbackError) {}

        // If fallback also fails, just return the data as-is but wrapped properly
        return {
          ...result,
          data: {
            IsNew: false,
            PromotionView: result.data, // Use the raw data as PromotionView
            PromoOrderViewList: [],
            PromoOrderItemViewList: [],
            PromoOfferViewList: [],
            PromoOfferItemViewList: [],
            PromoConditionViewList: [],
            ItemPromotionMapViewList: [],
            SchemeBranches: [],
            SchemeOrgs: [],
            SchemeBroadClassifications: []
          }
        };
      }
    }

    return result;
  }

  async createUpdatePromotion(promotionData: any): Promise<any> {
    const data = {
      IsNew: !promotionData.UID || promotionData.UID.startsWith("promo-"),
      PromotionView: promotionData
    };
    return this.makeRequest("POST", "/Promotion/CUDPromotionMaster", data);
  }

  async deletePromotion(promotionUID: string): Promise<any> {
    return this.makeRequest(
      "DELETE",
      "/Promotion/DeletePromotionDetailsbyUID",
      null,
      { PromotionUID: promotionUID }
    );
  }

  // Activate promotion
  async activatePromotion(promotionUID: string): Promise<IApiResponse<any>> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/Promotion/ActivatePromotion`,
        {
          method: "PUT",
          headers: this.getAuthHeaders(),
          body: JSON.stringify({ PromotionUID: promotionUID })
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const responseData = await response.json();

      // Handle the API response format
      let data;
      if (responseData.Data) {
        data = transformApiResponse(responseData.Data);
      } else if (responseData.data) {
        data = transformApiResponse(responseData.data);
      } else {
        data = transformApiResponse(responseData);
      }

      return {
        IsSuccess:
          responseData.IsSuccess !== false && responseData.isSuccess !== false,
        Data: data,
        StatusCode: response.status,
        ErrorMessage: responseData.ErrorMessage || responseData.errorMessage
      };
    } catch (error: any) {
      return {
        IsSuccess: false,
        Data: null,
        StatusCode: 500,
        ErrorMessage: error.message
      };
    }
  }

  // Deactivate promotion
  async deactivatePromotion(promotionUID: string): Promise<IApiResponse<any>> {
    try {
      const response = await fetch(
        `${API_BASE_URL}/Promotion/DeactivatePromotion`,
        {
          method: "PUT",
          headers: this.getAuthHeaders(),
          body: JSON.stringify({ PromotionUID: promotionUID })
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const responseData = await response.json();

      // Handle the API response format
      let data;
      if (responseData.Data) {
        data = transformApiResponse(responseData.Data);
      } else if (responseData.data) {
        data = transformApiResponse(responseData.data);
      } else {
        data = transformApiResponse(responseData);
      }

      return {
        IsSuccess:
          responseData.IsSuccess !== false && responseData.isSuccess !== false,
        Data: data,
        StatusCode: response.status,
        ErrorMessage: responseData.ErrorMessage || responseData.errorMessage
      };
    } catch (error: any) {
      return {
        IsSuccess: false,
        Data: null,
        StatusCode: 500,
        ErrorMessage: error.message
      };
    }
  }

  // SKU & Price APIs
  async getSKUDetails(pageNumber: number = 1, pageSize?: number): Promise<any> {
    const data = this.createPagingRequest(pageNumber, pageSize, "PRODUCTS");
    return this.makeRequest("POST", "/SKU/SelectAllSKUDetails", data);
  }

  async getAllSKUMasterData(
    pageNumber: number = 1,
    pageSize?: number
  ): Promise<any> {
    const data = {
      SKUUIDs: [],
      OrgUIDs: [],
      DistributionChannelUIDs: [],
      AttributeTypes: [],
      PageNumber: pageNumber,
      PageSize: pageSize || PAGINATION_CONFIG.PAGE_SIZES.PRODUCTS
    };
    return this.makeRequest("POST", "/SKU/GetAllSKUMasterData", data);
  }

  // Store & Customer APIs
  async getAllStores(pageNumber: number = 1, pageSize?: number): Promise<any> {
    const data = this.createPagingRequest(pageNumber, pageSize, "STORES");
    return this.makeRequest("POST", "/Store/SelectAllStore", data);
  }

  // Organization & Employee APIs
  async getOrganizationDetails(
    pageNumber: number = 1,
    pageSize?: number
  ): Promise<any> {
    const data = this.createPagingRequest(
      pageNumber,
      pageSize,
      "ORGANIZATIONS"
    );
    return this.makeRequest("POST", "/Org/GetOrgDetails", data);
  }

  async getEmployeeDetails(
    pageNumber: number = 1,
    pageSize?: number
  ): Promise<any> {
    const data = this.createPagingRequest(pageNumber, pageSize, "EMPLOYEES");
    return this.makeRequest("POST", "/Emp/GetEmpDetails", data);
  }

  // Legacy methods for backward compatibility
  async getActivePromotions(): Promise<any> {
    return this.getPromotionDetails();
  }

  // Export functionality for Promotions
  async exportPromotions(
    format: "csv" | "excel",
    filters?: { search?: string; status?: string }
  ): Promise<Blob> {
    try {
      // Get all promotions for export (use large page size)
      const response = await this.getPromotionDetails(1, 10000);

      if (!response.success || !response.data) {
        console.log("No promotion data found or API request failed");
        return this.exportPromotionsToCSV([]);
      }

      console.log("=== PROMOTION EXPORT DEBUG ===");
      console.log("Raw API response:", response);

      // Extract promotion data from different response formats
      let promotionData: any[] = [];

      if (
        response.data.Data?.PagedData &&
        Array.isArray(response.data.Data.PagedData)
      ) {
        promotionData = response.data.Data.PagedData;
      } else if (
        response.data.PagedData &&
        Array.isArray(response.data.PagedData)
      ) {
        promotionData = response.data.PagedData;
      } else if (response.data.Data && Array.isArray(response.data.Data)) {
        promotionData = response.data.Data;
      } else if (response.data.items && Array.isArray(response.data.items)) {
        promotionData = response.data.items;
      } else if (Array.isArray(response.data)) {
        promotionData = response.data;
      }

      console.log("Extracted promotion data:", promotionData.length, "records");

      // Apply client-side filtering if provided
      if (filters?.search) {
        const searchLower = filters.search.toLowerCase();
        const beforeFilter = promotionData.length;
        promotionData = promotionData.filter(
          (promotion: any) =>
            promotion.Name?.toLowerCase().includes(searchLower) ||
            promotion.Code?.toLowerCase().includes(searchLower) ||
            promotion.Description?.toLowerCase().includes(searchLower)
        );
        console.log(
          `Search filter applied: ${beforeFilter} -> ${promotionData.length} records`
        );
      }

      if (filters?.status && filters.status !== "all") {
        const beforeFilter = promotionData.length;
        promotionData = promotionData.filter((promotion: any) => {
          const isActive = Boolean(promotion.IsActive || promotion.isActive);
          const currentDate = new Date();
          const startDate = promotion.ValidFrom
            ? new Date(promotion.ValidFrom)
            : null;
          const endDate = promotion.ValidUpto
            ? new Date(promotion.ValidUpto)
            : null;

          let status = "inactive";
          if (isActive) {
            if (startDate && startDate > currentDate) {
              status = "scheduled";
            } else if (endDate && endDate < currentDate) {
              status = "expired";
            } else {
              status = "active";
            }
          }

          return status === filters.status;
        });
        console.log(
          `Status filter applied: ${beforeFilter} -> ${promotionData.length} records`
        );
      }

      console.log(
        "Final filtered promotion data for export:",
        promotionData.length,
        "records"
      );
      console.log("===============================");

      if (format === "csv") {
        return this.exportPromotionsToCSV(promotionData);
      } else {
        return this.exportPromotionsToExcel(promotionData);
      }
    } catch (error) {
      console.error("Failed to export promotions:", error);
      throw new Error("Failed to export promotions");
    }
  }

  private exportPromotionsToCSV(promotions: any[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Description",
      "Type",
      "Priority",
      "Valid From",
      "Valid Until",
      "Is Active",
      "Status",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date"
    ];

    const csvContent = [
      headers.join(","),
      ...promotions.map((promotion) => {
        // Determine status
        const isActive = Boolean(promotion.IsActive || promotion.isActive);
        const currentDate = new Date();
        const startDate = promotion.ValidFrom
          ? new Date(promotion.ValidFrom)
          : null;
        const endDate = promotion.ValidUpto
          ? new Date(promotion.ValidUpto)
          : null;

        let status = "Inactive";
        if (isActive) {
          if (startDate && startDate > currentDate) {
            status = "Scheduled";
          } else if (endDate && endDate < currentDate) {
            status = "Expired";
          } else {
            status = "Active";
          }
        }

        return [
          `"${promotion.Code || promotion.code || ""}"`,
          `"${promotion.Name || promotion.name || ""}"`,
          `"${
            promotion.Description ||
            promotion.Remarks ||
            promotion.description ||
            ""
          }"`,
          `"${promotion.Type || promotion.type || "N/A"}"`,
          `"${promotion.Priority || promotion.priority || "N/A"}"`,
          `"${
            promotion.ValidFrom
              ? new Date(promotion.ValidFrom).toLocaleDateString()
              : ""
          }"`,
          `"${
            promotion.ValidUpto
              ? new Date(promotion.ValidUpto).toLocaleDateString()
              : ""
          }"`,
          `"${isActive ? "Yes" : "No"}"`,
          `"${status}"`,
          `"${promotion.CreatedBy || promotion.createdBy || ""}"`,
          `"${
            promotion.CreatedTime
              ? new Date(promotion.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${promotion.ModifiedBy || promotion.modifiedBy || ""}"`,
          `"${
            promotion.ModifiedTime
              ? new Date(promotion.ModifiedTime).toLocaleDateString()
              : ""
          }"`
        ].join(",");
      })
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportPromotionsToExcel(promotions: any[]): Blob {
    // For now, return CSV format with Excel MIME type
    const csvContent = this.exportPromotionsToCSV(promotions);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;"
    });
  }
}

// Export a singleton instance
export const promotionService = new PromotionService();
export default promotionService;
