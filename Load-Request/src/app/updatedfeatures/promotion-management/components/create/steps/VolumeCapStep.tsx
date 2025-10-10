"use client";

import React, { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Progress } from "@/components/ui/progress";
import { 
  Trash2, 
  Shield, 
  Calendar, 
  Building2, 
  AlertTriangle, 
  TrendingUp, 
  Clock, 
  Users, 
  ChevronDown, 
  ChevronUp, 
  Plus, 
  Settings, 
  Info,
  DollarSign
} from "lucide-react";

interface VolumeCapsStepProps {
  volumeCaps: {
    enabled: boolean;
    overallCap: {
      type: "value" | "quantity" | "count";
      value: number;
      consumed: number;
    };
    invoiceCap: {
      maxDiscountValue: number;
      maxQuantity: number;
      maxApplications: number;
    };
    periodCaps: Array<{
      periodType: "daily" | "weekly" | "monthly" | "custom";
      customDays?: number;
      capType: "value" | "quantity" | "percentage";
      capValue: number;
      startOffset?: number;
    }>;
    hierarchyCaps: Array<{
      hierarchyType: "organization" | "location" | "branch" | "store" | "salesperson";
      hierarchyId: string;
      hierarchyName: string;
      capType: "value" | "quantity";
      capValue: number;
      periodType?: "total" | "monthly" | "weekly" | "daily";
    }>;
  };
  onVolumeCapsUpdate: (field: string, value: any) => void;
}

