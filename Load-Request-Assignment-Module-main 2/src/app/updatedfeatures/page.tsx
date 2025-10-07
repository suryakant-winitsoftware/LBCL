"use client";

import { UpdatedFeaturesMenu } from "@/components/navigation/UpdatedFeaturesMenu";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft } from "lucide-react";
import { useRouter } from "next/navigation";

export default function UpdatedFeaturesPage() {
  const router = useRouter();

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <div className="container mx-auto p-8">
        {/* Header */}
        <div className="mb-8">
          <Button
            variant="ghost"
            onClick={() => router.push("/dashboard")}
            className="mb-4"
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Dashboard
          </Button>
          
          <h1 className="text-4xl font-bold text-gray-900 dark:text-gray-100 mb-2">
            Updated Features Hub
          </h1>
          <p className="text-lg text-gray-600 dark:text-gray-400">
            Access all the newly updated and enhanced features of the WINIT system
          </p>
        </div>

        {/* Info Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <Card className="bg-blue-50 border-blue-200 dark:bg-blue-950 dark:border-blue-800">
            <CardHeader>
              <CardTitle className="text-blue-900 dark:text-blue-100">
                Route Management
              </CardTitle>
              <CardDescription className="text-blue-700 dark:text-blue-300">
                Create and manage master route templates with store assignments
              </CardDescription>
            </CardHeader>
          </Card>

          <Card className="bg-indigo-50 border-indigo-200 dark:bg-indigo-950 dark:border-indigo-800">
            <CardHeader>
              <CardTitle className="text-indigo-900 dark:text-indigo-100">
                Journey Plans
              </CardTitle>
              <CardDescription className="text-indigo-700 dark:text-indigo-300">
                View auto-generated journey plans (no manual creation)
              </CardDescription>
            </CardHeader>
          </Card>

          <Card className="bg-green-50 border-green-200 dark:bg-green-950 dark:border-green-800">
            <CardHeader>
              <CardTitle className="text-green-900 dark:text-green-100">
                Real-time Data
              </CardTitle>
              <CardDescription className="text-green-700 dark:text-green-300">
                All features now use live API data with no static content
              </CardDescription>
            </CardHeader>
          </Card>
        </div>

        {/* Features Menu */}
        <UpdatedFeaturesMenu />
      </div>
    </div>
  );
}