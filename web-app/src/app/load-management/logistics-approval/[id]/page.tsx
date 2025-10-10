'use client';

import { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Download, TrendingUp, AlertCircle, Plus, ChevronDown } from 'lucide-react';
import { useAuth } from '@/hooks/useAuth';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

interface OrderItem {
  itemCode: string;
  description: string;
  recommendedOrder: number;
  preOrder: number;
  bufferAdjustment: number;
  qtyApprovedTotal: number;
  availableStock: number;
  uom: string; // Unit of Measure: Carton, Box, Piece, Case, Pack, Kg, Liter, etc.
  skuId?: number;
  recommendedCustomers?: CustomerOrder[];
  preOrderCustomers?: CustomerOrder[];
  productType: 'Commercial' | 'POSM';
}

interface CustomerOrder {
  customer_name: string;
  contact_person: string;
  phone: string;
  email: string;
  quantity: number;
  delivery_date: string;
  order_date: string;
  outlet_name: string;
  outlet_type?: string;
}

export default function LogisticsApprovalDetailsPage() {
  const router = useRouter();
  const params = useParams();
  const requestId = params?.id as string;
  const { currentUser } = useAuth();
  
  const [orderItems, setOrderItems] = useState<OrderItem[]>([]);
  const [requestDetails, setRequestDetails] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [selectedCustomers, setSelectedCustomers] = useState<CustomerOrder[]>([]);
  const [selectedProduct, setSelectedProduct] = useState<{ name: string; code: string } | null>(null);
  const [selectedOrderType, setSelectedOrderType] = useState<'Recommended' | 'Pre-Order'>('Recommended');
  const [selectedQuantity, setSelectedQuantity] = useState(0);
  const [productFilter, setProductFilter] = useState<'all' | 'commercial' | 'posm'>('all');
  const [showSuccess, setShowSuccess] = useState(false);

  useEffect(() => {
    if (requestId) {
      loadRequestDetails();
    }
  }, [requestId]);

  const loadRequestDetails = () => {
    // Create mock request details
    const details = {
      movementCode: `SKTT01E000${requestId.padStart(4, '0')}`,
      routeCode: `[SKTT01]SKTT01`,
      salesmanId: `[LSR${requestId.padStart(3, '0')}]`,
      salesmanName: `Salesman ${requestId}`,
      loadType: parseInt(requestId) % 3 === 0 ? 'Normal' : 'Emergency',
      submittedDate: new Date().toLocaleDateString('en-GB'),
      requiredDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toLocaleDateString('en-GB'),
      status: 'Pending'
    };
    
    setRequestDetails(details);
    
    // Load mock order items
    const items: OrderItem[] = [
      {
        itemCode: '[SKU001]',
        description: 'Choco Bar 90ml',
        recommendedOrder: 45,
        preOrder: 15,
        bufferAdjustment: 10,
        qtyApprovedTotal: 70,
        availableStock: 150,
        uom: 'Piece',
        productType: 'Commercial',
        recommendedCustomers: [
          {
            customer_name: 'Raj Supermarket',
            contact_person: 'Mr. Rajesh Kumar',
            phone: '+91 98765 43210',
            email: 'raj.super@gmail.com',
            quantity: 20,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Raj Supermarket - MG Road',
            outlet_type: 'Supermarket'
          },
          {
            customer_name: 'City Fresh Mart',
            contact_person: 'Ms. Priya Sharma',
            phone: '+91 98765 43211',
            email: 'cityfresh@gmail.com',
            quantity: 15,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'City Fresh - Downtown',
            outlet_type: 'Hypermarket'
          },
          {
            customer_name: 'Quick Stop Store',
            contact_person: 'Mr. Amit Verma',
            phone: '+91 98765 43212',
            email: 'quickstop@gmail.com',
            quantity: 10,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Quick Stop - Station Road',
            outlet_type: 'Convenience Store'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'Green Valley Stores',
            contact_person: 'Mr. Vikram Singh',
            phone: '+91 98765 43214',
            email: 'greenvalley@gmail.com',
            quantity: 8,
            delivery_date: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Green Valley - Sector 5',
            outlet_type: 'Grocery Store'
          },
          {
            customer_name: 'Daily Needs Mart',
            contact_person: 'Ms. Anita Reddy',
            phone: '+91 98765 43215',
            email: 'dailyneeds@gmail.com',
            quantity: 7,
            delivery_date: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Daily Needs - Park Street',
            outlet_type: 'Traditional Trade'
          }
        ]
      },
      {
        itemCode: '[SKU002]',
        description: 'Vanilla Tub 500ml',
        recommendedOrder: 30,
        preOrder: 8,
        bufferAdjustment: 2,
        qtyApprovedTotal: 40,
        availableStock: 100,
        uom: 'Box',
        productType: 'Commercial',
        recommendedCustomers: [
          {
            customer_name: 'Metro Central',
            contact_person: 'Mr. Suresh Patel',
            phone: '+91 98765 43220',
            email: 'metro.central@gmail.com',
            quantity: 18,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Metro Central - Main Branch',
            outlet_type: 'Department Store'
          },
          {
            customer_name: 'Family Mart',
            contact_person: 'Ms. Kavita Joshi',
            phone: '+91 98765 43221',
            email: 'family.mart@gmail.com',
            quantity: 12,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Family Mart - Shopping Complex',
            outlet_type: 'Supermarket'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'Corner Store Plus',
            contact_person: 'Mr. Ravi Kumar',
            phone: '+91 98765 43222',
            email: 'corner.plus@gmail.com',
            quantity: 8,
            delivery_date: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Corner Store Plus - Residential Area',
            outlet_type: 'Local Store'
          }
        ]
      },
      {
        itemCode: '[POSM001]',
        description: 'Brand Banner 2x4ft',
        recommendedOrder: 5,
        preOrder: 2,
        bufferAdjustment: 0,
        qtyApprovedTotal: 7,
        availableStock: 20,
        uom: 'Piece',
        productType: 'POSM',
        recommendedCustomers: [
          {
            customer_name: 'Metro Store Downtown',
            contact_person: 'Mr. Arjun Mehta',
            phone: '+91 98765 43230',
            email: 'metro.downtown@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Metro Store - Downtown Branch',
            outlet_type: 'Department Store'
          },
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - Food Court Area',
            outlet_type: 'Hypermarket'
          },
          {
            customer_name: 'City Center Outlet',
            contact_person: 'Mr. Rohit Gupta',
            phone: '+91 98765 43232',
            email: 'citycenter@gmail.com',
            quantity: 1,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'City Center - Window Display',
            outlet_type: 'Shopping Complex'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'Metro Store Downtown',
            contact_person: 'Mr. Arjun Mehta',
            phone: '+91 98765 43230',
            email: 'metro.downtown@gmail.com',
            quantity: 1,
            delivery_date: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Metro Store - Grand Opening Event',
            outlet_type: 'Department Store'
          },
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 1,
            delivery_date: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - Weekend Promotion Setup',
            outlet_type: 'Hypermarket'
          }
        ]
      },
      {
        itemCode: '[POSM002]',
        description: 'Display Stand Metal',
        recommendedOrder: 3,
        preOrder: 1,
        bufferAdjustment: 1,
        qtyApprovedTotal: 5,
        availableStock: 15,
        uom: 'Unit',
        productType: 'POSM',
        recommendedCustomers: [
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - Product Showcase Area',
            outlet_type: 'Hypermarket'
          },
          {
            customer_name: 'City Center Outlet',
            contact_person: 'Mr. Rohit Gupta',
            phone: '+91 98765 43232',
            email: 'citycenter@gmail.com',
            quantity: 1,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'City Center - Counter-top Display',
            outlet_type: 'Shopping Complex'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 1,
            delivery_date: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - New Product Launch Display',
            outlet_type: 'Hypermarket'
          }
        ]
      },
      {
        itemCode: '[SKU003]',
        description: 'Strawberry Cup 100ml',
        recommendedOrder: 25,
        preOrder: 8,
        bufferAdjustment: 2,
        qtyApprovedTotal: 35,
        availableStock: 80,
        uom: 'Case',
        productType: 'Commercial',
        recommendedCustomers: [
          {
            customer_name: 'Fresh Foods Market',
            contact_person: 'Mr. Deepak Agarwal',
            phone: '+91 98765 43240',
            email: 'freshfoods@gmail.com',
            quantity: 15,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Fresh Foods - Central Market',
            outlet_type: 'Grocery Store'
          },
          {
            customer_name: 'Smart Shop Express',
            contact_person: 'Ms. Rekha Nair',
            phone: '+91 98765 43241',
            email: 'smartshop@gmail.com',
            quantity: 10,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Smart Shop - Express Lane',
            outlet_type: 'Convenience Store'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'Premium Mart',
            contact_person: 'Mr. Sanjay Kapoor',
            phone: '+91 98765 43242',
            email: 'premium.mart@gmail.com',
            quantity: 8,
            delivery_date: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Premium Mart - VIP Section',
            outlet_type: 'Premium Store'
          }
        ]
      },
      {
        itemCode: '[POSM003]',
        description: 'Promotional Stickers Pack',
        recommendedOrder: 15,
        preOrder: 5,
        bufferAdjustment: 0,
        qtyApprovedTotal: 20,
        availableStock: 50,
        uom: 'Pack',
        productType: 'POSM',
        recommendedCustomers: [
          {
            customer_name: 'Metro Store Downtown',
            contact_person: 'Mr. Arjun Mehta',
            phone: '+91 98765 43230',
            email: 'metro.downtown@gmail.com',
            quantity: 6,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Metro Store - Promotional Campaign',
            outlet_type: 'Department Store'
          },
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 5,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - Weekend Promotion',
            outlet_type: 'Hypermarket'
          },
          {
            customer_name: 'City Center Outlet',
            contact_person: 'Mr. Rohit Gupta',
            phone: '+91 98765 43232',
            email: 'citycenter@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 3 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'City Center - Point of Sale',
            outlet_type: 'Shopping Complex'
          },
          {
            customer_name: 'Express Corner Shop',
            contact_person: 'Mr. Kiran Joshi',
            phone: '+91 98765 43250',
            email: 'express.corner@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Express Corner - Grab & Go',
            outlet_type: 'Corner Store'
          }
        ],
        preOrderCustomers: [
          {
            customer_name: 'Metro Store Downtown',
            contact_person: 'Mr. Arjun Mehta',
            phone: '+91 98765 43230',
            email: 'metro.downtown@gmail.com',
            quantity: 3,
            delivery_date: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 4 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'Metro Store - Summer Campaign',
            outlet_type: 'Department Store'
          },
          {
            customer_name: 'SuperMart Mall',
            contact_person: 'Ms. Neha Singh',
            phone: '+91 98765 43231',
            email: 'supermart.mall@gmail.com',
            quantity: 2,
            delivery_date: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000).toISOString(),
            order_date: new Date(Date.now() - 5 * 24 * 60 * 60 * 1000).toISOString(),
            outlet_name: 'SuperMart - Special Event Stickers',
            outlet_type: 'Hypermarket'
          }
        ]
      }
    ];

    setOrderItems(items);
    setLoading(false);
  };

  const handleTotalQuantityChange = (index: number, value: number) => {
    const sanitizedValue = Math.max(0, Math.min(value, 10000));
    const updatedItems = [...orderItems];
    const item = updatedItems[index];
    
    updatedItems[index] = {
      ...item,
      qtyApprovedTotal: sanitizedValue,
      bufferAdjustment: sanitizedValue - item.recommendedOrder - item.preOrder
    };
    
    setOrderItems(updatedItems);
  };

  const handleShowCustomerDetails = (item: OrderItem, orderType: 'Recommended' | 'Pre-Order') => {
    const customers = orderType === 'Recommended' ? item.recommendedCustomers : item.preOrderCustomers;
    const quantity = orderType === 'Recommended' ? item.recommendedOrder : item.preOrder;
    
    if (customers && customers.length > 0) {
      setSelectedCustomers(customers);
      setSelectedProduct({ name: item.description, code: item.itemCode });
      setSelectedOrderType(orderType);
      setSelectedQuantity(quantity);
      setShowCustomerModal(true);
    }
  };

  const handleApprove = () => {
    setShowSuccess(true);
    setTimeout(() => {
      setShowSuccess(false);
      router.push('/load-management/logistics-approval/history');
    }, 2000);
  };

  const handleReject = () => {
    alert('Request rejected!');
    router.push('/load-management/logistics-approval/history');
  };

  if (loading) {
    return (
      <div className="p-8">
        <div className="max-w-7xl mx-auto">
          {/* Header Skeleton */}
          <div className="mb-8">
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-4">
                <Skeleton className="h-9 w-20" />
                <div>
                  <Skeleton className="h-8 w-72 mb-2" />
                  <Skeleton className="h-4 w-96" />
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <Skeleton className="h-9 w-32" />
                <Skeleton className="h-9 w-32" />
              </div>
            </div>
          </div>

          <div className="space-y-6">
            {/* Request Info Skeleton */}
            <div className="bg-card rounded-lg border p-6">
              <div className="grid grid-cols-2 gap-6">
                <div className="space-y-4">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="flex items-center justify-between">
                      <Skeleton className="h-4 w-32" />
                      <Skeleton className="h-4 w-40" />
                    </div>
                  ))}
                </div>
                <div className="space-y-4">
                  {[1, 2, 3, 4].map((i) => (
                    <div key={i} className="flex items-center justify-between">
                      <Skeleton className="h-4 w-32" />
                      <Skeleton className="h-4 w-40" />
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Product Table Skeleton */}
            <div className="bg-white rounded-lg border overflow-hidden">
              <div className="px-6 py-4 border-b">
                <div className="flex items-center justify-between">
                  <div>
                    <Skeleton className="h-6 w-48 mb-2" />
                    <Skeleton className="h-4 w-72" />
                  </div>
                  <div className="flex items-center space-x-4">
                    <Skeleton className="h-10 w-44" />
                    <div className="flex items-center space-x-4 pl-4 border-l">
                      <div className="text-right">
                        <Skeleton className="h-4 w-8 mb-1 ml-auto" />
                        <Skeleton className="h-3 w-12" />
                      </div>
                      <div className="text-right">
                        <Skeleton className="h-4 w-12 mb-1 ml-auto" />
                        <Skeleton className="h-3 w-16" />
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableHead><Skeleton className="h-4 w-16" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-28" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-12" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-32" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-24" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                      <TableHead><Skeleton className="h-4 w-20" /></TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {[1, 2, 3, 4, 5, 6].map((i) => (
                      <TableRow key={i}>
                        <TableCell><Skeleton className="h-5 w-16" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-32" /></TableCell>
                        <TableCell><Skeleton className="h-5 w-12" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-16" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                        <TableCell><Skeleton className="h-4 w-12" /></TableCell>
                        <TableCell><Skeleton className="h-9 w-20" /></TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            </div>

            {/* Action Buttons Skeleton */}
            <div className="flex justify-center space-x-4 pt-6">
              <Skeleton className="h-12 w-32" />
              <Skeleton className="h-12 w-32" />
            </div>
          </div>
        </div>
      </div>
    );
  }

  if (!requestDetails) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center bg-white p-8 rounded-lg shadow-lg border">
          <AlertCircle className="w-16 h-16 text-primary mx-auto mb-4" />
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Request Not Found</h2>
          <p className="text-muted-foreground mb-4">The requested load request could not be found.</p>
          <Button onClick={() => router.push('/load-management/logistics-approval/incoming')}>
            Back to Logistics Approval
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="p-8">
      {/* Success Message */}
      {showSuccess && (
        <div className="fixed top-20 right-4 z-50">
          <div className="bg-background rounded-lg shadow-lg border p-4 flex items-center space-x-3 min-w-[350px]">
            <div className="text-muted-foreground">
              <AlertCircle className="w-5 h-5" />
            </div>
            <div>
              <p className="font-medium">Request Approved!</p>
              <p className="text-sm text-muted-foreground">Load request has been successfully approved</p>
            </div>
          </div>
        </div>
      )}

      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-4">
              <Button
                variant="outline"
                size="sm"
                onClick={() => router.push('/load-management/logistics-approval/incoming')}
                className="flex items-center gap-2"
              >
                <ArrowLeft className="w-4 h-4" />
                Back
              </Button>
              <div>
                <h1 className="text-2xl font-bold">Manual Load Request Details</h1>
                <p className="text-sm text-muted-foreground mt-1">
                  Load Management › Logistics Approval › Details › {requestDetails.loadType} Load
                </p>
              </div>
            </div>
            <div className="flex items-center space-x-2">
              <Button variant="ghost" size="sm">
                Generate PDF
              </Button>
              <Button variant="ghost" size="sm" className="flex items-center gap-2">
                <Download className="w-4 h-4" />
                Export Excel
              </Button>
            </div>
          </div>
        </div>

        <div className="space-y-6">
          {/* Request Info */}
          <div className="bg-card rounded-lg border p-6">
            <div className="grid grid-cols-2 gap-6">
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Movement Code</span>
                  <span className="font-semibold">{requestDetails.movementCode}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Route Code</span>
                  <span className="font-semibold">{requestDetails.routeCode}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Salesman</span>
                  <span className="font-semibold">{requestDetails.salesmanId} {requestDetails.salesmanName}</span>
                </div>
              </div>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Movement Type</span>
                  <span className="font-semibold">Load</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Submitted Date</span>
                  <span className="font-semibold">{requestDetails.submittedDate}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Required Date</span>
                  <span className="font-semibold">{requestDetails.requiredDate}</span>
                </div>
                <div className="flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">Movement Status</span>
                  <Badge variant="secondary">Pending</Badge>
                </div>
              </div>
            </div>
          </div>


          {/* Product Tables with Tabs */}
          <div className="bg-white rounded-lg border overflow-hidden">
            {/* Header with Stats and Filter */}
            <div className="px-6 py-4 border-b">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-semibold">Load Request Items</h3>
                  <p className="text-sm text-muted-foreground">Review and approve product quantities</p>
                </div>
                <div className="flex items-center space-x-4">
                  {/* Product Type Filter Dropdown */}
                  <Select value={productFilter} onValueChange={(value: 'all' | 'commercial' | 'posm') => setProductFilter(value)}>
                    <SelectTrigger className="w-[180px]">
                      <SelectValue placeholder="Select product type" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Select All</SelectItem>
                      <SelectItem value="commercial">Commercial</SelectItem>
                      <SelectItem value="posm">POSM</SelectItem>
                    </SelectContent>
                  </Select>
                  
                  <div className="flex items-center space-x-4 pl-4 border-l">
                    <div className="text-right">
                      <div className="text-sm font-medium">
                        {productFilter === 'all' 
                          ? orderItems.length 
                          : productFilter === 'commercial'
                          ? orderItems.filter(item => item.productType === 'Commercial').length
                          : orderItems.filter(item => item.productType === 'POSM').length}
                      </div>
                      <div className="text-xs text-muted-foreground">Items</div>
                    </div>
                    <div className="text-right">
                      <div className="text-sm font-medium">
                        {productFilter === 'all'
                          ? orderItems.reduce((sum, item) => sum + (item.qtyApprovedTotal || 0), 0)
                          : productFilter === 'commercial'
                          ? orderItems.filter(item => item.productType === 'Commercial').reduce((sum, item) => sum + (item.qtyApprovedTotal || 0), 0)
                          : orderItems.filter(item => item.productType === 'POSM').reduce((sum, item) => sum + (item.qtyApprovedTotal || 0), 0)}
                      </div>
                      <div className="text-xs text-muted-foreground">Total Qty</div>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            {/* Table Content Based on Filter */}
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="px-4 py-3">SKU</TableHead>
                    <TableHead className="px-4 py-3">Product Name</TableHead>
                    <TableHead className="px-4 py-3 text-center">UOM</TableHead>
                    <TableHead className="px-4 py-3 text-center">Warehouse Stock</TableHead>
                    <TableHead className="px-4 py-3 text-center">Recommended</TableHead>
                    <TableHead className="px-4 py-3 text-center">Pre-Order</TableHead>
                    <TableHead className="px-4 py-3 text-center">Buffer Adj.</TableHead>
                    <TableHead className="px-4 py-3 text-center">Total Qty</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {orderItems
                    .filter(item => {
                      if (productFilter === 'all') return true;
                      if (productFilter === 'commercial') return item.productType === 'Commercial';
                      if (productFilter === 'posm') return item.productType === 'POSM';
                      return true;
                    })
                    .map((item, index) => {
                      const originalIndex = orderItems.findIndex(origItem => origItem === item);
                      return (
                        <TableRow key={originalIndex}>
                          <TableCell className="px-4 py-3">
                            <Badge variant="secondary" className="font-medium">
                              {item.itemCode.replace(/[\[\]]/g, '')}
                            </Badge>
                          </TableCell>
                          <TableCell className="px-4 py-3">
                            <span className="font-medium">{item.description}</span>
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            <Badge variant="outline" className="font-normal">
                              {item.uom}
                            </Badge>
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            <span className={item.availableStock < item.qtyApprovedTotal ? 'text-destructive font-semibold' : 'text-foreground font-medium'}>
                              {item.availableStock}
                            </span>
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            {(item.recommendedCustomers?.length || 0) > 0 ? (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleShowCustomerDetails(item, 'Recommended')}
                                className="font-medium text-blue-600 hover:text-blue-800"
                              >
                                {item.recommendedOrder}
                              </Button>
                            ) : (
                              <span className="font-medium">{item.recommendedOrder}</span>
                            )}
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            {(item.preOrderCustomers?.length || 0) > 0 ? (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => handleShowCustomerDetails(item, 'Pre-Order')}
                                className="font-medium text-purple-600 hover:text-purple-800"
                              >
                                {item.preOrder}
                              </Button>
                            ) : (
                              <span className="font-medium">{item.preOrder}</span>
                            )}
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            <span className="font-medium">{item.bufferAdjustment}</span>
                          </TableCell>
                          <TableCell className="px-4 py-3 text-center">
                            <Input
                              type="number"
                              value={item.qtyApprovedTotal || ''}
                              onChange={(e) => handleTotalQuantityChange(originalIndex, Number(e.target.value) || 0)}
                              className="w-20 text-center"
                              min="0"
                              max="10000"
                            />
                          </TableCell>
                        </TableRow>
                      );
                    })}
                </TableBody>
              </Table>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-center space-x-4 pt-6">
            <Button
              onClick={handleApprove}
              className="px-8 py-3 font-semibold"
              size="lg"
            >
              Approve
            </Button>
            <Button
              onClick={handleReject}
              variant="outline"
              className="px-8 py-3 font-semibold"
              size="lg"
            >
              Reject
            </Button>
          </div>
        </div>
      </div>

      {/* Customer Details Modal */}
      <Dialog open={showCustomerModal} onOpenChange={setShowCustomerModal}>
        <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="text-xl font-semibold">
              {selectedOrderType} Orders - {selectedProduct?.name}
            </DialogTitle>
          </DialogHeader>
          <div className="mt-4">
            <div className="bg-muted rounded-lg p-4 mb-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <span className="text-sm font-medium text-muted-foreground">Product Code:</span>
                  <p className="font-semibold">{selectedProduct?.code}</p>
                </div>
                <div>
                  <span className="text-sm font-medium text-muted-foreground">Order Type:</span>
                  <p className="font-semibold">{selectedOrderType}</p>
                </div>
                <div>
                  <span className="text-sm font-medium text-muted-foreground">Total Quantity:</span>
                  <p className="font-semibold text-foreground">{selectedQuantity} {orderItems.find(item => item.itemCode === selectedProduct?.code)?.uom || 'units'}</p>
                </div>
                <div>
                  <span className="text-sm font-medium text-muted-foreground">Customers:</span>
                  <p className="font-semibold">{selectedCustomers.length} customers</p>
                </div>
              </div>
            </div>
            
            <div className="space-y-4">
              {selectedCustomers.map((customer, index) => (
                <div key={index} className="bg-white border rounded-lg p-4">
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Customer</span>
                      <p className="font-semibold text-foreground">{customer.customer_name}</p>
                      <p className="text-sm text-muted-foreground">{customer.outlet_name}</p>
                    </div>
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Contact</span>
                      <p className="font-medium text-foreground">{customer.contact_person}</p>
                      <p className="text-sm text-muted-foreground">{customer.phone}</p>
                    </div>
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Quantity</span>
                      <p className="text-lg font-bold text-foreground">{customer.quantity} {orderItems.find(item => item.itemCode === selectedProduct?.code)?.uom || 'units'}</p>
                      <Badge variant="secondary" className="text-xs">{customer.outlet_type}</Badge>
                    </div>
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Order Date</span>
                      <p className="text-sm">{new Date(customer.order_date).toLocaleDateString()}</p>
                    </div>
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Delivery Date</span>
                      <p className="text-sm">{new Date(customer.delivery_date).toLocaleDateString()}</p>
                    </div>
                    <div>
                      <span className="text-xs font-medium text-muted-foreground uppercase">Email</span>
                      <p className="text-sm text-muted-foreground">{customer.email}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}