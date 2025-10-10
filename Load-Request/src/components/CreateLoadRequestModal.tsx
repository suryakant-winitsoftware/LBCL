'use client';

import { useState, useEffect } from 'react';
import { format } from 'date-fns';
import { Calendar, Package, Truck, AlertTriangle, Plus, Trash2 } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Calendar as CalendarComponent } from '@/components/ui/calendar';
import { Badge } from '@/components/ui/badge';
import { logisticsApprovalService, CreateLoadRequestDto } from '@/services/logistics-approval.service';
import { api } from '@/services/api';
import { toast } from 'sonner';
import { useAuth } from '@/hooks/useAuth';

interface CreateLoadRequestModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess?: () => void;
  defaultRouteUID?: string;
  defaultSalesmanUID?: string;
}

interface RequestItem {
  sku: string;
  productName: string;
  quantity: number;
  uom: string;
  unitPrice?: number;
}

interface RouteOption {
  value: string;
  label: string;
}

interface SalesmanOption {
  value: string;
  label: string;
}

export default function CreateLoadRequestModal({
  isOpen,
  onClose,
  onSuccess,
  defaultRouteUID,
  defaultSalesmanUID
}: CreateLoadRequestModalProps) {
  const { userDetails } = useAuth();
  const [loading, setLoading] = useState(false);
  const [routes, setRoutes] = useState<RouteOption[]>([]);
  const [salesmen, setSalesmen] = useState<SalesmanOption[]>([]);
  
  const [formData, setFormData] = useState<{
    routeUID: string;
    salesmanUID: string;
    loadType: 'Normal' | 'Emergency';
    productType: 'Commercial' | 'POSM';
    requiredDate: Date | undefined;
    priority: 'High' | 'Medium' | 'Low';
    notes: string;
    items: RequestItem[];
  }>({
    routeUID: defaultRouteUID || '',
    salesmanUID: defaultSalesmanUID || '',
    loadType: 'Normal',
    productType: 'Commercial',
    requiredDate: undefined,
    priority: 'Medium',
    notes: '',
    items: []
  });

  const [newItem, setNewItem] = useState<RequestItem>({
    sku: '',
    productName: '',
    quantity: 0,
    uom: 'PCS',
    unitPrice: 0
  });

  useEffect(() => {
    if (isOpen) {
      loadDropdownData();
    }
  }, [isOpen]);

  const loadDropdownData = async () => {
    try {
      const orgUID = userDetails?.orgUID || '';
      
      // Load routes
      const routeResponse = await api.dropdown.getRoute(orgUID);
      if (routeResponse?.Data) {
        setRoutes(routeResponse.Data.map((r: any) => ({
          value: r.UID || r.uid,
          label: r.Name || r.RouteCode || r.name
        })));
      }

      // Load salesmen/employees
      const employeeResponse = await api.dropdown.getEmployee(orgUID);
      if (employeeResponse?.Data) {
        setSalesmen(employeeResponse.Data.map((e: any) => ({
          value: e.UID || e.uid,
          label: e.Name || e.EmployeeName || e.name
        })));
      }
    } catch (error) {
      console.error('Error loading dropdown data:', error);
      // Use mock data if API fails
      setRoutes([
        { value: 'RT001', label: '[SKTT01]SKTT01' },
        { value: 'RT002', label: '[SKTT02]SKTT02' },
        { value: 'RT003', label: '[SKTT03]SKTT03' }
      ]);
      setSalesmen([
        { value: 'SM001', label: 'John Smith' },
        { value: 'SM002', label: 'Sarah Johnson' },
        { value: 'SM003', label: 'Mike Wilson' }
      ]);
    }
  };

  const handleAddItem = () => {
    if (!newItem.sku || !newItem.productName || newItem.quantity <= 0) {
      toast.error('Please fill all item fields with valid values');
      return;
    }

    setFormData({
      ...formData,
      items: [...formData.items, { ...newItem }]
    });

    // Reset new item form
    setNewItem({
      sku: '',
      productName: '',
      quantity: 0,
      uom: 'PCS',
      unitPrice: 0
    });
  };

  const handleRemoveItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index)
    });
  };

  const handleSubmit = async () => {
    // Validation
    if (!formData.routeUID) {
      toast.error('Please select a route');
      return;
    }
    if (!formData.salesmanUID) {
      toast.error('Please select a salesman');
      return;
    }
    if (!formData.requiredDate) {
      toast.error('Please select a required date');
      return;
    }
    if (formData.items.length === 0) {
      toast.error('Please add at least one item');
      return;
    }

    setLoading(true);
    try {
      const requestData: CreateLoadRequestDto = {
        routeUID: formData.routeUID,
        salesmanUID: formData.salesmanUID,
        loadType: formData.loadType,
        productType: formData.productType,
        requiredDate: format(formData.requiredDate, 'yyyy-MM-dd'),
        priority: formData.priority,
        notes: formData.notes,
        items: formData.items
      };

      const response = await logisticsApprovalService.createLoadRequest(requestData);
      
      toast.success('Load request created successfully!');
      
      // Reset form
      setFormData({
        routeUID: defaultRouteUID || '',
        salesmanUID: defaultSalesmanUID || '',
        loadType: 'Normal',
        productType: 'Commercial',
        requiredDate: undefined,
        priority: 'Medium',
        notes: '',
        items: []
      });

      onSuccess?.();
      onClose();
    } catch (error: any) {
      console.error('Error creating load request:', error);
      toast.error(error.message || 'Failed to create load request');
    } finally {
      setLoading(false);
    }
  };

  const getTotalQuantity = () => {
    return formData.items.reduce((sum, item) => sum + item.quantity, 0);
  };

  const getTotalValue = () => {
    return formData.items.reduce((sum, item) => sum + (item.quantity * (item.unitPrice || 0)), 0);
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Package className="w-5 h-5" />
            Create New Load Request
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6">
          {/* Basic Information */}
          <div className="space-y-4">
            <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
              Basic Information
            </h3>
            
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label>Route</Label>
                <Select
                  value={formData.routeUID}
                  onValueChange={(value) => setFormData({ ...formData, routeUID: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select route" />
                  </SelectTrigger>
                  <SelectContent>
                    {routes.map(route => (
                      <SelectItem key={route.value} value={route.value}>
                        {route.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Salesman</Label>
                <Select
                  value={formData.salesmanUID}
                  onValueChange={(value) => setFormData({ ...formData, salesmanUID: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select salesman" />
                  </SelectTrigger>
                  <SelectContent>
                    {salesmen.map(salesman => (
                      <SelectItem key={salesman.value} value={salesman.value}>
                        {salesman.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Load Type</Label>
                <Select
                  value={formData.loadType}
                  onValueChange={(value: 'Normal' | 'Emergency') => 
                    setFormData({ ...formData, loadType: value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Normal">Normal</SelectItem>
                    <SelectItem value="Emergency">
                      <div className="flex items-center gap-2">
                        <AlertTriangle className="w-4 h-4 text-destructive" />
                        Emergency
                      </div>
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Product Type</Label>
                <Select
                  value={formData.productType}
                  onValueChange={(value: 'Commercial' | 'POSM') => 
                    setFormData({ ...formData, productType: value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Commercial">Commercial</SelectItem>
                    <SelectItem value="POSM">POSM</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label>Required Date</Label>
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="w-full justify-start text-left font-normal"
                    >
                      <Calendar className="mr-2 h-4 w-4" />
                      {formData.requiredDate ? (
                        format(formData.requiredDate, 'PPP')
                      ) : (
                        <span>Pick a date</span>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <CalendarComponent
                      mode="single"
                      selected={formData.requiredDate}
                      onSelect={(date) => setFormData({ ...formData, requiredDate: date })}
                      initialFocus
                      disabled={(date) => date < new Date()}
                    />
                  </PopoverContent>
                </Popover>
              </div>

              <div className="space-y-2">
                <Label>Priority</Label>
                <Select
                  value={formData.priority}
                  onValueChange={(value: 'High' | 'Medium' | 'Low') => 
                    setFormData({ ...formData, priority: value })
                  }
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="High">
                      <Badge variant="destructive">High</Badge>
                    </SelectItem>
                    <SelectItem value="Medium">
                      <Badge variant="secondary">Medium</Badge>
                    </SelectItem>
                    <SelectItem value="Low">
                      <Badge variant="outline">Low</Badge>
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div className="space-y-2">
              <Label>Notes</Label>
              <textarea
                placeholder="Add any additional notes..."
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                className="min-h-[80px] w-full px-3 py-2 text-sm border border-input rounded-md focus:outline-none focus:ring-2 focus:ring-ring focus:border-transparent"
              />
            </div>
          </div>

          {/* Items Section */}
          <div className="space-y-4">
            <h3 className="text-sm font-medium text-muted-foreground uppercase tracking-wider">
              Request Items
            </h3>

            {/* Add Item Form */}
            <div className="border rounded-lg p-4 space-y-3">
              <div className="grid grid-cols-5 gap-3">
                <Input
                  placeholder="SKU"
                  value={newItem.sku}
                  onChange={(e) => setNewItem({ ...newItem, sku: e.target.value })}
                />
                <Input
                  placeholder="Product Name"
                  value={newItem.productName}
                  onChange={(e) => setNewItem({ ...newItem, productName: e.target.value })}
                  className="col-span-2"
                />
                <Input
                  type="number"
                  placeholder="Quantity"
                  value={newItem.quantity || ''}
                  onChange={(e) => setNewItem({ ...newItem, quantity: parseInt(e.target.value) || 0 })}
                />
                <Select
                  value={newItem.uom}
                  onValueChange={(value) => setNewItem({ ...newItem, uom: value })}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="PCS">PCS</SelectItem>
                    <SelectItem value="CTN">CTN</SelectItem>
                    <SelectItem value="BOX">BOX</SelectItem>
                    <SelectItem value="KG">KG</SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <Button
                type="button"
                onClick={handleAddItem}
                className="w-full"
                variant="secondary"
              >
                <Plus className="w-4 h-4 mr-2" />
                Add Item
              </Button>
            </div>

            {/* Items List */}
            {formData.items.length > 0 && (
              <div className="border rounded-lg overflow-hidden">
                <table className="w-full">
                  <thead className="bg-muted/50">
                    <tr>
                      <th className="text-left p-3 text-xs font-medium text-muted-foreground">SKU</th>
                      <th className="text-left p-3 text-xs font-medium text-muted-foreground">Product</th>
                      <th className="text-center p-3 text-xs font-medium text-muted-foreground">Quantity</th>
                      <th className="text-center p-3 text-xs font-medium text-muted-foreground">UOM</th>
                      <th className="text-center p-3 text-xs font-medium text-muted-foreground">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {formData.items.map((item, index) => (
                      <tr key={index} className="border-t">
                        <td className="p-3 text-sm">{item.sku}</td>
                        <td className="p-3 text-sm">{item.productName}</td>
                        <td className="p-3 text-sm text-center">{item.quantity}</td>
                        <td className="p-3 text-sm text-center">
                          <Badge variant="outline">{item.uom}</Badge>
                        </td>
                        <td className="p-3 text-center">
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            onClick={() => handleRemoveItem(index)}
                          >
                            <Trash2 className="w-4 h-4 text-destructive" />
                          </Button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-muted/30">
                    <tr>
                      <td colSpan={2} className="p-3 text-sm font-medium">Total</td>
                      <td className="p-3 text-sm text-center font-medium">{getTotalQuantity()}</td>
                      <td colSpan={2}></td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={loading}>
            {loading ? 'Creating...' : 'Create Request'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}