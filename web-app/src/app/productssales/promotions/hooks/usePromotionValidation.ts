import { PromotionV3FormData } from "../types/promotionV3.types";

export function usePromotionValidation() {
  const validateStep = (
    step: number,
    formData: PromotionV3FormData,
    selectedLevel: string,
    selectedFormat: string
  ): boolean => {
    switch (step) {
      case 1:
        return !!(selectedLevel && selectedFormat);

      case 2:
        return !!(
          formData.promotionName &&
          formData.promotionCode &&
          formData.validFrom &&
          formData.validUpto &&
          formData.validUpto >= formData.validFrom
        );

      case 3:
        // Configuration validation based on format
        // Map user-friendly format names to V3 format codes
        const formatMap: { [key: string]: string } = {
          'PERCENTAGE': 'IQPD',
          'FIXED_AMOUNT': 'IQFD',
          'QUANTITY_FREE': 'IQXF',
          'BUY_X_GET_Y': 'BQXF',
          'MULTI_PRODUCT': 'MPROD',
          'BY_VALUE': 'BYVALUE',
          'BY_QUANTITY': 'BYQTY',
          'LINE_COUNT': 'LINECOUNT',
          'BRAND_COUNT': 'BRANDCOUNT',
          'ANY_VALUE': 'ANYVALUE'
        };
        
        const v3Format = formatMap[selectedFormat] || selectedFormat;
        
        switch (v3Format) {
          case "MPROD":
            // Multi-product configuration validation
            if (
              !formData.productPromotions ||
              formData.productPromotions.length < 1  // MPROD requires at least 1 product configuration
            ) {
              return false;
            }

            // Check each product configuration
            const isValid = formData.productPromotions.every((product, index) => {
              // Skip inactive products
              if (!product.isActive) {
                return true;
              }

              // Must have products selected
              if (!product.selectedProducts || product.selectedProducts.length === 0) {
                return false;
              }

              // Must have a promotion type selected
              if (!product.promotionType) {
                return false;
              }

              // Validate based on promotion type (IQFD, IQPD, IQXF, BQXF)
              switch (product.promotionType) {
                case "IQFD":
                  return (
                    product.discountAmount !== undefined &&
                    product.discountAmount !== null &&
                    product.discountAmount > 0
                  );
                case "IQPD":
                  return (
                    product.discountPercent !== undefined &&
                    product.discountPercent !== null &&
                    product.discountPercent > 0 &&
                    product.discountPercent <= 100
                  );
                case "IQXF":
                  // For IQXF in MPROD, buyQty comes from product selection
                  // Only need to check freeQty
                  return (
                    product.freeQty !== undefined &&
                    product.freeQty !== null &&
                    product.freeQty > 0
                  );
                case "BQXF":
                  // For BQXF in MPROD, check for FOC products
                  return (
                    (product.focSelectedProducts && product.focSelectedProducts.length > 0) ||
                    (product.focProducts && product.focProducts.length > 0)
                  );
                default:
                  return false;
              }
            });
            
            return isValid;

          case "IQFD":
            // Item Fixed Discount
            const hasValidFixedDiscount = formData.hasSlabs 
              ? formData.slabs && formData.slabs.length > 0 && formData.slabs.every(slab => 
                  slab.minQty && slab.maxQty && slab.minQty < slab.maxQty && 
                  slab.discountAmount && slab.discountAmount > 0)
              : formData.discountAmount && formData.discountAmount > 0;
              
            return !!(
              // Product selection is handled in step 4, so we'll be lenient here
              (formData.productSelection?.selectionType === "all" ||
                formData.productSelection?.specificProducts?.length ||
                formData.productAttributes?.length ||
                formData.finalAttributeProducts?.length ||
                Object.keys(formData.productSelection?.hierarchySelections || {})
                  .length > 0 ||
                true) && // Allow all products as default
              hasValidFixedDiscount
            );

          case "IQPD":
            // Item Percentage Discount
            const hasValidPercentageDiscount = formData.hasSlabs 
              ? formData.slabs && formData.slabs.length > 0 && formData.slabs.every(slab => 
                  slab.minQty && slab.maxQty && slab.minQty < slab.maxQty && 
                  slab.discountPercent && slab.discountPercent > 0 && slab.discountPercent <= 100)
              : formData.discountPercent && formData.discountPercent > 0 && formData.discountPercent <= 100;
              
            return !!(
              // Product selection is handled in step 4, so we'll be lenient here
              (formData.productSelection?.selectionType === "all" ||
                formData.productSelection?.specificProducts?.length ||
                formData.productAttributes?.length ||
                formData.finalAttributeProducts?.length ||
                Object.keys(formData.productSelection?.hierarchySelections || {})
                  .length > 0 ||
                true) && // Allow all products as default
              hasValidPercentageDiscount
            );

          case "IQXF":
            // Item Quantity X Free
            // Buy quantity comes from product selection, only need free quantity
            return !!(
              // Must have exactly one product selected for IQXF
              formData.finalAttributeProducts?.length === 1 &&
              // Product must have a quantity configured (from product selection)
              (formData.finalAttributeProducts[0] as any)?.quantity > 0 &&
              // Must have free quantity configured (check both fields for compatibility)
              ((formData.getQty && formData.getQty > 0) || 
               (formData.freeQuantity && formData.freeQuantity > 0))
            );

          case "BQXF":
            // Buy Quantity X Free (FOC)
            // Buy quantities come from product selection, FOC products are selected separately
            // Quantities are managed by DynamicProductAttributes component
            return !!(
              // Must have buy products selected (quantities are handled by product selection component)
              formData.finalAttributeProducts && 
              formData.finalAttributeProducts.length > 0 &&
              // Must have FOC products selected
              ((formData.focSelectedProducts && formData.focSelectedProducts.length > 0) || 
               (formData.focProducts && formData.focProducts.length > 0))
            );

          case "BYVALUE":
            // Must have minimum value and discount configured
            return !!(
              formData.minValue && formData.minValue > 0 &&
              (
                (formData.offerType === 'percentage' && formData.discountPercent > 0) ||
                (formData.offerType === 'value' && formData.discountAmount > 0) ||
                (formData.offerType === 'foc' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0)
              )
            );
          
          case "BYQTY":
            // Must have minimum quantity and discount configured
            return !!(
              formData.minQty && formData.minQty > 0 &&
              (
                (formData.offerType === 'percentage' && formData.discountPercent > 0) ||
                (formData.offerType === 'value' && formData.discountAmount > 0) ||
                (formData.offerType === 'foc' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0)
              )
            );
          
          case "LINECOUNT":
            // Must have minimum line count and discount configured
            return !!(
              formData.minLineCount && formData.minLineCount > 0 &&
              (
                (formData.offerType === 'percentage' && formData.discountPercent > 0) ||
                (formData.offerType === 'value' && formData.discountAmount > 0) ||
                (formData.offerType === 'foc' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0)
              )
            );
          
          case "BRANDCOUNT":
            // Must have minimum brand count and discount configured
            return !!(
              formData.minBrandCount && formData.minBrandCount > 0 &&
              (
                (formData.offerType === 'percentage' && formData.discountPercent > 0) ||
                (formData.offerType === 'value' && formData.discountAmount > 0) ||
                (formData.offerType === 'foc' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0)
              )
            );
          
          case "ANYVALUE":
            // No threshold required, just need discount configured
            return !!(
              (formData.offerType === 'percentage' && formData.discountPercent > 0) ||
              (formData.offerType === 'value' && formData.discountAmount > 0) ||
              (formData.offerType === 'foc' && formData.focSelectedProducts && formData.focSelectedProducts.length > 0)
            );

          default:
            // For any unrecognized format, do basic validation
            console.log('Unrecognized format:', selectedFormat, 'v3Format:', v3Format);
            return !!(
              formData.promotionName &&
              formData.promotionCode &&
              (formData.discountAmount > 0 || formData.discountPercent > 0 || formData.buyQty > 0)
            );
        }

      case 4:
        // Footprint validation - at least one selection
        return true; // Footprint is optional, "all" is valid

      case 5:
        // Volume caps validation - optional
        return true;

      case 6:
        // Review step - no additional validation
        return true;

      default:
        return false;
    }
  };

  const getValidationErrors = (
    step: number,
    formData: PromotionV3FormData,
    selectedLevel: string,
    selectedFormat: string
  ): string[] => {
    const errors: string[] = [];

    switch (step) {
      case 1:
        if (!selectedLevel) errors.push("Please select a promotion level");
        if (!selectedFormat) errors.push("Please select a promotion format");
        break;

      case 2:
        if (!formData.promotionName) errors.push("Promotion name is required");
        if (!formData.promotionCode) errors.push("Promotion code is required");
        if (!formData.validFrom) errors.push("Valid from date is required");
        if (!formData.validUpto) errors.push("Valid to date is required");
        if (formData.validUpto < formData.validFrom)
          errors.push("End date must be after start date");
        break;

      case 3:
        if (selectedFormat === "MPROD") {
          if (!formData.productPromotions || formData.productPromotions.length < 1) {
            errors.push("Multi-Product Configuration requires at least 1 product configuration");
          } else {
            formData.productPromotions.forEach((product, index) => {
              if (!product.isActive) return; // Skip inactive products
              
              if (!product.selectedProducts || product.selectedProducts.length === 0) {
                errors.push(`Configuration ${index + 1}: Product selection is required`);
              }
              
              if (!product.promotionType) {
                errors.push(`Configuration ${index + 1}: Promotion type is required`);
              } else {
                switch (product.promotionType) {
                  case "IQFD":
                    if (!product.discountAmount || product.discountAmount <= 0) {
                      errors.push(`Configuration ${index + 1}: Fixed discount amount is required`);
                    }
                    break;
                  case "IQPD":
                    if (!product.discountPercent || product.discountPercent <= 0 || product.discountPercent > 100) {
                      errors.push(`Configuration ${index + 1}: Valid discount percentage (1-100%) is required`);
                    }
                    break;
                  case "IQXF":
                    // For IQXF in MPROD, only check freeQty (buyQty comes from product selection)
                    if (!product.freeQty || product.freeQty <= 0) {
                      errors.push(`Configuration ${index + 1}: Free quantity is required`);
                    }
                    break;
                  case "BQXF":
                    if (!product.focSelectedProducts?.length && !product.focProducts?.length) {
                      errors.push(`Configuration ${index + 1}: Free (FOC) product selection is required`);
                    }
                    break;
                }
              }
            });
          }
        }
        if (selectedFormat === "IQFD") {
          if (formData.hasSlabs) {
            if (!formData.slabs || formData.slabs.length === 0) {
              errors.push("At least one quantity slab is required");
            } else {
              formData.slabs.forEach((slab, index) => {
                if (!slab.minQty) errors.push(`Slab ${index + 1}: Minimum quantity is required`);
                if (!slab.maxQty) errors.push(`Slab ${index + 1}: Maximum quantity is required`);
                if (slab.minQty && slab.maxQty && slab.minQty >= slab.maxQty) {
                  errors.push(`Slab ${index + 1}: Maximum quantity must be greater than minimum quantity`);
                }
                if (!slab.discountAmount || slab.discountAmount <= 0) {
                  errors.push(`Slab ${index + 1}: Discount amount is required`);
                }
              });
            }
          } else if (!formData.discountAmount || formData.discountAmount <= 0) {
            errors.push("Discount amount is required");
          }
        }
        if (selectedFormat === "IQPD") {
          if (formData.hasSlabs) {
            if (!formData.slabs || formData.slabs.length === 0) {
              errors.push("At least one quantity slab is required");
            } else {
              formData.slabs.forEach((slab, index) => {
                if (!slab.minQty) errors.push(`Slab ${index + 1}: Minimum quantity is required`);
                if (!slab.maxQty) errors.push(`Slab ${index + 1}: Maximum quantity is required`);
                if (slab.minQty && slab.maxQty && slab.minQty >= slab.maxQty) {
                  errors.push(`Slab ${index + 1}: Maximum quantity must be greater than minimum quantity`);
                }
                if (!slab.discountPercent || slab.discountPercent <= 0 || slab.discountPercent > 100) {
                  errors.push(`Slab ${index + 1}: Valid discount percentage (1-100%) is required`);
                }
              });
            }
          } else if (!formData.discountPercent || formData.discountPercent <= 0 || formData.discountPercent > 100) {
            errors.push("Valid discount percentage (1-100%) is required");
          }
        }
        if (selectedFormat === "IQXF") {
          if (!formData.finalAttributeProducts?.length || formData.finalAttributeProducts.length !== 1) {
            errors.push("Please select exactly one product for IQXF promotion");
          } else if (!(formData.finalAttributeProducts[0] as any)?.quantity || (formData.finalAttributeProducts[0] as any).quantity <= 0) {
            errors.push("Please configure buy quantity in product selection");
          }
          if (!formData.getQty) errors.push("Free quantity (Y units) is required");
        }
        if (selectedFormat === "BQXF") {
          if (!formData.finalAttributeProducts || formData.finalAttributeProducts.length === 0) {
            errors.push("Please select products to buy (configure quantities in product selection)");
          }
          if (!formData.focSelectedProducts?.length && !formData.focProducts?.length) {
            errors.push("Please select FOC (free) products");
          }
        }
        
        // Invoice level format validations
        if (selectedFormat === "BYVALUE") {
          if (!formData.minValue || formData.minValue <= 0) {
            errors.push("Minimum invoice value is required");
          }
          if (!formData.offerType) {
            errors.push("Please select an offer type");
          } else if (formData.offerType === 'percentage' && (!formData.discountPercent || formData.discountPercent <= 0)) {
            errors.push("Discount percentage is required");
          } else if (formData.offerType === 'value' && (!formData.discountAmount || formData.discountAmount <= 0)) {
            errors.push("Discount amount is required");
          } else if (formData.offerType === 'foc' && (!formData.focSelectedProducts || formData.focSelectedProducts.length === 0)) {
            errors.push("Please select FOC products");
          }
        }
        
        if (selectedFormat === "BYQTY") {
          if (!formData.minQty || formData.minQty <= 0) {
            errors.push("Minimum total quantity is required");
          }
          if (!formData.offerType) {
            errors.push("Please select an offer type");
          } else if (formData.offerType === 'percentage' && (!formData.discountPercent || formData.discountPercent <= 0)) {
            errors.push("Discount percentage is required");
          } else if (formData.offerType === 'value' && (!formData.discountAmount || formData.discountAmount <= 0)) {
            errors.push("Discount amount is required");
          } else if (formData.offerType === 'foc' && (!formData.focSelectedProducts || formData.focSelectedProducts.length === 0)) {
            errors.push("Please select FOC products");
          }
        }
        
        if (selectedFormat === "LINECOUNT") {
          if (!formData.minLineCount || formData.minLineCount <= 0) {
            errors.push("Minimum line count is required");
          }
          if (!formData.offerType) {
            errors.push("Please select an offer type");
          } else if (formData.offerType === 'percentage' && (!formData.discountPercent || formData.discountPercent <= 0)) {
            errors.push("Discount percentage is required");
          } else if (formData.offerType === 'value' && (!formData.discountAmount || formData.discountAmount <= 0)) {
            errors.push("Discount amount is required");
          } else if (formData.offerType === 'foc' && (!formData.focSelectedProducts || formData.focSelectedProducts.length === 0)) {
            errors.push("Please select FOC products");
          }
        }
        
        if (selectedFormat === "BRANDCOUNT") {
          if (!formData.minBrandCount || formData.minBrandCount <= 0) {
            errors.push("Minimum brand count is required");
          }
          if (!formData.offerType) {
            errors.push("Please select an offer type");
          } else if (formData.offerType === 'percentage' && (!formData.discountPercent || formData.discountPercent <= 0)) {
            errors.push("Discount percentage is required");
          } else if (formData.offerType === 'value' && (!formData.discountAmount || formData.discountAmount <= 0)) {
            errors.push("Discount amount is required");
          } else if (formData.offerType === 'foc' && (!formData.focSelectedProducts || formData.focSelectedProducts.length === 0)) {
            errors.push("Please select FOC products");
          }
        }
        
        if (selectedFormat === "ANYVALUE") {
          if (!formData.offerType) {
            errors.push("Please select an offer type");
          } else if (formData.offerType === 'percentage' && (!formData.discountPercent || formData.discountPercent <= 0)) {
            errors.push("Discount percentage is required");
          } else if (formData.offerType === 'value' && (!formData.discountAmount || formData.discountAmount <= 0)) {
            errors.push("Discount amount is required");
          } else if (formData.offerType === 'foc' && (!formData.focSelectedProducts || formData.focSelectedProducts.length === 0)) {
            errors.push("Please select FOC products");
          }
        }
        
        break;
    }

    return errors;
  };

  return {
    validateStep,
    getValidationErrors,
  };
}