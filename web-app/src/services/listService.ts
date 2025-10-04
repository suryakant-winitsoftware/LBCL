import { authService } from "@/lib/auth-service";

export interface ListHeader {
  id?: number;
  uid: string;
  code: string;
  name: string;
  isEditable: boolean;
  isVisibleInUI: boolean;
  companyUID?: string;
  orgUID?: string;
  createdBy?: string;
  modifiedBy?: string;
  createdTime?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
}

export interface ListItem {
  id?: number;
  uid: string;
  code: string;
  name: string;
  listHeaderUID: string;
  serialNo?: number;
  isEditable: boolean;
  createdBy?: string;
  modifiedBy?: string;
  createdTime?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
}

export interface CreateListItemRequest {
  code: string;
  name: string;
  listHeaderUID: string;
  serialNo?: number;
  isEditable: boolean;
  uid: string;
}

export interface UpdateListItemRequest extends CreateListItemRequest {
  id: number;
}

export interface ListItemRequest {
  codes: string[];
  isCountRequired: boolean;
}

export interface ListItems {
  listItemRequest: ListItemRequest;
  pagingRequest: PagingRequest;
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias: SortCriteria[];
  filterCriterias: FilterCriteria[];
  isCountRequired: boolean;
}

export interface SortCriteria {
  sortParameter: string;
  direction: "Asc" | "Desc";
}

