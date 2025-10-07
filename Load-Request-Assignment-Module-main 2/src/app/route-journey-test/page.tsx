"use client";

import React, { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { 
  Route, 
  Calendar, 
  Clock, 
  CheckCircle, 
  AlertCircle, 
  Info,
  ArrowRight,
  RefreshCw
} from "lucide-react";
import { routeServiceFixed } from "@/services/routeService.fixed";
import { journeyPlanServiceFixed } from "@/services/journeyPlanService.fixed";
import { authService } from "@/lib/auth-service";
import moment from "moment";

export default function RouteJourneyTestPage() {
  const [routes, setRoutes] = useState<any[]>([]);
  const [journeyPlans, setJourneyPlans] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedDate, setSelectedDate] = useState(moment().format("YYYY-MM-DD"));
  const currentUser = authService.getCurrentUser();

  const loadRoutes = async () => {
    setLoading(true);
    try {
      const response = await routeServiceFixed.getRoutes({
        orgUID: currentUser?.orgUID || "",
        pageNumber: 1,
        pageSize: 10,
        isCountRequired: true
      });
      
      const routeData = response?.data?.pagedData || response?.pagedData || [];
      setRoutes(Array.isArray(routeData) ? routeData : []);
    } catch (error) {
      console.error("Error loading routes:", error);
      setRoutes([]);
    } finally {
      setLoading(false);
    }
  };

  const loadJourneyPlans = async () => {
    setLoading(true);
    try {
      const response = await journeyPlanServiceFixed.getTodayJourneyPlanDetails({
        visitDate: selectedDate,
        type: "Assigned",
        orgUID: currentUser?.orgUID || "",
        pageNumber: 1,
        pageSize: 100
      });
      
      const planData = response?.pagedData || [];
      setJourneyPlans(Array.isArray(planData) ? planData : []);
    } catch (error) {
      console.error("Error loading journey plans:", error);
      setJourneyPlans([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRoutes();
    loadJourneyPlans();
  }, [selectedDate]);

  const formatBeatHistoryUID = (routeUID: string, date: string) => {
    const dateParts = date.split('-');
    return `${routeUID}_${dateParts[0]}_${dateParts[1]}_${dateParts[2]}`;
  };

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-3xl font-bold mb-6">Route to Journey Plan Flow Test</h1>
      
      {/* Flow Explanation */}
      <Alert className="mb-6">
        <Info className="h-4 w-4" />
        <AlertTitle>How It Works</AlertTitle>
        <AlertDescription>
          <div className="mt-2 space-y-2">
            <div className="flex items-center gap-2">
              <Route className="h-4 w-4" />
              <span>1. Create Route (Master Template)</span>
            </div>
            <div className="flex items-center gap-2 ml-6">
              <ArrowRight className="h-4 w-4" />
              <Clock className="h-4 w-4" />
              <span>2. System auto-generates at 11 PM daily</span>
            </div>
            <div className="flex items-center gap-2 ml-6">
              <ArrowRight className="h-4 w-4" />
              <Calendar className="h-4 w-4" />
              <span>3. Journey Plans appear (Beat History)</span>
            </div>
          </div>
        </AlertDescription>
      </Alert>

      {/* Date Selector */}
      <Card className="mb-6">
        <CardHeader>
          <CardTitle>Select Date</CardTitle>
          <CardDescription>Choose a date to check journey plans</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex gap-4 items-center">
            <input
              type="date"
              value={selectedDate}
              onChange={(e) => setSelectedDate(e.target.value)}
              className="px-3 py-2 border rounded-md"
            />
            <Button onClick={() => { loadRoutes(); loadJourneyPlans(); }}>
              <RefreshCw className="h-4 w-4 mr-2" />
              Refresh
            </Button>
            <span className="text-sm text-gray-600">
              Journey plans are generated for: {moment(selectedDate).format("MMMM DD, YYYY")}
            </span>
          </div>
        </CardContent>
      </Card>

      <Tabs defaultValue="routes" className="space-y-4">
        <TabsList>
          <TabsTrigger value="routes">Your Routes ({routes.length})</TabsTrigger>
          <TabsTrigger value="journey">Journey Plans ({journeyPlans.length})</TabsTrigger>
          <TabsTrigger value="mapping">Route → Journey Mapping</TabsTrigger>
        </TabsList>

        <TabsContent value="routes">
          <Card>
            <CardHeader>
              <CardTitle>Active Routes</CardTitle>
              <CardDescription>These are your route templates</CardDescription>
            </CardHeader>
            <CardContent>
              {loading ? (
                <div>Loading routes...</div>
              ) : routes.length === 0 ? (
                <Alert>
                  <AlertCircle className="h-4 w-4" />
                  <AlertTitle>No Routes Found</AlertTitle>
                  <AlertDescription>
                    Create a route first at: /updatedfeatures/route-management/routes/create
                  </AlertDescription>
                </Alert>
              ) : (
                <div className="space-y-4">
                  {routes.map((route) => (
                    <div key={route.UID || route.uid} className="p-4 border rounded-lg">
                      <div className="flex justify-between items-start">
                        <div>
                          <h3 className="font-semibold">{route.Name || route.name}</h3>
                          <p className="text-sm text-gray-600">Code: {route.Code || route.code}</p>
                          <p className="text-sm text-gray-600">UID: {route.UID || route.uid}</p>
                          <div className="mt-2 flex gap-4 text-sm">
                            <span>Valid: {moment(route.ValidFrom || route.validFrom).format("MM/DD/YYYY")} - {moment(route.ValidUpto || route.validUpto).format("MM/DD/YYYY")}</span>
                            <span>Customers: {route.TotalCustomers || route.totalCustomers || 0}</span>
                            <span>Status: {route.Status || route.status}</span>
                          </div>
                        </div>
                        {(route.IsActive || route.isActive) && (
                          <CheckCircle className="h-5 w-5 text-green-500" />
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="journey">
          <Card>
            <CardHeader>
              <CardTitle>Journey Plans for {moment(selectedDate).format("MMMM DD, YYYY")}</CardTitle>
              <CardDescription>Auto-generated from active routes</CardDescription>
            </CardHeader>
            <CardContent>
              {loading ? (
                <div>Loading journey plans...</div>
              ) : journeyPlans.length === 0 ? (
                <Alert>
                  <Info className="h-4 w-4" />
                  <AlertTitle>No Journey Plans for This Date</AlertTitle>
                  <AlertDescription>
                    <ul className="mt-2 list-disc list-inside space-y-1">
                      <li>Journey plans are auto-generated at 11 PM daily</li>
                      <li>Check if route is active and valid for this date</li>
                      <li>Ensure route has assigned customers and salesman</li>
                      <li>Verify this date is not a holiday</li>
                    </ul>
                  </AlertDescription>
                </Alert>
              ) : (
                <div className="space-y-4">
                  {journeyPlans.map((plan) => (
                    <div key={plan.beatHistoryUID} className="p-4 border rounded-lg">
                      <div className="flex justify-between items-start">
                        <div>
                          <h3 className="font-semibold">{plan.routeName}</h3>
                          <p className="text-sm text-gray-600">Beat History UID: {plan.beatHistoryUID}</p>
                          <p className="text-sm text-gray-600">Salesman: {plan.salesmanName || "Unassigned"}</p>
                          <div className="mt-2 flex gap-4 text-sm">
                            <span>Scheduled: {plan.scheduleCall} stores</span>
                            <span>Visited: {plan.actualStoreVisits}</span>
                            <span>Pending: {plan.pendingVisits}</span>
                          </div>
                        </div>
                        <div className="text-right">
                          <span className="text-sm text-gray-600">
                            {moment(plan.visitDate).format("MM/DD/YYYY")}
                          </span>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="mapping">
          <Card>
            <CardHeader>
              <CardTitle>Route to Journey Plan Mapping</CardTitle>
              <CardDescription>Shows how routes become journey plans</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {routes.map((route) => {
                  const routeUID = route.UID || route.uid;
                  const expectedBeatUID = formatBeatHistoryUID(routeUID, selectedDate);
                  const matchingPlan = journeyPlans.find(p => 
                    p.beatHistoryUID === expectedBeatUID || 
                    p.routeUID === routeUID
                  );

                  return (
                    <div key={routeUID} className="p-4 border rounded-lg">
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <h4 className="font-semibold mb-2">Route Template</h4>
                          <p className="text-sm">Name: {route.Name || route.name}</p>
                          <p className="text-sm">Code: {route.Code || route.code}</p>
                          <p className="text-sm">UID: {routeUID}</p>
                        </div>
                        <div>
                          <h4 className="font-semibold mb-2">Journey Plan</h4>
                          {matchingPlan ? (
                            <>
                              <p className="text-sm text-green-600">✓ Generated</p>
                              <p className="text-sm">Beat UID: {matchingPlan.beatHistoryUID}</p>
                              <p className="text-sm">Status: Active</p>
                            </>
                          ) : (
                            <>
                              <p className="text-sm text-gray-500">Not generated</p>
                              <p className="text-sm text-gray-400">Expected: {expectedBeatUID}</p>
                              <p className="text-sm text-gray-400">Will generate at 11 PM</p>
                            </>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
                
                {routes.length === 0 && (
                  <Alert>
                    <Info className="h-4 w-4" />
                    <AlertDescription>
                      Create routes first to see the mapping
                    </AlertDescription>
                  </Alert>
                )}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Generation Status */}
      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Generation Status</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4" />
              <span>Next generation: Today at 11:00 PM</span>
            </div>
            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              <span>Generates plans for: Next 15 days</span>
            </div>
            <div className="flex items-center gap-2">
              <Info className="h-4 w-4" />
              <span>Beat History UID Format: {"{route_uid}"}_{"{YYYY}"}_{"{MM}"}_{"{DD}"}</span>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}