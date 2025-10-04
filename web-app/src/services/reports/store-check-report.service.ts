import { API_CONFIG } from "@/utils/config";
import {
  SalesOrder,
  SalesOrderLine,
  SalesOrderViewModel,
  SalesOrderInvoice,
  SalesOrderPrintView,
  SalesOrderLinePrintView,
  PagingRequest,
  PagedResponse,
  ApiResponse,
  StoreCheckReport,
  StoreCheckItem,
  StoreCheckFilters,
  StoreCheckSummary,
} from "@/types/store-check.types";

class StoreCheckReportService {
  private baseUrl = API_CONFIG.BASE_URL;

  // Helper method to get auth token
  private getAuthToken(): string | null {
    if (typeof window !== "undefined") {
      return localStorage.getItem("auth_token");
    }
    return null;
  }

  // Helper method to build headers with authentication
  private getHeaders(): HeadersInit {
    const token = this.getAuthToken();
    return {
      "Content-Type": "application/json",
      Accept: "application/json",
      ...(token && { Authorization: `Bearer ${token}` }),
    };
  }

  // Helper method to handle API responses
  private async handleResponse<T>(response: Response): Promise<T> {
    let responseText = '';
    try {
      responseText = await response.text();
      
      if (!response.ok) {
        // Try to parse error response
        let errorMessage = `HTTP error! status: ${response.status}`;
        try {
          const errorData = JSON.parse(responseText);
          errorMessage = errorData.title || errorData.message || errorData.error || errorMessage;
        } catch {
          // If not JSON, use the text as error message
          if (responseText) {
            errorMessage = responseText;
          }
        }
        throw new Error(errorMessage);
      }

      // Parse successful response
      const data = JSON.parse(responseText);
      
      // Log the raw response structure for debugging
      console.log("Service received raw response:", {
        hasData: data.Data !== undefined,
        hasPagedData: data.PagedData !== undefined,
        hasTotalCount: data.TotalCount !== undefined,
        isArray: Array.isArray(data),
        keys: Object.keys(data).slice(0, 10) // Show first 10 keys
      });
      
      // For getSalesOrders, we want to return the whole response to preserve paging info
      // Don't unwrap Data field for paged responses
      if (data.IsSuccess !== undefined || data.isSuccess !== undefined) {
        if (data.IsSuccess === false || data.isSuccess === false) {
          throw new Error(data.errorMessage || data.ErrorMessage || "Request failed");
        }
      }
      
      console.log("Returning entire response for further processing");
      return data;
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error(`Failed to parse response: ${responseText}`);
    }
  }

  // Get all sales orders with pagination and filtering
  async getSalesOrders(request: PagingRequest): Promise<PagedResponse<SalesOrderViewModel>> {
    try {
      const params = new URLSearchParams({
        pageNumber: request.pageNumber.toString(),
        pageSize: request.pageSize.toString(),
      });

      // Add sort criteria
      if (request.sortCriterias && request.sortCriterias.length > 0) {
        request.sortCriterias.forEach((criteria, index) => {
          params.append(`sortCriterias[${index}].sortColumn`, criteria.sortColumn);
          params.append(`sortCriterias[${index}].sortDirection`, criteria.sortDirection);
        });
      }

      // Add filter criteria
      if (request.filterCriterias && request.filterCriterias.length > 0) {
        request.filterCriterias.forEach((criteria, index) => {
          params.append(`filterCriterias[${index}].filterColumn`, criteria.filterColumn);
          params.append(`filterCriterias[${index}].filterValue`, criteria.filterValue);
          params.append(`filterCriterias[${index}].filterOperator`, criteria.filterOperator);
        });
      }

      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderDetailsAll?${params}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      return await this.handleResponse<PagedResponse<SalesOrderViewModel>>(response);
    } catch (error) {
      console.error("Error fetching sales orders:", error);
      throw error;
    }
  }

  // Get sales order by UID
  async getSalesOrderByUID(salesOrderUID: string): Promise<SalesOrder> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectSalesOrderByUID?SalesOrderUID=${salesOrderUID}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      const result = await this.handleResponse<any>(response);
      
      // Extract the actual data from the API response wrapper
      const salesOrderData = result.Data || result.data || result;
      
