"use client";

import { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/app/lbcl/components/ui/tabs";
import { SalesItineraryDashboard } from "./sales-itinerary-dashboard";
import { SalesItineraryDataDashboard } from "./sales-itinerary-data-dashboard";
import { SalesItineraryCalendar } from "./sales-itinerary-calendar";
import { SalesItineraryConfiguration } from "./sales-itinerary-configuration";
import { SalesItineraryExpenses } from "./sales-itinerary-expenses";
import { SalesItineraryTemplate } from "./sales-itinerary-template";

export function SalesItineraryPlanning() {
  const [activeTab, setActiveTab] = useState("dashboard");

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Tabs Navigation */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <div className="bg-white shadow-sm sticky top-0 z-10">
          <div className="w-full">
            <TabsList className="w-full justify-start md:justify-center h-auto p-0 bg-transparent border-b border-gray-200">
              <TabsTrigger
                value="dashboard"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">📊</span>
                <span>DASHBOARD</span>
              </TabsTrigger>
              <TabsTrigger
                value="calendar"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">📅</span>
                <span>ITINERARY</span>
              </TabsTrigger>
              <TabsTrigger
                value="expenses"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">💰</span>
                <span>EXPENSES</span>
              </TabsTrigger>
              <TabsTrigger
                value="template"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">📋</span>
                <span>TEMPLATE</span>
              </TabsTrigger>
              <TabsTrigger
                value="data"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">📈</span>
                <span>DATA ANALYSIS</span>
              </TabsTrigger>
              <TabsTrigger
                value="configuration"
                className="flex items-center gap-1.5 px-4 md:px-5 py-2.5 rounded-none border-b-3 border-transparent data-[state=active]:border-[#A08B5C] data-[state=active]:bg-[#A08B5C]/5 data-[state=active]:text-[#A08B5C] hover:bg-gray-50 transition-all duration-200 font-semibold text-xs md:text-sm"
              >
                <span className="text-sm md:text-base">⚙️</span>
                <span>CONFIG</span>
              </TabsTrigger>
            </TabsList>
          </div>
        </div>

        {/* Tab Content */}
        <div className="w-full px-3 md:px-6 lg:px-8 py-4 max-w-[1600px] mx-auto">
          <TabsContent value="dashboard" className="mt-0">
            <SalesItineraryDashboard />
          </TabsContent>
          <TabsContent value="calendar" className="mt-0">
            <SalesItineraryCalendar />
          </TabsContent>
          <TabsContent value="expenses" className="mt-0">
            <SalesItineraryExpenses />
          </TabsContent>
          <TabsContent value="template" className="mt-0">
            <SalesItineraryTemplate />
          </TabsContent>
          <TabsContent value="data" className="mt-0">
            <SalesItineraryDataDashboard />
          </TabsContent>
          <TabsContent value="configuration" className="mt-0">
            <SalesItineraryConfiguration />
          </TabsContent>
        </div>
      </Tabs>
    </div>
  );
}
