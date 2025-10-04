import { getAuthHeaders } from "@/lib/auth-service";
import { PagingRequest, PagedResponse } from "@/types/common.types";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

// SKU Price interfaces
export interface ISKUPrice {
  Id?: number;
  UID?: string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
  ServerAddTime?: Date | string;
  ServerModifiedTime?: Date | string;
  SKUName?: string;
  SKUPriceListUID?: string;
  SKUCode: string;
  UOM: string;
  Price: number;
  DefaultWSPrice?: number;
  DefaultRetPrice?: number;
  DummyPrice?: number;
  MRP: number;
  PriceUpperLimit?: number;
  PriceLowerLimit?: number;
  Status: string;
  ValidFrom: Date | string;
  ValidUpto: Date | string;
  IsActive: boolean;
  IsTaxIncluded?: boolean;
  VersionNo?: string;
  SKUUID: string;
  IsLatest?: number;
  LadderingAmount?: number;
  LadderingPercentage?: number;
}

// SKU Price List interfaces
export interface ISKUPriceList {
  Id?: number;
  UID?: string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
  ServerAddTime?: Date | string;
  ServerModifiedTime?: Date | string;
  CompanyUID?: string;
  Code: string;
  Name: string;
  Type?: string;
  OrgUID?: string;
  DistributionChannelUID?: string;
  Priority: number;
  SelectionGroup?: string;
  SelectionType?: string;
  SelectionUID?: string;
  IsActive: boolean;
  Status: string;
  ValidFrom?: Date | string;
  ValidUpto?: Date | string;
}

// SKU Price View DTO
export interface SKUPriceViewDTO {
  SKUPriceGroup: ISKUPriceList;
  SKUPriceList: ISKUPrice[];
}

// SKU Price Service
class SKUPriceService {
  private baseUrl = API_BASE_URL;

  private async fetchAPI<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const headers = await getAuthHeaders();

