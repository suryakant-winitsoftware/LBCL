"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import {
  ArrowLeft,
  Calendar,
  DollarSign,
  MapPin,
  Package,
  Users,
  FileText,
  Image,
  Mail,
} from "lucide-react";
import { initiativeService } from "@/services/initiativeService";
import {
  InitiativeFile,
  initiativeFileService,
} from "@/services/initiative-file.service";

// Interface matching the actual API response
interface ApiInitiativeResponse {
  InitiativeId: number;
  AllocationNo: string;
  Name: string;
  Description: string;
  SalesOrgCode: string;
  Brand: string;
  ContractAmount: number;
  ActivityType: string;
  DisplayType: string;
  DisplayLocation: string;
  CustomerType: string;
  StartDate: string;
  EndDate: string;
  Status: string;
  IsActive: boolean;
  Customers: ApiCustomer[];
  Products: ApiProduct[];
}

interface ApiCustomer {
  InitiativeCustomerId: number;
  CustomerCode: string;
  DisplayType: string;
  DisplayLocation: string;
  ExecutionStatus: string;
}

interface ApiProduct {
  InitiativeProductId: number;
  ItemCode: string;
  ItemName: string;
}

export default function InitiativeViewPage() {
  const params = useParams();
  const router = useRouter();
  const id = parseInt(params.id as string);

  const [initiative, setInitiative] = useState<ApiInitiativeResponse | null>(
    null
  );
  const [files, setFiles] = useState<InitiativeFile[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchInitiativeData = async () => {
      try {
        setLoading(true);

        // Fetch initiative details
        const initiativeData = await initiativeService.getInitiativeById(id);
        setInitiative(initiativeData);

        // Fetch associated files (all files)
        const filesData = await initiativeFileService.getInitiativeFiles(
          id.toString()
        );
        console.log("📁 Files fetched for initiative", id, ":", filesData);

        // Fetch only images using the new filtering method
        const imagesData = await initiativeFileService.getInitiativeImages(
          id.toString()
        );
        console.log("🖼️ Images fetched for initiative", id, ":", imagesData);
        console.log("🔍 Debugging - Initiative ID:", id, "Type:", typeof id);
        console.log(
          "🔍 Debugging - String ID:",
          id.toString(),
          "Type:",
          typeof id.toString()
        );
        console.log("🔍 Debugging - Files found:", filesData.length);
        if (filesData.length > 0) {
          console.log("🔍 Debugging - First file:", filesData[0]);
          filesData.forEach((file, index) => {
            console.log(`🔍 File ${index + 1}:`, {
              UID: file.UID,
              LinkedItemUID: file.LinkedItemUID,
              FileSysType: file.FileSysType,
              FileType: file.FileType,
              RelativePath: file.RelativePath,
              DisplayName: file.DisplayName,
              generatedURL: file.RelativePath
                ? initiativeFileService.getFileUrl(file.RelativePath)
                : "No RelativePath",
            });
          });
        }
        setFiles(filesData);

        // Test SPECIFIC filtering for Initiative images only
        try {
          const specificTest = {
            PageNumber: 1,
            PageSize: 100,
            IsCountRequired: true,
            FilterCriterias: [
              { Name: "LinkedItemType", Value: "Initiative" },
              { Name: "LinkedItemUID", Value: id.toString() },
              { Name: "FileType", Value: "image/jpeg" }, // Only get images
            ],
            SortCriterias: [],
          };

          console.log(
            "🔍 Testing specific filter for Initiative 17:",
            JSON.stringify(specificTest, null, 2)
          );

          const specificResponse = await fetch(
            `${
              process.env.NEXT_PUBLIC_API_URL || "https://multiplex-promotions-api.winitsoftware.com/api"
            }/FileSys/SelectAllFileSysDetails`,
            {
              method: "POST",
              headers: {
                Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
                "Content-Type": "application/json",
              },
              body: JSON.stringify(specificTest),
            }
          );

          const specificResult = await specificResponse.json();
          console.log(
            "🎯 SPECIFIC Initiative 17 API Response:",
            specificResult
          );
          console.log("🎯 Response Status:", specificResponse.status);
          console.log(
            "🎯 Response Headers:",
            Object.fromEntries(specificResponse.headers.entries())
          );

          if (specificResult.IsSuccess && specificResult.Data?.PagedData) {
            console.log(
              `🎯 Found ${specificResult.Data.PagedData.length} files specifically for Initiative 17:`
            );
            specificResult.Data.PagedData.forEach((file, index) => {
              console.log(`🎯 Initiative File ${index + 1}:`, {
                UID: file.UID,
                LinkedItemType: file.LinkedItemType,
                LinkedItemUID: file.LinkedItemUID,
                FileSysType: file.FileSysType,
                FileType: file.FileType,
                RelativePath: file.RelativePath,
                DisplayName: file.DisplayName,
              });
            });
          }
        } catch (err) {
          console.log("Failed to fetch specific Initiative files:", err);
        }
      } catch (err: any) {
        console.error("Error fetching initiative:", err);
        setError(err.message || "Failed to fetch initiative details");
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchInitiativeData();
    }
  }, [id]);

  const getStatusColor = (status: string) => {
    switch (status?.toLowerCase()) {
      case "active":
        return "bg-green-100 text-green-800 border-green-200";
      case "draft":
        return "bg-gray-100 text-gray-800 border-gray-200";
      case "submitted":
        return "bg-blue-100 text-blue-800 border-blue-200";
      case "cancelled":
        return "bg-red-100 text-red-800 border-red-200";
      default:
        return "bg-gray-100 text-gray-800 border-gray-200";
    }
  };

  const getFilesByType = (fileType: string) => {
    return files.filter((file) => file.FileSysType === fileType);
  };

  if (loading) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <div className="text-lg">Loading initiative details...</div>
        </div>
      </div>
    );
  }

  if (error || !initiative) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex flex-col items-center justify-center min-h-[400px]">
          <div className="text-lg text-red-600 mb-4">
            {error || "Initiative not found"}
          </div>
          <Button onClick={() => router.back()} variant="outline">
            <ArrowLeft className="w-4 h-4 mr-2" />
            Go Back
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      {/* Header */}
      <div className="flex items-center justify-between mb-8">
        <div className="flex items-center gap-4">
          <Button
            onClick={() => router.back()}
            variant="ghost"
            size="sm"
            className="p-2"
          >
            <ArrowLeft className="w-4 h-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold">{initiative.Name}</h1>
            <p className="text-gray-600 mt-1">Initiative Details</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <Badge className={getStatusColor(initiative.Status)}>
            {initiative.Status.toUpperCase()}
          </Badge>
          <Button
            onClick={() =>
              router.push(
                `/administration/configurations/initiatives/edit/${id}`
              )
            }
          >
            Edit Initiative
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Main Details */}
        <div className="lg:col-span-2 space-y-6">
          {/* Basic Information */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="w-5 h-5" />
                Basic Information
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Initiative ID
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.InitiativeId}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Allocation Number
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.AllocationNo}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Sales Org Code
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.SalesOrgCode}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Brand
                  </label>
                  <p className="text-lg font-semibold">{initiative.Brand}</p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Activity Type
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.ActivityType}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Customer Type
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.CustomerType}
                  </p>
                </div>
              </div>
              <Separator />
              <div>
                <label className="text-sm font-medium text-gray-600">
                  Description
                </label>
                <p className="text-gray-800 mt-1">{initiative.Description}</p>
              </div>
            </CardContent>
          </Card>

          {/* Display Details */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="w-5 h-5" />
                Display Details
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Display Type
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.DisplayType}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Display Location
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.DisplayLocation}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Status
                  </label>
                  <p className="text-lg font-semibold">
                    {initiative.IsActive ? "Active" : "Inactive"}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Customers Section */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Users className="w-5 h-5" />
                Customers ({initiative.Customers?.length || 0})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {initiative.Customers?.map((customer) => (
                  <div
                    key={customer.InitiativeCustomerId}
                    className="border rounded-lg p-3"
                  >
                    <div className="grid grid-cols-2 gap-2 text-sm">
                      <div>
                        <span className="font-medium">Customer Code:</span>{" "}
                        {customer.CustomerCode}
                      </div>
                      <div>
                        <span className="font-medium">Execution Status:</span>
                        <Badge variant="secondary" className="ml-1">
                          {customer.ExecutionStatus}
                        </Badge>
                      </div>
                      <div>
                        <span className="font-medium">Display Type:</span>{" "}
                        {customer.DisplayType}
                      </div>
                      <div>
                        <span className="font-medium">Display Location:</span>{" "}
                        {customer.DisplayLocation}
                      </div>
                    </div>
                  </div>
                ))}
                {(!initiative.Customers ||
                  initiative.Customers.length === 0) && (
                  <p className="text-gray-500 text-sm">No customers assigned</p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Products Section */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="w-5 h-5" />
                Products ({initiative.Products?.length || 0})
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {initiative.Products?.map((product) => (
                  <div
                    key={product.InitiativeProductId}
                    className="border rounded-lg p-3"
                  >
                    <div className="text-sm">
                      <p className="font-medium">{product.ItemName}</p>
                      <p className="text-gray-600 mt-1">
                        Code: {product.ItemCode}
                      </p>
                    </div>
                  </div>
                ))}
                {(!initiative.Products || initiative.Products.length === 0) && (
                  <p className="text-gray-500 text-sm">No products assigned</p>
                )}
              </div>
            </CardContent>
          </Card>

          {/* Files Section */}
          <Card>
            <CardHeader>
              <CardTitle>Attached Files</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-6">
                {/* POSM Files */}
                <div>
                  <h4 className="flex items-center gap-2 font-medium mb-3">
                    <Image className="w-4 h-4" />
                    POSM Files ({getFilesByType("POSM").length})
                  </h4>
                  {getFilesByType("POSM").length > 0 ? (
                    <div className="grid grid-cols-2 gap-4">
                      {getFilesByType("POSM").map((file) => (
                        <div key={file.UID} className="border rounded-lg p-3">
                          <p className="font-medium text-sm">
                            {file.DisplayName}
                          </p>
                          <p className="text-xs text-gray-500">
                            {initiativeFileService.formatFileSize(
                              file.FileSize
                            )}
                          </p>
                          {file.RelativePath && (
                            <img
                              src={
                                file.RelativePath
                                  ? `http://localhost:8000/${file.RelativePath}`
                                  : "/placeholder-product.svg"
                              }
                              alt={file.DisplayName || "POSM Image"}
                              className="w-full h-24 object-cover mt-2 rounded"
                              loading="lazy"
                              onError={(e) => {
                                const target =
                                  e.currentTarget as HTMLImageElement;
                                if (target.src !== "/placeholder-product.svg") {
                                  target.src = "/placeholder-product.svg";
                                }
                              }}
                            />
                          )}
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-gray-500 text-sm">
                      No POSM files attached
                    </p>
                  )}
                </div>

                <Separator />

                {/* Default Images */}
                <div>
                  <h4 className="flex items-center gap-2 font-medium mb-3">
                    <Image className="w-4 h-4" />
                    Default Images ({getFilesByType("DefaultImage").length})
                  </h4>
                  {getFilesByType("DefaultImage").length > 0 ? (
                    <div className="grid grid-cols-2 gap-4">
                      {getFilesByType("DefaultImage").map((file) => (
                        <div key={file.UID} className="border rounded-lg p-3">
                          <p className="font-medium text-sm">
                            {file.DisplayName}
                          </p>
                          <p className="text-xs text-gray-500">
                            {initiativeFileService.formatFileSize(
                              file.FileSize
                            )}
                          </p>
                          {file.RelativePath && (
                            <img
                              src={
                                file.RelativePath
                                  ? `http://localhost:8000/${file.RelativePath}`
                                  : "/placeholder-product.svg"
                              }
                              alt={file.DisplayName || "Default Image"}
                              className="w-full h-24 object-cover mt-2 rounded"
                              loading="lazy"
                              onError={(e) => {
                                const target =
                                  e.currentTarget as HTMLImageElement;
                                if (target.src !== "/placeholder-product.svg") {
                                  target.src = "/placeholder-product.svg";
                                }
                              }}
                            />
                          )}
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-gray-500 text-sm">
                      No default images attached
                    </p>
                  )}
                </div>

                <Separator />

                {/* Email Attachments */}
                <div>
                  <h4 className="flex items-center gap-2 font-medium mb-3">
                    <Mail className="w-4 h-4" />
                    Email Attachments (
                    {getFilesByType("EmailAttachment").length})
                  </h4>
                  {getFilesByType("EmailAttachment").length > 0 ? (
                    <div className="space-y-2">
                      {getFilesByType("EmailAttachment").map((file) => (
                        <div
                          key={file.UID}
                          className="flex items-center justify-between border rounded-lg p-3"
                        >
                          <div>
                            <p className="font-medium text-sm">
                              {file.DisplayName}
                            </p>
                            <p className="text-xs text-gray-500">
                              {initiativeFileService.formatFileSize(
                                file.FileSize
                              )}
                            </p>
                          </div>
                          {file.RelativePath && (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={() =>
                                file.RelativePath &&
                                window.open(
                                  `http://localhost:8000/${file.RelativePath}`
                                )
                              }
                            >
                              Download
                            </Button>
                          )}
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-gray-500 text-sm">
                      No email attachments
                    </p>
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Sidebar */}
        <div className="space-y-6">
          {/* Financial Info */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <DollarSign className="w-5 h-5" />
                Financial Information
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Contract Amount
                  </label>
                  <p className="text-2xl font-bold text-green-600">
                    ${initiative.ContractAmount?.toLocaleString() || "0"}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Timeline */}
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Calendar className="w-5 h-5" />
                Timeline
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    Start Date
                  </label>
                  <p className="text-lg font-semibold">
                    {new Date(initiative.StartDate).toLocaleDateString()}
                  </p>
                </div>
                <div>
                  <label className="text-sm font-medium text-gray-600">
                    End Date
                  </label>
                  <p className="text-lg font-semibold">
                    {new Date(initiative.EndDate).toLocaleDateString()}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Summary Stats */}
          <Card>
            <CardHeader>
              <CardTitle>Summary</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-2">
                    <Users className="w-4 h-4" />
                    Customers
                  </span>
                  <Badge variant="secondary">
                    {initiative.Customers?.length || 0}
                  </Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-2">
                    <Package className="w-4 h-4" />
                    Products
                  </span>
                  <Badge variant="secondary">
                    {initiative.Products?.length || 0}
                  </Badge>
                </div>
                <div className="flex items-center justify-between">
                  <span className="flex items-center gap-2">
                    <FileText className="w-4 h-4" />
                    Files
                  </span>
                  <Badge variant="secondary">{files.length}</Badge>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
