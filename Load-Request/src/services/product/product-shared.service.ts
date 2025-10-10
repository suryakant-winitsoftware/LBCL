import { getAuthHeaders } from "@/lib/auth-service";

// SharedObjects Product Model (matches backend database)
export interface SharedProduct {
  product_id?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  org_code: string;
  product_code: string;
  product_name: string;
  is_active: boolean;
  BaseUOM: string;
}

export interface ProductConfig {
  SKUConfigId?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  ProductCode: string;
  DistributionChannelOrgCode: string;
  CanBuy: boolean;
  CanSell: boolean;
  BuyingUOM: string;
  SellingUOM: string;
  IsActive: boolean;
}

export interface ProductUOM {
  ProductUOMId?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  ProductCode: string;
  Code: string;
  Name: string;
  Label: string;
  BarCode: string;
  IsBaseUOM: boolean;
  IsOuterUOM: boolean;
  Multiplier: number;
}

export interface ProductMasterData {
  Products: SharedProduct[];
  ProductConfigs?: ProductConfig[];
  ProductUOMs?: ProductUOM[];
}

class ProductSharedService {
  private baseURL =
    process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";

  // Get all products
  async getAllProducts(): Promise<SharedProduct[]> {
    const response = await fetch(`${this.baseURL}/Products/GetProductsAll`, {
      method: "GET",
      headers: {
        ...getAuthHeaders(),
        Accept: "application/json"
      }
    });

    if (!response.ok) {
      throw new Error(`Failed to fetch products: ${response.statusText}`);
    }

    return await response.json();
  }

  // Get product by product code
  async getProductByCode(productCode: string): Promise<SharedProduct> {
    const response = await fetch(
      `${
        this.baseURL
      }/Products/GetProductsByProductCode?productCode=${encodeURIComponent(
        productCode
      )}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch product: ${response.statusText}`);
    }

    return await response.json();
  }

  // Create product
  async createProduct(
    product: Omit<SharedProduct, "product_id">
  ): Promise<SharedProduct> {
    const now = new Date().toISOString();

    const productData = {
      ...product,
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };

    const response = await fetch(`${this.baseURL}/Products/CreateProduct`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(productData)
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(
        errorData.message || `Failed to create product: ${response.statusText}`
      );
    }

    return await response.json();
  }

  // Update product
  async updateProduct(product: SharedProduct): Promise<boolean> {
    const now = new Date().toISOString();

    const updateData = {
      ...product,
      ModifiedTime: now,
      ServerModifiedTime: now
    };

    const response = await fetch(`${this.baseURL}/Products/UpdateProduct`, {
      method: "PUT",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(updateData)
    });

    if (!response.ok) {
      throw new Error(`Failed to update product: ${response.statusText}`);
    }

    const result = await response.text();
    return result === "Update successfully";
  }

  // Delete product
  async deleteProduct(productCode: string): Promise<boolean> {
    const response = await fetch(
      `${this.baseURL}/Products/DeleteProduct?productCode=${encodeURIComponent(
        productCode
      )}`,
      {
        method: "DELETE",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to delete product: ${response.statusText}`);
    }

    const result = await response.text();
    return result === "Deleted successfully.";
  }

  // Get filtered products
  async getFilteredProducts(
    productCode?: string,
    productName?: string,
    createdTime?: Date,
    modifiedTime?: Date
  ): Promise<SharedProduct[]> {
    const params = new URLSearchParams();
    if (productCode) params.append("product_code", productCode);
    if (productName) params.append("product_name", productName);
    if (createdTime) params.append("CreatedTime", createdTime.toISOString());
    if (modifiedTime) params.append("ModifiedTime", modifiedTime.toISOString());

    const response = await fetch(
      `${this.baseURL}/Products/GetProductsFiltered?${params.toString()}`,
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
        `Failed to fetch filtered products: ${response.statusText}`
      );
    }

    return await response.json();
  }

  // Get paged products
  async getPagedProducts(
    pageNumber: number,
    pageSize: number
  ): Promise<SharedProduct[]> {
    const response = await fetch(
      `${this.baseURL}/Products/GetProductsPaged?pageNumber=${pageNumber}&pageSize=${pageSize}`,
      {
        method: "GET",
        headers: {
          ...getAuthHeaders(),
          Accept: "application/json"
        }
      }
    );

    if (!response.ok) {
      throw new Error(`Failed to fetch paged products: ${response.statusText}`);
    }

    return await response.json();
  }

  // Get master data with optional configs and UOMs
  async getProductMasterData(
    isProductConfigRequired: boolean = false,
    isProductUOMRequired: boolean = false
  ): Promise<ProductMasterData> {
    const params = new URLSearchParams();
    params.append(
      "isProductConfigRequired",
      isProductConfigRequired.toString()
    );
    params.append("isProductUOMRequired", isProductUOMRequired.toString());

    const response = await fetch(
      `${this.baseURL}/Products/GetProductsMasterData?${params.toString()}`,
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
        `Failed to fetch product master data: ${response.statusText}`
      );
    }

    return await response.json();
  }

  // Product Config methods
  async createProductConfig(
    config: Omit<ProductConfig, "SKUConfigId">
  ): Promise<ProductConfig> {
    const now = new Date().toISOString();

    const configData = {
      ...config,
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };

    const response = await fetch(
      `${this.baseURL}/Products/CreateProductConfig`,
      {
        method: "POST",
        headers: {
          ...getAuthHeaders(),
          "Content-Type": "application/json",
          Accept: "application/json"
        },
        body: JSON.stringify(configData)
      }
    );

    if (!response.ok) {
      throw new Error(
        `Failed to create product config: ${response.statusText}`
      );
    }

    return await response.json();
  }

  // Product UOM methods
  async createProductUOM(
    uom: Omit<ProductUOM, "ProductUOMId">
  ): Promise<ProductUOM> {
    const now = new Date().toISOString();

    const uomData = {
      ...uom,
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now
    };

    const response = await fetch(`${this.baseURL}/Products/CreateProductUOM`, {
      method: "POST",
      headers: {
        ...getAuthHeaders(),
        "Content-Type": "application/json",
        Accept: "application/json"
      },
      body: JSON.stringify(uomData)
    });

    if (!response.ok) {
      throw new Error(`Failed to create product UOM: ${response.statusText}`);
    }

    return await response.json();
  }
}

export const productSharedService = new ProductSharedService();
