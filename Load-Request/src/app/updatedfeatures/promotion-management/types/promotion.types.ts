// Comprehensive Promotion Types matching backend structure

// Base Promotion interface
export interface IPromotion {
  UID?: string;
  CompanyUID?: string;
  OrgUID: string;
  Code?: string;
  Name: string;
  Remarks?: string;
  Type?: string; // DISCOUNT, BOGO, PERCENTAGE, etc.
  PromoFormat?: string; // PERCENTAGE, FIXED, STANDARD
  Category?: string;
  ValidFrom: string;
  ValidUpto: string;
  Priority: number;
  Status?: string;
  HasSlabs?: boolean;
  HasFactSheet?: boolean;
  PromoTitle?: string;
  PromoMessage?: string;
  IsActive?: boolean;
  IsApprovalCreated?: boolean;
  CreatedByEmpUID?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ActionType?: string;

  // V3 Fields
  Level?: string; // "instant" or "invoice"
  Format?: string; // IQFD, IQPD, BYVALUE, etc.
  OrderType?: string; // "INVOICE" or "LINE"
  MaxDealCount?: number;
  MaxDiscountAmount?: number;
  SelectionModel?: string; // "any" or "all"
  MultiProductEnabled?: boolean;
  ContributionLevel1?: number;
  ContributionLevel2?: number;
  ContributionLevel3?: number;
}

// Extended Promotion View
export interface IPromotionView extends IPromotion {
  id?: string; // Optional - only used for Redux Toolkit entity adapter, not sent to backend
  ApplyForExcludedItems?: boolean;
  EndDateRemarks?: string;
  EndDateUpdatedByEmpUID?: string;
  SchemeExtendHistory?: ISchemeExtendHistory;
  SS?: number; // Synchronization status field
  ServerAddTime?: string;
  ServerModifiedTime?: string;
}

// Scheme Extension History
export interface ISchemeExtendHistory {
  SchemeType: string;
  SchemeUid: string;
  NewDate?: Date;
  ActionType?: string;
}

// Product/SKU related types
export interface ISKU {
  UID: string;
  Code: string;
  Name: string;
  AliasName?: string;
  LongName?: string;
  BaseUOM?: string;
  OuterUOM?: string;
  ParentUID?: string;
  SupplierOrgUID?: string;
  IsFocusSKU?: boolean;
}

export interface ISKUGroup {
  UID: string;
  SKUGroupTypeUID: string;
  Code: string;
  Name: string;
  ParentUID?: string;
  ParentName?: string;
  ItemLevel?: number;
}

export interface ISKUGroupType {
  UID: string;
  Code: string;
  Name: string;
  ParentUID?: string;
  ItemLevel?: number;
  AvailableForFilter?: boolean;
}

// Promotion Order Items (what triggers the promotion)
export interface IPromoOrderItem {
  UID?: string;
  PromoOrderUID?: string;
  ItemCriteriaType?: string; // SKU, SKUGROUP, BRAND, CATEGORY, etc.
  ItemCriteriaSelected?: string; // Comma-separated UIDs or ALL
  ItemQty?: number;
  ItemAmount?: number;
  MinQty?: number;
  MaxQty?: number;
  ItemUOM?: string;
  IsCompulsory?: boolean;
  ActionType?: string;
}

export interface IPromoOrderItemView extends IPromoOrderItem {
  ItemName?: string;
}

// Promotion Orders (conditions/requirements)
export interface IPromoOrder {
  UID: string;
  PromotionUID: string;
  OrderNo?: number;
  MinOrderValue?: number;
  MaxOrderValue?: number;
  MinOrderQty?: number;
  MaxOrderQty?: number;
  // New properties from backend model
  SelectionModel?: string;
  MinDealCount?: number;
  MaxDealCount?: number;
  MinPurchaseQty?: number;
  MinPurchaseValue?: number;
  ActionType?: string; // Add, Update, Delete
}

// Promotion Offers (what you get)
export interface IPromoOffer {
  UID: string;
  PromotionUID: string;
  OfferNo: number;
  OfferType: string; // DISCOUNT, FOC, PERCENTAGE
  ActionType?: string; // Add, Update, Delete
}

