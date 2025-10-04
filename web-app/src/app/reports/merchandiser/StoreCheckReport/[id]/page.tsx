"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { useToast } from "@/components/ui/use-toast";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Skeleton } from "@/components/ui/skeleton";
import {
  ArrowLeft,
  Download,
  Printer,
  Store,
  User,
  Calendar,
  Package,
  DollarSign,
  MapPin,
  Clock,
  CheckCircle2,
  AlertCircle,
  TrendingUp,
  TrendingDown,
} from "lucide-react";
import { format } from "date-fns";
import { formatDateToDayMonthYear, formatTime } from "@/utils/date-formatter";
import { storeCheckReportService } from "@/services/reports/store-check-report.service";
import {
  SalesOrder,
  SalesOrderLine,
  SalesOrderViewModel,
  SalesOrderPrintView,
  SalesOrderLinePrintView,
} from "@/types/store-check.types";

export default function StoreCheckDetailPage() {
  const params = useParams();
  const router = useRouter();
  const { toast } = useToast();
  
  const reportId = params.id as string;
  
  const [loading, setLoading] = useState(true);
  const [orderData, setOrderData] = useState<SalesOrderViewModel | null>(null);
  const [printView, setPrintView] = useState<SalesOrderPrintView | null>(null);
  const [linePrintView, setLinePrintView] = useState<SalesOrderLinePrintView[]>([]);

  useEffect(() => {
    if (reportId) {
      fetchReportDetails();
    }
  }, [reportId]);

  const fetchReportDetails = async () => {
    setLoading(true);
    try {
      // First, try to get the sales order by UID to check if it exists
      try {
        const salesOrderData = await storeCheckReportService.getSalesOrderByUID(reportId);
        console.log("Sales order data:", salesOrderData);
        
        if (salesOrderData) {
          // Create the view model structure
          const transformedData: SalesOrderViewModel = {
            salesOrder: salesOrderData,
            salesOrderLines: [],
            isNewOrder: false,
            actionType: 0,
          };
          
          // Try to get the lines separately
          try {
            const lines = await storeCheckReportService.getSalesOrderLines(reportId);
            transformedData.salesOrderLines = lines || [];
          } catch (lineError) {
            console.warn("Failed to fetch sales order lines:", lineError);
          }
          
          setOrderData(transformedData);
        }
      } catch (fetchError) {
        // If that fails, try the master data endpoint
        console.log("Trying master data endpoint...");
        const masterData = await storeCheckReportService.getSalesOrderMasterData(reportId);
        console.log("Master data received:", masterData);
        
        // The response structure is { IsNewOrder, SalesOrder, SalesOrderLines, ActionType }
        if (masterData) {
          // Log to see what we're getting
          console.log("Raw masterData:", JSON.stringify(masterData, null, 2));
          
          // Handle both PascalCase and camelCase
          const salesOrderFromMaster = (masterData as any).SalesOrder || 
                                       (masterData as any).salesOrder || 
                                       masterData.salesOrder;
          
          const salesOrderLines = (masterData as any).SalesOrderLines || 
                                  (masterData as any).salesOrderLines || 
                                  masterData.salesOrderLines || 
                                  [];
          
          if (!salesOrderFromMaster) {
            // If no SalesOrder in the response, this order doesn't exist
            console.error("No SalesOrder found in response. Keys available:", Object.keys(masterData));
            throw new Error("Sales order not found. Please check the Report ID.");
          }
          
          // Transform to lowercase property names for consistency
          const transformedSalesOrder: SalesOrder = {
            id: salesOrderFromMaster.Id || salesOrderFromMaster.id || 0,
            uid: salesOrderFromMaster.UID || salesOrderFromMaster.uid || "",
            salesOrderNumber: salesOrderFromMaster.SalesOrderNumber || salesOrderFromMaster.salesOrderNumber,
            companyUID: salesOrderFromMaster.CompanyUID || salesOrderFromMaster.companyUID,
            orgUID: salesOrderFromMaster.OrgUID || salesOrderFromMaster.orgUID,
            storeUID: salesOrderFromMaster.StoreUID || salesOrderFromMaster.storeUID,
            status: salesOrderFromMaster.Status || salesOrderFromMaster.status,
            orderType: salesOrderFromMaster.OrderType || salesOrderFromMaster.orderType,
            orderDate: salesOrderFromMaster.OrderDate || salesOrderFromMaster.orderDate,
            expectedDeliveryDate: salesOrderFromMaster.ExpectedDeliveryDate || salesOrderFromMaster.expectedDeliveryDate,
            deliveredDateTime: salesOrderFromMaster.DeliveredDateTime || salesOrderFromMaster.deliveredDateTime,
            currencyUID: salesOrderFromMaster.CurrencyUID || salesOrderFromMaster.currencyUID,
            totalAmount: salesOrderFromMaster.TotalAmount || salesOrderFromMaster.totalAmount || 0,
            totalDiscount: salesOrderFromMaster.TotalDiscount || salesOrderFromMaster.totalDiscount || 0,
            totalTax: salesOrderFromMaster.TotalTax || salesOrderFromMaster.totalTax || 0,
            netAmount: salesOrderFromMaster.NetAmount || salesOrderFromMaster.netAmount || 0,
            lineCount: salesOrderFromMaster.LineCount || salesOrderFromMaster.lineCount || 0,
            qtyCount: salesOrderFromMaster.QtyCount || salesOrderFromMaster.qtyCount || 0,
            empUID: salesOrderFromMaster.EmpUID || salesOrderFromMaster.empUID,
            jobPositionUID: salesOrderFromMaster.JobPositionUID || salesOrderFromMaster.jobPositionUID,
            beatHistoryUID: salesOrderFromMaster.BeatHistoryUID || salesOrderFromMaster.beatHistoryUID,
            routeUID: salesOrderFromMaster.RouteUID || salesOrderFromMaster.routeUID,
            storeHistoryUID: salesOrderFromMaster.StoreHistoryUID || salesOrderFromMaster.storeHistoryUID,
            createdBy: salesOrderFromMaster.CreatedBy || salesOrderFromMaster.createdBy,
            createdTime: salesOrderFromMaster.CreatedTime || salesOrderFromMaster.createdTime,
            modifiedBy: salesOrderFromMaster.ModifiedBy || salesOrderFromMaster.modifiedBy,
            modifiedTime: salesOrderFromMaster.ModifiedTime || salesOrderFromMaster.modifiedTime,
            ss: salesOrderFromMaster.SS || salesOrderFromMaster.ss || 0,
          };
          
          // Transform lines as well
          const transformedLines: SalesOrderLine[] = salesOrderLines.map((line: any) => ({
            id: line.Id || line.id || 0,
            uid: line.UID || line.uid || "",
            salesOrderUID: line.SalesOrderUID || line.salesOrderUID || "",
            lineNumber: line.LineNumber || line.lineNumber || 0,
            itemCode: line.ItemCode || line.itemCode,
            skuUID: line.SKUUID || line.skuUID || line.SkuUID,
            qty: line.Qty || line.qty || 0,
            unitPrice: line.UnitPrice || line.unitPrice || 0,
            totalAmount: line.TotalAmount || line.totalAmount || 0,
            totalDiscount: line.TotalDiscount || line.totalDiscount || 0,
            totalTax: line.TotalTax || line.totalTax || 0,
            netAmount: line.NetAmount || line.netAmount || 0,
            uom: line.UOM || line.uom || "PCS",
            deliveredQty: line.DeliveredQty || line.deliveredQty || 0,
            approvedQty: line.ApprovedQty || line.approvedQty || 0,
          }));
          
          const transformedData: SalesOrderViewModel = {
            salesOrder: transformedSalesOrder,
            salesOrderLines: transformedLines,
            isNewOrder: (masterData as any).IsNewOrder || false,
            actionType: (masterData as any).ActionType || 0,
          };
          
          console.log("Transformed data:", transformedData);
          setOrderData(transformedData);
        }
      }

      // Fetch print views
      try {
        const printData = await storeCheckReportService.getSalesOrderPrintView(reportId);
        setPrintView(printData);
      } catch (printError) {
        console.warn("Failed to fetch print view:", printError);
        // Continue without print view
      }

      try {
        const linePrintData = await storeCheckReportService.getSalesOrderLinePrintView(reportId);
        setLinePrintView(linePrintData);
      } catch (lineError) {
        console.warn("Failed to fetch line print view:", lineError);
        // Continue without line print view
      }
    } catch (error) {
      console.error("Error fetching report details:", error);
      toast({
        title: "Error",
        description: "Failed to load report details. Please check the report ID.",
        variant: "destructive",
      });
      // Set empty data to prevent undefined errors
      setOrderData(null);
    } finally {
      setLoading(false);
    }
  };

  const handlePrint = () => {
    window.print();
  };

  // Safe date formatting helper
  const formatSafeDate = (dateValue: any, formatString: string, fallback: string = "N/A") => {
    try {
      if (!dateValue) return fallback;
      const date = new Date(dateValue);
      if (isNaN(date.getTime())) return fallback;
      
      // Use common utility for consistent formatting
      if (formatString === "dd MMM, yyyy") {
        return formatDateToDayMonthYear(dateValue, fallback);
      }
      
      if (formatString === "hh:mm a") {
        return formatTime(dateValue, fallback);
      }
      
      return format(date, formatString);
    } catch (error) {
      return fallback;
    }
  };

  const handleExportPDF = () => {
    // Implement PDF export functionality
    toast({
      title: "Export PDF",
      description: "PDF export functionality coming soon",
    });
  };

  const getStatusBadge = (status: string) => {
    switch (status?.toUpperCase()) {
      case "DELIVERED":
      case "COMPLETED":
        return (
          <Badge className="bg-green-100 text-green-800">
            <CheckCircle2 className="h-3 w-3 mr-1" />
            {status}
          </Badge>
        );
      case "PENDING":
      case "DRAFT":
        return (
          <Badge className="bg-yellow-100 text-yellow-800">
            <Clock className="h-3 w-3 mr-1" />
            {status}
          </Badge>
        );
      case "CANCELLED":
        return (
          <Badge className="bg-red-100 text-red-800">
            <AlertCircle className="h-3 w-3 mr-1" />
            {status}
          </Badge>
        );
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  const calculateVariance = (actual: number, expected: number) => {
    if (expected === 0) return 0;
    return ((actual - expected) / expected) * 100;
  };

  // Safe number formatting helper
  const formatSafeNumber = (value: any, fallback: number = 0) => {
    const num = Number(value);
    return isNaN(num) ? fallback : num;
  };

  if (loading) {
    return (
      <div className="flex flex-col gap-6 p-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10" />
          <Skeleton className="h-8 w-64" />
        </div>
        <div className="grid gap-4 md:grid-cols-4">
          {[...Array(4)].map((_, i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        <Skeleton className="h-96" />
      </div>
    );
  }

  if (!orderData) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh]">
        <AlertCircle className="h-12 w-12 text-muted-foreground mb-4" />
        <p className="text-lg font-medium">Report not found</p>
        <p className="text-sm text-muted-foreground mb-4">
          The report you're looking for doesn't exist or has been removed
        </p>
        <Button onClick={() => router.push("/reports/merchandiser/StoreCheckReport")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Reports
        </Button>
      </div>
    );
  }

  const salesOrder = orderData?.salesOrder;
  const salesOrderLines = orderData?.salesOrderLines || [];
  
  if (!salesOrder) {
    return (
      <div className="flex flex-col items-center justify-center min-h-[60vh]">
        <AlertCircle className="h-12 w-12 text-muted-foreground mb-4" />
        <p className="text-lg font-medium">Invalid report data</p>
        <p className="text-sm text-muted-foreground mb-4">
          The report data is invalid or corrupted
        </p>
        <Button onClick={() => router.push("/reports/merchandiser/StoreCheckReport")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Reports
        </Button>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.push("/reports/merchandiser/StoreCheckReport")}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
          <div>
            <h1 className="text-2xl font-bold">Store Check Report Details</h1>
            <p className="text-sm text-muted-foreground">
              Report ID: {reportId}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={handlePrint}>
            <Printer className="h-4 w-4 mr-2" />
            Print
          </Button>
          <Button variant="outline" size="sm" onClick={handleExportPDF}>
            <Download className="h-4 w-4 mr-2" />
            Export PDF
          </Button>
        </div>
      </div>

      {/* Store Information */}
      <Card>
        <CardHeader>
          <CardTitle>Store Information</CardTitle>
          <CardDescription>Details about the store and check</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="grid gap-6 md:grid-cols-2">
            <div className="space-y-4">
              <div className="flex items-start gap-3">
                <Store className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Store Details</p>
                  <p className="text-sm text-muted-foreground">
                    {printView?.storeName || `Store ${salesOrder.storeUID}`}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Code: {printView?.storeCode || salesOrder.storeUID}
                  </p>
                  {printView?.addressLine1 && (
                    <p className="text-sm text-muted-foreground mt-1">
                      {printView.addressLine1}
                    </p>
                  )}
                </div>
              </div>

              <div className="flex items-start gap-3">
                <User className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Checked By</p>
                  <p className="text-sm text-muted-foreground">
                    Employee: {salesOrder.empUID || "N/A"}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Position: {salesOrder.jobPositionUID || "N/A"}
                  </p>
                </div>
              </div>
            </div>

            <div className="space-y-4">
              <div className="flex items-start gap-3">
                <Calendar className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Check Date</p>
                  <p className="text-sm text-muted-foreground">
                    {formatSafeDate(salesOrder.orderDate, "dd MMM, yyyy")}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {formatSafeDate(salesOrder.orderDate, "hh:mm a")}
                  </p>
                </div>
              </div>

              <div className="flex items-start gap-3">
                <MapPin className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Location</p>
                  <p className="text-sm text-muted-foreground">
                    Route: {salesOrder.routeUID || "N/A"}
                  </p>
                  {salesOrder.latitude && salesOrder.longitude && (
                    <p className="text-sm text-muted-foreground">
                      GPS: {salesOrder.latitude}, {salesOrder.longitude}
                    </p>
                  )}
                </div>
              </div>
            </div>
          </div>

          <Separator className="my-4" />

          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <span className="text-sm font-medium">Status:</span>
              {getStatusBadge(salesOrder.status)}
            </div>
            <div className="flex items-center gap-4 text-sm">
              <span className="text-muted-foreground">Order Type:</span>
              <Badge variant="outline">{salesOrder.orderType}</Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Summary Statistics */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total SKUs</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatSafeNumber(salesOrder.lineCount)}</div>
            <p className="text-xs text-muted-foreground">Items checked</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Quantity</CardTitle>
            <Package className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{formatSafeNumber(salesOrder.qtyCount)}</div>
            <p className="text-xs text-muted-foreground">Units counted</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Total Value</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${formatSafeNumber(salesOrder.totalAmount).toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground">Inventory value</p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Net Amount</CardTitle>
            <DollarSign className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              ${formatSafeNumber(salesOrder.netAmount || salesOrder.totalAmount).toLocaleString()}
            </div>
            <p className="text-xs text-muted-foreground">After discounts</p>
          </CardContent>
        </Card>
      </div>

      {/* Product Details */}
      <Card>
        <CardHeader>
          <CardTitle>Product Details</CardTitle>
          <CardDescription>
            Detailed breakdown of all products checked
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>SKU Code</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>UOM</TableHead>
                <TableHead className="text-right">Quantity</TableHead>
                <TableHead className="text-right">Unit Price</TableHead>
                <TableHead className="text-right">Total Amount</TableHead>
                <TableHead className="text-right">Variance</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {(() => {
                const itemsToDisplay = Array.isArray(linePrintView) ? linePrintView : Array.isArray(salesOrderLines) ? salesOrderLines : [];
                
                if (itemsToDisplay.length === 0) {
                  return (
                    <TableRow>
                      <TableCell colSpan={8} className="text-center text-muted-foreground py-8">
                        No product details available
                      </TableCell>
                    </TableRow>
                  );
                }
                
                return itemsToDisplay.map((line, index) => {
                  const orderLine = Array.isArray(salesOrderLines) ? salesOrderLines[index] : null;
                  const isLinePrintView = Array.isArray(linePrintView) && linePrintView.length > 0;
                  const displayLine = isLinePrintView ? line : orderLine;
                  const variance = calculateVariance(formatSafeNumber(displayLine?.qty), formatSafeNumber(displayLine?.recoQty));
                  
                  return (
                    <TableRow key={index}>
                      <TableCell className="font-medium">
                        {(displayLine as any)?.skuCode || (displayLine as any)?.itemCode || (orderLine as any)?.itemCode || "N/A"}
                      </TableCell>
                      <TableCell>{(displayLine as any)?.skuDescription || "Product"}</TableCell>
                      <TableCell>{(orderLine as any)?.uom || "PCS"}</TableCell>
                      <TableCell className="text-right">{formatSafeNumber(displayLine?.qty)}</TableCell>
                      <TableCell className="text-right">
                        ${formatSafeNumber(displayLine?.unitPrice).toFixed(2)}
                      </TableCell>
                      <TableCell className="text-right">
                        ${formatSafeNumber(displayLine?.totalAmount).toFixed(2)}
                      </TableCell>
                      <TableCell className="text-right">
                        {variance !== 0 && (
                          <span
                            className={`flex items-center justify-end gap-1 ${
                              variance > 0 ? "text-green-600" : "text-red-600"
                            }`}
                          >
                            {variance > 0 ? (
                              <TrendingUp className="h-3 w-3" />
                            ) : (
                              <TrendingDown className="h-3 w-3" />
                            )}
                            {Math.abs(variance).toFixed(1)}%
                          </span>
                        )}
                      </TableCell>
                      <TableCell>
                        {formatSafeNumber(displayLine?.deliveredQty) > 0 ? (
                          <Badge className="bg-green-100 text-green-800">
                            Delivered
                          </Badge>
                        ) : (
                          <Badge variant="secondary">Pending</Badge>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                });
              })()}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Additional Information */}
      {(salesOrder.notes || salesOrder.remarks || salesOrder.deliveryInstructions) && (
        <Card>
          <CardHeader>
            <CardTitle>Additional Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {salesOrder.notes && (
              <div>
                <p className="text-sm font-medium">Notes</p>
                <p className="text-sm text-muted-foreground">{salesOrder.notes}</p>
              </div>
            )}
            {salesOrder.remarks && (
              <div>
                <p className="text-sm font-medium">Remarks</p>
                <p className="text-sm text-muted-foreground">{salesOrder.remarks}</p>
              </div>
            )}
            {salesOrder.deliveryInstructions && (
              <div>
                <p className="text-sm font-medium">Delivery Instructions</p>
                <p className="text-sm text-muted-foreground">
                  {salesOrder.deliveryInstructions}
                </p>
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}