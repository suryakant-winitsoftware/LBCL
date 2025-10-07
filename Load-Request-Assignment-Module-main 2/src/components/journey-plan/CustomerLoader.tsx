"use client";

import React, { useState, useEffect, useRef, useCallback, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Users, Search } from "lucide-react";
import { api, apiService } from '@/services/api';

// Types
interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address: string;
  ContactNo: string;
  Type: string;
  Status: string;
  SeqNo: number;
}

interface CustomerWithTime {
  UID: string;
  Code: string;
  Name: string;
  Address: string;
  ContactNo: string;
  Type: string;
  Status: string;
  SeqNo: number;
  startTime: string;
  endTime: string;
  visitDuration: number;
}

interface CustomerLoaderProps {
  routeUID: string;
  orgUID: string;
  defaultDuration?: number;
  defaultTravelTime?: number;
  dayStartsAt?: string;
  onCustomersChange: (customers: CustomerWithTime[]) => void;
  selectedCustomers?: CustomerWithTime[];
  multiDayMode?: boolean;
  selectedDay?: number;
  dayWiseCustomers?: Record<number, CustomerWithTime[]>;
  onDayWiseCustomersChange?: (dayWiseCustomers: Record<number, CustomerWithTime[]>) => void;
}

export default function CustomerLoader({
  routeUID,
  orgUID,
  defaultDuration = 30,
  defaultTravelTime = 30,
  dayStartsAt = "00:00",
  onCustomersChange,
  selectedCustomers = [],
  multiDayMode = false,
  selectedDay = 1,
  dayWiseCustomers = {},
  onDayWiseCustomersChange
}: CustomerLoaderProps) {
  
  // State management
  const [routeCustomers, setRouteCustomers] = useState<Customer[]>([]);
  const [allRouteCustomers, setAllRouteCustomers] = useState<Customer[]>([]);
  const [customerPagination, setCustomerPagination] = useState({
    currentPage: 1,
    pageSize: 1000,
    totalCount: 0,
    hasMore: false
  });
  const [customerSearchTerm, setCustomerSearchTerm] = useState("");
  const [loadingMoreCustomers, setLoadingMoreCustomers] = useState(false);
  const [isLoadingCustomers, setIsLoadingCustomers] = useState(false);
  const [showCustomerDialog, setShowCustomerDialog] = useState(false);
  const [selectedCustomersForDay, setSelectedCustomersForDay] = useState<string[]>([]);
  const [customerSearchInDialog, setCustomerSearchInDialog] = useState("");

  // Refs for performance optimization
  const isLoadingCustomersRef = useRef(false);
  const apiCallsInProgress = useRef<Set<string>>(new Set());

  // Load route customers
  const loadRouteCustomers = useCallback(async (
    routeUID: string,
    loadMore: boolean = false
  ) => {
    if (isLoadingCustomersRef.current && !loadMore) {
      console.log("Already loading customers, skipping duplicate initial load");
      return;
    }

    const loadingKey = `customers-${routeUID}-${loadMore ? "more" : "initial"}`;
    if (apiCallsInProgress.current.has(loadingKey)) {
      console.log("Already loading customers with same key, skipping duplicate call");
      return;
    }

    if (!routeUID || !routeUID.trim()) {
      console.log("No route UID provided, skipping customer load");
      return;
    }

    if (loadMore) {
      setLoadingMoreCustomers(true);
    } else {
      setIsLoadingCustomers(true);
      isLoadingCustomersRef.current = true;
    }

    apiCallsInProgress.current.add(loadingKey);

    try {
      console.log("Loading customers for route:", routeUID, loadMore ? "(load more)" : "(initial)");

      const currentPage = loadMore ? customerPagination.currentPage + 1 : 1;

      const response = await apiService.post(
        `/Dropdown/GetCustomersByRouteUIDDropDown?routeUID=${routeUID}`,
        {}
      );

      if (response && (response as any).IsSuccess && Array.isArray((response as any).Data)) {
        const customers = (response as any).Data;

        if (loadMore) {
          setRouteCustomers(prev => [...prev, ...customers]);
          setAllRouteCustomers(prev => [...prev, ...customers]);
        } else {
          setRouteCustomers(customers);
          setAllRouteCustomers(customers);
        }

        setCustomerPagination({
          currentPage: currentPage,
          pageSize: customerPagination.pageSize,
          totalCount: customers.length,
          hasMore: customers.length === customerPagination.pageSize
        });

        console.log(`Loaded ${customers.length} customers for route ${routeUID}`);
      } else {
        console.error("Invalid response format for customers:", response);
        if (!loadMore) {
          setRouteCustomers([]);
          setAllRouteCustomers([]);
        }
      }
    } catch (error) {
      console.error("Error loading route customers:", error);

      if (!loadMore) {
        setRouteCustomers([]);
        setAllRouteCustomers([]);
      }
    } finally {
      const loadingKey = `customers-${routeUID}-${loadMore ? "more" : "initial"}`;
      apiCallsInProgress.current.delete(loadingKey);

      if (!loadMore) {
        setIsLoadingCustomers(false);
        isLoadingCustomersRef.current = false;
      } else {
        setLoadingMoreCustomers(false);
      }
    }
  }, [customerPagination.pageSize, customerPagination.currentPage]);

  // Calculate customer times
  const calculateCustomerTimes = useCallback(() => {
    let currentTime = dayStartsAt;
    const customersWithTimes: CustomerWithTime[] = [];

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

      customersWithTimes.push({
        UID: customer.UID,
        Code: customer.Code,
        Name: customer.Name,
        Address: customer.Address,
        ContactNo: customer.ContactNo,
        Type: customer.Type,
        Status: customer.Status,
        SeqNo: customer.SeqNo,
        startTime,
        endTime,
        visitDuration: defaultDuration
      });

      // Calculate next start time (current end time + travel time)
      const nextStartMinutes = endTimeMinutes + defaultTravelTime;
      const nextStartHours = Math.floor(nextStartMinutes / 60);
      const nextStartMins = nextStartMinutes % 60;
      currentTime = `${nextStartHours.toString().padStart(2, "0")}:${nextStartMins
        .toString()
        .padStart(2, "0")}`;
    });

    return customersWithTimes;
  }, [routeCustomers, dayStartsAt, defaultDuration, defaultTravelTime]);

  // Filtered customers for search
  const filteredCustomers = useMemo(() => {
    if (!customerSearchTerm.trim()) {
      return routeCustomers;
    }

    const searchLower = customerSearchTerm.toLowerCase();
    return routeCustomers.filter(customer =>
      customer.Name?.toLowerCase().includes(searchLower) ||
      customer.Code?.toLowerCase().includes(searchLower) ||
      customer.Address?.toLowerCase().includes(searchLower)
    );
  }, [routeCustomers, customerSearchTerm]);

  // Load more customers
  const loadMoreCustomers = useCallback(() => {
    if (customerPagination.hasMore && !loadingMoreCustomers && routeUID) {
      loadRouteCustomers(routeUID, true);
    }
  }, [customerPagination.hasMore, loadingMoreCustomers, routeUID, loadRouteCustomers]);

  // Effect to load customers when routeUID changes
  useEffect(() => {
    if (routeUID && routeUID.trim()) {
      setRouteCustomers([]);
      setAllRouteCustomers([]);
      setCustomerPagination({
        currentPage: 1,
        pageSize: 1000,
        totalCount: 0,
        hasMore: false
      });
      loadRouteCustomers(routeUID);
    }
  }, [routeUID, loadRouteCustomers]);

  // Effect to update parent when customers change
  useEffect(() => {
    const customersWithTimes = calculateCustomerTimes();
    onCustomersChange(customersWithTimes);
  }, [calculateCustomerTimes, onCustomersChange]);

  // Cleanup effect
  useEffect(() => {
    return () => {
      apiCallsInProgress.current.clear();
      isLoadingCustomersRef.current = false;
    };
  }, []);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Users className="h-5 w-5" />
          Customer Selection
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Search */}
        <div className="relative">
          <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
          <Input
            placeholder="Search customers..."
            value={customerSearchTerm}
            onChange={(e) => setCustomerSearchTerm(e.target.value)}
            className="pl-10"
          />
        </div>

        {/* Customer List */}
        <div className="space-y-2 max-h-60 overflow-y-auto">
          {isLoadingCustomers ? (
            <div className="text-center py-4">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
              <p className="text-sm text-muted-foreground mt-2">Loading customers...</p>
            </div>
          ) : filteredCustomers.length > 0 ? (
            filteredCustomers.map((customer) => (
              <div
                key={customer.UID}
                className="flex items-center justify-between p-3 border rounded-lg hover:bg-gray-50"
              >
                <div>
                  <p className="font-medium">{customer.Name}</p>
                  <p className="text-sm text-gray-600">{customer.Code}</p>
                  <p className="text-xs text-gray-500">{customer.Address}</p>
                </div>
                <Badge variant="outline">{customer.Type}</Badge>
              </div>
            ))
          ) : routeUID ? (
            <div className="text-center py-4">
              <p className="text-sm text-muted-foreground">
                {customerSearchTerm ? "No customers found matching your search." : "No customers found for this route."}
              </p>
            </div>
          ) : (
            <div className="text-center py-4">
              <p className="text-sm text-muted-foreground">Select a route to load customers.</p>
            </div>
          )}

          {/* Load More Button */}
          {customerPagination.hasMore && !isLoadingCustomers && (
            <Button
              variant="outline"
              onClick={loadMoreCustomers}
              disabled={loadingMoreCustomers}
              className="w-full"
            >
              {loadingMoreCustomers ? "Loading..." : "Load More Customers"}
            </Button>
          )}
        </div>

        {/* Customer Summary */}
        {routeCustomers.length > 0 && (
          <div className="pt-4 border-t">
            <p className="text-sm text-muted-foreground">
              Total Customers: {routeCustomers.length}
            </p>
            {selectedCustomers.length > 0 && (
              <p className="text-sm text-primary">
                Selected: {selectedCustomers.length}
              </p>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
}