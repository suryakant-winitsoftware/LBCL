import {
  FilterCriteria,
  PagingRequest,
  SortCriteria,
} from "@/types/common.types";
import {
  Product,
  ProductWithAttributes,
  CreateProductWithAttributesRequest,
  UpdateProductRequest,
  ProductAttribute,
} from "@/types/product.types";
import { getAuthHeaders } from "@/lib/auth-service";

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

class ProductService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // Get product by UID
  async getProductByUID(uid: string): Promise<Product> {
    const response = await fetch(
      `${this.baseURL}/Product/SelectProductByUID?UID=${uid}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch product: ${response.statusText}`);
    }

    const result: ApiResponse<Product> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch product");
    }

    return result.Data;
  }

  // Get all products with paging
  async getAllProducts(
    request: PagingRequest
  ): Promise<PagedResponse<Product>> {
    const response = await fetch(`${this.baseURL}/Product/SelectProductsAll`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify(request),
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch products: ${response.statusText}`);
    }

    const result: ApiResponse<PagedResponse<Product>> = await response.json();
    if (!result.IsSuccess) {
      throw new Error(result.ErrorMessage || "Failed to fetch products");
    }

    return result.Data;
  }

  // Create product with flexible attributes
  async createProductWithAttributes(
    request: CreateProductWithAttributesRequest
  ): Promise<string> {
    try {
      // Step 1: Create the base product
      const productResponse = await fetch(
        `${this.baseURL}/Product/CreateProduct`,
        {
          method: "POST",
          headers: {
            ...getAuthHeaders(),
            "Content-Type": "application/json",
            Accept: "application/json",
          },
          body: JSON.stringify({
            UID: request.ProductCode, // Use ProductCode as UID
            OrgUID: request.OrgUID,
            ProductCode: request.ProductCode,
            ProductName: request.ProductName,
            ProductAliasName: request.ProductAliasName || request.ProductName,
            LongName: request.LongName || request.ProductName,
            DisplayName: request.DisplayName || request.ProductName,
            BaseUOM: request.BaseUOM,
            FromDate: request.FromDate,
            ToDate: request.ToDate,
            IsActive: request.IsActive,
            CreatedBy: request.CreatedBy,
            ModifiedBy: request.ModifiedBy,
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString(),
          }),
        }
      );

      if (!productResponse.ok) {
        const errorData = await productResponse.json();
        throw new Error(
          errorData.ErrorMessage ||
            `Failed to create product: ${productResponse.statusText}`
        );
      }

      const productResult: ApiResponse<number> = await productResponse.json();
      if (!productResult.IsSuccess) {
        throw new Error(
          productResult.ErrorMessage || "Failed to create product"
        );
      }

      // Step 2: Create attributes if provided
      if (request.Attributes && request.Attributes.length > 0) {
        for (const attr of request.Attributes) {
          await this.createProductAttribute({
            ...attr,
            ProductCode: request.ProductCode,
            CreatedBy: request.CreatedBy,
            ModifiedBy: request.ModifiedBy,
          });
        }
      }

      return request.ProductCode; // Return the product code/UID
    } catch (error) {
      console.error("Error creating product with attributes:", error);
      throw error;
    }
  }

  // Create product attribute
  async createProductAttribute(attribute: ProductAttribute): Promise<boolean> {
    // Note: This endpoint needs to be implemented in the backend
    // For now, we'll prepare the structure
    const response = await fetch(
      `${this.baseURL}/ProductAttributes/CreateProductAttribute`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json",
        },
        body: JSON.stringify({
          UID: `${attribute.ProductCode}_${
            attribute.HierarchyType
          }_${Date.now()}`,
          ProductCode: attribute.ProductCode,
          HierachyType: attribute.HierarchyType, // Note: Backend uses "Hierachy" spelling
          HierachyCode: attribute.HierarchyCode,
          HierachyValue: attribute.HierarchyValue,
          CreatedBy: attribute.CreatedBy || "SYSTEM",
          ModifiedBy: attribute.ModifiedBy || "SYSTEM",
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
        }),
      }
    );

    // If the endpoint doesn't exist yet, we'll handle it gracefully
    if (response.status === 404) {
      console.warn(
        "ProductAttributes API not yet implemented. Attribute creation skipped."
      );
      return true; // Continue without error
    }

    if (!response.ok) {
      throw new Error(
        `Failed to create product attribute: ${response.statusText}`
      );
    }

    const result: ApiResponse<number> = await response.json();
    return result.IsSuccess;
  }

  // Get product attributes
  async getProductAttributes(productCode: string): Promise<ProductAttribute[]> {
    const request: PagingRequest = {
      PageNumber: 1,
      PageSize: 100,
      FilterCriterias: [
        {
          Name: "ProductCode",
          Value: productCode,
        },
      ],
      SortCriterias: [],
      IsCountRequired: false,
    };

    const response = await fetch(
      `${this.baseURL}/ProductAttributes/SelectProductAttributesAll`,
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
        `Failed to fetch product attributes: ${response.statusText}`
      );
    }

    const result: ApiResponse<PagedResponse<ProductAttribute>> =
      await response.json();
    if (!result.IsSuccess) {
      throw new Error(
        result.ErrorMessage || "Failed to fetch product attributes"
      );
    }

    return result.Data.PagedData;
  }

  // Get product with attributes
  async getProductWithAttributes(
    productCode: string
  ): Promise<ProductWithAttributes> {
    try {
      // Fetch product details
      const product = await this.getProductByUID(productCode);

      // Fetch attributes
      const attributes = await this.getProductAttributes(productCode);

      return {
        ...product,
        Attributes: attributes,
      };
    } catch (error) {
      console.error("Error fetching product with attributes:", error);
      throw error;
    }
  }

  // Update product
  async updateProduct(request: UpdateProductRequest): Promise<boolean> {
    const response = await fetch(`${this.baseURL}/Product/UpdateProduct`, {
      method: "PUT",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({
        ...request,
        ModifiedTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString(),
      }),
    });

    if (!response.ok) {
      throw new Error(`Failed to update product: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    return result.IsSuccess;
  }

  // Delete product
  async deleteProduct(uid: string): Promise<boolean> {
    const response = await fetch(
      `${this.baseURL}/Product/DeleteProduct?UID=${uid}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json",
        },
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to delete product: ${response.statusText}`);
    }

    const result: ApiResponse<number> = await response.json();
    return result.IsSuccess;
  }

  // Helper methods
  buildFilterCriteria(filters: Record<string, any>): FilterCriteria[] {
    const criteria: FilterCriteria[] = [];

    Object.entries(filters).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== "") {
        criteria.push({
          Name: key,
          Value: Array.isArray(value)
            ? JSON.stringify(value)
            : value.toString(),
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
        Direction: sortDirection === "asc" ? "Asc" : "Desc",
      },
    ];
  }
}

export const productService = new ProductService();