// Promotion Offer Items (specific benefits)
export interface IPromoOfferItem {
  UID?: string;
  PromoOfferUID?: string;
  ItemCriteriaType?: string;
  ItemCriteriaSelected?: string;
  ItemQty?: number;
  ItemAmount?: number;
  ItemUOM?: string;
  ActionType?: string;
}

export interface IPromoOfferItemView extends IPromoOfferItem {
  ItemName?: string;
}

// Promotion Conditions
export interface IPromoCondition {
  UID?: string;
  PromotionUID?: string;
  ConditionType?: string;
  ConditionValue?: string;
  MinValue?: number;
  MaxValue?: number;
  ActionType?: string;
}

export interface IPromoConditionView extends IPromoCondition {
  ConditionDescription?: string;
}

// Item to Promotion Mapping
export interface IItemPromotionMap {
  UID: string;
  SKUType: string; // SKU, SKUGROUP, ALL
  SKUTypeUID: string;
  PromotionUID: string;
}

export interface IItemPromotionMapView extends IItemPromotionMap {
  ItemName?: string;
  ItemCode?: string;
}

// Scheme mappings for organizational hierarchy
export interface ISchemeBranch {
  UID: string;
  BranchUID: string;
  LinkedItemUID: string; // Usually PromotionUID
  LinkedItemType: string; // Usually "PROMOTION"
  ActionType?: string;
}

export interface ISchemeOrg {
  UID: string;
  OrgUID: string;
  LinkedItemUID: string;
  LinkedItemType: string;
  ActionType?: string;
}

export interface ISchemeBroadClassification {
  UID: string;
  BroadClassificationLineUID: string;
  LinkedItemUID: string;
  LinkedItemType: string;
  ActionType?: string;
}

// Complete Promotion Master View (what gets sent to backend)
export interface IPromoMasterView {
  IsNew: boolean;
  PromotionView: IPromotionView;
  PromoOrderViewList: IPromoOrder[];
  PromoOrderItemViewList: IPromoOrderItemView[];
  PromoOfferViewList: IPromoOffer[];
  PromoOfferItemViewList: IPromoOfferItemView[];
  PromoConditionViewList: IPromoConditionView[];
  ItemPromotionMapViewList: IItemPromotionMap[];
  SchemeBranches: ISchemeBranch[];
  SchemeOrgs: ISchemeOrg[];
  SchemeBroadClassifications: ISchemeBroadClassification[];
  
  // V3 Reconstructed Data (returned by GetPromotionDetailsByUID)
  ReconstructedProductSelections?: IProductSelectionSummary;
  ReconstructedFootprintData?: IFootprintDataSummary;
  
  // Additional fields from React version
  ProductPromotionConfigs?: IProductPromotionConfig[];
  PromotionVolumeCap?: IPromotionVolumeCap;
}

// Promotion Slabs for tiered promotions
export interface IPromotionSlab {
  minQty: number;
  maxQty: number;
  offerType: "PERCENTAGE" | "FIXED" | "FOC";
  offerValue: number;
}

// API Response wrapper
export interface IApiResponse<T> {
  IsSuccess: boolean;
  Data: T;
  Message?: string;
  StatusCode?: number;
}

// Promotion Types enumeration
export const PROMOTION_TYPES = {
  DISCOUNT: "DISCOUNT",
  BOGO: "BOGO",
  BUNDLE: "BUNDLE",
  PERCENTAGE: "PERCENTAGE",
  FIXED: "FIXED",
  FOC: "FOC"
} as const;

// Promotion Formats
export const PROMOTION_FORMATS = {
  IQFD: "IQFD", // Item Quantity for Discount
  IQPD: "IQPD", // Item Quantity for Percentage Discount  
  BQXF: "BQXF", // Buy Quantity X for Free
  BYVALUE: "BYVALUE", // Buy by Value
  PERCENTAGE: "PERCENTAGE",
  FIXED: "FIXED"
} as const;

export type PromotionType = keyof typeof PROMOTION_TYPES;
export type PromotionFormat = keyof typeof PROMOTION_FORMATS;

