"use client";

import { useRouter } from "next/navigation";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

export default function TestRoutesPage() {
  const router = useRouter();

  const routes = [
    {
      title: "Route Management",
      items: [
        { name: "Manage Routes", path: "/updatedfeatures/route-management/routes/manage" },
        { name: "Create Route", path: "/updatedfeatures/route-management/routes/create" },
        { name: "Customer Mapping", path: "/updatedfeatures/route-management/routes/customer-mapping" },
      ]
    },
    {
      title: "Journey Plan Management",
      items: [
        { name: "View Plans", path: "/updatedfeatures/journey-plan-management/view-plans" },
        { name: "Manage Plans", path: "/updatedfeatures/journey-plan-management/journey-plans/manage" },
        { name: "Beat History", path: "/updatedfeatures/journey-plan-management/beat-history" },
        { name: "Analytics", path: "/updatedfeatures/journey-plan-management/analytics" },
        { name: "Live Dashboard", path: "/updatedfeatures/journey-plan-management/live-dashboard" },
      ]
    },
    {
      title: "Settings",
      items: [
        { name: "Holiday Management", path: "/updatedfeatures/settings/holiday-management" },
      ]
    },
    {
      title: "Direct Test Links",
      items: [
        { name: "Dashboard", path: "/dashboard" },
        { name: "Updated Features Hub", path: "/updatedfeatures" },
      ]
    }
  ];

  return (
    <div className="container mx-auto p-8">
      <h1 className="text-3xl font-bold mb-8">Test Routes - Direct Navigation</h1>
      
      <div className="mb-4 p-4 bg-blue-50 border border-blue-200 rounded-lg">
        <p className="text-sm text-blue-800">
          Click any button below to navigate directly to that page. 
          If a page doesn't load, check the browser console for errors.
        </p>
      </div>

      <div className="space-y-6">
        {routes.map((section) => (
          <Card key={section.title}>
            <CardHeader>
              <CardTitle>{section.title}</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                {section.items.map((item) => (
                  <div key={item.path} className="flex flex-col gap-2">
                    <Button
                      variant="outline"
                      className="w-full justify-start"
                      onClick={() => {
                        console.log(`Navigating to: ${item.path}`);
                        router.push(item.path);
                      }}
                    >
                      {item.name}
                    </Button>
                    <code className="text-xs text-gray-500 px-2">{item.path}</code>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card className="mt-6">
        <CardHeader>
          <CardTitle>Test Direct URL Access</CardTitle>
          <CardDescription>
            You can also type these URLs directly in your browser address bar
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-2">
            <div className="p-3 bg-gray-50 rounded-md">
              <p className="text-sm font-mono">
                http://localhost:3000/updatedfeatures/route-management/routes/create
              </p>
            </div>
            <div className="p-3 bg-gray-50 rounded-md">
              <p className="text-sm font-mono">
                http://localhost:3000/updatedfeatures/journey-plan-management/view-plans
              </p>
            </div>
            <div className="p-3 bg-gray-50 rounded-md">
              <p className="text-sm font-mono">
                http://localhost:3000/updatedfeatures/settings/holiday-management
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}