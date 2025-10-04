import { PagingRequest } from "@/types/common.types";
import { getAuthHeaders } from "@/lib/auth-service";

export interface UOMType {
  Id: number;
  UID: string;
  Name: string;
  Label: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface SKUUOM {
  Id: number;
  UID: string;
  SKUUID: string;
  Code: string;
  Name: string;
  Label: string;
  Barcodes?: string;
  IsBaseUom?: boolean; // For backwards compatibility
  IsOuterUom?: boolean; // For backwards compatibility
  is_base_uom?: boolean; // Database field name
  is_outer_uom?: boolean; // Database field name
  Multiplier: number;
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

class UOMService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api";

  /**
   * Get all UOM types from database
   */
  async getUOMTypes(): Promise<UOMType[]> {
    const request: PagingRequest = {
      PageNumber: 0, // Try 0 to avoid pagination
      PageSize: 0, // Try 0 to get all records
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

    const responseText = await response.text();

    if (!response.ok) {
      // Return empty array instead of throwing
      return [];
    }

    try {
      const result: ApiResponse<PagedResponse<UOMType>> =
        JSON.parse(responseText);

      if (!result.IsSuccess) {
        return [];
      }

      return result.Data?.PagedData || [];
    } catch (e) {
      return [];
    }
  }

  /**
   * Get all SKU UOMs from the actual sku_uom table
   */
  async getSKUUOMs(): Promise<SKUUOM[]> {
    // Debug: Try without pagination to avoid OFFSET SQL error
    const request: PagingRequest = {
      PageNumber: 0, // Set to 0 to disable pagination
      PageSize: 0, // Set to 0 to get all records
      FilterCriterias: [],
      SortCriterias: [],
      IsCountRequired: false,
    };

    const response = await fetch(
      `${this.baseURL}/SKUUOM/SelectAllSKUUOMDetails`,
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

    const errorText = await response.text();

    if (!response.ok) {
      // Don't throw, return empty array to allow fallback
      return [];
    }

    let result: ApiResponse<PagedResponse<SKUUOM>>;
    try {
      result = JSON.parse(errorText);
    } catch (e) {
      throw new Error("Invalid response from server");
    }

    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch SKU UOMs");
    }

    return result.Data?.PagedData || [];
  }

  // Cache for UOM data to avoid multiple API calls
  private _cachedUOMs: SKUUOM[] | null = null;
  private _cachedUOMTypes: UOMType[] | null = null;
  private _cacheTimestamp: number = 0;
  private readonly CACHE_DURATION = 60000; // 1 minute cache

  private async getCachedSKUUOMs(): Promise<SKUUOM[]> {
    const now = Date.now();
    if (this._cachedUOMs && now - this._cacheTimestamp < this.CACHE_DURATION) {
      return this._cachedUOMs;
    }

    this._cachedUOMs = await this.getSKUUOMs();
    this._cacheTimestamp = now;
    return this._cachedUOMs;
  }

  private async getCachedUOMTypes(): Promise<UOMType[]> {
    const now = Date.now();
    if (
      this._cachedUOMTypes &&
      now - this._cacheTimestamp < this.CACHE_DURATION
    ) {
      return this._cachedUOMTypes;
    }

    this._cachedUOMTypes = await this.getUOMTypes();
    this._cacheTimestamp = now;
    return this._cachedUOMTypes;
  }

  /**
   * Get base UOMs (filtered by IsBaseUom flag from database)
   */
  async getBaseUOMs(): Promise<
    { code: string; name: string; label: string }[]
  > {
    try {
      // First try cached SKU UOM table
      const allUOMs = await this.getCachedSKUUOMs();

      if (allUOMs && allUOMs.length > 0) {
        const baseUOMs = allUOMs.filter(
          (uom) => uom.IsBaseUom === true || uom.is_base_uom === true
        );

        // Get unique UOM codes (avoid duplicates)
        const uniqueUOMs = new Map<string, SKUUOM>();
        baseUOMs.forEach((uom) => {
          if (uom.Code && !uniqueUOMs.has(uom.Code)) {
            uniqueUOMs.set(uom.Code, uom);
          }
        });

        const uniqueBaseUOMs = Array.from(uniqueUOMs.values());

        if (uniqueBaseUOMs.length > 0) {
          return uniqueBaseUOMs.map((uom) => ({
            code: uom.Code,
            name: uom.Name || uom.Code,
            label: uom.Label || uom.Name || uom.Code,
          }));
        }
      }
    } catch (error) {
      // Try fallback to UOM types
    }

    // Use UOM types and filter for base UOMs (smaller units)
    try {
      const uomTypes = await this.getCachedUOMTypes();

      if (uomTypes.length > 0) {
        // For base UOMs, prefer smaller units like EA
        const baseUOMTypes = uomTypes.filter((uom) => {
          const name = (uom.Name || "").toLowerCase();
          const label = (uom.Label || "").toLowerCase();
          const uid = (uom.UID || "").toLowerCase();
          return (
            name.includes("ea") || uid.includes("ea") || label.includes("ea")
          );
        });

        if (baseUOMTypes.length > 0) {
          return baseUOMTypes.map((uom) => ({
            code: uom.UID,
            name: uom.Name,
            label: uom.Label,
          }));
        }

        // If no EA found, return first UOM type as base
        return [
          {
            code: uomTypes[0].UID,
            name: uomTypes[0].Name,
            label: uomTypes[0].Label,
          },
        ];
      }
    } catch (error) {
      // Silent fail
    }

    return [];
  }

  /**
   * Get outer UOMs (filtered by IsOuterUom flag from database)
   */
  async getOuterUOMs(): Promise<
    { code: string; name: string; label: string }[]
  > {
    try {
      // First try cached SKU UOM table
      const allUOMs = await this.getCachedSKUUOMs();

      if (allUOMs && allUOMs.length > 0) {
        const outerUOMs = allUOMs.filter(
          (uom) => uom.IsOuterUom === true || uom.is_outer_uom === true
        );

        // Get unique UOM codes (avoid duplicates)
        const uniqueUOMs = new Map<string, SKUUOM>();
        outerUOMs.forEach((uom) => {
          if (uom.Code && !uniqueUOMs.has(uom.Code)) {
            uniqueUOMs.set(uom.Code, uom);
          }
        });

        const uniqueOuterUOMs = Array.from(uniqueUOMs.values());

        if (uniqueOuterUOMs.length > 0) {
          return uniqueOuterUOMs.map((uom) => ({
            code: uom.Code,
            name: uom.Name || uom.Code,
            label: uom.Label || uom.Name || uom.Code,
          }));
        }
      }
    } catch (error) {
      // Try fallback to UOM types
    }

    // Use UOM types and filter for outer UOMs (larger units)
    try {
      const uomTypes = await this.getCachedUOMTypes();

      if (uomTypes.length > 0) {
        // For outer UOMs, prefer larger units like Case, Carton, CTN
        const outerUOMTypes = uomTypes.filter((uom) => {
          const name = (uom.Name || "").toLowerCase();
          const label = (uom.Label || "").toLowerCase();
          const uid = (uom.UID || "").toLowerCase();
          return (
            name.includes("case") ||
            name.includes("carton") ||
            name.includes("ctn") ||
            uid.includes("case") ||
            uid.includes("ctn") ||
            label.includes("ctn")
          );
        });

        if (outerUOMTypes.length > 0) {
          return outerUOMTypes.map((uom) => ({
            code: uom.UID,
            name: uom.Name,
            label: uom.Label,
          }));
        }

        // If no specific outer types, exclude EA and return others
        const nonEATypes = uomTypes.filter((uom) => {
          const uid = (uom.UID || "").toLowerCase();
          return !uid.includes("ea");
        });

        if (nonEATypes.length > 0) {
          return nonEATypes.map((uom) => ({
            code: uom.UID,
            name: uom.Name,
            label: uom.Label,
          }));
        }

        // If all are EA, return all types
        return uomTypes.map((uom) => ({
          code: uom.UID,
          name: uom.Name,
          label: uom.Label,
        }));
      }
    } catch (error) {
      // Silent fail
    }

    return [];
  }

  /**
   * Get all unique UOM codes and names for dropdowns
   */
  async getUOMOptions(): Promise<
    { code: string; name: string; label: string }[]
  > {
    try {
      // Try to get cached UOM types first
      const uomTypes = await this.getCachedUOMTypes();

      if (uomTypes && uomTypes.length > 0) {
        return uomTypes.map((uom) => ({
          code: uom.Name,
          name: uom.Name,
          label: uom.Label || uom.Name,
        }));
      }
    } catch (error) {
      // Try fallback to SKU UOMs
    }

    // Fallback to cached SKU UOMs
    try {
      const skuUOMs = await this.getCachedSKUUOMs();

      const uniqueUOMs = new Map<string, { name: string; label: string }>();

      skuUOMs.forEach((uom) => {
        if (uom.Code && !uniqueUOMs.has(uom.Code)) {
          uniqueUOMs.set(uom.Code, {
            name: uom.Name || uom.Code,
            label: uom.Label || uom.Name || uom.Code,
          });
        }
      });

      return Array.from(uniqueUOMs.entries()).map(([code, data]) => ({
        code,
        name: data.name,
        label: data.label,
      }));
    } catch (error) {
      // Silent fail
      // Return empty array - no hardcoded defaults
      return [];
    }
  }
}

export const uomService = new UOMService();