      // Transform PascalCase to match our interface expectations
      if (salesOrderData) {
        const transformedSalesOrder: SalesOrder = {
          id: salesOrderData.Id || salesOrderData.id || 0,
          uid: salesOrderData.UID || salesOrderData.uid || "",
          salesOrderNumber: salesOrderData.SalesOrderNumber || salesOrderData.salesOrderNumber,
          companyUID: salesOrderData.CompanyUID || salesOrderData.companyUID,
          orgUID: salesOrderData.OrgUID || salesOrderData.orgUID,
          storeUID: salesOrderData.StoreUID || salesOrderData.storeUID,
          status: salesOrderData.Status || salesOrderData.status,
          orderType: salesOrderData.OrderType || salesOrderData.orderType,
          orderDate: salesOrderData.OrderDate || salesOrderData.orderDate,
          expectedDeliveryDate: salesOrderData.ExpectedDeliveryDate || salesOrderData.expectedDeliveryDate,
          deliveredDateTime: salesOrderData.DeliveredDateTime || salesOrderData.deliveredDateTime,
          currencyUID: salesOrderData.CurrencyUID || salesOrderData.currencyUID,
          totalAmount: salesOrderData.TotalAmount || salesOrderData.totalAmount || 0,
          totalDiscount: salesOrderData.TotalDiscount || salesOrderData.totalDiscount || 0,
          totalTax: salesOrderData.TotalTax || salesOrderData.totalTax || 0,
          netAmount: salesOrderData.NetAmount || salesOrderData.netAmount || 0,
          lineCount: salesOrderData.LineCount || salesOrderData.lineCount || 0,
          qtyCount: salesOrderData.QtyCount || salesOrderData.qtyCount || 0,
          empUID: salesOrderData.EmpUID || salesOrderData.empUID,
          jobPositionUID: salesOrderData.JobPositionUID || salesOrderData.jobPositionUID,
          beatHistoryUID: salesOrderData.BeatHistoryUID || salesOrderData.beatHistoryUID,
          routeUID: salesOrderData.RouteUID || salesOrderData.routeUID,
          storeHistoryUID: salesOrderData.StoreHistoryUID || salesOrderData.storeHistoryUID,
          createdBy: salesOrderData.CreatedBy || salesOrderData.createdBy,
          createdTime: salesOrderData.CreatedTime || salesOrderData.createdTime,
          modifiedBy: salesOrderData.ModifiedBy || salesOrderData.modifiedBy,
          modifiedTime: salesOrderData.ModifiedTime || salesOrderData.modifiedTime,
          ss: salesOrderData.SS || salesOrderData.ss || 0,
        };
        
        return transformedSalesOrder;
      }
      
