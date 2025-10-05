import { FilterCriteria, SortCriteria } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";

// Paging Request Interface
interface PagingRequest {
  PageNumber: number;
  PageSize: number;
  IsCountRequired: boolean;
  FilterCriterias: FilterCriteria[];
  SortCriterias: SortCriteria[];
}

// SKU Group Type Interfaces
export interface SKUGroupType {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  ParentUID?: string;
  ItemLevel: number;
  AvailableForFilter: boolean;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
}

export interface SKUGroupTypeView {
  UID: string;
  Code: string;
  Name: string;
  ItemLevel: number;
}

export interface SKUAttributeLevel {
  [key: string]: any;
}

// SKU Group Interfaces
export interface SKUGroup {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  SkuGroupTypeUID: string;
  ParentUID?: string;
  ParentName?: string;
  ItemLevel: number;
  SupplierOrgUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
}

export interface SKUGroupView {
  UID: string;
  Code: string;
  Name: string;
  SkuGroupTypeUID: string;
  ItemLevel: number;
}

export interface SKUGroupSelectionItem {
  UID: string;
  Code: string;
  Name: string;
  ParentUID?: string;
  ItemLevel: number;
}

export interface SKUGroupItemView {
  [key: string]: any;
}

// SKU to Group Mapping Interface
export interface SKUToGroupMapping {
  Id: number;
  UID: string;
  SKUUID: string;
  SKUGroupUID: string;
  CreatedBy: string;
  CreatedTime: string;
  ModifiedBy: string;
  ModifiedTime: string;
  ServerAddTime: string;
  ServerModifiedTime: string;
}

// API Response Interfaces
export interface ApiResponse<T> {
  Data: T;
  StatusCode: number;
  IsSuccess: boolean;
  ErrorMessage?: string;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
  CurrentPage: number;
  PageSize: number;
}

