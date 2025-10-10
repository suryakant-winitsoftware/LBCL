// PromotionV3 Type Definitions - Minimal version for Next.js

// Product Selection Types
export interface ProductSelection {
  selectionType: 'all' | 'hierarchy' | 'specific';
  hierarchySelections: { [key: string]: string[] };
  specificProducts: Array<string | ProductInfo>;
  excludedProducts: Array<string | ProductInfo>;
  selectedFinalProducts?: Array<string | ProductInfo>;
  productQuantities?: { [productUid: string]: number };
  
  // Legacy fields for backward compatibility
  type?: "all" | "hierarchy" | "specific";
  brand?: string[];
  category?: string[];
  subCategory?: string[];
  skuGroup?: string[];
  sku?: string[];
}

// Product Info for UI display
export interface ProductInfo {
  UID: string;
  Code: string;
  Name: string;
  hierarchyPath?: string;
}

// Footprint Types
export interface Footprint {
  type: "all" | "hierarchy" | "specific";
  dynamicHierarchy?: { [key: string]: string[] };
  selectedOrgs?: string[];
  finalOrgUID?: string;
  companyUID?: string;
  organizationUID?: string;
  selectedStoreGroups?: string[];
  selectedBranches?: string[];
  selectedStores?: string[];
  selectedCustomers?: string[];
  selectedSalesmen?: string[];
  specificStores?: string[];
  organization?: string[];
  location?: string[];
  branch?: string[];
  storeGroup?: string[];
  route?: string[];
  salesPerson?: string[];
  stores?: string[];
  selectedCountries?: string[];
  selectedDivisions?: string[];
  locationHierarchy?: {
    countries: string[];
    divisions: string[];
  };
  // Legacy fields
  selectionType?: string;
  organizations?: string[];
  storeGroups?: string[];
  branches?: string[];
  salespeople?: string[];
  excludedStores?: string[];
  excludedSalespeople?: string[];
}

// FOC Items Types
export interface FOCItem {
  itemCode: string;
  itemName: string;
  qty: number;
  uom: string;
}

export interface FOCItems {
  guaranteed: FOCItem[];
  choice: {
    maxSelections: number;
    items: FOCItem[];
  };
}

// Compulsory Items for Buy Criteria
export interface CompulsoryItem {
  itemCode: string;
  itemName: string;
  minQty: number;
}

// Promotion Slab for tiered promotions
export interface PromotionSlab {
  slabNo?: number;
  minQty: number | null;
  maxQty: number | null;
  offerType: "PERCENTAGE" | "FIXED" | "FOC";
  offerValue?: number;
  discountPercent?: number | null;
  discountAmount?: number | null;
}

// Multi-Product Promotion Configuration
export interface ProductPromotionConfig {
  // Core fields (required)
  productId: string;
  productName: string;
  promotionType?: 'IQFD' | 'IQPD' | 'IQXF' | 'BQXF';
  isActive: boolean;
  
  // Product selection (optional)
  productSelection?: ProductSelection;
  productQuantities?: { [productUid: string]: number };
  selectedProducts?: any[]; // All selected products for this config
  productAttributes?: any[]; // Product attribute selection
  
  // Quantity thresholds
  quantityThreshold?: number; // For IQFD/IQPD
  buyQty?: number; // For IQXF/BQXF
  
  // Discount fields (match individual promotion field names)
  discountAmount?: number; // For IQFD
  discountPercent?: number; // For IQPD (not discountPercentage!)
  maxDiscountAmount?: number; // Cap for IQPD
  
  // Free item fields
  freeQty?: number; // For IQXF/BQXF
  freeItemId?: string; // For BQXF only
  freeItemName?: string; // For BQXF display
  freeItemUOM?: string; // For BQXF
  freeItemUID?: string; // For BQXF
  
  // FOC configuration
  focProducts?: any[];
  focSelectedProducts?: any[];
  focProductQuantities?: { [productUid: string]: number };
  focProductSelection?: ProductSelection;
  focProductAttributes?: any[];
  
