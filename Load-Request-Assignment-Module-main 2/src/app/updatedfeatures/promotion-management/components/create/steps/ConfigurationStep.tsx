"use client";

import React, { useState, useEffect } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Textarea } from "@/components/ui/textarea";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { AlertCircle, Info, Package, Plus, Minus, Trash2, ShoppingCart } from "lucide-react";
import { PromotionV3FormData } from "../../../types/promotionV3.types";
import DynamicProductAttributes, { ProductAttribute } from "../../product-selection/DynamicProductAttributes";
import MultiProductConfiguration from "../../multi-product/MultiProductConfiguration";

interface ConfigurationStepProps {
  selectedLevel: string;
  selectedFormat: string;
  formData: PromotionV3FormData;
  onFormDataChange: (field: string, value: any) => void;
  orgUid?: string;
}

export default function ConfigurationStep({
  selectedLevel,
  selectedFormat,
  formData,
  onFormDataChange,
  orgUid
}: ConfigurationStepProps) {
  // Local state for FOC product selection (simplified for now)
  const [focProducts, setFocProducts] = useState<any[]>(formData.focProducts || []);

  // Render slab configuration for IQFD and IQPD
  const renderSlabConfiguration = (offerType: "FIXED" | "PERCENTAGE") => {
    const isPercentage = offerType === "PERCENTAGE";

    return (
      <div className="space-y-4">
        {/* Slab Configuration Toggle */}
        <div className="flex items-center gap-3 p-4 border border-gray-200 rounded-lg">
          <input
            type="checkbox"
            id={`hasSlabs${isPercentage ? "Percentage" : ""}`}
            checked={formData.hasSlabs}
            onChange={(e) => {
              onFormDataChange("hasSlabs", e.target.checked);
              if (e.target.checked) {
                // Initialize with one slab
                onFormDataChange("slabs", [
                  {
                    slabNo: 1,
                    minQty: null,
                    maxQty: null,
                    offerType: offerType,
                    ...(isPercentage
                      ? { discountPercent: null }
                      : { discountAmount: null })
                  }
                ]);
              } else {
                onFormDataChange("slabs", []);
              }
            }}
            className="w-4 h-4 text-purple-600"
          />
          <label
            htmlFor={`hasSlabs${isPercentage ? "Percentage" : ""}`}
            className="font-medium cursor-pointer"
          >
            Enable Quantity-based Slabs/Tiers
          </label>
          <span className="text-xs text-gray-500 ml-auto">(Optional)</span>
        </div>

        {!formData.hasSlabs ? (
          <>
            <div>
              <Label htmlFor="discount" className="text-sm font-medium text-gray-700 mb-1.5 block">
                {isPercentage ? "Discount Percentage (%)" : "Discount Amount (₹)"}
                <span className="text-red-500 ml-1">*</span>
              </Label>
              <Input
                id="discount"
                type="number"
                value={
                  isPercentage
                    ? (formData.discountPercent || '')
                    : (formData.discountAmount || '')
                }
                onChange={(e) =>
                  onFormDataChange(
                    isPercentage ? "discountPercent" : "discountAmount",
                    e.target.value ? parseFloat(e.target.value) : null
                  )
                }
                placeholder={
                  isPercentage
                    ? "Enter discount percentage"
                    : "Enter fixed discount amount"
                }
                {...(isPercentage && { min: "0.01", max: "100", step: "0.01" })}
                {...(!isPercentage && { min: "0.01", step: "0.01" })}
                className="h-10 w-full mt-1"
              />
            </div>
            {isPercentage && (
              <div>
                <Label htmlFor="maxDiscountAmount" className="text-sm font-medium text-gray-700 mb-1.5 block">
                  Maximum Discount Amount (₹) <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="maxDiscountAmount"
                  type="number"
                  value={formData.maxDiscountAmount || ''}
                  onChange={(e) =>
                    onFormDataChange(
                      "maxDiscountAmount",
                      e.target.value ? parseFloat(e.target.value) : null
                    )
                  }
                  placeholder="Optional max discount cap"
                  min="0.01"
                  step="0.01"
                  className="h-10 w-full mt-1"
                />
              </div>
            )}
          </>
        ) : (
          <div className="space-y-4">
            <div className="flex justify-between items-center">
              <h4 className="font-medium">Quantity Slabs</h4>
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  const lastSlab = formData.slabs?.[formData.slabs.length - 1];
                  const newSlab = {
                    slabNo: (formData.slabs?.length || 0) + 1,
                    minQty: lastSlab?.maxQty ? lastSlab.maxQty + 1 : null,
                    maxQty: null,
                    offerType: offerType,
                    ...(isPercentage
                      ? { discountPercent: null }
                      : { discountAmount: null })
                  };
                  onFormDataChange("slabs", [
                    ...(formData.slabs || []),
                    newSlab
                  ]);
                }}
              >
                <Plus className="w-4 h-4 mr-2" />
                Add Slab
              </Button>
            </div>

            {formData.slabs?.map((slab, index) => {
              const hasErrors = !slab.minQty || !slab.maxQty || 
                               (slab.minQty && slab.maxQty && slab.minQty >= slab.maxQty) ||
                               (!slab.discountAmount && !slab.discountPercent);
              
              return (
                <div key={index} className={`border rounded-lg p-4 ${hasErrors ? 'border-red-300' : 'border-gray-200'}`}>
                    <div className="grid grid-cols-4 gap-3">
                      <div>
                        <Label className="text-xs text-gray-700 mb-1 block">
                          Min Qty <span className="text-red-500">*</span>
                        </Label>
                        <Input
                          type="number"
                          value={slab.minQty || ''}
                          onChange={(e) => {
                            const newSlabs = [...(formData.slabs || [])];
                            newSlabs[index].minQty = e.target.value ? parseInt(e.target.value) : null;
                            onFormDataChange("slabs", newSlabs);
                          }}
                          placeholder="Min"
                          min="1"
                          className="h-10 w-full"
                        />
                      </div>
                      <div>
                        <Label className="text-xs text-gray-700 mb-1 block">
                          Max Qty <span className="text-red-500">*</span>
                        </Label>
                        <Input
                          type="number"
                          value={slab.maxQty || ''}
                          onChange={(e) => {
                            const newSlabs = [...(formData.slabs || [])];
                            newSlabs[index].maxQty = e.target.value ? parseInt(e.target.value) : null;
                            onFormDataChange("slabs", newSlabs);
                          }}
                          placeholder="Max"
                          min="1"
                          className="h-10 w-full"
                        />
                      </div>
                      <div>
                        <Label className="text-xs text-gray-700 mb-1 block">
                          {isPercentage ? "Discount (%)" : "Discount (₹)"} <span className="text-red-500">*</span>
                        </Label>
                        <Input
                          type="number"
                          value={
                            isPercentage
                              ? (slab.discountPercent || '')
                              : (slab.discountAmount || '')
                          }
                          onChange={(e) => {
                            const newSlabs = [...(formData.slabs || [])];
                            if (isPercentage) {
                              newSlabs[index].discountPercent = e.target.value ? parseFloat(e.target.value) : null;
                            } else {
                              newSlabs[index].discountAmount = e.target.value ? parseFloat(e.target.value) : null;
                            }
                            onFormDataChange("slabs", newSlabs);
                          }}
                          placeholder="Value"
                          {...(isPercentage && { min: "0.01", max: "100", step: "0.01" })}
                          {...(!isPercentage && { min: "0.01", step: "0.01" })}
                          className="h-10 w-full"
                        />
                      </div>
                      <div className="flex items-end">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => {
                            const newSlabs = formData.slabs?.filter(
                              (_, i) => i !== index
                            );
                            onFormDataChange("slabs", newSlabs);
                          }}
                          className="text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="w-4 h-4" />
                        </Button>
                      </div>
                    </div>
                    {hasErrors && (
                      <div className="mt-2 text-xs text-red-500">
                        {!slab.minQty && <div>• Minimum quantity is required</div>}
                        {!slab.maxQty && <div>• Maximum quantity is required</div>}
                        {slab.minQty && slab.maxQty && slab.minQty >= slab.maxQty && 
                          <div>• Maximum quantity must be greater than minimum quantity</div>}
                        {!slab.discountAmount && !slab.discountPercent && 
                          <div>• Discount value is required</div>}
                      </div>
                    )}
                </div>
              );
            })}
          </div>
        )}

        {selectedFormat === "IQFD" && (
          <div>
            <Label htmlFor="maxDealCount" className="text-sm font-medium text-gray-700 mb-1.5 block">Maximum Deal Count</Label>
            <Input
              id="maxDealCount"
              type="number"
              value={formData.maxDealCount || ''}
              onChange={(e) =>
                onFormDataChange("maxDealCount", e.target.value ? parseInt(e.target.value) : 0)
              }
              placeholder="Max times this can be applied"
              className="h-10 w-full mt-1"
            />
          </div>
        )}
      </div>
    );
  };

  // Render Buy X Get Y configuration for IQXF and BQXF
  const renderBuyXGetYConfiguration = () => (
    <div className="space-y-4">
      {/* IQXF Configuration */}
      {selectedFormat === "IQXF" && (
        <>
          <Alert className="border-blue-200 mb-4">
            <Info className="h-4 w-4 text-blue-600" />
            <AlertDescription className="text-blue-700">
              <strong>Buy X Get Y Configuration:</strong><br/>
              • The "Buy X" quantity is set in the product selection above (Min. Purchase Qty)<br/>
              • Configure the "Get Y Free" quantity below
            </AlertDescription>
          </Alert>

          <div>
            <Label htmlFor="freeQuantity" className="text-sm font-medium text-gray-700 mb-1.5 block">
              Get Y Units Free <span className="text-red-500">*</span>
            </Label>
            <Input
              id="freeQuantity"
              type="number"
              value={formData.freeQuantity || formData.getQty || ''}
              onChange={(e) => {
                const value = e.target.value ? parseInt(e.target.value) : 0;
                // Update both freeQuantity and getQty for compatibility
                onFormDataChange("freeQuantity", value);
                onFormDataChange("getQty", value);
              }}
              placeholder="e.g., 1"
              min="1"
              className="h-10 w-full mt-1"
            />
            <p className="text-xs text-gray-500 mt-1">
              Number of free units customer gets for each qualifying purchase
            </p>
          </div>

          <div>
            <Label htmlFor="maxDealCount" className="text-sm font-medium text-gray-700 mb-1.5 block">Maximum Applications per Invoice</Label>
            <Input
              id="maxDealCount"
              type="number"
              value={formData.maxDealCount === 0 ? '' : formData.maxDealCount}
              onChange={(e) =>
                onFormDataChange("maxDealCount", e.target.value ? parseInt(e.target.value) : 0)
              }
              placeholder="Leave empty for unlimited"
              min="0"
              className="h-10 w-full mt-1"
            />
            <p className="text-xs text-gray-500 mt-1">
              How many times this offer can be applied per product in a single invoice
            </p>
          </div>

          <Alert className="border-green-200">
            <Info className="h-4 w-4 text-green-600" />
            <AlertDescription className="text-green-700">
              <strong>Single Product IQXF:</strong><br/>
              • Customer buys minimum quantity set in product selection<br/>
              • Gets {formData.freeQuantity || formData.getQty || 'Y'} units of the same product FREE<br/>
              • Can be applied {formData.maxDealCount || 'unlimited'} times per product in a single invoice
            </AlertDescription>
          </Alert>
        </>
      )}

      {/* BQXF Configuration */}
      {selectedFormat === "BQXF" && (
        <>
          <Alert className="border-blue-200">
            <Info className="h-4 w-4 text-blue-600" />
            <AlertDescription className="text-blue-700">
              <strong>Buy Quantity X Free (FOC) Configuration:</strong><br/>
              • Step 1: Select products to buy from the product selection above<br/>
              • Step 2: Set buy quantities (X) for each selected product<br/>
              • Step 3: Select free products (Y) below with their quantities
            </AlertDescription>
          </Alert>

          {/* FOC Product Selection - Using DynamicProductAttributes */}
          <div className="space-y-4">
            <h4 className="text-sm font-medium">Free Products (FOC) Configuration</h4>
            <p className="text-xs text-gray-500">
              Select products that customers will receive free when they meet the buy requirements.
            </p>
            
            {/* Use DynamicProductAttributes for FOC product selection */}
            <DynamicProductAttributes
              orgUid={orgUid}
              value={formData.focProductAttributes || []}
              onChange={(attributes) => {
                onFormDataChange("focProductAttributes", attributes);
              }}
              onFinalProductsChange={(products) => {
                // Store the selected FOC products with their quantities
                onFormDataChange("focSelectedProducts", products);
                
                // Initialize FOC products with quantities from product selection
                const updatedFocProducts = products.map(product => {
                  // Use quantity from product selection (added by DynamicProductAttributes)
                  const productQuantity = (product as any).quantity || 1;
                  return {
                    UID: product.UID,
                    ItemCode: product.Code,
                    Code: product.Code,
                    productId: product.UID,
                    productCode: product.Code,
                    productName: product.Name,
                    Name: product.Name,
                    quantity: productQuantity,
                    Quantity: productQuantity,
                    UOM: (product as any).UOM || null
                  };
                });
                onFormDataChange("focProducts", updatedFocProducts);
              }}
            />
          </div>

          {/* Application Limits */}
          <div className="space-y-4">
            <h4 className="text-sm font-medium">Application Limits</h4>
            
            <div>
              <Label htmlFor="maxApplicationsPerInvoice" className="text-sm font-medium text-gray-700 mb-1.5 block">
                Maximum Applications Per Invoice
              </Label>
              <Input
                id="maxApplicationsPerInvoice"
                type="number"
                value={formData.maxApplicationsPerInvoice || ''}
                onChange={(e) => {
                  onFormDataChange("maxApplicationsPerInvoice", e.target.value ? parseInt(e.target.value) : 0)
                }}
                min="1"
                placeholder="e.g., 5"
                className="h-10 w-full mt-1"
              />
              <p className="text-xs text-gray-500 mt-1">
                How many times this promotion can be applied within a single invoice
              </p>
            </div>

            <div>
              <Label htmlFor="maxDealCount2" className="text-sm font-medium text-gray-700 mb-1.5 block">Maximum Total Invoices</Label>
              <Input
                id="maxDealCount2"
                type="number"
                value={formData.maxDealCount || ''}
                onChange={(e) => {
                  onFormDataChange("maxDealCount", e.target.value ? parseInt(e.target.value) : 0)
                }}
                min="1"
                placeholder="e.g., 100"
                className="h-10 w-full mt-1"
              />
              <p className="text-xs text-gray-500 mt-1">
                Total number of invoices that can use this promotion (overall limit)
              </p>
            </div>
          </div>
        </>
      )}
    </div>
  );

  // Render Invoice Level configuration
  const renderInvoiceLevelConfiguration = () => {
    const getThresholdLabel = () => {
      switch (selectedFormat) {
        case "BYVALUE":
          return "Minimum Invoice Value (₹)";
        case "BYQTY":
          return "Minimum Total Quantity";
        case "LINECOUNT":
          return "Minimum Line Items";
        case "BRANDCOUNT":
          return "Minimum Brand Count";
        default:
          return "";
      }
    };

    return (
      <div className="space-y-4">
        {/* Info Box for Invoice Level Promotions */}
        <Alert className="border-blue-200">
          <Info className="h-4 w-4 text-blue-600" />
          <AlertDescription className="text-blue-700">
            <strong>Invoice Level Promotion:</strong><br/>
            This promotion will apply to the entire invoice when the configured threshold is met.
            {selectedFormat === "ANYVALUE" && " No minimum threshold required - applies to all invoices."}
          </AlertDescription>
        </Alert>
        {selectedFormat !== "ANYVALUE" && (
          <div>
            <Label htmlFor="threshold" className="text-sm font-medium text-gray-700 mb-1.5 block">
              {getThresholdLabel()} <span className="text-red-500">*</span>
            </Label>
            <Input
              id="threshold"
              type="number"
              value={
                selectedFormat === "BYVALUE"
                  ? (formData.minValue || '')
                  : selectedFormat === "BYQTY"
                  ? (formData.minQty || '')
                  : selectedFormat === "LINECOUNT"
                  ? (formData.minLineCount || '')
                  : selectedFormat === "BRANDCOUNT"
                  ? (formData.minBrandCount || '')
                  : ''
              }
              onChange={(e) => {
                const value = e.target.value ? parseFloat(e.target.value) : 0;
                const field =
                  selectedFormat === "BYVALUE"
                    ? "minValue"
                    : selectedFormat === "BYQTY"
                    ? "minQty"
                    : selectedFormat === "LINECOUNT"
                    ? "minLineCount"
                    : "minBrandCount";
                onFormDataChange(field, value);
              }}
              placeholder={
                selectedFormat === "BYVALUE"
                  ? "e.g., 1000"
                  : selectedFormat === "BYQTY"
                  ? "e.g., 10"
                  : selectedFormat === "LINECOUNT"
                  ? "e.g., 5"
                  : selectedFormat === "BRANDCOUNT"
                  ? "e.g., 3"
                  : "Enter minimum threshold"
              }
              min="0"
              step={selectedFormat === "BYVALUE" ? "0.01" : "1"}
              className="h-10 w-full mt-1"
            />
            <p className="text-xs text-gray-500 mt-1">
              {selectedFormat === "BYVALUE" && "Invoice total must be at least this amount"}
              {selectedFormat === "BYQTY" && "Total quantity of all items must be at least this amount"}
              {selectedFormat === "LINECOUNT" && "Invoice must have at least this many line items"}
              {selectedFormat === "BRANDCOUNT" && "Invoice must include products from at least this many brands"}
            </p>
          </div>
        )}

        {/* Offer Type Selection */}
        <div>
          <Label className="text-sm font-medium text-gray-700 mb-1.5 block">
            Offer Type <span className="text-red-500">*</span>
          </Label>
          <RadioGroup
            value={formData.offerType || "percentage"}
            onValueChange={(value) => {
              onFormDataChange("offerType", value);
              // Clear previous values when switching offer types
              if (value === "percentage") {
                onFormDataChange("discountAmount", 0);
                onFormDataChange("focSelectedProducts", []);
                onFormDataChange("focProducts", []);
              } else if (value === "value") {
                onFormDataChange("discountPercent", 0);
                onFormDataChange("focSelectedProducts", []);
                onFormDataChange("focProducts", []);
              } else if (value === "foc") {
                onFormDataChange("discountPercent", 0);
                onFormDataChange("discountAmount", 0);
              }
            }}
            className="mt-2"
          >
            <div className="flex items-center space-x-2">
              <RadioGroupItem value="percentage" id="percentage" />
              <Label htmlFor="percentage">Percentage</Label>
            </div>
            <div className="flex items-center space-x-2">
              <RadioGroupItem value="value" id="value" />
              <Label htmlFor="value">Fixed Value</Label>
            </div>
            <div className="flex items-center space-x-2">
              <RadioGroupItem value="foc" id="foc" />
              <Label htmlFor="foc">FOC Items</Label>
            </div>
          </RadioGroup>
        </div>

        {/* Offer Configuration based on type */}
        {formData.offerType === "percentage" && (
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor="discountPercent" className="text-sm font-medium text-gray-700 mb-1.5 block">
                Discount Percentage (%) <span className="text-red-500">*</span>
              </Label>
              <Input
                id="discountPercent"
                type="number"
                value={formData.discountPercent || ''}
                onChange={(e) =>
                  onFormDataChange(
                    "discountPercent",
                    e.target.value ? parseFloat(e.target.value) : 0
                  )
                }
                placeholder="Enter percentage"
                min="0"
                max="100"
                className="h-10 w-full mt-1"
              />
            </div>
            <div>
              <Label htmlFor="maxDiscountCap" className="text-sm font-medium text-gray-700 mb-1.5 block">Max Discount Cap (₹)</Label>
              <Input
                id="maxDiscountCap"
                type="number"
                value={formData.maxDiscountAmount || ''}
                onChange={(e) =>
                  onFormDataChange(
                    "maxDiscountAmount",
                    e.target.value ? parseFloat(e.target.value) : 0
                  )
                }
                placeholder="Optional"
                className="h-10 w-full mt-1"
              />
            </div>
          </div>
        )}

        {formData.offerType === "value" && (
          <div>
            <Label htmlFor="discountAmount2" className="text-sm font-medium text-gray-700 mb-1.5 block">
              Discount Amount (₹) <span className="text-red-500">*</span>
            </Label>
            <Input
              id="discountAmount2"
              type="number"
              value={formData.discountAmount || ''}
              onChange={(e) =>
                onFormDataChange("discountAmount", e.target.value ? parseFloat(e.target.value) : 0)
              }
              placeholder="Enter fixed discount amount"
              className="h-10 w-full mt-1"
            />
          </div>
        )}

        {formData.offerType === "foc" && (
          <div className="space-y-4">
            <div className="border border-gray-200 rounded-lg p-5 space-y-4">
              <h4 className="text-sm font-medium">Free Products (FOC) Configuration <span className="text-red-500">*</span></h4>
              <p className="text-xs text-gray-500">
                Select products that customers will receive free when the invoice meets the threshold.
              </p>
              
              {/* Use DynamicProductAttributes for FOC product selection */}
              <DynamicProductAttributes
                orgUid={orgUid}
                value={formData.focProductAttributes || []}
                onChange={(attributes) => {
                  onFormDataChange("focProductAttributes", attributes);
                }}
                onFinalProductsChange={(products) => {
                  // Store the selected FOC products with their quantities
                  onFormDataChange("focSelectedProducts", products);
                  
                  // Initialize FOC products with quantities from product selection
                  const updatedFocProducts = products.map(product => ({
                    ...product,
                    ItemCode: product.Code || product.ItemCode,
                    quantity: (product as any).quantity || 1,
                    Quantity: (product as any).quantity || 1
                  }));
                  onFormDataChange("focProducts", updatedFocProducts);
                }}
                showQuantityInput={true}
                minQuantity={1}
                title="Select Free Products"
              />
            </div>
          </div>
        )}
        
        {/* Maximum Applications field for invoice level */}
        <div>
          <Label htmlFor="maxDealCount" className="text-sm font-medium text-gray-700 mb-1.5 block">
            Maximum Applications per Invoice
          </Label>
          <Input
            id="maxDealCount"
            type="number"
            value={formData.maxDealCount || ''}
            onChange={(e) =>
              onFormDataChange("maxDealCount", e.target.value ? parseInt(e.target.value) : 1)
            }
            placeholder="Default: 1"
            min="1"
            className="h-10 w-full mt-1"
          />
          <p className="text-xs text-gray-500 mt-1">
            How many times this promotion can be applied in a single invoice (usually 1 for invoice level)
          </p>
        </div>
      </div>
    );
  };

  // Determine which configuration to render
  const renderConfigFields = () => {
    switch (selectedFormat) {
      case "IQFD":
        return renderSlabConfiguration("FIXED");
      case "IQPD":
        return renderSlabConfiguration("PERCENTAGE");
      case "IQXF":
      case "BQXF":
        return renderBuyXGetYConfiguration();
      case "MPROD":
        return null; // Multi-Product handled separately
      case "BYVALUE":
      case "BYQTY":
      case "LINECOUNT":
      case "BRANDCOUNT":
      case "ANYVALUE":
        return renderInvoiceLevelConfiguration();
      default:
        return null;
    }
  };

  return (
    <div className="max-w-[75%] space-y-4">
      <div className="mb-6">
        <h2 className="text-xl font-medium text-gray-900">Configuration</h2>
      </div>


      {/* Product Attributes - Only for instant level (not invoice level or MPROD) */}
      {selectedLevel === "instant" && selectedFormat !== "MPROD" && (
        <>
          {/* IQXF specific warning */}
          {selectedFormat === "IQXF" && (
            <Alert className="mb-4 border-amber-200">
              <AlertCircle className="h-4 w-4 text-amber-600" />
              <AlertDescription className="text-amber-700">
                <strong>Important: IQXF Single Product Requirement</strong><br/>
                For Item Quantity X Free promotions, you must select exactly ONE product. 
                The customer will buy X units of this product and get Y units free of the same product.
              </AlertDescription>
            </Alert>
          )}

          {/* Product Attributes with integrated selection modes */}
          <DynamicProductAttributes
            orgUid={orgUid}
            value={formData.productAttributes || []}
            onChange={(attributes) =>
              onFormDataChange("productAttributes", attributes)
            }
            onFinalProductsChange={(products) => {
              // Store final products for later use in promotion logic
              onFormDataChange("finalAttributeProducts", products);
              
              // Also update specificProducts if in specific mode
              if (formData.productSelectionMode === 'specific' || selectedFormat === 'IQXF' || selectedFormat === 'BQXF') {
                // Ensure products have proper structure for API
                const formattedProducts = products.map(product => ({
                  ...product,
                  ItemCode: product.Code,
                  quantity: (product as any).quantity || 1,
                  Quantity: (product as any).quantity || 1
                }));
                onFormDataChange("specificProducts", formattedProducts);
                
                // For IQXF, also set buyQuantity from the selected product
                if (selectedFormat === 'IQXF' && products.length > 0) {
                  const buyQty = (products[0] as any).quantity || 1;
                  onFormDataChange("buyQuantity", buyQty);
                }
              }
            }}
            onSelectionModeChange={(mode) => {
              // Store the selection mode in form data
              onFormDataChange("productSelectionMode", mode);
              
              // Clear product selections when changing mode
              if (mode === 'all') {
                onFormDataChange("specificProducts", []);
                onFormDataChange("finalAttributeProducts", []);
              }
            }}
          />

          {/* IQXF Note - Buy quantity should be configured in product selection */}
          {selectedFormat === "IQXF" && (
            <div className="border border-blue-200 bg-blue-50 rounded-lg p-4 mt-4">
              <p className="text-sm text-blue-800">
                <strong>Note:</strong> Configure the buy quantity directly in the product selection above. 
                The free quantity will be configured separately below.
              </p>
            </div>
          )}
        </>
      )}

      {/* Multi-Product Configuration */}
      {selectedLevel === "instant" && selectedFormat === "MPROD" && (
        <div className="border border-gray-200 rounded-lg p-5">
            <MultiProductConfiguration
              productPromotions={formData.productPromotions || []}
              onChange={(promotions) => onFormDataChange("productPromotions", promotions)}
              orgUid={orgUid || ""}
              selectedFormat={selectedFormat}
            />
        </div>
      )}

      {/* Configuration Fields */}
      {(selectedLevel === "invoice" || (selectedLevel === "instant" && selectedFormat !== "MPROD")) && (
        <div className="border border-gray-200 rounded-lg p-5">
          <h3 className="text-sm font-semibold text-gray-900 mb-4">Promotion Configuration</h3>
            {renderConfigFields()}
        </div>
      )}

      {/* Order Type for Slabs */}
      {formData.hasSlabs && selectedFormat !== "MPROD" && (
        <div className="border border-gray-200 rounded-lg p-5">
          <h3 className="text-sm font-semibold text-gray-900 mb-4">Order Type</h3>
            <p className="text-sm text-gray-600 mb-4">
              Select how the slab calculation should be applied
            </p>
            <RadioGroup
              value={formData.orderType || "LINE"}
              onValueChange={(value) => onFormDataChange("orderType", value)}
            >
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="LINE" id="line" />
                <Label htmlFor="line">
                  Line Level
                  <span className="text-gray-500 text-sm ml-2">
                    (Calculate per line item)
                  </span>
                </Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="INVOICE" id="invoice" />
                <Label htmlFor="invoice">
                  Invoice Level
                  <span className="text-gray-500 text-sm ml-2">
                    (Calculate for entire invoice)
                  </span>
                </Label>
              </div>
            </RadioGroup>
        </div>
      )}
    </div>
  );
}