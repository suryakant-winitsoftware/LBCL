import React, { useState, useEffect, useRef } from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion, AnimatePresence } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useToast } from '@/components/ui/use-toast';
import { Separator } from '@/components/ui/separator';
import { 
  Search,
  Plus,
  X,
  MapPin,
  Clock,
  GripVertical,
  Store,
  Route,
  AlertCircle,
  CheckCircle,
  Info,
  ChevronRight,
  Users,
  Calendar
} from 'lucide-react';
import moment from 'moment';
import { cn } from '@/lib/utils';

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

interface StepStoreAssignmentProps {
  form: UseFormReturn<any>;
  availableStores: Customer[];
  loading?: boolean;
  onStoresUpdate?: (stores: RouteCustomer[]) => void;
}

export const StepStoreAssignment: React.FC<StepStoreAssignmentProps> = ({
  form,
  availableStores = [],
  loading = false,
  onStoresUpdate
}) => {
  const { toast } = useToast();
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStores, setSelectedStores] = useState<RouteCustomer[]>([]);
  const [draggedStore, setDraggedStore] = useState<RouteCustomer | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);
  
  // Get route timing configuration from form
  const routeStartTime = form.watch('visitTime') || '09:00';
  const routeEndTime = form.watch('endTime') || '18:00';
  const defaultVisitDuration = form.watch('visitDuration') || 30;
  const defaultTravelTime = form.watch('travelTime') || 15;
  
  // Load existing route customers if editing
  useEffect(() => {
    const existingCustomers = form.getValues('routeCustomers');
    if (existingCustomers && existingCustomers.length > 0) {
      setSelectedStores(existingCustomers);
    }
  }, []);

  // Update form when stores change
  useEffect(() => {
    form.setValue('routeCustomers', selectedStores);
    form.setValue('totalCustomers', selectedStores.length);
    if (onStoresUpdate) {
      onStoresUpdate(selectedStores);
    }
  }, [selectedStores]);

  // Calculate visit time for a store based on its position
  const calculateVisitTime = (index: number): string => {
    const [hours, minutes] = routeStartTime.split(':').map(Number);
    let totalMinutes = hours * 60 + minutes;
    
    // Add time for all previous stores
    for (let i = 0; i < index; i++) {
      totalMinutes += (selectedStores[i]?.visitDuration || defaultVisitDuration);
      totalMinutes += (selectedStores[i]?.travelTime || defaultTravelTime);
    }
    
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
  };

  // Calculate end time for a store
  const calculateEndTime = (startTime: string, duration: number): string => {
    const [hours, minutes] = startTime.split(':').map(Number);
    const totalMinutes = hours * 60 + minutes + duration;
    
    const newHours = Math.floor(totalMinutes / 60);
    const newMinutes = totalMinutes % 60;
    
    return `${String(newHours).padStart(2, '0')}:${String(newMinutes).padStart(2, '0')}`;
  };

  // Add store to route
  const addStoreToRoute = (store: Customer) => {
    // Check if store is already added
    if (selectedStores.find(s => s.UID === store.UID)) {
      toast({
        title: "Store Already Added",
        description: `${store.Name} is already in the route`,
        variant: "destructive"
      });
      return;
    }

    const newIndex = selectedStores.length;
    const visitTime = calculateVisitTime(newIndex);
    
    const newRouteCustomer: RouteCustomer = {
      ...store,
      seqNo: newIndex + 1,
      visitTime,
      endTime: calculateEndTime(visitTime, defaultVisitDuration),
      visitDuration: defaultVisitDuration,
      travelTime: defaultTravelTime,
      isActive: true
    };

    setSelectedStores([...selectedStores, newRouteCustomer]);
    
    toast({
      title: "Store Added",
      description: `${store.Name} has been added to the route`,
    });
  };

  // Remove store from route
  const removeStoreFromRoute = (storeUID: string) => {
    const filtered = selectedStores.filter(s => s.UID !== storeUID);
    // Recalculate sequence and times
    const updated = filtered.map((store, index) => ({
      ...store,
      seqNo: index + 1,
      visitTime: calculateVisitTime(index),
      endTime: calculateEndTime(calculateVisitTime(index), store.visitDuration)
    }));
    
    setSelectedStores(updated);
    
    toast({
      title: "Store Removed",
      description: "Store has been removed from the route",
    });
  };

  // Handle drag start
  const handleDragStart = (e: React.DragEvent, store: RouteCustomer, index?: number) => {
    setDraggedStore(store);
    e.dataTransfer.effectAllowed = 'move';
    if (index !== undefined) {
      e.dataTransfer.setData('sourceIndex', index.toString());
    }
  };

  // Handle drag over
  const handleDragOver = (e: React.DragEvent, index?: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    setDragOverIndex(index ?? null);
  };

  // Handle drag leave
  const handleDragLeave = () => {
    setDragOverIndex(null);
  };

  // Handle drop
  const handleDrop = (e: React.DragEvent, targetIndex?: number) => {
    e.preventDefault();
    setDragOverIndex(null);
    
    if (!draggedStore) return;
    
    const sourceIndexStr = e.dataTransfer.getData('sourceIndex');
    const sourceIndex = sourceIndexStr ? parseInt(sourceIndexStr) : undefined;
    
    if (sourceIndex !== undefined && targetIndex !== undefined) {
      // Reordering existing stores
      const newStores = [...selectedStores];
      newStores.splice(sourceIndex, 1);
      newStores.splice(targetIndex, 0, draggedStore);
      
      // Recalculate sequence and times
      const updated = newStores.map((store, index) => ({
        ...store,
        seqNo: index + 1,
        visitTime: calculateVisitTime(index),
        endTime: calculateEndTime(calculateVisitTime(index), store.visitDuration)
      }));
      
      setSelectedStores(updated);
    } else if (targetIndex === undefined && sourceIndex === undefined) {
      // Adding new store from available list - handled by addStoreToRoute
    }
    
    setDraggedStore(null);
  };

  // Update individual store timing
  const updateStoreTiming = (storeUID: string, field: 'visitDuration' | 'travelTime', value: number) => {
    const updated = selectedStores.map((store, index) => {
      if (store.UID === storeUID) {
        const updatedStore = { ...store, [field]: value };
        // Recalculate times for this and subsequent stores
        const visitTime = calculateVisitTime(index);
        return {
          ...updatedStore,
          visitTime,
          endTime: calculateEndTime(visitTime, updatedStore.visitDuration)
        };
      }
      return store;
    });
    
    // Recalculate times for all subsequent stores
    const fullyUpdated = updated.map((store, index) => ({
      ...store,
      visitTime: calculateVisitTime(index),
      endTime: calculateEndTime(calculateVisitTime(index), store.visitDuration)
    }));
    
    setSelectedStores(fullyUpdated);
  };

  // Filter available stores
  const filteredAvailableStores = availableStores.filter(store => 
    !selectedStores.find(s => s.UID === store.UID) &&
    (store.Name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
     store.Code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
     store.Address?.toLowerCase().includes(searchTerm.toLowerCase()))
  );

  // Calculate total route time
  const calculateTotalRouteTime = () => {
    if (selectedStores.length === 0) return '0 hours';
    
    const totalMinutes = selectedStores.reduce((sum, store) => 
      sum + store.visitDuration + store.travelTime, 0
    );
    
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    
    return `${hours}h ${minutes}m`;
  };

  // Calculate estimated completion time
  const calculateCompletionTime = () => {
    if (selectedStores.length === 0) return routeStartTime;
    
    const lastStore = selectedStores[selectedStores.length - 1];
    return lastStore.endTime;
  };

  return (
    <div className="space-y-6">
      {/* Header with Instructions */}
      <Card className="bg-blue-50 border-blue-200">
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5" />
            <div>
              <h4 className="font-semibold text-blue-900 mb-1">Store Assignment</h4>
              <p className="text-sm text-blue-800">
                Add stores to this route and arrange them in the order they should be visited. 
                The system will calculate visit times based on the duration and travel time settings.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Available Stores */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Store className="h-5 w-5" />
              Available Stores
            </CardTitle>
            <CardDescription>
              Search and add stores to the route
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {/* Search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
                <Input
                  placeholder="Search by name, code, or address..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9"
                />
              </div>

              {/* Store List */}
              <ScrollArea className="h-[500px]">
                <div className="space-y-2 pr-4">
                  {loading ? (
                    <div className="text-center py-8 text-gray-500">
                      Loading stores...
                    </div>
                  ) : filteredAvailableStores.length === 0 ? (
                    <div className="text-center py-8">
                      <Store className="h-12 w-12 mx-auto mb-2 text-gray-300" />
                      <p className="text-gray-500">
                        {searchTerm ? 'No stores found matching your search' : 'No available stores'}
                      </p>
                    </div>
                  ) : (
                    filteredAvailableStores.map((store) => (
                      <motion.div
                        key={store.UID}
                        initial={{ opacity: 0, y: 10 }}
                        animate={{ opacity: 1, y: 0 }}
                        className="p-3 border rounded-lg hover:bg-gray-50 cursor-pointer transition-colors group"
                        onClick={() => addStoreToRoute(store)}
                        draggable
                        onDragStart={(e) => {
                          e.stopPropagation();
                          const tempRouteCustomer: RouteCustomer = {
                            ...store,
                            seqNo: 0,
                            visitTime: '',
                            endTime: '',
                            visitDuration: defaultVisitDuration,
                            travelTime: defaultTravelTime,
                            isActive: true
                          };
                          handleDragStart(e, tempRouteCustomer);
                        }}
                      >
                        <div className="flex items-center justify-between">
                          <div className="flex-1">
                            <p className="font-medium">{store.Name}</p>
                            <div className="flex items-center gap-4 text-sm text-gray-600 mt-1">
                              <span className="flex items-center gap-1">
                                <MapPin className="h-3 w-3" />
                                {store.Code}
                              </span>
                              {store.Address && (
                                <span className="truncate max-w-[200px]">
                                  {store.Address}
                                </span>
                              )}
                            </div>
                          </div>
                          <Plus className="h-4 w-4 text-gray-400 group-hover:text-gray-600" />
                        </div>
                      </motion.div>
                    ))
                  )}
                </div>
              </ScrollArea>

              {/* Summary */}
              <div className="pt-4 border-t">
                <div className="flex justify-between text-sm text-gray-600">
                  <span>Available stores</span>
                  <span className="font-medium">{filteredAvailableStores.length}</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Selected Stores with Sequence */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg flex items-center gap-2">
              <Route className="h-5 w-5" />
              Route Stores ({selectedStores.length})
            </CardTitle>
            <CardDescription>
              Drag to reorder visit sequence
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ScrollArea className="h-[500px]">
              {selectedStores.length === 0 ? (
                <div 
                  className="flex flex-col items-center justify-center h-[400px] border-2 border-dashed rounded-lg"
                  onDragOver={(e) => handleDragOver(e)}
                  onDragLeave={handleDragLeave}
                  onDrop={(e) => {
                    e.preventDefault();
                    if (draggedStore && !selectedStores.find(s => s.UID === draggedStore.UID)) {
                      addStoreToRoute(draggedStore);
                    }
                    setDragOverIndex(null);
                  }}
                >
                  <MapPin className="h-12 w-12 text-gray-300 mb-2" />
                  <p className="text-gray-500 text-center">
                    Drag stores here or click + to add
                  </p>
                  <p className="text-sm text-gray-400 mt-1">
                    Stores will be visited in the order shown
                  </p>
                </div>
              ) : (
                <div className="space-y-2 pr-4">
                  {selectedStores.map((store, index) => (
                    <motion.div
                      key={store.UID}
                      layout
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      exit={{ opacity: 0, x: 20 }}
                      draggable
                      onDragStart={(e) => handleDragStart(e, store, index)}
                      onDragOver={(e) => handleDragOver(e, index)}
                      onDragLeave={handleDragLeave}
                      onDrop={(e) => handleDrop(e, index)}
                      className={cn(
                        "p-4 border rounded-lg bg-white transition-all cursor-move",
                        dragOverIndex === index && "border-blue-500 bg-blue-50"
                      )}
                    >
                      <div className="flex items-start gap-3">
                        <div className="flex items-center gap-2">
                          <GripVertical className="h-4 w-4 text-gray-400" />
                          <Badge variant="outline" className="min-w-[32px] justify-center">
                            {store.seqNo}
                          </Badge>
                        </div>
                        
                        <div className="flex-1 space-y-3">
                          <div>
                            <p className="font-medium">{store.Name}</p>
                            <p className="text-sm text-gray-600">{store.Code}</p>
                          </div>
                          
                          <div className="grid grid-cols-2 gap-3">
                            <div>
                              <Label className="text-xs">Visit Time</Label>
                              <div className="flex items-center gap-1 text-sm">
                                <Clock className="h-3 w-3" />
                                <span>{store.visitTime} - {store.endTime}</span>
                              </div>
                            </div>
                            
                            <div className="flex gap-2">
                              <div>
                                <Label className="text-xs">Duration</Label>
                                <Input
                                  type="number"
                                  value={store.visitDuration}
                                  onChange={(e) => updateStoreTiming(store.UID, 'visitDuration', parseInt(e.target.value) || 30)}
                                  className="h-7 w-16 text-xs"
                                  min="5"
                                  max="240"
                                />
                              </div>
                              
                              <div>
                                <Label className="text-xs">Travel</Label>
                                <Input
                                  type="number"
                                  value={store.travelTime}
                                  onChange={(e) => updateStoreTiming(store.UID, 'travelTime', parseInt(e.target.value) || 15)}
                                  className="h-7 w-16 text-xs"
                                  min="0"
                                  max="120"
                                />
                              </div>
                            </div>
                          </div>
                        </div>
                        
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => removeStoreFromRoute(store.UID)}
                          className="hover:text-red-600"
                        >
                          <X className="h-4 w-4" />
                        </Button>
                      </div>
                    </motion.div>
                  ))}
                </div>
              )}
            </ScrollArea>

            {/* Route Summary */}
            {selectedStores.length > 0 && (
              <div className="mt-4 pt-4 border-t space-y-2">
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Total stores</span>
                  <span className="font-medium">{selectedStores.length}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Total duration</span>
                  <span className="font-medium">{calculateTotalRouteTime()}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Start time</span>
                  <span className="font-medium">{routeStartTime}</span>
                </div>
                <div className="flex justify-between text-sm">
                  <span className="text-gray-600">Est. completion</span>
                  <span className="font-medium">{calculateCompletionTime()}</span>
                </div>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Validation Messages */}
      {selectedStores.length === 0 && (
        <Card className="bg-yellow-50 border-yellow-200">
          <CardContent className="p-4">
            <div className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-yellow-600" />
              <p className="text-sm text-yellow-800">
                Please add at least one store to the route to continue
              </p>
            </div>
          </CardContent>
        </Card>
      )}

      {selectedStores.length > 0 && calculateCompletionTime() > routeEndTime && (
        <Card className="bg-orange-50 border-orange-200">
          <CardContent className="p-4">
            <div className="flex items-center gap-2">
              <AlertCircle className="h-5 w-5 text-orange-600" />
              <p className="text-sm text-orange-800">
                The estimated completion time ({calculateCompletionTime()}) exceeds the route end time ({routeEndTime}). 
                Consider reducing visit durations or number of stores.
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
};