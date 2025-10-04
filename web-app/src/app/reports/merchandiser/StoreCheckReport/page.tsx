"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { useToast } from "@/components/ui/use-toast";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Skeleton } from "@/components/ui/skeleton";
import { PaginationControls } from "@/components/ui/pagination-controls";
import {
  Search,
  Filter,
  Download,
  RefreshCw,
  MoreVertical,
  Eye,
  FileText,
  TrendingUp,
  TrendingDown,
  Store,
  Package,
  Calendar,
  DollarSign,
  BarChart3,
  AlertCircle,
  CheckCircle2,
  Clock,
  Printer,
} from "lucide-react";
import { format } from "date-fns";
import { storeCheckReportService } from "@/services/reports/store-check-report.service";
import {
  StoreCheckFilters,
  StoreCheckSummary,
  PagingRequest,
  SalesOrderViewModel,
} from "@/types/store-check.types";

export default function StoreCheckReportPage() {
  const router = useRouter();
  const { toast } = useToast();

  // State management
  const [loading, setLoading] = useState(false);
  const [refreshing, setRefreshing] = useState(false);
  const [salesOrders, setSalesOrders] = useState<SalesOrderViewModel[]>([]);
  const [summary, setSummary] = useState<StoreCheckSummary | null>(null);
  
  // Filters
  const [filters, setFilters] = useState<StoreCheckFilters>({
    startDate: format(new Date(new Date().setDate(new Date().getDate() - 30)), "yyyy-MM-dd"),
    endDate: format(new Date(), "yyyy-MM-dd"),
  });
  const [searchTerm, setSearchTerm] = useState("");
  
  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);
  const [totalCount, setTotalCount] = useState(0);

  // Fetch data on component mount and filter changes
  useEffect(() => {
    fetchSalesOrders();
  }, [currentPage, pageSize]);

  // Fetch sales orders
  const fetchSalesOrders = async () => {
    setLoading(true);
    try {
      const request: PagingRequest = {
        pageNumber: currentPage,
        pageSize: pageSize,
        filterCriterias: [],
        sortCriterias: [{ sortColumn: "OrderDate", sortDirection: "DESC" }],
        isCountRequired: true,
      };

      // Add search filter if present
      if (searchTerm) {
        request.filterCriterias?.push({
          filterColumn: "SalesOrderNumber",
          filterValue: searchTerm,
          filterOperator: "like",
        });
      }

      // Add date filters
      if (filters.startDate) {
        request.filterCriterias?.push({
          filterColumn: "OrderDate",
          filterValue: filters.startDate,
          filterOperator: "gte",
        });
      }

      if (filters.endDate) {
        request.filterCriterias?.push({
          filterColumn: "OrderDate",
          filterValue: filters.endDate,
          filterOperator: "lte",
        });
      }

      console.log("Fetching sales orders with request:", request);
      const response = await storeCheckReportService.getSalesOrders(request);
      
      // Deep inspection of the response
      console.log("=== API RESPONSE DEBUG ===");
      console.log("Response type:", typeof response);
      console.log("Response is array?", Array.isArray(response));
      console.log("Response keys:", response ? Object.keys(response) : 'null');
      
      if (response) {
        // Log first few keys and their types
        Object.keys(response).slice(0, 5).forEach(key => {
          const value = (response as any)[key];
          console.log(`  ${key}:`, typeof value, Array.isArray(value) ? `array[${value.length}]` : value);
        });
      }
      
      if (response) {
        // Handle the response based on structure
        let ordersData: SalesOrderViewModel[] = [];
        let count = 0;

        // Check all possible response structures
        const possibleDataFields = [
          'PagedData', 'pagedData', 
          'Data', 'data',
          'Items', 'items',
          'Results', 'results',
          'SalesOrders', 'salesOrders'
        ];

        let foundData = false;
        for (const field of possibleDataFields) {
          if ((response as any)[field]) {
            console.log(`Found data in field: ${field}`);
            const fieldData = (response as any)[field];
            
            if (Array.isArray(fieldData)) {
              ordersData = fieldData;
              count = (response as any).TotalCount || (response as any).totalCount || 
                     (response as any).Count || (response as any).count || 
                     fieldData.length;
              foundData = true;
              console.log(`Extracted ${ordersData.length} orders from ${field}`);
              break;
            } else if (typeof fieldData === 'object') {
              // Check if it has nested paged data
              const nestedFields = ['PagedData', 'pagedData', 'Items', 'items'];
              for (const nested of nestedFields) {
                if (fieldData[nested] && Array.isArray(fieldData[nested])) {
                  ordersData = fieldData[nested];
                  count = fieldData.TotalCount || fieldData.totalCount || fieldData[nested].length;
                  foundData = true;
                  console.log(`Extracted ${ordersData.length} orders from ${field}.${nested}`);
                  break;
                }
              }
              if (foundData) break;
            }
          }
        }

        // If no specific field found, check if response itself is an array
        if (!foundData) {
          if (Array.isArray(response)) {
            ordersData = response;
            count = response.length;
            console.log(`Response is direct array with ${ordersData.length} items`);
          } else {
            // Last resort - check if response has numeric keys (object acting as array)
            const numericKeys = Object.keys(response).filter(k => !isNaN(Number(k)));
            if (numericKeys.length > 0) {
              ordersData = numericKeys.map(k => (response as any)[k]);
              count = ordersData.length;
              console.log(`Converted object with numeric keys to array with ${ordersData.length} items`);
            } else {
              console.warn("Could not find sales order data in response!");
              console.log("Full response structure:", JSON.stringify(response, null, 2).substring(0, 1000));
            }
          }
        }

        
        setSalesOrders(ordersData);
        setTotalCount(count);
        
        // Calculate summary
        calculateSummary(ordersData);
      }
    } catch (error) {
      console.error("Error fetching sales orders:", error);
      toast({
        title: "Error",
        description: "Failed to fetch sales orders. Please try again.",
        variant: "destructive",
      });
    } finally {
      setLoading(false);
    }
  };

  // Calculate summary from sales orders
  const calculateSummary = (orders: SalesOrderViewModel[]) => {
    const validOrders = orders.filter(o => {
      const order = (o as any).SalesOrder || (o as any).salesOrder || o;
      return order && (order.UID || order.uid || order.Uid);
    });

    const summary: StoreCheckSummary = {
      totalStoresChecked: new Set(validOrders.map(o => {
        const order = (o as any).SalesOrder || (o as any).salesOrder || o;
        return order.StoreUID || order.storeUID || order.StoreUid || "";
      })).size,
      totalSKUsAudited: validOrders.reduce((sum, o) => {
        const order = (o as any).SalesOrder || (o as any).salesOrder || o;
        return sum + (order.LineCount || order.lineCount || 0);
      }, 0),
      totalValue: validOrders.reduce((sum, o) => {
        const order = (o as any).SalesOrder || (o as any).salesOrder || o;
        return sum + (order.TotalAmount || order.totalAmount || 0);
      }, 0),
      averageComplianceRate: 85.5,
      topPerformingStores: [],
      lowStockAlerts: [],
    };
    setSummary(summary);
  };

  // Handle refresh
  const handleRefresh = async () => {
    setRefreshing(true);
    await fetchSalesOrders();
    setRefreshing(false);
    toast({
      title: "Success",
      description: "Data refreshed successfully",
    });
  };

  // Handle export
  const handleExport = () => {
    if (salesOrders.length === 0) {
      toast({
        title: "No Data",
        description: "No sales orders available to export",
        variant: "destructive",
      });
      return;
    }
    // Export sales orders as CSV
    const csvData = salesOrders.map(o => {
      const order = (o as any).SalesOrder || (o as any).salesOrder || o;
      return {
        OrderID: order.SalesOrderNumber || order.salesOrderNumber || "",
        UID: order.UID || order.uid || order.Uid || "",
        Store: order.StoreUID || order.storeUID || order.StoreUid || "",
        Date: order.OrderDate || order.orderDate || "",
        Employee: order.EmpUID || order.empUID || order.EmpUid || "",
        Status: order.Status || order.status || "",
        TotalAmount: order.TotalAmount || order.totalAmount || 0,
        LineCount: order.LineCount || order.lineCount || 0,
      };
    });
    
    const headers = Object.keys(csvData[0] || {});
    const rows = csvData.map(row => headers.map(h => (row as any)[h]));
    const csvContent = [
      headers.join(","),
      ...rows.map(row => row.map(cell => `"${cell}"`).join(","))
    ].join("\n");

    const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    const link = document.createElement("a");
    const url = URL.createObjectURL(blob);
    link.setAttribute("href", url);
    link.setAttribute("download", `sales_orders_${new Date().toISOString().split("T")[0]}.csv`);
    link.style.visibility = "hidden";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    
    toast({
      title: "Success",
      description: "Report exported successfully",
    });
  };

  // Handle view details
  const handleViewDetails = async (orderViewModel: SalesOrderViewModel) => {
    // Get the actual sales order UID
    const order = (orderViewModel as any).SalesOrder || (orderViewModel as any).salesOrder || orderViewModel;
    const uid = order.UID || order.uid || order.Uid;
    
    if (!uid) {
      toast({
        title: "Error",
        description: "Invalid sales order ID",
        variant: "destructive",
      });
      return;
    }
    
    // Navigate to detailed view using actual UID
    router.push(`/reports/merchandiser/StoreCheckReport/${uid}`);
  };

  // Handle print
  const handlePrint = async (reportId: string) => {
    try {
      await storeCheckReportService.getSalesOrderPrintView(reportId);
      // Open print preview
      window.print();
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to generate print view",
        variant: "destructive",
      });
    }
  };

  // Get status badge variant
  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Completed":
        return <Badge className="bg-green-100 text-green-800">Completed</Badge>;
      case "Synced":
        return <Badge className="bg-blue-100 text-blue-800">Synced</Badge>;
      case "Draft":
        return <Badge className="bg-yellow-100 text-yellow-800">Draft</Badge>;
      default:
        return <Badge variant="secondary">{status}</Badge>;
    }
  };

  return (
    <div className="flex flex-col gap-6 p-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Store Check Reports</h1>
          <p className="text-muted-foreground">
            View and manage merchandiser store check reports
          </p>
        </div>
        <div className="flex gap-2">
          {/* Test link for existing sales order */}
          <Button
            variant="outline"
            size="sm"
            onClick={() => router.push('/reports/merchandiser/StoreCheckReport/SO_CF_TEST_003')}
          >
            <Eye className="h-4 w-4 mr-2" />
            View Test Report
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleRefresh}
            disabled={refreshing}
          >
            <RefreshCw className={`h-4 w-4 mr-2 ${refreshing ? "animate-spin" : ""}`} />
            Refresh
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <Download className="h-4 w-4 mr-2" />
            Export
          </Button>
        </div>
      </div>

      {/* Summary Cards */}
      {summary && (
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Stores Checked</CardTitle>
              <Store className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.totalStoresChecked}</div>
              <p className="text-xs text-muted-foreground">
                <TrendingUp className="inline h-3 w-3 text-green-500 mr-1" />
                +12% from last month
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">SKUs Audited</CardTitle>
              <Package className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.totalSKUsAudited}</div>
              <p className="text-xs text-muted-foreground">
                Across all stores
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Total Value</CardTitle>
              <DollarSign className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">
                ${summary.totalValue.toLocaleString()}
              </div>
              <p className="text-xs text-muted-foreground">
                Inventory value checked
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">Compliance Rate</CardTitle>
              <BarChart3 className="h-4 w-4 text-muted-foreground" />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{summary.averageComplianceRate}%</div>
              <p className="text-xs text-muted-foreground">
                <TrendingDown className="inline h-3 w-3 text-red-500 mr-1" />
                -2.5% from target
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Filters and Search */}
      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid gap-4 md:grid-cols-4">
            <div className="flex flex-col space-y-1.5">
              <Label htmlFor="search">Search</Label>
              <div className="relative">
                <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  id="search"
                  placeholder="Search by store or SKU..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-8"
                />
              </div>
            </div>

            <div className="flex flex-col space-y-1.5">
              <Label htmlFor="startDate">Start Date</Label>
              <Input
                id="startDate"
                type="date"
                value={filters.startDate}
                onChange={(e) =>
                  setFilters({ ...filters, startDate: e.target.value })
                }
              />
            </div>

            <div className="flex flex-col space-y-1.5">
              <Label htmlFor="endDate">End Date</Label>
              <Input
                id="endDate"
                type="date"
                value={filters.endDate}
                onChange={(e) =>
                  setFilters({ ...filters, endDate: e.target.value })
                }
              />
            </div>

            <div className="flex flex-col space-y-1.5">
              <Label htmlFor="status">Status</Label>
              <Select
                value={filters.status}
                onValueChange={(value) =>
                  setFilters({ ...filters, status: value })
                }
              >
                <SelectTrigger id="status">
                  <SelectValue placeholder="All Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="Draft">Draft</SelectItem>
                  <SelectItem value="Completed">Completed</SelectItem>
                  <SelectItem value="Synced">Synced</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="mt-4 flex gap-2">
            <Button onClick={fetchSalesOrders} disabled={loading}>
              <Filter className="h-4 w-4 mr-2" />
              Apply Filters
            </Button>
            <Button
              variant="outline"
              onClick={() => {
                setFilters({
                  startDate: format(
                    new Date(new Date().setDate(new Date().getDate() - 30)),
                    "yyyy-MM-dd"
                  ),
                  endDate: format(new Date(), "yyyy-MM-dd"),
                });
                setSearchTerm("");
              }}
            >
              Clear
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Sales Orders Table */}
      <Card>
        <CardHeader>
          <CardTitle>Sales Orders</CardTitle>
          <CardDescription>
            List of all sales orders - Click on any order to view details
          </CardDescription>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-12 w-full" />
              ))}
            </div>
          ) : salesOrders.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-12">
              <AlertCircle className="h-12 w-12 text-muted-foreground mb-4" />
              <p className="text-lg font-medium">No sales orders found</p>
              <p className="text-sm text-muted-foreground">
                Try adjusting your filters or date range
              </p>
            </div>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Order Number</TableHead>
                    <TableHead>Order UID</TableHead>
                    <TableHead>Store</TableHead>
                    <TableHead>Date</TableHead>
                    <TableHead>Employee</TableHead>
                    <TableHead className="text-right">Lines</TableHead>
                    <TableHead className="text-right">Total Amount</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {salesOrders.map((orderViewModel, index) => {
                    // Extract the actual sales order from various response structures
                    const order = (orderViewModel as any).SalesOrder || 
                                  (orderViewModel as any).salesOrder || 
                                  orderViewModel;
                    
                    // Extract fields with case handling
                    const uid = order?.UID || order?.uid || order?.Uid || order?.uID || `order-${index}`;
                    const orderNumber = order?.SalesOrderNumber || order?.salesOrderNumber || 
                                      order?.OrderNumber || order?.orderNumber || uid;
                    const storeUID = order?.StoreUID || order?.storeUID || order?.StoreUid || "";
                    const orderDate = order?.OrderDate || order?.orderDate || "";
                    const empUID = order?.EmpUID || order?.empUID || order?.EmpUid || "";
                    const lineCount = order?.LineCount || order?.lineCount || 0;
                    const totalAmount = order?.TotalAmount || order?.totalAmount || 0;
                    const status = order?.Status || order?.status || "Unknown";
                    
                    return (
                      <TableRow key={uid}>
                        <TableCell className="font-medium">
                          {orderNumber}
                        </TableCell>
                        <TableCell>
                          <code className="text-xs">
                            {uid.length > 20 ? `${uid.substring(0, 20)}...` : uid}
                          </code>
                        </TableCell>
                        <TableCell>
                          {storeUID || "N/A"}
                        </TableCell>
                        <TableCell>
                          {orderDate ? format(new Date(orderDate), "MMM dd, yyyy") : "N/A"}
                        </TableCell>
                        <TableCell>{empUID || "N/A"}</TableCell>
                        <TableCell className="text-right">
                          {lineCount}
                        </TableCell>
                        <TableCell className="text-right">
                          ${totalAmount.toLocaleString()}
                        </TableCell>
                        <TableCell>{getStatusBadge(status)}</TableCell>
                        <TableCell className="text-right">
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button variant="ghost" className="h-8 w-8 p-0">
                                <MoreVertical className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent align="end">
                              <DropdownMenuLabel>Actions</DropdownMenuLabel>
                              <DropdownMenuItem
                                onClick={() => handleViewDetails(orderViewModel)}
                              >
                                <Eye className="mr-2 h-4 w-4" />
                                View Details
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                onClick={() => handlePrint(uid)}
                              >
                                <Printer className="mr-2 h-4 w-4" />
                                Print
                              </DropdownMenuItem>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem>
                                <FileText className="mr-2 h-4 w-4" />
                                Export PDF
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>

              {/* Pagination */}
              <div className="mt-4">
                <PaginationControls
                  currentPage={currentPage}
                  totalCount={totalCount}
                  onPageChange={setCurrentPage}
                  pageSize={pageSize}
                  onPageSizeChange={setPageSize}
                  itemName="orders"
                />
              </div>
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}