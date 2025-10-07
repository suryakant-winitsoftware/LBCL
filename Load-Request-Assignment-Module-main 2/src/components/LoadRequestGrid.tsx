'use client';

import { useState, useEffect } from 'react';
import '@/app/load-management/styles/modal-fixes.css';
import { useRouter } from 'next/navigation';
import { Plus, Search, ShoppingCart, Package, TrendingUp, Filter, ChevronRight, Sparkles, Box, Layers, X, CheckCircle, Minus, AlertTriangle, Clock, Calendar, Users, Truck } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { skuService, SKUListView } from '@/services/sku/sku.service';
import { productService, Product } from '@/app/load-management/services/product.service';
import { customerService, Customer } from '@/app/load-management/services/customer.service';
import { PagingRequest } from '@/types/common.types';
import { format } from 'date-fns';
import { API_CONFIG } from '@/utils/config';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
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

interface LoadRequestItem {
  sku: string;
  product_name: string;
  recommended_order: number;
  pre_order: number;
  buffer_adjustment: number;
  total_order_qty: number;
  quantity_display?: string;
  uom: string;
  customer_distribution?: CustomerDistribution[];
  pre_order_distribution?: PreOrderDistribution[];
}

interface CustomerDistribution {
  customer: string;
  quantity: number;
  quantity_display?: string;
  reason: string;
  route?: string;
  lsr?: string;
  truck?: string;
  uom?: string;
  otherProducts?: {
    sku: string;
    product_name: string;
    quantity: number;
    uom: string;
  }[];
}

interface PreOrderDistribution {
  customer: string;
  quantity: number;
  quantity_display?: string;
  reason: string;
  route?: string;
  lsr?: string;
  truck?: string;
  uom?: string;
  otherProducts?: {
    sku: string;
    product_name: string;
    quantity: number;
    uom: string;
  }[];
}

interface AvailableProduct {
  id: number;
  uid: string;
  code: string;
  name: string;
  longName: string;
  aliasName?: string;
  baseUOM: string;
  outerUOM?: string;
  category?: string;
  brand?: string;
  isActive: boolean;
  available_qty?: number;
  reserved_qty?: number;
  net_available?: number;
  mrp?: number;
  rate?: number;
  discount?: number;
  tax?: number;
  minOrderQty?: number;
  maxOrderQty?: number;
  packSize?: number;
  weight?: number;
  volume?: number;
  expiryDate?: string;
  batchNo?: string;
  warehouse?: string;
  lastSaleDate?: string;
  avgDailySale?: number;
  selected_quantity?: number;
}

interface SelectedProduct extends AvailableProduct {
  selected_quantity: number;
}

interface LoadRequestGridProps {
  onCreateOrder?: () => void;
  showCategoryBadges?: boolean;
  showCategorySelection?: boolean;
}

// Utility function to format quantity with UOM
const formatQuantityWithUOM = (quantity: number, uom: string): string => {
  const uomLower = uom?.toLowerCase() || '';
  
  // Handle cartons (assuming 30 pieces per carton)
  if (uomLower === 'ctn' || uomLower === 'carton') {
    if (quantity === 1) {
      return '1 carton of 30 pcs';
    }
    return `${quantity} cartons of 30 pcs each`;
  }
  
  // Handle dozens (12 pieces)
  if (uomLower === 'dz' || uomLower === 'dozen') {
    if (quantity === 1) {
      return '1 dozen of 12 pieces';
    }
    return `${quantity} dozen of 12 pieces each`;
  }
  
  // Handle boxes
  if (uomLower === 'box' || uomLower === 'bx') {
    return `${quantity} box${quantity !== 1 ? 'es' : ''}`;
  }
  
  // Handle pieces
  if (uomLower === 'pcs' || uomLower === 'piece' || uomLower === 'pieces') {
    return `${quantity} piece${quantity !== 1 ? 's' : ''}`;
  }
  
  // Handle sets
  if (uomLower === 'set' || uomLower === 'sets') {
    return `${quantity} set${quantity !== 1 ? 's' : ''}`;
  }
  
  // Handle kits
  if (uomLower === 'kit' || uomLower === 'kits') {
    return `${quantity} kit${quantity !== 1 ? 's' : ''}`;
  }
  
  // Handle units
  if (uomLower === 'unit' || uomLower === 'units') {
    return `${quantity} unit${quantity !== 1 ? 's' : ''}`;
  }
  
  // Handle bottles
  if (uomLower === 'btl' || uomLower === 'bottle') {
    return `${quantity} bottle${quantity !== 1 ? 's' : ''}`;
  }
  
  // Default format
  return `${quantity} ${uom}`;
};

// Generate route names
const generateRouteName = (index: number): string => {
  const routes = [
    'Route A - Downtown',
    'Route B - Mall District',
    'Route C - Industrial Zone',
    'Route D - Suburban Area',
    'Route E - City Center',
    'Route F - Airport Road',
    'Route G - Harbor Area',
    'Route H - University District'
  ];
  return routes[index % routes.length];
};

// Generate LSR assignments
const generateLSRAssignment = (index: number): { lsr: string; truck: string } => {
  const lsrAssignments = [
    { lsr: 'LSR001 - John Smith', truck: 'TRK-101' },
    { lsr: 'LSR002 - Sarah Johnson', truck: 'TRK-102' },
    { lsr: 'LSR003 - Mike Wilson', truck: 'TRK-103' },
    { lsr: 'LSR004 - Emily Brown', truck: 'TRK-104' },
    { lsr: 'LSR005 - David Lee', truck: 'TRK-105' },
    { lsr: 'LSR006 - Lisa Anderson', truck: 'TRK-106' },
    { lsr: 'LSR007 - Tom Martinez', truck: 'TRK-107' },
    { lsr: 'LSR008 - Amy Taylor', truck: 'TRK-108' }
  ];
  return lsrAssignments[index % lsrAssignments.length];
};

