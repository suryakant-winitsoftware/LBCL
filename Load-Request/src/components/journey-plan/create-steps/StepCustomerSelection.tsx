import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Search, AlertCircle, Clock, Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';

interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  Type?: string;
  Status?: string;
  SeqNo?: number;
}

interface CustomerWithTime extends Customer {
  startTime: string;
  endTime: string;
  visitDuration: number;
  visitDay?: Date;
}

interface StepCustomerSelectionProps {
  form: UseFormReturn<any>;
  routeCustomers: Customer[];
  selectedCustomersWithTimes: CustomerWithTime[];
  setSelectedCustomersWithTimes: (customers: CustomerWithTime[]) => void;
  customerSearchTerm: string;
  setCustomerSearchTerm: (term: string) => void;
  dropdownsLoading: { customers: boolean };
  calculateCustomerTimes: () => void;
  toggleCustomerSelection: (customerUID: string) => void;
}

export const StepCustomerSelection: React.FC<StepCustomerSelectionProps> = ({
  form,
  routeCustomers,
  selectedCustomersWithTimes,
  setSelectedCustomersWithTimes,
  customerSearchTerm,
  setCustomerSearchTerm,
  dropdownsLoading,
  calculateCustomerTimes,
  toggleCustomerSelection,
}) => {
  const handleSelectAll = () => {
    const dayStartTime = form.getValues("dayStartsAt") || "00:00";
    const defaultDuration = form.getValues("defaultDuration") || 30;
    const defaultTravelTime = form.getValues("defaultTravelTime") || 30;
    
    let currentTime = dayStartTime;
    const allCustomersWithTimes: CustomerWithTime[] = [];
    
    routeCustomers.forEach((customer, index) => {
      const startTime = currentTime;
      const endTimeMinutes = 
        parseInt(startTime.split(":")[0]) * 60 +
        parseInt(startTime.split(":")[1]) +
        defaultDuration;
      const endHours = Math.floor(endTimeMinutes / 60);
      const endMins = endTimeMinutes % 60;
      const endTime = `${endHours.toString().padStart(2, "0")}:${endMins
        .toString()
        .padStart(2, "0")}`;
      
      allCustomersWithTimes.push({
        ...customer,
        startTime,
        endTime,
        visitDuration: defaultDuration,
      });
      
      if (index < routeCustomers.length - 1) {
        const nextStartMinutes = endTimeMinutes + defaultTravelTime;
        const nextStartHours = Math.floor(nextStartMinutes / 60);
        const nextStartMins = nextStartMinutes % 60;
        currentTime = `${nextStartHours
          .toString()
          .padStart(2, "0")}:${nextStartMins.toString().padStart(2, "0")}`;
      }
    });
    
    setSelectedCustomersWithTimes(allCustomersWithTimes);
    form.setValue("selectedCustomersWithTimes", allCustomersWithTimes.map(c => ({
      customerUID: c.UID,
      startTime: c.startTime,
      endTime: c.endTime,
      visitDuration: c.visitDuration,
    })));
  };

  const handleClearAll = () => {
    setSelectedCustomersWithTimes([]);
    form.setValue("selectedCustomersWithTimes", []);
  };

  const filteredCustomers = routeCustomers.filter(customer =>
    customer.Name?.toLowerCase().includes(customerSearchTerm.toLowerCase()) ||
    customer.Code?.toLowerCase().includes(customerSearchTerm.toLowerCase()) ||
    customer.Address?.toLowerCase().includes(customerSearchTerm.toLowerCase())
  );

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      <Card>
        <CardHeader>
          <CardTitle>Customer Selection</CardTitle>
          <CardDescription>
            Select customers and configure visit times for the journey plan
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {dropdownsLoading.customers ? (
            <div className="flex items-center justify-center py-8">
              <Loader2 className="h-8 w-8 animate-spin" />
            </div>
          ) : routeCustomers.length === 0 ? (
            <Alert>
              <AlertCircle className="h-4 w-4" />
              <AlertTitle>No Customers</AlertTitle>
              <AlertDescription>
                No customers found for the selected route. Please select a different route.
              </AlertDescription>
            </Alert>
          ) : (
            <>
              {/* Action Buttons and Counter */}
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleSelectAll}
                  >
                    Select All
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleClearAll}
                  >
                    Clear All
                  </Button>
                </div>
                <Badge variant="secondary" className="text-sm">
                  {selectedCustomersWithTimes.length} / {routeCustomers.length} selected
                </Badge>
              </div>

              {/* Search Bar */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-muted-foreground h-4 w-4" />
                <Input
                  type="text"
                  placeholder="Search customers by name, code, or address..."
                  value={customerSearchTerm}
                  onChange={(e) => setCustomerSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>

              {/* Customer List */}
              <ScrollArea className="h-[400px] border rounded-lg">
                <div className="p-4 space-y-2">
                  {filteredCustomers.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">
                      No customers match your search
                    </div>
                  ) : (
                    filteredCustomers.map((customer) => {
                      const isSelected = selectedCustomersWithTimes.some(c => c.UID === customer.UID);
                      const selectedCustomer = selectedCustomersWithTimes.find(c => c.UID === customer.UID);
                      
                      return (
                        <div
                          key={customer.UID}
                          className={cn(
                            "p-3 rounded-lg border transition-all cursor-pointer",
                            isSelected 
                              ? "bg-primary/5 border-primary shadow-sm" 
                              : "hover:bg-muted/50 hover:border-muted-foreground/20"
                          )}
                          onClick={() => toggleCustomerSelection(customer.UID)}
                        >
                          <div className="flex items-start gap-3">
                            <Checkbox
                              checked={isSelected}
                              onCheckedChange={() => toggleCustomerSelection(customer.UID)}
                              onClick={(e) => e.stopPropagation()}
                              className="mt-1"
                            />
                            <div className="flex-1">
                              <div className="flex items-center gap-2">
                                <span className="font-medium">{customer.Name}</span>
                                {customer.SeqNo && (
                                  <Badge variant="outline" className="text-xs">
                                    #{customer.SeqNo}
                                  </Badge>
                                )}
                              </div>
                              <div className="text-sm text-muted-foreground mt-1">
                                <span className="font-mono text-xs">{customer.Code}</span>
                                {customer.Address && (
                                  <span> • {customer.Address}</span>
                                )}
                                {customer.ContactNo && (
                                  <span> • {customer.ContactNo}</span>
                                )}
                              </div>
                              {isSelected && selectedCustomer && (
                                <div className="mt-2 flex items-center gap-2 text-sm">
                                  <Clock className="h-3 w-3 text-primary" />
                                  <span className="text-primary font-medium">
                                    {selectedCustomer.startTime} - {selectedCustomer.endTime}
                                  </span>
                                  <span className="text-muted-foreground">
                                    ({selectedCustomer.visitDuration} min visit)
                                  </span>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      );
                    })
                  )}
                </div>
              </ScrollArea>

              {/* Helper Text */}
              {selectedCustomersWithTimes.length === 0 && (
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>
                    Please select at least one customer to create a journey plan
                  </AlertDescription>
                </Alert>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
};