// V3 Reconstructed Data Interfaces - missing from original migration
export interface ISelectedProductInfo {
  Code: string;
  Name: string;
  MinQuantity: number;
  IsCompulsory: boolean;
}

export interface ICompulsoryItemInfo {
  ProductUID: string;
  ProductCode?: string;
  ProductName: string;
  MinQuantity: number;
}

export interface IProductSelectionSummary {
  TotalProducts: number;
  ExcludedProducts: number;
  NetProducts: number;
  ProductsByHierarchy: { [key: string]: number };
  DynamicMetrics: { [key: string]: any };
  SelectionType: string;
  HierarchySelections: { [key: string]: string[] };
  SpecificProducts: ISelectedProductInfo[];
  ExcludedProductsList: string[];
  SelectedFinalProducts: string[];
  ProductQuantities: { [key: string]: number };
  CompulsoryItems: ICompulsoryItemInfo[];
}

export interface IFootprintDataSummary {
  SelectionType: string;
  DynamicHierarchySelections: { [key: string]: string[] };
  OrganizationSelections: string[];
  StoreGroupSelections: string[];
  BranchSelections: string[];
  StoreSelections?: string[];
  CustomerSelections?: string[];
  SalesmanSelections: string[];
  ExcludedStores: string[];
  ExcludedSalesPersons: string[];
  TotalCoverage?: string;
  CoverageByType?: { [key: string]: number };
}

// Product Promotion Config interface (from React version)
export interface IProductPromotionConfig {
  UID?: string;
  PromotionUID?: string;
  ConfigType?: string;
  ConfigName?: string;
  ConfigData?: string;
  IsActive?: boolean;
  Priority?: number;
  ProductGroupUID?: string;
  DiscountAmount?: number;
  DiscountPercentage?: number;
  MaximumDiscountAmount?: number;
  MinimumPurchaseAmount?: number;
  MinimumPurchaseQuantity?: number;
  MaximumDealCount?: number;
  BuyQuantity?: number;
  GetQuantity?: number;
  GetUnitsFree?: number;
  FreeQuantity?: number;
  FreeProductId?: string;
  MinimumInvoiceValue?: number;
  MaximumInvoiceValue?: number;
  MinimumTotalQuantity?: number;
  MaximumTotalQuantity?: number;
  InvoiceDiscountType?: string;
  InvoiceDiscountAmount?: number;
  InvoiceDiscountPercentage?: number;
  MaximumApplicationsPerInvoice?: number;
  OrderType?: string;
  DiscountType?: string;
  OfferType?: string;
  MinimumLineCount?: number;
  MaximumLineCount?: number;
  MinimumBrandCount?: number;
  MaximumBrandCount?: number;
  ConfigOrder?: number;
  SlabOrder?: number;
  MinSlabQuantity?: number;
  MaxSlabQuantity?: number;
  SlabDiscountAmount?: number;
  SlabDiscountPercentage?: number;
  PromotionType?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerCreatedTime?: string;
  ServerModifiedTime?: string;
  Deleted?: boolean;
}

// Volume Cap interface (from React version)
export interface IPromotionVolumeCap {
  UID?: string;
  PromotionUID?: string;
  Enabled?: boolean;
  OverallCapType?: string;
  OverallCapValue?: number;
  OverallCapConsumed?: number;
  InvoiceMaxDiscountValue?: number;
  InvoiceMaxQuantity?: number;
  InvoiceMaxApplications?: number;
  TotalCapEnabled?: boolean;
  TotalCapValue?: number;
  TotalCapUnit?: string;
  DailyCapEnabled?: boolean;
  DailyCapValue?: number;
  WeeklyCapEnabled?: boolean;
  WeeklyCapValue?: number;
  MonthlyCapEnabled?: boolean;
  MonthlyCapValue?: number;
  StoreCapEnabled?: boolean;
  StoreCapValue?: number;
  RegionCapEnabled?: boolean;
  RegionCapValue?: number;
  CustomerCapEnabled?: boolean;
  CustomerCapValue?: number;
  CapResetFrequency?: string;
  CapResetDay?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

