"use client";

import React from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Separator } from "@/components/ui/separator";
import { 
  CheckCircle, 
  AlertCircle, 
  Calendar,
  MapPin,
  Package,
  DollarSign,
  Shield,
  Info,
  Tag,
  FileText,
  Settings
} from "lucide-react";
import { PromotionV3FormData } from "../../../types/promotionV3.types";
import { PromotionLevel } from "../../../utils/promotionConfig";

interface ReviewStepProps {
  formData: PromotionV3FormData;
  selectedLevel: string;
  selectedFormat: string;
  promotionLevels: PromotionLevel[];
}

export default function ReviewStep({
  formData,
  selectedLevel,
  selectedFormat,
  promotionLevels
}: ReviewStepProps) {
  const getPromotionType = () => {
    const level = promotionLevels.find(l => l.id === selectedLevel);
    const format = level?.formats.find(f => f.value === selectedFormat);
    return {
      level: level?.name || '',
      format: format?.label || ''
    };
  };

  const { level, format } = getPromotionType();

  const getConfigurationSummary = () => {
    switch (selectedFormat) {
      case 'MPROD':
        if (formData.productPromotions && formData.productPromotions.length > 0) {
          const activePromos = formData.productPromotions.filter(p => p.isActive);
          const details = activePromos.map(p => {
            if (p.promotionType === 'IQXF') {
              return `${p.productName}: Buy ${p.buyQty || 'X'} Get ${p.getQty || 'Y'} Free`;
            } else if (p.promotionType === 'BQXF') {
              const focCount = p.focProducts?.length || 0;
              return `${p.productName}: Buy configured qty, Get ${focCount} FOC items`;
            } else if (p.promotionType === 'IQPD') {
              return `${p.productName}: ${p.discountPercentage || 0}% discount`;
            } else if (p.promotionType === 'IQFD') {
              return `${p.productName}: ₹${p.discountValue || 0} off`;
            }
            return `${p.productName}: Not configured`;
          });
          return details.join(' | ');
        }
        return 'No product configurations';
      
      case 'IQFD':
        if (formData.hasSlabs) {
          return `${formData.slabs?.length || 0} quantity slabs configured`;
        }
        return `Fixed discount: ₹${formData.discountAmount || 0}`;
      
      case 'IQPD':
        if (formData.hasSlabs) {
          return `${formData.slabs?.length || 0} quantity slabs configured`;
        }
        return `Percentage discount: ${formData.discountPercent || 0}%${
          formData.maxDiscountAmount ? ` (max ₹${formData.maxDiscountAmount})` : ''
        }`;
      
      case 'IQXF':
        // IQXF requires exactly one product with buy quantity from product selection
        if (formData.finalAttributeProducts?.length === 1) {
          const product = formData.finalAttributeProducts[0];
          const buyQuantity = (product as any).quantity || formData.minQty || 0;
          const freeQuantity = formData.getQty || 0;
          const productName = product.Name || product.name || 'selected product';
          
          return `Buy ${buyQuantity} units of ${productName}, Get ${freeQuantity} units FREE`;
        } else if (formData.finalAttributeProducts?.length > 1) {
          return `Error: IQXF requires exactly one product (${formData.finalAttributeProducts.length} selected)`;
        } else {
          return `No product selected - Buy X Get ${formData.getQty || 'Y'} Free`;
        }
      
      case 'BQXF':
        const buyProducts = formData.finalAttributeProducts || [];
        const focProducts = formData.focSelectedProducts || formData.focProducts || [];
        
        if (buyProducts.length > 0 && focProducts.length > 0) {
          const buyDetails = buyProducts.map((p: any) => 
            `${p.Name || p.name} (${(p as any).quantity || 1} units)`
          ).join(', ');
          
          const focDetails = focProducts.map((p: any) => {
            const focProductData = formData.focProducts?.find((fp: any) => 
              fp.productId === p.UID || fp.productId === p.productId
            );
            const quantity = focProductData?.quantity || (p as any).quantity || 1;
            return `${p.Name || p.name || p.productName} (${quantity} units FREE)`;
          }).join(', ');
          
          return `Buy: ${buyDetails} → Get: ${focDetails}`;
        } else if (buyProducts.length === 0) {
          return `No buy products selected, ${focProducts.length} FOC products configured`;
        } else {
          return `${buyProducts.length} buy products selected, no FOC products configured`;
        }
      
      case 'BYVALUE':
        return `Min invoice value: ₹${formData.minValue || 0}`;
      
      case 'BYQTY':
        return `Min quantity: ${formData.minQty || 0}`;
      
      case 'LINECOUNT':
        return `Min line items: ${formData.minLineCount || 0}`;
      
      case 'BRANDCOUNT':
        return `Min brands: ${formData.minBrandCount || 0}`;
      
      case 'ANYVALUE':
        return `Any invoice value`;
      
      default:
        return 'Configuration pending';
    }
  };

  const getProductSelectionSummary = () => {
    // For multi-product configurations
    if (selectedFormat === 'MPROD' && formData.productPromotions) {
      return formData.productPromotions.filter(p => p.isActive).map(p => ({
        name: p.productName || 'Product',
        code: p.productId || '',
        quantity: p.quantityThreshold || 0
      }));
    }
    
    // For product attributes (hierarchy or specific products)
    if (Array.isArray(formData.finalAttributeProducts) && formData.finalAttributeProducts.length > 0) {
      return formData.finalAttributeProducts.map((p: any) => ({
        name: p.Name || p.name || '',
        code: p.Code || p.code || '',
        quantity: (p as any).quantity || formData.productSelection?.productQuantities?.[p.UID || p.uid] || 1
      }));
    }
    
    // For direct product selection
    if (formData.productSelection?.selectedFinalProducts && formData.productSelection.selectedFinalProducts.length > 0) {
      return formData.productSelection.selectedFinalProducts.map((p: any) => ({
        name: p.Name || p.name || '',
        code: p.Code || p.code || '',
        quantity: (p as any).quantity || formData.productSelection?.productQuantities?.[p.UID || p.uid] || 1
      }));
    }
    
    return [];
  };

  const getFOCProductsSummary = () => {
    // For BQXF, use focSelectedProducts with quantities from focProducts
    if (selectedFormat === 'BQXF' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0) {
      return formData.focSelectedProducts.map((p: any) => {
        const focProductData = formData.focProducts?.find((fp: any) => 
          fp.productId === p.UID || fp.productId === p.productId
        );
        return {
          name: p.Name || p.name || p.productName || '',
          code: p.Code || p.code || p.productCode || '',
          quantity: focProductData?.quantity || (p as any).quantity || 1
        };
      });
    }
    
    // Fallback to focProducts
    if (formData.focProducts && formData.focProducts.length > 0) {
      return formData.focProducts.map((p: any) => ({
        name: p.productName || p.Name || '',
        code: p.productCode || p.Code || '',
        quantity: p.quantity || 1
      }));
    }
    return [];
  };

  const getProductAttributesSummary = () => {
    // Check if we have product attributes stored as an array
    if (formData.productAttributes && Array.isArray(formData.productAttributes) && formData.productAttributes.length > 0) {
      // productAttributes is an array of {type, code, value, level} objects from DynamicProductAttributes
      const hierarchy: string[] = [];
      const attributesByLevel = new Map<number, any>();
      
      // Sort attributes by level to build hierarchy path
      formData.productAttributes.forEach((attr: any) => {
        if (attr.level !== undefined) {
          attributesByLevel.set(attr.level, attr);
        }
      });
      
      // Build hierarchy path from sorted levels
      const sortedLevels = Array.from(attributesByLevel.keys()).sort((a, b) => a - b);
      sortedLevels.forEach(level => {
        const attr = attributesByLevel.get(level);
        if (attr && attr.value) {
          hierarchy.push(attr.value);
        }
      });
      
      if (hierarchy.length > 0) {
        return [{
          type: 'Hierarchy',
          path: hierarchy.join(' → '),
          productCount: formData.finalAttributeProducts?.length || 0
        }];
      }
    }
    
    // Check for direct selection mode using productSelection
    const selectionType = formData.productSelection?.selectionType;
    if (selectionType === 'specific' && formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
      return [{
        type: 'Specific Products',
        path: `${formData.finalAttributeProducts.length} products selected`,
        productCount: formData.finalAttributeProducts.length
      }];
    }
    
    // Check for all products mode
    if (selectionType === 'all') {
      return [{
        type: 'All Products',
        path: 'All products in catalog',
        productCount: formData.finalAttributeProducts?.length || 0
      }];
    }
    
    return [];
  };

  const getFootprintSummary = () => {
    if (formData.footprint.type === 'all') {
      return { 
        type: 'All Stores',
        summary: 'Promotion applies to all stores in the system',
        details: []
      };
    } else if (formData.footprint.type === 'hierarchy') {
      const details = [];
      
      // Check for dynamicHierarchy object
      if (formData.footprint.dynamicHierarchy) {
        Object.entries(formData.footprint.dynamicHierarchy).forEach(([key, values]) => {
          if (values && Array.isArray(values) && values.length > 0) {
            // Format the key to be more readable
            const displayName = key
              .replace(/([A-Z])/g, ' $1')
              .replace(/^./, str => str.toUpperCase())
              .trim();
            
            details.push({
              level: displayName,
              count: values.length,
              items: values.slice(0, 3).map((v: any) => {
                // Handle different value formats
                if (typeof v === 'string') return v;
                return v.name || v.Name || v.label || v.Label || v.value || 'Item';
              })
            });
          }
        });
      }
      
      // Also check for selectedHierarchy array format
      if (!details.length && formData.footprint.selectedHierarchy) {
        if (Array.isArray(formData.footprint.selectedHierarchy)) {
          formData.footprint.selectedHierarchy.forEach((item: any) => {
            if (item.type && item.values && item.values.length > 0) {
              details.push({
                level: item.type,
                count: item.values.length,
                items: item.values.slice(0, 3).map((v: any) => v.name || v.Name || v)
              });
            }
          });
        }
      }
      
      return {
        type: 'Organization Hierarchy',
        summary: details.length > 0 
          ? `${details.map(d => `${d.count} ${d.level}${d.count > 1 ? 's' : ''}`).join(', ')}`
          : 'No hierarchy selected',
        details
      };
    } else if (formData.footprint.type === 'specific') {
      const stores = formData.footprint.specificStores || [];
      return {
        type: 'Specific Stores',
        summary: `${stores.length} store${stores.length !== 1 ? 's' : ''} selected`,
        details: stores.length > 0 ? [{
          level: 'Stores',
          count: stores.length,
          items: stores.slice(0, 3).map((s: any) => {
            if (typeof s === 'string') return s;
            return s.storeName || s.StoreName || s.name || s.Name || 'Store';
          })
        }] : []
      };
    }
    
    return {
      type: 'Unknown',
      summary: 'Footprint configuration not set',
      details: []
    };
  };

  const getVolumeCapsStatus = () => {
    if (!formData.volumeCaps || !formData.volumeCaps.enabled) {
      return { 
        enabled: false, 
        summary: 'Disabled',
        details: []
      };
    }
    
    const details = [];
    
    // Overall cap details
    if (formData.volumeCaps.overallCap && formData.volumeCaps.overallCap.value > 0) {
      const capType = formData.volumeCaps.overallCap.type || 'value';
      const capValue = formData.volumeCaps.overallCap.value;
      details.push({
        type: 'Overall Cap',
        description: capType === 'amount' 
          ? `Max Amount: ₹${capValue.toLocaleString()}`
          : capType === 'quantity' 
          ? `Max Quantity: ${capValue.toLocaleString()} units`
          : `Max Value: ${capValue.toLocaleString()}`
      });
    }
    
    // Period caps details
    if (formData.volumeCaps.periodCaps && formData.volumeCaps.periodCaps.length > 0) {
      formData.volumeCaps.periodCaps.forEach((cap: any) => {
        if (cap.value > 0) {
          const periodName = cap.period === 'day' ? 'Daily' 
            : cap.period === 'week' ? 'Weekly'
            : cap.period === 'month' ? 'Monthly' 
            : cap.period;
          
          details.push({
            type: `${periodName} Cap`,
            description: cap.type === 'amount'
              ? `₹${cap.value.toLocaleString()}`
              : `${cap.value.toLocaleString()} ${cap.type === 'quantity' ? 'units' : ''}`
          });
        }
      });
    }
    
    // Hierarchy caps details
    if (formData.volumeCaps.hierarchyCaps && formData.volumeCaps.hierarchyCaps.length > 0) {
      formData.volumeCaps.hierarchyCaps.forEach((cap: any) => {
        if (cap.value > 0) {
          details.push({
            type: `${cap.hierarchyLevel || 'Hierarchy'} Cap`,
            description: cap.type === 'amount'
              ? `₹${cap.value.toLocaleString()}`
              : `${cap.value.toLocaleString()} ${cap.type === 'quantity' ? 'units' : ''}`
          });
        }
      });
    }
    
    return {
      enabled: true,
      summary: details.length > 0 
        ? `${details.length} cap${details.length !== 1 ? 's' : ''} configured`
        : 'Enabled but not configured',
      details
    };
  };

  const volumeCaps = getVolumeCapsStatus();

  const formatDate = (dateStr: string) => {
    if (!dateStr) return 'Not set';
    const date = new Date(dateStr);
    return date.toLocaleDateString('en-US', { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  };

  return (
    <div className="max-w-4xl">
      <h2 className="text-xl font-semibold mb-6">Review & Confirm</h2>
      
      <div className="space-y-6">
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
                <p className="text-sm text-gray-600">Promotion Name</p>
                <p className="font-medium">{formData.promotionName || 'Not set'}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Promotion Code</p>
                <p className="font-medium font-mono">{formData.promotionCode || 'Not set'}</p>
              </div>
            </div>
            <div>
              <p className="text-sm text-gray-600 mb-1">Remarks</p>
              <p className="text-sm">{formData.remarks || 'No remarks provided'}</p>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-gray-600">Valid From</p>
                <p className="font-medium">{formatDate(formData.validFrom)}</p>
              </div>
              <div>
                <p className="text-sm text-gray-600">Valid Until</p>
                <p className="font-medium">{formatDate(formData.validUpto)}</p>
              </div>
            </div>
            <div>
              <p className="text-sm text-gray-600">Priority</p>
              <Badge variant="secondary">{formData.priority}</Badge>
            </div>
          </CardContent>
        </Card>

        {/* Promotion Type */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Tag className="w-5 h-5" />
              Promotion Type
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-4">
              <Badge className="bg-purple-100 text-purple-800">{level}</Badge>
              <Badge className="bg-blue-100 text-blue-800">{format}</Badge>
            </div>
          </CardContent>
        </Card>

        {/* Product Selection */}
        {(selectedLevel === "instant" || selectedFormat === "MPROD") && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Package className="w-5 h-5" />
                Product Selection
              </CardTitle>
            </CardHeader>
            <CardContent>
              {(() => {
                const products = getProductSelectionSummary();
                const focProducts = getFOCProductsSummary();
                const productAttributes = getProductAttributesSummary();
                
                if (products.length === 0 && focProducts.length === 0 && productAttributes.length === 0) {
                  return (
                    <p className="text-sm text-gray-500">No products selected</p>
                  );
                }
                
                return (
                  <div className="space-y-4">
                    {/* Product Attributes/Hierarchy */}
                    {productAttributes.length > 0 && (
                      <div>
                        <p className="text-sm font-medium mb-2">Product Selection Method</p>
                        <div className="bg-blue-50 rounded-lg p-3">
                          {productAttributes.map((attr, index) => (
                            <div key={index} className="flex items-center justify-between text-sm mb-1 last:mb-0">
                              <div className="flex items-center gap-2">
                                <Badge variant="outline" className="text-xs">
                                  {attr.type}
                                </Badge>
                                <span className="text-gray-700">{attr.path}</span>
                              </div>
                              {attr.productCount > 0 && (
                                <span className="text-xs text-gray-500">
                                  {attr.productCount} product{attr.productCount !== 1 ? 's' : ''}
                                </span>
                              )}
                            </div>
                          ))}
                        </div>
                      </div>
                    )}
                    
                    {/* Selected Products */}
                    {products.length > 0 && (
                      <div>
                        <p className="text-sm font-medium mb-2">
                          Selected Products ({products.length})
                        </p>
                        <div className="bg-gray-50 rounded-lg p-3 max-h-40 overflow-y-auto">
                          <div className="space-y-1">
                            {products.slice(0, 5).map((product, index) => (
                              <div key={index} className="flex items-center justify-between text-sm">
                                <span className="text-gray-700">
                                  {product.name} 
                                  {product.code && <span className="text-gray-500 ml-1">({product.code})</span>}
                                </span>
                                {product.quantity > 0 && (
                                  <Badge variant="outline" className="text-xs">
                                    Qty: {product.quantity}
                                  </Badge>
                                )}
                              </div>
                            ))}
                            {products.length > 5 && (
                              <p className="text-xs text-gray-500 mt-2">
                                ...and {products.length - 5} more products
                              </p>
                            )}
                          </div>
                        </div>
                      </div>
                    )}
                    
                    {/* FOC Products */}
                    {focProducts.length > 0 && (
                      <div>
                        <p className="text-sm font-medium mb-2">
                          FOC Products ({focProducts.length})
                        </p>
                        <div className="bg-green-50 rounded-lg p-3 max-h-40 overflow-y-auto">
                          <div className="space-y-1">
                            {focProducts.slice(0, 5).map((product, index) => (
                              <div key={index} className="flex items-center justify-between text-sm">
                                <span className="text-gray-700">
                                  {product.name}
                                  {product.code && <span className="text-gray-500 ml-1">({product.code})</span>}
                                </span>
                                <Badge variant="outline" className="text-xs bg-green-100">
                                  Free Qty: {product.quantity}
                                </Badge>
                              </div>
                            ))}
                            {focProducts.length > 5 && (
                              <p className="text-xs text-gray-500 mt-2">
                                ...and {focProducts.length - 5} more FOC products
                              </p>
                            )}
                          </div>
                        </div>
                      </div>
                    )}
                  </div>
                );
              })()}
            </CardContent>
          </Card>
        )}

        {/* Configuration */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Settings className="w-5 h-5" />
              Configuration
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <div className="flex items-start gap-2">
                <DollarSign className="w-4 h-4 text-gray-500 mt-0.5" />
                <div>
                  <p className="text-sm font-medium">Offer Details</p>
                  <p className="text-sm text-gray-600">{getConfigurationSummary()}</p>
                </div>
              </div>
              {formData.hasSlabs && (
                <div className="flex items-start gap-2">
                  <Info className="w-4 h-4 text-gray-500 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">Slab Configuration</p>
                    <p className="text-sm text-gray-600">
                      {formData.slabs?.length || 0} slabs configured, 
                      Order type: {formData.orderType || 'LINE'}
                    </p>
                  </div>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Footprint */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <MapPin className="w-5 h-5" />
              Footprint
            </CardTitle>
          </CardHeader>
          <CardContent>
            {(() => {
              const footprint = getFootprintSummary();
              return (
                <div className="space-y-3">
                  <div className="flex items-center gap-2">
                    <Badge variant="outline">{footprint.type}</Badge>
                    <span className="text-sm text-gray-600">{footprint.summary}</span>
                  </div>
                  
                  {footprint.details.length > 0 && (
                    <div className="bg-gray-50 rounded-lg p-3">
                      {footprint.details.map((detail, index) => (
                        <div key={index} className="mb-2 last:mb-0">
                          <p className="text-xs font-medium text-gray-700 mb-1">
                            {detail.level} ({detail.count})
                          </p>
                          <div className="flex flex-wrap gap-1">
                            {detail.items.map((item, i) => (
                              <Badge key={i} variant="secondary" className="text-xs">
                                {typeof item === 'string' ? item : item.name || 'Item'}
                              </Badge>
                            ))}
                            {detail.count > 3 && (
                              <span className="text-xs text-gray-500 ml-1">
                                +{detail.count - 3} more
                              </span>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              );
            })()}
          </CardContent>
        </Card>

        {/* Volume Caps */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Shield className="w-5 h-5" />
              Volume Caps
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <div className="flex items-center gap-2">
                {volumeCaps.enabled ? (
                  <Badge className="bg-green-100 text-green-800">Enabled</Badge>
                ) : (
                  <Badge variant="secondary">Disabled</Badge>
                )}
                <span className="text-sm text-gray-600">{volumeCaps.summary}</span>
              </div>
              
              {volumeCaps.enabled && volumeCaps.details && volumeCaps.details.length > 0 && (
                <div className="bg-gray-50 rounded-lg p-3">
                  <div className="space-y-2">
                    {volumeCaps.details.map((detail: any, index: number) => (
                      <div key={index} className="flex items-center justify-between text-sm">
                        <span className="font-medium text-gray-700">{detail.type}</span>
                        <span className="text-gray-600">{detail.description}</span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Validation Status */}
        <Alert className={formData.promotionName && formData.promotionCode 
          ? "bg-green-50 border-green-200" 
          : "bg-yellow-50 border-yellow-200"}>
          {formData.promotionName && formData.promotionCode ? (
            <>
              <CheckCircle className="h-4 w-4 text-green-600" />
              <AlertDescription className="text-green-800">
                <strong>Ready to create!</strong> All required fields are complete. 
                Review the details above and click "Create Promotion" to proceed.
              </AlertDescription>
            </>
          ) : (
            <>
              <AlertCircle className="h-4 w-4 text-yellow-600" />
              <AlertDescription className="text-yellow-800">
                <strong>Missing required fields!</strong> Please go back and complete:
                <ul className="list-disc list-inside mt-1">
                  {!formData.promotionName && <li>Promotion Name</li>}
                  {!formData.promotionCode && <li>Promotion Code</li>}
                </ul>
              </AlertDescription>
            </>
          )}
        </Alert>

      </div>
    </div>
  );
}