export default function VolumeCapStep({
  volumeCaps,
  onVolumeCapsUpdate
}: VolumeCapsStepProps) {
  const [showPeriodCaps, setShowPeriodCaps] = useState(false);
  const [showHierarchyCaps, setShowHierarchyCaps] = useState(false);

  const addPeriodCap = () => {
    const newCap = {
      period: "day" as const,  // Changed to match ReviewStep expectation
      type: "amount" as const,  // Changed to match ReviewStep expectation
      value: 0,
      periodType: "daily" as const, // Keep for backward compatibility
      capType: "value" as const,
      capValue: 0,
      startOffset: 0
    };
    onVolumeCapsUpdate("periodCaps", [...volumeCaps.periodCaps, newCap]);
  };

  const updatePeriodCap = (index: number, field: string, value: any) => {
    const updatedCaps = [...volumeCaps.periodCaps];
    updatedCaps[index] = { ...updatedCaps[index], [field]: value };
    
    // Also update the ReviewStep expected fields
    if (field === "periodType") {
      updatedCaps[index].period = value === "daily" ? "day" 
        : value === "weekly" ? "week"
        : value === "monthly" ? "month"
        : value;
    }
    if (field === "capType") {
      updatedCaps[index].type = value === "value" ? "amount"
        : value === "quantity" ? "quantity" 
        : value;
    }
    if (field === "capValue") {
      updatedCaps[index].value = value;
    }
    
    onVolumeCapsUpdate("periodCaps", updatedCaps);
  };

  const removePeriodCap = (index: number) => {
    const updatedCaps = volumeCaps.periodCaps.filter((_, i) => i !== index);
    onVolumeCapsUpdate("periodCaps", updatedCaps);
  };

  const addHierarchyCap = () => {
    const newCap = {
      hierarchyLevel: "Store",  // Added for ReviewStep
      hierarchyType: "store" as const,
      hierarchyId: "",
      hierarchyName: "",
      type: "amount" as const,  // Added for ReviewStep
      value: 0,  // Added for ReviewStep
      capType: "value" as const,
      capValue: 0,
      periodType: "total" as const
    };
    onVolumeCapsUpdate("hierarchyCaps", [...volumeCaps.hierarchyCaps, newCap]);
  };

  const updateHierarchyCap = (index: number, field: string, value: any) => {
    const updatedCaps = [...volumeCaps.hierarchyCaps];
    updatedCaps[index] = { ...updatedCaps[index], [field]: value };
    
    // Also update the ReviewStep expected fields
    if (field === "hierarchyType") {
      updatedCaps[index].hierarchyLevel = value.charAt(0).toUpperCase() + value.slice(1);
    }
    if (field === "capType") {
      updatedCaps[index].type = value === "value" ? "amount"
        : value === "quantity" ? "quantity"
        : value;
    }
    if (field === "capValue") {
      updatedCaps[index].value = value;
    }
    
    onVolumeCapsUpdate("hierarchyCaps", updatedCaps);
  };

  const removeHierarchyCap = (index: number) => {
    const updatedCaps = volumeCaps.hierarchyCaps.filter((_, i) => i !== index);
    onVolumeCapsUpdate("hierarchyCaps", updatedCaps);
  };

  const consumedPercentage = volumeCaps.overallCap.value > 0 
    ? Math.min((volumeCaps.overallCap.consumed / volumeCaps.overallCap.value) * 100, 100)
    : 0;

  return (
    <div className="max-w-[75%] space-y-4">
      <div className="mb-6">
        <h2 className="text-xl font-medium text-gray-900">Volume Caps</h2>
      </div>

      {/* Enable Toggle Card */}
      <div className="border border-gray-200 rounded-lg p-5">
          <div className="flex items-center justify-between">
            <div>
              <h3 className="text-base font-medium text-gray-900 mb-1">
                Usage Limits
              </h3>
              <p className="text-sm text-gray-500">
                Set maximum limits to control promotion usage and budget
              </p>
            </div>
            <Switch
              checked={volumeCaps.enabled}
              onCheckedChange={(checked) => onVolumeCapsUpdate("enabled", checked)}
            />
          </div>

          {volumeCaps.enabled && (
            <div className="mt-6 space-y-6">
              {/* Overall Cap Section */}
              <div>
                <h3 className="text-sm font-medium text-gray-900 mb-4">Overall Promotion Cap</h3>
                <div className="grid md:grid-cols-2 gap-4">
                  <div>
                    <Label htmlFor="capType" className="text-sm font-medium text-gray-700 mb-1.5 block">Cap Type</Label>
                    <Select
                      value={volumeCaps.overallCap.type}
                      onValueChange={(value) =>
                        onVolumeCapsUpdate("overallCap", {
                          ...volumeCaps.overallCap,
                          type: value
                        })
                      }
                    >
                      <SelectTrigger id="capType" className="h-10 mt-1">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="value">Total Value (₹)</SelectItem>
                        <SelectItem value="quantity">Total Quantity</SelectItem>
                        <SelectItem value="count">Usage Count</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <Label htmlFor="capLimit" className="text-sm font-medium text-gray-700 mb-1.5 block">Cap Limit</Label>
                    <Input
                      id="capLimit"
                      type="number"
                      value={volumeCaps.overallCap.value || ''}
                      onChange={(e) =>
                        onVolumeCapsUpdate("overallCap", {
                          ...volumeCaps.overallCap,
                          value: e.target.value ? parseFloat(e.target.value) : 0
                        })
                      }
                      placeholder="Enter cap limit"
                      className="h-10 mt-1"
                    />
                  </div>
                </div>

                {volumeCaps.overallCap.value > 0 && (
                  <div className="mt-4 p-3  rounded-lg">
                    <div className="flex items-center justify-between mb-2">
                      <span className="text-xs text-gray-600">Usage</span>
                      <span className="text-xs font-medium text-gray-900">
                        {volumeCaps.overallCap.consumed} / {volumeCaps.overallCap.value}
                        {volumeCaps.overallCap.type === 'value' && ' ₹'}
                      </span>
                    </div>
                    <Progress value={consumedPercentage} className="h-2" />
                  </div>
                )}
              </div>

              {/* Invoice Level Caps */}
              <div>
                <div className="flex items-center gap-3 mb-4">
                  <Settings className="w-4 h-4 text-gray-500" />
                  <h3 className="text-sm font-medium text-gray-900">Per Invoice Limits</h3>
                </div>

                <div className="grid md:grid-cols-3 gap-4 p-4  rounded-lg">
                  <div>
                    <Label htmlFor="maxDiscountValue" className="text-xs text-gray-700 mb-1 block">
                      Max Discount Value
                    </Label>
                    <div className="relative">
                      <DollarSign className="absolute left-3 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
                      <Input
                        id="maxDiscountValue"
                        type="number"
                        value={volumeCaps.invoiceCap.maxDiscountValue || ''}
                        onChange={(e) =>
                          onVolumeCapsUpdate("invoiceCap", {
                            ...volumeCaps.invoiceCap,
                            maxDiscountValue: e.target.value ? parseFloat(e.target.value) : 0
                          })
                        }
                        placeholder="0.00"
                        className="pl-9 h-10 mt-1"
                      />
                    </div>
                  </div>
                  <div>
                    <Label htmlFor="maxQuantity" className="text-xs text-gray-700 mb-1 block">
                      Max Quantity
                    </Label>
                    <Input
                      id="maxQuantity"
                      type="number"
                      value={volumeCaps.invoiceCap.maxQuantity || ''}
                      onChange={(e) =>
                        onVolumeCapsUpdate("invoiceCap", {
                          ...volumeCaps.invoiceCap,
                          maxQuantity: e.target.value ? parseInt(e.target.value) : 0
                        })
                      }
                      placeholder="0"
                      className="h-10 mt-1"
                    />
                  </div>
                  <div>
                    <Label htmlFor="maxApplications" className="text-xs text-gray-700 mb-1 block">
                      Max Applications
                    </Label>
                    <Input
                      id="maxApplications"
                      type="number"
                      value={volumeCaps.invoiceCap.maxApplications || ''}
                      onChange={(e) =>
                        onVolumeCapsUpdate("invoiceCap", {
                          ...volumeCaps.invoiceCap,
                          maxApplications: e.target.value ? parseInt(e.target.value) : 0
                        })
                      }
                      placeholder="0"
                      className="h-10 mt-1"
                    />
                  </div>
                </div>
              </div>

              {/* Period Caps Section */}
              <div>
                <button
                  onClick={() => setShowPeriodCaps(!showPeriodCaps)}
                  className="flex items-center justify-between w-full p-4  border border-gray-200 rounded-lg hover: transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <Clock className="w-4 h-4 text-gray-500" />
                    <div className="text-left">
                      <h3 className="text-sm font-medium text-gray-900">Period-based Limits</h3>
                      <p className="text-xs text-gray-500 mt-0.5">
                        Set daily, weekly, or monthly caps
                      </p>
                    </div>
                  </div>
                  {showPeriodCaps ? (
                    <ChevronUp className="w-5 h-5 text-gray-400" />
                  ) : (
                    <ChevronDown className="w-5 h-5 text-gray-400" />
                  )}
                </button>

                {showPeriodCaps && (
                  <div className="mt-4 space-y-3">
                    {volumeCaps.periodCaps.map((cap, index) => (
                      <div key={index} className="border border-gray-200 rounded-lg p-4">
                        <div className="flex items-start justify-between">
                          <div className="grid grid-cols-3 gap-3 flex-1">
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Period</Label>
                              <Select
                                value={cap.periodType}
                                onValueChange={(value) => updatePeriodCap(index, "periodType", value)}
                              >
                                <SelectTrigger className="h-10 mt-1">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="daily">Daily</SelectItem>
                                  <SelectItem value="weekly">Weekly</SelectItem>
                                  <SelectItem value="monthly">Monthly</SelectItem>
                                  <SelectItem value="custom">Custom</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Cap Type</Label>
                              <Select
                                value={cap.capType}
                                onValueChange={(value) => updatePeriodCap(index, "capType", value)}
                              >
                                <SelectTrigger className="h-10 mt-1">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="value">Value (₹)</SelectItem>
                                  <SelectItem value="quantity">Quantity</SelectItem>
                                  <SelectItem value="percentage">Percentage</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Cap Value</Label>
                              <Input
                                type="number"
                                value={cap.capValue || ''}
                                onChange={(e) =>
                                  updatePeriodCap(index, "capValue", e.target.value ? parseFloat(e.target.value) : 0)
                                }
                                placeholder="0"
                                className="h-10 mt-1"
                              />
                            </div>
                          </div>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => removePeriodCap(index)}
                            className="ml-2 text-red-500 hover:text-red-600"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={addPeriodCap}
                      className="w-full"
                    >
                      <Plus className="w-4 h-4 mr-2" />
                      Add Period Cap
                    </Button>
                  </div>
                )}
              </div>

              {/* Hierarchy Caps Section */}
              <div>
                <button
                  onClick={() => setShowHierarchyCaps(!showHierarchyCaps)}
                  className="flex items-center justify-between w-full p-4  border border-gray-200 rounded-lg hover: transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <Building2 className="w-4 h-4 text-gray-500" />
                    <div className="text-left">
                      <h3 className="text-sm font-medium text-gray-900">Hierarchy-based Limits</h3>
                      <p className="text-xs text-gray-500 mt-0.5">
                        Set limits per store, branch, or region
                      </p>
                    </div>
                  </div>
                  {showHierarchyCaps ? (
                    <ChevronUp className="w-5 h-5 text-gray-400" />
                  ) : (
                    <ChevronDown className="w-5 h-5 text-gray-400" />
                  )}
                </button>

                {showHierarchyCaps && (
                  <div className="mt-4 space-y-3">
                    {volumeCaps.hierarchyCaps.map((cap, index) => (
                      <div key={index} className="border border-gray-200 rounded-lg p-4">
                        <div className="flex items-start justify-between">
                          <div className="grid grid-cols-4 gap-3 flex-1">
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Hierarchy Type</Label>
                              <Select
                                value={cap.hierarchyType}
                                onValueChange={(value) => updateHierarchyCap(index, "hierarchyType", value)}
                              >
                                <SelectTrigger className="h-10 mt-1">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="organization">Organization</SelectItem>
                                  <SelectItem value="location">Location</SelectItem>
                                  <SelectItem value="branch">Branch</SelectItem>
                                  <SelectItem value="store">Store</SelectItem>
                                  <SelectItem value="salesperson">Salesperson</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Name</Label>
                              <Input
                                type="text"
                                value={cap.hierarchyName || ''}
                                onChange={(e) =>
                                  updateHierarchyCap(index, "hierarchyName", e.target.value)
                                }
                                placeholder="Enter name"
                                className="h-10 mt-1"
                              />
                            </div>
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Cap Type</Label>
                              <Select
                                value={cap.capType}
                                onValueChange={(value) => updateHierarchyCap(index, "capType", value)}
                              >
                                <SelectTrigger className="h-10 mt-1">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  <SelectItem value="value">Value (₹)</SelectItem>
                                  <SelectItem value="quantity">Quantity</SelectItem>
                                </SelectContent>
                              </Select>
                            </div>
                            <div>
                              <Label className="text-xs text-gray-700 mb-1 block">Cap Value</Label>
                              <Input
                                type="number"
                                value={cap.capValue || ''}
                                onChange={(e) =>
                                  updateHierarchyCap(index, "capValue", e.target.value ? parseFloat(e.target.value) : 0)
                                }
                                placeholder="0"
                                className="h-10 mt-1"
                              />
                            </div>
                          </div>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => removeHierarchyCap(index)}
                            className="ml-2 text-red-500 hover:text-red-600"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={addHierarchyCap}
                      className="w-full"
                    >
                      <Plus className="w-4 h-4 mr-2" />
                      Add Hierarchy Cap
                    </Button>
                  </div>
                )}
              </div>
            </div>
          )}
      </div>

    </div>
  );
}