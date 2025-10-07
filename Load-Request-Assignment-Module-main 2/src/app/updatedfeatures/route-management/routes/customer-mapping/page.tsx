"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { useForm } from "react-hook-form";
import { Card, CardHeader, CardTitle, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Label } from "@/components/ui/label";
import {
  ArrowLeft,
  Save,
  Route,
  Store,
  Clock,
  MapPin,
  Info,
  Search,
  Plus,
  X,
  GripVertical,
  ChevronRight,
  Loader2
} from "lucide-react";
import { routeServiceFixed } from "@/services/routeService.fixed";
import { customerService } from "@/services/customerService";
import { authService } from "@/lib/auth-service";
import moment from 'moment';
import { cn } from "@/lib/utils";

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
}

interface RouteCustomer extends Customer {
  seqNo: number;
  visitTime: string;
  endTime: string;
  visitDuration: number;
  travelTime: number;
  isActive: boolean;
}

interface DraggedStore {
  store: Customer | RouteCustomer;
  sourceIndex?: number;
  isFromRoute?: boolean;
}

interface RouteDetails {
  UID: string;
  Code: string;
  Name: string;
  OrgUID: string;
  VisitTime?: string;
  EndTime?: string;
  VisitDuration: number;
  TravelTime: number;
  TotalCustomers: number;
  IsActive: boolean;
  Status: string;
}

