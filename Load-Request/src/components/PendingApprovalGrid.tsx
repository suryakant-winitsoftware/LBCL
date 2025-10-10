'use client';

import { useState, useEffect } from 'react';
import { X, Plus, Search, Check, AlertCircle, AlertTriangle, Clock, User, ChevronRight, Calendar, Users, Truck } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { format } from 'date-fns';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Calendar as CalendarComponent } from '@/components/ui/calendar';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';

interface PendingRequestItem {
  request_id: number;
  lsr_id: number;
  lsr_name: string;
  sku: string;
  product_name: string;
  requested_qty: number;
  approved_qty: number;
  stock_available: number;
  waiting_hours: number;
  priority: 'High' | 'Medium' | 'Low';
  customer_name: string;
  journey_date: string;
  customer_distribution?: CustomerDistribution[];
}

interface CustomerDistribution {
  customer: string;
  quantity: number;
  reason: string;
}

interface PendingApprovalGridProps {
  onProcessApproval?: () => void;
}

export default function PendingApprovalGrid({ onProcessApproval }: PendingApprovalGridProps) {
  const [activeTab, setActiveTab] = useState<'Commercial Items' | 'POSM'>('Commercial Items');
  const [loadItems, setLoadItems] = useState<PendingRequestItem[]>([]);
  const [posmItems, setPosmItems] = useState<PendingRequestItem[]>([]);
  const [showBreakdown, setShowBreakdown] = useState(false);
  const [showBulkActions, setShowBulkActions] = useState(false);
  const [selectedItem, setSelectedItem] = useState<PendingRequestItem | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [deliveryDate, setDeliveryDate] = useState<Date | undefined>(new Date());
  const [dateFilter, setDateFilter] = useState<'today' | 'tomorrow' | 'this-week' | 'custom'>('today');
  const [customDateRange, setCustomDateRange] = useState<{from: Date | undefined; to: Date | undefined}>({from: undefined, to: undefined});

  useEffect(() => {
    // Load mock data for pending approval items
    const commercialMockData: PendingRequestItem[] = [
      {
        request_id: 1001,
        lsr_id: 101,
        lsr_name: 'John Smith',
        sku: 'SKU001',
        product_name: 'Choco Bar 90ml',
        requested_qty: 120,
        approved_qty: 100,
        stock_available: 150,
        waiting_hours: 4,
        priority: 'High',
        customer_name: 'Metro Store Downtown',
        journey_date: new Date().toLocaleDateString('en-GB'),
        customer_distribution: [
          { customer: 'Metro Store Downtown', quantity: 40, reason: 'High foot traffic area' },
          { customer: 'SuperMart Mall', quantity: 30, reason: 'Popular weekend destination' },
          { customer: 'City Center Outlet', quantity: 30, reason: 'Regular customer base' }
        ]
      },
      {
        request_id: 1002,
        lsr_id: 102,
        lsr_name: 'Sarah Johnson',
        sku: 'SKU002',
        product_name: 'Vanilla Tub 500ml',
        requested_qty: 60,
        approved_qty: 60,
        stock_available: 80,
        waiting_hours: 2,
        priority: 'Medium',
        customer_name: 'SuperMart Mall',
        journey_date: new Date().toLocaleDateString('en-GB'),
        customer_distribution: [
          { customer: 'SuperMart Mall', quantity: 35, reason: 'Popular weekend destination' },
          { customer: 'City Center Outlet', quantity: 25, reason: 'Regular customer base' }
        ]
      },
      {
        request_id: 1003,
        lsr_id: 103,
        lsr_name: 'Mike Wilson',
        sku: 'SKU003',
        product_name: 'Strawberry Cup 100ml',
        requested_qty: 80,
        approved_qty: 75,
        stock_available: 100,
        waiting_hours: 6,
        priority: 'High',
        customer_name: 'City Center Outlet',
        journey_date: new Date().toLocaleDateString('en-GB'),
        customer_distribution: [
          { customer: 'City Center Outlet', quantity: 40, reason: 'Regular customer base' },
          { customer: 'Express Corner Shop', quantity: 35, reason: 'Quick grab location' }
        ]
      }
    ];

    const posmMockData: PendingRequestItem[] = [
      {
        request_id: 2001,
        lsr_id: 201,
        lsr_name: 'Emily Brown',
        sku: 'POSM001',
        product_name: 'Brand Banner 2x4ft',
        requested_qty: 5,
        approved_qty: 5,
        stock_available: 10,
        waiting_hours: 1,
        priority: 'Low',
        customer_name: 'Metro Store Downtown',
        journey_date: new Date().toLocaleDateString('en-GB')
      },
      {
        request_id: 2002,
        lsr_id: 202,
        lsr_name: 'David Lee',
        sku: 'POSM002',
        product_name: 'Display Stand Metal',
        requested_qty: 3,
        approved_qty: 3,
        stock_available: 8,
        waiting_hours: 3,
        priority: 'Medium',
        customer_name: 'SuperMart Mall',
        journey_date: new Date().toLocaleDateString('en-GB')
      }
    ];

    setLoadItems(commercialMockData);
    setPosmItems(posmMockData);
  }, []);

  const currentItems = activeTab === 'Commercial Items' ? loadItems : posmItems;
  const totalItems = currentItems.length;
  const totalRequestedQty = currentItems.reduce((sum, item) => sum + item.requested_qty, 0);
  const totalApprovedQty = currentItems.reduce((sum, item) => sum + item.approved_qty, 0);

  const handleApprovalChange = (requestId: number, approvedQty: number) => {
    if (activeTab === 'Commercial Items') {
      setLoadItems(prev => prev.map(item => 
        item.request_id === requestId 
          ? { ...item, approved_qty: Math.max(0, Math.min(approvedQty, item.stock_available)) }
          : item
      ));
    } else {
      setPosmItems(prev => prev.map(item => 
        item.request_id === requestId 
          ? { ...item, approved_qty: Math.max(0, Math.min(approvedQty, item.stock_available)) }
          : item
      ));
    }
  };

  const handleShowBreakdown = (item: PendingRequestItem) => {
    setSelectedItem(item);
    setShowBreakdown(true);
  };

  const handleProcessBatch = () => {
    setShowBulkActions(true);
  };

  const handleConfirmApproval = () => {
    if (onProcessApproval) {
      onProcessApproval();
    }
    setShowBulkActions(false);
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High': return 'bg-red-100 text-red-800';
      case 'Medium': return 'bg-yellow-100 text-yellow-800';
      case 'Low': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  const filteredItems = currentItems.filter(item =>
    item.product_name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.sku.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.lsr_name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Header with Stats */}
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold text-gray-900">Pending Approval Grid</h2>
          <p className="text-sm text-muted-foreground mt-1">Review and approve load request items</p>
        </div>
        
        {/* Quick Stats */}
        <div className="flex space-x-3">
          <div className="bg-white rounded-lg px-4 py-2 border shadow-xs">
            <div className="flex items-center space-x-2">
              <Clock className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Items</span>
              <span className="text-lg font-bold text-gray-900">{totalItems}</span>
            </div>
          </div>
          <div className="bg-white rounded-lg px-4 py-2 border shadow-xs">
            <div className="flex items-center space-x-2">
              <AlertCircle className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Requested</span>
              <span className="text-lg font-bold text-gray-900">{totalRequestedQty}</span>
            </div>
          </div>
          <div className="bg-white rounded-lg px-4 py-2 border shadow-xs">
            <div className="flex items-center space-x-2">
              <Check className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Approved</span>
              <span className="text-lg font-bold text-gray-900">{totalApprovedQty}</span>
            </div>
          </div>
        </div>
      </div>
      
      {/* Tabs */}
      <div className="bg-gray-50 rounded-lg p-1 inline-flex">
        <button
          onClick={() => setActiveTab('Commercial Items')}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-all ${
            activeTab === 'Commercial Items'
              ? 'bg-white text-gray-900 shadow-xs'
              : 'text-gray-500 hover:text-gray-700'
          }`}
        >
          Commercial Items
        </button>
        <button
          onClick={() => setActiveTab('POSM')}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-all ml-1 ${
            activeTab === 'POSM'
              ? 'bg-white text-gray-900 shadow-xs'
              : 'text-gray-500 hover:text-gray-700'
          }`}
        >
          POSM
        </button>
      </div>

      {/* Date Filter Section */}
      <div className="bg-white rounded-lg border shadow-xs p-4 space-y-4">
        {/* Journey Date */}
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Label className="text-sm font-medium text-foreground w-40">Journey Date:</Label>
            <Select value={dateFilter} onValueChange={(value: any) => setDateFilter(value)}>
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Select date range" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="today">Today</SelectItem>
                <SelectItem value="tomorrow">Tomorrow</SelectItem>
                <SelectItem value="this-week">This Week</SelectItem>
                <SelectItem value="custom">Custom Date</SelectItem>
              </SelectContent>
            </Select>
            
            {dateFilter === 'custom' && (
              <Popover>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className="w-[240px] justify-start text-left font-normal"
                  >
                    <Calendar className="mr-2 h-4 w-4" />
                    {deliveryDate ? format(deliveryDate, 'PPP') : <span>Pick a date</span>}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0" align="start">
                  <CalendarComponent
                    mode="single"
                    selected={deliveryDate}
                    onSelect={setDeliveryDate}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            )}
            
            {dateFilter !== 'custom' && (
              <div className="text-sm text-muted-foreground">
                {dateFilter === 'today' && `Journey: ${format(new Date(), 'PPP')}`}
                {dateFilter === 'tomorrow' && `Journey: ${format(new Date(new Date().setDate(new Date().getDate() + 1)), 'PPP')}`}
                {dateFilter === 'this-week' && `Journey: ${format(new Date(), 'PP')} - ${format(new Date(new Date().setDate(new Date().getDate() + 7)), 'PP')}`}
              </div>
            )}
          </div>
          
          <div className="flex items-center space-x-2">
            <Badge variant="default" className="text-xs">
              <Truck className="w-3 h-3 mr-1" />
              Journey: {dateFilter === 'custom' && deliveryDate ? format(deliveryDate, 'dd MMM yyyy') : 
               dateFilter === 'today' ? 'Today' :
               dateFilter === 'tomorrow' ? 'Tomorrow' : 
               'This Week'}
            </Badge>
          </div>
        </div>
      </div>

      {/* Search and Controls */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="relative">
            <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
            <Input
              placeholder="Search items..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="pl-10 w-64"
            />
          </div>
        </div>
        <div className="flex space-x-2">
          <Button variant="outline" size="sm">
            <Plus className="w-4 h-4 mr-2" />
            Add Items
          </Button>
          <Button onClick={handleProcessBatch} size="sm">
            <Check className="w-4 h-4 mr-2" />
            Process Batch
          </Button>
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-lg border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b">
          <h3 className="text-lg font-semibold text-foreground">Pending Approvals</h3>
          <p className="text-sm text-muted-foreground">Review and approve load request items</p>
        </div>
        <div className="w-full">
          <Table>
            <TableHeader>
              <TableRow className="bg-muted/30">
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider">SKU</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider">Product Name</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider">LSR</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider text-center">Requested</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider text-center">Stock</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider text-center">Approved</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider">Priority</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider">Customer</TableHead>
                <TableHead className="px-4 py-3 font-semibold text-xs text-muted-foreground uppercase tracking-wider text-center">Action</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredItems.map((item, index) => (
                <TableRow key={item.request_id} className={`hover:bg-muted/20 transition-colors ${index % 2 === 0 ? 'bg-background' : 'bg-muted/10'}`}>
                  <TableCell className="px-4 py-3">
                    <Badge variant="secondary" className="font-medium">
                      {item.sku}
                    </Badge>
                  </TableCell>
                  <TableCell className="px-4 py-3">
                    <div className="text-sm font-medium text-foreground">{item.product_name}</div>
                  </TableCell>
                  <TableCell className="px-4 py-3">
                    <div className="space-y-1">
                      <div className="text-sm font-medium text-foreground">[{item.lsr_id}]</div>
                      <div className="text-xs text-muted-foreground">{item.lsr_name}</div>
                    </div>
                  </TableCell>
                  <TableCell className="px-4 py-3 text-center">
                    <span className="text-sm font-semibold text-foreground">{item.requested_qty}</span>
                  </TableCell>
                  <TableCell className="px-4 py-3 text-center">
                    <span className={`text-sm font-medium ${item.stock_available < item.requested_qty ? 'text-destructive' : 'text-muted-foreground'}`}>
                      {item.stock_available}
                    </span>
                  </TableCell>
                  <TableCell className="px-4 py-3 text-center">
                    <Input
                      type="number"
                      value={item.approved_qty}
                      onChange={(e) => handleApprovalChange(item.request_id, Number(e.target.value))}
                      className="w-20 text-center text-sm font-medium"
                      min={0}
                      max={item.stock_available}
                    />
                  </TableCell>
                  <TableCell className="px-4 py-3">
                    <Badge 
                      variant={item.priority === 'High' ? 'destructive' : item.priority === 'Medium' ? 'secondary' : 'default'}
                      className="text-xs"
                    >
                      {item.priority}
                    </Badge>
                  </TableCell>
                  <TableCell className="px-4 py-3">
                    <div className="text-sm text-muted-foreground">{item.customer_name}</div>
                  </TableCell>
                  <TableCell className="px-4 py-3 text-center">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleShowBreakdown(item)}
                      className="inline-flex items-center space-x-1 text-xs"
                    >
                      <span>Details</span>
                      <ChevronRight className="w-3 h-3" />
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>

        {/* Summary Footer */}
        <div className="bg-gray-50 border-t px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-6">
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-gray-600">Total Items:</span>
                <span className="text-sm font-semibold text-gray-900">{totalItems}</span>
              </div>
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-gray-600">Requested:</span>
                <span className="text-sm font-semibold text-gray-900">{totalRequestedQty}</span>
              </div>
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-gray-600">Approved:</span>
                <span className="text-sm font-semibold text-gray-900">{totalApprovedQty}</span>
              </div>
            </div>
            <div className="text-sm text-muted-foreground">
              {filteredItems.length} of {totalItems} items shown
            </div>
          </div>
        </div>
      </div>

      {/* Customer Distribution Breakdown Modal */}
      <Dialog open={showBreakdown} onOpenChange={setShowBreakdown}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle className="text-lg font-semibold text-gray-900">
              Customer Distribution - {selectedItem?.product_name}
            </DialogTitle>
          </DialogHeader>
          <div className="mt-4">
            <div className="bg-gray-50 rounded-lg p-4 mb-4">
              <p className="text-sm text-gray-600">
                SKU: <span className="font-medium">{selectedItem?.sku}</span>
              </p>
              <p className="text-sm text-gray-600">
                LSR: <span className="font-medium">{selectedItem?.lsr_name}</span>
              </p>
              <p className="text-sm text-gray-600">
                Total Approved: <span className="font-semibold text-gray-900">{selectedItem?.approved_qty} units</span>
              </p>
            </div>
            {selectedItem?.customer_distribution && (
              <Table>
                <TableHeader>
                  <TableRow className="bg-gray-50">
                    <TableHead>Customer</TableHead>
                    <TableHead className="text-center">Quantity</TableHead>
                    <TableHead>Reason</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {selectedItem.customer_distribution.map((dist, index) => (
                    <TableRow key={index}>
                      <TableCell className="font-medium">{dist.customer}</TableCell>
                      <TableCell className="text-center">{dist.quantity}</TableCell>
                      <TableCell className="text-gray-600">{dist.reason}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </div>
        </DialogContent>
      </Dialog>

      {/* Bulk Actions Modal */}
      <Dialog open={showBulkActions} onOpenChange={setShowBulkActions}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle className="text-lg font-semibold text-gray-900">
              Process Batch Approval
            </DialogTitle>
          </DialogHeader>
          <div className="mt-4 space-y-4">
            <p className="text-sm text-gray-600">
              Are you sure you want to process the approval for all {activeTab.toLowerCase()}?
            </p>
            
            <div className="bg-gray-50 rounded-lg p-4">
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>Total Items:</span>
                  <span className="font-semibold">{totalItems}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span>Total Requested:</span>
                  <span>{totalRequestedQty}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span>Total Approved:</span>
                  <span className="font-semibold text-green-600">{totalApprovedQty}</span>
                </div>
              </div>
            </div>
            
            <div className="flex space-x-3 pt-4">
              <Button
                variant="outline"
                onClick={() => setShowBulkActions(false)}
                className="flex-1"
              >
                Cancel
              </Button>
              <Button
                onClick={handleConfirmApproval}
                className="flex-1"
              >
                Confirm Approval
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}