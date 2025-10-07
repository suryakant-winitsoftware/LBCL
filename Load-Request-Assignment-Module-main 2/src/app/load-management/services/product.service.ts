import { getAuthHeaders } from "@/lib/auth-service";

export interface ProductFilterCriteria {
  pageNumber: number;
  pageSize: number;
  filterCriterias: any[];
  isCountRequired: boolean;
}

export interface Product {
  SKUUID: string;
  SKUCode: string;
  SKUName: string;
  SKULongName: string;
  BaseUOM: string;
  OuterUOM: string;
  IsActive: boolean;
  HSNCode?: string;
  ProductCategoryId?: number;
  Price?: number;
  Stock?: number;
  Weight?: number;
  Volume?: number;
}

export interface ProductResponse {
  Data: Product[];
  TotalCount: number;
  IsSuccess: boolean;
  Message?: string;
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
  PageNumber: number;
  PageSize: number;
}

class ProductService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000";
  }

  async fetchProducts(
    criteria?: Partial<ProductFilterCriteria>
  ): Promise<ProductResponse> {
    const defaultCriteria = {
      pageNumber: 1,
      pageSize: 150,
      filterCriterias: [],
      isCountRequired: true,
      ...criteria
    };

    try {
      const response = await fetch(
        `${this.baseUrl}/api/SKU/SelectAllSKUDetailsWebView`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            ...getAuthHeaders()
          },
          body: JSON.stringify(defaultCriteria)
        }
      );

      if (!response.ok) {
        throw new Error(`Failed to fetch products: ${response.statusText}`);
      }

      const data = await response.json();
      console.log("Product API Raw Response:", data);
      console.log("Response type:", typeof data);
      console.log("Response keys:", Object.keys(data || {}));

      // Handle different possible response structures
      let resultData = [];
      let totalCount = 0;

      // Check if the response has a nested Data property
      if (data && data.Data) {
        console.log("Found data.Data, type:", typeof data.Data);
        console.log("data.Data keys:", Object.keys(data.Data));

        if (data.Data.PagedData) {
          resultData = data.Data.PagedData;
          totalCount = data.Data.TotalCount || 0;
          console.log("Using data.Data.PagedData");
        } else if (Array.isArray(data.Data)) {
          resultData = data.Data;
          totalCount = data.Data.length;
          console.log("Using data.Data array");
        } else if (data.Data.Data) {
          resultData = data.Data.Data;
          totalCount = data.Data.TotalCount || 0;
          console.log("Using data.Data.Data");
        }
      } else if (data && data.PagedData) {
        resultData = data.PagedData;
        totalCount = data.TotalCount || 0;
        console.log("Using data.PagedData");
      } else if (Array.isArray(data)) {
        resultData = data;
        totalCount = data.length;
        console.log("Response is array");
      } else {
        console.log("Unknown response structure");
      }

      console.log("Final result data length:", resultData.length);
      console.log("First item:", resultData[0]);

      return {
        Data: resultData,
        TotalCount: totalCount,
        IsSuccess: true
      };
    } catch (error) {
      console.error("Error fetching products:", error);
      return {
        Data: [],
        TotalCount: 0,
        IsSuccess: false,
        Message: error instanceof Error ? error.message : "Unknown error"
      };
    }
  }

  async searchProducts(
    searchTerm: string,
    pageSize: number = 50
  ): Promise<ProductResponse> {
    const criteria = {
      pageNumber: 1,
      pageSize,
      filterCriterias: [
        {
          columnName: "SKUName",
          filterOperator: "Contains",
          filterValue: searchTerm
        }
      ],
      isCountRequired: true
    };

    return this.fetchProducts(criteria);
  }

  async getProductByCode(skuCode: string): Promise<Product | null> {
    const criteria = {
      pageNumber: 1,
      pageSize: 1,
      filterCriterias: [
        {
          columnName: "SKUCode",
          filterOperator: "Equals",
          filterValue: skuCode
        }
      ],
      isCountRequired: false
    };

    const response = await this.fetchProducts(criteria);
    return response.Data && response.Data.length > 0 ? response.Data[0] : null;
  }

  async getActiveProducts(
    pageNumber: number = 1,
    pageSize: number = 100
  ): Promise<ProductResponse> {
    const criteria = {
      pageNumber,
      pageSize,
      filterCriterias: [],
      isCountRequired: true
    };

    return this.fetchProducts(criteria);
  }

  async getProductsByCategory(
    categoryId: number,
    pageNumber: number = 1,
    pageSize: number = 50
  ): Promise<ProductResponse> {
    const criteria = {
      pageNumber,
      pageSize,
      filterCriterias: [
        {
          columnName: "ProductCategoryId",
          filterOperator: "Equals",
          filterValue: categoryId.toString()
        }
      ],
      isCountRequired: true
    };

    return this.fetchProducts(criteria);
  }
}

export const productService = new ProductService();
