"use client";

import React from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Route,
  Calendar,
  MapPin,
  Users,
  CalendarDays,
  Settings,
  TrendingUp,
  FileText,
  Package,
  Store,
  Truck,
  BarChart3,
  Map
} from "lucide-react";

interface FeatureCard {
  title: string;
  description: string;
  icon: React.ReactNode;
  path: string;
  badge?: string;
  color: string;
}

export function UpdatedFeaturesMenu() {
  const router = useRouter();

  const routeManagementFeatures: FeatureCard[] = [
    {
      title: "Manage Routes",
      description: "View and manage all route templates",
      icon: <Route className="h-5 w-5" />,
      path: "/updatedfeatures/route-management/routes/manage",
      badge: "Core",
      color: "text-blue-600 bg-blue-50 border-blue-200 hover:bg-blue-100"
    },
    {
      title: "Create Route",
      description: "Create new route with schedule and stores",
      icon: <MapPin className="h-5 w-5" />,
      path: "/updatedfeatures/route-management/routes/create",
      color: "text-green-600 bg-green-50 border-green-200 hover:bg-green-100"
    },
    {
      title: "Customer Mapping",
      description: "Assign stores to existing routes",
      icon: <Store className="h-5 w-5" />,
      path: "/updatedfeatures/route-management/routes/customer-mapping",
      badge: "Updated",
      color: "text-purple-600 bg-purple-50 border-purple-200 hover:bg-purple-100"
    }
  ];

  const journeyPlanFeatures: FeatureCard[] = [
    {
      title: "View Journey Plans",
      description: "View auto-generated journey plans",
      icon: <Calendar className="h-5 w-5" />,
      path: "/updatedfeatures/journey-plan-management/view-plans",
      badge: "Fixed",
      color: "text-indigo-600 bg-indigo-50 border-indigo-200 hover:bg-indigo-100"
    },
    {
      title: "Manage Journey Plans",
      description: "Manage and track journey plan execution",
      icon: <Users className="h-5 w-5" />,
      path: "/updatedfeatures/journey-plan-management/journey-plans/manage",
      color: "text-orange-600 bg-orange-50 border-orange-200 hover:bg-orange-100"
    },
    {
      title: "Beat History",
      description: "View historical beat execution data",
      icon: <FileText className="h-5 w-5" />,
      path: "/updatedfeatures/journey-plan-management/beat-history",
      color: "text-teal-600 bg-teal-50 border-teal-200 hover:bg-teal-100"
    },
    {
      title: "Analytics Dashboard",
      description: "Journey plan analytics and insights",
      icon: <BarChart3 className="h-5 w-5" />,
      path: "/updatedfeatures/journey-plan-management/analytics",
      color: "text-pink-600 bg-pink-50 border-pink-200 hover:bg-pink-100"
    },
    {
      title: "Live Dashboard",
      description: "Real-time journey execution tracking",
      icon: <TrendingUp className="h-5 w-5" />,
      path: "/updatedfeatures/journey-plan-management/live-dashboard",
      badge: "Live",
      color: "text-red-600 bg-red-50 border-red-200 hover:bg-red-100"
    }
  ];

  const settingsFeatures: FeatureCard[] = [
    {
      title: "Holiday Management",
      description: "Manage holidays affecting journey plans",
      icon: <CalendarDays className="h-5 w-5" />,
      path: "/updatedfeatures/settings/holiday-management",
      badge: "New",
      color: "text-cyan-600 bg-cyan-50 border-cyan-200 hover:bg-cyan-100"
    }
  ];

  const skuFeatures: FeatureCard[] = [
    {
      title: "Product Management",
      description: "Manage products and SKUs",
      icon: <Package className="h-5 w-5" />,
      path: "/updatedfeatures/sku-management/products/manage",
      color: "text-violet-600 bg-violet-50 border-violet-200 hover:bg-violet-100"
    },
    {
      title: "Create Product",
      description: "Add new products to catalog",
      icon: <Package className="h-5 w-5" />,
      path: "/updatedfeatures/sku-management/products/create",
      color: "text-amber-600 bg-amber-50 border-amber-200 hover:bg-amber-100"
    }
  ];

  const renderFeatureCard = (feature: FeatureCard) => (
    <Card
      key={feature.path}
      className={`cursor-pointer transition-all hover:shadow-lg border ${feature.color}`}
      onClick={() => router.push(feature.path)}
    >
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className={`p-2 rounded-lg ${feature.color}`}>
              {feature.icon}
            </div>
            <div>
              <CardTitle className="text-base font-semibold">
                {feature.title}
              </CardTitle>
              {feature.badge && (
                <Badge variant="secondary" className="mt-1 text-xs">
                  {feature.badge}
                </Badge>
              )}
            </div>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <CardDescription className="text-sm">
          {feature.description}
        </CardDescription>
      </CardContent>
    </Card>
  );

  return (
    <div className="space-y-8">
      {/* Route Management Section */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <Map className="h-6 w-6 text-blue-600" />
          <h2 className="text-xl font-bold text-gray-900">Route Management</h2>
          <Badge variant="outline" className="text-blue-600">
            Master Templates
          </Badge>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {routeManagementFeatures.map(renderFeatureCard)}
        </div>
      </div>

      {/* Journey Plan Management Section */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <Truck className="h-6 w-6 text-indigo-600" />
          <h2 className="text-xl font-bold text-gray-900">Journey Plan Management</h2>
          <Badge variant="outline" className="text-indigo-600">
            Auto-Generated
          </Badge>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {journeyPlanFeatures.map(renderFeatureCard)}
        </div>
      </div>

      {/* Settings Section */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <Settings className="h-6 w-6 text-cyan-600" />
          <h2 className="text-xl font-bold text-gray-900">Settings & Configuration</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {settingsFeatures.map(renderFeatureCard)}
        </div>
      </div>

      {/* SKU Management Section */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <Package className="h-6 w-6 text-violet-600" />
          <h2 className="text-xl font-bold text-gray-900">SKU Management</h2>
        </div>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {skuFeatures.map(renderFeatureCard)}
        </div>
      </div>
    </div>
  );
}