import { getAuthHeaders } from "@/lib/auth-service";
import { FilterCriteria, SortCriteria } from "@/types/common.types";

interface PagingRequest {
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
  FilterCriterias: FilterCriteria[];
  SortCriterias: SortCriteria[];
}

export interface SKUGroup {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  SKUGroupTypeUID: string;
  ParentUID?: string;
  ItemLevel: number;
  SupplierOrgUID: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export interface SKUGroupType {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  ParentUID?: string;
  ItemLevel: number;
  AvailableForFilter: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  IsSelected?: boolean;
}

export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

class SKUGroupService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  // SKU Groups
  async getAllSKUGroups(
    request: PagingRequest
  ): Promise<ApiResponse<PagedResponse<SKUGroup>>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectAllSKUGroupDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(request),
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU groups: ${response.statusText}`);
    }

    const result: ApiResponse<PagedResponse<SKUGroup>> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU groups");
    }

    return result;
  }

  async getSKUGroupByUID(uid: string): Promise<SKUGroup> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectSKUGroupByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU group: ${response.statusText}`);
    }

    const result: ApiResponse<SKUGroup> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group");
    }

    return result.Data;
  }

  async getSKUGroupView(): Promise<SKUGroup[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectSKUGroupView`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU group view: ${response.statusText}`);
    }

    const result: ApiResponse<SKUGroup[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group view");
    }

    return result.Data;
  }

  async createSKUGroup(skuGroup: Partial<SKUGroup>): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKUGroup/CreateSKUGroup`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(skuGroup),
    });

    if (!response.ok) {
      throw new Error(`Failed to create SKU group: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to create SKU group");
    }

    return result.Data;
  }

  async updateSKUGroup(skuGroup: Partial<SKUGroup>): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKUGroup/UpdateSKUGroup`, {
      method: "PUT",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(skuGroup),
    });

    if (!response.ok) {
      throw new Error(`Failed to update SKU group: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to update SKU group");
    }

    return result.Data;
  }

  async deleteSKUGroup(uid: string): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/DeleteSKUGroup?UID=${uid}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to delete SKU group: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to delete SKU group");
    }

    return result.Data;
  }

  // SKU Group Types
  async getAllSKUGroupTypes(
    request: PagingRequest
  ): Promise<ApiResponse<PagedResponse<SKUGroupType>>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectAllSKUGroupTypeDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(request),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group types: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUGroupType>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group types");
    }

    return result;
  }

  async getSKUGroupTypeByUID(uid: string): Promise<SKUGroupType> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectSKUGroupTypeByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU group type: ${response.statusText}`);
    }

    const result: ApiResponse<SKUGroupType> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group type");
    }

    return result.Data;
  }

  async getSKUGroupTypeView(): Promise<SKUGroupType[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectSKUGroupTypeView`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group type view: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroupType[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU group type view"
      );
    }

    return result.Data;
  }

  async createSKUGroupType(groupType: Partial<SKUGroupType>): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/CreateSKUGroupType`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(groupType),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to create SKU group type: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to create SKU group type");
    }

    return result.Data;
  }

  async updateSKUGroupType(groupType: Partial<SKUGroupType>): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/UpdateSKUGroupType`,
      {
        method: "PUT",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify(groupType),
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to update SKU group type: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to update SKU group type");
    }

    return result.Data;
  }

  async deleteSKUGroupType(uid: string): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/DeleteSKUGroupTypeByUID?UID=${uid}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to delete SKU group type: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to delete SKU group type");
    }

    return result.Data;
  }
}

export const skuGroupService = new SKUGroupService();
