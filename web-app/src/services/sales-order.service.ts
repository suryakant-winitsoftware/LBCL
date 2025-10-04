import { API_CONFIG } from '@/utils/config';

export interface SalesOrder {
  UID: string;
  SalesOrderNumber: string;
  DraftOrderNumber?: string;
  CompanyUID?: string;
  OrgUID?: string;
  DistributionChannelUID?: string;
  DeliveredByOrgUID?: string;
  StoreUID: string;
  Status: string;
  OrderType: string;
  OrderDate: string;
  ExpectedDeliveryDate?: string;
  DeliveredDateTime?: string;
  CustomerPO?: string;
  CurrencyUID?: string;
  PaymentType?: string;
  TotalAmount: number;
  TotalDiscount: number;
  TotalTax: number;
  NetAmount: number;
  LineCount: number;
  QtyCount: number;
  TotalFakeAmount?: number;
  ReferenceNumber?: string;
  Source?: string;
  TotalLineDiscount?: number;
  TotalCashDiscount?: number;
  TotalHeaderDiscount?: number;
  TotalExciseDuty?: number;
  TotalLineTax?: number;
  TotalHeaderTax?: number;
  CashSalesCustomer?: string;
  CashSalesAddress?: string;
  ReferenceUID?: string;
  ReferenceType?: string;
  JobPositionUID?: string;
  EmpUID?: string;
  BeatHistoryUID?: string;
  RouteUID?: string;
  StoreHistoryUID?: string;
  TotalCreditLimit?: number;
  AvailableCreditLimit?: number;
  Latitude?: string;
  Longitude?: string;
  IsOffline?: boolean;
  CreditDays?: number;
  Notes?: string;
  DeliveryInstructions?: string;
  Remarks?: string;
  VehicleUID?: string;
  IsStockUpdateRequired?: boolean;
  IsInvoiceGenerationRequired?: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface SalesOrderLine {
  UID: string;
  SalesOrderUID: string;
  SalesOrderLineUID?: string;
  LineNumber: number;
  ItemCode?: string;
  ItemType?: string;
  ItemName?: string;
  BasePrice?: number;
  UnitPrice: number;
  FakeUnitPrice?: number;
  BaseUOM?: string;
  UoM?: string;
  UOMConversionToBU?: number;
  RecoUOM?: string;
  RecoQty?: number;
  RecoUOMConversionToBU?: number;
  RecoQtyBU?: number;
  ModelQtyBU?: number;
  Qty: number;
  QtyBU?: number;
  VanQtyBU?: number;
  DeliveredQty?: number;
  MissedQty?: number;
  ReturnedQty?: number;
  TotalAmount: number;
  TotalDiscount?: number;
  LineTaxAmount?: number;
  ProrataTaxAmount?: number;
  TotalTax?: number;
  NetAmount: number;
  NetFakeAmount?: number;
  SKUPriceListUID?: string;
  SKUPriceUID?: string;
  ProrataDiscountAmount?: number;
  LineDiscountAmount?: number;
  MRP?: number;
  CostUnitPrice?: number;
  ParentUID?: string;
  IsPromotionApplied?: boolean;
  Volume?: number;
  VolumeUnit?: string;
  Weight?: number;
  WeightUnit?: string;
  StockType?: string;
  Remarks?: string;
  TotalCashDiscount?: number;
  TotalExciseDuty?: number;
  SKUUID?: string;
  ApprovedQty?: number;
}

export interface SalesOrderViewModel {
  SalesOrder: SalesOrder;
  SalesOrderLines?: SalesOrderLine[];
}

class SalesOrderService {
  private baseUrl = API_CONFIG.BASE_URL;

  private getAuthToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('auth_token');
    }
    return null;
  }

  private getHeaders(): HeadersInit {
    const token = this.getAuthToken();
    return {
      'Content-Type': 'application/json',
      Accept: 'application/json',
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(`HTTP error! status: ${response.status}, message: ${errorText}`);
    }
    return response.json();
  }

  // Get all sales orders with pagination
  async getAllSalesOrders(pageNumber: number = 1, pageSize: number = 10) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderDetailsAll?pageNumber=${pageNumber}&pageSize=${pageSize}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales orders:', error);
      throw error;
    }
  }

  // Get sales orders by route UID
  async getSalesOrdersByRoute(routeUID: string, pageNumber: number = 1, pageSize: number = 100) {
    try {
      // Route information is stored in beat_history_uid field, not route_uid
      // Format: "RTMERCHAND282_2025_09_15" contains route info
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderDetailsAll?pageNumber=${pageNumber}&pageSize=${pageSize}&filterCriteria=BeatHistoryUID:${routeUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales orders by route:', error);
      throw error;
    }
  }

  // Get sales orders by employee
  async getSalesOrdersByEmployee(empUID: string, pageNumber: number = 1, pageSize: number = 100) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderDetailsAll?pageNumber=${pageNumber}&pageSize=${pageSize}&filterCriteria=EmpUID:${empUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales orders by employee:', error);
      throw error;
    }
  }

  // Get sales orders by beat history
  async getSalesOrdersByBeatHistory(beatHistoryUID: string, pageNumber: number = 1, pageSize: number = 100) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderDetailsAll?pageNumber=${pageNumber}&pageSize=${pageSize}&filterCriteria=BeatHistoryUID:${beatHistoryUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales orders by beat history:', error);
      throw error;
    }
  }

  // Get specific sales order by UID
  async getSalesOrderByUID(salesOrderUID: string) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderByUID?SalesOrderUID=${salesOrderUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales order:', error);
      throw error;
    }
  }

  // Get sales order lines
  async getSalesOrderLines(salesOrderUID: string) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderLinesBySalesOrderUID?salesOrderUID=${salesOrderUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales order lines:', error);
      throw error;
    }
  }

  // Get sales order invoices
  async getSalesOrderInvoices(storeUID?: string) {
    try {
      const url = storeUID 
        ? `${this.baseUrl}/SalesOrder/GetAllSalesOrderInvoices?storeUID=${storeUID}`
        : `${this.baseUrl}/SalesOrder/GetAllSalesOrderInvoices`;
      
      const response = await fetch(url, {
        method: 'GET',
        headers: this.getHeaders(),
      });
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales order invoices:', error);
      throw error;
    }
  }

  // Get sales order print view
  async getSalesOrderPrintView(salesOrderUID: string) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderPrintView?SalesOrderUID=${salesOrderUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales order print view:', error);
      throw error;
    }
  }

  // Get sales order line print view
  async getSalesOrderLinePrintView(salesOrderUID: string) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderLinePrintView?SalesOrderUID=${salesOrderUID}`,
        {
          method: 'GET',
          headers: this.getHeaders(),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error fetching sales order line print view:', error);
      throw error;
    }
  }

  // Update sales order status
  async updateSalesOrderStatus(salesOrderStatus: any) {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/UpdateSalesOrderStatus`,
        {
          method: 'PUT',
          headers: this.getHeaders(),
          body: JSON.stringify(salesOrderStatus),
        }
      );
      return this.handleResponse(response);
    } catch (error) {
      console.error('Error updating sales order status:', error);
      throw error;
    }
  }
}

const salesOrderService = new SalesOrderService();
export default salesOrderService;