export default function LoadRequestGrid({ onCreateOrder, showCategoryBadges = false, showCategorySelection = false }: LoadRequestGridProps) {
  const router = useRouter();
  const { currentUser } = useAuth();
  
  // Define current user LSR - for now using first LSR, can be updated based on user role/assignment
  const currentUserLSR = 'LSR001 - John Smith';
  
  const [activeTab, setActiveTab] = useState<'Commercial Items' | 'POSM'>('Commercial Items');
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [loadingInitialData, setLoadingInitialData] = useState(true);
  const [loadItems, setLoadItems] = useState<LoadRequestItem[]>([]);

  const [posmItems, setPosmItems] = useState<LoadRequestItem[]>([]);

  // Debug state changes
  useEffect(() => {
    console.log('loadItems state changed:', loadItems.length);
  }, [loadItems]);
  
  useEffect(() => {
    console.log('posmItems state changed:', posmItems.length);
  }, [posmItems]);

  const [showBreakdown, setShowBreakdown] = useState(false);
  const [showPreOrderBreakdown, setShowPreOrderBreakdown] = useState(false);
  const [showTotalOrderDetails, setShowTotalOrderDetails] = useState(false);
  const [showAddItems, setShowAddItems] = useState(false);
  const [selectedItem, setSelectedItem] = useState<LoadRequestItem | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [availableProducts, setAvailableProducts] = useState<AvailableProduct[]>([]);
  const [selectedProducts, setSelectedProducts] = useState<AvailableProduct[]>([]);
  const [filteredProducts, setFilteredProducts] = useState<AvailableProduct[]>([]);
  const [showPriorityDropdown, setShowPriorityDropdown] = useState(false);
  const [selectedPriority, setSelectedPriority] = useState<'emergency' | 'normal'>('normal');
  const [loadingProducts, setLoadingProducts] = useState(false);
  const [deliveryDate, setDeliveryDate] = useState<Date | undefined>(new Date());
  const [dateFilter, setDateFilter] = useState<'today' | 'tomorrow' | 'this-week' | 'custom'>('today');
  const [customDateRange, setCustomDateRange] = useState<{from: Date | undefined; to: Date | undefined}>({from: undefined, to: undefined});
  const [selectedItems, setSelectedItems] = useState<Set<string>>(new Set());
  const [selectAllCategory, setSelectAllCategory] = useState<'none' | 'commercial' | 'posm' | 'all'>('none');
  const [showPreSalesModal, setShowPreSalesModal] = useState(false);
  const [customerList, setCustomerList] = useState<Customer[]>([]);
  const [loadingCustomers, setLoadingCustomers] = useState(false);

  // Load initial data on component mount
  useEffect(() => {
    loadCustomers();
    loadInitialData(); // Load initial data immediately
  }, []);

  // Refresh data when customers are loaded to update with real customer names
  useEffect(() => {
    if (customerList.length > 0) {
      console.log('Customers loaded, refreshing data with customer names');
      loadInitialData();
    }
  }, [customerList]);

  const loadCustomers = async () => {
    try {
      setLoadingCustomers(true);
      const customerResponse = await customerService.getActiveCustomers(1, 100);
      if (customerResponse.IsSuccess && customerResponse.Data) {
        setCustomerList(customerResponse.Data);
        console.log('Loaded customers:', customerResponse.Data.length);
      }
    } catch (error) {
      console.error('Error loading customers:', error);
    } finally {
      setLoadingCustomers(false);
    }
  };

  const loadInitialData = async () => {
    try {
      console.log('Starting loadInitialData...');
      console.log('API_CONFIG.BASE_URL:', API_CONFIG.BASE_URL);
      const apiUrl = `${API_CONFIG.BASE_URL.replace('/api', '')}/api/SKU/GetAllSKUMasterData`;
      console.log('Calling API URL:', apiUrl);
      setLoadingInitialData(true);
      
      const response = await fetch(apiUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTQ1NjM1ODEsImlzcyI6Im15aXNzdWVyIn0.QXc2NiYt64mRNNYJytLrWkTiX20KgFdSDjl2kLsuR1A',
        },
        body: JSON.stringify({
          pageNumber: 1,
          pageSize: 100,
          filterCriterias: [],
          isCountRequired: true
        })
      });

      console.log('Response status:', response.status, response.statusText);
      const data = await response.json();
      console.log('API Response:', data?.IsSuccess ? `Success - ${data.Data?.PagedData?.length || 0} items` : 'Failed');
      console.log('Full API Response:', data);

      if (data.IsSuccess && data.Data?.PagedData?.length > 0) {
        console.log('API Success - Processing', data.Data.PagedData.length, 'products');
        // Create Commercial Items (first 5 products)
        const commercialItems = data.Data.PagedData.slice(0, 5).map((item: any, index: number) => {
          const sku = item.SKU;
          // Generate realistic quantities between 50-200 units
          const totalUnits = Math.floor(Math.random() * 151) + 50;
          
          // Define conversion factors
          const unitsPerCarton = 24; // 24 units per carton
          const unitsPerPack = 6;    // 6 units per pack  
          const unitsPerDozen = 12;  // 12 units per dozen
          
          // Calculate cartons, packs, dozens, and remaining units
          let remaining = totalUnits;
          const cartons = Math.floor(remaining / unitsPerCarton);
          remaining = remaining % unitsPerCarton;
          
          const packs = Math.floor(remaining / unitsPerPack);
          remaining = remaining % unitsPerPack;
          
          const dozens = Math.floor(remaining / unitsPerDozen);
          const units = remaining % unitsPerDozen;
          
          // Create dynamic quantity display with proper hierarchy
          let quantityParts = [];
          if (cartons > 0) {
            quantityParts.push(`${cartons} carton${cartons > 1 ? 's' : ''}`);
          }
          if (packs > 0) {
            quantityParts.push(`${packs} pack${packs > 1 ? 's' : ''}`);
          }
          if (dozens > 0) {
            quantityParts.push(`${dozens} dozen${dozens > 1 ? 's' : ''}`);
          }
          if (units > 0) {
            quantityParts.push(`${units} unit${units > 1 ? 's' : ''}`);
          }
          
          const quantityDisplay = quantityParts.join('\n');
          
          // Generate pre-order quantities (20-60 units)
          const preOrderUnits = Math.floor(Math.random() * 41) + 20;
          
          // Calculate pre-order UOM breakdown
          let preOrderRemaining = preOrderUnits;
          const preOrderCartons = Math.floor(preOrderRemaining / unitsPerCarton);
          preOrderRemaining = preOrderRemaining % unitsPerCarton;
          
          const preOrderPacks = Math.floor(preOrderRemaining / unitsPerPack);
          preOrderRemaining = preOrderRemaining % unitsPerPack;
          
          const preOrderDozens = Math.floor(preOrderRemaining / unitsPerDozen);
          const preOrderUnitsLeft = preOrderRemaining % unitsPerDozen;
          
          // Create pre-order quantity display
          let preOrderQuantityParts = [];
          if (preOrderCartons > 0) {
            preOrderQuantityParts.push(`${preOrderCartons} carton${preOrderCartons > 1 ? 's' : ''}`);
          }
          if (preOrderPacks > 0) {
            preOrderQuantityParts.push(`${preOrderPacks} pack${preOrderPacks > 1 ? 's' : ''}`);
          }
          if (preOrderDozens > 0) {
            preOrderQuantityParts.push(`${preOrderDozens} dozen${preOrderDozens > 1 ? 's' : ''}`);
          }
          if (preOrderUnitsLeft > 0) {
            preOrderQuantityParts.push(`${preOrderUnitsLeft} unit${preOrderUnitsLeft > 1 ? 's' : ''}`);
          }
          
          const preOrderQuantityDisplay = preOrderQuantityParts.join('\n');

          return {
            sku: sku.Code || `COMM-${index + 1}`,
            product_name: sku.Name || sku.LongName || `Commercial Product ${index + 1}`,
            recommended_order: totalUnits,
            pre_order: preOrderUnits,
            buffer_adjustment: 0,
            total_order_qty: totalUnits + preOrderUnits,
            quantity_display: quantityDisplay,
            uom: sku.BaseUOM || 'PCS',
            customer_distribution: customerList.length > 0 ? [{
              customer: customerList[index % customerList.length].Name || customerList[index % customerList.length].Code || 'Customer',
              quantity: totalUnits,
              quantity_display: quantityDisplay,
              reason: 'Regular order',
              uom: sku.BaseUOM || 'PCS'
            }] : [{
              customer: 'Loading customers...',
              quantity: totalUnits,
              quantity_display: quantityDisplay,
              reason: 'Regular order',
              uom: sku.BaseUOM || 'PCS'
            }],
            pre_order_distribution: [{
              customer: [
                'Metro Supermarket Chain',
                'FreshMart Retail Store',
                'SuperValue Grocery',
                'QuickStop Market',
                'GrandBazar Outlet'
              ][index % 5],
              quantity: preOrderUnits,
              quantity_display: preOrderQuantityDisplay,
              reason: [
                'Advance booking for promotion',
                'Bulk order for weekend sale',
                'Pre-order for new store opening',
                'Festival season advance booking',
                'Corporate bulk purchase'
              ][index % 5],
              uom: sku.BaseUOM || 'PCS',
              otherProducts: [
                {
                  sku: `REL-${Math.floor(Math.random() * 100) + 1}`,
                  product_name: 'Related Commercial Item',
                  quantity: Math.floor(Math.random() * 20) + 5,
                  uom: 'PCS'
                }
              ]
            }]
          };
        });

        // Create POSM Items (next 5 products)
        const posmProducts = data.Data.PagedData.slice(5, 10).map((item: any, index: number) => {
          const sku = item.SKU;
          // Generate realistic quantities between 30-120 units for POSM items
          const totalUnits = Math.floor(Math.random() * 91) + 30;
          
          // Define conversion factors
          const unitsPerCarton = 24; // 24 units per carton
          const unitsPerPack = 6;    // 6 units per pack  
          const unitsPerDozen = 12;  // 12 units per dozen
          
          // Calculate cartons, packs, dozens, and remaining units
          let remaining = totalUnits;
          const cartons = Math.floor(remaining / unitsPerCarton);
          remaining = remaining % unitsPerCarton;
          
          const packs = Math.floor(remaining / unitsPerPack);
          remaining = remaining % unitsPerPack;
          
          const dozens = Math.floor(remaining / unitsPerDozen);
          const units = remaining % unitsPerDozen;
          
          // Create dynamic quantity display with proper hierarchy
          let quantityParts = [];
          if (cartons > 0) {
            quantityParts.push(`${cartons} carton${cartons > 1 ? 's' : ''}`);
          }
          if (packs > 0) {
            quantityParts.push(`${packs} pack${packs > 1 ? 's' : ''}`);
          }
          if (dozens > 0) {
            quantityParts.push(`${dozens} dozen${dozens > 1 ? 's' : ''}`);
          }
          if (units > 0) {
            quantityParts.push(`${units} unit${units > 1 ? 's' : ''}`);
          }
          
          const quantityDisplay = quantityParts.join('\n');
          
          // Generate pre-order quantities for POSM (15-45 units)
          const preOrderUnits = Math.floor(Math.random() * 31) + 15;
          
          // Calculate pre-order UOM breakdown
          let preOrderRemaining = preOrderUnits;
          const preOrderCartons = Math.floor(preOrderRemaining / unitsPerCarton);
          preOrderRemaining = preOrderRemaining % unitsPerCarton;
          
          const preOrderPacks = Math.floor(preOrderRemaining / unitsPerPack);
          preOrderRemaining = preOrderRemaining % unitsPerPack;
          
          const preOrderDozens = Math.floor(preOrderRemaining / unitsPerDozen);
          const preOrderUnitsLeft = preOrderRemaining % unitsPerDozen;
          
          // Create pre-order quantity display
          let preOrderQuantityParts = [];
          if (preOrderCartons > 0) {
            preOrderQuantityParts.push(`${preOrderCartons} carton${preOrderCartons > 1 ? 's' : ''}`);
          }
          if (preOrderPacks > 0) {
            preOrderQuantityParts.push(`${preOrderPacks} pack${preOrderPacks > 1 ? 's' : ''}`);
          }
          if (preOrderDozens > 0) {
            preOrderQuantityParts.push(`${preOrderDozens} dozen${preOrderDozens > 1 ? 's' : ''}`);
          }
          if (preOrderUnitsLeft > 0) {
            preOrderQuantityParts.push(`${preOrderUnitsLeft} unit${preOrderUnitsLeft > 1 ? 's' : ''}`);
          }
          
          const preOrderQuantityDisplay = preOrderQuantityParts.join('\n');

          return {
            sku: sku.Code || `POSM-${index + 1}`,
            product_name: sku.Name || sku.LongName || `POSM Product ${index + 1}`,
            recommended_order: totalUnits,
            pre_order: preOrderUnits,
            buffer_adjustment: 0,
            total_order_qty: totalUnits + preOrderUnits,
            quantity_display: quantityDisplay,
            uom: sku.BaseUOM || 'PCS',
            customer_distribution: customerList.length > 0 ? [{
              customer: customerList[index % customerList.length].Name || customerList[index % customerList.length].Code || 'Customer',
              quantity: totalUnits,
              quantity_display: quantityDisplay,
              reason: 'Regular order',
              uom: sku.BaseUOM || 'PCS'
            }] : [{
              customer: 'Loading customers...',
              quantity: totalUnits,
              quantity_display: quantityDisplay,
              reason: 'Regular order',
              uom: sku.BaseUOM || 'PCS'
            }],
            pre_order_distribution: [{
              customer: [
                'BigMart Store Network',
                'HyperCity Mall Outlet',
                'ShopMore Retail Chain',
                'MegaStore Complex',
                'CityCenter Shopping Hub'
              ][index % 5],
              quantity: preOrderUnits,
              quantity_display: preOrderQuantityDisplay,
              reason: [
                'Festival season pre-booking',
                'POSM display campaign setup',
                'New product launch preparation',
                'Holiday promotion materials',
                'Brand visibility enhancement'
              ][index % 5],
              uom: sku.BaseUOM || 'PCS',
              otherProducts: [
                {
                  sku: `POSM-REL-${Math.floor(Math.random() * 50) + 1}`,
                  product_name: 'Related POSM Display Item',
                  quantity: Math.floor(Math.random() * 15) + 3,
                  uom: 'PCS'
                }
              ]
            }]
          };
        });

        console.log('Setting API data - Commercial:', commercialItems.length, 'POSM:', posmProducts.length);
        console.log('Sample commercial item:', commercialItems[0]);
        console.log('Sample POSM item:', posmProducts[0]);
        setLoadItems(commercialItems);
        setPosmItems(posmProducts);
        console.log('API data set successfully');

        // Also populate availableProducts for the modal
        const availableProducts = data.Data.PagedData.map((item: any, index: number) => {
          const sku = item.SKU;
          return {
            id: sku.Id || index + 1,
            uid: sku.UID || '',
            code: sku.Code || `SKU${index}`,
            name: sku.Name || sku.LongName || `Product ${index}`,
            longName: sku.LongName || sku.Name || `Product ${index}`,
            baseUOM: sku.BaseUOM || 'PCS',
            category: 'General',
            brand: '',
            isActive: sku.IsActive !== false,
            available_qty: Math.floor(Math.random() * 500) + 100,
            reserved_qty: Math.floor(Math.random() * 50),
            net_available: 0,
            mrp: Math.floor(Math.random() * 1000) + 50,
            rate: Math.floor(Math.random() * 900) + 45,
            selected_quantity: 0
          };
        });
        
        availableProducts.forEach((p: any) => {
          p.net_available = p.available_qty - p.reserved_qty;
        });
        
        setAvailableProducts(availableProducts);
        setFilteredProducts(availableProducts);
      } else {
        console.log('API call unsuccessful or no data - using fallback');
        // Fallback Commercial Items
        const fallbackCommercialItems = [
          (() => {
            const totalUnits = 26; // Example: 26 units = 2 dozens 2 units
            const dozens = Math.floor(totalUnits / 12);
            const remainingUnits = totalUnits % 12;
            const quantityDisplay = dozens > 0 && remainingUnits > 0 ? 
              `${dozens} dozen${dozens > 1 ? 's' : ''} ${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}` :
              dozens > 0 ? `${dozens} dozen${dozens > 1 ? 's' : ''}` : `${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}`;
            return { sku: 'COMM-001', product_name: 'Sample Commercial Product 1', recommended_order: totalUnits, pre_order: 0, buffer_adjustment: 0, total_order_qty: totalUnits, quantity_display: quantityDisplay, uom: 'PCS', customer_distribution: [{ customer: customerList.length > 0 ? customerList[0].Name || customerList[0].Code || 'Customer' : 'Loading customers...', quantity: totalUnits, quantity_display: quantityDisplay, reason: 'Regular order', uom: 'PCS' }], pre_order_distribution: [] };
          })(),
          (() => {
            const totalUnits = 39; // Example: 39 units = 3 dozens 3 units
            const dozens = Math.floor(totalUnits / 12);
            const remainingUnits = totalUnits % 12;
            const quantityDisplay = dozens > 0 && remainingUnits > 0 ? 
              `${dozens} dozen${dozens > 1 ? 's' : ''} ${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}` :
              dozens > 0 ? `${dozens} dozen${dozens > 1 ? 's' : ''}` : `${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}`;
            return { sku: 'COMM-002', product_name: 'Sample Commercial Product 2', recommended_order: totalUnits, pre_order: 0, buffer_adjustment: 0, total_order_qty: totalUnits, quantity_display: quantityDisplay, uom: 'PCS', customer_distribution: [{ customer: customerList.length > 1 ? customerList[1].Name || customerList[1].Code || 'Customer' : customerList.length > 0 ? customerList[0].Name || customerList[0].Code || 'Customer' : 'Loading customers...', quantity: totalUnits, quantity_display: quantityDisplay, reason: 'Regular order', uom: 'PCS' }], pre_order_distribution: [] };
          })()
        ];
        
        // Fallback POSM Items
        const fallbackPosmItems = [
          (() => {
            const totalUnits = 18; // Example: 18 units = 1 dozen 6 units
            const dozens = Math.floor(totalUnits / 12);
            const remainingUnits = totalUnits % 12;
            const quantityDisplay = dozens > 0 && remainingUnits > 0 ? 
              `${dozens} dozen ${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}` :
              dozens > 0 ? `${dozens} dozen${dozens > 1 ? 's' : ''}` : `${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}`;
            return { sku: 'POSM-001', product_name: 'Sample POSM Product 1', recommended_order: totalUnits, pre_order: 0, buffer_adjustment: 0, total_order_qty: totalUnits, quantity_display: quantityDisplay, uom: 'PCS', customer_distribution: [{ customer: customerList.length > 2 ? customerList[2].Name || customerList[2].Code || 'Customer' : customerList.length > 0 ? customerList[0].Name || customerList[0].Code || 'Customer' : 'Loading customers...', quantity: totalUnits, quantity_display: quantityDisplay, reason: 'Regular order', uom: 'PCS' }], pre_order_distribution: [] };
          })(),
          (() => {
            const totalUnits = 35; // Example: 35 units = 2 dozens 11 units
            const dozens = Math.floor(totalUnits / 12);
            const remainingUnits = totalUnits % 12;
            const quantityDisplay = dozens > 0 && remainingUnits > 0 ? 
              `${dozens} dozen${dozens > 1 ? 's' : ''} ${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}` :
              dozens > 0 ? `${dozens} dozen${dozens > 1 ? 's' : ''}` : `${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}`;
            return { sku: 'POSM-002', product_name: 'Sample POSM Product 2', recommended_order: totalUnits, pre_order: 0, buffer_adjustment: 0, total_order_qty: totalUnits, quantity_display: quantityDisplay, uom: 'PCS', customer_distribution: [{ customer: customerList.length > 3 ? customerList[3].Name || customerList[3].Code || 'Customer' : customerList.length > 0 ? customerList[0].Name || customerList[0].Code || 'Customer' : 'Loading customers...', quantity: totalUnits, quantity_display: quantityDisplay, reason: 'Regular order', uom: 'PCS' }], pre_order_distribution: [] };
          })()
        ];
        
        console.log('Setting fallback data - Commercial:', fallbackCommercialItems.length, 'POSM:', fallbackPosmItems.length);
        console.log('Sample fallback commercial:', fallbackCommercialItems[0]);
        console.log('Sample fallback POSM:', fallbackPosmItems[0]);
        setLoadItems(fallbackCommercialItems);
        setPosmItems(fallbackPosmItems);
        console.log('Fallback data set successfully');
        
        // Fallback availableProducts for the modal
        const fallbackAvailableProducts = [
          { id: 1, uid: 'SKU001', code: 'ECO001', name: 'Sample Product 1', longName: 'Sample Product 1', baseUOM: 'PCS', category: 'General', brand: '', isActive: true, available_qty: 100, reserved_qty: 10, net_available: 90, mrp: 500, rate: 450, selected_quantity: 0 }
        ];
        setAvailableProducts(fallbackAvailableProducts);
        setFilteredProducts(fallbackAvailableProducts);
      }
    } catch (error) {
      console.error('Error loading data:', error);
      console.log('Using error fallback data');
      const fallbackItems = [
        (() => {
          const totalUnits = 38; // Example: 38 units = 3 dozens 2 units
          const dozens = Math.floor(totalUnits / 12);
          const remainingUnits = totalUnits % 12;
          const quantityDisplay = dozens > 0 && remainingUnits > 0 ? 
            `${dozens} dozen${dozens > 1 ? 's' : ''} ${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}` :
            dozens > 0 ? `${dozens} dozen${dozens > 1 ? 's' : ''}` : `${remainingUnits} unit${remainingUnits > 1 ? 's' : ''}`;
          return { sku: 'ERROR001', product_name: 'Error Fallback Product', recommended_order: totalUnits, pre_order: 0, buffer_adjustment: 0, total_order_qty: totalUnits, quantity_display: quantityDisplay, uom: 'PCS', customer_distribution: [{ customer: 'Error Customer', quantity: totalUnits, quantity_display: quantityDisplay, reason: 'Fallback', uom: 'PCS' }], pre_order_distribution: [] };
        })()
      ];
      setLoadItems(fallbackItems);
      
      // Error fallback availableProducts for the modal  
      const errorFallbackAvailableProducts = [
        { id: 1, uid: 'ERROR001', code: 'ERROR001', name: 'Error Fallback Product', longName: 'Error Fallback Product', baseUOM: 'PCS', category: 'General', brand: '', isActive: true, available_qty: 100, reserved_qty: 10, net_available: 90, mrp: 500, rate: 450, selected_quantity: 0 }
      ];
      setAvailableProducts(errorFallbackAvailableProducts);
      setFilteredProducts(errorFallbackAvailableProducts);
    } finally {
      console.log('=== FINAL STATE DEBUG ===');
      console.log('loadItems length:', loadItems.length);
      console.log('posmItems length:', posmItems.length);
      console.log('Setting loadingInitialData to false');
      setLoadingInitialData(false);
    }
  };

  // Removed complex updateLoadItemsWithCustomers function - now using simple direct data mapping

  // Handle product link click - show the product details
  const handleProductClick = (productSku: string) => {
    // Find the product in either commercial or POSM items
    const commercialItem = loadItems.find(item => item.sku === productSku);
    const posmItem = posmItems.find(item => item.sku === productSku);
    
    const foundItem = commercialItem || posmItem;
    
    if (foundItem) {
      setSelectedItem(foundItem);
    } else {
      console.log('Product not found in current items:', productSku);
    }
  };

  // Handle search filtering
  useEffect(() => {
    if (!searchTerm || searchTerm.trim() === '') {
      setFilteredProducts(availableProducts);
    } else {
      const searchLower = searchTerm.toLowerCase();
      const filtered = availableProducts.filter(product => {
        // Make sure we have valid product data
        if (!product) return false;
        
        return (
          (product.name && product.name.toLowerCase().includes(searchLower)) ||
          (product.code && product.code.toLowerCase().includes(searchLower)) ||
          (product.longName && product.longName.toLowerCase().includes(searchLower)) ||
          (product.aliasName && product.aliasName.toLowerCase().includes(searchLower))
        );
      });
      setFilteredProducts(filtered);
    }
  }, [searchTerm, availableProducts]);

  const currentItems = activeTab === 'Commercial Items' ? loadItems : posmItems;
  const totalItems = currentItems.length;
  const totalQuantity = currentItems.reduce((sum, item) => sum + item.total_order_qty, 0);
  const totalRecommended = currentItems.reduce((sum, item) => sum + item.recommended_order, 0);
  const totalPreOrder = currentItems.reduce((sum, item) => sum + item.pre_order, 0);

  console.log('=== RENDER DEBUG ===');
  console.log('Active Tab:', activeTab);
  console.log('Load Items:', loadItems.length);
  console.log('POSM Items:', posmItems.length);
  console.log('Current Items:', currentItems.length);
  console.log('Loading Initial Data:', loadingInitialData);

  const handleBufferAdjustmentChange = (sku: string, value: number) => {
    if (activeTab === 'Commercial Items') {
      setLoadItems(prev => prev.map(item => 
        item.sku === sku 
          ? { 
              ...item, 
              buffer_adjustment: value,
              total_order_qty: item.recommended_order + item.pre_order + value
            }
          : item
      ));
    } else if (activeTab === 'POSM') {
      // POSM items are read-only in the current implementation
      // But we can enable this if needed in the future
    }
  };

  const handleShowBreakdown = (item: LoadRequestItem) => {
    setSelectedItem(item);
    setShowBreakdown(true);
  };

  const handleShowPreOrderBreakdown = (item: LoadRequestItem) => {
    setSelectedItem(item);
    setShowPreOrderBreakdown(true);
  };

  const handleShowTotalOrderDetails = (item: LoadRequestItem) => {
    setSelectedItem(item);
    setShowTotalOrderDetails(true);
  };

  const handleCreateOrder = () => {
    if (activeTab === 'Commercial Items') {
      setActiveTab('POSM');
    } else if (activeTab === 'POSM') {
      setShowPriorityDropdown(true);
    }
  };

  const handlePrioritySubmit = () => {
    setShowPriorityDropdown(false);
    if (onCreateOrder) {
      onCreateOrder();
    }
    
    setTimeout(() => {
      router.push('/load-management/lsr/pending');
    }, 1500);
  };

  const handleAddProduct = (product: AvailableProduct) => {
    const quantity = product.selected_quantity || 1;
    
    // Create new load request item
    const newItem: LoadRequestItem = {
      sku: product.code,
      product_name: product.longName || product.name,
      recommended_order: quantity,
      pre_order: 0,
      buffer_adjustment: 0,
      total_order_qty: quantity,
      uom: product.baseUOM
    };

    // Add to appropriate list
    if (activeTab === 'Commercial Items') {
      setLoadItems(prev => [...prev, newItem]);
    }

    // Reset selections
    setSelectedProducts([]);
    setShowAddItems(false);
    setSearchTerm('');
  };

  const handleProductQuantityChange = (productId: number, quantity: number) => {
    setAvailableProducts(prev => 
      prev.map(product => 
        product.id === productId 
          ? { ...product, selected_quantity: Math.max(0, quantity) }
          : product
      )
    );
    setFilteredProducts(prev => 
      prev.map(product => 
        product.id === productId 
          ? { ...product, selected_quantity: Math.max(0, quantity) }
          : product
      )
    );
  };

  const handleAddSelectedProducts = () => {
    const productsToAdd = filteredProducts.filter(p => (p.selected_quantity || 0) > 0);
    
    productsToAdd.forEach(product => {
      handleAddProduct(product);
    });

    // Reset all quantities
    setAvailableProducts(prev => 
      prev.map(product => ({ ...product, selected_quantity: 0 }))
    );
    setFilteredProducts(prev => 
      prev.map(product => ({ ...product, selected_quantity: 0 }))
    );
  };

  const handleSelectAll = (category: 'commercial' | 'posm' | 'all' | 'none') => {
    setSelectAllCategory(category);
    
    if (category === 'none') {
      setSelectedItems(new Set());
    } else if (category === 'all') {
      const allItems = [...loadItems, ...posmItems];
      setSelectedItems(new Set(allItems.map(item => item.sku)));
    } else if (category === 'commercial') {
      setSelectedItems(new Set(loadItems.map(item => item.sku)));
    } else if (category === 'posm') {
      setSelectedItems(new Set(posmItems.map(item => item.sku)));
    }
  };

  const handleItemSelect = (sku: string) => {
    const newSelectedItems = new Set(selectedItems);
    if (newSelectedItems.has(sku)) {
      newSelectedItems.delete(sku);
    } else {
      newSelectedItems.add(sku);
    }
    setSelectedItems(newSelectedItems);
    
    // Update selectAllCategory based on current selection
    const commercialItems = loadItems.map(item => item.sku);
    const posmItemSkus = posmItems.map(item => item.sku);
    const allItemSkus = [...commercialItems, ...posmItemSkus];
    
    if (newSelectedItems.size === 0) {
      setSelectAllCategory('none');
    } else if (allItemSkus.every(sku => newSelectedItems.has(sku))) {
      setSelectAllCategory('all');
    } else if (commercialItems.every(sku => newSelectedItems.has(sku)) && !posmItemSkus.some(sku => newSelectedItems.has(sku))) {
      setSelectAllCategory('commercial');
    } else if (posmItemSkus.every(sku => newSelectedItems.has(sku)) && !commercialItems.some(sku => newSelectedItems.has(sku))) {
      setSelectAllCategory('posm');
    } else {
      setSelectAllCategory('none');
    }
  };

  return (
    <div className="space-y-6">
      {/* Header with Stats */}
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="bg-muted rounded-lg p-3">
            <Layers className="w-6 h-6 text-muted-foreground" />
          </div>
          <div>
            <h2 className="text-2xl font-bold text-foreground">Load Request Grid</h2>
            <p className="text-sm text-muted-foreground mt-1">Manage your product inventory and orders</p>
          </div>
        </div>
        
        {/* Quick Stats */}
        <div className="flex space-x-3 items-center">
          <div className="bg-white rounded-lg px-4 py-2 border shadow-xs">
            <div className="flex items-center space-x-2">
              <Package className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Items</span>
              <span className="text-lg font-bold text-foreground">{totalItems}</span>
            </div>
          </div>
          <div className="bg-white rounded-lg px-4 py-2 border shadow-xs">
            <div className="flex items-center space-x-2">
              <Box className="w-4 h-4 text-muted-foreground" />
              <span className="text-xs text-muted-foreground">Total Qty</span>
              <span className="text-lg font-bold text-foreground">{totalQuantity}</span>
            </div>
          </div>
        </div>
      </div>
      
      {/* Date Filter Section */}
      <div className="bg-white rounded-lg border shadow-xs p-4 space-y-4">
        {/* Planned Delivery Date */}
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <Label className="text-sm font-medium text-foreground w-40">Planned Delivery Date:</Label>
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
                {dateFilter === 'today' && `Delivery: ${format(new Date(), 'PPP')}`}
                {dateFilter === 'tomorrow' && `Delivery: ${format(new Date(new Date().setDate(new Date().getDate() + 1)), 'PPP')}`}
                {dateFilter === 'this-week' && `Delivery: ${format(new Date(), 'PP')} - ${format(new Date(new Date().setDate(new Date().getDate() + 7)), 'PP')}`}
              </div>
            )}
          </div>
          
          <div className="flex items-center space-x-2">
            <Badge variant="default" className="text-xs">
              <Truck className="w-3 h-3 mr-1" />
              Planned: {dateFilter === 'custom' && deliveryDate ? format(deliveryDate, 'dd MMM yyyy') : 
               dateFilter === 'today' ? 'Today' :
               dateFilter === 'tomorrow' ? 'Tomorrow' : 
               'This Week'}
            </Badge>
          </div>
        </div>
      </div>
      
      {/* Tabs */}
      <div className="bg-muted rounded-lg p-1 inline-flex">
        <button
          onClick={() => setActiveTab('Commercial Items')}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-all ${
            activeTab === 'Commercial Items'
              ? 'bg-white text-foreground shadow-xs'
              : 'text-muted-foreground hover:text-muted-foreground'
          }`}
        >
          <div className="flex items-center space-x-2">
            <ShoppingCart className="w-4 h-4" />
            <span>Commercial Items</span>
          </div>
        </button>
        <button
          onClick={() => setActiveTab('POSM')}
          className={`px-4 py-2 text-sm font-medium rounded-md transition-all ml-1 ${
            activeTab === 'POSM'
              ? 'bg-white text-foreground shadow-xs'
              : 'text-muted-foreground hover:text-muted-foreground'
          }`}
        >
          <div className="flex items-center space-x-2">
            <Sparkles className="w-4 h-4" />
            <span>POSM</span>
          </div>
        </button>
      </div>

      {/* Category Selection */}
      {showCategorySelection && (
        <div className="bg-white rounded-lg border shadow-xs p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Label className="text-sm font-medium">Select Items:</Label>
              <div className="flex items-center space-x-2">
                <Button
                  variant={selectAllCategory === 'all' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => handleSelectAll('all')}
                  className="text-xs"
                >
                  All Items ({loadItems.length + posmItems.length})
                </Button>
                <Button
                  variant={selectAllCategory === 'commercial' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => handleSelectAll('commercial')}
                  className="text-xs"
                >
                  Commercial ({loadItems.length})
                </Button>
                <Button
                  variant={selectAllCategory === 'posm' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => handleSelectAll('posm')}
                  className="text-xs"
                >
                  POSM ({posmItems.length})
                </Button>
                <Button
                  variant={selectAllCategory === 'none' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => handleSelectAll('none')}
                  className="text-xs"
                >
                  Clear Selection
                </Button>
              </div>
            </div>
            <div className="text-sm text-muted-foreground">
              {selectedItems.size} of {loadItems.length + posmItems.length} items selected
            </div>
          </div>
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-lg border shadow-xs overflow-hidden">
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-muted border-b">
              <tr>
                {showCategorySelection && (
                  <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider w-12">
                    <input
                      type="checkbox"
                      checked={selectAllCategory !== 'none'}
                      onChange={(e) => {
                        if (e.target.checked) {
                          handleSelectAll(activeTab === 'Commercial Items' ? 'commercial' : activeTab === 'POSM' ? 'posm' : 'all');
                        } else {
                          handleSelectAll('none');
                        }
                      }}
                      className="rounded border-gray-300 text-primary focus:ring-primary"
                    />
                  </th>
                )}
                <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  <div className="flex items-center space-x-2">
                    <span>SKU</span>
                  </div>
                </th>
                <th className="px-6 py-3 text-left text-xs font-medium text-muted-foreground uppercase tracking-wider">Product Name</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-muted-foreground uppercase tracking-wider">UOM</th>
                <th className="px-6 py-3 text-center text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  <div className="flex items-center justify-center space-x-2">
                    <TrendingUp className="w-4 h-4" />
                    <span>Recommended</span>
                  </div>
                </th>
                <th className="px-6 py-3 text-center text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  <div className="flex items-center justify-center space-x-2">
                    <Clock className="w-4 h-4" />
                    <span>Pre-Order</span>
                  </div>
                </th>
                <th className="px-6 py-3 text-center text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  <div className="flex items-center justify-center space-x-2">
                    <Plus className="w-4 h-4" />
                    <span>Buffer Adj.</span>
                  </div>
                </th>
                <th className="px-6 py-3 text-center text-xs font-medium text-muted-foreground uppercase tracking-wider">
                  <div className="flex items-center justify-center space-x-2">
                    <Box className="w-4 h-4" />
                    <span>Total Qty</span>
                  </div>
                </th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {currentItems.map((item, index) => (
                <tr key={item.sku} className="hover:bg-muted transition-colors">
                  {showCategorySelection && (
                    <td className="px-6 py-4 whitespace-nowrap">
                      <input
                        type="checkbox"
                        checked={selectedItems.has(item.sku)}
                        onChange={() => handleItemSelect(item.sku)}
                        className="rounded border-gray-300 text-primary focus:ring-primary"
                      />
                    </td>
                  )}
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center space-x-3">
                      <div className="bg-muted rounded-lg px-3 py-1.5">
                        <span className="text-sm font-semibold text-muted-foreground">{item.sku}</span>
                      </div>
                      {showCategoryBadges && (
                        <Badge variant="outline" className="text-xs">
                          {activeTab === 'Commercial Items' ? 'Commercial' : 'POSM'}
                        </Badge>
                      )}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-foreground">{item.product_name}</div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <Badge variant="outline" className="text-xs">
                      {item.uom}
                    </Badge>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleShowBreakdown(item)}
                      className="inline-flex items-center space-x-1"
                    >
                      <span className="font-semibold">{item.recommended_order}</span>
                      <ChevronRight className="w-4 h-4" />
                    </Button>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleShowPreOrderBreakdown(item)}
                      className="inline-flex items-center space-x-1"
                    >
                      <span className="font-semibold">{item.pre_order}</span>
                      <ChevronRight className="w-4 h-4" />
                    </Button>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <input
                      type="number"
                      value={item.buffer_adjustment || ''}
                      onChange={(e) => handleBufferAdjustmentChange(item.sku, Number(e.target.value) || 0)}
                      className="w-20 px-3 py-2 text-center text-sm font-medium border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                      placeholder="0"
                    />
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-center">
                    <Button
                      variant="default"
                      size="sm"
                      onClick={() => handleShowTotalOrderDetails(item)}
                      className="inline-flex items-center space-x-1"
                    >
                      <span className="font-semibold">{item.total_order_qty} PCS</span>
                      <ChevronRight className="w-4 h-4" />
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Summary Footer */}
        <div className="bg-muted border-t px-6 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-6">
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Total Items:</span>
                <span className="text-sm font-semibold text-foreground">{totalItems}</span>
              </div>
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Recommended:</span>
                <span className="text-sm font-semibold text-foreground">{totalRecommended}</span>
              </div>
              <div className="flex items-center space-x-2">
                <span className="text-sm font-medium text-muted-foreground">Pre-Orders:</span>
                <span className="text-sm font-semibold text-foreground">{totalPreOrder}</span>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <Box className="w-4 h-4 text-muted-foreground" />
              <span className="text-sm font-medium text-muted-foreground">Total Quantity:</span>
              <span className="text-lg font-bold text-foreground">{totalQuantity}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-3">
        <Button
          onClick={() => setShowAddItems(true)}
          variant="outline"
          className="inline-flex items-center"
        >
          <Plus className="w-4 h-4 mr-2" />
          {activeTab === 'Commercial Items' ? 'Add Commercial Item' : 'Add POSM Item'}
        </Button>
        
        <Button
          onClick={handleCreateOrder}
          variant="default"
          className="inline-flex items-center"
        >
          <ShoppingCart className="w-4 h-4 mr-2" />
          {activeTab === 'Commercial Items' ? 'Next: POSM Items' : 'Create Order'}
        </Button>
      </div>

      {/* Customer Distribution Breakdown Modal */}
      <Dialog open={showBreakdown} onOpenChange={setShowBreakdown}>
        <DialogContent className="w-[650px] max-h-[600px] overflow-hidden bg-white rounded-xl shadow-2xl">
          <DialogHeader className="pb-3 border-b bg-gray-50/50">
            <div className="flex items-center gap-3 px-2">
              <div className="bg-gradient-to-br from-blue-500 to-blue-600 rounded-lg p-2 shadow-sm">
                <Users className="h-5 w-5 text-white" />
              </div>
              <DialogTitle className="text-base font-semibold text-gray-800">
                <div className="flex items-center divide-x divide-gray-300">
                  <span className="pr-3">LSR: <span className="font-bold">{currentUserLSR}</span></span>
                  <span className="px-3">Truck: <span className="font-bold">{generateLSRAssignment(0).truck}</span></span>
                  <span className="pl-3">Route: <span className="font-bold">{generateRouteName(0)}</span></span>
                </div>
              </DialogTitle>
            </div>
          </DialogHeader>
          <div className="space-y-4 overflow-y-auto max-h-[450px] p-4">
            <Card className="bg-gradient-to-br from-blue-50 via-indigo-50 to-purple-50 border-blue-200 shadow-sm">
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div className="grid grid-cols-2 gap-8">
                    <div>
                      <p className="text-xs uppercase tracking-wider text-gray-600 mb-1">Product</p>
                      <p className="text-lg font-bold text-gray-900">{selectedItem?.product_name}</p>
                      <p className="text-sm text-gray-500 mt-1">SKU: <span className="font-mono">{selectedItem?.sku}</span></p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wider text-gray-600 mb-1">Total Recommended Order</p>
                      <p className="text-2xl font-bold text-blue-600">{selectedItem?.recommended_order} <span className="text-base text-gray-600">{selectedItem?.uom}</span></p>
                    </div>
                  </div>
                  <Button
                    onClick={() => {
                      setShowBreakdown(false);
                      setShowPreSalesModal(true);
                    }}
                    className="bg-blue-600 hover:bg-blue-700 text-white text-xs px-3 py-1.5 rounded-md shadow-sm"
                  >
                    <TrendingUp className="w-3 h-3 mr-1.5" />
                    View Pre-sales
                  </Button>
                </div>
              </CardContent>
            </Card>
            
            <Card className="border-gray-200 shadow-sm">
              <CardHeader className="bg-gray-50 py-3 px-4 border-b">
                <div>
                  <h3 className="text-base font-semibold text-gray-900">Distribution Details - Your Assignments</h3>
                  <p className="text-xs text-gray-500 mt-0.5">Your assigned deliveries and route details</p>
                </div>
              </CardHeader>
              <CardContent className="p-0">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-gray-50/50 hover:bg-gray-50">
                      <TableHead className="font-semibold text-xs text-gray-700">Customer</TableHead>
                      <TableHead className="text-center font-semibold text-xs text-gray-700">Quantity</TableHead>
                      <TableHead className="font-semibold text-xs text-gray-700">Reason</TableHead>
                    </TableRow>
                  </TableHeader>
                <TableBody>
                  {selectedItem?.customer_distribution?.filter(dist => {
                    const assignedLSR = dist.lsr || generateLSRAssignment(selectedItem.customer_distribution?.indexOf(dist) || 0).lsr;
                    return assignedLSR === currentUserLSR;
                  }).map((dist, index) => {
                    const originalIndex = selectedItem?.customer_distribution?.indexOf(dist) || 0;
                    const lsrAssignment = generateLSRAssignment(originalIndex);
                    return (
                    <TableRow key={index} className="hover:bg-blue-50/30 transition-colors border-b border-gray-100">
                      <TableCell className="font-medium text-gray-800 text-sm py-3">{dist.customer}</TableCell>
                      <TableCell className="text-center py-3">
                        <div className="inline-block bg-blue-100 text-blue-700 px-3 py-1.5 rounded-md">
                          <div className="whitespace-pre-line leading-tight font-semibold text-sm">
                            {dist.quantity_display || `${dist.quantity} ${((dist.uom || selectedItem?.uom || 'units').toLowerCase().replace('pcs', 'units').replace('pieces', 'units').replace('cartons', 'carton').replace('dozens', 'dozen'))}`}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="text-gray-600 text-sm py-3">{dist.reason}</TableCell>
                    </TableRow>
                    );
                  })}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </DialogContent>
      </Dialog>

      {/* Pre-Order Breakdown Modal */}
      <Dialog open={showPreOrderBreakdown} onOpenChange={setShowPreOrderBreakdown}>
        <DialogContent className="w-[650px] max-h-[600px] overflow-hidden bg-white rounded-xl shadow-2xl">
          <DialogHeader className="pb-3 border-b bg-gray-50/50">
            <div className="flex items-center gap-3 px-2">
              <div className="bg-gradient-to-br from-purple-500 to-purple-600 rounded-lg p-2 shadow-sm">
                <Clock className="h-5 w-5 text-white" />
              </div>
              <DialogTitle className="text-base font-semibold text-gray-800">
                <div className="flex items-center divide-x divide-gray-300">
                  <span className="pr-3">LSR: <span className="font-bold">{currentUserLSR}</span></span>
                  <span className="px-3">Truck: <span className="font-bold">{generateLSRAssignment(0).truck}</span></span>
                  <span className="pl-3">Route: <span className="font-bold">{generateRouteName(0)}</span></span>
                </div>
              </DialogTitle>
            </div>
          </DialogHeader>
          <div className="space-y-4 overflow-y-auto max-h-[450px] p-4">
            <Card className="bg-gradient-to-br from-purple-50 via-pink-50 to-indigo-50 border-purple-200 shadow-sm">
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div className="grid grid-cols-2 gap-8">
                    <div>
                      <p className="text-xs uppercase tracking-wider text-gray-600 mb-1">Product</p>
                      <p className="text-lg font-bold text-gray-900">{selectedItem?.product_name}</p>
                      <p className="text-sm text-gray-500 mt-1">SKU: <span className="font-mono">{selectedItem?.sku}</span></p>
                    </div>
                    <div>
                      <p className="text-xs uppercase tracking-wider text-gray-600 mb-1">Total Pre-Orders</p>
                      <p className="text-2xl font-bold text-purple-600">{selectedItem?.pre_order} <span className="text-base text-gray-600">{selectedItem?.uom}</span></p>
                    </div>
                  </div>
                  <Button
                    onClick={() => {
                      setShowPreOrderBreakdown(false);
                      setShowPreSalesModal(true);
                    }}
                    className="bg-purple-600 hover:bg-purple-700 text-white text-xs px-3 py-1.5 rounded-md shadow-sm"
                  >
                    <TrendingUp className="w-3 h-3 mr-1.5" />
                    View Pre-sales
                  </Button>
                </div>
              </CardContent>
            </Card>
            
            <Card className="border-gray-200 shadow-sm">
              <CardHeader className="bg-gray-50 py-3 px-4 border-b">
                <div>
                  <h3 className="text-base font-semibold text-gray-900">Pre-Order Details - Your Assignments</h3>
                  <p className="text-xs text-gray-500 mt-0.5">Your assigned pre-order deliveries and route details</p>
                </div>
              </CardHeader>
              <CardContent className="p-0">
                <Table>
                  <TableHeader>
                    <TableRow className="bg-gray-50/50 hover:bg-gray-50">
                      <TableHead className="font-semibold text-xs text-gray-700">Customer</TableHead>
                      <TableHead className="text-center font-semibold text-xs text-gray-700">Quantity</TableHead>
                      <TableHead className="font-semibold text-xs text-gray-700">Reason</TableHead>
                    </TableRow>
                  </TableHeader>
                <TableBody>
                  {selectedItem?.pre_order_distribution?.map((dist, index) => {
                    const originalIndex = index;
                    const lsrAssignment = generateLSRAssignment(originalIndex);
                    return (
                    <TableRow key={index} className="hover:bg-purple-50/30 transition-colors border-b border-gray-100">
                      <TableCell className="font-medium text-gray-800 text-sm py-3">{dist.customer}</TableCell>
                      <TableCell className="text-center py-3">
                        <div className="inline-block bg-purple-100 text-purple-700 px-3 py-1.5 rounded-md">
                          <div className="whitespace-pre-line leading-tight font-semibold text-sm">
                            {dist.quantity_display || `${dist.quantity} ${((dist.uom || selectedItem?.uom || 'units').toLowerCase().replace('pcs', 'units').replace('pieces', 'units').replace('cartons', 'carton').replace('dozens', 'dozen'))}`}
                          </div>
                        </div>
                      </TableCell>
                      <TableCell className="text-gray-600 text-sm py-3">{dist.reason}</TableCell>
                    </TableRow>
                    );
                  })}
                  </TableBody>
                </Table>
              </CardContent>
            </Card>
          </div>
        </DialogContent>
      </Dialog>

      {/* Total Order Details Modal */}
      <Dialog open={showTotalOrderDetails} onOpenChange={setShowTotalOrderDetails}>
        <DialogContent className="w-[500px] max-h-[500px] overflow-hidden">
          <DialogHeader className="pb-4">
            <DialogTitle className="text-xl font-bold text-foreground flex items-center gap-3">
              <div className="bg-green-50 dark:bg-green-950/30 rounded-full p-2">
                <Package className="h-5 w-5 text-green-600 dark:text-green-400" />
              </div>
              Order Summary - {selectedItem?.product_name}
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-3 overflow-y-auto max-h-[400px] px-1">
            <div className="bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 rounded-xl p-6 border border-green-100 dark:border-green-800">
              <div className="text-center mb-4">
                <p className="text-sm font-medium text-green-700 dark:text-green-300">Product SKU</p>
                <p className="text-lg font-bold text-green-900 dark:text-green-100">{selectedItem?.sku}</p>
              </div>
              
              <div className="space-y-4">
                <div className="flex justify-between items-center py-2 border-b border-green-200/50">
                  <span className="text-sm font-medium text-green-700 dark:text-green-300">Recommended Order:</span>
                  <span className="font-bold text-green-900 dark:text-green-100">{formatQuantityWithUOM(selectedItem?.recommended_order || 0, selectedItem?.uom || 'PCS')}</span>
                </div>
                <div className="flex justify-between items-center py-2 border-b border-green-200/50">
                  <span className="text-sm font-medium text-green-700 dark:text-green-300">Pre-Order:</span>
                  <span className="font-bold text-green-900 dark:text-green-100">{formatQuantityWithUOM(selectedItem?.pre_order || 0, selectedItem?.uom || 'PCS')}</span>
                </div>
                <div className="flex justify-between items-center py-2 border-b border-green-200/50">
                  <span className="text-sm font-medium text-green-700 dark:text-green-300">Buffer Adjustment:</span>
                  <span className="font-bold text-green-900 dark:text-green-100">{formatQuantityWithUOM(selectedItem?.buffer_adjustment || 0, selectedItem?.uom || 'PCS')}</span>
                </div>
                <div className="bg-white/60 dark:bg-gray-800/40 rounded-lg p-4 mt-4">
                  <div className="flex justify-between items-center">
                    <span className="text-base font-bold text-green-800 dark:text-green-200">Total Order Quantity:</span>
                    <span className="text-2xl font-bold text-green-600 dark:text-green-400">{formatQuantityWithUOM(selectedItem?.total_order_qty || 0, selectedItem?.uom || 'PCS')}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Priority Selection Modal */}
      <Dialog open={showPriorityDropdown} onOpenChange={setShowPriorityDropdown}>
        <DialogContent className="max-w-md bg-white rounded-2xl">
          <DialogHeader className="px-6 pt-6 pb-2">
            <DialogTitle className="text-xl font-bold text-foreground">
              Select Priority
            </DialogTitle>
          </DialogHeader>
          <div className="px-6 pb-6 space-y-4">
            <p className="text-sm text-muted-foreground">
              Choose the priority level for your POSM order:
            </p>
            
            <div className="space-y-3">
              <label className="flex items-center space-x-3 p-3 border border-gray-200 rounded-lg hover:border-red-300 hover:bg-red-50 cursor-pointer transition-colors">
                <input
                  type="radio"
                  name="priority"
                  value="emergency"
                  checked={selectedPriority === 'emergency'}
                  onChange={(e) => setSelectedPriority(e.target.value as 'emergency' | 'normal')}
                  className="w-4 h-4 text-red-600 focus:ring-red-500"
                />
                <div className="flex items-center space-x-3">
                  <AlertTriangle className="w-5 h-5 text-red-600 flex-shrink-0" />
                  <div>
                    <p className="font-semibold text-foreground">Emergency</p>
                    <p className="text-sm text-muted-foreground">High priority, urgent delivery required</p>
                  </div>
                </div>
              </label>
              
              <label className="flex items-center space-x-3 p-3 border border-gray-200 rounded-lg hover:border-blue-300 hover:bg-blue-50 cursor-pointer transition-colors">
                <input
                  type="radio"
                  name="priority"
                  value="normal"
                  checked={selectedPriority === 'normal'}
                  onChange={(e) => setSelectedPriority(e.target.value as 'emergency' | 'normal')}
                  className="w-4 h-4 text-blue-600 focus:ring-blue-500"
                />
                <div className="flex items-center space-x-3">
                  <Package className="w-5 h-5 text-blue-600 flex-shrink-0" />
                  <div>
                    <p className="font-semibold text-foreground">Normal</p>
                    <p className="text-sm text-muted-foreground">Standard priority, regular delivery schedule</p>
                  </div>
                </div>
              </label>
            </div>
            
            <div className="flex space-x-3 pt-2">
              <Button
                variant="outline"
                onClick={() => setShowPriorityDropdown(false)}
                className="flex-1"
              >
                Cancel
              </Button>
              <Button
                onClick={handlePrioritySubmit}
                className={`flex-1 ${
                  selectedPriority === 'emergency' 
                    ? 'bg-red-600 hover:bg-red-700' 
                    : 'bg-blue-600 hover:bg-blue-700'
                }`}
              >
                Submit {selectedPriority === 'emergency' ? 'Emergency' : 'Normal'} Order
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Add Products Modal */}
      <Dialog open={showAddItems} onOpenChange={(open) => {
        setShowAddItems(open);
        // Products are already loaded from loadInitialData
        if (open && availableProducts.length === 0) {
          loadInitialData(); // Re-load initial data if needed
        }
      }}>
        <DialogContent className="w-[90vw] max-w-[1200px] h-[90vh] max-h-[90vh] p-0 overflow-hidden flex flex-col">
          <div className="flex flex-col h-full">
            {/* Header */}
            <div className="shrink-0 px-6 py-4 border-b bg-card">
              <DialogTitle className="text-lg font-semibold text-foreground">
                Add {activeTab === 'Commercial Items' ? 'Commercial' : 'POSM'} Products
              </DialogTitle>
              <p className="text-sm text-muted-foreground mt-1">
                Search and select products to add to your load request
              </p>
            </div>

            {/* Search and Filters */}
            <div className="shrink-0 px-6 py-3 border-b bg-background">
              <div className="flex flex-col sm:flex-row items-start sm:items-center gap-3">
                <div className="flex-1 w-full relative">
                  <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                  <Input
                    type="text"
                    placeholder="Search products..."
                    value={searchTerm}
                    onChange={(e) => {
                      setSearchTerm(e.target.value);
                    }}
                    className="pl-9 h-9"
                  />
                </div>
                <div className="flex items-center gap-2">
                  <div className="text-sm text-muted-foreground">
                    {filteredProducts.length} products • {filteredProducts.filter(p => (p.selected_quantity || 0) > 0).length} selected
                  </div>
                </div>
              </div>
            </div>
            
            {/* Debug Info */}
            <div className="px-6 py-2 bg-muted/50 text-xs text-muted-foreground border-b">
              Available: {availableProducts.length} | Filtered: {filteredProducts.length} | Loading: {loadingInitialData ? 'Yes' : 'No'}
            </div>
            
            {/* Products Table Container */}
            <div className="flex-1 min-h-0 overflow-y-auto overflow-x-auto bg-background relative">
              {(loadingProducts || loadingInitialData) ? (
                <div className="flex flex-col items-center justify-center h-full py-12">
                  <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary mb-4"></div>
                  <p className="text-sm font-medium text-foreground">Loading products...</p>
                  <p className="text-xs text-muted-foreground mt-1">Please wait while we fetch your inventory</p>
                </div>
              ) : filteredProducts.length === 0 ? (
                <div className="flex flex-col items-center justify-center h-full py-12">
                  <Package className="h-12 w-12 text-muted-foreground mb-4" />
                  <h3 className="text-sm font-medium text-foreground mb-1">No products found</h3>
                  <p className="text-xs text-muted-foreground text-center max-w-sm">
                    {searchTerm 
                      ? `No products match your search "${searchTerm}"`
                      : availableProducts.length > 0 
                        ? 'Products loaded but not showing. Try clearing search.'
                        : 'No products loaded from API'}
                  </p>
                  {availableProducts.length === 0 && (
                    <Button 
                      variant="outline" 
                      size="sm" 
                      onClick={() => {
                        loadInitialData();
                      }}
                      className="mt-4"
                    >
                      Retry Loading Products
                    </Button>
                  )}
                </div>
              ) : (
                <div className="w-full">
                  <Table className="w-full min-w-[1400px]">
                    <TableHeader className="sticky top-0 z-10 bg-gray-50 border-b">
                        <TableRow>
                          <TableHead className="w-10 px-2 py-2">
                            <input
                              type="checkbox"
                              className="rounded border-gray-300"
                              onChange={(e) => {
                                if (e.target.checked) {
                                  // Select all visible products
                                  filteredProducts.forEach(product => {
                                    handleProductQuantityChange(product.id, product.minOrderQty || 1);
                                  });
                                } else {
                                  // Deselect all
                                  filteredProducts.forEach(product => {
                                    handleProductQuantityChange(product.id, 0);
                                  });
                                }
                              }}
                            />
                          </TableHead>
                          <TableHead className="w-24 px-2 py-2 text-xs font-medium text-gray-700">SKU Code</TableHead>
                          <TableHead className="px-2 py-2 text-xs font-medium text-gray-700">Product Name</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-xs font-medium text-gray-700">Category</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-xs font-medium text-gray-700">Brand</TableHead>
                          <TableHead className="w-16 px-2 py-2 text-center text-xs font-medium text-gray-700">UOM</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-center text-xs font-medium text-gray-700">Pack Size</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-right text-xs font-medium text-gray-700">MRP</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-right text-xs font-medium text-gray-700">Rate</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-center text-xs font-medium text-gray-700">Stock</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-center text-xs font-medium text-gray-700">Reserved</TableHead>
                          <TableHead className="w-20 px-2 py-2 text-center text-xs font-medium text-gray-700">Available</TableHead>
                          <TableHead className="w-24 px-2 py-2 text-center text-xs font-medium text-gray-700">Avg Sale/Day</TableHead>
                          <TableHead className="w-32 px-2 py-2 text-center text-xs font-medium text-gray-700">Order Qty</TableHead>
                          <TableHead className="w-24 px-2 py-2 text-right text-xs font-medium text-gray-700">Amount</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody className="divide-y divide-gray-100">
                        {filteredProducts.map((product, index) => {
                          const quantity = product.selected_quantity || 0;
                          const rate = product.rate || product.mrp || 0;
                          const amount = quantity * rate;
                          const isSelected = quantity > 0;
                          const stockStatus = (product.net_available || 0) > 0 ? 'in-stock' : 'out-of-stock';
                          
                          return (
                            <TableRow 
                              key={product.id}
                              className={`hover:bg-gray-50 transition-colors ${
                                isSelected ? 'bg-blue-50 border-l-4 border-l-blue-500' : ''
                              }`}
                            >
                              <TableCell className="px-2 py-2">
                                <input
                                  type="checkbox"
                                  className="rounded border-gray-300"
                                  checked={isSelected}
                                  onChange={(e) => {
                                    handleProductQuantityChange(
                                      product.id, 
                                      e.target.checked ? (product.minOrderQty || 1) : 0
                                    );
                                  }}
                                />
                              </TableCell>
                              <TableCell className="px-2 py-2">
                                <div className="font-mono text-xs font-medium text-gray-900" title={product.code}>
                                  {product.code}
                                </div>
                              </TableCell>
                              <TableCell className="px-2 py-2">
                                <div className="min-w-0">
                                  <div className="text-sm font-medium text-gray-900 truncate" title={product.longName || product.name}>
                                    {product.longName || product.name}
                                  </div>
                                  {product.aliasName && (
                                    <div className="text-xs text-gray-500 truncate" title={product.aliasName}>
                                      {product.aliasName}
                                    </div>
                                  )}
                                </div>
                              </TableCell>
                              <TableCell className="px-2 py-2">
                                <Badge variant="outline" className="text-xs">
                                  {product.category || 'General'}
                                </Badge>
                              </TableCell>
                              <TableCell className="px-2 py-2">
                                <span className="text-xs text-gray-600">
                                  {product.brand || '-'}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <span className="text-xs font-medium">
                                  {product.baseUOM}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <span className="text-xs">
                                  {product.packSize || 1}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-right">
                                <span className="text-xs font-medium">
                                  ₹{product.mrp?.toFixed(2) || '0.00'}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-right">
                                <span className="text-xs font-medium text-green-600">
                                  ₹{product.rate?.toFixed(2) || product.mrp?.toFixed(2) || '0.00'}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <span className="text-xs font-medium">
                                  {product.available_qty || 0}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <span className="text-xs text-orange-600">
                                  {product.reserved_qty || 0}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <Badge 
                                  variant={stockStatus === 'in-stock' ? "default" : "destructive"}
                                  className={`text-xs ${stockStatus === 'in-stock' ? 'bg-green-100 text-green-800 border-green-300' : ''}`}
                                >
                                  {product.net_available || 0}
                                </Badge>
                              </TableCell>
                              <TableCell className="px-2 py-2 text-center">
                                <span className="text-xs text-gray-600">
                                  {product.avgDailySale?.toFixed(1) || '0'}
                                </span>
                              </TableCell>
                              <TableCell className="px-2 py-2">
                                <div className="flex items-center justify-center gap-1">
                                  <Button
                                    variant="outline"
                                    size="icon"
                                    className="h-7 w-7 hover:bg-gray-100"
                                    onClick={() => handleProductQuantityChange(product.id, Math.max(0, quantity - (product.packSize || 1)))}
                                    disabled={quantity <= 0}
                                  >
                                    <Minus className="h-3 w-3" />
                                  </Button>
                                  <Input
                                    type="number"
                                    value={quantity}
                                    onChange={(e) => {
                                      const newQty = Math.max(0, parseInt(e.target.value) || 0);
                                      handleProductQuantityChange(product.id, newQty);
                                    }}
                                    className="h-7 w-16 text-center text-xs font-medium"
                                    min={product.minOrderQty || 0}
                                    max={product.maxOrderQty || 9999}
                                  />
                                  <Button
                                    variant="outline"
                                    size="icon"
                                    className="h-7 w-7 hover:bg-gray-100"
                                    onClick={() => handleProductQuantityChange(product.id, quantity + (product.packSize || 1))}
                                    disabled={Boolean(product.maxOrderQty && quantity >= product.maxOrderQty)}
                                  >
                                    <Plus className="h-3 w-3" />
                                  </Button>
                                </div>
                                {product.minOrderQty && quantity < product.minOrderQty && quantity > 0 && (
                                  <div className="text-xs text-red-500 text-center mt-1">
                                    Min: {product.minOrderQty}
                                  </div>
                                )}
                              </TableCell>
                              <TableCell className="px-2 py-2 text-right">
                                <span className="text-sm font-bold text-gray-900">
                                  ₹{amount.toFixed(2)}
                                </span>
                              </TableCell>
                            </TableRow>
                          );
                        })}
                      </TableBody>
                    </Table>
                </div>
              )}
            </div>
            
            {/* Footer Actions */}
            <div className="shrink-0 px-6 py-4 border-t-2 bg-white shadow-inner">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-6">
                  <div className="text-sm text-gray-600">
                    <span className="text-lg font-bold text-gray-900">
                      {filteredProducts.filter(p => (p.selected_quantity || 0) > 0).length}
                    </span>
                    {' '}items selected
                  </div>
                  <div className="border-l pl-6">
                    <div className="text-xs text-gray-500">Total Quantity</div>
                    <div className="text-lg font-bold text-gray-900">
                      {filteredProducts.reduce((sum, p) => sum + (p.selected_quantity || 0), 0)}
                    </div>
                  </div>
                  <div className="border-l pl-6">
                    <div className="text-xs text-gray-500">Total Amount</div>
                    <div className="text-lg font-bold text-primary">
                      ₹{filteredProducts.reduce((sum, p) => {
                        const qty = p.selected_quantity || 0;
                        const rate = p.rate || p.mrp || 0;
                        return sum + (qty * rate);
                      }, 0).toFixed(2)}
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => {
                      setShowAddItems(false);
                      setSearchTerm('');
                      setAvailableProducts(prev => 
                        prev.map(product => ({ ...product, selected_quantity: 0 }))
                      );
                      setFilteredProducts(prev => 
                        prev.map(product => ({ ...product, selected_quantity: 0 }))
                      );
                    }}
                  >
                    Cancel
                  </Button>
                  <Button
                    size="sm"
                    onClick={handleAddSelectedProducts}
                    disabled={filteredProducts.filter(p => (p.selected_quantity || 0) > 0).length === 0}
                  >
                    Add to Load Request
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Pre-sales Modal */}
      <Dialog open={showPreSalesModal} onOpenChange={setShowPreSalesModal}>
        <DialogContent className="w-[850px] max-h-[90vh] overflow-hidden flex flex-col">
          <DialogHeader className="pb-4 flex-shrink-0">
            <DialogTitle className="text-xl font-bold text-foreground flex items-center gap-3">
              <div className="bg-green-50 dark:bg-green-950/30 rounded-full p-2">
                <TrendingUp className="h-5 w-5 text-green-600 dark:text-green-400" />
              </div>
              Pre-sales Overview
            </DialogTitle>
          </DialogHeader>
          <div className="space-y-4 overflow-y-auto flex-1 px-2 pb-4 scrollbar-thin scrollbar-thumb-gray-300 scrollbar-track-gray-100">
            {/* Pre-sales Summary */}
            <div className="bg-gradient-to-r from-green-50 to-emerald-50 dark:from-green-950/20 dark:to-emerald-950/20 rounded-lg p-4 border border-green-100 dark:border-green-800">
              <div className="grid grid-cols-3 gap-4">
                <div className="text-center">
                  <p className="text-sm font-medium text-green-700 dark:text-green-300">Total Pre-orders</p>
                  <p className="text-2xl font-bold text-green-900 dark:text-green-100">{totalPreOrder}</p>
                </div>
                <div className="text-center">
                  <p className="text-sm font-medium text-green-700 dark:text-green-300">Commercial Items</p>
                  <p className="text-2xl font-bold text-green-900 dark:text-green-100">{loadItems.reduce((sum, item) => sum + item.pre_order, 0)}</p>
                </div>
                <div className="text-center">
                  <p className="text-sm font-medium text-green-700 dark:text-green-300">POSM Items</p>
                  <p className="text-2xl font-bold text-green-900 dark:text-green-100">{posmItems.reduce((sum, item) => sum + item.pre_order, 0)}</p>
                </div>
              </div>
            </div>

            {/* Pre-sales Details */}
            <div className="bg-white dark:bg-gray-900 rounded-lg border shadow-sm overflow-hidden">
              <div className="bg-muted/50 px-4 py-3 border-b sticky top-0 bg-white z-10">
                <h3 className="text-lg font-semibold text-foreground">Pre-sales Details by Product</h3>
                <p className="text-sm text-muted-foreground">Detailed breakdown of all pre-orders</p>
              </div>
              <div className="max-h-[450px] overflow-auto relative">
                <Table className="w-full min-w-[700px]">
                <TableHeader className="sticky top-0 bg-white z-10">
                  <TableRow className="bg-muted/30">
                    <TableHead className="w-[30%] min-w-[200px] font-semibold text-xs px-4 py-3">Product</TableHead>
                    <TableHead className="w-[15%] min-w-[120px] font-semibold text-xs px-4 py-3">SKU</TableHead>
                    <TableHead className="w-[15%] min-w-[100px] text-center font-semibold text-xs px-4 py-3">Pre-order Qty</TableHead>
                    <TableHead className="w-[20%] min-w-[150px] font-semibold text-xs px-4 py-3">Customer</TableHead>
                    <TableHead className="w-[20%] min-w-[150px] font-semibold text-xs px-4 py-3">Reason</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {[...loadItems, ...posmItems].map((item, index) => 
                    item.pre_order_distribution?.map((preOrder, preOrderIndex) => (
                      <TableRow key={`${item.sku}-${preOrderIndex}`} className="hover:bg-muted/20 border-b">
                        <TableCell className="w-[30%] min-w-[200px] font-medium text-sm px-4 py-3 align-middle">{item.product_name}</TableCell>
                        <TableCell className="w-[15%] min-w-[120px] text-sm font-mono px-4 py-3 align-middle">{item.sku}</TableCell>
                        <TableCell className="w-[15%] min-w-[100px] text-center text-sm px-4 py-3 align-middle">
                          <div className="whitespace-nowrap">
                            {preOrder.quantity_display || `${preOrder.quantity} units`}
                          </div>
                        </TableCell>
                        <TableCell className="w-[20%] min-w-[150px] text-sm px-4 py-3 align-middle">{preOrder.customer}</TableCell>
                        <TableCell className="w-[20%] min-w-[150px] text-sm text-muted-foreground px-4 py-3 align-middle">{preOrder.reason}</TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
              <div className="h-4"></div>
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}