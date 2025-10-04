// UOM Types Service - For generic UOM types from uom_type table
// This is what should be used in product creation dropdowns

import { getAuthHeaders } from "@/lib/auth-service";
import { PagingRequest } from "@/types/common.types";

export interface UOMType {
  Id: number;
  UID: string;
  Name: string;
  Label: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
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

class UOMTypesService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";
  private _cachedUOMTypes: UOMType[] = [];
  private _lastCacheTime: number = 0;
  private readonly CACHE_DURATION = 300000; // 5 minutes

  /**
   * Get all UOM types from uom_type table (generic UOM types for product creation)
   */
  async getAllUOMTypes(): Promise<UOMType[]> {
    const now = Date.now();

    // Return cached data if fresh
    if (
      this._cachedUOMTypes.length > 0 &&
      now - this._lastCacheTime < this.CACHE_DURATION
    ) {
      return this._cachedUOMTypes;
    }

    try {
      const request: PagingRequest = {
        PageNumber: 0,
        PageSize: 0,
        FilterCriterias: [],
        SortCriterias: [],
        IsCountRequired: false,
      };

      const response = await fetch(
        `${this.baseURL}/UOMType/SelectAllUOMTypeDetails`,
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
        throw new Error(`UOM Types API failed: ${response.statusText}`);
      }

      const result: ApiResponse<PagedResponse<UOMType>> = await response.json();

      if (!result.IsSuccess) {
        throw new Error(`UOM Types API error: ${result.ErrorMessage}`);
      }

      const uomTypes = result.Data?.PagedData || [];

      // Cache the results
      this._cachedUOMTypes = uomTypes;
      this._lastCacheTime = now;

      console.log(`✅ Loaded ${uomTypes.length} UOM Types from uom_type table`);

      // Log detailed information for debugging
      if (uomTypes.length > 0) {
        console.log(
          "[UOM Service] Sample UOM Types loaded:",
          uomTypes.slice(0, 5).map((u) => ({
            UID: u.UID,
            Name: u.Name,
            Label: u.Label,
          }))
        );
      } else {
        console.warn("[UOM Service] No UOM Types returned from API!");
      }

      return uomTypes;
    } catch (error) {
      console.error("Failed to load UOM Types:", error);
      // Return empty array instead of hardcoded fallback
      return [];
    }
  }

  /**
   * Get base UOM types (typically smaller units)
   */
  async getBaseUOMTypes(): Promise<UOMType[]> {
    const allTypes = await this.getAllUOMTypes();

    // Return all types for now - let user choose any as base UOM
    // Previously this was filtering too restrictively
    console.log(
      `[UOM Service] Returning ${allTypes.length} types for Base UOM dropdown`
    );
    return allTypes;

    // OLD CODE - Too restrictive filtering
    // const baseUnitCodes = ["EA", "KG", "G", "L", "ML", "PCS"];
    // return allTypes.filter(
    //   (type) =>
    //     baseUnitCodes.includes(type.UID) ||
    //     baseUnitCodes.includes(type.Name.toUpperCase())
    // );
  }

  /**
   * Get outer UOM types (typically packaging units)
   */
  async getOuterUOMTypes(): Promise<UOMType[]> {
    const allTypes = await this.getAllUOMTypes();

    // Return all types for now - let user choose any as outer UOM
    // Previously this was filtering too restrictively
    console.log(
      `[UOM Service] Returning ${allTypes.length} types for Outer UOM dropdown`
    );
    return allTypes;

    // OLD CODE - Too restrictive filtering
    // const packagingUnitCodes = ["BOX", "CASE", "CTN", "CARTON", "PALLET"];
    // return allTypes.filter(
    //   (type) =>
    //     packagingUnitCodes.includes(type.UID) ||
    //     packagingUnitCodes.includes(type.Name.toUpperCase())
    // );
  }

  /**
   * Search UOM types by name or code
   */
  async searchUOMTypes(searchTerm: string): Promise<UOMType[]> {
    const allTypes = await this.getAllUOMTypes();

    if (!searchTerm || searchTerm.trim() === "") {
      return allTypes;
    }

    const term = searchTerm.toLowerCase().trim();
    return allTypes.filter(
      (type) =>
        type.UID.toLowerCase().includes(term) ||
        type.Name.toLowerCase().includes(term) ||
        type.Label?.toLowerCase().includes(term)
    );
  }

  /**
   * Get UOM type by UID/Code
   */
  async getUOMTypeByCode(code: string): Promise<UOMType | null> {
    const allTypes = await this.getAllUOMTypes();
    return (
      allTypes.find(
        (type) => type.UID === code || type.Name === code || type.Label === code
      ) || null
    );
  }

  /**
   * Clear cache to force fresh data load
   */
  clearCache(): void {
    this._cachedUOMTypes = [];
    this._lastCacheTime = 0;
  }

  /**
   * Format UOM type for display
   */
  formatUOMType(uomType: UOMType): string {
    return `${uomType.UID} - ${uomType.Name}`;
  }
}

export const uomTypesService = new UOMTypesService();