export default function CustomerMappingPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const routeUID = searchParams.get("routeUID");
  
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [routeDetails, setRouteDetails] = useState<RouteDetails | null>(null);
  const [availableStores, setAvailableStores] = useState<Customer[]>([]);
  const [selectedStores, setSelectedStores] = useState<RouteCustomer[]>([]);
  const [searchTerm, setSearchTerm] = useState("");
  const [draggedStore, setDraggedStore] = useState<DraggedStore | null>(null);
  const currentUser = authService.getCurrentUser();

  const form = useForm({
    defaultValues: {
      routeCustomers: [],
      totalCustomers: 0,
      visitTime: "09:00",
      endTime: "18:00",
      visitDuration: 30,
      travelTime: 15
    }
  });

  // Calculate times
  const calculateVisitTime = (index: number): string => {
    const visitTime = form.getValues('visitTime') || '09:00';
    const [hours, minutes] = visitTime.split(':').map(Number);
    let totalMinutes = hours * 60 + minutes;
    
    for (let i = 0; i < index; i++) {
      totalMinutes += (selectedStores[i]?.visitDuration || form.getValues('visitDuration'));
      if (i < index - 1) {
        totalMinutes += (selectedStores[i]?.travelTime || form.getValues('travelTime'));
      }
    }
    
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
  };

  useEffect(() => {
    if (routeUID) {
      loadRouteAndStores();
    } else {
      // If no routeUID, redirect to route selection
      router.push("/updatedfeatures/route-management/routes/manage");
    }
  }, [routeUID]);

  const loadRouteAndStores = async () => {
    setLoading(true);
    try {
      // Load route details
      const route = await routeService.getRouteByUID(routeUID!);
      setRouteDetails(route);
      
      // Set form defaults from route
      form.setValue('visitTime', route.VisitTime || '09:00');
      form.setValue('endTime', route.EndTime || '18:00');
      form.setValue('visitDuration', route.VisitDuration || 30);
      form.setValue('travelTime', route.TravelTime || 15);
      
      // Load available stores for the organization
      const stores = await loadAvailableStores(route.OrgUID);
      setAvailableStores(stores);
      
      // Load existing route customers if any
      const existingCustomers = await routeServiceFixed.getRouteCustomers(routeUID!);
      if (existingCustomers && existingCustomers.length > 0) {
        setSelectedStores(existingCustomers);
        form.setValue('routeCustomers', existingCustomers);
        form.setValue('totalCustomers', existingCustomers.length);
      }
    } catch (error) {
      console.error("Error loading route data:", error);
      toast({
        title: "Error",
        description: "Failed to load route information",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const loadAvailableStores = async (orgUID: string): Promise<Customer[]> => {
    try {
      // Fetch real stores from API
      const stores = await customerService.getStores(orgUID);
      
      // If no stores found, try getting all customers for the org
      if (!stores || stores.length === 0) {
        const allCustomers = await customerService.getCustomersByOrg(orgUID);
        // Filter for active customers that could be stores
        return allCustomers.filter(c => c.IsActive !== false);
      }
      
      return stores;
    } catch (error) {
      console.error("Error loading stores:", error);
      
      // Fallback: try to get all customers
      try {
        const allCustomers = await customerService.getAllCustomers();
        return allCustomers.filter(c => 
          c.OrgUID === orgUID && c.IsActive !== false
        );
      } catch (fallbackError) {
        console.error("Fallback error loading customers:", error);
        toast({
          title: "Warning",
          description: "Could not load stores. Please check your connection.",
          variant: "destructive"
        });
        return [];
      }
    }
  };

  // Add store to route
  const addStoreToRoute = (store: Customer) => {
    const newIndex = selectedStores.length;
    const visitTime = calculateVisitTime(newIndex);
    const visitDuration = form.getValues('visitDuration');
    const travelTime = form.getValues('travelTime');
    
    const newRouteCustomer: RouteCustomer = {
      ...store,
      seqNo: newIndex + 1,
      visitTime,
      endTime: moment(visitTime, 'HH:mm').add(visitDuration, 'minutes').format('HH:mm'),
      visitDuration,
      travelTime,
      isActive: true
    };

    setSelectedStores([...selectedStores, newRouteCustomer]);
    setAvailableStores(prev => prev.filter(s => s.UID !== store.UID));
  };

  // Remove store from route
  const removeStoreFromRoute = (storeUID: string) => {
    const store = selectedStores.find(s => s.UID === storeUID);
    if (!store) return;

    const filtered = selectedStores.filter(s => s.UID !== storeUID);
    const updated = filtered.map((s, index) => ({
      ...s,
      seqNo: index + 1,
      visitTime: calculateVisitTime(index)
    }));
    
    setSelectedStores(updated);
    
    // Add back to available
    const { seqNo, visitTime, endTime, visitDuration, travelTime, isActive, ...baseStore } = store;
    setAvailableStores(prev => [...prev, baseStore]);
  };

  // Drag handlers
  const handleDragStart = (e: React.DragEvent, store: Customer | RouteCustomer, isFromRoute = false, index?: number) => {
    setDraggedStore({
      store,
      sourceIndex: index,
      isFromRoute
    });
    e.dataTransfer.effectAllowed = 'move';
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  };

  const handleDrop = (e: React.DragEvent, targetIndex?: number) => {
    e.preventDefault();
    
    if (!draggedStore) return;

    if (draggedStore.isFromRoute && targetIndex !== undefined) {
      // Reordering within route
      const newStores = [...selectedStores];
      const [removed] = newStores.splice(draggedStore.sourceIndex!, 1);
      newStores.splice(targetIndex, 0, removed);
      
      // Update sequence numbers and times
      const updated = newStores.map((s, index) => ({
        ...s,
        seqNo: index + 1,
        visitTime: calculateVisitTime(index)
      }));
      
      setSelectedStores(updated);
    } else if (!draggedStore.isFromRoute) {
      // Adding from available to route
      addStoreToRoute(draggedStore.store as Customer);
    }
    
    setDraggedStore(null);
  };

  const handleSave = async () => {
    if (selectedStores.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please add at least one store to the route",
        variant: "destructive"
      });
      return;
    }

    setSaving(true);
    try {
      // Prepare customer data
      const customersData = selectedStores.map(s => ({
        storeUID: s.UID,
        seqNo: s.seqNo,
        visitTime: s.visitTime,
        endTime: s.endTime,
        visitDuration: s.visitDuration,
        travelTime: s.travelTime,
        isActive: s.isActive
      }));

      // Save route customers
      await routeServiceFixed.updateRouteCustomers(routeUID!, customersData);
      
      // Update route total customers count
      await routeServiceFixed.updateRoute({
        UID: routeUID!,
        TotalCustomers: selectedStores.length,
        ModifiedBy: currentUser?.uid || 'SYSTEM',
        ModifiedTime: moment().toISOString()
      });
      
      toast({
        title: "Success",
        description: `Successfully assigned ${selectedStores.length} stores to the route`,
      });
      
      router.push("/updatedfeatures/route-management/routes/manage");
    } catch (error) {
      console.error("Error saving route customers:", error);
      toast({
        title: "Error",
        description: "Failed to save store assignments",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6 space-y-6">
        <Skeleton className="h-10 w-64" />
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <Skeleton className="h-[600px]" />
          <Skeleton className="h-[600px]" />
        </div>
      </div>
    );
  }

  if (!routeDetails) {
    return (
      <div className="container mx-auto p-6">
        <Card>
          <CardContent className="p-12 text-center">
            <Route className="h-12 w-12 mx-auto mb-4 text-gray-400" />
            <h3 className="text-lg font-semibold mb-2">Route Not Found</h3>
            <p className="text-gray-600 mb-4">The requested route could not be found.</p>
            <Button onClick={() => router.push("/updatedfeatures/route-management/routes/manage")}>
              Back to Routes
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-4">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => router.push("/updatedfeatures/route-management/routes/manage")}
            >
              <ArrowLeft className="h-4 w-4 mr-2" />
              Back to Routes
            </Button>
          </div>
          <div className="flex gap-2">
            <Button
              variant="outline"
              onClick={() => router.push(`/updatedfeatures/route-management/routes/view/${routeUID}`)}
            >
              View Route Details
            </Button>
            <Button
              onClick={handleSave}
              disabled={saving || selectedStores.length === 0}
            >
              <Save className="h-4 w-4 mr-2" />
              {saving ? "Saving..." : `Save Assignments (${routeCustomers.length})`}
            </Button>
          </div>
        </div>

        <div>
          <h1 className="text-2xl font-bold mb-2">Customer Mapping</h1>
          <p className="text-gray-600">Assign and sequence stores for route visits</p>
        </div>
      </div>

      {/* Route Information */}
      <Card className="mb-6">
        <CardContent className="p-4">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <p className="text-sm text-gray-600">Route Code</p>
              <p className="font-medium">{routeDetails.Code}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Route Name</p>
              <p className="font-medium">{routeDetails.Name}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Current Stores</p>
              <p className="font-medium">{routeDetails.TotalCustomers || 0}</p>
            </div>
            <div>
              <p className="text-sm text-gray-600">Status</p>
              <Badge variant={routeDetails.IsActive ? "success" : "secondary"}>
                {routeDetails.Status}
              </Badge>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Info Message */}
      <Card className="mb-6 bg-blue-50 border-blue-200">
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5" />
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">Store Assignment Instructions</h4>
              <ul className="text-sm text-blue-800 space-y-1">
                <li>• Search and add stores from the available list on the left</li>
                <li>• Drag and drop stores on the right to reorder the visit sequence</li>
                <li>• Adjust visit duration and travel time for each store as needed</li>
                <li>• The system will automatically calculate visit times based on sequence and durations</li>
                <li>• Click Save when you're satisfied with the assignments</li>
              </ul>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Store Assignment Component */}
      <StepStoreAssignment
        form={form}
        availableStores={availableStores}
        loading={false}
        onStoresUpdate={handleStoresUpdate}
      />

      {/* Action Buttons */}
      <div className="flex justify-between mt-6">
        <Button
          variant="outline"
          onClick={() => router.push("/updatedfeatures/route-management/routes/manage")}
        >
          Cancel
        </Button>
        <Button
          onClick={handleSave}
          disabled={saving || selectedStores.length === 0}
          className="min-w-[200px]"
        >
          {saving ? (
            <>
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
              Saving...
            </>
          ) : (
            <>
              <Save className="h-4 w-4 mr-2" />
              Save {selectedStores.length} Store Assignments
            </>
          )}
        </Button>
      </div>
    </div>
  );
}