class HierarchyService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // ============ SKU GROUP TYPE MANAGEMENT ============

  async getAllSKUGroupTypes(
    request: PagingRequest
  ): Promise<PagedResponse<SKUGroupType>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectAllSKUGroupTypeDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
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

    return result.Data;
  }

  async getSKUGroupTypeByUID(uid: string): Promise<SKUGroupType> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectSKUGroupTypeByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
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

  async createSKUGroupType(groupType: Partial<SKUGroupType>): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/CreateSKUGroupType`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(groupType)
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

  async updateSKUGroupType(groupType: SKUGroupType): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/UpdateSKUGroupType`,
      {
        method: "PUT",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(groupType)
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
          Accept: "application/json"
        }
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

  async getSKUGroupTypeView(): Promise<SKUGroupTypeView[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectSKUGroupTypeView`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group type view: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroupTypeView[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU group type view"
      );
    }

    return result.Data;
  }

  async getSKUAttributeDDL(): Promise<SKUAttributeLevel> {
    const response = await fetch(
      `${this.baseURL}/SKUGroupType/SelectSKUAttributeDDL`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU attribute DDL: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUAttributeLevel> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU attribute DDL"
      );
    }

    return result.Data;
  }

  // ============ SKU GROUP MANAGEMENT ============

  async getAllSKUGroups(
    request: PagingRequest
  ): Promise<PagedResponse<SKUGroup>> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectAllSKUGroupDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU groups: ${response.statusText}`);
    }

    const result: ApiResponse<PagedResponse<SKUGroup>> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU groups");
    }

    return result.Data;
  }

  async getSKUGroupByUID(uid: string): Promise<SKUGroup> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectSKUGroupByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
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

  async createSKUGroup(group: Partial<SKUGroup>): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKUGroup/CreateSKUGroup`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(group)
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

  async insertSKUGroupHierarchy(type: string, uid: string): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/InsertSKUGroupHierarchy?type=${type}&uid=${uid}`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to insert SKU group hierarchy: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to insert SKU group hierarchy"
      );
    }

    return result.Data;
  }

  async updateSKUGroup(group: SKUGroup): Promise<number> {
    const response = await fetch(`${this.baseURL}/SKUGroup/UpdateSKUGroup`, {
      method: "PUT",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(group)
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
          Accept: "application/json"
        }
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

  async getSKUGroupView(): Promise<SKUGroupView[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectSKUGroupView`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch SKU group view: ${response.statusText}`);
    }

    const result: ApiResponse<SKUGroupView[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU group view");
    }

    return result.Data;
  }

  async getSKUGroupSelectionItemBySKUGroupTypeUID(
    skuGroupTypeUID: string,
    parentUID?: string
  ): Promise<SKUGroupSelectionItem[]> {
    let url = `${this.baseURL}/SKUGroup/GetSKUGroupSelectionItemBySKUGroupTypeUID?skuGroupTypeUID=${skuGroupTypeUID}`;
    if (parentUID) {
      url += `&parentUID=${parentUID}`;
    }

    const response = await fetch(url, {
      method: "GET",
      headers: {
        ...getAuthHeaders(),
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group selection items: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroupSelectionItem[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU group selection items"
      );
    }

    return result.Data;
  }

  async getAllSKUGroupBySKUGroupTypeUID(
    skuGroupTypeUID: string
  ): Promise<SKUGroup[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/GetAllSKUGroupBySKUGroupTypeUID?skuGroupTypeUID=${skuGroupTypeUID}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU groups by type: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroup[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU groups by type"
      );
    }

    return result.Data;
  }

  async getAllSKUGroupItemViews(
    request: PagingRequest
  ): Promise<SKUGroupItemView[]> {
    const response = await fetch(
      `${this.baseURL}/SKUGroup/SelectAllSKUGroupItemViews`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU group item views: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroupItemView[]> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU group item views"
      );
    }

    return result.Data;
  }

  // ============ SKU TO GROUP MAPPING ============

  async getAllSKUToGroupMappings(
    request: PagingRequest
  ): Promise<PagedResponse<SKUToGroupMapping>> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/SelectAllSKUToGroupMappingDetails`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(request)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU to group mappings: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUToGroupMapping>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU to group mappings"
      );
    }

    return result.Data;
  }

  async getSKUToGroupMappingByUID(uid: string): Promise<SKUToGroupMapping> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/SelectSKUToGroupMappingByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to fetch SKU to group mapping: ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUToGroupMapping> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch SKU to group mapping"
      );
    }

    return result.Data;
  }

  async createSKUToGroupMapping(
    mapping: Partial<SKUToGroupMapping>
  ): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/CreateSKUToGroupMapping`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(mapping)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to create SKU to group mapping: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to create SKU to group mapping"
      );
    }

    return result.Data;
  }

  async updateSKUToGroupMapping(mapping: SKUToGroupMapping): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/UpdateSKUToGroupMapping`,
      {
        method: "PUT",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(mapping)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to update SKU to group mapping: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to update SKU to group mapping"
      );
    }

    return result.Data;
  }

  async deleteSKUToGroupMapping(uid: string): Promise<number> {
    const response = await fetch(
      `${this.baseURL}/SKUToGroupMapping/DeleteSKUToGroupMappingByUID?UID=${uid}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to delete SKU to group mapping: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to delete SKU to group mapping"
      );
    }

    return result.Data;
  }

  // ============ UTILITY METHODS ============

  buildFilterCriteria(filters: Record<string, any>): FilterCriteria[] {
    const criteria: FilterCriteria[] = [];

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        criteria.push({
          Name: key,
          Value: Array.isArray(value) ? JSON.stringify(value) : value.toString()
        });
      }
    });

    return criteria;
  }

  buildSortCriteria(
    sortField: string,
    sortDirection: "asc" | "desc"
  ): SortCriteria[] {
    return [
      {
        SortParameter: sortField,
        Direction: sortDirection === "asc" ? "Asc" : "Desc"
      }
    ];
  }

  buildPagingRequest(
    pageNumber: number = 1,
    pageSize: number = 10,
    filters: Record<string, any> = {},
    sortField?: string,
    sortDirection: "asc" | "desc" = "asc",
    isCountRequired: boolean = true
  ): PagingRequest {
    return {
      PageNumber: pageNumber,
      PageSize: pageSize,
      IsCountRequired: isCountRequired,
      FilterCriterias: this.buildFilterCriteria(filters),
      SortCriterias: sortField
        ? this.buildSortCriteria(sortField, sortDirection)
        : []
    };
  }
}

export const hierarchyService = new HierarchyService();