    const response = await fetch(`${this.baseUrl}${endpoint}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...headers,
        ...options.headers,
      },
    });

    if (!response.ok) {
      const text = await response.text();

      // For 404 responses, return empty data instead of throwing error
      if (response.status === 404) {
        return { PagedData: [], TotalCount: 0 } as T;
      }

      // Log error for other status codes
      console.error(
        `SKU Price API error - Status: ${response.status}, Endpoint: ${endpoint}`
      );
      console.error("Response body:", text);

      // Try to parse as JSON if possible
      let error: any = { message: "Request failed" };
      try {
        error = JSON.parse(text);
      } catch {
        // Not JSON, use the text as message
        error = {
          message: text || `Request failed with status ${response.status}`,
        };
      }
      throw new Error(error.ErrorMessage || error.message || "Request failed");
    }

    // Check if response is JSON
    const contentType = response.headers.get("content-type");
    if (!contentType || !contentType.includes("application/json")) {
      const text = await response.text();
      console.error("Non-JSON response received from SKU Price API:", text);
      throw new Error("API returned non-JSON response");
    }

    const data = await response.json();
    return data.Data;
  }

  // SKU Price Methods
  async getAllSKUPrices(
    request: PagingRequest
  ): Promise<PagedResponse<ISKUPrice>> {
    // Convert to lowercase property names for API
    const apiRequest = {
      pageNumber: request.PageNumber || 1,
      pageSize: request.PageSize || 10,
      filterCriterias: request.FilterCriterias || [],
      sortCriterias: request.SortCriterias || [],
      isCountRequired: request.IsCountRequired ?? true,
    };

    console.log(
      "📤 SKU Price API Request:",
      JSON.stringify(apiRequest, null, 2)
    );

    return this.fetchAPI<PagedResponse<ISKUPrice>>(
      "/SKUPrice/SelectAllSKUPriceDetails",
      {
        method: "POST",
        body: JSON.stringify(apiRequest),
      }
    );
  }

  async getSKUPriceByUID(uid: string): Promise<ISKUPrice> {
    return this.fetchAPI<ISKUPrice>(`/SKUPrice/SelectSKUPriceByUID?UID=${uid}`);
  }

  async createSKUPrice(price: ISKUPrice): Promise<number> {
    return this.fetchAPI<number>("/SKUPrice/CreateSKUPrice", {
      method: "POST",
      body: JSON.stringify(price),
    });
  }

  async updateSKUPrice(price: ISKUPrice): Promise<number> {
    // Workaround for backend bug: The backend has a SQL syntax error in SelectSKUPriceByUID
    // which is called before update. We'll use UpdateSKUPriceList instead which bypasses the check
    const priceWithActionType = {
      ...price,
      ActionType: 1, // 1 = Update in the ActionType enum
    };

    // Use UpdateSKUPriceList endpoint which doesn't call SelectSKUPriceByUID first
    return this.fetchAPI<number>("/SKUPrice/UpdateSKUPriceList", {
      method: "PUT",
      body: JSON.stringify([priceWithActionType]), // Send as array
    });
  }

  async deleteSKUPrice(uid: string): Promise<number> {
    return this.fetchAPI<number>(`/SKUPrice/DeleteSKUPrice?UID=${uid}`, {
      method: "DELETE",
    });
  }

  async updateSKUPriceList(prices: ISKUPrice[]): Promise<number> {
    return this.fetchAPI<number>("/SKUPrice/UpdateSKUPriceList", {
      method: "PUT",
      body: JSON.stringify(prices),
    });
  }

  // SKU Price List Methods
  async getAllPriceLists(
    request: PagingRequest
  ): Promise<PagedResponse<ISKUPriceList>> {
    return this.fetchAPI<PagedResponse<ISKUPriceList>>(
      "/SKUPriceList/SelectAllSKUPriceListDetails",
      {
        method: "POST",
        body: JSON.stringify(request),
      }
    );
  }

  async getPriceListByUID(uid: string): Promise<ISKUPriceList> {
    return this.fetchAPI<ISKUPriceList>(
      `/SKUPriceList/SelectSKUPriceListByUID?UID=${uid}`
    );
  }

  async createPriceList(priceList: ISKUPriceList): Promise<number> {
    return this.fetchAPI<number>("/SKUPriceList/CreateSKUPriceList", {
      method: "POST",
      body: JSON.stringify(priceList),
    });
  }

  async updatePriceList(priceList: ISKUPriceList): Promise<number> {
    return this.fetchAPI<number>("/SKUPriceList/UpdateSKUPriceList", {
      method: "PUT",
      body: JSON.stringify(priceList),
    });
  }

  async deletePriceList(uid: string): Promise<number> {
    return this.fetchAPI<number>(
      `/SKUPriceList/DeleteSKUPriceList?UID=${uid}`,
      {
        method: "DELETE",
      }
    );
  }

  // SKU Price View Methods
  async getSKUPriceView(
    request: PagingRequest,
    uid: string
  ): Promise<PagedResponse<SKUPriceViewDTO>> {
    return this.fetchAPI<PagedResponse<SKUPriceViewDTO>>(
      `/SKUPrice/SelectSKUPriceViewByUID?UID=${uid}`,
      {
        method: "POST",
        body: JSON.stringify(request),
      }
    );
  }

  async createSKUPriceView(priceView: SKUPriceViewDTO): Promise<number> {
    return this.fetchAPI<number>("/SKUPrice/CreateSKUPriceView", {
      method: "POST",
      body: JSON.stringify(priceView),
    });
  }

  async updateSKUPriceView(priceView: SKUPriceViewDTO): Promise<number> {
    return this.fetchAPI<number>("/SKUPrice/UpdateSKUPriceView", {
      method: "PUT",
      body: JSON.stringify(priceView),
    });
  }

  async createStandardPriceForSKU(skuUID: string): Promise<number> {
    return this.fetchAPI<number>("/SKUPrice/CreateStandardPriceForSKU", {
      method: "POST",
      body: JSON.stringify(skuUID),
    });
  }

  // Export functionality for SKU Prices
  async exportSKUPrices(
    format: "csv" | "excel",
    searchTerm?: string
  ): Promise<Blob> {
    try {
      // Build request to get all SKU prices for export
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000, // Get up to 10,000 prices
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: true,
      };

      const response = await this.getAllSKUPrices(request);
      let priceData = response.PagedData || [];

      // Apply client-side filtering if search term provided
      if (searchTerm) {
        const searchLower = searchTerm.toLowerCase();
        priceData = priceData.filter(
          (price) =>
            price.SKUCode?.toLowerCase().includes(searchLower) ||
            price.SKUName?.toLowerCase().includes(searchLower)
        );
      }

      if (format === "csv") {
        return this.exportPricesToCSV(priceData);
      } else {
        return this.exportPricesToExcel(priceData);
      }
    } catch (error) {
      console.error("Failed to export SKU prices:", error);
      throw new Error("Failed to export SKU prices");
    }
  }

  // Export functionality for Price Lists
  async exportPriceLists(
    format: "csv" | "excel",
    searchTerm?: string
  ): Promise<Blob> {
    try {
      // Build request to get all price lists for export
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000, // Get up to 10,000 price lists
        FilterCriterias: searchTerm
          ? [
              { Name: "Name", Value: searchTerm, Type: 1 },
              { Name: "Code", Value: searchTerm, Type: 1 },
            ]
          : [],
        SortCriterias: [],
        IsCountRequired: true,
      };

      const response = await this.getAllPriceLists(request);
      const priceListData = response.PagedData || [];

      if (format === "csv") {
        return this.exportPriceListsToCSV(priceListData);
      } else {
        return this.exportPriceListsToExcel(priceListData);
      }
    } catch (error) {
      console.error("Failed to export price lists:", error);
      throw new Error("Failed to export price lists");
    }
  }

  private exportPricesToCSV(prices: ISKUPrice[]): Blob {
    const headers = [
      "SKU Code",
      "Product Name",
      "Price List UID",
      "UOM",
      "Price",
      "MRP",
      "Default WS Price",
      "Default Retail Price",
      "Price Upper Limit",
      "Price Lower Limit",
      "Status",
      "Valid From",
      "Valid To",
      "Is Active",
      "Is Tax Included",
      "Laddering Amount",
      "Laddering Percentage",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...prices.map((price) =>
        [
          `"${price.SKUCode || ""}"`,
          `"${price.SKUName || ""}"`,
          `"${price.SKUPriceListUID || ""}"`,
          `"${price.UOM || ""}"`,
          `"${price.Price || 0}"`,
          `"${price.MRP || 0}"`,
          `"${price.DefaultWSPrice || 0}"`,
          `"${price.DefaultRetPrice || 0}"`,
          `"${price.PriceUpperLimit || 0}"`,
          `"${price.PriceLowerLimit || 0}"`,
          `"${price.Status || ""}"`,
          `"${
            price.ValidFrom
              ? new Date(price.ValidFrom).toLocaleDateString()
              : ""
          }"`,
          `"${
            price.ValidUpto
              ? new Date(price.ValidUpto).toLocaleDateString()
              : ""
          }"`,
          `"${price.IsActive ? "Active" : "Inactive"}"`,
          `"${price.IsTaxIncluded ? "Yes" : "No"}"`,
          `"${price.LadderingAmount || 0}"`,
          `"${price.LadderingPercentage || 0}"`,
          `"${price.CreatedBy || ""}"`,
          `"${
            price.CreatedTime
              ? new Date(price.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${price.ModifiedBy || ""}"`,
          `"${
            price.ModifiedTime
              ? new Date(price.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportPriceListsToCSV(priceLists: ISKUPriceList[]): Blob {
    const headers = [
      "Code",
      "Name",
      "Type",
      "Organization UID",
      "Company UID",
      "Distribution Channel UID",
      "Priority",
      "Selection Group",
      "Selection Type",
      "Selection UID",
      "Status",
      "Valid From",
      "Valid To",
      "Is Active",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...priceLists.map((priceList) =>
        [
          `"${priceList.Code || ""}"`,
          `"${priceList.Name || ""}"`,
          `"${priceList.Type || ""}"`,
          `"${priceList.OrgUID || ""}"`,
          `"${priceList.CompanyUID || ""}"`,
          `"${priceList.DistributionChannelUID || ""}"`,
          `"${priceList.Priority || 0}"`,
          `"${priceList.SelectionGroup || ""}"`,
          `"${priceList.SelectionType || ""}"`,
          `"${priceList.SelectionUID || ""}"`,
          `"${priceList.Status || ""}"`,
          `"${
            priceList.ValidFrom
              ? new Date(priceList.ValidFrom).toLocaleDateString()
              : ""
          }"`,
          `"${
            priceList.ValidUpto
              ? new Date(priceList.ValidUpto).toLocaleDateString()
              : ""
          }"`,
          `"${priceList.IsActive ? "Active" : "Inactive"}"`,
          `"${priceList.CreatedBy || ""}"`,
          `"${
            priceList.CreatedTime
              ? new Date(priceList.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${priceList.ModifiedBy || ""}"`,
          `"${
            priceList.ModifiedTime
              ? new Date(priceList.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportPricesToExcel(prices: ISKUPrice[]): Blob {
    // For now, return CSV format with Excel MIME type
    const csvContent = this.exportPricesToCSV(prices);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }

  private exportPriceListsToExcel(priceLists: ISKUPriceList[]): Blob {
    // For now, return CSV format with Excel MIME type
    const csvContent = this.exportPriceListsToCSV(priceLists);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }

  // Export functionality for TPP (Time Period Pricing) - Hierarchical view
  async exportTPPPrices(
    format: "csv" | "excel",
    searchTerm?: string
  ): Promise<Blob> {
    try {
      // Build request to get all SKU prices for export
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000, // Get up to 10,000 prices
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: true,
      };

      const response = await this.getAllSKUPrices(request);
      let priceData = response.PagedData || [];

      // Apply client-side search filtering
      if (searchTerm) {
        const searchLower = searchTerm.toLowerCase();
        priceData = priceData.filter(
          (price) =>
            price.SKUCode?.toLowerCase().includes(searchLower) ||
            price.SKUName?.toLowerCase().includes(searchLower)
        );
      }

      // Group prices by SKU to create hierarchical structure (same logic as TPPPriceTab)
      const groupedPrices = priceData.reduce(
        (acc: { [key: string]: ISKUPrice[] }, price) => {
          const key = price.SKUCode || "";
          if (!acc[key]) {
            acc[key] = [];
          }
          acc[key].push(price);
          return acc;
        },
        {}
      );

      // Flatten the hierarchy for CSV export with parent-child indicators
      const flattenedData: any[] = [];
      Object.entries(groupedPrices).forEach(([skuCode, prices]) => {
        if (prices.length > 0) {
          // Sort prices by ValidFrom date (earliest first)
          const sortedPrices = prices.sort((a, b) => {
            const dateA = a.ValidFrom ? new Date(a.ValidFrom).getTime() : 0;
            const dateB = b.ValidFrom ? new Date(b.ValidFrom).getTime() : 0;
            return dateA - dateB;
          });

          // Add all prices with hierarchy indicators
          sortedPrices.forEach((price, index) => {
            flattenedData.push({
              ...price,
              HierarchyLevel: index === 0 ? "Parent" : "Child",
              HierarchyIndex: index,
              SKUGroup: skuCode,
            });
          });
        }
      });

      if (format === "csv") {
        return this.exportTPPPricesToCSV(flattenedData);
      } else {
        return this.exportTPPPricesToExcel(flattenedData);
      }
    } catch (error) {
      console.error("Failed to export TPP prices:", error);
      throw new Error("Failed to export TPP prices");
    }
  }

  private exportTPPPricesToCSV(tppPrices: any[]): Blob {
    const headers = [
      "SKU Group",
      "Hierarchy Level",
      "Hierarchy Index",
      "SKU Code",
      "Product Name",
      "Price List UID",
      "UOM",
      "Cost Price",
      "MRP",
      "A.Retail Price",
      "Default WS Price",
      "Price Upper Limit",
      "Price Lower Limit",
      "Valid From",
      "Valid To",
      "Status",
      "Is Active",
      "Is Tax Included",
      "Laddering Amount",
      "Laddering Percentage",
      "Created By",
      "Created Date",
      "Modified By",
      "Modified Date",
    ];

    const csvContent = [
      headers.join(","),
      ...tppPrices.map((price) =>
        [
          `"${price.SKUGroup || ""}"`,
          `"${price.HierarchyLevel || ""}"`,
          `"${price.HierarchyIndex || 0}"`,
          `"${price.SKUCode || ""}"`,
          `"${price.SKUName || ""}"`,
          `"${price.SKUPriceListUID || ""}"`,
          `"${price.UOM || ""}"`,
          `"${price.Price || 0}"`,
          `"${price.MRP || 0}"`,
          `"${price.DefaultRetPrice || 0}"`,
          `"${price.DefaultWSPrice || 0}"`,
          `"${price.PriceUpperLimit || 0}"`,
          `"${price.PriceLowerLimit || 0}"`,
          `"${
            price.ValidFrom
              ? new Date(price.ValidFrom).toLocaleDateString()
              : ""
          }"`,
          `"${
            price.ValidUpto
              ? new Date(price.ValidUpto).toLocaleDateString()
              : ""
          }"`,
          `"${price.Status || ""}"`,
          `"${price.IsActive ? "Active" : "Inactive"}"`,
          `"${price.IsTaxIncluded ? "Yes" : "No"}"`,
          `"${price.LadderingAmount || 0}"`,
          `"${price.LadderingPercentage || 0}"`,
          `"${price.CreatedBy || ""}"`,
          `"${
            price.CreatedTime
              ? new Date(price.CreatedTime).toLocaleDateString()
              : ""
          }"`,
          `"${price.ModifiedBy || ""}"`,
          `"${
            price.ModifiedTime
              ? new Date(price.ModifiedTime).toLocaleDateString()
              : ""
          }"`,
        ].join(",")
      ),
    ].join("\n");

    return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
  }

  private exportTPPPricesToExcel(tppPrices: any[]): Blob {
    // For now, return CSV format with Excel MIME type
    const csvContent = this.exportTPPPricesToCSV(tppPrices);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const skuPriceService = new SKUPriceService();
