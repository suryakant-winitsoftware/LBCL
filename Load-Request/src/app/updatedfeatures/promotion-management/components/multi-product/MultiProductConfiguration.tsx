"use client";

import React, { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import {
  Trash2,
  Plus,
  Minus,
  ChevronDown,
  ChevronUp,
  Package,
  ShoppingCart,
  Info,
  AlertCircle
} from "lucide-react";
import DynamicProductAttributes from "../product-selection/DynamicProductAttributes";
import { ProductPromotionConfig } from "../../types/promotionV3.types";

interface MultiProductConfigurationProps {
  productPromotions: ProductPromotionConfig[];
  onChange: (promotions: ProductPromotionConfig[]) => void;
  orgUid: string;
  selectedFormat: string;
}

export default function MultiProductConfiguration({
  productPromotions,
  onChange,
  orgUid,
  selectedFormat
}: MultiProductConfigurationProps) {
  const [expandedProducts, setExpandedProducts] = useState<
    Record<string, boolean>
  >({});

  const toggleProductExpansion = (productId: string) => {
    setExpandedProducts((prev) => ({
      ...prev,
      [productId]: !prev[productId]
    }));
  };

  const addProduct = () => {
    const tempId = `temp-${Date.now()}`;
    const newProduct: ProductPromotionConfig = {
      productId: tempId, // Temporary ID until product is selected
      productName: "Configuration " + (productPromotions.length + 1),
      productSelection: {
        selectionType: "specific",
        hierarchySelections: {},
        specificProducts: [],
        excludedProducts: []
      },
      quantityThreshold: 1,
      promotionType: undefined,
      isActive: true,
      hasSlabs: false
    };
    onChange([...productPromotions, newProduct]);
    setExpandedProducts((prev) => ({ ...prev, [tempId]: true }));
  };

  const removeProduct = (productId: string) => {
    onChange(productPromotions.filter((p) => p.productId !== productId));
    setExpandedProducts((prev) => {
      const newExpanded = { ...prev };
      delete newExpanded[productId];
      return newExpanded;
    });
  };

  const updateProduct = (
    productId: string,
    updates: Partial<ProductPromotionConfig>
  ) => {
    onChange(
      productPromotions.map((p) =>
        p.productId === productId ? { ...p, ...updates } : p
      )
    );
  };

  const renderSlabConfiguration = (product: ProductPromotionConfig) => {
    const promotionType = product.promotionType || "IQPD";

    return (
      <div className="space-y-4">
        <div className="flex items-center gap-3 p-3  rounded-lg">
          <input
            type="checkbox"
            id={`slabs-${product.productId}`}
            checked={product.hasSlabs || false}
            onChange={(e) => {
              updateProduct(product.productId, {
                hasSlabs: e.target.checked,
                slabs:
                  e.target.checked &&
                  (!product.slabs || product.slabs.length === 0)
                    ? [
                        {
                          slabNo: 1,
                          minQty: 1,
                          maxQty: 999999,
                          offerType:
                            promotionType === "IQPD"
                              ? "PERCENTAGE"
                              : promotionType === "IQFD"
                              ? "FIXED"
                              : "FOC",
                          ...(promotionType === "IQPD"
                            ? { discountPercent: 0 }
                            : {}),
                          ...(promotionType === "IQFD"
                            ? { discountAmount: 0 }
                            : {}),
                          ...(promotionType === "IQXF" ||
                          promotionType === "BQXF"
                            ? { focItems: [] }
                            : {})
                        }
                      ]
                    : product.slabs
              });
            }}
            className="text-purple-600"
          />
          <label
            htmlFor={`slabs-${product.productId}`}
            className="font-medium cursor-pointer"
          >
            Enable Quantity-based Slabs/Tiers
          </label>
        </div>

        {product.hasSlabs && product.slabs ? (
          <div className="space-y-3">
            <div className="flex justify-between items-center">
              <h5 className="text-sm font-medium">Quantity Slabs</h5>
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  const newSlab = {
                    slabNo: (product.slabs?.length || 0) + 1,
                    minQty:
                      product.slabs?.[product.slabs.length - 1]?.maxQty || 1,
                    maxQty: 999999,
                    offerType: (promotionType === "IQPD"
                      ? "PERCENTAGE"
                      : promotionType === "IQFD"
                      ? "FIXED"
                      : "FOC") as "PERCENTAGE" | "FIXED" | "FOC",
                    ...(promotionType === "IQPD" ? { discountPercent: 0 } : {}),
                    ...(promotionType === "IQFD" ? { discountAmount: 0 } : {}),
                    ...(promotionType === "IQXF" || promotionType === "BQXF"
                      ? { focItems: [] }
                      : {})
                  };
                  updateProduct(product.productId, {
                    slabs: [...(product.slabs || []), newSlab]
                  });
                }}
              >
                <Plus className="w-3 h-3 mr-1" />
                Add Slab
              </Button>
            </div>

            {product.slabs.map((slab, index) => (
              <div
                key={index}
                className="border border-gray-200 rounded-lg p-3"
              >
                <div className="grid grid-cols-4 gap-3">
                  <div>
                    <label className="text-xs text-gray-600">Min Qty</label>
                    <Input
                      type="number"
                      value={slab.minQty || ""}
                      onChange={(e) => {
                        const newSlabs = [...(product.slabs || [])];
                        newSlabs[index].minQty = e.target.value
                          ? parseInt(e.target.value)
                          : 1;
                        updateProduct(product.productId, { slabs: newSlabs });
                      }}
                      onFocus={(e) => {
                        if (e.target.value === "1") {
                          e.target.value = "";
                        }
                      }}
                      min="1"
                      className="h-10 w-full mt-1"
                    />
                  </div>
                  <div>
                    <label className="text-xs text-gray-600">Max Qty</label>
                    <Input
                      type="number"
                      value={slab.maxQty || ""}
                      onChange={(e) => {
                        const newSlabs = [...(product.slabs || [])];
                        newSlabs[index].maxQty = e.target.value
                          ? parseInt(e.target.value)
                          : 999999;
                        updateProduct(product.productId, { slabs: newSlabs });
                      }}
                      onFocus={(e) => {
                        if (e.target.value === "999999") {
                          e.target.value = "";
                        }
                      }}
                      className="h-10 w-full mt-1"
                    />
                  </div>
                  <div>
                    <label className="text-xs text-gray-600">
                      {promotionType === "IQPD"
                        ? "Discount %"
                        : promotionType === "IQFD"
                        ? "Discount ₹"
                        : "Free Units"}
                    </label>
                    <Input
                      type="number"
                      value={
                        promotionType === "IQPD"
                          ? slab.discountPercent || ""
                          : promotionType === "IQFD"
                          ? slab.discountAmount || ""
                          : slab.focItems?.[0]?.qty || ""
                      }
                      onChange={(e) => {
                        const newSlabs = [...(product.slabs || [])];
                        const value = e.target.value
                          ? parseFloat(e.target.value)
                          : 0;
                        if (promotionType === "IQPD") {
                          newSlabs[index].discountPercent = value;
                        } else if (promotionType === "IQFD") {
                          newSlabs[index].discountAmount = value;
                        } else {
                          newSlabs[index].focItems = [
                            {
                              itemCode: "",
                              itemName: "",
                              qty: value,
                              uom: "PCS"
                            }
                          ];
                        }
                        updateProduct(product.productId, { slabs: newSlabs });
                      }}
                      onFocus={(e) => {
                        if (e.target.value === "0") {
                          e.target.value = "";
                        }
                      }}
                      {...(promotionType === "IQPD" && {
                        min: "0",
                        max: "100"
                      })}
                      className="h-10 w-full mt-1"
                    />
                  </div>
                  <div className="flex items-end">
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => {
                        const newSlabs = product.slabs?.filter(
                          (_, i) => i !== index
                        );
                        updateProduct(product.productId, { slabs: newSlabs });
                      }}
                      className="text-red-600 hover:text-red-700"
                    >
                      Remove
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        ) : (
          renderSimpleDiscountConfiguration(product)
        )}
      </div>
    );
  };

  const renderSimpleDiscountConfiguration = (
    product: ProductPromotionConfig
  ) => {
    const promotionType = product.promotionType || "IQPD";

    switch (promotionType) {
      case "IQPD":
        return (
          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label htmlFor={`discount-percent-${product.productId}`}>
                Discount Percentage (%)
              </Label>
              <Input
                id={`discount-percent-${product.productId}`}
                type="number"
                value={product.discountPercent || ""}
                onChange={(e) =>
                  updateProduct(product.productId, {
                    discountPercent: e.target.value
                      ? parseFloat(e.target.value)
                      : 0
                  })
                }
                onFocus={(e) => {
                  if (e.target.value === "0") {
                    e.target.value = "";
                  }
                }}
                min="0"
                max="100"
                className="w-full"
              />
            </div>
            <div>
              <Label htmlFor={`max-discount-${product.productId}`}>
                Max Discount Cap (₹)
              </Label>
              <Input
                id={`max-discount-${product.productId}`}
                type="number"
                value={product.maxDiscountAmount || ""}
                onChange={(e) =>
                  updateProduct(product.productId, {
                    maxDiscountAmount: e.target.value
                      ? parseFloat(e.target.value)
                      : undefined
                  })
                }
                onFocus={(e) => {
                  if (e.target.value === "0") {
                    e.target.value = "";
                  }
                }}
                placeholder="Optional"
                className="w-full"
              />
            </div>
          </div>
        );

      case "IQFD":
        return (
          <div>
            <Label htmlFor={`fixed-discount-${product.productId}`}>
              Fixed Discount Amount (₹)
            </Label>
            <Input
              id={`fixed-discount-${product.productId}`}
              type="number"
              value={product.discountAmount || ""}
              onChange={(e) =>
                updateProduct(product.productId, {
                  discountAmount: e.target.value
                    ? parseFloat(e.target.value)
                    : 0
                })
              }
              onFocus={(e) => {
                if (e.target.value === "0") {
                  e.target.value = "";
                }
              }}
              className="w-full"
            />
          </div>
        );

      case "IQXF":
      case "BQXF":
        return null;

      default:
        return null;
    }
  };

  return (
    <div className="space-y-4">
      {/* Header Section */}
      <div className="mb-6">
        <h3 className="text-lg font-semibold text-gray-900">
          Multi-Product Configuration
        </h3>
        <p className="text-sm text-gray-600 mt-1">
          Apply different promotion types to different product groups in a
          single promotion
        </p>
      </div>

      {/* Empty State */}
      {productPromotions.length === 0 && (
        <div className="border border-gray-200 rounded-lg p-6">
          <div className="text-center">
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-gray-100 mb-3">
              <Package className="w-6 h-6 text-gray-600" />
            </div>
            <h3 className="text-sm font-medium text-gray-900 mb-1">
              No Product Configurations
            </h3>
            <p className="text-xs text-gray-500 mb-4">
              Add product configurations to create multi-product promotions
            </p>
            <Button onClick={addProduct} variant="outline" size="sm">
              <Plus className="w-4 h-4 mr-2" />
              Add Product Configuration
            </Button>
          </div>
        </div>
      )}

      {/* Product Configurations */}
      {productPromotions.length > 0 && (
        <>
          {productPromotions.map((product, index) => (
            <div
              key={product.productId}
              className="border border-gray-200 rounded-lg overflow-hidden"
            >
              <div
                className="p-4 cursor-pointer flex justify-between items-center hover:bg-gray-50"
                onClick={() => toggleProductExpansion(product.productId)}
              >
                <div className="flex items-center gap-4">
                  <span className="text-sm font-medium text-gray-600">
                    Product {index + 1}
                  </span>
                  <h4 className="font-medium">
                    {product.selectedProducts &&
                    product.selectedProducts.length > 0
                      ? `${product.selectedProducts.length} Product${
                          product.selectedProducts.length > 1 ? "s" : ""
                        } Selected`
                      : "No Products Selected"}
                  </h4>
                  <div className="flex gap-2">
                    {(product.promotionType === "IQFD" ||
                      product.promotionType === "IQPD") && (
                      <Badge variant="secondary" className="text-xs">
                        Min Qty: {product.quantityThreshold || 1}
                      </Badge>
                    )}
                    {product.promotionType === "IQXF" && (
                      <Badge variant="secondary" className="text-xs">
                        Get: {product.freeQty || 1} Free
                      </Badge>
                    )}
                    {product.promotionType === "BQXF" && (
                      <Badge variant="secondary" className="text-xs">
                        Uses Product Qtys
                      </Badge>
                    )}
                    <Badge variant="default" className="text-xs">
                      {product.promotionType === "IQPD" &&
                        `${product.discountPercent || 0}% discount`}
                      {product.promotionType === "IQFD" &&
                        `₹${product.discountAmount || 0} off`}
                      {product.promotionType === "IQXF" &&
                        `Buy X get ${product.freeQty || 1} free`}
                      {product.promotionType === "BQXF" &&
                        `Buy X get ${product.focProducts?.length || 0} FOC`}
                      {!product.promotionType && "Not configured"}
                    </Badge>
                    {product.hasSlabs && (
                      <Badge variant="outline" className="text-xs">
                        {product.slabs?.length || 0} slabs
                      </Badge>
                    )}
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={(e) => {
                      e.stopPropagation();
                      removeProduct(product.productId);
                    }}
                    className="text-red-600 hover:text-red-700"
                  >
                    <Trash2 className="w-4 h-4" />
                  </Button>
                  {expandedProducts[product.productId] ? (
                    <ChevronUp className="w-5 h-5 text-gray-600" />
                  ) : (
                    <ChevronDown className="w-5 h-5 text-gray-600" />
                  )}
                </div>
              </div>

              {expandedProducts[product.productId] && (
                <div className="p-5 space-y-4 border-t border-gray-200">
                  {/* Configuration Name */}
                  <div>
                    <Label
                      htmlFor={`config-name-${product.productId}`}
                      className="text-sm font-medium text-gray-700 mb-1.5 block"
                    >
                      Configuration Name
                    </Label>
                    <Input
                      id={`config-name-${product.productId}`}
                      type="text"
                      value={product.productName}
                      onChange={(e) =>
                        updateProduct(product.productId, {
                          productName: e.target.value
                        })
                      }
                      className="h-10 w-full mt-1"
                    />
                  </div>

                  {/* Product Selection - Allow multiple products like individual promotions */}
                  <div className="space-y-3">
                    <Label>
                      Product Selection <span className="text-red-500">*</span>
                    </Label>
                    <p className="text-xs text-gray-600">
                      Select products that will receive{" "}
                      {product.promotionType
                        ? product.promotionType === "IQFD"
                          ? "fixed discount"
                          : product.promotionType === "IQPD"
                          ? "percentage discount"
                          : product.promotionType === "IQXF"
                          ? "buy X get Y free offer"
                          : product.promotionType === "BQXF"
                          ? "buy X get different item free"
                          : "this promotion"
                        : "this promotion"}
                    </p>

                    <DynamicProductAttributes
                      orgUid={orgUid}
                      value={product.productAttributes || []}
                      onChange={(attributes) => {
                        updateProduct(product.productId, {
                          productAttributes: attributes
                        });
                      }}
                      onFinalProductsChange={(finalProducts) => {
                        // Update with all selected products
                        if (finalProducts && finalProducts.length > 0) {
                          // Store all selected products
                          updateProduct(product.productId, {
                            selectedProducts: finalProducts,
                            productSelection: {
                              ...product.productSelection,
                              selectionType: "specific",
                              selectedFinalProducts: finalProducts,
                              specificProducts: finalProducts
                            }
                          });
                        }
                      }}
                      disabled={false}
                    />

                    {/* Show selected products count */}
                    {product.selectedProducts &&
                      product.selectedProducts.length > 0 && (
                        <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
                          <p className="text-sm font-medium text-green-900">
                            {product.selectedProducts.length} Product
                            {product.selectedProducts.length > 1 ? "s" : ""}{" "}
                            Selected
                          </p>
                          <div className="mt-2 space-y-1 max-h-32 overflow-y-auto">
                            {product.selectedProducts
                              .slice(0, 5)
                              .map((p: any, idx: number) => (
                                <p key={idx} className="text-xs text-green-700">
                                  • {p.Name} ({p.Code})
                                </p>
                              ))}
                            {product.selectedProducts.length > 5 && (
                              <p className="text-xs text-green-600 font-medium">
                                ...and {product.selectedProducts.length - 5}{" "}
                                more
                              </p>
                            )}
                          </div>
                        </div>
                      )}
                  </div>

                  {/* Promotion Type Selection */}
                  <div>
                    <Label className="text-sm font-medium text-gray-700 mb-1.5 block">
                      Promotion Type <span className="text-red-500">*</span>
                    </Label>
                    <div className="grid grid-cols-2 gap-3 mt-1">
                      <label className="flex items-center gap-2 p-3 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <input
                          type="radio"
                          name={`promotionType-${product.productId}`}
                          value="IQFD"
                          checked={product.promotionType === "IQFD"}
                          onChange={(e) => {
                            updateProduct(product.productId, {
                              promotionType: e.target.value as
                                | "IQFD"
                                | "IQPD"
                                | "IQXF"
                                | "BQXF",
                              discountAmount: 0,
                              discountPercent: undefined,
                              buyQty: undefined,
                              freeQty: undefined
                            });
                          }}
                          className="text-purple-600"
                        />
                        <div>
                          <span className="text-sm font-medium">
                            Item Fixed Discount
                          </span>
                          <p className="text-xs text-gray-600">
                            Fixed amount off per item
                          </p>
                        </div>
                      </label>

                      <label className="flex items-center gap-2 p-3 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <input
                          type="radio"
                          name={`promotionType-${product.productId}`}
                          value="IQPD"
                          checked={product.promotionType === "IQPD"}
                          onChange={(e) => {
                            updateProduct(product.productId, {
                              promotionType: e.target.value as
                                | "IQFD"
                                | "IQPD"
                                | "IQXF"
                                | "BQXF",
                              discountPercent: 0,
                              discountAmount: undefined,
                              buyQty: undefined,
                              freeQty: undefined
                            });
                          }}
                          className="text-purple-600"
                        />
                        <div>
                          <span className="text-sm font-medium">
                            Item Percentage Discount
                          </span>
                          <p className="text-xs text-gray-600">
                            Percentage off per item
                          </p>
                        </div>
                      </label>

                      <label className="flex items-center gap-2 p-3 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <input
                          type="radio"
                          name={`promotionType-${product.productId}`}
                          value="IQXF"
                          checked={product.promotionType === "IQXF"}
                          onChange={(e) => {
                            updateProduct(product.productId, {
                              promotionType: e.target.value as
                                | "IQFD"
                                | "IQPD"
                                | "IQXF"
                                | "BQXF",
                              buyQty: 1,
                              freeQty: 1,
                              discountAmount: undefined,
                              discountPercent: undefined
                            });
                          }}
                          className="text-purple-600"
                        />
                        <div>
                          <span className="text-sm font-medium">
                            Item Quantity X Free
                          </span>
                          <p className="text-xs text-gray-600">
                            Buy X units, get Y free
                          </p>
                        </div>
                      </label>

                      <label className="flex items-center gap-2 p-3 border rounded-lg cursor-pointer hover:bg-gray-50">
                        <input
                          type="radio"
                          name={`promotionType-${product.productId}`}
                          value="BQXF"
                          checked={product.promotionType === "BQXF"}
                          onChange={(e) => {
                            updateProduct(product.productId, {
                              promotionType: e.target.value as
                                | "IQFD"
                                | "IQPD"
                                | "IQXF"
                                | "BQXF",
                              buyQty: 1,
                              freeQty: 1,
                              focProducts: [],
                              focProductQuantities: {},
                              focProductSelection: {
                                selectionType: "specific",
                                hierarchySelections: {},
                                specificProducts: [],
                                excludedProducts: [],
                                selectedFinalProducts: []
                              },
                              discountAmount: undefined,
                              discountPercent: undefined
                            });
                          }}
                          className="text-purple-600"
                        />
                        <div>
                          <span className="text-sm font-medium">
                            Buy Quantity X Free (FOC)
                          </span>
                          <p className="text-xs text-gray-600">
                            Buy X quantity, get FOC items
                          </p>
                        </div>
                      </label>
                    </div>
                  </div>

                  {/* Quantity Threshold */}
                  {product.promotionType !== "IQXF" &&
                    product.promotionType !== "BQXF" && (
                      <div>
                        <Label htmlFor={`min-qty-${product.productId}`}>
                          Minimum Quantity Required
                        </Label>
                        <Input
                          id={`min-qty-${product.productId}`}
                          type="number"
                          value={product.quantityThreshold || ""}
                          onChange={(e) =>
                            updateProduct(product.productId, {
                              quantityThreshold: e.target.value
                                ? parseInt(e.target.value)
                                : 1
                            })
                          }
                          onFocus={(e) => {
                            if (e.target.value === "1") {
                              e.target.value = "";
                            }
                          }}
                          min="1"
                          placeholder="Enter minimum quantity"
                          className="h-10 w-full mt-1"
                        />
                        <p className="text-xs text-gray-600 mt-1">
                          Customer must purchase at least this quantity to
                          trigger the promotion
                        </p>
                      </div>
                    )}

                  {/* IQXF Configuration */}
                  {product.promotionType === "IQXF" && (
                    <div className="space-y-4 p-4 bg-blue-50 rounded-lg">
                      <h4 className="text-sm font-medium text-gray-700">
                        Buy X Get Y Free Configuration
                      </h4>
                      <p className="text-xs text-gray-600">
                        Buy quantity is set per product in selection. Configure
                        free quantity below.
                      </p>
                      <div>
                        <Label htmlFor={`free-qty-${product.productId}`}>
                          Free Quantity <span className="text-red-500">*</span>
                        </Label>
                        <Input
                          id={`free-qty-${product.productId}`}
                          type="number"
                          value={product.freeQty || product.getQty || ""}
                          onChange={(e) => {
                            const value = e.target.value
                              ? parseInt(e.target.value)
                              : 0;
                            updateProduct(product.productId, {
                              freeQty: value,
                              getQty: value // For compatibility
                            });
                          }}
                          placeholder="e.g., 1"
                          min="1"
                          className="h-10 w-full mt-1"
                        />
                        <p className="text-xs text-gray-500 mt-1">
                          Number of free units customer gets for each qualifying
                          purchase
                        </p>
                      </div>
                    </div>
                  )}

                  {/* BQXF Configuration */}
                  {product.promotionType === "BQXF" && (
                    <div className="space-y-4 p-4 bg-green-50 rounded-lg">
                      <h4 className="text-sm font-medium text-gray-700">
                        Free Products (FOC) Configuration
                      </h4>
                      <p className="text-xs text-gray-600">
                        Select products to give free when buy requirements are
                        met.
                      </p>

                      {/* Use DynamicProductAttributes for FOC product selection */}
                      <DynamicProductAttributes
                        orgUid={orgUid}
                        value={product.focProductAttributes || []}
                        onChange={(attributes) => {
                          updateProduct(product.productId, {
                            focProductAttributes: attributes
                          });
                        }}
                        onFinalProductsChange={(products) => {
                          // Store the selected FOC products
                          updateProduct(product.productId, {
                            focSelectedProducts: products,
                            focProductSelection: {
                              ...product.focProductSelection,
                              selectedFinalProducts: products
                            }
                          });

                          // Initialize FOC products with default quantities if not already set
                          const existingFocProducts = product.focProducts || [];
                          const updatedFocProducts = products.map((p) => {
                            const existing = existingFocProducts.find(
                              (foc: any) => foc.productId === p.UID
                            );
                            return (
                              existing || {
                                productId: p.UID,
                                productCode: p.Code,
                                productName: p.Name,
                                quantity: 1
                              }
                            );
                          });
                          updateProduct(product.productId, {
                            focProducts: updatedFocProducts
                          });
                        }}
                      />

                      {/* Show selected FOC products with quantity controls */}
                      {product.focSelectedProducts &&
                        product.focSelectedProducts.length > 0 && (
                          <div className="border border-gray-200 rounded-lg mt-4">
                            <div className="p-4">
                              <h5 className="text-sm font-medium mb-3">
                                FOC Product Quantities
                              </h5>
                              <p className="text-xs text-gray-500 mb-4">
                                Set the quantity for each free product that
                                customers will receive
                              </p>

                              <div className="space-y-2">
                                {product.focSelectedProducts.map(
                                  (focProduct: any, index: number) => (
                                    <div
                                      key={focProduct.UID}
                                      className="flex items-center justify-between p-3 border rounded-lg"
                                    >
                                      <div className="flex-1">
                                        <div className="font-medium text-sm">
                                          {focProduct.Name}
                                        </div>
                                        <div className="text-xs text-gray-500">
                                          {focProduct.Code}
                                        </div>
                                      </div>

                                      <div className="flex items-center gap-2">
                                        <Label className="text-xs text-gray-600 mr-2">
                                          Qty:
                                        </Label>
                                        <Button
                                          type="button"
                                          variant="outline"
                                          size="sm"
                                          onClick={() => {
                                            const updatedProducts = [
                                              ...(product.focProducts || [])
                                            ];
                                            const productIndex =
                                              updatedProducts.findIndex(
                                                (p) =>
                                                  p.productId === focProduct.UID
                                              );
                                            if (productIndex >= 0) {
                                              const currentQty =
                                                updatedProducts[productIndex]
                                                  .quantity || 1;
                                              if (currentQty > 1) {
                                                updatedProducts[
                                                  productIndex
                                                ].quantity = currentQty - 1;
                                                updateProduct(
                                                  product.productId,
                                                  {
                                                    focProducts: updatedProducts
                                                  }
                                                );
                                              }
                                            }
                                          }}
                                          className="h-8 w-8 p-0"
                                        >
                                          <Minus className="w-3 h-3" />
                                        </Button>

                                        <Input
                                          type="number"
                                          value={
                                            product.focProducts?.find(
                                              (p: any) =>
                                                p.productId === focProduct.UID
                                            )?.quantity || 1
                                          }
                                          onChange={(e) => {
                                            const updatedProducts = [
                                              ...(product.focProducts || [])
                                            ];
                                            const productIndex =
                                              updatedProducts.findIndex(
                                                (p) =>
                                                  p.productId === focProduct.UID
                                              );
                                            if (productIndex >= 0) {
                                              updatedProducts[
                                                productIndex
                                              ].quantity =
                                                parseInt(e.target.value) || 1;
                                              updateProduct(product.productId, {
                                                focProducts: updatedProducts
                                              });
                                            }
                                          }}
                                          min="1"
                                          className="w-16 text-center"
                                        />

                                        <Button
                                          type="button"
                                          variant="outline"
                                          size="sm"
                                          onClick={() => {
                                            const updatedProducts = [
                                              ...(product.focProducts || [])
                                            ];
                                            const productIndex =
                                              updatedProducts.findIndex(
                                                (p) =>
                                                  p.productId === focProduct.UID
                                              );
                                            if (productIndex >= 0) {
                                              const currentQty =
                                                updatedProducts[productIndex]
                                                  .quantity || 1;
                                              updatedProducts[
                                                productIndex
                                              ].quantity = currentQty + 1;
                                              updateProduct(product.productId, {
                                                focProducts: updatedProducts
                                              });
                                            }
                                          }}
                                          className="h-8 w-8 p-0"
                                        >
                                          <Plus className="w-3 h-3" />
                                        </Button>

                                        <div className="flex gap-1 ml-2">
                                          {[1, 2, 5, 10].map((qty) => (
                                            <Button
                                              key={qty}
                                              type="button"
                                              variant={
                                                product.focProducts?.find(
                                                  (p: any) =>
                                                    p.productId ===
                                                    focProduct.UID
                                                )?.quantity === qty
                                                  ? "default"
                                                  : "outline"
                                              }
                                              size="sm"
                                              onClick={() => {
                                                const updatedProducts = [
                                                  ...(product.focProducts || [])
                                                ];
                                                const productIndex =
                                                  updatedProducts.findIndex(
                                                    (p) =>
                                                      p.productId ===
                                                      focProduct.UID
                                                  );
                                                if (productIndex >= 0) {
                                                  updatedProducts[
                                                    productIndex
                                                  ].quantity = qty;
                                                  updateProduct(
                                                    product.productId,
                                                    {
                                                      focProducts:
                                                        updatedProducts
                                                    }
                                                  );
                                                }
                                              }}
                                              className="h-8 px-2 text-xs"
                                            >
                                              {qty}
                                            </Button>
                                          ))}
                                        </div>
                                      </div>
                                    </div>
                                  )
                                )}
                              </div>
                            </div>
                          </div>
                        )}
                    </div>
                  )}

                  {/* Discount Configuration */}
                  {renderSlabConfiguration(product)}

                  {/* Active Status */}
                  <div className="flex items-center gap-3 p-4  rounded-lg">
                    <input
                      type="checkbox"
                      id={`active-${product.productId}`}
                      checked={product.isActive}
                      onChange={(e) =>
                        updateProduct(product.productId, {
                          isActive: e.target.checked
                        })
                      }
                      className="text-purple-600"
                    />
                    <label
                      htmlFor={`active-${product.productId}`}
                      className="font-medium cursor-pointer"
                    >
                      Active Configuration
                    </label>
                  </div>
                </div>
              )}
            </div>
          ))}

          {/* Add Product Button */}
          <div className="mt-4 text-center">
            <Button
              variant="outline"
              onClick={addProduct}
              className="flex items-center gap-2 mx-auto"
            >
              <Plus className="w-4 h-4" />
              Add Another Product Configuration
            </Button>
          </div>

          {/* Configuration Summary */}
          {productPromotions.length > 0 && (
            <div className="border border-gray-200 rounded-lg p-5 mt-4">
              <h3 className="text-sm font-semibold text-gray-900 mb-4">
                Configuration Summary
              </h3>
              <div className="space-y-3">
                {productPromotions.map((product, index) => {
                  const getPromotionTypeName = (type: string) => {
                    switch (type) {
                      case "IQFD":
                        return "Item Fixed Discount";
                      case "IQPD":
                        return "Item Percentage Discount";
                      case "IQXF":
                        return "Item Quantity X Free";
                      case "BQXF":
                        return "Buy Quantity X Free (FOC)";
                      default:
                        return "Not Configured";
                    }
                  };

                  const getPromotionRule = (product: any) => {
                    switch (product.promotionType) {
                      case "IQFD":
                        return `Buy ${
                          product.quantityThreshold || 1
                        }+ units → Get ₹${
                          product.discountAmount || 0
                        } off per item`;
                      case "IQPD":
                        return `Buy ${
                          product.quantityThreshold || 1
                        }+ units → Get ${
                          product.discountPercent || 0
                        }% discount${
                          product.maxDiscountAmount
                            ? ` (max ₹${product.maxDiscountAmount})`
                            : ""
                        }`;
                      case "IQXF":
                        return `Buy X units → Get ${product.freeQty || 1} unit${
                          (product.freeQty || 1) > 1 ? "s" : ""
                        } free`;
                      case "BQXF":
                        const focCount = product.focProducts?.length || 0;
                        return `Buy configured quantities → Get ${focCount} FOC product${
                          focCount !== 1 ? "s" : ""
                        }`;
                      default:
                        return "Configuration incomplete";
                    }
                  };

                  return (
                    <div
                      key={product.productId}
                      className="flex items-start gap-4 p-4 border border-gray-200 rounded-lg"
                    >
                      <div className="flex-shrink-0 w-8 h-8 bg-gray-600 text-white rounded-full flex items-center justify-center text-sm font-medium">
                        {index + 1}
                      </div>
                      <div className="flex-1 space-y-2">
                        <div className="flex items-center justify-between">
                          <h4 className="font-medium text-gray-900">
                            {product.productName}
                          </h4>
                          <div className="flex gap-2">
                            <Badge
                              variant={
                                product.isActive ? "default" : "secondary"
                              }
                            >
                              {product.isActive ? "Active" : "Inactive"}
                            </Badge>
                            {product.hasSlabs && (
                              <Badge variant="outline">
                                {product.slabs?.length || 0} Slabs
                              </Badge>
                            )}
                          </div>
                        </div>
                        <div className="text-sm text-gray-600">
                          <span className="font-medium">Type:</span>{" "}
                          {getPromotionTypeName(product.promotionType || "")}
                        </div>
                        <div className="text-sm text-blue-700 font-medium">
                          <span className="font-medium">Rule:</span>{" "}
                          {getPromotionRule(product)}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Summary Stats */}
              <div className="mt-6 pt-4 border-t border-blue-200">
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-center">
                  <div>
                    <div className="text-2xl font-bold text-blue-900">
                      {productPromotions.length}
                    </div>
                    <div className="text-sm text-blue-700">Total Products</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-blue-900">
                      {productPromotions.filter((p) => p.isActive).length}
                    </div>
                    <div className="text-sm text-blue-700">Active Configs</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-blue-900">
                      {productPromotions.filter((p) => p.hasSlabs).length}
                    </div>
                    <div className="text-sm text-blue-700">With Slabs</div>
                  </div>
                  <div>
                    <div className="text-2xl font-bold text-blue-900">
                      {
                        new Set(productPromotions.map((p) => p.promotionType))
                          .size
                      }
                    </div>
                    <div className="text-sm text-blue-700">Different Types</div>
                  </div>
                </div>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
