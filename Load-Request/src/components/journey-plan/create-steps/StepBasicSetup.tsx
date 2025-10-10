import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Filter, FilterX } from 'lucide-react';
import { OrganizationLevel } from '@/utils/organizationHierarchyUtils';
import { MultiSelect, MultiSelectOption } from '@/components/ui/multi-select';

interface StepBasicSetupProps {
  form: UseFormReturn<any>;
  routes: any[];
  employees: any[];
  vehicles: any[];
  locations: any[];
  dropdownsLoading: {
    organizations: boolean;
    routes: boolean;
    employees: boolean;
    vehicles: boolean;
    locations: boolean;
    customers: boolean;
  };
  orgLevels: OrganizationLevel[];
  selectedOrgs: string[];
  handleOrganizationSelect: (levelIndex: number, value: string) => void;
  showAvailableRoutesOnly?: boolean;
  onToggleRouteFilter?: () => void;
  allRoutesCount?: number;
  routesWithExistingPlans?: string[];
}

export const StepBasicSetup: React.FC<StepBasicSetupProps> = ({
  form,
  routes,
  employees,
  vehicles,
  locations,
  dropdownsLoading,
  orgLevels,
  selectedOrgs,
  handleOrganizationSelect,
  showAvailableRoutesOnly = true,
  onToggleRouteFilter,
  allRoutesCount = 0,
  routesWithExistingPlans = [],
}) => {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      <Card>
        <CardHeader>
          <CardTitle>Basic Setup</CardTitle>
          <CardDescription>
            Select organization, route, and employee for the journey plan
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Organization Hierarchy Selection */}
          <div className="space-y-4">
            <FormLabel>Organization Hierarchy *</FormLabel>
            {orgLevels.map((level, index) => (
              <FormItem key={index}>
                <FormLabel>{level.orgTypeName}</FormLabel>
                <Select
                  onValueChange={(value) => handleOrganizationSelect(index, value)}
                  value={selectedOrgs[index] || ""}
                  disabled={dropdownsLoading.organizations || level.organizations.length === 0}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder={`Select ${level.orgTypeName.toLowerCase()}`} />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {level.organizations.map((org) => (
                      <SelectItem key={org.UID} value={org.UID}>
                        {org.Name} ({org.Code})
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {index === 0 && <FormMessage />}
              </FormItem>
            ))}
            {orgLevels.length === 0 && dropdownsLoading.organizations && (
              <div className="text-sm text-muted-foreground">
                Loading organization hierarchy...
              </div>
            )}
          </div>

          {/* Route Filter Toggle */}
          {allRoutesCount > 0 && (
            <div className="flex items-center justify-between p-3 bg-muted/50 rounded-lg">
              <div className="flex items-center gap-2">
                <Filter className="h-4 w-4 text-muted-foreground" />
                <div className="flex flex-col">
                  <span className="text-sm font-medium">Route Filter</span>
                  <span className="text-xs text-muted-foreground">
                    {showAvailableRoutesOnly 
                      ? "Showing only routes without existing journey plans" 
                      : "Showing all routes (routes with existing plans are disabled)"
                    }
                  </span>
                </div>
                <Badge variant={showAvailableRoutesOnly ? "default" : "secondary"}>
                  {showAvailableRoutesOnly ? `${routes.length} Available` : `${allRoutesCount} Total`}
                </Badge>
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={onToggleRouteFilter}
                className="text-xs"
              >
                {showAvailableRoutesOnly ? (
                  <>
                    <FilterX className="h-3 w-3 mr-1" />
                    Show All Routes
                  </>
                ) : (
                  <>
                    <Filter className="h-3 w-3 mr-1" />
                    Show Available Only
                  </>
                )}
              </Button>
            </div>
          )}

          {/* Route Selection */}
          <FormField
            control={form.control}
            name="routeUID"
            render={({ field }) => (
              <FormItem>
                <FormLabel>
                  Route *
                  {showAvailableRoutesOnly && routes.length < allRoutesCount && (
                    <span className="text-xs text-muted-foreground ml-2">
                      ({routes.length} available of {allRoutesCount} total routes)
                    </span>
                  )}
                  {!showAvailableRoutesOnly && routes.length > 0 && (
                    <span className="text-xs text-muted-foreground ml-2">
                      (Routes with existing plans cannot be selected)
                    </span>
                  )}
                </FormLabel>
                <Select 
                  onValueChange={field.onChange} 
                  value={field.value}
                  disabled={!form.watch("orgUID") || dropdownsLoading.routes}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder={dropdownsLoading.routes ? "Loading..." : "Select route"} />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {routes.map((route) => (
                      <SelectItem 
                        key={route.Value || route.UID} 
                        value={route.Value || route.UID}
                        disabled={route.hasExistingPlan}
                        className={route.hasExistingPlan ? "opacity-60 bg-yellow-50 border-yellow-200 cursor-not-allowed" : ""}
                      >
                        <div className="flex items-center justify-between w-full">
                          <span className={route.hasExistingPlan ? "line-through" : ""}>{route.Label || route.Name}</span>
                          {route.hasExistingPlan && (
                            <Badge variant="destructive" className="ml-2 text-xs">
                              Journey Plan Exists
                            </Badge>
                          )}
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Employee Selection - Multi-select */}
          <FormField
            control={form.control}
            name="selectedEmployees"
            render={({ field }) => {
              // Convert employees to MultiSelectOption format
              const employeeOptions: MultiSelectOption[] = employees.map(emp => ({
                value: emp.JobPositionUID,
                label: `${emp.Name} - ${emp.LoginId}`,
                isAssigned: emp.IsAssignedToRoute
              }));

              return (
                <FormItem>
                  <FormLabel>
                    Employee(s) *
                    {employees.filter(e => e.IsAssignedToRoute).length > 1 && (
                      <span className="text-xs text-muted-foreground ml-2">
                        (Multiple employees available for this route)
                      </span>
                    )}
                  </FormLabel>
                  <FormControl>
                    <MultiSelect
                      options={employeeOptions}
                      selected={field.value || []}
                      onChange={(values) => {
                        field.onChange(values);
                        // Update loginIds for all selected employees
                        const selectedEmps = employees.filter(e => 
                          values.includes(e.JobPositionUID)
                        );
                        const loginIds = selectedEmps.map(e => e.LoginId);
                        form.setValue("loginIds", loginIds);
                        
                        // For backward compatibility, set single fields if only one selected
                        if (values.length === 1) {
                          form.setValue("jobPositionUID", values[0]);
                          form.setValue("loginId", loginIds[0]);
                        } else {
                          form.setValue("jobPositionUID", "");
                          form.setValue("loginId", "");
                        }
                      }}
                      placeholder={
                        dropdownsLoading.employees 
                          ? "Loading..." 
                          : employees.length === 0 
                          ? "Select a route first" 
                          : "Select employee(s)"
                      }
                      disabled={!form.watch("orgUID") || dropdownsLoading.employees || employees.length === 0}
                    />
                  </FormControl>
                  <FormMessage />
                  {field.value && field.value.length > 1 && (
                    <p className="text-xs text-muted-foreground mt-1">
                      {field.value.length} employees selected. Separate journey plans will be created for each.
                    </p>
                  )}
                </FormItem>
              );
            }}
          />

          {/* Optional Fields */}
          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="vehicleUID"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Vehicle (Optional)</FormLabel>
                  <Select 
                    onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)} 
                    value={field.value || "__none__"}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select vehicle" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="__none__">None</SelectItem>
                      {vehicles.map((vehicle) => (
                        <SelectItem key={vehicle.Value || vehicle.UID} value={vehicle.Value || vehicle.UID}>
                          {vehicle.Label || vehicle.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="locationUID"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Location (Optional)</FormLabel>
                  <Select 
                    onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)} 
                    value={field.value || "__none__"}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select location" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectItem value="__none__">None</SelectItem>
                      {locations.map((location) => (
                        <SelectItem key={location.Value || location.UID} value={location.Value || location.UID}>
                          {location.Label || location.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </FormItem>
              )}
            />
          </div>
        </CardContent>
      </Card>

      {/* Optional Enhanced Planning Fields */}
      <Card>
        <CardHeader>
          <CardTitle>Optional Planning Details</CardTitle>
          <CardDescription>
            Additional information to enhance journey plan execution
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Warehouse Org and Emergency Contact */}
          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="routeWHOrgUID"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Warehouse Organization (Optional)</FormLabel>
                  <FormControl>
                    <Select 
                      onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)} 
                      value={field.value || "__none__"}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select warehouse org" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="__none__">None</SelectItem>
                        {/* Add warehouse organizations here when API is available */}
                        <SelectItem value="WH_001">Main Warehouse</SelectItem>
                        <SelectItem value="WH_002">Regional Warehouse</SelectItem>
                      </SelectContent>
                    </Select>
                  </FormControl>
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="emergencyContact"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Emergency Contact (Optional)</FormLabel>
                  <FormControl>
                    <Input 
                      {...field} 
                      placeholder="Contact number or name"
                    />
                  </FormControl>
                </FormItem>
              )}
            />
          </div>

        </CardContent>
      </Card>
    </motion.div>
  );
};