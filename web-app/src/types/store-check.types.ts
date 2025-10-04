// Store Check Report Types and Interfaces

// Sales Order interfaces matching backend response
export interface SalesOrder {
  id: number;
  uid: string;
  salesOrderNumber?: string;
  draftOrderNumber?: string;
  companyUID?: string;
  orgUID?: string;
  distributionChannelUID?: string;
  deliveredByOrgUID?: string;
  storeUID: string;
  status: string;
  orderType: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  deliveredDateTime?: string;
  customerPO?: string;
  currencyUID?: string;
  paymentType?: string;
  totalAmount: number;
  totalDiscount: number;
  totalTax: number;
  netAmount: number;
  lineCount: number;
  qtyCount: number;
  totalFakeAmount?: number;
  referenceNumber?: string;
  source?: string;
  totalLineDiscount?: number;
  totalCashDiscount?: number;
  totalHeaderDiscount?: number;
  totalExciseDuty?: number;
  totalLineTax?: number;
  totalHeaderTax?: number;
  cashSalesCustomer?: string;
  cashSalesAddress?: string;
  jobPositionUID?: string;
  empUID?: string;
  beatHistoryUID?: string;
  routeUID?: string;
  storeHistoryUID?: string;
  totalCreditLimit?: number;
  availableCreditLimit?: number;
  latitude?: string;
  longitude?: string;
  isOffline?: boolean;
  creditDays?: number;
  notes?: string;
  deliveryInstructions?: string;
  remarks?: string;
  isTemperatureCheckEnabled?: boolean;
  alwaysPrintedFlag?: number;
  purchaseOrderNoRequiredFlag?: number;
  isWithPrintedInvoicesFlag?: boolean;
  isStockUpdateRequired?: boolean;
  isInvoiceGenerationRequired?: boolean;
  actionType?: number;
  ss?: number;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
}

export interface SalesOrderLine {
  id: number;
  uid: string;
  salesOrderUID: string;
  lineNumber: number;
  itemCode?: string;
  itemType?: string;
  basePrice?: number;
  unitPrice: number;
  fakeUnitPrice?: number;
  baseUOM?: string;
  uom?: string;
  uomConversionToBU?: number;
  recoUOM?: string;
  recoQty?: number;
  recoUOMConversionToBU?: number;
  recoQtyBU?: number;
  modelQtyBU?: number;
  qty: number;
  qtyBU?: number;
  vanQtyBU?: number;
  deliveredQty?: number;
  missedQty?: number;
  returnedQty?: number;
  totalAmount: number;
  totalDiscount?: number;
  lineTaxAmount?: number;
  prorataTaxAmount?: number;
  totalTax?: number;
  netAmount: number;
  netFakeAmount?: number;
  skuPriceUID?: string;
  prorataDiscountAmount?: number;
  lineDiscountAmount?: number;
  mrp?: number;
  costUnitPrice?: number;
  parentUID?: string;
  isPromotionApplied?: boolean;
  volume?: number;
  volumeUnit?: string;
  weight?: number;
  weightUnit?: string;
  stockType?: string;
  remarks?: string;
  skuUID?: string;
  approvedQty?: number;
  actionType?: number;
  ss?: number;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
  taxData?: string;
}

export interface SalesOrderViewModel {
  salesOrder: SalesOrder;
  salesOrderLines?: SalesOrderLine[];
  isNewOrder?: boolean;
  actionType?: number;
}

export interface SalesOrderInvoice {
  salesOrderUID: string;
  salesOrderNumber: string;
  invoiceNumber?: string;
  invoiceDate?: string;
  storeUID: string;
  storeName?: string;
  totalAmount: number;
  netAmount: number;
  status: string;
  orderDate: string;
}

export interface SalesOrderPrintView {
  salesOrderNumber: string;
  status: string;
  orderType: string;
  storeCode?: string;
  storeNumber?: string;
  storeName?: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  deliveredDateTime?: string;
  currencySymbol?: string;
  totalAmount: number;
  totalDiscount: number;
  totalTax: number;
  netAmount: number;
  lineCount: number;
  qtyCount: number;
  totalLineDiscount?: number;
  totalCashDiscount?: number;
  totalHeaderDiscount?: number;
  totalExciseDuty?: number;
  totalLineTax?: number;
  totalHeaderTax?: number;
  addressLine1?: string;
}

export interface SalesOrderLinePrintView {
  lineNumber: number;
  skuCode?: string;
  skuDescription?: string;
  unitPrice: number;
  uomConversionToBU?: number;
  recoQty?: number;
  qty: number;
  deliveredQty?: number;
  totalAmount: number;
  totalDiscount?: number;
  totalTax?: number;
  netAmount: number;
}

// API Request/Response Types
export interface PagingRequest {
  sortCriterias?: SortCriteria[];
  pageNumber: number;
  pageSize: number;
  filterCriterias?: FilterCriteria[];
  isCountRequired: boolean;
}

export interface SortCriteria {
  sortColumn: string;
  sortDirection: 'ASC' | 'DESC';
}

export interface FilterCriteria {
  filterColumn: string;
  filterValue: string;
  filterOperator: 'eq' | 'ne' | 'lt' | 'gt' | 'lte' | 'gte' | 'like' | 'in' | 'between';
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T> {
  data?: T;
  statusCode: number;
  isSuccess: boolean;
  errorMessage?: string;
  currentServerTime?: string;
}

// Store Check specific types
export interface StoreCheckReport {
  reportId: string;
  storeUID: string;
  storeName: string;
  storeCode: string;
  checkDate: string;
  empUID: string;
  empName: string;
  beatHistoryUID?: string;
  routeUID?: string;
  items: StoreCheckItem[];
  totalSKUs: number;
  totalValue: number;
  createdTime: string;
  status: 'Draft' | 'Completed' | 'Synced';
}

export interface StoreCheckItem {
  skuUID: string;
  skuCode: string;
  skuName: string;
  categoryUID?: string;
  categoryName?: string;
  storeQty: number;
  backStoreQty: number;
  totalQty: number;
  unitPrice: number;
  totalValue: number;
  uom: string;
  lastCheckDate?: string;
  variance?: number;
}

// Filter Options
export interface StoreCheckFilters {
  storeUID?: string;
  empUID?: string;
  startDate?: string;
  endDate?: string;
  status?: string;
  routeUID?: string;
}

// Dashboard Summary
export interface StoreCheckSummary {
  totalStoresChecked: number;
  totalSKUsAudited: number;
  totalValue: number;
  averageComplianceRate: number;
  topPerformingStores: StorePerformance[];
  lowStockAlerts: LowStockAlert[];
}

export interface StorePerformance {
  storeUID: string;
  storeName: string;
  complianceRate: number;
  totalChecks: number;
  lastCheckDate: string;
}

export interface LowStockAlert {
  storeUID: string;
  storeName: string;
  skuUID: string;
  skuName: string;
  currentStock: number;
  minimumStock: number;
  alertLevel: 'Critical' | 'Warning' | 'Low';
}