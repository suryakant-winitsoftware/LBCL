"use client";

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { productService, Product } from '../../services/product.service';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useToast } from '@/components/ui/use-toast';
import {
  ArrowLeft,
  Save,
  Truck,
  Package,
  User,
  MapPin,
  Calendar,
  Clock,
  Weight,
  Box,
  Phone,
  AlertCircle,
} from 'lucide-react';
import { Alert, AlertDescription } from '@/components/ui/alert';

interface LoadFormData {
  vehicleId: string;
  vehicleNumber: string;
  driverName: string;
  driverContact: string;
  routeId: string;
  loadDate: string;
  startTime: string;
  endTime: string;
  capacity: number;
  notes: string;
  items: LoadItem[];
}

interface LoadItem {
  productId: string;
  productName: string;
  productCode: string;
  quantity: number;
  weight: number;
  volume: number;
  baseUOM: string;
}

const CreateLoad: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  const [loading, setLoading] = useState(false);
  const [vehicles, setVehicles] = useState<any[]>([]);
  const [routes, setRoutes] = useState<any[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loadingProducts, setLoadingProducts] = useState(false);
  
  const [formData, setFormData] = useState<LoadFormData>({
    vehicleId: '',
    vehicleNumber: '',
    driverName: '',
    driverContact: '',
    routeId: '',
    loadDate: '',
    startTime: '',
    endTime: '',
    capacity: 0,
    notes: '',
    items: [],
  });

  const [errors, setErrors] = useState<Partial<Record<keyof LoadFormData, string>>>({});

  useEffect(() => {
    loadInitialData();
  }, []);

  const loadInitialData = async () => {
    try {
      setLoading(true);
      
      // Load vehicles and routes (mock data for now)
      setVehicles([
        { id: 'VH001', number: 'MH-12-AB-1234', capacity: 2000, driver: 'John Doe', contact: '+91 9876543210' },
        { id: 'VH002', number: 'MH-12-CD-5678', capacity: 2500, driver: 'Jane Smith', contact: '+91 9876543211' },
        { id: 'VH003', number: 'MH-12-EF-9012', capacity: 2000, driver: 'Mike Johnson', contact: '+91 9876543212' },
      ]);

      setRoutes([
        { id: 'RT001', name: 'North Zone Route', stops: 25, distance: 150 },
        { id: 'RT002', name: 'South Zone Route', stops: 30, distance: 180 },
        { id: 'RT003', name: 'East Zone Route', stops: 20, distance: 120 },
        { id: 'RT004', name: 'West Zone Route', stops: 28, distance: 160 },
      ]);

      // Fetch products from API
      setLoadingProducts(true);
      const productResponse = await productService.getActiveProducts(1, 100);
      if (productResponse.IsSuccess && productResponse.Data) {
        setProducts(productResponse.Data);
      } else {
        console.error('Failed to fetch products');
        toast({
          title: 'Warning',
          description: 'Failed to load products. Using default values.',
          variant: 'destructive',
        });
      }

    } catch (error) {
      console.error('Error loading data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load initial data',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
      setLoadingProducts(false);
    }
  };

  const handleVehicleChange = (vehicleId: string) => {
    const vehicle = vehicles.find(v => v.id === vehicleId);
    if (vehicle) {
      setFormData({
        ...formData,
        vehicleId,
        vehicleNumber: vehicle.number,
        driverName: vehicle.driver,
        driverContact: vehicle.contact,
        capacity: vehicle.capacity,
      });
    }
  };

  const addItem = () => {
    setFormData({
      ...formData,
      items: [
        ...formData.items,
        {
          productId: '',
          productName: '',
          productCode: '',
          quantity: 0,
          weight: 0,
          volume: 0,
          baseUOM: '',
        },
      ],
    });
  };

  const updateItem = (index: number, field: keyof LoadItem, value: any) => {
    const updatedItems = [...formData.items];
    updatedItems[index] = {
      ...updatedItems[index],
      [field]: value,
    };

    // Auto-calculate weight and volume when product is selected
    if (field === 'productId') {
      const product = products.find(p => p.SKUUID === value);
      if (product) {
        updatedItems[index].productName = product.SKULongName || product.SKUName;
        updatedItems[index].productCode = product.SKUCode;
        updatedItems[index].baseUOM = product.BaseUOM;
        // Using default weight/volume values - these should come from product data
        const defaultWeight = product.Weight || 5;
        const defaultVolume = product.Volume || 10;
        updatedItems[index].weight = defaultWeight * updatedItems[index].quantity;
        updatedItems[index].volume = defaultVolume * updatedItems[index].quantity;
      }
    }

    // Recalculate weight and volume when quantity changes
    if (field === 'quantity') {
      const product = products.find(p => p.SKUUID === updatedItems[index].productId);
      if (product) {
        const defaultWeight = product.Weight || 5;
        const defaultVolume = product.Volume || 10;
        updatedItems[index].weight = defaultWeight * value;
        updatedItems[index].volume = defaultVolume * value;
      }
    }

    setFormData({ ...formData, items: updatedItems });
  };

  const removeItem = (index: number) => {
    setFormData({
      ...formData,
      items: formData.items.filter((_, i) => i !== index),
    });
  };

  const calculateTotals = () => {
    const totalWeight = formData.items.reduce((sum, item) => sum + item.weight, 0);
    const totalVolume = formData.items.reduce((sum, item) => sum + item.volume, 0);
    const totalItems = formData.items.reduce((sum, item) => sum + item.quantity, 0);
    const utilization = formData.capacity > 0 ? (totalWeight / formData.capacity) * 100 : 0;

    return { totalWeight, totalVolume, totalItems, utilization };
  };

  const validateForm = (): boolean => {
    const newErrors: Partial<Record<keyof LoadFormData, string>> = {};

    if (!formData.vehicleId) newErrors.vehicleId = 'Vehicle is required';
    if (!formData.routeId) newErrors.routeId = 'Route is required';
    if (!formData.loadDate) newErrors.loadDate = 'Load date is required';
    if (!formData.startTime) newErrors.startTime = 'Start time is required';
    if (!formData.endTime) newErrors.endTime = 'End time is required';
    if (formData.items.length === 0) newErrors.items = 'At least one item is required';

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validateForm()) {
      toast({
        title: 'Validation Error',
        description: 'Please fill in all required fields',
        variant: 'destructive',
      });
      return;
    }

    try {
      setLoading(true);
      
      // API call would go here
      console.log('Creating load:', formData);
      
      toast({
        title: 'Success',
        description: 'Load created successfully',
      });

      router.push('/updatedfeatures/load-management/loads/manage');
    } catch (error) {
      console.error('Error creating load:', error);
      toast({
        title: 'Error',
        description: 'Failed to create load',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const totals = calculateTotals();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="ghost"
          size="icon"
          onClick={() => router.back()}
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Load</h1>
          <p className="text-muted-foreground">
            Schedule a new vehicle load for delivery
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Items
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totals.totalItems}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Weight
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totals.totalWeight} kg</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Total Volume
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{totals.totalVolume} m³</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="pb-3">
            <CardTitle className="text-sm font-medium text-muted-foreground">
              Utilization
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className={`text-2xl font-bold ${
              totals.utilization > 100 ? 'text-destructive' : 
              totals.utilization > 90 ? 'text-muted-foreground' : 
              'text-foreground'
            }`}>
              {totals.utilization.toFixed(1)}%
            </div>
          </CardContent>
        </Card>
      </div>

      {totals.utilization > 100 && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Warning: Load exceeds vehicle capacity by {(totals.utilization - 100).toFixed(1)}%
          </AlertDescription>
        </Alert>
      )}

      {/* Vehicle & Driver Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Truck className="h-5 w-5" />
            Vehicle & Driver Information
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="vehicle">
                Vehicle <span className="text-red-500">*</span>
              </Label>
              <Select
                value={formData.vehicleId}
                onValueChange={handleVehicleChange}
              >
                <SelectTrigger id="vehicle">
                  <SelectValue placeholder="Select vehicle" />
                </SelectTrigger>
                <SelectContent>
                  {vehicles.map((vehicle) => (
                    <SelectItem key={vehicle.id} value={vehicle.id}>
                      {vehicle.number} (Capacity: {vehicle.capacity}kg)
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.vehicleId && (
                <p className="text-sm text-destructive">{errors.vehicleId}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="vehicleNumber">Vehicle Number</Label>
              <Input
                id="vehicleNumber"
                value={formData.vehicleNumber}
                disabled
                className="bg-muted"
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="driverName">Driver Name</Label>
              <div className="flex items-center gap-2">
                <User className="h-4 w-4 text-muted-foreground" />
                <Input
                  id="driverName"
                  value={formData.driverName}
                  onChange={(e) => setFormData({ ...formData, driverName: e.target.value })}
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="driverContact">Driver Contact</Label>
              <div className="flex items-center gap-2">
                <Phone className="h-4 w-4 text-muted-foreground" />
                <Input
                  id="driverContact"
                  value={formData.driverContact}
                  onChange={(e) => setFormData({ ...formData, driverContact: e.target.value })}
                />
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Route & Schedule */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <MapPin className="h-5 w-5" />
            Route & Schedule
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="route">
                Route <span className="text-red-500">*</span>
              </Label>
              <Select
                value={formData.routeId}
                onValueChange={(value) => setFormData({ ...formData, routeId: value })}
              >
                <SelectTrigger id="route">
                  <SelectValue placeholder="Select route" />
                </SelectTrigger>
                <SelectContent>
                  {routes.map((route) => (
                    <SelectItem key={route.id} value={route.id}>
                      {route.name} ({route.stops} stops, {route.distance}km)
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {errors.routeId && (
                <p className="text-sm text-destructive">{errors.routeId}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="loadDate">
                Load Date <span className="text-red-500">*</span>
              </Label>
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <Input
                  id="loadDate"
                  type="date"
                  value={formData.loadDate}
                  onChange={(e) => setFormData({ ...formData, loadDate: e.target.value })}
                />
              </div>
              {errors.loadDate && (
                <p className="text-sm text-destructive">{errors.loadDate}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="startTime">
                Start Time <span className="text-red-500">*</span>
              </Label>
              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-muted-foreground" />
                <Input
                  id="startTime"
                  type="time"
                  value={formData.startTime}
                  onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                />
              </div>
              {errors.startTime && (
                <p className="text-sm text-destructive">{errors.startTime}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="endTime">
                End Time <span className="text-red-500">*</span>
              </Label>
              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-muted-foreground" />
                <Input
                  id="endTime"
                  type="time"
                  value={formData.endTime}
                  onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                />
              </div>
              {errors.endTime && (
                <p className="text-sm text-destructive">{errors.endTime}</p>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Load Items */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              Load Items
            </div>
            <Button onClick={addItem} size="sm">
              Add Item
            </Button>
          </CardTitle>
        </CardHeader>
        <CardContent>
          {errors.items && (
            <Alert variant="destructive" className="mb-4">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>{errors.items}</AlertDescription>
            </Alert>
          )}

          <div className="space-y-4">
            {formData.items.map((item, index) => (
              <div key={index} className="grid gap-4 md:grid-cols-5 p-4 border rounded-lg">
                <div className="space-y-2">
                  <Label>Product</Label>
                  <Select
                    value={item.productId}
                    onValueChange={(value) => updateItem(index, 'productId', value)}
                    disabled={loadingProducts}
                  >
                    <SelectTrigger>
                      <SelectValue placeholder={loadingProducts ? "Loading products..." : "Select product"} />
                    </SelectTrigger>
                    <SelectContent>
                      {products.map((product) => (
                        <SelectItem key={product.SKUUID} value={product.SKUUID}>
                          {product.SKUCode} - {product.SKULongName || product.SKUName}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>Quantity ({item.baseUOM || 'Units'})</Label>
                  <Input
                    type="number"
                    value={item.quantity}
                    onChange={(e) => updateItem(index, 'quantity', parseInt(e.target.value) || 0)}
                    min="0"
                  />
                </div>

                <div className="space-y-2">
                  <Label>Weight (kg)</Label>
                  <Input
                    type="number"
                    value={item.weight}
                    disabled
                    className="bg-muted"
                  />
                </div>

                <div className="space-y-2">
                  <Label>Volume (m³)</Label>
                  <Input
                    type="number"
                    value={item.volume}
                    disabled
                    className="bg-muted"
                  />
                </div>

                <div className="flex items-end">
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => removeItem(index)}
                  >
                    Remove
                  </Button>
                </div>
              </div>
            ))}

            {formData.items.length === 0 && (
              <div className="text-center py-8 text-muted-foreground">
                No items added. Click "Add Item" to start adding products to the load.
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Notes */}
      <Card>
        <CardHeader>
          <CardTitle>Additional Notes</CardTitle>
        </CardHeader>
        <CardContent>
          <Textarea
            placeholder="Enter any special instructions or notes..."
            value={formData.notes}
            onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
            rows={4}
          />
        </CardContent>
      </Card>

      {/* Actions */}
      <div className="flex gap-4 justify-end">
        <Button
          variant="outline"
          onClick={() => router.back()}
          disabled={loading}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          disabled={loading}
        >
          <Save className="h-4 w-4 mr-2" />
          {loading ? 'Creating...' : 'Create Load'}
        </Button>
      </div>
    </div>
  );
};

export default CreateLoad;