"use client";

import React, { useState, useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Card, CardHeader, CardTitle, CardContent, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/use-toast";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import {
  ArrowLeft,
  Save,
  Search,
  Plus,
  X,
  MapPin,
  Clock,
  ChevronRight,
  Store,
  Navigation,
  Loader2,
  CheckCircle,
  AlertCircle,
  GripVertical,
  Filter,
  RefreshCw,
  Building2,
  Phone,
  Hash,
  Calendar,
  Timer,
  Route as RouteIcon
} from "lucide-react";
import { routeServiceFixed } from "@/services/routeService.fixed";
import { customerService } from "@/services/customerService";
import { authService } from "@/lib/auth-service";
import { motion, AnimatePresence, Reorder } from "framer-motion";
import { cn } from "@/lib/utils";

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
  City?: string;
  PinCode?: string;
}

interface RouteCustomer extends Customer {
  seqNo: number;
  visitTime: string;
  endTime: string;
  visitDuration: number;
  travelTime: number;
  isActive: boolean;
}

export default function ModernCustomerMappingPage() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { toast } = useToast();
  const routeUID = searchParams.get("routeUID");

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTab, setSelectedTab] = useState("available");
  
  // Data states
  const [routeDetails, setRouteDetails] = useState<any>(null);
  const [availableCustomers, setAvailableCustomers] = useState<Customer[]>([]);
  const [assignedCustomers, setAssignedCustomers] = useState<RouteCustomer[]>([]);
  const [filteredCustomers, setFilteredCustomers] = useState<Customer[]>([]);
  
  // Route timing settings
  const [routeSettings, setRouteSettings] = useState({
    startTime: "09:00",
    endTime: "18:00",
    defaultVisitDuration: 30,
    defaultTravelTime: 15
  });

  const currentUser = authService.getCurrentUser();

  useEffect(() => {
    if (routeUID) {
      loadRouteAndCustomers();
    } else {
      router.push("/updatedfeatures/route-management/routes/manage");
    }
  }, [routeUID]);

  useEffect(() => {
    // Filter customers based on search
    const filtered = availableCustomers.filter(customer => {
      const searchLower = searchTerm.toLowerCase();
      return (
        customer.Name?.toLowerCase().includes(searchLower) ||
        customer.Code?.toLowerCase().includes(searchLower) ||
        customer.Address?.toLowerCase().includes(searchLower) ||
        customer.ContactNo?.includes(searchTerm)
      );
    });
    setFilteredCustomers(filtered);
  }, [searchTerm, availableCustomers]);

  const loadRouteAndCustomers = async () => {
    setLoading(true);
    try {
      // Load route details
      const route = await routeServiceFixed.getRouteByUID(routeUID!);
      setRouteDetails(route);
      
      // Set timing from route
      setRouteSettings({
        startTime: route.VisitTime || "09:00",
        endTime: route.EndTime || "18:00",
        defaultVisitDuration: route.VisitDuration || 30,
        defaultTravelTime: route.TravelTime || 15
      });

      // Load all customers for the organization
      const customers = await customerService.getStores(route.OrgUID || currentUser?.orgUID || "");
      
      // Load existing route customers
      const existingCustomers = await routeServiceFixed.getRouteCustomers(routeUID!);
      
      // Filter out already assigned customers from available list
      const assignedUIDs = new Set(existingCustomers.map((c: any) => c.UID || c.storeUID));
      const available = customers.filter(c => !assignedUIDs.has(c.UID));
      
      setAvailableCustomers(available);
      setFilteredCustomers(available);
      setAssignedCustomers(existingCustomers);
    } catch (error) {
      console.error("Error loading data:", error);
      toast({
        title: "Error",
        description: "Failed to load route information",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  const calculateVisitTime = (index: number): string => {
    const [hours, minutes] = routeSettings.startTime.split(':').map(Number);
    let totalMinutes = hours * 60 + minutes;
    
    for (let i = 0; i < index; i++) {
      totalMinutes += assignedCustomers[i].visitDuration;
      if (i < index - 1) {
        totalMinutes += assignedCustomers[i].travelTime;
      }
    }
    
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
  };

  const calculateEndTime = (startTime: string, duration: number): string => {
    const [hours, minutes] = startTime.split(':').map(Number);
    const totalMinutes = hours * 60 + minutes + duration;
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
  };

  const addCustomerToRoute = (customer: Customer) => {
    const newIndex = assignedCustomers.length;
    const visitTime = calculateVisitTime(newIndex);
    
    const newRouteCustomer: RouteCustomer = {
      ...customer,
      seqNo: newIndex + 1,
      visitTime,
      endTime: calculateEndTime(visitTime, routeSettings.defaultVisitDuration),
      visitDuration: routeSettings.defaultVisitDuration,
      travelTime: routeSettings.defaultTravelTime,
      isActive: true
    };

    setAssignedCustomers([...assignedCustomers, newRouteCustomer]);
    
    // Remove from available list
    setAvailableCustomers(prev => prev.filter(c => c.UID !== customer.UID));
    
    toast({
      title: "Customer Added",
      description: `${customer.Name} has been added to the route`,
    });
  };

  const removeCustomerFromRoute = (customerUID: string) => {
    const customer = assignedCustomers.find(c => c.UID === customerUID);
    if (!customer) return;

    // Remove from assigned
    const filtered = assignedCustomers.filter(c => c.UID !== customerUID);
    
    // Recalculate sequence and times
    const updated = filtered.map((c, index) => ({
      ...c,
      seqNo: index + 1,
      visitTime: calculateVisitTime(index),
      endTime: calculateEndTime(calculateVisitTime(index), c.visitDuration)
    }));
    
    setAssignedCustomers(updated);
    
    // Add back to available
    const { seqNo, visitTime, endTime, visitDuration, travelTime, isActive, ...baseCustomer } = customer;
    setAvailableCustomers(prev => [...prev, baseCustomer]);
    
    toast({
      title: "Customer Removed",
      description: `${customer.Name} has been removed from the route`,
    });
  };

  const handleReorder = (newOrder: RouteCustomer[]) => {
    // Recalculate times for new order
    const updated = newOrder.map((customer, index) => ({
      ...customer,
      seqNo: index + 1,
      visitTime: calculateVisitTime(index),
      endTime: calculateEndTime(calculateVisitTime(index), customer.visitDuration)
    }));
    setAssignedCustomers(updated);
  };

  const updateCustomerTiming = (customerUID: string, field: 'visitDuration' | 'travelTime', value: number) => {
    const updated = assignedCustomers.map((customer, index) => {
      if (customer.UID === customerUID) {
        const updatedCustomer = { ...customer, [field]: value };
        const visitTime = calculateVisitTime(index);
        return {
          ...updatedCustomer,
          visitTime,
          endTime: calculateEndTime(visitTime, updatedCustomer.visitDuration)
        };
      }
      return customer;
    });
    
    // Recalculate all times
    const fullyUpdated = updated.map((customer, index) => ({
      ...customer,
      visitTime: calculateVisitTime(index),
      endTime: calculateEndTime(calculateVisitTime(index), customer.visitDuration)
    }));
    
    setAssignedCustomers(fullyUpdated);
  };

  const handleSave = async () => {
    if (assignedCustomers.length === 0) {
      toast({
        title: "Validation Error",
        description: "Please add at least one customer to the route",
        variant: "destructive"
      });
      return;
    }

    setSaving(true);
    try {
      // Prepare customer data for API
      const customersData = assignedCustomers.map(c => ({
        storeUID: c.UID,
        seqNo: c.seqNo,
        visitTime: c.visitTime,
        endTime: c.endTime,
        visitDuration: c.visitDuration,
        travelTime: c.travelTime,
        isActive: c.isActive
      }));

      await routeServiceFixed.updateRouteCustomers(routeUID!, customersData);
      
      // Update route with customer count
      await routeServiceFixed.updateRoute({
        UID: routeUID!,
        TotalCustomers: assignedCustomers.length,
        ModifiedBy: currentUser?.uid || 'SYSTEM',
        ModifiedTime: new Date().toISOString()
      });
      
      toast({
        title: "Success",
        description: `Successfully saved ${assignedCustomers.length} customers to the route`,
      });
      
      router.push("/updatedfeatures/route-management/routes/manage");
    } catch (error) {
      console.error("Error saving:", error);
      toast({
        title: "Error",
        description: "Failed to save customer assignments",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  const getTotalRouteTime = () => {
    const totalMinutes = assignedCustomers.reduce((sum, c) => 
      sum + c.visitDuration + c.travelTime, 0
    );
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return `${hours}h ${minutes}m`;
  };

  if (loading) {
    return (
      <div className="container mx-auto p-6">
        <div className="space-y-4">
          <Skeleton className="h-10 w-64" />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Skeleton className="h-[600px]" />
            <Skeleton className="h-[600px]" />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-950">
      <div className="container mx-auto p-6">
        {/* Header */}
        <div className="mb-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button
                variant="ghost"
                size="icon"
                onClick={() => router.push("/updatedfeatures/route-management/routes/manage")}
              >
                <ArrowLeft className="h-5 w-5" />
              </Button>
              <div>
                <h1 className="text-3xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
                  Customer Mapping
                </h1>
                <p className="text-gray-600 dark:text-gray-400 mt-1">
                  Assign and sequence customers for route visits
                </p>
              </div>
            </div>
            <div className="flex gap-3">
              <Button variant="outline" onClick={loadRouteAndCustomers}>
                <RefreshCw className="h-4 w-4 mr-2" />
                Refresh
              </Button>
              <Button 
                onClick={handleSave} 
                disabled={saving || assignedCustomers.length === 0}
                className="bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700"
              >
                {saving ? (
                  <>
                    <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                    Saving...
                  </>
                ) : (
                  <>
                    <Save className="h-4 w-4 mr-2" />
                    Save ({assignedCustomers.length} Customers)
                  </>
                )}
              </Button>
            </div>
          </div>
        </div>

        {/* Route Info Card */}
        {routeDetails && (
          <Card className="mb-6 border-blue-200 dark:border-blue-900 bg-gradient-to-r from-blue-50 to-purple-50 dark:from-blue-950 dark:to-purple-950">
            <CardContent className="p-6">
              <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div>
                  <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 mb-1">
                    <RouteIcon className="h-4 w-4" />
                    <span className="text-sm">Route Code</span>
                  </div>
                  <p className="font-semibold">{routeDetails.Code}</p>
                </div>
                <div>
                  <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 mb-1">
                    <Building2 className="h-4 w-4" />
                    <span className="text-sm">Route Name</span>
                  </div>
                  <p className="font-semibold">{routeDetails.Name}</p>
                </div>
                <div>
                  <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 mb-1">
                    <Store className="h-4 w-4" />
                    <span className="text-sm">Total Customers</span>
                  </div>
                  <p className="font-semibold">{assignedCustomers.length}</p>
                </div>
                <div>
                  <div className="flex items-center gap-2 text-gray-600 dark:text-gray-400 mb-1">
                    <Clock className="h-4 w-4" />
                    <span className="text-sm">Total Time</span>
                  </div>
                  <p className="font-semibold">{getTotalRouteTime()}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {/* Main Content */}
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Available Customers */}
          <Card className="h-[700px] flex flex-col">
            <CardHeader className="pb-3">
              <CardTitle className="flex items-center gap-2">
                <Store className="h-5 w-5" />
                Available Customers
              </CardTitle>
              <CardDescription>
                Search and add customers to the route
              </CardDescription>
            </CardHeader>
            <CardContent className="flex-1 flex flex-col">
              {/* Search */}
              <div className="relative mb-4">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  placeholder="Search by name, code, or phone..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>

              {/* Customer List */}
              <ScrollArea className="flex-1">
                <div className="space-y-2 pr-4">
                  {filteredCustomers.length === 0 ? (
                    <div className="text-center py-12">
                      <Store className="h-12 w-12 mx-auto mb-3 text-gray-300" />
                      <p className="text-gray-500">
                        {searchTerm ? 'No customers found' : 'No available customers'}
                      </p>
                    </div>
                  ) : (
                    <AnimatePresence>
                      {filteredCustomers.map((customer) => (
                        <motion.div
                          key={customer.UID}
                          initial={{ opacity: 0, x: -20 }}
                          animate={{ opacity: 1, x: 0 }}
                          exit={{ opacity: 0, x: -20 }}
                          className="p-4 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800 cursor-pointer transition-all group"
                          onClick={() => addCustomerToRoute(customer)}
                        >
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <div className="flex items-center gap-2 mb-1">
                                <p className="font-semibold">{customer.Name}</p>
                                <Badge variant="outline" className="text-xs">
                                  {customer.Code}
                                </Badge>
                              </div>
                              <div className="flex items-center gap-4 text-sm text-gray-600 dark:text-gray-400">
                                {customer.Address && (
                                  <span className="flex items-center gap-1">
                                    <MapPin className="h-3 w-3" />
                                    {customer.Address}
                                  </span>
                                )}
                                {customer.ContactNo && (
                                  <span className="flex items-center gap-1">
                                    <Phone className="h-3 w-3" />
                                    {customer.ContactNo}
                                  </span>
                                )}
                              </div>
                            </div>
                            <Plus className="h-5 w-5 text-gray-400 group-hover:text-blue-600 transition-colors" />
                          </div>
                        </motion.div>
                      ))}
                    </AnimatePresence>
                  )}
                </div>
              </ScrollArea>

              {/* Summary */}
              <div className="pt-4 mt-4 border-t">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600 dark:text-gray-400">Available</span>
                  <span className="font-semibold">{filteredCustomers.length} customers</span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Assigned Customers */}
          <Card className="h-[700px] flex flex-col">
            <CardHeader className="pb-3">
              <CardTitle className="flex items-center gap-2">
                <Navigation className="h-5 w-5" />
                Route Sequence
              </CardTitle>
              <CardDescription>
                Drag to reorder, click timing to edit
              </CardDescription>
            </CardHeader>
            <CardContent className="flex-1 flex flex-col">
              {assignedCustomers.length === 0 ? (
                <div className="flex-1 flex items-center justify-center">
                  <div className="text-center">
                    <div className="w-24 h-24 mx-auto mb-4 rounded-full bg-gray-100 dark:bg-gray-800 flex items-center justify-center">
                      <MapPin className="h-12 w-12 text-gray-400" />
                    </div>
                    <p className="text-gray-500 mb-2">No customers assigned</p>
                    <p className="text-sm text-gray-400">
                      Add customers from the left panel
                    </p>
                  </div>
                </div>
              ) : (
                <>
                  <ScrollArea className="flex-1">
                    <Reorder.Group
                      axis="y"
                      values={assignedCustomers}
                      onReorder={handleReorder}
                      className="space-y-2 pr-4"
                    >
                      {assignedCustomers.map((customer, index) => (
                        <Reorder.Item
                          key={customer.UID}
                          value={customer}
                          className="relative"
                        >
                          <div className="p-4 border rounded-lg bg-white dark:bg-gray-900 hover:shadow-md transition-all">
                            <div className="flex items-start gap-3">
                              <div className="flex items-center gap-2">
                                <GripVertical className="h-5 w-5 text-gray-400 cursor-move" />
                                <div className="w-8 h-8 rounded-full bg-gradient-to-r from-blue-600 to-purple-600 text-white flex items-center justify-center text-sm font-semibold">
                                  {customer.seqNo}
                                </div>
                              </div>

                              <div className="flex-1">
                                <div className="flex items-center justify-between mb-2">
                                  <div>
                                    <p className="font-semibold">{customer.Name}</p>
                                    <p className="text-sm text-gray-600 dark:text-gray-400">
                                      {customer.Code} • {customer.Address}
                                    </p>
                                  </div>
                                  <Button
                                    variant="ghost"
                                    size="icon"
                                    onClick={() => removeCustomerFromRoute(customer.UID)}
                                    className="hover:text-red-600"
                                  >
                                    <X className="h-4 w-4" />
                                  </Button>
                                </div>

                                <div className="grid grid-cols-2 gap-4 mt-3">
                                  <div>
                                    <Label className="text-xs text-gray-600 dark:text-gray-400">Visit Time</Label>
                                    <div className="flex items-center gap-1 text-sm font-medium">
                                      <Clock className="h-3 w-3" />
                                      {customer.visitTime} - {customer.endTime}
                                    </div>
                                  </div>
                                  
                                  <div className="flex gap-2">
                                    <div className="flex-1">
                                      <Label className="text-xs text-gray-600 dark:text-gray-400">Duration</Label>
                                      <Input
                                        type="number"
                                        value={customer.visitDuration}
                                        onChange={(e) => updateCustomerTiming(
                                          customer.UID, 
                                          'visitDuration', 
                                          parseInt(e.target.value) || 30
                                        )}
                                        className="h-7 text-xs"
                                        min="5"
                                        max="240"
                                      />
                                    </div>
                                    
                                    <div className="flex-1">
                                      <Label className="text-xs text-gray-600 dark:text-gray-400">Travel</Label>
                                      <Input
                                        type="number"
                                        value={customer.travelTime}
                                        onChange={(e) => updateCustomerTiming(
                                          customer.UID, 
                                          'travelTime', 
                                          parseInt(e.target.value) || 15
                                        )}
                                        className="h-7 text-xs"
                                        min="0"
                                        max="120"
                                      />
                                    </div>
                                  </div>
                                </div>
                              </div>
                            </div>

                            {/* Connection Line */}
                            {index < assignedCustomers.length - 1 && (
                              <div className="absolute left-[30px] bottom-0 w-0.5 h-4 bg-gradient-to-b from-blue-600 to-purple-600 transform translate-y-full z-10" />
                            )}
                          </div>
                        </Reorder.Item>
                      ))}
                    </Reorder.Group>
                  </ScrollArea>

                  {/* Summary */}
                  <div className="pt-4 mt-4 border-t space-y-2">
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600 dark:text-gray-400">Total Customers</span>
                      <span className="font-semibold">{assignedCustomers.length}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600 dark:text-gray-400">Route Duration</span>
                      <span className="font-semibold">{getTotalRouteTime()}</span>
                    </div>
                    <div className="flex justify-between text-sm">
                      <span className="text-gray-600 dark:text-gray-400">Est. Completion</span>
                      <span className="font-semibold">
                        {assignedCustomers.length > 0 
                          ? assignedCustomers[assignedCustomers.length - 1].endTime 
                          : routeSettings.startTime}
                      </span>
                    </div>
                  </div>
                </>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Warning if completion time exceeds route end time */}
        {assignedCustomers.length > 0 && 
         assignedCustomers[assignedCustomers.length - 1].endTime > routeSettings.endTime && (
          <Alert className="mt-4 border-orange-200 bg-orange-50 dark:bg-orange-950">
            <AlertCircle className="h-4 w-4 text-orange-600" />
            <AlertDescription className="text-orange-800 dark:text-orange-200">
              The estimated completion time ({assignedCustomers[assignedCustomers.length - 1].endTime}) 
              exceeds the route end time ({routeSettings.endTime}). 
              Consider reducing visit durations or number of customers.
            </AlertDescription>
          </Alert>
        )}
      </div>
    </div>
  );
}