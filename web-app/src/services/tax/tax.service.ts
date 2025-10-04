import { getAuthHeaders } from "@/lib/auth-service";

export interface ITax {
  Id?: number;
  UID: string;
  Name: string;
  LegalName?: string;
  Code: string;
  ApplicableAt?: string;
  TaxCalculationType?: string;
  BaseTaxRate: number;
  Status?: string;
  ValidFrom?: string;
  ValidUpto?: string;
  IsTaxOnTaxApplicable?: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export interface ITaxGroup {
  Id?: number;
  UID: string;
  Name: string;
  Code: string;
  ActionType?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export interface ITaxSelectionItem {
  Id?: number;
  IsSelected: boolean;
  TaxUID: string;
  TaxName: string;
  ActionType?: number;
}

export interface ITaxMaster {
  Tax: ITax;
  TaxSKUMapList?: any[];
}

export interface ITaxGroupTaxes {
  Id?: number;
  UID?: string;
  TaxGroupUID: string;
  TaxUID: string;
  ActionType: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface ITaxGroupMaster {
  TaxGroup: ITaxGroup;
  TaxGroupTaxes?: ITaxGroupTaxes[];
}

export interface ITaxSkuMap {
  Id?: number;
  UID: string;
  CompanyUID?: string;
  SKUUID: string;
  TaxUID: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
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

class TaxService {
  private baseUrl =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // Tax APIs (Working)
  async getTaxDetails(request: PagingRequest): Promise<PagedResponse<ITax>> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/GetTaxDetails`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(`Failed to fetch tax details: ${response.statusText}`);
      }

      const result: ApiResponse<PagedResponse<ITax>> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching tax details:", error);
      throw error;
    }
  }

  async getTaxByUID(uid: string): Promise<ITax | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/Tax/GetTaxByUID?UID=${uid}`,
        {
          method: "GET",
          headers: getAuthHeaders(),
        }
      );

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(`Failed to fetch tax by UID: ${response.statusText}`);
      }

      const result: ApiResponse<ITax> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching tax by UID:", error);
      throw error;
    }
  }

  async getTaxSelectionItems(uid: string): Promise<ITaxSelectionItem[]> {
    try {
      const response = await fetch(
        `${this.baseUrl}/Tax/GetTaxSelectionItems?UID=${uid}`,
        {
          method: "GET",
          headers: getAuthHeaders(),
        }
      );

      if (!response.ok) {
        throw new Error(
          `Failed to fetch tax selection items: ${response.statusText}`
        );
      }

      const result: ApiResponse<ITaxSelectionItem[]> = await response.json();
      return result.Data || [];
    } catch (error) {
      console.error("Error fetching tax selection items:", error);
      throw error;
    }
  }

  async selectTaxMasterViewByUID(uid: string): Promise<ITaxMaster | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/Tax/SelectTaxMasterViewByUID?UID=${uid}`,
        {
          method: "GET",
          headers: getAuthHeaders(),
        }
      );

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(
          `Failed to fetch tax master view: ${response.statusText}`
        );
      }

      const result: ApiResponse<ITaxMaster> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching tax master view:", error);
      throw error;
    }
  }

  // Tax Group APIs (Working)
  async getTaxGroupDetails(
    request: PagingRequest
  ): Promise<PagedResponse<ITaxGroup>> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/GetTaxGroupDetails`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(request),
      });

      if (!response.ok) {
        throw new Error(
          `Failed to fetch tax group details: ${response.statusText}`
        );
      }

      const result: ApiResponse<PagedResponse<ITaxGroup>> =
        await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching tax group details:", error);
      throw error;
    }
  }

  async getTaxGroupByUID(uid: string): Promise<ITaxGroup | null> {
    try {
      const response = await fetch(
        `${this.baseUrl}/Tax/GetTaxGroupByUID?UID=${uid}`,
        {
          method: "GET",
          headers: getAuthHeaders(),
        }
      );

      if (response.status === 404) {
        return null;
      }

      if (!response.ok) {
        throw new Error(
          `Failed to fetch tax group by UID: ${response.statusText}`
        );
      }

      const result: ApiResponse<ITaxGroup> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error fetching tax group by UID:", error);
      throw error;
    }
  }

  // Create/Update/Delete APIs (Currently have issues but including for future fixes)
  async createTax(tax: ITax): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/CreateTax`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(tax),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.ErrorMessage || "Failed to create tax");
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error creating tax:", error);
      throw error;
    }
  }

  async updateTax(tax: ITax): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/UpdateTax`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(tax),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.ErrorMessage || "Failed to update tax");
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error updating tax:", error);
      throw error;
    }
  }

  async deleteTax(uid: string): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/DeleteTax?UID=${uid}`, {
        method: "DELETE",
        headers: getAuthHeaders(),
      });

      if (!response.ok) {
        const error = await response.json();

        // Check for foreign key constraint violation
        if (
          error.ErrorMessage &&
          error.ErrorMessage.includes("foreign key constraint")
        ) {
          if (error.ErrorMessage.includes("tax_group_taxes")) {
            throw new Error(
              "Cannot delete this tax because it is assigned to one or more tax groups. Please remove this tax from all tax groups first, then try deleting again."
            );
          } else if (error.ErrorMessage.includes("tax_sku_map")) {
            throw new Error(
              "Cannot delete this tax because it is mapped to one or more SKUs. Please remove all SKU mappings for this tax first, then try deleting again."
            );
          } else {
            throw new Error(
              "Cannot delete this tax because it is being used by other records. Please remove all dependencies first, then try deleting again."
            );
          }
        }

        throw new Error(error.ErrorMessage || "Failed to delete tax");
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error deleting tax:", error);
      throw error;
    }
  }

  // Tax Master APIs
  async createTaxMaster(taxMaster: {
    Tax: ITax;
    TaxSKUMapList: ITaxSkuMap[];
  }): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/CreateTaxMaster`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(taxMaster),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.ErrorMessage || "Failed to create tax master");
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error creating tax master:", error);
      throw error;
    }
  }

  // Tax Group Master APIs
  async createTaxGroupMaster(taxGroupMaster: ITaxGroupMaster): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/CreateTaxGroupMaster`, {
        method: "POST",
        headers: getAuthHeaders(),
        body: JSON.stringify(taxGroupMaster),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.ErrorMessage || "Failed to create tax group master"
        );
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error creating tax group master:", error);
      throw error;
    }
  }

  async updateTaxGroupMaster(taxGroupMaster: ITaxGroupMaster): Promise<number> {
    try {
      const response = await fetch(`${this.baseUrl}/Tax/UpdateTaxGroupMaster`, {
        method: "PUT",
        headers: getAuthHeaders(),
        body: JSON.stringify(taxGroupMaster),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(
          error.ErrorMessage || "Failed to update tax group master"
        );
      }

      const result: ApiResponse<number> = await response.json();
      return result.Data;
    } catch (error) {
      console.error("Error updating tax group master:", error);
      throw error;
    }
  }
}

export const taxService = new TaxService();
