import { getAuthHeaders } from "@/lib/auth-service";

// Warehouse Stock Models
export interface IWHStockSummary {
  OrgUID: string;
  WarehouseUID: string;
  WarehouseCode: string;
  WarehouseName: string;
  SKUUID: string;
  SKUCode: string;
  SKUName: string;
  BatchNumber: string;
  SalableQty: number;
  NonSalableQty: number;
  ReservedQty: number;
  TotalQty: number;
}

export interface IWHStockRequest {
  Id?: number;
  UID: string;
  CompanyUID?: string;
  SourceOrgUID: string;
  SourceWHUID: string;
  TargetOrgUID: string;
  TargetWHUID: string;
  Code: string;
  RequestType: string;
  RequestByEmpUID: string;
  JobPositionUID: string;
  RequiredByDate: string;
  Status: string;
  Remarks: string;
  StockType: string;
  RouteUID: string;
  OrgUID: string;
  WareHouseUID: string;
  YearMonth: number;
  ActionType: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface IWHStockRequestLine {
  Id?: number;
  UID: string;
  WHStockRequestUID: string;
  SKUUID: string;
  SKUCode: string;
  RequestedQty: number;
  ApprovedQty: number;
  UOM: string;
  BatchNumber?: string;
  ExpiryDate?: string;
  StockType: string;
  Remarks?: string;
  ActionType: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface IWHStockLedger {
  Id?: number;
  UID: string;
  CompanyUID: string;
  WarehouseUID: string;
  OrgUID: string;
  SKUUID: string;
  SKUCode: string;
  Type: number;
  ReferenceType: string;
  ReferenceUID: string;
  BatchNumber: string;
  ExpiryDate?: string;
  Qty: number;
  UOM: string;
  StockType: string;
  SerialNo: string;
  VersionNo: string;
  ParentWhUID: string;
  YearMonth: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface IWHRequestTemplateModel {
  WHStockRequest: IWHStockRequest;
  WHStockRequestLines: IWHStockRequestLine[];
  WHStockLedgerList?: IWHStockLedger[];
}

export interface IWHStockRequestItemView {
  UID: string;
  RequestCode: string;
  Code?: string; // Keep for backward compatibility
  RequestType: string;
  Status: string;
  // Source Organization
  SourceOrgUID?: string;
  SourceOrgCode?: string;
  SourceOrgName?: string;
  // Source Warehouse
  SourceWHUID?: string;
  SourceWHCode?: string;
  SourceWHName?: string;
  // Target Organization
  TargetOrgUID?: string;
  TargetOrgCode?: string;
  TargetOrgName?: string;
  // Target Warehouse
  TargetWHUID?: string;
  TargetWHCode?: string;
  TargetWHName?: string;
  // Legacy fields
  SourceCode?: string;
  SourceName?: string;
  TargetCode?: string;
  TargetName?: string;
  // Other fields
  RequestByEmpName?: string;
  RequiredByDate: string;
  StockType?: string;
  TotalItems?: number;
  RequestedTime: string;
  ModifiedTime?: string;
  CreatedTime?: string; // Keep for backward compatibility
  OrgUID?: string;
  Remarks?: string;
  RouteCode?: string;
  RouteName?: string;
  YearMonth?: number;
}

export interface IViewLoadRequestItemView {
  WHStockRequest: IWHStockRequest;
  WHStockRequestLines: IWHStockRequestLine[];
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  isCountRequired: boolean;
  filterCriterias?: Array<{
    name: string;
    value: string;
    operator?: string;
  }>;
  sortCriterias?: Array<{
    sortParameter: string;
    direction: string;
  }>;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

class InventoryService {
  private baseUrl =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // Stock Updater APIs
  async getWHStockSummary(
    orgUID: string,
    wareHouseUID: string
  ): Promise<IWHStockSummary[]> {
    try {
      const response = await fetch(
        `${this.baseUrl}/StockUpdater/GetWHStockSummary?orgUID=${orgUID}&wareHouseUID=${wareHouseUID}`,
        {
          method: "GET",
          headers: getAuthHeaders()
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error("API Error Response:", errorText);
        throw new Error(
          `Failed to fetch stock summary: ${response.statusText}`
        );
      }

      const result: ApiResponse<IWHStockSummary[]> = await response.json();
      return result.Data || [];
    } catch (error) {
      console.error("Error fetching stock summary:", error);
      // Return empty data instead of throwing error to prevent UI crash
      return [];
    }
  }

  async updateStockAsync(stockLedgers: IWHStockLedger[]): Promise<number> {
    try {
      const response = await fetch(
        `${this.baseUrl}/StockUpdater/UpdateStockAsync`,
        {
          method: "POST",
          headers: getAuthHeaders(),
          body: JSON.stringify(stockLedgers)
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error("API Error Response:", errorText);
        throw new Error(`Failed to update stock: ${response.statusText}`);
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error updating stock:", error);
      throw error;
    }
  }

  // WHStock APIs
  async selectLoadRequestData(
    request: PagingRequest,
    stockType: string
  ): Promise<PagedResponse<IWHStockRequestItemView>> {
    try {
      const response = await fetch(
        `${this.baseUrl}/WHStock/SelectLoadRequestData?StockType=${stockType}`,
        {
          method: "POST",
          headers: getAuthHeaders(),
          body: JSON.stringify(request)
        }
      );

      if (!response.ok) {
        const errorText = await response.text();
        console.error("API Error Response:", errorText);
        throw new Error(
          `Failed to fetch load request data: ${response.statusText}`
        );
      }

      const result: ApiResponse<PagedResponse<IWHStockRequestItemView>> =
        await response.json();
      return result.Data || { PagedData: [], TotalCount: 0 };
    } catch (error) {
      console.error("Error fetching load request data:", error);
      // Return empty data instead of throwing error to prevent UI crash
      return { PagedData: [], TotalCount: 0 };
    }
  }

  async selectLoadRequestDataByUID(
    uid: string
  ): Promise<IViewLoadRequestItemView | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/WHStock/SelectLoadRequestDataByUID?UID=${uid}`,
        {
          method: "GET",
          headers: getAuthHeaders()
        }
      );

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(
          `Failed to fetch load request by UID: ${response.statusText}`
        );
      }

      const result: ApiResponse<IViewLoadRequestItemView> =
        await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching load request by UID:", error);
      throw error;
    }
  }

  async cudWHStock(requestTemplate: IWHRequestTemplateModel): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/WHStock/CUDWHStock`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(requestTemplate)
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.ErrorMessage || "Failed to create/update warehouse stock"
        );
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error with CUD warehouse stock:", error);
      throw error;
    }
  }

  async createWHStockFromQueue(
    requestTemplates: IWHRequestTemplateModel[]
  ): Promise<string> {
    try {
      const response = await fetch(
        `${this.baseUrl}/WHStock/CreateWHStockFromQueue`,
        {
          method: "POST",
          headers: getAuthHeaders(),
          body: JSON.stringify(requestTemplates)
        }
      );

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.ErrorMessage || "Failed to create warehouse stock from queue"
        );
      }

      const result = await response.json();
      return result.Message || "Request submitted successfully";
    } catch (error) {
      console.error("Error creating warehouse stock from queue:", error);
      throw error;
    }
  }

  async cudWHStockRequestLine(
    stockRequestLines: IWHStockRequestLine[]
  ): Promise<number> {
    try {
      const response = await fetch(
        `${this.baseUrl}/WHStock/CUDWHStockRequestLine`,
        {
          method: "POST",
          headers: getAuthHeaders(),
          body: JSON.stringify(stockRequestLines)
        }
      );

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.ErrorMessage || "Failed to update stock request lines"
        );
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error with CUD stock request lines:", error);
      throw error;
    }
  }
}

export const inventoryService = new InventoryService();
