"use client";

import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  CheckCircle,
  XCircle,
  Clock,
  Database,
  Server,
  Search
} from "lucide-react";

interface ApiTestResult {
  endpoint: string;
  method: string;
  status: number;
  success: boolean;
  data?: any;
  error?: string;
  responseTime: number;
}

export default function ApiInvestigationPage() {
  const [isRunning, setIsRunning] = useState(false);
  const [results, setResults] = useState<ApiTestResult[]>([]);
  const [summary, setSummary] = useState<any>(null);
  const [dataStructures, setDataStructures] = useState<any[]>([]);

  const runInvestigation = async () => {
    setIsRunning(true);
    setResults([]);
    setSummary(null);
    setDataStructures([]);

    try {
      const API_BASE_URL = "http://localhost:8000/api";
      const AUTH_TOKEN =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiQURNSU4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3VzZXJkYXRhIjoiYWRtaW4iLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJBZG1pbiIsInBlcm1pc3Npb25zIjpbInJlYWQiLCJ3cml0ZSJdLCJleHAiOjE3NTQ1NjM1ODEsImlzcyI6Im15aXNzdWVyIn0.QXc2NiYt64mRNNYJytLrWkTiX20KgFdSDjl2kLsuR1A";

      const headers = {
        Authorization: `Bearer ${AUTH_TOKEN}`,
        "Content-Type": "application/json",
        Accept: "application/json"
      };

      const defaultPagingRequest = {
        PageNumber: 1,
        PageSize: 10,
        IsCountRequired: true,
        FilterCriterias: [],
        SortCriterias: []
      };

      const testEndpoint = async (
        endpoint: string,
        method: "GET" | "POST",
        body?: any
      ) => {
        const startTime = Date.now();

        try {
          const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method,
            headers,
            body: body ? JSON.stringify(body) : undefined
          });

          const responseTime = Date.now() - startTime;
          const data = await response.json();

          return {
            endpoint,
            method,
            status: response.status,
            success: response.ok,
            data,
            responseTime
          };
        } catch (error) {
          const responseTime = Date.now() - startTime;
          return {
            endpoint,
            method,
            status: 0,
            success: false,
            error: error instanceof Error ? error.message : "Unknown error",
            responseTime
          };
        }
      };

      // Test all endpoints
      const endpoints = [
        {
          path: "/SKUGroupType/SelectAllSKUGroupTypeDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKUGroupType/SelectSKUGroupTypeView",
          method: "GET" as const
        },
        { path: "/SKUGroupType/SelectSKUAttributeDDL", method: "GET" as const },
        {
          path: "/SKUGroup/SelectAllSKUGroupDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        { path: "/SKUGroup/SelectSKUGroupView", method: "GET" as const },
        {
          path: "/SKUGroup/SelectAllSKUGroupItemViews",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKUToGroupMapping/SelectAllSKUToGroupMappingDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKU/SelectAllSKUDetailsWebView",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKUAttributes/SelectAllSKUAttributesDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKUUOM/SelectAllSKUUOMDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        },
        {
          path: "/SKUConfig/SelectAllSKUConfigDetails",
          method: "POST" as const,
          body: defaultPagingRequest
        }
      ];

      const testResults: ApiTestResult[] = [];
      const structures: any[] = [];

      for (const endpoint of endpoints) {
        console.log(`Testing ${endpoint.path}...`);
        const result = await testEndpoint(
          endpoint.path,
          endpoint.method,
          endpoint.body
        );
        testResults.push(result);
        setResults([...testResults]);

        // Extract data structure if successful
        if (result.success && result.data) {
          let sampleData = null;
          if (result.data.Data?.PagedData?.[0]) {
            sampleData = result.data.Data.PagedData[0];
          } else if (Array.isArray(result.data.Data) && result.data.Data[0]) {
            sampleData = result.data.Data[0];
          } else if (result.data.Data) {
            sampleData = result.data.Data;
          }

          if (sampleData) {
            structures.push({
              endpoint: result.endpoint,
              structure: sampleData,
              count:
                result.data.Data?.PagedData?.length ||
                (Array.isArray(result.data.Data) ? result.data.Data.length : 1),
              totalCount: result.data.Data?.TotalCount || null
            });
          }
        }

        // Small delay to avoid overwhelming the server
        await new Promise((resolve) => setTimeout(resolve, 100));
      }

      // Generate summary
      const successful = testResults.filter((r) => r.success);
      const failed = testResults.filter((r) => !r.success);

      setSummary({
        total: testResults.length,
        successful: successful.length,
        failed: failed.length,
        totalTime: testResults.reduce((sum, r) => sum + r.responseTime, 0),
        averageTime:
          testResults.reduce((sum, r) => sum + r.responseTime, 0) /
          testResults.length
      });

      setDataStructures(structures);
    } catch (error) {
      console.error("Investigation failed:", error);
    } finally {
      setIsRunning(false);
    }
  };

  return (
    <div className="container mx-auto py-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Deep API Investigation
          </h1>
          <p className="text-muted-foreground">
            Comprehensive testing of all SKU-related APIs and database
            structures
          </p>
        </div>
        <Button onClick={runInvestigation} disabled={isRunning} size="lg">
          {isRunning ? (
            <>
              <Clock className="h-4 w-4 mr-2 animate-spin" />
              Running Investigation...
            </>
          ) : (
            <>
              <Search className="h-4 w-4 mr-2" />
              Start Investigation
            </>
          )}
        </Button>
      </div>

      {summary && (
        <Alert>
          <Database className="h-4 w-4" />
          <AlertDescription>
            Investigation completed: {summary.successful}/{summary.total} APIs
            successful ({summary.failed} failed) in {summary.totalTime}ms (avg:{" "}
            {Math.round(summary.averageTime)}ms per request)
          </AlertDescription>
        </Alert>
      )}

      <Tabs defaultValue="results" className="w-full">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="results">API Test Results</TabsTrigger>
          <TabsTrigger value="structures">Data Structures</TabsTrigger>
          <TabsTrigger value="analysis">Analysis</TabsTrigger>
        </TabsList>

        <TabsContent value="results" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Server className="h-5 w-5" />
                API Endpoint Results
              </CardTitle>
              <CardDescription>
                Real-time results from testing all SKU-related API endpoints
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ScrollArea className="h-[600px]">
                <div className="space-y-3">
                  {results.map((result, index) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-2">
                        <div className="flex items-center gap-2">
                          {result.success ? (
                            <CheckCircle className="h-4 w-4 text-green-500" />
                          ) : (
                            <XCircle className="h-4 w-4 text-red-500" />
                          )}
                          <Badge
                            variant={
                              result.method === "GET" ? "secondary" : "default"
                            }
                          >
                            {result.method}
                          </Badge>
                          <code className="text-sm">{result.endpoint}</code>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge
                            variant={result.success ? "default" : "destructive"}
                          >
                            {result.status}
                          </Badge>
                          <span className="text-sm text-muted-foreground">
                            {result.responseTime}ms
                          </span>
                        </div>
                      </div>

                      {result.error && (
                        <p className="text-sm text-red-500 mt-2">
                          {result.error}
                        </p>
                      )}

                      {result.success && result.data && (
                        <div className="mt-2">
                          <p className="text-sm text-muted-foreground">
                            {result.data.Data?.PagedData
                              ? `${result.data.Data.PagedData.length} items (Total: ${result.data.Data.TotalCount})`
                              : Array.isArray(result.data.Data)
                              ? `${result.data.Data.length} items`
                              : "Data received"}
                          </p>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="structures" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Database className="h-5 w-5" />
                Database Structure Analysis
              </CardTitle>
              <CardDescription>
                Sample data structures from each successful API response
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ScrollArea className="h-[600px]">
                <div className="space-y-6">
                  {dataStructures.map((structure, index) => (
                    <div key={index} className="border rounded-lg p-4">
                      <div className="flex items-center justify-between mb-3">
                        <h3 className="font-semibold">{structure.endpoint}</h3>
                        <div className="flex gap-2">
                          <Badge variant="outline">
                            {structure.count} items
                          </Badge>
                          {structure.totalCount && (
                            <Badge variant="outline">
                              Total: {structure.totalCount}
                            </Badge>
                          )}
                        </div>
                      </div>

                      <ScrollArea className="h-[200px]">
                        <pre className="text-xs bg-muted p-3 rounded overflow-x-auto">
                          {JSON.stringify(structure.structure, null, 2)}
                        </pre>
                      </ScrollArea>
                    </div>
                  ))}
                </div>
              </ScrollArea>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="analysis" className="space-y-4">
          <div className="grid gap-4">
            <Card>
              <CardHeader>
                <CardTitle>Database Schema Analysis</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div>
                  <h4 className="font-semibold mb-2">
                    Core Tables Identified:
                  </h4>
                  <ul className="space-y-2 text-sm">
                    <li>
                      • <strong>sku_group_type</strong> - Hierarchy type
                      definitions (Category, Brand, etc.)
                    </li>
                    <li>
                      • <strong>sku_group</strong> - Actual group instances
                      within each type
                    </li>
                    <li>
                      • <strong>sku</strong> - Main product/SKU table
                    </li>
                    <li>
                      • <strong>sku_attributes</strong> - Additional SKU
                      properties
                    </li>
                    <li>
                      • <strong>sku_uom</strong> - Unit of measure
                      configurations
                    </li>
                    <li>
                      • <strong>sku_config</strong> - Organization-specific SKU
                      settings
                    </li>
                    <li>
                      • <strong>sku_to_group_mapping</strong> - Many-to-many
                      SKU-to-group relationships
                    </li>
                  </ul>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>API Performance Analysis</CardTitle>
              </CardHeader>
              <CardContent>
                {summary && (
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                    <div className="text-center">
                      <div className="text-2xl font-bold text-green-600">
                        {summary.successful}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Successful
                      </div>
                    </div>
                    <div className="text-center">
                      <div className="text-2xl font-bold text-red-600">
                        {summary.failed}
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Failed
                      </div>
                    </div>
                    <div className="text-center">
                      <div className="text-2xl font-bold">
                        {summary.totalTime}ms
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Total Time
                      </div>
                    </div>
                    <div className="text-center">
                      <div className="text-2xl font-bold">
                        {Math.round(summary.averageTime)}ms
                      </div>
                      <div className="text-sm text-muted-foreground">
                        Avg Response
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
