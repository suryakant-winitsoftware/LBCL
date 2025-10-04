'use client';

import { useParams, useRouter } from 'next/navigation';
import React, { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from '@/components/ui/table';
import { 
  ArrowLeft, 
  ShoppingCart, 
  Package, 
  DollarSign, 
  Calendar, 
  User, 
  MapPin, 
  Clock, 
  CheckCircle, 
  XCircle, 
  AlertCircle, 
  TrendingUp, 
  Store, 
  FileText, 
  Truck, 
  CreditCard, 
  Hash, 
  BarChart3, 
  Info, 
  ChevronDown, 
  ChevronRight, 
  Receipt, 
  ShoppingBag,
  PackageCheck,
  PackageX,
  Banknote,
  Calculator,
  Activity
} from 'lucide-react';
import salesOrderService, { SalesOrder, SalesOrderLine, SalesOrderViewModel } from '@/services/sales-order.service';
import { format } from 'date-fns';
import { Skeleton } from '@/components/ui/skeleton';

export default function SalesOrderDetailPage() {
  const params = useParams();
  const router = useRouter();
  const routeUID = params.uid as string;
  
  const [salesOrders, setSalesOrders] = useState<SalesOrderViewModel[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedOrders, setExpandedOrders] = useState<Set<string>>(new Set());
  const [orderLines, setOrderLines] = useState<{ [key: string]: SalesOrderLine[] }>({});
  const [loadingLines, setLoadingLines] = useState<Set<string>>(new Set());

  // Summary statistics
  const [summary, setSummary] = useState({
    totalOrders: 0,
    totalRevenue: 0,
    totalQuantity: 0,
    averageOrderValue: 0,
    completedOrders: 0,
    pendingOrders: 0,
    totalDiscount: 0,
    totalTax: 0,
    totalItems: 0
  });

  useEffect(() => {
    loadSalesOrders();
  }, [routeUID]);

  const loadSalesOrders = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Get sales orders for this route
      const response = await salesOrderService.getSalesOrdersByRoute(routeUID);
      
      if (response?.Data) {
        const ordersData = response.Data as SalesOrderViewModel[];
        setSalesOrders(ordersData);
        
        // Calculate summary statistics
        calculateSummary(ordersData);
      }
    } catch (err) {
      console.error('Error loading sales orders:', err);
      setError('Failed to load sales orders');
    } finally {
      setIsLoading(false);
    }
  };

  const calculateSummary = (orders: SalesOrderViewModel[]) => {
    const stats = {
      totalOrders: orders.length,
      totalRevenue: 0,
      totalQuantity: 0,
      averageOrderValue: 0,
      completedOrders: 0,
      pendingOrders: 0,
      totalDiscount: 0,
      totalTax: 0,
      totalItems: 0
    };

    orders.forEach(orderData => {
      const order = orderData.SalesOrder;
      stats.totalRevenue += order.NetAmount || 0;
      stats.totalQuantity += order.QtyCount || 0;
      stats.totalDiscount += order.TotalDiscount || 0;
      stats.totalTax += order.TotalTax || 0;
      stats.totalItems += order.LineCount || 0;
      
      if (order.Status === 'DELIVERED' || order.Status === 'COMPLETED') {
        stats.completedOrders++;
      } else {
        stats.pendingOrders++;
      }
    });

    stats.averageOrderValue = stats.totalOrders > 0 ? stats.totalRevenue / stats.totalOrders : 0;

    setSummary(stats);
  };

  const toggleOrderExpansion = async (orderUID: string) => {
    const newExpanded = new Set(expandedOrders);
    
    if (newExpanded.has(orderUID)) {
      newExpanded.delete(orderUID);
    } else {
      newExpanded.add(orderUID);
      
      // Load order lines if not already loaded
      if (!orderLines[orderUID]) {
        await loadOrderLines(orderUID);
      }
    }
    
    setExpandedOrders(newExpanded);
  };

  const loadOrderLines = async (orderUID: string) => {
    try {
      setLoadingLines(prev => new Set(prev).add(orderUID));
      const response = await salesOrderService.getSalesOrderLines(orderUID);
      
      if (response?.Data) {
        setOrderLines(prev => ({
          ...prev,
          [orderUID]: response.Data
        }));
      }
    } catch (err) {
      console.error('Error loading order lines:', err);
    } finally {
      setLoadingLines(prev => {
        const newSet = new Set(prev);
        newSet.delete(orderUID);
        return newSet;
      });
    }
  };

  const getStatusIcon = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'DELIVERED':
      case 'COMPLETED':
        return <CheckCircle className="h-5 w-5 text-green-500" />;
      case 'CANCELLED':
      case 'FAILED':
        return <XCircle className="h-5 w-5 text-red-500" />;
      case 'PENDING':
      case 'PROCESSING':
        return <AlertCircle className="h-5 w-5 text-orange-500" />;
      default:
        return <Info className="h-5 w-5 text-gray-500" />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status?.toUpperCase()) {
      case 'DELIVERED':
      case 'COMPLETED':
        return 'bg-green-100 text-green-800 border-green-200';
      case 'CANCELLED':
      case 'FAILED':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'PENDING':
      case 'PROCESSING':
        return 'bg-orange-100 text-orange-800 border-orange-200';
      default:
        return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('en-US', {
      minimumFractionDigits: 2,
      maximumFractionDigits: 2
    }).format(amount || 0);
  };

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        {/* Header Skeleton */}
        <div className="mb-6">
          <Skeleton className="h-8 w-48 mb-2" />
          <Skeleton className="h-4 w-64" />
        </div>

        {/* Summary Cards Skeleton */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} className="p-4">
              <Skeleton className="h-12 w-12 mb-3" />
              <Skeleton className="h-6 w-24 mb-2" />
              <Skeleton className="h-4 w-32" />
            </Card>
          ))}
        </div>

        {/* Table Skeleton */}
        <Card className="p-6">
          <Skeleton className="h-6 w-32 mb-4" />
          <div className="space-y-3">
            {[1, 2, 3].map((i) => (
              <Skeleton key={i} className="h-16 w-full" />
            ))}
          </div>
        </Card>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto p-6">
        <Card className="p-6">
          <div className="text-center">
            <XCircle className="h-12 w-12 text-red-500 mx-auto mb-4" />
            <h2 className="text-xl font-semibold mb-2">Error Loading Sales Orders</h2>
            <p className="text-gray-600 mb-4">{error}</p>
            <Button onClick={() => router.back()}>
              <ArrowLeft className="h-4 w-4 mr-2" />
              Go Back
            </Button>
          </div>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center gap-4 mb-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => router.back()}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back
          </Button>
        </div>
        <h1 className="text-2xl font-bold flex items-center gap-3">
          <ShoppingCart className="h-6 w-6 text-blue-600" />
          Sales Orders Report
        </h1>
        <p className="text-gray-600">Route: {routeUID}</p>
      </div>

      {/* Summary Dashboard */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-6">
        {/* Total Orders Card */}
        <Card className="p-4 border-l-4 border-blue-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Orders</p>
              <p className="text-2xl font-bold text-gray-900">{summary.totalOrders}</p>
              <div className="flex gap-4 mt-2">
                <span className="text-xs text-green-600">
                  ✓ {summary.completedOrders} Completed
                </span>
                <span className="text-xs text-orange-600">
                  ⏳ {summary.pendingOrders} Pending
                </span>
              </div>
            </div>
            <div className="p-3 bg-blue-100 rounded-lg">
              <ShoppingCart className="h-6 w-6 text-blue-600" />
            </div>
          </div>
        </Card>

        {/* Total Revenue Card */}
        <Card className="p-4 border-l-4 border-green-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Revenue</p>
              <p className="text-2xl font-bold text-gray-900">{formatCurrency(summary.totalRevenue)}</p>
              <p className="text-xs text-gray-500 mt-1">
                Avg: {formatCurrency(summary.averageOrderValue)}/order
              </p>
            </div>
            <div className="p-3 bg-green-100 rounded-lg">
              <DollarSign className="h-6 w-6 text-green-600" />
            </div>
          </div>
        </Card>

        {/* Total Items Card */}
        <Card className="p-4 border-l-4 border-purple-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Total Items</p>
              <p className="text-2xl font-bold text-gray-900">{summary.totalItems}</p>
              <p className="text-xs text-gray-500 mt-1">
                Qty: {summary.totalQuantity} units
              </p>
            </div>
            <div className="p-3 bg-purple-100 rounded-lg">
              <Package className="h-6 w-6 text-purple-600" />
            </div>
          </div>
        </Card>

        {/* Discounts & Tax Card */}
        <Card className="p-4 border-l-4 border-orange-500">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 mb-1">Discounts & Tax</p>
              <div className="space-y-1">
                <p className="text-sm">
                  <span className="text-gray-600">Discount:</span>
                  <span className="font-semibold text-red-600 ml-2">-{formatCurrency(summary.totalDiscount)}</span>
                </p>
                <p className="text-sm">
                  <span className="text-gray-600">Tax:</span>
                  <span className="font-semibold text-blue-600 ml-2">+{formatCurrency(summary.totalTax)}</span>
                </p>
              </div>
            </div>
            <div className="p-3 bg-orange-100 rounded-lg">
              <Calculator className="h-6 w-6 text-orange-600" />
            </div>
          </div>
        </Card>
      </div>

      {/* Orders Table */}
      <Card className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <Receipt className="h-5 w-5 text-gray-600" />
            Order Details
          </h2>
          <span className="text-sm text-gray-500">
            {salesOrders.length} orders found
          </span>
        </div>

        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-10"></TableHead>
                <TableHead>Order Number</TableHead>
                <TableHead>Date & Time</TableHead>
                <TableHead>Store</TableHead>
                <TableHead>Items</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Payment</TableHead>
                <TableHead>Employee</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {salesOrders.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8">
                    <ShoppingCart className="h-12 w-12 text-gray-300 mx-auto mb-3" />
                    <p className="text-gray-500">No sales orders found for this route</p>
                  </TableCell>
                </TableRow>
              ) : (
                salesOrders.map((orderData) => {
                  const order = orderData.SalesOrder;
                  const isExpanded = expandedOrders.has(order.UID);
                  const isLoadingLines = loadingLines.has(order.UID);
                  const lines = orderLines[order.UID] || [];

                  return (
                    <React.Fragment key={order.UID}>
                      <TableRow 
                        className="cursor-pointer hover:bg-gray-50"
                        onClick={() => toggleOrderExpansion(order.UID)}
                      >
                        <TableCell>
                          {isExpanded ? (
                            <ChevronDown className="h-4 w-4 text-gray-500" />
                          ) : (
                            <ChevronRight className="h-4 w-4 text-gray-500" />
                          )}
                        </TableCell>
                        <TableCell className="font-medium">
                          {order.SalesOrderNumber || order.DraftOrderNumber || 'N/A'}
                        </TableCell>
                        <TableCell>
                          {order.OrderDate ? (
                            <div>
                              <p className="text-sm">{format(new Date(order.OrderDate), 'dd MMM yyyy')}</p>
                              <p className="text-xs text-gray-500">{format(new Date(order.OrderDate), 'HH:mm')}</p>
                            </div>
                          ) : 'N/A'}
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Store className="h-4 w-4 text-gray-500" />
                            <span className="text-sm">{order.StoreUID || 'N/A'}</span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <Package className="h-4 w-4 text-gray-500" />
                            <span>{order.LineCount || 0}</span>
                            <span className="text-xs text-gray-500">({order.QtyCount || 0} qty)</span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div>
                            <p className="font-semibold">{formatCurrency(order.NetAmount)}</p>
                            <p className="text-xs text-gray-500">Total: {formatCurrency(order.TotalAmount)}</p>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            {getStatusIcon(order.Status)}
                            <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(order.Status)}`}>
                              {order.Status || 'Unknown'}
                            </span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <CreditCard className="h-4 w-4 text-gray-500" />
                            <span className="text-sm">{order.PaymentType || 'N/A'}</span>
                          </div>
                        </TableCell>
                        <TableCell>
                          <div className="flex items-center gap-2">
                            <User className="h-4 w-4 text-gray-500" />
                            <span className="text-sm">{order.EmpUID || 'N/A'}</span>
                          </div>
                        </TableCell>
                      </TableRow>

                      {/* Expanded Order Details */}
                      {isExpanded && (
                        <TableRow>
                          <TableCell colSpan={9} className="bg-gray-50 p-0">
                            <div className="p-6">
                              {/* Order Additional Details */}
                              <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 mb-6">
                                {/* Order Information */}
                                <div className="bg-white rounded-lg p-4 border border-gray-200">
                                  <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
                                    <FileText className="h-4 w-4 text-blue-600" />
                                    Order Information
                                  </h4>
                                  <div className="space-y-2 text-sm">
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Order Type:</span>
                                      <span className="font-medium">{order.OrderType || 'N/A'}</span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Source:</span>
                                      <span className="font-medium">{order.Source || 'N/A'}</span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Reference:</span>
                                      <span className="font-medium">{order.ReferenceNumber || 'N/A'}</span>
                                    </div>
                                    {order.CustomerPO && (
                                      <div className="flex justify-between">
                                        <span className="text-gray-600">Customer PO:</span>
                                        <span className="font-medium">{order.CustomerPO}</span>
                                      </div>
                                    )}
                                  </div>
                                </div>

                                {/* Financial Summary */}
                                <div className="bg-white rounded-lg p-4 border border-gray-200">
                                  <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
                                    <DollarSign className="h-4 w-4 text-green-600" />
                                    Financial Summary
                                  </h4>
                                  <div className="space-y-2 text-sm">
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Subtotal:</span>
                                      <span className="font-medium">{formatCurrency(order.TotalAmount)}</span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Discount:</span>
                                      <span className="font-medium text-red-600">-{formatCurrency(order.TotalDiscount)}</span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Tax:</span>
                                      <span className="font-medium">+{formatCurrency(order.TotalTax)}</span>
                                    </div>
                                    <div className="pt-2 border-t border-gray-200">
                                      <div className="flex justify-between">
                                        <span className="text-gray-900 font-semibold">Net Total:</span>
                                        <span className="font-bold text-green-600">{formatCurrency(order.NetAmount)}</span>
                                      </div>
                                    </div>
                                  </div>
                                </div>

                                {/* Delivery Information */}
                                <div className="bg-white rounded-lg p-4 border border-gray-200">
                                  <h4 className="font-semibold text-gray-900 mb-3 flex items-center gap-2">
                                    <Truck className="h-4 w-4 text-purple-600" />
                                    Delivery Information
                                  </h4>
                                  <div className="space-y-2 text-sm">
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Expected:</span>
                                      <span className="font-medium">
                                        {(() => {
                                          if (!order.ExpectedDeliveryDate) return 'N/A';
                                          const date = new Date(order.ExpectedDeliveryDate);
                                          // Check for invalid date or year 0001
                                          if (isNaN(date.getTime()) || date.getFullYear() <= 1) {
                                            return 'N/A';
                                          }
                                          return format(date, 'dd MMM yyyy');
                                        })()}
                                      </span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Delivered:</span>
                                      <span className="font-medium">
                                        {(() => {
                                          if (!order.DeliveredDateTime) return 'Not delivered';
                                          const date = new Date(order.DeliveredDateTime);
                                          // Check for invalid date or year 0001
                                          if (isNaN(date.getTime()) || date.getFullYear() <= 1) {
                                            return 'Not delivered';
                                          }
                                          return format(date, 'dd MMM yyyy HH:mm');
                                        })()}
                                      </span>
                                    </div>
                                    {order.VehicleUID && (
                                      <div className="flex justify-between">
                                        <span className="text-gray-600">Vehicle:</span>
                                        <span className="font-medium">{order.VehicleUID}</span>
                                      </div>
                                    )}
                                  </div>
                                </div>
                              </div>

                              {/* Order Lines */}
                              <div className="bg-white rounded-lg border border-gray-200">
                                <div className="p-4 border-b border-gray-200">
                                  <h4 className="font-semibold text-gray-900 flex items-center gap-2">
                                    <ShoppingBag className="h-4 w-4 text-orange-600" />
                                    Order Items
                                  </h4>
                                </div>
                                
                                {isLoadingLines ? (
                                  <div className="p-4 space-y-2">
                                    <Skeleton className="h-10 w-full" />
                                    <Skeleton className="h-10 w-full" />
                                  </div>
                                ) : lines.length > 0 ? (
                                  <div className="overflow-x-auto">
                                    <table className="w-full">
                                      <thead className="bg-gray-50">
                                        <tr className="text-left text-xs text-gray-500 uppercase">
                                          <th className="px-4 py-2">#</th>
                                          <th className="px-4 py-2">Item Code</th>
                                          <th className="px-4 py-2">Description</th>
                                          <th className="px-4 py-2">UOM</th>
                                          <th className="px-4 py-2">Qty</th>
                                          <th className="px-4 py-2">Unit Price</th>
                                          <th className="px-4 py-2">Discount</th>
                                          <th className="px-4 py-2">Tax</th>
                                          <th className="px-4 py-2">Net Amount</th>
                                        </tr>
                                      </thead>
                                      <tbody className="divide-y divide-gray-200">
                                        {lines.map((line) => (
                                          <tr key={line.UID} className="text-sm">
                                            <td className="px-4 py-3">{line.LineNumber}</td>
                                            <td className="px-4 py-3 font-medium">{line.ItemCode || 'N/A'}</td>
                                            <td className="px-4 py-3">{line.ItemName || line.ItemType || 'N/A'}</td>
                                            <td className="px-4 py-3">{line.UoM || 'N/A'}</td>
                                            <td className="px-4 py-3">
                                              <div>
                                                <p className="font-medium">{line.Qty}</p>
                                                {line.DeliveredQty !== line.Qty && (
                                                  <p className="text-xs text-gray-500">
                                                    Delivered: {line.DeliveredQty || 0}
                                                  </p>
                                                )}
                                              </div>
                                            </td>
                                            <td className="px-4 py-3">{formatCurrency(line.UnitPrice)}</td>
                                            <td className="px-4 py-3 text-red-600">
                                              {line.TotalDiscount ? `-${formatCurrency(line.TotalDiscount)}` : '-'}
                                            </td>
                                            <td className="px-4 py-3">
                                              {line.TotalTax ? formatCurrency(line.TotalTax) : '-'}
                                            </td>
                                            <td className="px-4 py-3 font-semibold">
                                              {formatCurrency(line.NetAmount)}
                                            </td>
                                          </tr>
                                        ))}
                                      </tbody>
                                    </table>
                                  </div>
                                ) : (
                                  <div className="p-8 text-center text-gray-500">
                                    <Package className="h-8 w-8 text-gray-300 mx-auto mb-2" />
                                    No line items found
                                  </div>
                                )}
                              </div>

                              {/* Notes and Remarks */}
                              {(order.Notes || order.Remarks || order.DeliveryInstructions) && (
                                <div className="mt-6 bg-yellow-50 rounded-lg p-4 border border-yellow-200">
                                  <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                                    <Info className="h-4 w-4 text-yellow-600" />
                                    Additional Information
                                  </h4>
                                  <div className="space-y-2 text-sm">
                                    {order.Notes && (
                                      <div>
                                        <span className="font-medium text-gray-700">Notes:</span>
                                        <p className="text-gray-600 mt-1">{order.Notes}</p>
                                      </div>
                                    )}
                                    {order.Remarks && (
                                      <div>
                                        <span className="font-medium text-gray-700">Remarks:</span>
                                        <p className="text-gray-600 mt-1">{order.Remarks}</p>
                                      </div>
                                    )}
                                    {order.DeliveryInstructions && (
                                      <div>
                                        <span className="font-medium text-gray-700">Delivery Instructions:</span>
                                        <p className="text-gray-600 mt-1">{order.DeliveryInstructions}</p>
                                      </div>
                                    )}
                                  </div>
                                </div>
                              )}
                            </div>
                          </TableCell>
                        </TableRow>
                      )}
                    </React.Fragment>
                  );
                })
              )}
            </TableBody>
          </Table>
        </div>
      </Card>
    </div>
  );
}