'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { 
  Table, 
  TableBody, 
  TableCell, 
  TableHead, 
  TableHeader, 
  TableRow 
} from '@/components/ui/table';
import { 
  ShoppingCart, 
  Search, 
  Filter, 
  Calendar,
  TrendingUp,
  Package,
  DollarSign,
  Store,
  User,
  ChevronRight,
  Activity,
  BarChart3,
  CheckCircle,
  XCircle,
  AlertCircle,
  Info,
  ChevronDown,
  Receipt,
  CreditCard,
  MapPin,
  FileText,
  Truck,
  Calculator
} from 'lucide-react';
import salesOrderService, { SalesOrder, SalesOrderLine, SalesOrderViewModel } from '@/services/sales-order.service';
import salesTeamActivityService from '@/services/sales-team-activity.service';
import { format } from 'date-fns';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';

export default function SalesOrdersPage() {
  // Helper function to format dates from server
  // Server sends dates in UTC format without 'Z' suffix
  // We need to treat them as UTC and convert to local time
  const formatServerDate = (dateString: string, formatStr: string) => {
    if (!dateString) return 'N/A';
    try {
      // Server sends: "2025-09-30T08:32:50" which is actually UTC time
      // Add 'Z' to explicitly mark it as UTC, then convert to local
      const utcDateString = dateString.endsWith('Z') ? dateString : dateString + 'Z';
      const date = new Date(utcDateString);

      if (isNaN(date.getTime()) || date.getFullYear() <= 1) {
        return 'N/A';
      }

      return format(date, formatStr);
    } catch {
      return 'N/A';
    }
  };
  const router = useRouter();
  const searchParams = useSearchParams();
  const routeFilter = searchParams.get('route');
  
  const [salesOrders, setSalesOrders] = useState<SalesOrderViewModel[]>([]);
  const [allSalesOrders, setAllSalesOrders] = useState<SalesOrderViewModel[]>([]); // Store unfiltered orders
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedRoute, setSelectedRoute] = useState(routeFilter || 'all');
  const [selectedRole, setSelectedRole] = useState('');
  const [selectedEmployee, setSelectedEmployee] = useState('');
  const [dateFrom, setDateFrom] = useState('');
  const [dateTo, setDateTo] = useState('');
  const [expandedOrders, setExpandedOrders] = useState<Set<string>>(new Set());
  const [orderLines, setOrderLines] = useState<{ [key: string]: SalesOrderLine[] }>({});
  const [loadingLines, setLoadingLines] = useState<Set<string>>(new Set());
  
  // Data states for filters
  const [jobPositions, setJobPositions] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);

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

  // Available routes (extracted from actual data)
  const [availableRoutes] = useState([
    { value: 'all', label: 'All Routes' },
    { value: 'RTMERCHAND282', label: 'RTMERCHAND282 - Merchandiser Route' },
    { value: 'RTPROMOTER572', label: 'RTPROMOTER572 - Promoter Route' },
    { value: 'RTTEST349', label: 'RTTEST349 - Test Route' }
  ]);

  // Helper function to extract route from beat_history_uid
  const extractRouteFromBeatHistory = (beatHistoryUID: string) => {
    if (!beatHistoryUID) return 'N/A';
    // Format: "RTMERCHAND282_2025_09_15" -> "RTMERCHAND282"
    return beatHistoryUID.split('_')[0] || beatHistoryUID;
  };

  // Load roles for role filter
  const loadRoles = async () => {
    try {
      const roles = await salesTeamActivityService.getRoles();
      setJobPositions(roles);
      console.log(`Loaded ${roles.length} roles`);
    } catch (error) {
      console.error('Error loading roles:', error);
    }
  };

  // Load employees for employee filter
  const loadEmployees = async (roleUID?: string) => {
    try {
      const empData = await salesTeamActivityService.getEmployees('EPIC01', roleUID);
      setEmployees(empData);
      console.log(`Loaded ${empData.length} employees${roleUID ? ` for role ${roleUID}` : ''}`);
    } catch (error) {
      console.error('Error loading employees:', error);
    }
  };

  // Apply client-side filters
  const applyFilters = (ordersList: SalesOrderViewModel[] = allSalesOrders) => {
    console.log('🔍 Applying filters:', {
      ordersCount: ordersList.length,
      searchTerm,
      selectedRoute,
      selectedRole,
      selectedEmployee,
      dateFrom,
      dateTo
    });

    let filteredOrders = [...ordersList];

    // Filter by date range
    if (dateFrom || dateTo) {
      filteredOrders = filteredOrders.filter(orderData => {
        const order = orderData.SalesOrder;
        if (!order.OrderDate) return false;

        try {
          const orderDate = new Date(order.OrderDate);
          orderDate.setHours(0, 0, 0, 0); // Reset time to compare only dates

          if (dateFrom) {
            const fromDate = new Date(dateFrom);
            fromDate.setHours(0, 0, 0, 0);
            if (orderDate < fromDate) return false;
          }

          if (dateTo) {
            const toDate = new Date(dateTo);
            toDate.setHours(23, 59, 59, 999); // End of day
            if (orderDate > toDate) return false;
          }

          return true;
        } catch {
          return false;
        }
      });
    }

    // Filter by route
    if (selectedRoute !== 'all') {
      filteredOrders = filteredOrders.filter(orderData => {
        const order = orderData.SalesOrder;
        const routeFromBeat = extractRouteFromBeatHistory(order.BeatHistoryUID || '');
        return routeFromBeat === selectedRoute;
      });
    }

    // Filter by search term
    if (searchTerm.trim()) {
      const searchLower = searchTerm.toLowerCase();
      filteredOrders = filteredOrders.filter(orderData => {
        const order = orderData.SalesOrder;
        const routeFromBeat = extractRouteFromBeatHistory(order.BeatHistoryUID || '');
        return (
          order.SalesOrderNumber?.toLowerCase().includes(searchLower) ||
          order.DraftOrderNumber?.toLowerCase().includes(searchLower) ||
          order.StoreUID?.toLowerCase().includes(searchLower) ||
          order.EmpUID?.toLowerCase().includes(searchLower) ||
          routeFromBeat.toLowerCase().includes(searchLower) ||
          order.BeatHistoryUID?.toLowerCase().includes(searchLower)
        );
      });
    }

    // Filter by role (using EmpUID to match against employees with that role)
    if (selectedRole) {
      const roleEmployees = employees.filter(emp => emp.RoleUID === selectedRole).map(emp => emp.UID);
      if (roleEmployees.length > 0) {
        filteredOrders = filteredOrders.filter(orderData => {
          const order = orderData.SalesOrder;
          return roleEmployees.includes(order.EmpUID);
        });
      }
    }

    // Filter by employee
    if (selectedEmployee) {
      filteredOrders = filteredOrders.filter(orderData => {
        const order = orderData.SalesOrder;
        return order.EmpUID === selectedEmployee ||
               order.CreatedBy === selectedEmployee ||
               order.ModifiedBy === selectedEmployee;
      });
    }

    console.log(`🔍 Final filtered orders: ${filteredOrders.length}`);
    setSalesOrders(filteredOrders);
    calculateSummary(filteredOrders, orderLines);
  };

  // Load initial data
  useEffect(() => {
    loadSalesOrders();
    loadRoles();
    loadEmployees();
  }, []);

  // Reload employees when role changes
  useEffect(() => {
    if (selectedRole) {
      console.log('🔄 Role changed, reloading employees for role:', selectedRole);
      loadEmployees(selectedRole);
    } else {
      console.log('📄 Loading all employees (no role filter)');
      loadEmployees();
    }
  }, [selectedRole]);

  // Apply filters when filter values change
  useEffect(() => {
    if (allSalesOrders.length > 0) {
      applyFilters();
    }
  }, [searchTerm, selectedRoute, selectedRole, selectedEmployee, dateFrom, dateTo, allSalesOrders]);

  const loadSalesOrders = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Load all sales orders (increased page size to get more data)
      const response = await salesOrderService.getAllSalesOrders(1, 1000);
      
      if (response?.Data) {
        const ordersData = response.Data as SalesOrderViewModel[];
        setAllSalesOrders(ordersData); // Store all orders
        applyFilters(ordersData); // Apply current filters
        console.log(`Loaded ${ordersData.length} sales orders`);
      }
    } catch (err) {
      console.error('Error loading sales orders:', err);
      setError('Failed to load sales orders');
      setAllSalesOrders([]);
      setSalesOrders([]);
    } finally {
      setIsLoading(false);
    }
  };

  const calculateSummary = (orders: SalesOrderViewModel[], orderLinesData: { [key: string]: SalesOrderLine[] } = {}) => {
    console.log('🧮 Calculating summary for orders:', orders.length);

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

    orders.forEach((orderData, index) => {
      const order = orderData.SalesOrder;

      // Fix calculation logic - use line items total if available and different, otherwise use header total
      let orderAmount = 0;

      // Check if we have line items for this order in the expanded state
      const linesForThisOrder = orderLinesData[order.UID] || [];

      if (linesForThisOrder.length > 0) {
        const lineItemsTotal = linesForThisOrder.reduce((sum, line) => sum + (Number(line.NetAmount) || 0), 0);
        const headerTotal = Number(order.NetAmount) > 0 ? Number(order.NetAmount) : Number(order.TotalAmount);

        // Use line items total if it's significantly different and higher than header total
        if (lineItemsTotal > headerTotal && Math.abs(lineItemsTotal - headerTotal) > 0.01) {
          orderAmount = lineItemsTotal;
        } else {
          orderAmount = headerTotal;
        }
      } else {
        // Fallback to header totals
        if (Number(order.NetAmount) > 0) {
          orderAmount = Number(order.NetAmount);
        } else if (Number(order.TotalAmount) > 0) {
          orderAmount = Number(order.TotalAmount);
        }
      }

      // For very detailed debugging, log the specific problematic order
      if (order.SalesOrderNumber === 'SO_SELLOUT_1759128597756_1000' || index < 3) {
        console.log(`💰 Amount calculation for ${order.SalesOrderNumber}:`, {
          NetAmount: order.NetAmount,
          TotalAmount: order.TotalAmount,
          lineItemsCount: linesForThisOrder.length,
          lineItemsTotal: linesForThisOrder.length > 0 ? linesForThisOrder.reduce((sum, line) => sum + (Number(line.NetAmount) || 0), 0) : 'N/A',
          finalAmount: orderAmount,
          TotalDiscount: order.TotalDiscount,
          TotalTax: order.TotalTax,
          TotalLineTax: order.TotalLineTax,
          TotalHeaderTax: order.TotalHeaderTax,
          TotalExciseDuty: order.TotalExciseDuty
        });
      }
      const orderQty = Number(order.QtyCount) || 0;
      const orderDiscount = Number(order.TotalDiscount) || 0;
      const orderTax = Number(order.TotalTax) || 0;
      const orderLines = Number(order.LineCount) || 0;

      // Debug logging for problematic calculations
      if (index < 3) { // Log first 3 orders for debugging
        console.log(`📊 Order ${index + 1} (${order.SalesOrderNumber}):`, {
          NetAmount: order.NetAmount,
          TotalAmount: order.TotalAmount,
          calculatedAmount: orderAmount,
          QtyCount: order.QtyCount,
          LineCount: order.LineCount,
          TotalDiscount: order.TotalDiscount,
          TotalTax: order.TotalTax
        });
      }

      stats.totalRevenue += orderAmount;
      stats.totalQuantity += orderQty;
      stats.totalDiscount += orderDiscount;
      stats.totalTax += orderTax;
      stats.totalItems += orderLines;

      // Fix status comparison to be case-insensitive
      const status = (order.Status || '').toUpperCase();
      if (status === 'DELIVERED' || status === 'COMPLETED') {
        stats.completedOrders++;
      } else {
        stats.pendingOrders++;
      }
    });

    stats.averageOrderValue = stats.totalOrders > 0 ? stats.totalRevenue / stats.totalOrders : 0;

    console.log('📈 Final summary stats:', stats);
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

      // Debug: Check what tax fields are available
      if (response && response.length > 0) {
        console.log('🔍 Tax field check for first line item:', {
          TotalTax: response[0].TotalTax,
          LineTaxAmount: response[0].LineTaxAmount,
          ProrataTaxAmount: response[0].ProrataTaxAmount,
          allFields: Object.keys(response[0])
        });
      }
      
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

  // Handle filter changes
  const handleSearch = (value: string) => {
    setSearchTerm(value);
  };

  const handleRouteChange = (value: string) => {
    setSelectedRoute(value);
  };

  const handleRoleChange = (value: string) => {
    const newRoleValue = value === 'all' ? '' : value;
    setSelectedRole(newRoleValue);
    
    // Clear current employee selection since role changed
    if (selectedEmployee) {
      setSelectedEmployee('');
    }
  };

  const handleEmployeeChange = (value: string) => {
    setSelectedEmployee(value === 'all' ? '' : value);
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
            <Button onClick={loadSalesOrders}>
              Try Again
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
        <h1 className="text-2xl font-bold flex items-center gap-3">
          <ShoppingCart className="h-6 w-6 text-blue-600" />
          Sales Orders Report
        </h1>
        <p className="text-gray-600">
          {selectedRoute === 'all' ? 'All sales orders' : `Sales orders for route: ${selectedRoute}`}
        </p>
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
                  {summary.completedOrders} Completed
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

      {/* Filters Section */}
      <Card className="p-3 mb-4 border-0 bg-muted/30">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-7 gap-3">
          {/* Date From */}
          <div className="relative">
            <Calendar className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground pointer-events-none" />
            <Input
              type="date"
              value={dateFrom}
              onChange={(e) => setDateFrom(e.target.value)}
              placeholder="From Date"
              className="pl-8 h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary"
            />
          </div>

          {/* Date To */}
          <div className="relative">
            <Calendar className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground pointer-events-none" />
            <Input
              type="date"
              value={dateTo}
              onChange={(e) => setDateTo(e.target.value)}
              placeholder="To Date"
              className="pl-8 h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary"
            />
          </div>

          {/* Search */}
          <div className="relative">
            <Search className="absolute left-2 top-1/2 transform -translate-y-1/2 h-3.5 w-3.5 text-muted-foreground" />
            <Input
              type="text"
              placeholder="Search orders..."
              value={searchTerm}
              onChange={(e) => handleSearch(e.target.value)}
              className="pl-8 h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary"
            />
          </div>

          {/* Route Filter */}
          <Select value={selectedRoute} onValueChange={handleRouteChange}>
            <SelectTrigger className="h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary">
              <SelectValue placeholder="Route" />
            </SelectTrigger>
            <SelectContent>
              {availableRoutes.map((route) => (
                <SelectItem key={route.value} value={route.value}>
                  {route.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* Role Filter */}
          <Select value={selectedRole || 'all'} onValueChange={handleRoleChange}>
            <SelectTrigger className="h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary">
              <SelectValue placeholder="Role" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Roles</SelectItem>
              {jobPositions.map((role) => (
                <SelectItem key={role.UID} value={role.UID}>
                  {role.Name || role.Code || role.UID}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* Employee Filter */}
          <Select value={selectedEmployee || 'all'} onValueChange={handleEmployeeChange}>
            <SelectTrigger className="h-9 text-sm bg-background border-muted-foreground/20 focus:border-primary">
              <SelectValue placeholder="Employee" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Employees</SelectItem>
              {employees.map((employee) => (
                <SelectItem key={employee.UID} value={employee.UID}>
                  {employee.Label || employee.Name || employee.Code || employee.UID}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>

          {/* Clear Filters Button */}
          <Button
            variant="outline"
            onClick={() => {
              setSearchTerm('');
              setSelectedRoute('all');
              setSelectedRole('');
              setSelectedEmployee('');
            }}
            className="h-9 text-sm"
          >
            Clear Filters
          </Button>
        </div>

        {/* Filter Summary */}
        {(searchTerm || selectedRoute !== 'all' || selectedRole || selectedEmployee) && (
          <div className="flex items-center gap-2 text-sm text-muted-foreground mt-3">
            <span>Active filters:</span>
            {searchTerm && (
              <Badge variant="secondary" className="text-xs">
                Search: {searchTerm}
              </Badge>
            )}
            {selectedRoute !== 'all' && (
              <Badge variant="secondary" className="text-xs">
                Route: {availableRoutes.find(r => r.value === selectedRoute)?.label || selectedRoute}
              </Badge>
            )}
            {selectedRole && (
              <Badge variant="secondary" className="text-xs">
                Role: {jobPositions.find(r => r.UID === selectedRole)?.Name || jobPositions.find(r => r.UID === selectedRole)?.Code || selectedRole}
              </Badge>
            )}
            {selectedEmployee && (
              <Badge variant="secondary" className="text-xs">
                Employee: {employees.find(e => e.UID === selectedEmployee)?.Label || employees.find(e => e.UID === selectedEmployee)?.Name || selectedEmployee}
              </Badge>
            )}
          </div>
        )}
      </Card>

      {/* Orders Table */}
      <Card className="p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-semibold flex items-center gap-2">
            <Receipt className="h-5 w-5 text-gray-600" />
            Sales Orders
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
                <TableHead>Route</TableHead>
                <TableHead>Items</TableHead>
                <TableHead>Amount</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Employee</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {salesOrders.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} className="text-center py-8">
                    <ShoppingCart className="h-12 w-12 text-gray-300 mx-auto mb-3" />
                    <p className="text-gray-500">No sales orders found</p>
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
                              <p className="text-sm">{formatServerDate(order.OrderDate, 'dd MMM yyyy')}</p>
                              <p className="text-xs text-gray-500">{formatServerDate(order.OrderDate, 'HH:mm')}</p>
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
                            <MapPin className="h-4 w-4 text-gray-500" />
                            <span className="text-sm">{extractRouteFromBeatHistory(order.BeatHistoryUID || '')}</span>
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
                            <p className="font-semibold">
                              {(() => {
                                // Calculate correct amount - if we have line items loaded, use their total
                                const linesForThisOrder = orderLines[order.UID] || [];
                                if (linesForThisOrder.length > 0) {
                                  const lineItemsTotal = linesForThisOrder.reduce((sum, line) => sum + (Number(line.NetAmount) || 0), 0);
                                  const headerTotal = Number(order.NetAmount) > 0 ? Number(order.NetAmount) : Number(order.TotalAmount);

                                  // If line items total is significantly different and higher, use line items total
                                  if (lineItemsTotal > headerTotal && Math.abs(lineItemsTotal - headerTotal) > 0.01) {
                                    return formatCurrency(lineItemsTotal);
                                  }
                                }
                                // Otherwise use header total
                                return formatCurrency(Number(order.NetAmount) > 0 ? order.NetAmount : order.TotalAmount);
                              })()}
                            </p>
                          </div>
                        </TableCell>
                        <TableCell>
                          <span className={`px-2 py-1 rounded-full text-xs font-medium border ${getStatusColor(order.Status)}`}>
                            {order.Status || 'Unknown'}
                          </span>
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
                                  <h4 className="font-semibold text-gray-900 mb-3">
                                    Financial Summary
                                  </h4>
                                  <div className="space-y-2 text-sm">
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Subtotal:</span>
                                      <span className="font-medium">
                                        {(() => {
                                          // Always calculate subtotal from line items if available
                                          if (lines && lines.length > 0) {
                                            // Calculate subtotal from line items (qty * price, before discount/tax)
                                            const lineSubtotal = lines.reduce((sum, line) => {
                                              const qty = Number(line.Qty) || 0;
                                              const price = Number(line.UnitPrice) || 0;
                                              return sum + (qty * price);
                                            }, 0);

                                            // Always use the calculated line items subtotal when we have line items
                                            return formatCurrency(lineSubtotal);
                                          }
                                          // Fall back to header total only if no line items
                                          return formatCurrency(Number(order.TotalAmount) || 0);
                                        })()}
                                      </span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Discount:</span>
                                      <span className="font-medium text-red-600">
                                        -{(() => {
                                          // Always use line items discount total if available
                                          if (lines && lines.length > 0) {
                                            const lineDiscount = lines.reduce((sum, line) => sum + (Number(line.TotalDiscount) || 0), 0);
                                            // Always use the calculated line items discount when we have line items
                                            return formatCurrency(Math.abs(lineDiscount));
                                          }
                                          // Fall back to header discount only if no line items
                                          return formatCurrency(Number(order.TotalDiscount) || 0);
                                        })()}
                                      </span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Tax:</span>
                                      <span className="font-medium">
                                        +{(() => {
                                          // Always use line items tax total if available
                                          if (lines && lines.length > 0) {
                                            const lineTax = lines.reduce((sum, line) => sum + (Number(line.TotalTax) || 0), 0);
                                            // Always use the calculated line items tax when we have line items
                                            return formatCurrency(lineTax);
                                          }
                                          // Fall back to header tax only if no line items
                                          return formatCurrency(Number(order.TotalTax) || 0);
                                        })()}
                                      </span>
                                    </div>
                                    <div className="pt-2 border-t border-gray-200">
                                      <div className="flex justify-between">
                                        <span className="text-gray-900 font-semibold">Net Total:</span>
                                        <span className="font-bold text-green-600">
                                          {(() => {
                                            // Always use the correct calculated total from line items if available
                                            if (lines && lines.length > 0) {
                                              const lineItemsTotal = lines.reduce((sum, line) => {
                                                const qty = Number(line.Qty) || 0;
                                                const unitPrice = Number(line.UnitPrice) || 0;
                                                const discount = Number(line.TotalDiscount) || 0;
                                                const tax = Number(line.TotalTax) || 0;
                                                const calculatedNet = (qty * unitPrice) - discount + tax;
                                                return sum + calculatedNet;
                                              }, 0);
                                              // Always use the calculated line items total when we have line items
                                              return formatCurrency(lineItemsTotal);
                                            }
                                            // Fall back to header total only if no line items
                                            return formatCurrency(Number(order.NetAmount) > 0 ? order.NetAmount : order.TotalAmount);
                                          })()}
                                        </span>
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
                                        {formatServerDate(order.ExpectedDeliveryDate, 'dd MMM yyyy')}
                                      </span>
                                    </div>
                                    <div className="flex justify-between">
                                      <span className="text-gray-600">Delivered:</span>
                                      <span className="font-medium">
                                        {order.DeliveredDateTime ? formatServerDate(order.DeliveredDateTime, 'dd MMM yyyy HH:mm') : 'Not delivered'}
                                      </span>
                                    </div>
                                  </div>
                                </div>
                              </div>

                              {/* Order Lines */}
                              <div className="bg-white rounded-lg border border-gray-200">
                                <div className="p-4 border-b border-gray-200">
                                  <h4 className="font-semibold text-gray-900 flex items-center gap-2">
                                    <Package className="h-4 w-4 text-orange-600" />
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
                                        {(() => {
                                          // Group items by ItemCode and UnitPrice for better display
                                          const groupedItems = lines.reduce((acc, line) => {
                                            const key = `${line.ItemCode}_${line.UnitPrice}`;
                                            if (!acc[key]) {
                                              acc[key] = {
                                                ...line,
                                                Qty: 0,
                                                NetAmount: 0,
                                                TotalDiscount: 0,
                                                TotalTax: 0,
                                                originalLines: []
                                              };
                                            }
                                            acc[key].Qty += Number(line.Qty) || 0;
                                            acc[key].NetAmount += Number(line.NetAmount) || 0;
                                            acc[key].TotalDiscount += Number(line.TotalDiscount) || 0;
                                            acc[key].TotalTax += Number(line.TotalTax) || 0;
                                            acc[key].originalLines.push(line);
                                            return acc;
                                          }, {});

                                          const aggregatedLines = Object.values(groupedItems);

                                          return aggregatedLines.map((groupedLine, index) => {
                                            // Calculate totals
                                            const qty = Number(groupedLine.Qty) || 0;
                                            const unitPrice = Number(groupedLine.UnitPrice) || 0;
                                            const lineDiscount = Number(groupedLine.TotalDiscount) || 0;
                                            const lineTax = Number(groupedLine.TotalTax) || 0;

                                            // Calculate the correct net amount
                                            const subtotal = qty * unitPrice;
                                            const calculatedNetAmount = subtotal - lineDiscount + lineTax;

                                            // Use calculated amount if backend data seems incorrect
                                            const backendNetAmount = Number(groupedLine.NetAmount) || 0;
                                            const netAmount = Math.abs(calculatedNetAmount - backendNetAmount) > 0.01 ? calculatedNetAmount : backendNetAmount;

                                            return (
                                              <tr
                                                key={`${groupedLine.ItemCode}_${index}`}
                                                className="text-sm"
                                              >
                                                <td className="px-4 py-3">
                                                  {index + 1}
                                                </td>
                                                <td className="px-4 py-3 font-medium">
                                                  {groupedLine.ItemCode || 'N/A'}
                                                </td>
                                                <td className="px-4 py-3">{groupedLine.ItemName || groupedLine.ItemCode || 'N/A'}</td>
                                                <td className="px-4 py-3">{groupedLine.UoM || 'N/A'}</td>
                                                <td className="px-4 py-3">
                                                  <p className="font-medium">{qty}</p>
                                                </td>
                                                <td className="px-4 py-3">{formatCurrency(unitPrice)}</td>
                                                <td className="px-4 py-3 text-red-600">
                                                  {lineDiscount > 0 ? `-${formatCurrency(lineDiscount)}` : '-'}
                                                </td>
                                                <td className="px-4 py-3">
                                                  {lineTax > 0 ? `+${formatCurrency(lineTax)}` : '-'}
                                                </td>
                                                <td className="px-4 py-3 font-semibold">
                                                  {formatCurrency(netAmount)}
                                                </td>
                                              </tr>
                                            );
                                          });
                                        })()}

                                        {/* Summary Row - Based on actual line items (not aggregated) */}
                                        {lines.length > 0 && (() => {
                                          const totalQty = lines.reduce((sum, line) => sum + (Number(line.Qty) || 0), 0);
                                          const totalDiscount = lines.reduce((sum, line) => sum + (Number(line.TotalDiscount) || 0), 0);
                                          const totalTax = lines.reduce((sum, line) => sum + (Number(line.TotalTax) || 0), 0);

                                          // Calculate correct total net amount
                                          const totalNet = lines.reduce((sum, line) => {
                                            const qty = Number(line.Qty) || 0;
                                            const unitPrice = Number(line.UnitPrice) || 0;
                                            const discount = Number(line.TotalDiscount) || 0;
                                            const tax = Number(line.TotalTax) || 0;
                                            const calculatedNet = (qty * unitPrice) - discount + tax;
                                            return sum + calculatedNet;
                                          }, 0);

                                          return (
                                            <tr className="bg-gray-100 font-semibold text-sm">
                                              <td colSpan={4} className="px-4 py-3 text-right">
                                                <strong>Actual Line Items Total:</strong>
                                              </td>
                                              <td className="px-4 py-3">{totalQty}</td>
                                              <td className="px-4 py-3">-</td>
                                              <td className="px-4 py-3 text-red-600">
                                                {totalDiscount > 0 ? `-${formatCurrency(totalDiscount)}` : '-'}
                                              </td>
                                              <td className="px-4 py-3">
                                                {totalTax > 0 ? `+${formatCurrency(totalTax)}` : '-'}
                                              </td>
                                              <td className="px-4 py-3 font-bold text-green-600">
                                                {formatCurrency(totalNet)}
                                              </td>
                                            </tr>
                                          );
                                        })()}
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