export interface FilterCriteria {
  name: string;
  value: string;
  type: "Equal" | "Like" | "NotEqual";
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

class ListService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  }

  private async makeRequest<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const token = authService.getToken();
    if (!token) {
      throw new Error("No authentication token available");
    }

    const url = `${this.baseUrl}/ListItemHeader/${endpoint}`;
    const response = await fetch(url, {
      ...options,
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
        Accept: "application/json",
        ...options.headers,
      },
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP ${response.status}: ${errorText}`);
    }

    const data = await response.json();
    if (!data.IsSuccess) {
      throw new Error(data.ErrorMessage || "API request failed");
    }

    return data.Data;
  }

  // List Headers APIs
  async getListHeaders(
    request: PagingRequest
  ): Promise<PagedResponse<ListHeader>> {
    const requestBody = {
      PageNumber: request.pageNumber,
      PageSize: request.pageSize,
      SortCriterias: request.sortCriterias.map((sc) => ({
        SortParameter: sc.sortParameter,
        Direction: sc.direction,
      })),
      FilterCriterias: request.filterCriterias.map((fc) => ({
        Name: fc.name,
        Value: fc.value,
        Type: fc.type,
      })),
      IsCountRequired: request.isCountRequired,
    };

    const apiResponse = await this.makeRequest<{
      PagedData: any[];
      TotalCount: number;
    }>("GetListHeaders", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });

    const transformedData = (apiResponse.PagedData || []).map((item) => ({
      id: item.Id,
      uid: item.UID,
      code: item.Code,
      name: item.Name,
      isEditable: item.IsEditable,
      isVisibleInUI: item.IsVisibleInUI,
      companyUID: item.CompanyUID,
      orgUID: item.OrgUID,
      createdBy: item.CreatedBy,
      modifiedBy: item.ModifiedBy,
      createdTime: item.CreatedTime,
      modifiedTime: item.ModifiedTime,
      serverAddTime: item.ServerAddTime,
      serverModifiedTime: item.ServerModifiedTime,
    }));

    return {
      pagedData: transformedData,
      totalCount: apiResponse.TotalCount || 0,
    };
  }

  // List Items APIs
  async getListItemsByCodes(
    codes: string[],
    isCountRequired: boolean = true
  ): Promise<PagedResponse<ListItem>> {
    const requestBody = {
      Codes: codes,
      isCountRequired,
    };

    const apiResponse = await this.makeRequest<{
      PagedData: any[];
      TotalCount: number;
    }>("GetListItemsByCodes", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });

    const transformedData = (apiResponse.PagedData || []).map((item) => ({
      id: item.Id,
      uid: item.UID,
      code: item.Code,
      name: item.Name,
      listHeaderUID: item.ListHeaderUID,
      serialNo: item.SerialNo,
      isEditable: item.IsEditable,
      createdBy: item.CreatedBy,
      modifiedBy: item.ModifiedBy,
      createdTime: item.CreatedTime,
      modifiedTime: item.ModifiedTime,
      serverAddTime: item.ServerAddTime,
      serverModifiedTime: item.ServerModifiedTime,
    }));

    return {
      pagedData: transformedData,
      totalCount: apiResponse.TotalCount || 0,
    };
  }

  async getListItemsByHeaderUID(headerUID: string): Promise<ListItem[]> {
    try {
      const apiResponse = await this.makeRequest<any[]>(
        "GetListItemsByHeaderUID",
        {
          method: "POST",
          body: JSON.stringify(headerUID),
        }
      );

      return (apiResponse || []).map((item) => ({
        id: item.Id,
        uid: item.UID,
        code: item.Code,
        name: item.Name,
        listHeaderUID: item.ListHeaderUID,
        serialNo: item.SerialNo,
        isEditable: item.IsEditable,
        createdBy: item.CreatedBy,
        modifiedBy: item.ModifiedBy,
        createdTime: item.CreatedTime,
        modifiedTime: item.ModifiedTime,
        serverAddTime: item.ServerAddTime,
        serverModifiedTime: item.ServerModifiedTime,
      }));
    } catch (error) {
      // Fallback: Get all items and filter by header UID
      console.warn(
        "GetListItemsByHeaderUID failed, using fallback approach:",
        error
      );
      try {
        // Get header code by UID to use in the fallback
        const response = await this.getListItemsByCodes([headerUID], true);
        return response.pagedData.filter(
          (item) => item.listHeaderUID === headerUID
        );
      } catch (fallbackError) {
        console.error("Fallback approach also failed:", fallbackError);
        throw new Error("Failed to retrieve list items for this header");
      }
    }
  }

  async getListItemsByUID(uid: string): Promise<ListItem> {
    const apiResponse = await this.makeRequest<any>(
      `GetListItemsByUID?UID=${encodeURIComponent(uid)}`,
      {
        method: "GET",
      }
    );

    return {
      id: apiResponse.Id,
      uid: apiResponse.UID,
      code: apiResponse.Code,
      name: apiResponse.Name,
      listHeaderUID: apiResponse.ListHeaderUID,
      serialNo: apiResponse.SerialNo,
      isEditable: apiResponse.IsEditable,
      createdBy: apiResponse.CreatedBy,
      modifiedBy: apiResponse.ModifiedBy,
      createdTime: apiResponse.CreatedTime,
      modifiedTime: apiResponse.ModifiedTime,
      serverAddTime: apiResponse.ServerAddTime,
      serverModifiedTime: apiResponse.ServerModifiedTime,
    };
  }

  async createListItem(listItem: CreateListItemRequest): Promise<number> {
    const now = new Date().toISOString();
    const requestBody = {
      Code: listItem.code,
      Name: listItem.name,
      ListHeaderUID: listItem.listHeaderUID,
      SerialNo: listItem.serialNo || 1,
      IsEditable: listItem.isEditable ?? true,
      UID: listItem.uid,
      CreatedBy: "ADMIN",
      ModifiedBy: "ADMIN",
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now,
    };

    return this.makeRequest<number>("CreateListItem", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });
  }

  async updateListItem(listItem: UpdateListItemRequest): Promise<number> {
    const now = new Date().toISOString();
    const requestBody = {
      Id: listItem.id,
      Code: listItem.code,
      Name: listItem.name,
      ListHeaderUID: listItem.listHeaderUID,
      SerialNo: listItem.serialNo || 1,
      IsEditable: listItem.isEditable ?? true,
      UID: listItem.uid,
      ModifiedBy: "ADMIN",
      ModifiedTime: now,
      ServerModifiedTime: now,
    };

    return this.makeRequest<number>("UpdateListItem", {
      method: "PUT",
      body: JSON.stringify(requestBody),
    });
  }

  async deleteListItem(uid: string): Promise<number> {
    return this.makeRequest<number>(
      `DeleteListItemByUID?UID=${encodeURIComponent(uid)}`,
      {
        method: "DELETE",
      }
    );
  }

  async getListItemsByListHeaderCodes(
    listItems: ListItems
  ): Promise<PagedResponse<ListItem>> {
    const requestBody = {
      ListItemRequest: {
        Codes: listItems.listItemRequest.codes,
        isCountRequired: listItems.listItemRequest.isCountRequired,
      },
      PagingRequest: {
        PageNumber: listItems.pagingRequest.pageNumber,
        PageSize: listItems.pagingRequest.pageSize,
        SortCriterias: listItems.pagingRequest.sortCriterias.map((sc) => ({
          SortParameter: sc.sortParameter,
          Direction: sc.direction,
        })),
        FilterCriterias: listItems.pagingRequest.filterCriterias.map((fc) => ({
          Name: fc.name,
          Value: fc.value,
          Type: fc.type,
        })),
        IsCountRequired: listItems.pagingRequest.isCountRequired,
      },
    };

    const apiResponse = await this.makeRequest<{
      PagedData: any[];
      TotalCount: number;
    }>("GetListItemsByListHeaderCodes", {
      method: "POST",
      body: JSON.stringify(requestBody),
    });

    const transformedData = (apiResponse.PagedData || []).map((item) => ({
      id: item.Id,
      uid: item.UID,
      code: item.Code,
      name: item.Name,
      listHeaderUID: item.ListHeaderUID,
      serialNo: item.SerialNo,
      isEditable: item.IsEditable,
      createdBy: item.CreatedBy,
      modifiedBy: item.ModifiedBy,
      createdTime: item.CreatedTime,
      modifiedTime: item.ModifiedTime,
      serverAddTime: item.ServerAddTime,
      serverModifiedTime: item.ServerModifiedTime,
    }));

    return {
      pagedData: transformedData,
      totalCount: apiResponse.TotalCount || 0,
    };
  }

  // Helper method to generate unique UID
  generateUID(name: string): string {
    const timestamp = new Date()
      .toISOString()
      .replace(/[-:T.]/g, "")
      .slice(0, 14);
    const cleanName = name.toUpperCase().replace(/[^A-Z0-9]/g, "_");
    return `${cleanName}_${timestamp}`;
  }

  // Validate list item data
  validateListItem(listItem: Partial<CreateListItemRequest>): string[] {
    const errors: string[] = [];

    if (!listItem.code?.trim()) {
      errors.push("Code is required");
    }
    if (!listItem.name?.trim()) {
      errors.push("Name is required");
    }
    if (!listItem.listHeaderUID?.trim()) {
      errors.push("List Header is required");
    }

    return errors;
  }
}

export const listService = new ListService();