      throw new Error("No sales order data found in response");
    } catch (error) {
      console.error("Error fetching sales order by UID:", error);
      throw error;
    }
  }

  // Get sales order master data (with lines)
  async getSalesOrderMasterData(salesOrderUID: string): Promise<SalesOrderViewModel> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderMasterDataBySalesOrderUID?salesOrderUID=${salesOrderUID}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      return await this.handleResponse<SalesOrderViewModel>(response);
    } catch (error) {
      console.error("Error fetching sales order master data:", error);
      throw error;
    }
  }

  // Get sales order lines
  async getSalesOrderLines(salesOrderUID: string): Promise<SalesOrderLine[]> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderLinesBySalesOrderUID?salesOrderUID=${salesOrderUID}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      return await this.handleResponse<SalesOrderLine[]>(response);
    } catch (error) {
      console.error("Error fetching sales order lines:", error);
      throw error;
    }
  }

  // Get sales order invoices
  async getSalesOrderInvoices(storeUID?: string): Promise<SalesOrderInvoice[]> {
    try {
      const url = storeUID
        ? `${this.baseUrl}/SalesOrder/GetAllSalesOrderInvoices?storeUID=${storeUID}`
        : `${this.baseUrl}/SalesOrder/GetAllSalesOrderInvoices`;

      const response = await fetch(url, {
        method: "GET",
        headers: this.getHeaders(),
      });

      return await this.handleResponse<SalesOrderInvoice[]>(response);
    } catch (error) {
      console.error("Error fetching sales order invoices:", error);
      throw error;
    }
  }

  // Get sales order print view
  async getSalesOrderPrintView(salesOrderUID: string): Promise<SalesOrderPrintView> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderPrintView?SalesOrderUID=${salesOrderUID}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      return await this.handleResponse<SalesOrderPrintView>(response);
    } catch (error) {
      console.error("Error fetching sales order print view:", error);
      throw error;
    }
  }

  // Get sales order line print view
  async getSalesOrderLinePrintView(salesOrderUID: string): Promise<SalesOrderLinePrintView[]> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/GetSalesOrderLinePrintView?SalesOrderUID=${salesOrderUID}`,
        {
          method: "GET",
          headers: this.getHeaders(),
        }
      );

      return await this.handleResponse<SalesOrderLinePrintView[]>(response);
    } catch (error) {
      console.error("Error fetching sales order line print view:", error);
      throw error;
    }
  }

  // Get delivered pre-sales
  async getDeliveredPreSales(
    request: PagingRequest,
    startDate: string,
    endDate: string,
    status: string
  ): Promise<PagedResponse<any>> {
    try {
      const response = await fetch(
        `${this.baseUrl}/SalesOrder/SelectDeliveredPreSales?startDate=${startDate}&endDate=${endDate}&Status=${status}`,
        {
          method: "POST",
          headers: this.getHeaders(),
          body: JSON.stringify(request),
        }
      );

      return await this.handleResponse<PagedResponse<any>>(response);
    } catch (error) {
      console.error("Error fetching delivered pre-sales:", error);
      throw error;
    }
  }

  // Store Check specific methods (these would need actual endpoints when available)
  async getStoreCheckReports(filters: StoreCheckFilters): Promise<StoreCheckReport[]> {
    // This is a placeholder - would need actual endpoint
    console.log("Getting store check reports with filters:", filters);
    
    // For now, we can use sales order data to simulate store check reports
    const request: PagingRequest = {
      pageNumber: 1,
      pageSize: 100,
      filterCriterias: [],
      sortCriterias: [{ sortColumn: "OrderDate", sortDirection: "DESC" }],
      isCountRequired: true,
    };

    if (filters.storeUID) {
      request.filterCriterias?.push({
        filterColumn: "StoreUID",
        filterValue: filters.storeUID,
        filterOperator: "eq",
      });
    }

    try {
      const salesOrders = await this.getSalesOrders(request);
      
      // Transform sales orders to store check reports
      const reports: StoreCheckReport[] = [];
      
      // Handle different response structures
      let ordersToProcess: SalesOrderViewModel[] = [];
      
      if (salesOrders.pagedData) {
        ordersToProcess = salesOrders.pagedData;
      } else if (Array.isArray(salesOrders)) {
        ordersToProcess = salesOrders;
      } else if ((salesOrders as any).Data) {
        ordersToProcess = Array.isArray((salesOrders as any).Data) 
          ? (salesOrders as any).Data 
          : [(salesOrders as any).Data];
      }
      
      for (const order of ordersToProcess) {
        // Make sure we have valid sales order data
        const salesOrder = order.salesOrder || (order as any).SalesOrder || order;
        const salesOrderLines = order.salesOrderLines || (order as any).SalesOrderLines || [];
        
        // Check if we have valid UID
        const orderUID = salesOrder.uid || (salesOrder as any).UID;
        if (!salesOrder || !orderUID) continue;
        
        reports.push({
          reportId: orderUID,  // Use actual sales order UID
          storeUID: salesOrder.storeUID || "",
          storeName: `Store ${salesOrder.storeUID}`,
          storeCode: salesOrder.storeUID || "",
          checkDate: salesOrder.orderDate || new Date().toISOString(),
          empUID: salesOrder.empUID || "",
          empName: `Employee ${salesOrder.empUID}`,
          beatHistoryUID: salesOrder.beatHistoryUID,
          routeUID: salesOrder.routeUID,
          items: salesOrderLines.map((line: SalesOrderLine) => ({
            skuUID: line.skuUID || line.itemCode || "",
            skuCode: line.itemCode || "",
            skuName: `Product ${line.itemCode}`,
            storeQty: line.qty || 0,
            backStoreQty: 0,
            totalQty: line.qty || 0,
            unitPrice: line.unitPrice || 0,
            totalValue: line.totalAmount || 0,
            uom: line.uom || "PCS",
          })),
          totalSKUs: salesOrder.lineCount || 0,
          totalValue: salesOrder.totalAmount || 0,
          createdTime: salesOrder.createdTime || salesOrder.orderDate,
          status: (salesOrder.status || "").toUpperCase() === "DELIVERED" ? "Completed" : "Draft",
        });
      }

      return reports;
    } catch (error) {
      console.error("Error getting store check reports:", error);
      throw error;
    }
  }

  // Get store check summary
  async getStoreCheckSummary(filters: StoreCheckFilters): Promise<StoreCheckSummary> {
    try {
      const reports = await this.getStoreCheckReports(filters);
      
      // Calculate summary from reports
      const summary: StoreCheckSummary = {
        totalStoresChecked: new Set(reports.map(r => r.storeUID)).size,
        totalSKUsAudited: reports.reduce((sum, r) => sum + r.totalSKUs, 0),
        totalValue: reports.reduce((sum, r) => sum + r.totalValue, 0),
        averageComplianceRate: 85.5, // Mock value
        topPerformingStores: [],
        lowStockAlerts: [],
      };

      return summary;
    } catch (error) {
      console.error("Error getting store check summary:", error);
      throw error;
    }
  }

  // Export report to CSV
  exportToCSV(reports: StoreCheckReport[]): void {
    const headers = [
      "Report ID",
      "Store Code",
      "Store Name",
      "Check Date",
      "Employee",
      "Total SKUs",
      "Total Value",
      "Status",
    ];

    const rows = reports.map(report => [
      report.reportId,
      report.storeCode,
      report.storeName,
      report.checkDate,
      report.empName,
      report.totalSKUs.toString(),
      report.totalValue.toFixed(2),
      report.status,
    ]);

    const csvContent = [
      headers.join(","),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(",")),
    ].join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", `store_check_report_${new Date().toISOString().split("T")[0]}.csv`);
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}

export const storeCheckReportService = new StoreCheckReportService();