  // Slab configuration
  hasSlabs?: boolean;
  slabs?: PromotionSlab[];
  
  // Additional fields for backward compatibility
  maxApplications?: number;
}

// Main V3 Form Data Interface
export interface PromotionV3FormData {
  // Basic Information
  promotionName: string;
  promotionCode: string;
  promotionId?: string;
  remarks?: string;
  category?: string;
  level: 'instant' | 'invoice' | '';
  format: string;
  orderType: 'INVOICE' | 'LINE';
  validFrom: string;
  validUpto: string;
  priority: number;
  promoTitle?: string;
  promoMessage?: string;
  
  // Product Selection
  productSelection?: ProductSelection;
  productSelectionMode?: 'all' | 'hierarchy' | 'specific';
  specificProducts?: Array<{
    UID: string;
    Code: string;
    Name: string;
    productId?: string;
    productCode?: string;
    productName?: string;
  }>;
  
  // Product Attributes for refined targeting
  productAttributes?: Array<{
    type: string;
    code: string;
    value: string;
    level: number;
  }>;
  
  // Final products derived from product attributes
  finalAttributeProducts?: Array<{
    UID: string;
    Code: string;
    Name: string;
    GroupName?: string;
    GroupTypeName?: string;
    MRP?: number;
    IsActive?: boolean;
  }>;
  
  // Footprint
  footprint?: Footprint;
  
  // Configuration
  productConfigs: ProductPromotionConfig[];
  
  // Volume Caps
  volumeCaps?: {
    enabled: boolean;
    overallCap: {
      type: "value" | "quantity" | "count";
      value: number;
      consumed: number;
    };
    invoiceCap: {
      maxDiscountValue: number;
      maxQuantity: number;
      maxApplications: number;
    };
    periodCaps: Array<{
      periodType: "daily" | "weekly" | "monthly" | "custom";
      customDays?: number;
      capType: "value" | "quantity" | "percentage";
      capValue: number;
      startOffset?: number;
    }>;
    hierarchyCaps: Array<{
      hierarchyType: "organization" | "location" | "branch" | "store" | "salesperson";
      hierarchyId: string;
      hierarchyName: string;
      capType: "value" | "quantity";
      capValue: number;
      periodType?: "total" | "monthly" | "weekly" | "daily";
    }>;
    timeCaps?: Array<{
      period: string;
      capType: string;
      value: number;
      autoReset: boolean;
      isActive: boolean;
    }>;
    // Legacy fields
    dailyCap?: number;
    weeklyCap?: number;
    monthlyCap?: number;
    customerCap?: number;
  };
  
  // Additional Configuration Fields
  discountAmount?: number;
  discountPercent?: number;
  buyQty?: number;
  getQty?: number;
  freeQuantity?: number;
  buyQuantity?: number;
  minValue?: number;
  minQty?: number;
  minLineCount?: number;
  minBrandCount?: number;
  maxLineCount?: number;
  maxBrandCount?: number;
  requiredBrands?: string[];
  maxValue?: number;
  maxQty?: number;
  maxDiscountAmount?: number;
  maxDealCount?: number;
  maxFOCSelections?: number;
  selectionModel?: string;
  focProducts?: any[];
  focSelectedProducts?: any[];
  focProductQuantities?: { [key: string]: number };
  offerType?: string;
  maxApplicationsPerInvoice?: number;
  productPromotions?: ProductPromotionConfig[];
  
  // Additional Fields
  hasSlabs?: boolean;
  slabs?: PromotionSlab[];
  focItems?: FOCItems;
  compulsoryItems?: CompulsoryItem[];
  excludedItems?: string[];
  applyForExcludedItems?: boolean;
  isActive: boolean;
  isApprovalCreated?: boolean;
  approvalStatus?: string;
}

// API Response Types
export interface PromotionV3Response {
  success: boolean;
  data?: PromotionV3FormData;
  error?: string;
}

export interface ProductHierarchyResponse {
  success: boolean;
  data?: any[];
  error?: string;
}

export interface StoreHierarchyResponse {
  success: boolean;
  data?: any[];
  error?: string;
}