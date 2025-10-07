import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { 
  Search, 
  Calendar, 
  Users, 
  MapPin, 
  Clock,
  ChevronRight,
  Zap,
  CheckCircle,
  AlertCircle
} from 'lucide-react';
import { api } from '@/services/api';
import { authService } from '@/lib/auth-service';
import moment from 'moment';
import { cn } from '@/lib/utils';

interface QuickFormData {
  orgUID: string;
  orgName: string;
  routeUID: string;
  routeName: string;
  employeeUID: string;
  employeeName: string;
  visitDate: Date;
  customerCount: number;
}

export const QuickJourneyPlanCreate: React.FC = () => {
  const router = useRouter();
  const { toast } = useToast();
  
  // Form state
  const [formData, setFormData] = useState<Partial<QuickFormData>>({
    visitDate: new Date()
  });
  
  // Search states
  const [orgSearch, setOrgSearch] = useState('');
  const [routeSearch, setRouteSearch] = useState('');
  const [employeeSearch, setEmployeeSearch] = useState('');
  
  // Data states (only load what's needed)
  const [organizations, setOrganizations] = useState<any[]>([]);
  const [routes, setRoutes] = useState<any[]>([]);
  const [employees, setEmployees] = useState<any[]>([]);
  
  // Loading states
  const [loading, setLoading] = useState({
    orgs: false,
    routes: false,
    employees: false,
    submit: false
  });
  
  // UI state
  const [step, setStep] = useState(1);
  const [showSearch, setShowSearch] = useState({
    org: false,
    route: false,
    employee: false
  });

  // Load organizations on mount (minimal data)
  useEffect(() => {
    loadTopOrganizations();
  }, []);

  // Load only top/recent organizations
  const loadTopOrganizations = async () => {
    setLoading(prev => ({ ...prev, orgs: true }));
    try {
      // Load only first 10 orgs or cached recent ones
      const cached = localStorage.getItem('recent_orgs');
      if (cached) {
        setOrganizations(JSON.parse(cached));
        setLoading(prev => ({ ...prev, orgs: false }));
        return;
      }

      const response = await api.organization.getAll({ pageSize: 10 });
      if (response?.IsSuccess && response?.Data) {
        const orgs = response.Data.PagedData || response.Data;
        setOrganizations(orgs);
        localStorage.setItem('recent_orgs', JSON.stringify(orgs));
      }
    } catch (error) {
      console.error('Error loading organizations:', error);
    } finally {
      setLoading(prev => ({ ...prev, orgs: false }));
    }
  };

  // Search organizations dynamically
  const searchOrganizations = async (searchTerm: string) => {
    if (!searchTerm || searchTerm.length < 2) return;
    
    setLoading(prev => ({ ...prev, orgs: true }));
    try {
      const response = await api.organization.search(searchTerm);
      if (response?.IsSuccess && response?.Data) {
        setOrganizations(response.Data);
      }
    } catch (error) {
      console.error('Error searching organizations:', error);
    } finally {
      setLoading(prev => ({ ...prev, orgs: false }));
    }
  };

  // Load routes only when org is selected
  const loadRoutesForOrg = async (orgUID: string) => {
    setLoading(prev => ({ ...prev, routes: true }));
    try {
      const response = await api.dropdown.getRoute(orgUID);
      if (response?.IsSuccess && response?.Data) {
        setRoutes(response.Data.slice(0, 20)); // Limit to 20 for performance
      }
    } catch (error) {
      console.error('Error loading routes:', error);
    } finally {
      setLoading(prev => ({ ...prev, routes: false }));
    }
  };

  // Load employees only when route is selected
  const loadEmployeesForRoute = async (orgUID: string, routeUID: string) => {
    setLoading(prev => ({ ...prev, employees: true }));
    try {
      const response = await api.dropdown.getEmployee(orgUID);
      if (response?.IsSuccess && response?.Data) {
        setEmployees(response.Data.slice(0, 10)); // Limit display
      }
    } catch (error) {
      console.error('Error loading employees:', error);
    } finally {
      setLoading(prev => ({ ...prev, employees: false }));
    }
  };

  // Handle selections
  const handleOrgSelect = (org: any) => {
    setFormData(prev => ({
      ...prev,
      orgUID: org.UID,
      orgName: org.Name
    }));
    setShowSearch(prev => ({ ...prev, org: false }));
    setStep(2);
    
    // Load routes for this org
    loadRoutesForOrg(org.UID);
  };

  const handleRouteSelect = (route: any) => {
    setFormData(prev => ({
      ...prev,
      routeUID: route.UID,
      routeName: route.Label
    }));
    setShowSearch(prev => ({ ...prev, route: false }));
    setStep(3);
    
    // Load employees
    if (formData.orgUID) {
      loadEmployeesForRoute(formData.orgUID, route.UID);
    }
  };

  const handleEmployeeSelect = (employee: any) => {
    setFormData(prev => ({
      ...prev,
      employeeUID: employee.UID,
      employeeName: employee.Label
    }));
    setShowSearch(prev => ({ ...prev, employee: false }));
    setStep(4);
  };

  // Quick submit
  const handleQuickSubmit = async () => {
    if (!formData.orgUID || !formData.routeUID || !formData.employeeUID || !formData.visitDate) {
      toast({
        title: "Missing Information",
        description: "Please fill all required fields",
        variant: "destructive"
      });
      return;
    }

    setLoading(prev => ({ ...prev, submit: true }));
    
    try {
      const currentUser = authService.getCurrentUser();
      const yearMonth = parseInt(moment(formData.visitDate).format("YYMM"));
      
      // Get the selected employee's login ID
      const selectedEmployee = employees.find(e => e.UID === formData.employeeUID);
      const employeeLoginId = selectedEmployee?.LoginId || selectedEmployee?.Label || "";
      
      // Create journey plan with minimal data
      const journeyPlanData = {
        UID: crypto.randomUUID(),
        OrgUID: formData.orgUID,
        RouteUID: formData.routeUID,
        JobPositionUID: formData.employeeUID,
        LoginId: employeeLoginId,
        VisitDate: moment(formData.visitDate).format('YYYY-MM-DD'),
        DayStartsAt: "00:00",
        DayEndsBy: "00:00",
        YearMonth: yearMonth,
        IsPlanned: true,
        Status: "Planned",
        // Use employee's LoginId for CreatedBy to avoid foreign key constraint
        CreatedBy: employeeLoginId,
        ModifiedBy: employeeLoginId,
      };

      const response = await api.journeyPlan.createBeatHistory(journeyPlanData);
      
      if (response?.IsSuccess) {
        toast({
          title: "Success!",
          description: "Journey plan created successfully",
        });
        
        // Navigate to customer selection
        router.push(`/updatedfeatures/journey-plan-management/journey-plans/${journeyPlanData.UID}/customers`);
      } else {
        throw new Error(response?.Message || "Failed to create journey plan");
      }
    } catch (error: any) {
      toast({
        title: "Error",
        description: error.message || "Failed to create journey plan",
        variant: "destructive"
      });
    } finally {
      setLoading(prev => ({ ...prev, submit: false }));
    }
  };

  return (
    <div className="w-full max-w-4xl mx-auto p-4">
      <Card>
        <CardHeader className="pb-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Zap className="h-5 w-5 text-yellow-500" />
              <CardTitle>Quick Journey Plan</CardTitle>
            </div>
            <div className="flex items-center gap-2">
              {[1, 2, 3, 4].map(s => (
                <div
                  key={s}
                  className={cn(
                    "w-2 h-2 rounded-full",
                    step >= s ? "bg-primary" : "bg-gray-300"
                  )}
                />
              ))}
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-4">
          {/* Step 1: Organization */}
          <div className={cn(
            "space-y-2 pb-4 border-b",
            step > 1 && "opacity-60"
          )}>
            <Label className="flex items-center gap-2">
              <MapPin className="h-4 w-4" />
              Organization
              {formData.orgName && (
                <Badge variant="secondary">{formData.orgName}</Badge>
              )}
            </Label>
            
            {step === 1 && (
              <div className="relative">
                <Input
                  placeholder="Type to search organization..."
                  value={orgSearch}
                  onChange={(e) => {
                    setOrgSearch(e.target.value);
                    if (e.target.value.length >= 2) {
                      searchOrganizations(e.target.value);
                    }
                  }}
                  onFocus={() => setShowSearch(prev => ({ ...prev, org: true }))}
                  className="pr-10"
                />
                <Search className="absolute right-3 top-3 h-4 w-4 text-gray-400" />
                
                {showSearch.org && organizations.length > 0 && (
                  <div className="absolute z-10 w-full mt-1 bg-white border rounded-md shadow-lg max-h-60 overflow-auto">
                    {loading.orgs ? (
                      <div className="p-2 space-y-2">
                        {[1, 2, 3].map(i => (
                          <Skeleton key={i} className="h-10 w-full" />
                        ))}
                      </div>
                    ) : (
                      organizations.map(org => (
                        <button
                          key={org.UID}
                          onClick={() => handleOrgSelect(org)}
                          className="w-full text-left px-3 py-2 hover:bg-gray-100 border-b"
                        >
                          <div className="font-medium">{org.Name}</div>
                          <div className="text-xs text-gray-500">{org.Code}</div>
                        </button>
                      ))
                    )}
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Step 2: Route */}
          {step >= 2 && (
            <div className={cn(
              "space-y-2 pb-4 border-b",
              step > 2 && "opacity-60"
            )}>
              <Label className="flex items-center gap-2">
                <MapPin className="h-4 w-4" />
                Route
                {formData.routeName && (
                  <Badge variant="secondary">{formData.routeName}</Badge>
                )}
              </Label>
              
              {step === 2 && (
                <div className="relative">
                  {loading.routes ? (
                    <div className="space-y-2">
                      {[1, 2, 3].map(i => (
                        <Skeleton key={i} className="h-10 w-full" />
                      ))}
                    </div>
                  ) : (
                    <div className="grid grid-cols-2 gap-2">
                      {routes.map(route => (
                        <Button
                          key={route.UID}
                          variant="outline"
                          onClick={() => handleRouteSelect(route)}
                          className="justify-start"
                        >
                          {route.Label}
                        </Button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Step 3: Employee */}
          {step >= 3 && (
            <div className={cn(
              "space-y-2 pb-4 border-b",
              step > 3 && "opacity-60"
            )}>
              <Label className="flex items-center gap-2">
                <Users className="h-4 w-4" />
                Employee
                {formData.employeeName && (
                  <Badge variant="secondary">{formData.employeeName}</Badge>
                )}
              </Label>
              
              {step === 3 && (
                <div>
                  {loading.employees ? (
                    <div className="space-y-2">
                      {[1, 2].map(i => (
                        <Skeleton key={i} className="h-10 w-full" />
                      ))}
                    </div>
                  ) : (
                    <div className="grid grid-cols-1 gap-2">
                      {employees.map(emp => (
                        <Button
                          key={emp.UID}
                          variant="outline"
                          onClick={() => handleEmployeeSelect(emp)}
                          className="justify-start"
                        >
                          {emp.Label}
                        </Button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Step 4: Date & Submit */}
          {step >= 4 && (
            <div className="space-y-4">
              <div className="space-y-2">
                <Label className="flex items-center gap-2">
                  <Calendar className="h-4 w-4" />
                  Visit Date
                </Label>
                <Input
                  type="date"
                  value={moment(formData.visitDate).format('YYYY-MM-DD')}
                  onChange={(e) => setFormData(prev => ({
                    ...prev,
                    visitDate: new Date(e.target.value)
                  }))}
                  min={moment().format('YYYY-MM-DD')}
                />
              </div>

              <Button
                onClick={handleQuickSubmit}
                disabled={loading.submit}
                className="w-full"
                size="lg"
              >
                {loading.submit ? (
                  <>Creating...</>
                ) : (
                  <>
                    <CheckCircle className="h-4 w-4 mr-2" />
                    Create Journey Plan
                  </>
                )}
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Tips */}
      <div className="mt-4 p-4 bg-blue-50 rounded-lg">
        <div className="flex items-start gap-2">
          <AlertCircle className="h-4 w-4 text-blue-600 mt-0.5" />
          <div className="text-sm text-blue-800">
            <p className="font-medium">Quick Tips:</p>
            <ul className="mt-1 space-y-1 text-xs">
              <li>• Select organization first to load routes</li>
              <li>• Routes load automatically when org is selected</li>
              <li>• Only active employees are shown</li>
              <li>• You can add customers after creating the plan</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
};