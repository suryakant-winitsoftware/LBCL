// Product Types with Flexible Hierarchy Support

export interface ProductAttribute {
  UID?: string;
  ProductCode: string;
  HierarchyType: string;  // Can be ANY label: Category, Brand, Department, Custom, etc.
  HierarchyCode: string;
  HierarchyValue: string;
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
}

export interface Product {
  UID: string;
  OrgUID: string;
  ProductCode: string;
  ProductName: string;
  ProductAliasName?: string;
  LongName?: string;
  DisplayName?: string;
  FromDate: string;
  ToDate: string;
  IsActive: boolean;
  BaseUOM: string;
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

export interface ProductWithAttributes extends Product {
  Attributes: ProductAttribute[];
}

export interface CreateProductRequest {
  ProductCode: string;
  ProductName: string;
  ProductAliasName?: string;
  LongName?: string;
  DisplayName?: string;
  OrgUID: string;
  BaseUOM: string;
  FromDate: string;
  ToDate: string;
  IsActive: boolean;
  CreatedBy: string;
  ModifiedBy: string;
}

export interface CreateProductWithAttributesRequest extends CreateProductRequest {
  Attributes: Omit<ProductAttribute, 'ProductCode' | 'UID' | 'CreatedTime' | 'ModifiedTime'>[];
}

export interface UpdateProductRequest extends Partial<CreateProductRequest> {
  UID: string;
  ModifiedBy: string;
}

// Dynamic hierarchy types - will be loaded from database
export type HierarchyType = string;

// UOM Options
export const UOM_OPTIONS = [
  { value: 'EA', label: 'Each (EA)' },
  { value: 'KG', label: 'Kilogram (KG)' },
  { value: 'G', label: 'Gram (G)' },
  { value: 'L', label: 'Liter (L)' },
  { value: 'ML', label: 'Milliliter (ML)' },
  { value: 'PCS', label: 'Pieces (PCS)' },
  { value: 'BOX', label: 'Box (BOX)' },
  { value: 'CASE', label: 'Case (CASE)' },
  { value: 'PALLET', label: 'Pallet (PALLET)' }
];