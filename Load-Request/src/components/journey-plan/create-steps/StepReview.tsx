import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Separator } from '@/components/ui/separator';
import { ScrollArea } from '@/components/ui/scroll-area';
import { 
  Calendar, 
  Clock, 
  User, 
  Building2, 
  MapPin, 
  Car, 
  Users, 
  AlertCircle,
  CheckCircle2,
  CalendarDays
} from 'lucide-react';
import { format } from 'date-fns';

interface CustomerWithTime {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  ContactNo?: string;
  startTime: string;
  endTime: string;
  visitDuration: number;
  visitDay?: Date;
}

interface StepReviewProps {
  form: UseFormReturn<any>;
  organizations: any[];
  routes: any[];
  employees: any[];
  vehicles: any[];
  locations: any[];
  selectedCustomersWithTimes: CustomerWithTime[];
  selectedSchedule: any;
}

export const StepReview: React.FC<StepReviewProps> = ({
  form,
  organizations,
  routes,
  employees,
  vehicles,
  locations,
  selectedCustomersWithTimes,
  selectedSchedule,
}) => {
  const formValues = form.watch();
  
  const selectedOrg = organizations.find(o => o.UID === formValues.orgUID);
  const selectedRoute = routes.find(r => r.Value === formValues.routeUID || r.UID === formValues.routeUID);
  
  // Handle multiple selected employees (with safety check for undefined employees)
  const selectedEmployeesList = formValues.selectedEmployees?.length > 0 && employees
    ? employees.filter(e => formValues.selectedEmployees.includes(e.JobPositionUID))
    : [];
  
  // For backward compatibility, also check single employee selection
  const selectedEmployee = selectedEmployeesList.length > 0 
    ? selectedEmployeesList[0] 
    : (employees || []).find(e => e.JobPositionUID === formValues.jobPositionUID);
    
  const selectedVehicle = (vehicles || []).find(v => v.Value === formValues.vehicleUID || v.UID === formValues.vehicleUID);
  const selectedLocation = (locations || []).find(l => l.Value === formValues.locationUID || l.UID === formValues.locationUID);
  
  const isMultiDay = formValues.endDate && formValues.endDate !== formValues.visitDate;
  
  const getScheduleTypeLabel = (type: string) => {
    switch(type) {
      case 'D': return 'Daily';
      case 'W': return 'Weekly';
      case 'F': return 'Fortnightly';
      case 'M': return 'Monthly';
      default: return type;
    }
  };

  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      <Card>
        <CardHeader>
          <CardTitle>Review & Create</CardTitle>
          <CardDescription>
            Review your journey plan details before creating
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Basic Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Basic Information</h3>
            <div className="space-y-3">
              {/* Organization */}
              <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                <Building2 className="h-5 w-5 text-primary mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-muted-foreground">Organization</p>
                  <p className="font-medium">{selectedOrg?.Name || 'Not selected'}</p>
                </div>
              </div>
              
              {/* Route */}
              <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                <MapPin className="h-5 w-5 text-primary mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-muted-foreground">Route</p>
                  <p className="font-medium">{selectedRoute?.Label || selectedRoute?.Name || 'Not selected'}</p>
                </div>
              </div>
              
              {/* Employees */}
              <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                <User className="h-5 w-5 text-primary mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-muted-foreground">Employee{selectedEmployeesList.length > 1 ? 's' : ''}</p>
                  {selectedEmployeesList.length > 0 ? (
                    <div className="space-y-1">
                      {selectedEmployeesList.map((emp, index) => (
                        <p key={emp.JobPositionUID} className="font-medium">
                          {emp.Name}
                          {emp.LoginId && (
                            <span className="text-sm text-muted-foreground ml-2">
                              ({emp.LoginId})
                            </span>
                          )}
                        </p>
                      ))}
                    </div>
                  ) : (
                    <p className="font-medium">
                      {selectedEmployee?.Name || 'Not selected'}
                      {selectedEmployee?.LoginId && (
                        <span className="text-sm text-muted-foreground ml-2">
                          ({selectedEmployee.LoginId})
                        </span>
                      )}
                    </p>
                  )}
                </div>
              </div>
              
              {/* Vehicle (if selected) */}
              {selectedVehicle && (
                <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                  <Car className="h-5 w-5 text-primary mt-0.5" />
                  <div className="flex-1">
                    <p className="text-sm text-muted-foreground">Vehicle</p>
                    <p className="font-medium">{selectedVehicle.Label || selectedVehicle.Name}</p>
                  </div>
                </div>
              )}
              
              {/* Location (if selected) */}
              {selectedLocation && (
                <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                  <MapPin className="h-5 w-5 text-primary mt-0.5" />
                  <div className="flex-1">
                    <p className="text-sm text-muted-foreground">Location</p>
                    <p className="font-medium">{selectedLocation.Label || selectedLocation.Name}</p>
                  </div>
                </div>
              )}
            </div>
            
            {/* Optional Planning Details Section */}
            {(formValues.routeWHOrgUID || formValues.emergencyContact) && (
              <>
                <Separator className="my-6" />
                <div className="space-y-3">
                  <h3 className="text-lg font-semibold flex items-center gap-2">
                    <Building2 className="h-5 w-5" />
                    Optional Planning Details
                  </h3>
                  
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                    {/* Warehouse Organization */}
                    {formValues.routeWHOrgUID && (
                      <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                        <Building2 className="h-4 w-4 text-primary mt-0.5" />
                        <div className="flex-1">
                          <p className="text-sm text-muted-foreground">Warehouse Organization</p>
                          <p className="font-medium text-sm">{formValues.routeWHOrgUID}</p>
                        </div>
                      </div>
                    )}

                    {/* Emergency Contact */}
                    {formValues.emergencyContact && (
                      <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                        <User className="h-4 w-4 text-primary mt-0.5" />
                        <div className="flex-1">
                          <p className="text-sm text-muted-foreground">Emergency Contact</p>
                          <p className="font-medium text-sm">{formValues.emergencyContact}</p>
                        </div>
                      </div>
                    )}

                  </div>
                </div>
              </>
            )}
          </div>

          <Separator />

          {/* Schedule Information */}
          <div className="space-y-4">
            <h3 className="text-lg font-semibold">Schedule Information</h3>
            <div className="space-y-3">
              {/* Visit Date */}
              <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                <Calendar className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-muted-foreground">Visit Date</p>
                  <p className="font-medium">
                    {formValues.visitDate ? format(new Date(formValues.visitDate), 'PPP') : 'Not selected'}
                  </p>
                </div>
              </div>
              
              {/* End Date (if multi-day) */}
              {isMultiDay && (
                <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                  <CalendarDays className="h-5 w-5 text-muted-foreground mt-0.5" />
                  <div className="flex-1">
                    <p className="text-sm text-muted-foreground">End Date</p>
                    <p className="font-medium">
                      {format(new Date(formValues.endDate), 'PPP')}
                    </p>
                  </div>
                </div>
              )}
              
              {/* Working Hours */}
              <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                <Clock className="h-5 w-5 text-muted-foreground mt-0.5" />
                <div className="flex-1">
                  <p className="text-sm text-muted-foreground">Working Hours</p>
                  <p className="font-medium">
                    {formValues.dayStartsAt || '00:00'} - {formValues.dayEndsBy || '00:00'}
                  </p>
                </div>
              </div>
              
              {/* Schedule Type (if available) */}
              {selectedSchedule && (
                <div className="flex items-start gap-3 p-3 bg-background rounded-lg border">
                  <CalendarDays className="h-5 w-5 text-muted-foreground mt-0.5" />
                  <div className="flex-1">
                    <p className="text-sm text-muted-foreground">Schedule Type</p>
                    <p className="font-medium">
                      {getScheduleTypeLabel(selectedSchedule.ScheduleType)}
                      {selectedSchedule.ScheduleCode && (
                        <span className="text-sm text-muted-foreground ml-2">
                          ({selectedSchedule.ScheduleCode})
                        </span>
                      )}
                    </p>
                  </div>
                </div>
              )}
              
              {/* Visit Duration and Travel Time */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div className="p-3 bg-muted/50 rounded-lg border">
                  <p className="text-sm text-muted-foreground">Default Visit Duration</p>
                  <p className="text-lg font-semibold">{formValues.defaultDuration || 30} minutes</p>
                </div>
                <div className="p-3 bg-muted/50 rounded-lg border">
                  <p className="text-sm text-muted-foreground">Default Travel Time</p>
                  <p className="text-lg font-semibold">{formValues.defaultTravelTime || 30} minutes</p>
                </div>
              </div>
            </div>
          </div>

          <Separator />

          {/* Customer Selection */}
          <div>
            <div className="flex items-center justify-between mb-3">
              <h3 className="text-lg font-semibold">Customer Selection</h3>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="text-sm">
                  <Calendar className="h-3 w-3 mr-1" />
                  {formValues.visitDate ? format(new Date(formValues.visitDate), 'dd MMM yyyy') : 'No date'}
                </Badge>
                <Badge variant="secondary" className="text-sm">
                  <Users className="h-3 w-3 mr-1" />
                  {selectedCustomersWithTimes.length} customer{selectedCustomersWithTimes.length !== 1 ? 's' : ''}
                </Badge>
              </div>
            </div>
            
            {selectedCustomersWithTimes.length > 0 ? (
              <>
                <div className="mb-2 p-2 bg-muted/50 rounded-lg flex items-center justify-between">
                  <span className="text-sm text-muted-foreground">
                    Scheduled for: {formValues.visitDate ? format(new Date(formValues.visitDate), 'EEEE, dd MMMM yyyy') : 'Date not set'}
                  </span>
                  <span className="text-sm font-medium">
                    Working hours: {formValues.dayStartsAt || '00:00'} - {formValues.dayEndsBy || '00:00'}
                  </span>
                </div>
                <ScrollArea className="h-[300px] border rounded-lg">
                  <div className="p-4 space-y-2">
                    {selectedCustomersWithTimes.map((customer, index) => (
                    <div
                      key={customer.UID}
                      className="flex items-center justify-between p-3 rounded-lg border bg-card hover:bg-muted/50 transition-colors"
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <Badge variant="outline" className="text-xs">
                            #{index + 1}
                          </Badge>
                          <span className="font-medium">{customer.Name}</span>
                          <span className="text-sm text-muted-foreground">
                            ({customer.Code})
                          </span>
                        </div>
                        {customer.Address && (
                          <p className="text-sm text-muted-foreground mt-1">
                            {customer.Address}
                          </p>
                        )}
                        {customer.visitDay && (
                          <p className="text-xs text-muted-foreground mt-1">
                            <CalendarDays className="h-3 w-3 inline mr-1" />
                            {format(new Date(customer.visitDay), 'EEEE, dd MMM yyyy')}
                          </p>
                        )}
                      </div>
                      <div className="flex flex-col items-end gap-1">
                        <div className="flex items-center gap-2 text-sm">
                          <Clock className="h-3 w-3 text-primary" />
                          <span className="font-medium">
                            {customer.startTime} - {customer.endTime}
                          </span>
                        </div>
                        <span className="text-xs text-muted-foreground">
                          Duration: {customer.visitDuration} minutes
                        </span>
                      </div>
                    </div>
                  ))}
                  </div>
                </ScrollArea>
              </>
            ) : (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  No customers selected. Please go back and select at least one customer.
                </AlertDescription>
              </Alert>
            )}
          </div>

          {/* Summary */}
          <div className="bg-primary/5 rounded-lg p-4">
            <div className="flex items-center gap-2 mb-2">
              <CheckCircle2 className="h-5 w-5 text-primary" />
              <h4 className="font-semibold">Journey Plan Summary</h4>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mt-3">
              <div>
                <p className="text-sm text-muted-foreground">Total Customers</p>
                <p className="text-2xl font-bold">{selectedCustomersWithTimes.length}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Visit Time</p>
                <p className="text-2xl font-bold">
                  {selectedCustomersWithTimes.reduce((sum, c) => sum + c.visitDuration, 0)} min
                </p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Status</p>
                <Badge variant={selectedCustomersWithTimes.length > 0 ? "default" : "destructive"}>
                  {selectedCustomersWithTimes.length > 0 ? 'Ready to Create' : 'Incomplete'}
                </Badge>
              </div>
            </div>
          </div>

          {/* Validation Messages */}
          {selectedCustomersWithTimes.length === 0 && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertDescription>
                Please select at least one customer before creating the journey plan.
              </AlertDescription>
            </Alert>
          )}
        </CardContent>
      </Card>
    </motion.div>
  );
};