import { PagingRequest } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";
import { skuGroupService } from "@/services/sku-group.service";

// Real API response types based on actual backend
export interface SKUGroupType {
  Id: number;
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  ParentUID?: string;
  ItemLevel: number;
  AvailableForFilter: boolean;
  ShowInUI: boolean;
  ShowInTemplate: boolean;
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
}

export interface HierarchyOption {
  code: string;
  value: string;
  type: string;
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

class HierarchyService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  /**
   * Get ALL hierarchy types dynamically from database
   */
  async getHierarchyTypes(): Promise<SKUGroupType[]> {
    const request: PagingRequest = {
      PageNumber: 1,
      PageSize: 100,
      FilterCriterias: [],
      SortCriterias: [
        {
          SortParameter: "ItemLevel",
          Direction: "Asc"
        }
      ],
      IsCountRequired: false
    };

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
      const errorText = await response.text();
      throw new Error(
        `Failed to fetch hierarchy types: ${response.status} ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<SKUGroupType>> =
      await response.json();

    if (!result.IsSuccess) {
      return []; // Return empty array instead of throwing error
    }

    // Handle null or undefined PagedData
    if (!result.Data || !result.Data.PagedData) {
      return [];
    }

    return result.Data.PagedData;
  }

  /**
   * Get hierarchy options for ANY hierarchy type dynamically
   */
  async getHierarchyOptionsForType(
    hierarchyTypeUID: string
  ): Promise<HierarchyOption[]> {
    // Backend fixed to use LEFT JOIN so it will work for top-level items (Brands) with no parent
    const url = `${
      this.baseURL
    }/SKUGroup/GetAllSKUGroupBySKUGroupTypeUID?skuGroupTypeUID=${encodeURIComponent(
      hierarchyTypeUID
    )}`;

    const response = await fetch(url, {
      method: "GET",
      headers: {
        ...getAuthHeaders(),
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(
        `Failed to fetch hierarchy options: ${response.status} ${response.statusText}`
      );
    }

    const result: ApiResponse<SKUGroup[]> = await response.json();

    if (!result.IsSuccess) {
      return []; // Return empty array instead of throwing error
    }

    // Handle null or undefined Data
    if (!result.Data || result.Data.length === 0) {
      return [];
    }

    // Get the hierarchy type name from the first result or from cache
    const hierarchyTypes = await this.getHierarchyTypes();
    const hierarchyType = hierarchyTypes.find(
      (t) => t.UID === hierarchyTypeUID
    );
    const typeName = hierarchyType ? hierarchyType.Name : "Unknown";

    return result.Data.map((group) => ({
      code: group.Code,
      value: group.Name,
      type: typeName
    }));
  }

  /**
   * Get ALL hierarchy options grouped by type - completely dynamic
   */
  async getAllHierarchyOptions(): Promise<Record<string, HierarchyOption[]>> {
    try {
      // First get all hierarchy types dynamically
      const hierarchyTypes = await this.getHierarchyTypes();
      const results: Record<string, HierarchyOption[]> = {};

      // Initialize all results with empty arrays first
      hierarchyTypes.forEach((type) => {
        results[type.Name] = [];
      });

      // Fetch options for each hierarchy type dynamically
      for (let i = 0; i < hierarchyTypes.length; i++) {
        const type = hierarchyTypes[i];

        try {
          const options = await this.getHierarchyOptionsForType(type.UID);
          results[type.Name] = options;
        } catch (error) {
          results[type.Name] = [];
        }
      }

      return results;
    } catch (error) {
      return {};
    }
  }

  /**
   * Get options for a specific hierarchy type by name (completely dynamic)
   */
  async getOptionsByTypeName(typeName: string): Promise<HierarchyOption[]> {
    try {
      const hierarchyTypes = await this.getHierarchyTypes();
      const hierarchyType = hierarchyTypes.find((t) => t.Name === typeName);

      if (!hierarchyType) {
        return [];
      }

      return await this.getHierarchyOptionsForType(hierarchyType.UID);
    } catch (error) {
      return [];
    }
  }

  /**
   * Get child SKU Groups by parent UID for cascading selection
   */
  async getChildSKUGroups(parentUID: string): Promise<SKUGroup[]> {
    try {
      // Get all SKU groups (we'll filter client-side)
      const allGroups = await skuGroupService.getAllSKUGroups();

      // Filter groups by parent UID and convert to our SKUGroup type
      const childGroups: SKUGroup[] = allGroups
        .filter((group) => group.ParentUID === parentUID)
        .map((group) => ({
          Id: group.Id || 0,
          UID: group.UID,
          Code: group.Code,
          Name: group.Name,
          SKUGroupTypeUID: group.SKUGroupTypeUID,
          ParentUID: group.ParentUID || "",
          ItemLevel: group.ItemLevel,
          SupplierOrgUID: group.SupplierOrgUID || ""
        }));

      // Sort by name
      childGroups.sort((a, b) => a.Name.localeCompare(b.Name));

      return childGroups;
    } catch (error) {
      console.error("Error getting child SKU groups:", error);
      return [];
    }
  }

  /**
   * Search hierarchy options across all types
   */
  async searchHierarchyOptions(
    query: string,
    hierarchyTypeName?: string
  ): Promise<HierarchyOption[]> {
    const allOptions = await this.getAllHierarchyOptions();
    const results: HierarchyOption[] = [];

    for (const [typeName, options] of Object.entries(allOptions)) {
      if (hierarchyTypeName && typeName !== hierarchyTypeName) continue;

      const filtered = options.filter(
        (option) =>
          option.code.toLowerCase().includes(query.toLowerCase()) ||
          option.value.toLowerCase().includes(query.toLowerCase())
      );

      results.push(...filtered);
    }

    return results;
  }

  /**
   * Get the first hierarchy type (usually Brand or whatever is at level 1)
   */
  async getDefaultHierarchyType(): Promise<SKUGroupType | null> {
    try {
      const types = await this.getHierarchyTypes();
      return types.length > 0 ? types[0] : null;
    } catch (error) {
      return null;
    }
  }

  /**
   * Check if SKU Groups exist in the system
   */
  async checkSKUGroupsExist(): Promise<boolean> {
    try {
      // Check if any SKU Groups exist
      const allGroups = await skuGroupService.getAllSKUGroups();

      return allGroups.length > 0;
    } catch (error) {
      return false;
    }
  }

  /**
   * Get hierarchy validation status
   */
  async getHierarchyStatus(): Promise<{
    hasTypes: boolean;
    hasGroups: boolean;
    typeCount: number;
    groupCount: number;
    message: string;
  }> {
    try {
      const types = await this.getHierarchyTypes();
      const groups = await skuGroupService.getAllSKUGroups();

      const hasTypes = types.length > 0;
      const hasGroups = groups.length > 0;

      let message = "";
      if (!hasTypes) {
        message =
          "No SKU Group Types configured. Please configure hierarchy types in the admin panel.";
      } else if (!hasGroups) {
        message =
          "Hierarchy structure exists but no SKU Groups are configured. Please add SKU Groups through the admin panel.";
      } else {
        message = "Hierarchy is properly configured.";
      }

      return {
        hasTypes,
        hasGroups,
        typeCount: types.length,
        groupCount: groups.length,
        message
      };
    } catch (error) {
      return {
        hasTypes: false,
        hasGroups: false,
        typeCount: 0,
        groupCount: 0,
        message: "Error checking hierarchy status"
      };
    }
  }
}

export const hierarchyService = new HierarchyService();
