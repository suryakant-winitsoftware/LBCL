// PromotionV3Service - Updated to use OLD CUDPromotionMaster API
// All V3 API calls removed - using proven old API structure

import { PromotionV3FormData, PromotionV3Response, ProductSelection, Footprint } from "../types/promotionV3.types";
import { IPromoMasterView } from "../types/promotion.types";

class PromotionV3Service {
  private cache = new Map<string, any>();
  private cacheExpiry = new Map<string, number>();

  // Create promotion using OLD CUDPromotionMaster API with additional calls for configs and caps
  async createPromotion(formData: PromotionV3FormData): Promise<PromotionV3Response> {
    try {
      const { API_CONFIG, getCommonHeaders } = await import('./api-config');
      const { authService } = await import('@/lib/auth-service');
      
      const currentUser = authService.getCurrentUser();
      const employeeUID = currentUser?.uid || currentUser?.id || null;
      
      if (!employeeUID) {
        return {
          success: false,
          error: 'User authentication required. Please log in again.'
        };
      }
      
      // Convert V3 form data to OLD API PromoMasterView format
      console.log('[DEBUG] About to call convertToOldAPIFormat with:', { formData, employeeUID });
      let promoMasterView;
      try {
        promoMasterView = this.convertToOldAPIFormat(formData, employeeUID);
        console.log('[DEBUG] convertToOldAPIFormat returned:', promoMasterView);
      } catch (error) {
        console.error('[DEBUG] Error in convertToOldAPIFormat:', error);
        return {
          success: false,
          error: `Failed to convert form data: ${error.message}`
        };
      }
      
      if (!promoMasterView) {
        console.error('[DEBUG] convertToOldAPIFormat returned null/undefined');
        return {
          success: false,
          error: 'Failed to convert form data - null result'
        };
      }
      
      const promotionUID = promoMasterView.PromotionView?.UID;
      
      // Clean the data to remove null/undefined values that cause Dapper issues
      const cleanedPromoMasterView = this.cleanObject(promoMasterView);
      console.log('[DEBUG] Cleaned promoMasterView successfully');
      
      // Debug: Check PromoOrderItemViewList for null MaxQty values
      console.log('[DEBUG] PromoOrderItemViewList before cleaning:', promoMasterView.PromoOrderItemViewList);
      console.log('[DEBUG] PromoOrderItemViewList after cleaning:', cleanedPromoMasterView.PromoOrderItemViewList);
      
      // Debug log for org UID
      console.log('Promotion creation - OrgUID:', promoMasterView.PromotionView?.org_uid || promoMasterView.PromotionView?.OrgUID);
      console.log('Promotion creation - CompanyUID:', promoMasterView.PromotionView?.company_uid || promoMasterView.PromotionView?.CompanyUID);
      console.log('Full PromotionView being sent:', promoMasterView.PromotionView);
      
      // CRITICAL DEBUG: Check if PromotionView exists
      if (!promoMasterView.PromotionView) {
        console.error('[CRITICAL] PromotionView is null/undefined!');
        console.error('promoMasterView structure:', Object.keys(promoMasterView));
      }
      
      // ENHANCED DEBUG LOGGING FOR MISSING DATA
      console.log('\n========== ENHANCED DEBUG LOGGING ==========');
      console.log('Raw formData received:', formData);
      console.log('PromoOfferItemViewList count:', promoMasterView.PromoOfferItemViewList?.length || 0);
      console.log('PromoOfferItemViewList data:', promoMasterView.PromoOfferItemViewList);
      console.log('SchemeOrgs count:', promoMasterView.SchemeOrgs?.length || 0);
      console.log('SchemeOrgs data:', promoMasterView.SchemeOrgs);
      console.log('SchemeBranches count:', promoMasterView.SchemeBranches?.length || 0);
      console.log('SchemeBranches data:', promoMasterView.SchemeBranches);
      console.log('=========================================\n');
      
      // Step 1: Call OLD API endpoint for main promotion
      console.log('[DEBUG] Request payload being sent to backend:', JSON.stringify(cleanedPromoMasterView, null, 2));
      console.log('[DEBUG] Request payload stringified length:', JSON.stringify(cleanedPromoMasterView).length);
      
      const response = await fetch(`${API_CONFIG.baseURL}/Promotion/CUDPromotionMaster`, {
        method: 'POST',
        headers: {
          ...getCommonHeaders(),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(cleanedPromoMasterView)
      });

      if (!response.ok) {
        throw new Error(`API Error: ${response.status}`);
      }

      const result = await response.json();
      
      if (result.Data <= 0) {
        return {
          success: false,
          error: 'Failed to create promotion'
        };
      }
      
      // Volume caps and product configs are now handled within CUDPromotionMaster
      // No additional API calls needed - everything is saved in one transaction
      
      return {
        success: true,
        data: {} as any
      };
      
    } catch (error: any) {
      console.error('Error creating promotion:', error);
      return {
        success: false,
        error: error.message || 'Failed to create promotion'
      };
    }
  }

  // Convert V3 format to OLD API PromoMasterView format
  private convertToOldAPIFormat(formData: PromotionV3FormData, employeeUID: string): IPromoMasterView {
    console.log('[DEBUG] ===== FORM DATA ANALYSIS =====');
    console.log('[DEBUG] Promotion format:', formData.format);
    console.log('[DEBUG] Free quantity fields check:');
    console.log('  - formData.freeQuantity:', formData.freeQuantity);
    console.log('  - formData.getQty:', formData.getQty);
    console.log('  - formData.getFree:', formData.getFree);
    console.log('  - formData.buyXGetY:', formData.buyXGetY);
    console.log('  - formData.getQuantity:', formData.getQuantity);
    console.log('  - formData.freeQty:', formData.freeQty);
    console.log('[DEBUG] Buy/Get configuration:', formData.buyGetConfig);
    console.log('[DEBUG] Offers configuration:', formData.offers);
    console.log('[DEBUG] FOC fields check:');
    console.log('  - formData.focSelectedProducts:', formData.focSelectedProducts);
    console.log('  - formData.focItems:', formData.focItems);
    console.log('  - formData.focProducts:', formData.focProducts);
    console.log('[DEBUG] Volume Cap fields check:');
    console.log('  - formData.volumeCaps:', formData.volumeCaps);
    console.log('  - formData.volumeCap:', formData.volumeCap);
    console.log('  - formData.volumeLimit:', formData.volumeLimit);
    console.log('  - formData.invoiceLimit:', formData.invoiceLimit);
    console.log('  - formData.perInvoiceLimit:', formData.perInvoiceLimit);
    console.log('[DEBUG] ==========================');
    
    const promotionUID = this.generateUID();
    const promoOrderUID = this.generateUID();
    
    // Extract discount values from slabs if hasSlabs is true
    const processedFormData = { ...formData };
    if (formData.hasSlabs && formData.slabs && formData.slabs.length > 0) {
      // Use the first slab's discount values as the main promotion discount
      const firstSlab = formData.slabs[0];
      if (firstSlab.discountAmount && firstSlab.discountAmount > 0) {
        processedFormData.discountAmount = firstSlab.discountAmount;
      }
      if (firstSlab.discountPercent && firstSlab.discountPercent > 0) {
        processedFormData.discountPercent = firstSlab.discountPercent;
      }
    }
    
    // Build ItemPromotionMapViewList for product hierarchy
    const itemPromotionMapList = this.buildItemPromotionMapList(processedFormData, promotionUID);
    
    // Build PromoOrderViewList
    const promoOrderList = this.buildPromoOrderList(processedFormData, promotionUID, promoOrderUID, employeeUID);
    
    // Build PromoOrderItemViewList for product criteria
    const promoOrderItemList = this.buildPromoOrderItemList(processedFormData, promotionUID, promoOrderUID, employeeUID);
    
    // Build PromoOfferViewList
    const promoOfferList = this.buildPromoOfferList(processedFormData, promotionUID, promoOrderUID, employeeUID);
    
    // Build PromoConditionViewList
    const promoConditionList = this.buildPromoConditionList(processedFormData, promoOrderList, employeeUID);
    
    // Build organization mappings
    const { schemeOrgs, schemeBranches } = this.buildOrganizationMappings(processedFormData, promotionUID);
    
    console.log('[DEBUG] SchemeOrgs built:', schemeOrgs.length, schemeOrgs);
    console.log('[DEBUG] SchemeBranches built:', schemeBranches.length, schemeBranches);
    
    // IMPORTANT: The backend C# code expects PascalCase for property names
    // but the database columns use snake_case. The C# Dapper mapping handles this.
    const promotionView = {
      // Use PascalCase for C# compatibility
      UID: promotionUID,
      CompanyUID: this.extractCompanyUID(processedFormData),
      OrgUID: this.extractPrimaryOrgUID(processedFormData),
      Code: processedFormData.promotionCode,
      Name: processedFormData.promotionName,
      Remarks: processedFormData.remarks || '',
      Category: this.determineCategory(processedFormData),
      HasSlabs: processedFormData.hasSlabs || false,
      CreatedByEmpUID: employeeUID,
      CreatedBy: employeeUID,
      ModifiedBy: employeeUID,
      ValidFrom: processedFormData.validFrom,
      ValidUpto: processedFormData.validUpto,
      Type: processedFormData.level === 'invoice' ? 'Invoice' : 'Instant',
      PromoFormat: processedFormData.format, // IQFD, IQPD, IQXF, BQXF, etc.
      IsActive: true,
      PromoTitle: processedFormData.promoTitle || processedFormData.promotionName,
      PromoMessage: processedFormData.promoMessage || '',
      Priority: processedFormData.priority || 0,
      HasFactSheet: false,
      ContributionLevel1: 0,
      ContributionLevel2: 0,
      ContributionLevel3: 0,
      SS: 0,
      CreatedTime: new Date().toISOString(),
      ModifiedTime: new Date().toISOString(),
      ServerAddTime: new Date().toISOString(),
      ServerModifiedTime: new Date().toISOString(),
      ActionType: 0  // Use integer 0 for Add (C# enum value)
    };
    
    console.log('[DEBUG] PromotionView created with UID:', promotionView.UID);
    console.log('[DEBUG] PromotionView CompanyUID:', promotionView.CompanyUID);
    console.log('[DEBUG] PromotionView OrgUID:', promotionView.OrgUID);
    console.log('[DEBUG] Full PromotionView object:', JSON.stringify(promotionView, null, 2));
    
    const result = {
      IsNew: true,
      PromotionView: promotionView,
      
      // Product mappings using existing tables
      ItemPromotionMapViewList: itemPromotionMapList || [],
      PromoOrderViewList: promoOrderList || [],
      PromoOrderItemViewList: promoOrderItemList || [],
      PromoOfferViewList: promoOfferList || [],
      PromoOfferItemViewList: this.buildPromoOfferItemList(processedFormData, promoOfferList, employeeUID) || [],
      PromoConditionViewList: promoConditionList || [],
      
      // Organization mappings - these must match ISchemeBranch, ISchemeOrg interfaces
      SchemeBranches: schemeBranches || [],
      SchemeOrgs: schemeOrgs || [],
      SchemeBroadClassifications: [],
      
      // Volume cap configuration
      PromotionVolumeCap: this.buildVolumeCapConfiguration(processedFormData, promotionUID, employeeUID),
      PromotionHierarchyCapViewList: this.buildHierarchyCapConfiguration(processedFormData, promotionUID, employeeUID),
      PromotionPeriodCapViewList: this.buildPeriodCapConfiguration(processedFormData, promotionUID, employeeUID)
    };
    
    console.log('[DEBUG] Final PromoMasterView structure:', JSON.stringify(result, null, 2));
    console.log('[DEBUG] ========== HIERARCHY AND PERIOD CAPS DEBUG ==========');
    console.log('[DEBUG] PromotionHierarchyCapViewList length:', result.PromotionHierarchyCapViewList?.length || 0);
    console.log('[DEBUG] PromotionPeriodCapViewList length:', result.PromotionPeriodCapViewList?.length || 0);
    if (result.PromotionHierarchyCapViewList?.length > 0) {
      console.log('[DEBUG] First hierarchy cap:', result.PromotionHierarchyCapViewList[0]);
    }
    if (result.PromotionPeriodCapViewList?.length > 0) {
      console.log('[DEBUG] First period cap:', result.PromotionPeriodCapViewList[0]);
    }
    console.log('[DEBUG] ================================================');
    return result;
  }
  
  // Build Volume Cap Configuration
  private buildVolumeCapConfiguration(formData: any, promotionUID: string, employeeUID: string): any | null {
    let volumeConfig = null;
    
    // Check different possible sources for volume cap data
    if (formData.volumeCaps) {
      volumeConfig = formData.volumeCaps;
    } else if (formData.volumeCap) {
      volumeConfig = formData.volumeCap;
    } else if (formData.volumeLimit) {
      volumeConfig = formData.volumeLimit;
    } else if (formData.perInvoiceLimit) {
      volumeConfig = formData.perInvoiceLimit;
    } else if (formData.invoiceLimit) {
      volumeConfig = formData.invoiceLimit;
    }
    
    // Check for volume cap in limits configuration
    if (!volumeConfig && formData.limits) {
      volumeConfig = formData.limits.volume || formData.limits.volumeCap || formData.limits.invoice;
    }
    
    // Check for volume cap in constraints
    if (!volumeConfig && formData.constraints) {
      volumeConfig = formData.constraints.volume || formData.constraints.volumeCap || formData.constraints.invoice;
    }
    
    console.log('[DEBUG] Volume configuration found:', volumeConfig);
    
    if (volumeConfig) {
      const now = new Date().toISOString();
      return {
        UID: this.generateUID(),
        PromotionUID: promotionUID,
        Enabled: volumeConfig.enabled ?? true,
        OverallCapType: (volumeConfig.overallCap?.type || volumeConfig.type || 'VALUE').toUpperCase(),
        OverallCapValue: volumeConfig.overallCap?.value || volumeConfig.value || volumeConfig.overallCapValue || 0,
        OverallCapConsumed: 0,
        InvoiceMaxDiscountValue: volumeConfig.invoiceCap?.maxDiscountValue || volumeConfig.maxDiscountValue || volumeConfig.invoiceMaxDiscountValue || 0,
        InvoiceMaxQuantity: volumeConfig.invoiceCap?.maxQuantity || volumeConfig.maxQuantity || volumeConfig.invoiceMaxQuantity || 0,
        InvoiceMaxApplications: volumeConfig.invoiceCap?.maxApplications || volumeConfig.maxApplications || volumeConfig.invoiceMaxApplications || 100,
        ActionType: 0,  // 0 = Add enum value
        CreatedTime: now,
        ModifiedTime: now,
        ServerAddTime: now,
        ServerModifiedTime: now,
        CreatedBy: employeeUID || 'SYSTEM',
        ModifiedBy: employeeUID || 'SYSTEM'
      };
    }
    
    return null;
  }

  // Build Hierarchy Cap Configuration
  private buildHierarchyCapConfiguration(formData: any, promotionUID: string, employeeUID: string): any[] {
    const hierarchyCaps = [];
    
    // Debug: Log hierarchy caps data sources
    console.log('[DEBUG] Checking hierarchy caps - volumeCaps.hierarchyCaps:', formData.volumeCaps?.hierarchyCaps);
    
    // Check for hierarchy caps data
    let hierarchyConfig = null;
    if (formData.hierarchyCaps) {
      hierarchyConfig = formData.hierarchyCaps;
    } else if (formData.volumeCaps?.hierarchyCaps) {
      hierarchyConfig = formData.volumeCaps.hierarchyCaps;
    } else if (formData.limits?.hierarchyCaps) {
      hierarchyConfig = formData.limits.hierarchyCaps;
    } else if (formData.limits?.hierarchy) {
      hierarchyConfig = formData.limits.hierarchy;
    } else if (formData.constraints?.hierarchyCaps) {
      hierarchyConfig = formData.constraints.hierarchyCaps;
    } else if (formData.constraints?.hierarchy) {
      hierarchyConfig = formData.constraints.hierarchy;
    }
    
    console.log('[DEBUG] Final hierarchy configuration found:', hierarchyConfig);
    
    if (hierarchyConfig && Array.isArray(hierarchyConfig)) {
      const now = new Date().toISOString();
      
      hierarchyConfig.forEach((hierCap: any) => {
        hierarchyCaps.push({
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          HierarchyType: hierCap.hierarchyType || hierCap.type || 'store',
          HierarchyUID: hierCap.hierarchyId || hierCap.hierarchyUID || '',
          HierarchyName: hierCap.hierarchyName || hierCap.name || '',
          CapType: hierCap.capType || 'value',
          CapValue: hierCap.capValue || hierCap.value || 0,
          CapConsumed: 0,
          IsActive: true,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now,
          CreatedBy: employeeUID || 'SYSTEM',
          ModifiedBy: employeeUID || 'SYSTEM'
        });
      });
    }
    
    console.log('[DEBUG] Built hierarchy caps array:', hierarchyCaps);
    console.log('[DEBUG] Hierarchy caps count:', hierarchyCaps.length);
    return hierarchyCaps;
  }

  // Build Period Cap Configuration
  private buildPeriodCapConfiguration(formData: any, promotionUID: string, employeeUID: string): any[] {
    const periodCaps = [];
    
    // Debug: Log period caps data sources
    console.log('[DEBUG] Checking period caps - volumeCaps.periodCaps:', formData.volumeCaps?.periodCaps);
    
    // Check for period caps data
    let periodConfig = null;
    if (formData.periodCaps) {
      periodConfig = formData.periodCaps;
    } else if (formData.volumeCaps?.periodCaps) {
      periodConfig = formData.volumeCaps.periodCaps;
    } else if (formData.limits?.periodCaps) {
      periodConfig = formData.limits.periodCaps;
    } else if (formData.limits?.period) {
      periodConfig = formData.limits.period;
    } else if (formData.constraints?.periodCaps) {
      periodConfig = formData.constraints.periodCaps;
    } else if (formData.constraints?.period) {
      periodConfig = formData.constraints.period;
    }
    
    console.log('[DEBUG] Final period configuration found:', periodConfig);
    
    if (periodConfig && Array.isArray(periodConfig)) {
      const now = new Date().toISOString();
      
      periodConfig.forEach((periodCap: any) => {
        const startDate = periodCap.startDate || new Date().toISOString();
        const endDate = periodCap.endDate || new Date(new Date().getTime() + 30*24*60*60*1000).toISOString(); // Default 30 days
        
        periodCaps.push({
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          PeriodType: periodCap.periodType || periodCap.type || 'daily',
          CapType: periodCap.capType || 'value',
          CapValue: periodCap.capValue || periodCap.value || 0,
          CapConsumed: 0,
          StartDate: startDate,
          EndDate: endDate,
          IsActive: true,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: now,
          ModifiedTime: now,
          ServerAddTime: now,
          ServerModifiedTime: now,
          CreatedBy: employeeUID || 'SYSTEM',
          ModifiedBy: employeeUID || 'SYSTEM'
        });
      });
    }
    
    console.log('[DEBUG] Built period caps array:', periodCaps);
    console.log('[DEBUG] Period caps count:', periodCaps.length);
    return periodCaps;
  }

  // Build ItemPromotionMapViewList - handles product hierarchy properly
  private buildItemPromotionMapList(formData: any, promotionUID: string): any[] {
    const itemMaps = [];
    
    // Handle different selection modes
    if (formData.productSelectionMode === 'all') {
      // All products
      itemMaps.push({
        UID: this.generateUID(),
        SKUType: 'ALL',
        SKUTypeUID: '*',
        PromotionUID: promotionUID,
        SS: 0,
        ActionType: 0,  // 0 = Add enum value
        CreatedTime: new Date().toISOString(),
        ModifiedTime: new Date().toISOString()
      });
    }
    else if (formData.productSelectionMode === 'hierarchy' && formData.productAttributes) {
      // Hierarchy selections (Category/Brand/SubCategory)
      formData.productAttributes.forEach((attr: any) => {
        // Map hierarchy type correctly
        const skuType = this.mapHierarchyType(attr.type || attr.level);
        
        // SKUTypeUID must be a string, not an array
        // If attr.value is an array, join it or iterate over it
        const values = Array.isArray(attr.value) ? attr.value : [attr.value || attr.code];
        
        values.forEach((value: any) => {
          if (value) {  // Only add if value exists
            itemMaps.push({
              UID: this.generateUID(),
              SKUType: skuType, // 'Category', 'Brand', 'SubCategory'
              SKUTypeUID: String(value), // Ensure it's always a string
              PromotionUID: promotionUID,
              SS: 0,
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString()
            });
          }
        });
      });
      
      // Add specific products if selected within hierarchy
      if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
        formData.finalAttributeProducts.forEach((product: any) => {
          const productId = product.ItemCode || product.Code || product.UID || product.uid;
          if (productId) {
            itemMaps.push({
              UID: this.generateUID(),
              SKUType: 'SKU',
              SKUTypeUID: String(productId), // Ensure it's always a string
              PromotionUID: promotionUID,
              SS: 0,
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString()
            });
          }
        });
      }
    }
    else if (formData.productSelectionMode === 'specific' && formData.specificProducts) {
      // Specific products only
      formData.specificProducts.forEach((product: any) => {
        itemMaps.push({
          UID: this.generateUID(),
          SKUType: 'SKU',
          SKUTypeUID: product.ItemCode || product.UID,
          PromotionUID: promotionUID,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString()
        });
      });
    }
    
    // Handle excluded products
    if (formData.excludedProducts && formData.excludedProducts.length > 0) {
      formData.excludedProducts.forEach((product: any) => {
        itemMaps.push({
          UID: this.generateUID(),
          SKUType: 'EXCLUDED_SKU',
          SKUTypeUID: product.ItemCode || product.UID,
          PromotionUID: promotionUID,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString()
        });
      });
    }
    
    return itemMaps;
  }

  // Build PromoOrderViewList
  private buildPromoOrderList(formData: any, promotionUID: string, promoOrderUID: string, employeeUID: string): any[] {
    const now = new Date().toISOString();
    return [{
      // Use PascalCase for C# compatibility
      UID: promoOrderUID,
      PromotionUID: promotionUID,
      SelectionModel: formData.selectionModel || 'any',
      QualificationLevel: formData.level === 'invoice' ? 'INVOICE' : 'LINE',
      MinDealCount: 1,
      MaxDealCount: formData.maxDealCount || formData.maxApplicationsPerInvoice || formData.volumeCaps?.invoiceCap?.maxApplications || 100,
      SS: 0,
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now,
      CreatedBy: employeeUID || 'SYSTEM',
      ModifiedBy: employeeUID || 'SYSTEM',
      ActionType: 0  // 0 = Add enum value
    }];
  }

  // Build PromoOrderItemViewList for complex product criteria - COMPREHENSIVE FIX
  private buildPromoOrderItemList(formData: any, promotionUID: string, promoOrderUID: string, employeeUID: string): any[] {
    const items: any[] = [];
    const format = formData.format;
    const selectionMode = formData.productSelectionMode || formData.productSelection?.selectionType || 'all';
    
    console.log('[DEBUG] Building PromoOrderItems - Format:', format, 'SelectionMode:', selectionMode);
    console.log('[DEBUG] Full formData for product selection:', {
      productSelectionMode: formData.productSelectionMode,
      productSelection: formData.productSelection,
      productAttributes: formData.productAttributes,
      finalAttributeProducts: formData.finalAttributeProducts?.length || 0,
      specificProducts: formData.specificProducts?.length || 0,
      hierarchyProducts: formData.hierarchyProducts || null
    });
    
    // Special handling for Invoice Level formats - they apply to entire invoice, not specific products
    if (format === 'BYVALUE' || format === 'BYQTY' || format === 'LINECOUNT' || format === 'BRANDCOUNT' || format === 'ANYVALUE') {
      // Invoice level promotions apply to all products in the invoice
      // Return a single "ALL" item to indicate this applies to entire invoice
      const orderItem: any = {
        UID: this.generateUID(),
        PromotionUID: promotionUID,
        PromoOrderUID: promoOrderUID,
        ParentUID: null,
        ItemCriteriaType: 'ALL',
        ItemCriteriaSelected: 'ALL',
        IsCompulsory: false,
        ItemUOM: null,
        MinQty: 0, // No minimum quantity for invoice level
        PromoSplit: 0,
        SS: 0,
        ActionType: 0,  // 0 = Add enum value
        CreatedTime: new Date().toISOString(),
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString()
      };
      items.push(orderItem);
      return items;
    }
    
    // Special handling for IQXF - Item Quantity X Free (single product with buy X get Y free)
    if (format === 'IQXF') {
      if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
        const product = formData.finalAttributeProducts[0];
        const buyQty = (product as any).quantity || formData.buyQty || formData.minQty || 1;
        
        const orderItem: any = {
          // Use PascalCase for C# compatibility
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          PromoOrderUID: promoOrderUID,
          ParentUID: null,
          ItemCriteriaType: 'SKU',
          ItemCriteriaSelected: product.Code || product.UID,
          IsCompulsory: true,
          ItemUOM: product.UOM || 'PCS',
          MinQty: buyQty,
          MaxQty: buyQty,
          PromoSplit: 0,
          SS: 0,
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          CreatedBy: employeeUID || 'SYSTEM',
          ModifiedBy: employeeUID || 'SYSTEM',
          ActionType: 0,  // 0 = Add enum value
          // Store product selection type
          // Specific product selection
        };
        items.push(orderItem);
        console.log('[DEBUG] Added IQXF product:', orderItem);
      }
      return items;
    }
    
    // Special handling for BQXF - Buy Quantity X Free (multiple buy products, get FOC items)
    if (format === 'BQXF') {
      if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
        formData.finalAttributeProducts.forEach((product: any, index: number) => {
          const qty = (product as any).quantity || 1;
          
          const orderItem: any = {
            UID: this.generateUID(),
            PromotionUID: promotionUID,
            PromoOrderUID: promoOrderUID,
            ParentUID: null,
            ItemCriteriaType: 'SKU',
            ItemCriteriaSelected: product.Code || product.UID,
            IsCompulsory: false,
            ItemUOM: null,
            MinQty: qty,
            MaxQty: qty, // For BQXF, max = min (exact quantity per product)
            PromoSplit: 0,
            SS: 0,
            ActionType: 0,  // 0 = Add enum value
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          };
          items.push(orderItem);
        });
      }
      return items;
    }
    
    // Special handling for IQFD/IQPD - Item Fixed/Percentage Discount
    if (format === 'IQFD' || format === 'IQPD') {
      // Determine actual selection mode by checking what data exists
      let actualSelectionMode = 'all';
      
      // Check if hierarchy data exists
      if (formData.productAttributes && formData.productAttributes.length > 0) {
        actualSelectionMode = 'hierarchy';
      }
      // Check if specific products are selected
      else if (formData.specificProducts && formData.specificProducts.length > 0) {
        actualSelectionMode = 'specific';
      }
      // Check if final attribute products exist (products selected within hierarchy)
      else if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
        actualSelectionMode = 'hierarchy'; // Products selected within hierarchy
      }
      
      console.log('[DEBUG] Determined actualSelectionMode:', actualSelectionMode);
      
      // Handle based on actual selection mode
      if (actualSelectionMode === 'all') {
        // ALL products selection
        const orderItem: any = {
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          PromoOrderUID: promoOrderUID,
          ParentUID: null,
          ItemCriteriaType: 'ALL',
          ItemCriteriaSelected: 'ALL',
          IsCompulsory: false,
          ItemUOM: 'PCS',
          MinQty: formData.minQty || 1,
          PromoSplit: 0,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          // All products selection
        };
        // Don't include MaxQty for ALL items - no upper limit needed
        items.push(orderItem);
        console.log('[DEBUG] Added ALL products selection:', orderItem);
        
      } else if (actualSelectionMode === 'hierarchy') {
        // HIERARCHY selection - Create proper parent-child structure
        
        // Create a parent item for the hierarchy
        const hierarchyParentUID = this.generateUID();
        const hierarchyParent: any = {
          UID: hierarchyParentUID,
          PromotionUID: promotionUID,
          PromoOrderUID: promoOrderUID,
          ParentUID: null, // This is the root parent
          ItemCriteriaType: 'HIERARCHY_ROOT',
          ItemCriteriaSelected: 'PRODUCT_HIERARCHY',
          IsCompulsory: false,
          ItemUOM: 'PCS',
          MinQty: formData.minQty || 1,
          PromoSplit: 0,
          SS: 0,
          ActionType: 0,
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          // Selection type: hierarchy-based
        };
        items.push(hierarchyParent);
        
        // Process product attributes (Category, Brand, SubCategory) as children
        if (formData.productAttributes && formData.productAttributes.length > 0) {
          formData.productAttributes.forEach((attr: any) => {
            const values = Array.isArray(attr.value) ? attr.value : [attr.value];
            values.forEach((value: string) => {
              if (value) {
                const hierarchyItem: any = {
                  UID: this.generateUID(),
                  PromotionUID: promotionUID,
                  PromoOrderUID: promoOrderUID,
                  ParentUID: hierarchyParentUID, // Link to parent
                  ItemCriteriaType: attr.type?.toUpperCase() || 'CATEGORY',
                  ItemCriteriaSelected: value,
                  IsCompulsory: false,
                  ItemUOM: 'PCS',
                  MinQty: formData.minQty || 1,
                  PromoSplit: 0,
                  SS: 0,
                  ActionType: 0,
                  CreatedTime: new Date().toISOString(),
                  ModifiedTime: new Date().toISOString(),
                  ServerAddTime: new Date().toISOString(),
                  ServerModifiedTime: new Date().toISOString(),
                  // Selection type: hierarchy-based
                };
                items.push(hierarchyItem);
              }
            });
          });
        }
        
        // Process specific products selected within hierarchy as children
        if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
          formData.finalAttributeProducts.forEach((product: any) => {
            const productItem: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              PromoOrderUID: promoOrderUID,
              ParentUID: hierarchyParentUID, // Link to parent
              ItemCriteriaType: 'SKU',
              ItemCriteriaSelected: product.Code || product.ItemCode || product.UID,
              IsCompulsory: false,
              ItemUOM: product.UOM || 'PCS',
              MinQty: product.quantity || formData.minQty || 1,
              MaxQty: product.quantity || null,
              PromoSplit: 0,
              SS: 0,
              ActionType: 0,
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString(),
              ServerAddTime: new Date().toISOString(),
              ServerModifiedTime: new Date().toISOString(),
              // Selection type: hierarchy-based,
              // Product details stored in ItemCriteriaSelected
            };
            items.push(productItem);
          });
        }
        
        // Add child items for selected hierarchy elements
        // Process SKU Groups
        if (formData.selectedSKUGroups && formData.selectedSKUGroups.length > 0) {
          formData.selectedSKUGroups.forEach((group: any) => {
            const childItem: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              PromoOrderUID: promoOrderUID,
              ParentUID: parentUID,
              ItemCriteriaType: 'SKUGROUP',
              ItemCriteriaSelected: group.UID || group.uid,
              IsCompulsory: false,
              ItemUOM: 'PCS',
              MinQty: 1,
              PromoSplit: 0,
              SS: 0,
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString(),
              ServerAddTime: new Date().toISOString(),
              ServerModifiedTime: new Date().toISOString(),
              // Reserved for future use: HierarchySelections
            };
            items.push(childItem);
          });
        }
        
        // Process Brands
        if (formData.selectedBrands && formData.selectedBrands.length > 0) {
          formData.selectedBrands.forEach((brand: any) => {
            const childItem: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              PromoOrderUID: promoOrderUID,
              ParentUID: parentUID,
              ItemCriteriaType: 'BRAND',
              ItemCriteriaSelected: brand.UID || brand.uid,
              IsCompulsory: false,
              ItemUOM: 'PCS',
              MinQty: 1,
              PromoSplit: 0,
              SS: 0,
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString(),
              ServerAddTime: new Date().toISOString(),
              ServerModifiedTime: new Date().toISOString(),
              // Reserved for future use: HierarchySelections
            };
            items.push(childItem);
          });
        }
        
        console.log('[DEBUG] Added hierarchy selection with', items.length - 1, 'child items');
        
      } else if (actualSelectionMode === 'specific') {
        // SPECIFIC product selection - user directly selected products
        const productsToProcess = formData.specificProducts || formData.finalAttributeProducts || [];
        
        if (productsToProcess.length > 0) {
          // Add individual products with their quantities
          productsToProcess.forEach((product: any, index: number) => {
            const qty = product.quantity || product.Quantity || 1;
            const orderItem: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              PromoOrderUID: promoOrderUID,
              ParentUID: null,
              ItemCriteriaType: 'SKU',
              ItemCriteriaSelected: product.Code || product.ItemCode || product.UID || product.uid,
              IsCompulsory: false,
              ItemUOM: product.UOM || 'PCS',
              MinQty: qty,
              MaxQty: qty,
              PromoSplit: 0,
              SS: 0,
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString(),
              ServerAddTime: new Date().toISOString(),
              ServerModifiedTime: new Date().toISOString(),
              // Specific product with quantity
            };
            items.push(orderItem);
          });
          
          console.log('[DEBUG] Added', productsToProcess.length, 'specific products');
        }
      }
      return items;
    }
    
    // Special handling for MPROD - Multi-Product Configuration
    if (format === 'MPROD' && formData.productPromotions) {
      formData.productPromotions.forEach((config: any, configIndex: number) => {
        if (config.isActive && config.selectedProducts && config.selectedProducts.length > 0) {
          const configGroupId = configIndex + 1; // 1-based configuration group ID
          
          // Create order items for each selected product in this configuration
          config.selectedProducts.forEach((product: any, productIndex: number) => {
            const orderItem: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              PromoOrderUID: promoOrderUID,
              ParentUID: null, // Keep null for normal use
              ItemCriteriaType: 'SKU',
              ItemCriteriaSelected: product.Code || product.UID,
              IsCompulsory: false,
              ItemUOM: product.UOM || null,
              MinQty: product.quantity || product.Quantity || config.quantityThreshold || config.buyQty || 1,
              PromoSplit: 0, // Keep 0 for financial purposes
              SS: 0, // Keep SS as 0 for finance
              ActionType: 0,  // 0 = Add enum value
              CreatedTime: new Date().toISOString(),
              ModifiedTime: new Date().toISOString(),
              ServerAddTime: new Date().toISOString(),
              ServerModifiedTime: new Date().toISOString(),
              // New columns for MPROD configuration tracking
              ConfigGroupId: configGroupId,
              ConfigName: config.productName || `Configuration ${configGroupId}`,
              ConfigPromotionType: config.promotionType
            };
            // Set MaxQty based on product quantity or config threshold
            const productQty = product.quantity || product.Quantity;
            if (productQty && productQty > 0) {
              orderItem.MaxQty = productQty;
            } else if (config.quantityThreshold && config.quantityThreshold > 0) {
              orderItem.MaxQty = config.quantityThreshold;
            }
            items.push(orderItem);
          });
        }
      });
      return items;
    }
    
    // Default handling for other formats
    if (formData.productSelectionMode === 'specific' || formData.specificProducts) {
      const products = formData.specificProducts || formData.finalAttributeProducts || [];
      products.forEach((product: any, index: number) => {
        const orderItem: any = {
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          PromoOrderUID: promoOrderUID,
          ParentUID: null,
          ItemCriteriaType: 'SKU',
          ItemCriteriaSelected: product.Code || product.UID || product.ItemCode,
          IsCompulsory: false,
          ItemUOM: product.UOM || null,
          MinQty: product.quantity || 1,
          PromoSplit: 0,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          ServerAddTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString()
        };
        // Only include MaxQty if product has a specific quantity
        if (product.quantity && product.quantity > 0) {
          orderItem.MaxQty = product.quantity;
        }
        items.push(orderItem);
      });
    } else if (formData.productSelectionMode === 'hierarchy' && formData.productAttributes) {
      if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
        formData.finalAttributeProducts.forEach((product: any, index: number) => {
          const orderItem: any = {
            UID: this.generateUID(),
            PromotionUID: promotionUID,
            PromoOrderUID: promoOrderUID,
            ParentUID: null,
            ItemCriteriaType: 'SKU',
            ItemCriteriaSelected: product.Code || product.UID,
            IsCompulsory: false,
            ItemUOM: null,
            MinQty: product.quantity || 1,
            PromoSplit: 0,
            SS: 0,
            ActionType: 0,  // 0 = Add enum value
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString()
          };
          // Only include MaxQty if product has a specific quantity
          if (product.quantity && product.quantity > 0) {
            orderItem.MaxQty = product.quantity;
          }
          items.push(orderItem);
        });
      } else {
        formData.productAttributes.forEach((attr: any, index: number) => {
          items.push({
            UID: this.generateUID(),
            PromotionUID: promotionUID,
            PromoOrderUID: promoOrderUID,
            ParentUID: null,
            ItemCriteriaType: this.mapHierarchyType(attr.type || attr.level),
            ItemCriteriaSelected: attr.code || attr.value,
            IsCompulsory: attr.isCompulsory || false,
            ItemUOM: attr.uom || null,
            MinQty: 1,
            // MaxQty: null, // Don't send null values to avoid Dapper issues
            PromoSplit: 0,
            SS: 0,
            ActionType: 0,  // 0 = Add enum value
            CreatedTime: new Date().toISOString(),
            ModifiedTime: new Date().toISOString(),
            ServerAddTime: new Date().toISOString(),
            ServerModifiedTime: new Date().toISOString(),
            // Hierarchy and exclusion metadata - reserved for future use
          });
        });
      }
    } else if (formData.productSelectionMode === 'all') {
      items.push({
        UID: this.generateUID(),
        PromotionUID: promotionUID,
        PromoOrderUID: promoOrderUID,
        ParentUID: null,
        ItemCriteriaType: 'ALL',
        ItemCriteriaSelected: '*',
        IsCompulsory: false,
        ItemUOM: null,
        MinQty: 1,
        // No MaxQty for 'ALL' - no upper limit
        PromoSplit: 0,
        SS: 0,
        ActionType: 0,  // 0 = Add enum value
        CreatedTime: new Date().toISOString(),
        ModifiedTime: new Date().toISOString(),
        ServerAddTime: new Date().toISOString(),
        ServerModifiedTime: new Date().toISOString()
      });
    }
    
    return items;
  }

  // Build PromoOfferViewList
  private buildPromoOfferList(formData: any, promotionUID: string, promoOrderUID: string, employeeUID: string): any[] {
    const promoOfferUID = this.generateUID();
    const now = new Date().toISOString();
    
    // Determine offer type based on format - use specific types for clarity
    let offerType = 'Discount';
    let hasOfferItems = false;
    
    if (formData.format === 'IQXF') {
      offerType = 'SAME_ITEM_FREE';  // Same item free
      hasOfferItems = true;
    } else if (formData.format === 'BQXF') {
      offerType = 'DIFFERENT_ITEM_FREE';  // Different item free
      hasOfferItems = true;
    } else if (formData.format === 'IQPD') {
      offerType = 'PERCENTAGE';
    } else if (formData.format === 'IQFD') {
      offerType = 'FIXED_AMOUNT';
    } else if (formData.format === 'MPROD') {
      offerType = 'MULTI_PRODUCT';  // Multi-product configuration
      // Check if any product has FOC items
      if (formData.productPromotions) {
        hasOfferItems = formData.productPromotions.some((p: any) => 
          p.promotionType === 'IQXF' || p.promotionType === 'BQXF'
        );
      }
    }
    
    return [{
      // Use PascalCase for C# compatibility
      UID: promoOfferUID,
      PromotionUID: promotionUID,
      PromoOrderUID: promoOrderUID,
      Type: offerType,
      QualificationLevel: formData.level === 'invoice' ? 'INVOICE' : 'LINE',
      ApplicationLevel: formData.level === 'invoice' ? 'INVOICE' : 'LINE',
      SelectionModel: formData.selectionModel || 'any',
      HasOfferItemSelection: hasOfferItems,
      SS: 0,
      CreatedTime: now,
      ModifiedTime: now,
      ServerAddTime: now,
      ServerModifiedTime: now,
      CreatedBy: employeeUID,
      ModifiedBy: employeeUID,
      ActionType: 0  // 0 = Add enum value
    }];
  }

  // Build PromoOfferItemViewList (for FOC items)
  private buildPromoOfferItemList(formData: any, promoOfferList: any[], employeeUID: string): any[] {
    const items: any[] = [];
    
    // Handle IQXF promotion - create FOC item for the same product with free quantity
    if (formData.format === 'IQXF') {
      console.log('[DEBUG] Building IQXF PromoOfferItems');
      
      // For IQXF, the free item is the same as the buy item
      const buyProduct = formData.finalAttributeProducts?.[0] || formData.specificProducts?.[0];
      
      // Check multiple possible sources for free quantity
      let freeQuantity = formData.freeQuantity || formData.getQty || formData.getFree || formData.getQuantity || formData.freeQty;
      
      // Check if it's in buyGetConfig or buyXGetY structure
      if (!freeQuantity && formData.buyGetConfig) {
        freeQuantity = formData.buyGetConfig.getQty || formData.buyGetConfig.getFree || formData.buyGetConfig.freeQuantity;
      }
      
      // Check if it's in buyXGetY structure
      if (!freeQuantity && formData.buyXGetY) {
        freeQuantity = formData.buyXGetY.getQty || formData.buyXGetY.getFree || formData.buyXGetY.freeQuantity;
      }
      
      // Check if it's in offers configuration
      if (!freeQuantity && formData.offers && formData.offers.length > 0) {
        const offer = formData.offers[0];
        freeQuantity = offer.freeQuantity || offer.getQty || offer.getFree || offer.quantity;
      }
      
      // Check other possible configuration structures
      if (!freeQuantity && formData.configuration) {
        freeQuantity = formData.configuration.freeQuantity || formData.configuration.getQty || formData.configuration.getFree;
      }
      
      console.log('[DEBUG] IQXF buy product:', buyProduct);
      console.log('[DEBUG] IQXF free quantity:', freeQuantity);
      
      if (buyProduct && freeQuantity && freeQuantity > 0 && promoOfferList.length > 0) {
        const promoOfferUID = promoOfferList[0].UID;
        const promotionUID = promoOfferList[0].PromotionUID;
        
        const productCode = buyProduct.ItemCode || buyProduct.Code || buyProduct.UID;
        
        items.push({
          UID: this.generateUID(),
          PromoOfferUID: promoOfferUID,
          PromotionUID: promotionUID,
          ItemCriteriaType: 'SKU',
          ItemCriteriaSelected: productCode,
          ItemUOM: buyProduct.UOM || null,
          // Free quantity fields
          Quantity: freeQuantity,
          MinQuantity: freeQuantity,
          MaxQuantity: freeQuantity,
          // Product identification
          ProductUID: buyProduct.UID,
          ProductCode: productCode,
          // Additional fields
          IsCompulsory: false,
          IsActive: true,
          Deleted: false,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          CreatedBy: employeeUID,
          ModifiedBy: employeeUID
        });
        
        console.log('[DEBUG] Created IQXF PromoOfferItem:', items[0]);
      }
      
      return items;
    }
    
    // Handle BQXF and other FOC promotions
    let focItems = [];
    
    // Try different field names and structures
    if (formData.focSelectedProducts && Array.isArray(formData.focSelectedProducts)) {
      focItems = formData.focSelectedProducts;
    } else if (formData.focItems && Array.isArray(formData.focItems)) {
      focItems = formData.focItems;
    } else if (formData.focProducts && Array.isArray(formData.focProducts)) {
      focItems = formData.focProducts;
    } else if (formData.focItems && formData.focItems.guaranteed && Array.isArray(formData.focItems.guaranteed)) {
      focItems = formData.focItems.guaranteed;
    } else if (formData.focItems && formData.focItems.choice && formData.focItems.choice.products) {
      focItems = formData.focItems.choice.products;
    }
    
    console.log('[DEBUG] Building PromoOfferItems - FOC items count:', focItems.length);
    console.log('[DEBUG] FOC items data:', focItems);
    console.log('[DEBUG] Raw focItems structure:', formData.focItems);
    console.log('[DEBUG] Raw focSelectedProducts:', formData.focSelectedProducts);
    
    if (focItems.length > 0 && promoOfferList.length > 0) {
      const promoOfferUID = promoOfferList[0].UID;
      const promotionUID = promoOfferList[0].PromotionUID;
      
      focItems.forEach((focItem: any) => {
        // Extract proper product details
        const productCode = focItem.ItemCode || focItem.Code || focItem.code || focItem.productCode;
        const productUID = focItem.UID || focItem.productId || focItem.productUID;
        const quantity = focItem.quantity || focItem.Quantity || 1;
        
        items.push({
          UID: this.generateUID(),
          PromoOfferUID: promoOfferUID,
          PromotionUID: promotionUID,  // Add PromotionUID
          ItemCriteriaType: 'SKU',
          ItemCriteriaSelected: productCode,
          ItemUOM: focItem.UOM || focItem.uom || null,
          // Quantity fields properly mapped
          Quantity: quantity,
          MinQuantity: quantity,  // FOC items have fixed quantity
          MaxQuantity: quantity,  // FOC items have fixed quantity
          // Product identification
          ProductUID: productUID,
          ProductCode: productCode,
          // Additional fields
          IsCompulsory: false,
          IsActive: true,
          Deleted: false,
          SS: 0,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString(),
          CreatedBy: employeeUID,
          ModifiedBy: employeeUID
        });
      });
    }
    
    // Handle MPROD promotion - create FOC items for configurations with IQXF/BQXF promotions
    if (formData.format === 'MPROD' && formData.productPromotions) {
      console.log('[DEBUG] Building MPROD PromoOfferItems');
      
      formData.productPromotions.forEach((config: any, configIndex: number) => {
        if (config.isActive && promoOfferList.length > 0) {
          const promoOfferUID = promoOfferList[0].UID;
          const promotionUID = promoOfferList[0].PromotionUID;
          const configGroupId = configIndex + 1; // 1-based configuration group ID
          
          // Handle IQXF configurations (same item free for all selected products)
          console.log(`[DEBUG] Config ${configIndex + 1}: promotionType=${config.promotionType}, freeQty=${config.freeQty}, selectedProducts=${config.selectedProducts?.length}`);
          if (config.promotionType === 'IQXF' && config.freeQty > 0 && config.selectedProducts) {
            config.selectedProducts.forEach((product: any) => {
              items.push({
                UID: this.generateUID(),
                PromoOfferUID: promoOfferUID,
                PromotionUID: promotionUID,
                ItemCriteriaType: 'SKU',
                ItemCriteriaSelected: product.Code || product.UID,
                ItemUOM: product.UOM || null,
                Quantity: config.freeQty,
                MinQuantity: config.freeQty,
                MaxQuantity: config.freeQty,
                ProductUID: product.UID || null,
                ProductCode: product.Code || product.UID,
                IsCompulsory: false,
                IsActive: true,
                Deleted: false,
                SS: 0, // Keep SS as 0 for finance
                ActionType: 0,
                CreatedTime: new Date().toISOString(),
                ModifiedTime: new Date().toISOString(),
                CreatedBy: employeeUID,
                ModifiedBy: employeeUID,
                // New field for MPROD configuration support
                ConfigGroupId: configGroupId
              });
            });
          }
          
          // Handle BQXF configurations (different item free)
          console.log(`[DEBUG] Config ${configIndex + 1}: BQXF focSelectedProducts=${config.focSelectedProducts?.length}, focProducts=${config.focProducts?.length}`);
          if (config.promotionType === 'BQXF') {
            console.log(`[DEBUG] Config ${configIndex + 1}: Full BQXF data:`, JSON.stringify(config, null, 2));
          }
          if (config.promotionType === 'BQXF' && ((config.focSelectedProducts && config.focSelectedProducts.length > 0) || (config.focProducts && config.focProducts.length > 0))) {
            // Handle focSelectedProducts if available
            if (config.focSelectedProducts && config.focSelectedProducts.length > 0) {
              config.focSelectedProducts.forEach((focProduct: any) => {
                const qty = config.focProducts?.find((fp: any) => fp.productId === focProduct.UID)?.quantity || 1;
                items.push({
                  UID: this.generateUID(),
                  PromoOfferUID: promoOfferUID,
                  PromotionUID: promotionUID,
                  ItemCriteriaType: 'SKU',
                  ItemCriteriaSelected: focProduct.Code || focProduct.UID,
                  ItemUOM: focProduct.UOM || null,
                  Quantity: qty,
                  MinQuantity: qty,
                  MaxQuantity: qty,
                  ProductUID: focProduct.UID || null,
                  ProductCode: focProduct.Code || focProduct.UID,
                  IsCompulsory: false,
                  IsActive: true,
                  Deleted: false,
                  SS: 0, // Keep SS as 0 for finance
                  ActionType: 0,
                  CreatedTime: new Date().toISOString(),
                  ModifiedTime: new Date().toISOString(),
                  CreatedBy: employeeUID,
                  ModifiedBy: employeeUID,
                  // New field for MPROD configuration support
                  ConfigGroupId: configGroupId
                });
              });
            }
            
            // Handle focProducts if available (alternative structure)
            if (config.focProducts && config.focProducts.length > 0) {
              config.focProducts.forEach((focProduct: any) => {
                const qty = focProduct.quantity || focProduct.Quantity || 1;
                const productCode = focProduct.Code || focProduct.code || focProduct.productCode || focProduct.UID || focProduct.productId;
                items.push({
                  UID: this.generateUID(),
                  PromoOfferUID: promoOfferUID,
                  PromotionUID: promotionUID,
                  ItemCriteriaType: 'SKU',
                  ItemCriteriaSelected: productCode,
                ItemUOM: focProduct.UOM || null,
                Quantity: qty,
                MinQuantity: qty,
                MaxQuantity: qty,
                  ProductUID: focProduct.UID || focProduct.productId || null,
                  ProductCode: productCode,
                  IsCompulsory: false,
                  IsActive: true,
                  Deleted: false,
                  SS: 0, // Keep SS as 0 for finance
                  ActionType: 0,
                  CreatedTime: new Date().toISOString(),
                  ModifiedTime: new Date().toISOString(),
                  CreatedBy: employeeUID,
                  ModifiedBy: employeeUID,
                  // New field for MPROD configuration support
                  ConfigGroupId: configGroupId
                });
              });
            }
          }
        }
      });
    }
    
    return items;
  }

  // Build PromoConditionViewList - PROPERLY HANDLES SLABS
  private buildPromoConditionList(formData: any, promoOrderList: any[], employeeUID: string): any[] {
    const conditions = [];
    const now = new Date().toISOString();
    
    // Get PromotionUID from promoOrderList
    const promotionUID = promoOrderList.length > 0 ? promoOrderList[0].PromotionUID : null;
    
    // Check if promotion has slabs
    if (formData.hasSlabs && formData.slabs && formData.slabs.length > 0) {
      // Create multiple conditions for each slab
      formData.slabs.forEach((slab: any) => {
        if (promoOrderList.length > 0 && promotionUID) {
          const slabCondition: any = {
            // Use PascalCase for C# compatibility
            UID: this.generateUID(),
            PromotionUID: promotionUID,
            ReferenceUID: promoOrderList[0].UID,
            ReferenceType: 'PromoOrder',
            ConditionType: this.mapConditionType(formData.format),
            Min: slab.minQty || slab.minValue || 1,
            MaxDealCount: formData.maxDealCount || formData.maxApplicationsPerInvoice || formData.volumeCaps?.invoiceCap?.maxApplications || 100,
            UOM: null,
            AllUOMConversion: false,
            ValueType: slab.discountPercent ? 'PERCENTAGE' : 'AMOUNT',
            IsProrated: false,
            SS: 0, // Use index as sequence
            // Add discount fields from slab
            DiscountType: slab.discountPercent ? 'PERCENTAGE' : 'FIXED_AMOUNT',
            DiscountPercentage: slab.discountPercent || null,
            DiscountAmount: slab.discountAmount || slab.fixedDiscountAmount || null,
            FreeQuantity: null,
            CreatedTime: now,
            ModifiedTime: now,
            ServerAddTime: now,
            ServerModifiedTime: now,
            CreatedBy: employeeUID || 'SYSTEM',
            ModifiedBy: employeeUID || 'SYSTEM',
            ActionType: 0  // 0 = Add enum value
          };
          // Only include Max if specified in slab
          if (slab.maxQty || slab.maxValue) {
            slabCondition.Max = slab.maxQty || slab.maxValue;
          }
          conditions.push(slabCondition);
        }
      });
    } else if (formData.format === 'MPROD' && formData.productPromotions) {
      // MPROD - Multi-Product Configuration - create condition for each configuration group
      formData.productPromotions.forEach((config: any, configIndex: number) => {
        if (config.isActive && config.selectedProducts && config.selectedProducts.length > 0) {
          const configGroupId = configIndex + 1;
          // Find order items for this configuration group using ConfigGroupId
          const orderItems = promoOrderList.filter(item => item.ConfigGroupId === configGroupId);
          
          console.log(`[DEBUG] Config ${configGroupId}: Found ${orderItems.length} order items`, orderItems.map(i => ({ConfigGroupId: i.ConfigGroupId, UID: i.UID})));
          
          if (orderItems.length > 0) {
            // Create config details JSON for storage
            const configDetails = {
              configName: config.productName,
              promotionType: config.promotionType,
              productCount: config.selectedProducts.length,
              hasSlabs: config.hasSlabs || false,
              maxDiscountAmount: config.maxDiscountAmount || null
            };
            
            const productCondition: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              ReferenceUID: orderItems[0].UID, // Reference the order item
              ReferenceType: 'PromoOrder',
              ConditionType: 'QUANTITY',
              Min: config.quantityThreshold || config.buyQty || 1,
              MaxDealCount: config.maxApplications || 100,
              UOM: null,
              AllUOMConversion: false,
              ValueType: this.getProductPromoValueType(config),
              IsProrated: false,
              SS: 0, // Keep SS as 0 for finance
              // Set discount type and values based on promotion type
              DiscountType: this.getProductPromoDiscountType(config),
              DiscountPercentage: config.promotionType === 'IQPD' ? (config.discountPercent || config.discountPercentage) : null,
              DiscountAmount: config.promotionType === 'IQFD' ? config.discountAmount : null,
              FreeQuantity: (config.promotionType === 'IQXF' || config.promotionType === 'BQXF') ? 
                           (config.freeQty || config.getQty) : null,
              ActionType: 0,
              CreatedTime: now,
              ModifiedTime: now,
              // New columns for MPROD tracking
              ConfigGroupId: configGroupId,
              ConfigDetails: JSON.stringify(configDetails)
            };
            
            conditions.push(productCondition);
          } else {
            console.warn(`[WARN] No order items found for config group ${configGroupId}. Creating condition anyway.`);
            // Create condition without specific order item reference
            const configDetails = {
              configName: config.productName,
              promotionType: config.promotionType,
              productCount: config.selectedProducts.length,
              hasSlabs: config.hasSlabs || false,
              maxDiscountAmount: config.maxDiscountAmount || null
            };
            
            const fallbackCondition: any = {
              UID: this.generateUID(),
              PromotionUID: promotionUID,
              ReferenceUID: promotionUID, // Reference promotion directly
              ReferenceType: 'PROMOTION',
              ConditionType: 'QUANTITY',
              Min: config.quantityThreshold || config.buyQty || 1,
              MaxDealCount: config.maxApplications || 100,
              UOM: null,
              AllUOMConversion: false,
              ValueType: this.getProductPromoValueType(config),
              IsProrated: false,
              SS: 0,
              DiscountType: this.getProductPromoDiscountType(config),
              DiscountPercentage: config.promotionType === 'IQPD' ? (config.discountPercent || config.discountPercentage) : null,
              DiscountAmount: config.promotionType === 'IQFD' ? config.discountAmount : null,
              FreeQuantity: (config.promotionType === 'IQXF' || config.promotionType === 'BQXF') ? 
                           (config.freeQty || config.getQty) : null,
              ActionType: 0,
              CreatedTime: now,
              ModifiedTime: now,
              ConfigGroupId: configGroupId,
              ConfigDetails: JSON.stringify(configDetails)
            };
            
            conditions.push(fallbackCondition);
          }
        }
      });
    } else {
      // Single condition for other non-slab promotions
      if (promoOrderList.length > 0 && promotionUID) {
        // Set minimum value based on format type
        let minValue = 1;
        
        // Invoice level formats - set specific thresholds based on format
        if (formData.format === 'BYVALUE') {
          minValue = formData.minValue || 0;
        } else if (formData.format === 'BYQTY') {
          minValue = formData.minQty || 1;
        } else if (formData.format === 'LINECOUNT') {
          minValue = formData.minLineCount || 1;
        } else if (formData.format === 'BRANDCOUNT') {
          minValue = formData.minBrandCount || 1;
        } else if (formData.format === 'ANYVALUE') {
          minValue = 0; // No minimum threshold
        } else if (formData.format === 'IQXF' || formData.format === 'BQXF') {
          // Get the minimum quantity from selected products
          if (formData.specificProducts && formData.specificProducts.length > 0) {
            // Use the quantity from the first selected product (for single product IQXF)
            minValue = formData.specificProducts[0].quantity || 1;
          } else if (formData.finalAttributeProducts && formData.finalAttributeProducts.length > 0) {
            minValue = formData.finalAttributeProducts[0].quantity || 1;
          }
          
          // Check multiple possible sources for buy quantity
          if (!minValue || minValue === 1) {
            minValue = formData.buyQuantity || formData.buyQty || formData.minQty || formData.minQuantity;
            
            // Check in buyGetConfig or buyXGetY structure
            if (!minValue && formData.buyGetConfig) {
              minValue = formData.buyGetConfig.buyQty || formData.buyGetConfig.buyQuantity || formData.buyGetConfig.minQty;
            }
            
            // Check in buyXGetY structure
            if (!minValue && formData.buyXGetY) {
              minValue = formData.buyXGetY.buyQty || formData.buyXGetY.buyQuantity || formData.buyXGetY.minQty;
            }
            
            // Check in configuration
            if (!minValue && formData.configuration) {
              minValue = formData.configuration.buyQty || formData.configuration.buyQuantity || formData.configuration.minQty;
            }
            
            // Fallback to default
            minValue = minValue || 1;
          }
        }
          
        // For IQXF/BQXF, include free quantity in the main condition
        let freeQty = null;
        if (formData.format === 'IQXF') {
          // For IQXF - get free quantity from form fields
          freeQty = formData.freeQuantity || formData.getQty || formData.getFree || formData.getQuantity || formData.freeQty;
          
          if (!freeQty && formData.buyGetConfig) {
            freeQty = formData.buyGetConfig.getQty || formData.buyGetConfig.getFree || formData.buyGetConfig.freeQuantity;
          }
          
          if (!freeQty && formData.buyXGetY) {
            freeQty = formData.buyXGetY.getQty || formData.buyXGetY.getFree || formData.buyXGetY.freeQuantity;
          }
          
          if (!freeQty && formData.offers && formData.offers.length > 0) {
            const offer = formData.offers[0];
            freeQty = offer.freeQuantity || offer.getQty || offer.getFree || offer.quantity;
          }
        } else if (formData.format === 'BQXF') {
          // For BQXF - calculate total free quantity from FOC items
          let focItems = formData.focSelectedProducts || formData.focItems || formData.focProducts || [];
          
          // Handle nested structure
          if (!Array.isArray(focItems) && focItems) {
            if (focItems.guaranteed && Array.isArray(focItems.guaranteed)) {
              focItems = focItems.guaranteed;
            } else if (focItems.choice && focItems.choice.products) {
              focItems = focItems.choice.products;
            }
          }
          
          // Sum up quantities from all FOC items
          if (Array.isArray(focItems) && focItems.length > 0) {
            freeQty = focItems.reduce((total: number, item: any) => {
              const itemQty = item.quantity || item.Quantity || 1;
              return total + itemQty;
            }, 0);
          }
          
          console.log('[DEBUG] BQXF free quantity calculated:', freeQty, 'from items:', focItems);
        }
        
        const condition: any = {
          UID: this.generateUID(),
          PromotionUID: promotionUID,
          ReferenceUID: promoOrderList[0].UID,
          ReferenceType: 'PromoOrder',
          ConditionType: this.mapConditionType(formData.format),
          Min: minValue,
          // Additional fields for promo_condition table
          MaxDealCount: formData.maxDealCount || formData.maxApplicationsPerInvoice || formData.volumeCaps?.invoiceCap?.maxApplications || 100,
          UOM: null,
          AllUOMConversion: false,
          ValueType: this.getValueType(formData),
          IsProrated: false,
          SS: 0,
          // Add discount/free fields based on format - use specific types
          DiscountType: formData.format === 'IQFD' ? 'FIXED_AMOUNT' : 
                       (formData.format === 'IQPD' ? 'PERCENTAGE' : 
                       (formData.format === 'IQXF' ? 'IQXF' : 
                       (formData.format === 'BQXF' ? 'BQXF' :
                       // Invoice level formats - determine discount type based on form data
                       ((formData.format === 'BYVALUE' || formData.format === 'BYQTY' || formData.format === 'LINECOUNT' || formData.format === 'BRANDCOUNT' || formData.format === 'ANYVALUE') ?
                         (formData.discountPercent || formData.discountPercentage ? 'PERCENTAGE' : 'FIXED_AMOUNT') : null)))),
          DiscountPercentage: (formData.format === 'IQPD' || 
                              (formData.format === 'BYVALUE' || formData.format === 'BYQTY' || formData.format === 'LINECOUNT' || formData.format === 'BRANDCOUNT' || formData.format === 'ANYVALUE')) ? 
                             (formData.discountPercent || formData.discountPercentage || 0) : 
                             (formData.format === 'IQXF' || formData.format === 'BQXF' ? null : null),
          DiscountAmount: (formData.format === 'IQFD' || 
                          (formData.format === 'BYVALUE' || formData.format === 'BYQTY' || formData.format === 'LINECOUNT' || formData.format === 'BRANDCOUNT' || formData.format === 'ANYVALUE')) ? 
                         (formData.discountAmount || formData.fixedDiscountAmount || 0) : 
                         (formData.format === 'IQXF' || formData.format === 'BQXF' ? null : null),
          FreeQuantity: (formData.format === 'IQXF' || formData.format === 'BQXF') ? freeQty : null,
          ActionType: 0,  // 0 = Add enum value
          CreatedTime: new Date().toISOString(),
          ModifiedTime: new Date().toISOString()
        };
        // Only include Max if there's a meaningful maximum value
        if (formData.maxQuantity || formData.maxValue) {
          condition.Max = formData.maxQuantity || formData.maxValue;
        }
        conditions.push(condition);
      }
    }
    
    // No need for additional offer conditions - all info is in the main condition
    
    return conditions;
  }

  // Build organization mappings
  private buildOrganizationMappings(formData: any, promotionUID: string): any {
    const schemeOrgs: any[] = [];
    const schemeBranches: any[] = [];
    const now = new Date().toISOString();
    
    if (formData.footprint) {
      // Map organizations to scheme_org table
      const organizations = formData.footprint.organizations || formData.footprint.selectedOrgs || [];
      
      if (organizations && organizations.length > 0) {
        organizations.forEach((org: any) => {
          const orgUID = typeof org === 'string' ? org : (org.UID || org.uid || org.id);
          
          if (orgUID) {
            schemeOrgs.push({
              // Use PascalCase for C# compatibility
              UID: this.generateUID(),
              LinkedItemType: 'PROMOTION',
              LinkedItemUID: promotionUID,
              OrgUID: orgUID,
              CreatedTime: now,
              ModifiedTime: now,
              ActionType: 0  // 0 = Add enum value
            });
          }
        });
      }
      
      // FIXED: Map branches/stores - handle all possible field names
      const stores = formData.footprint.branches || 
                    formData.footprint.stores || 
                    formData.footprint.selectedStores || 
                    [];
                    
      if (stores && stores.length > 0) {
        stores.forEach((store: any) => {
          // Handle both object format and string format
          const storeUID = typeof store === 'string' ? store : (store.UID || store.uid || store.id);
          
          if (storeUID) {
            schemeBranches.push({
              // Use PascalCase for C# compatibility
              UID: this.generateUID(),
              LinkedItemType: 'PROMOTION',
              LinkedItemUID: promotionUID,
              BranchCode: storeUID,
              CreatedTime: now,
              ModifiedTime: now,
              SchemeUID: promotionUID,
              ActionType: 0  // 0 = Add enum value
            });
          }
          
          // Also add the organization from the store if not already added
          if (typeof store === 'object' && store.FranchiseeOrgUID && !schemeOrgs.some(org => org.OrgUID === store.FranchiseeOrgUID)) {
            schemeOrgs.push({
              UID: this.generateUID(),
              LinkedItemType: 'PROMOTION',
              LinkedItemUID: promotionUID,  // This serves as the scheme UID
              OrgUID: store.FranchiseeOrgUID,
              ActionType: 0  // 0 = Add enum value
            });
          }
        });
      }
      
      // ADDITIONAL: Handle specificStores array which contains detailed store objects
      if (formData.footprint.specificStores && formData.footprint.specificStores.length > 0) {
        formData.footprint.specificStores.forEach((store: any) => {
          const storeUID = store.uid || store.UID || store.id;
          
          if (storeUID && !schemeBranches.some(branch => branch.BranchCode === storeUID)) {
            schemeBranches.push({
              UID: this.generateUID(),
              LinkedItemType: 'PROMOTION',
              LinkedItemUID: promotionUID,
              BranchCode: storeUID,
              ActionType: 0  // 0 = Add enum value
            });
          }
          
          // Add organization from specific store if available
          if (store.FranchiseeOrgUID && !schemeOrgs.some(org => org.OrgUID === store.FranchiseeOrgUID)) {
            schemeOrgs.push({
              UID: this.generateUID(),
              LinkedItemType: 'PROMOTION',
              LinkedItemUID: promotionUID,
              OrgUID: store.FranchiseeOrgUID,
              ActionType: 0  // 0 = Add enum value
            });
          }
        });
      }
    }
    
    // IMPORTANT: Ensure at least one org is present (required by backend)
    if (schemeOrgs.length === 0) {
      const defaultOrgUID = this.extractPrimaryOrgUID(formData);
      if (defaultOrgUID && defaultOrgUID !== 'UNIVERSAL') {
        schemeOrgs.push({
          // Use PascalCase for C# compatibility
          UID: this.generateUID(),
          LinkedItemType: 'PROMOTION',
          LinkedItemUID: promotionUID,
          OrgUID: defaultOrgUID,
          CreatedTime: now,
          ModifiedTime: now,
          ActionType: 0  // 0 = Add enum value
        });
      }
    }
    
    return { schemeOrgs, schemeBranches };
  }

  // Helper functions
  private mapHierarchyType(type: string): string {
    const typeMap: any = {
      'category': 'Category',
      'brand': 'Brand',
      'subcategory': 'SubCategory',
      'product_group': 'ProductGroup',
      'parent': 'Category',
      'child': 'SubCategory',
      'sub_child': 'Brand'
    };
    return typeMap[type?.toLowerCase()] || type || 'Category';
  }

  private mapConditionType(format: string): string {
    const conditionMap: any = {
      'IQFD': 'QUANTITY',
      'IQPD': 'QUANTITY',
      'IQXF': 'QUANTITY',
      'BQXF': 'QUANTITY',
      'BYVALUE': 'VALUE',
      'BYQTY': 'QUANTITY',
      'LINECOUNT': 'LINE_COUNT',
      'BRANDCOUNT': 'BRAND_COUNT',
      'ANYVALUE': 'ANY'
    };
    return conditionMap[format] || 'QUANTITY';
  }

  // Removed buildOfferCondition - no longer needed as all conditions are in main condition

  private extractCompanyUID(formData: any): string {
    const companyUID = formData.companyUID || 
           formData.footprint?.company?.UID || 
           formData.footprint?.organizations?.[0]?.CompanyUID ||
           'WINIT';  // Use a known valid company
    console.log('Extracted CompanyUID:', companyUID);
    return companyUID;
  }

  private extractPrimaryOrgUID(formData: any): string {
    // Check if footprint is "all" - for universal promotions
    if (formData.footprint?.selectionType === 'all' || 
        formData.footprintType === 'all' ||
        formData.storeSelection === 'all') {
      console.log('Universal promotion - footprint is ALL');
      // For universal promotions, we need to use a special marker
      // that the backend will recognize and handle appropriately
      return 'UNIVERSAL';
    }
    
    // Get the store data which has valid FranchiseeOrgUID
    const firstStore = formData.footprint?.selectedStores?.[0] || 
                       formData.footprint?.stores?.[0];
    
    // Use the FranchiseeOrgUID from store data as it's guaranteed to be valid
    if (firstStore?.FranchiseeOrgUID) {
      console.log('Using FranchiseeOrgUID from store:', firstStore.FranchiseeOrgUID);
      return firstStore.FranchiseeOrgUID;
    }
    
    // Fallback to form data sources
    const orgUID = formData.orgUID || 
                   formData.footprint?.organizationUID || 
                   formData.footprint?.finalOrgUID ||
                   formData.footprint?.organizations?.[0]?.UID;
    
    if (orgUID && orgUID !== 'ALL') {
      console.log('Using OrgUID from form data:', orgUID);
      return orgUID;
    }
    
    // For universal promotions without specific org
    console.log('No specific OrgUID - treating as universal promotion');
    return 'UNIVERSAL';
  }

  private determineCategory(formData: any): string {
    return formData.category || 'GENERAL';
  }

  private generateUID(): string {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
      const r = Math.random() * 16 | 0;
      const v = c === 'x' ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    }).toUpperCase();
  }

  private getProductPromoDiscountType(productPromo: any): string {
    // Get discount type for individual product in MPROD
    switch(productPromo.promotionType) {
      case 'IQFD': return 'FIXED_AMOUNT';
      case 'IQPD': return 'PERCENTAGE';
      case 'IQXF': return 'IQXF';
      case 'BQXF': return 'BQXF';
      default: return 'FIXED_AMOUNT';
    }
  }

  private getProductPromoValueType(productPromo: any): string {
    // Get value type for individual product in MPROD
    switch(productPromo.promotionType) {
      case 'IQFD': return 'AMOUNT';
      case 'IQPD': return 'PERCENTAGE';
      case 'IQXF': 
      case 'BQXF': return 'QUANTITY';
      default: return 'AMOUNT';
    }
  }

  private getValueType(formData: any): string {
    // Determine value type based on promotion format
    if (formData.format === 'IQPD' || formData.discountPercent > 0) {
      return 'PERCENTAGE';
    } else if (formData.format === 'IQFD' || formData.discountAmount > 0) {
      return 'AMOUNT';
    } else if (formData.format === 'IQXF' || formData.format === 'BQXF') {
      return 'QUANTITY';
    }
    return 'AMOUNT';
  }

  // Clean undefined/null values from objects before sending to backend
  private cleanObject(obj: any): any {
    try {
      if (obj === null || obj === undefined) {
        return obj;
      }
      
      if (Array.isArray(obj)) {
        return obj.map(item => this.cleanObject(item));
      } 
      
      if (typeof obj === 'object') {
        // Handle primitive wrapper objects, functions, etc.
        if (obj.constructor !== Object && obj.constructor !== Array) {
          // For objects like Date, RegExp, etc., return as-is or convert to string
          if (obj instanceof Date) {
            return obj.toISOString();
          }
          return obj;
        }
        
        const cleaned: any = {};
        Object.keys(obj).forEach(key => {
          const value = obj[key];
          if (value !== null && value !== undefined) {
            cleaned[key] = this.cleanObject(value);
          }
        });
        return cleaned;
      }
      
      return obj;
    } catch (error) {
      console.error('[DEBUG] Error cleaning object:', error, 'Original object:', obj);
      return obj; // Return original if cleaning fails
    }
  }

  // Update existing promotion with old API
  async updatePromotion(promotionId: string, formData: PromotionV3FormData): Promise<PromotionV3Response> {
    try {
      const { API_CONFIG, getCommonHeaders } = await import('./api-config');
      const { authService } = await import('@/lib/auth-service');
      
      const currentUser = authService.getCurrentUser();
      const employeeUID = currentUser?.uid || currentUser?.id || null;
      
      const promoMasterView = this.convertToOldAPIFormat(formData, employeeUID || '');
      promoMasterView.IsNew = false;
      promoMasterView.PromotionView.UID = promotionId;
      promoMasterView.PromotionView.ActionType = 'Update';
      
      // Clean the data to remove null/undefined values
      const cleanedPromoMasterView = this.cleanObject(promoMasterView);
      
      // Step 1: Update main promotion
      const response = await fetch(`${API_CONFIG.baseURL}/Promotion/CUDPromotionMaster`, {
        method: 'POST',
        headers: {
          ...getCommonHeaders(),
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(cleanedPromoMasterView)
      });

      const result = await response.json();
      
      if (result.Data <= 0) {
        return {
          success: false,
          error: 'Failed to update promotion'
        };
      }
      
      // Volume caps and product configs are now handled within CUDPromotionMaster
      // No additional API calls needed - everything is updated in one transaction
      
      return {
        success: true,
        data: {} as any
      };
      
    } catch (error: any) {
      return {
        success: false,
        error: error.message || 'Failed to update promotion'
      };
    }
  }

  // Get promotion details using old API
  async getPromotionDetails(promotionId: string): Promise<any> {
    try {
      const { API_CONFIG, getCommonHeaders } = await import('./api-config');
      
      const response = await fetch(
        `${API_CONFIG.baseURL}/Promotion/GetPromotionDetailsByUID?PromotionUID=${promotionId}`,
        {
          method: 'GET',
          headers: getCommonHeaders()
        }
      );

      const result = await response.json();
      
      // Debug logging
      console.log('[DEBUG] ========== PROMOTION DATA RECEIVED ==========');
      console.log('[DEBUG] Full response:', result);
      if (result.Data) {
        console.log('[DEBUG] Promotion Name:', result.Data.PromotionView?.Name);
        console.log('[DEBUG] PromoOrders count:', result.Data.PromoOrderViewList?.length || 0);
        console.log('[DEBUG] PromoOrderItems count:', result.Data.PromoOrderItemViewList?.length || 0);
        console.log('[DEBUG] PromoOffers count:', result.Data.PromoOfferViewList?.length || 0);
        console.log('[DEBUG] PromoOfferItems count:', result.Data.PromoOfferItemViewList?.length || 0);
        console.log('[DEBUG] PromoConditions count:', result.Data.PromoConditionViewList?.length || 0);
        console.log('[DEBUG] ItemPromotionMaps (Products) count:', result.Data.ItemPromotionMapViewList?.length || 0);
        console.log('[DEBUG] VolumeCap:', result.Data.PromotionVolumeCap ? 'Present' : 'NULL');
        console.log('[DEBUG] SchemeOrgs (Stores) count:', result.Data.SchemeOrgs?.length || 0);
        console.log('[DEBUG] SchemeBranches count:', result.Data.SchemeBranches?.length || 0);
        
        // Log actual data for debugging
        if (result.Data.PromoOrderViewList?.length > 0) {
          console.log('[DEBUG] First PromoOrder:', result.Data.PromoOrderViewList[0]);
        }
        if (result.Data.ItemPromotionMapViewList?.length > 0) {
          console.log('[DEBUG] First ItemPromotionMap:', result.Data.ItemPromotionMapViewList[0]);
        }
        if (result.Data.SchemeOrgs?.length > 0) {
          console.log('[DEBUG] First SchemeOrg:', result.Data.SchemeOrgs[0]);
        }
      }
      console.log('[DEBUG] ==============================================');
      
      return result.Data || null;
      
    } catch (error) {
      console.error('Error fetching promotion:', error);
      return null;
    }
  }

  // Alias for backward compatibility
  async getPromotionById(promotionId: string): Promise<any> {
    return this.getPromotionDetails(promotionId);
  }

  // Delete promotion using old API (soft delete)
  async deletePromotion(promotionId: string): Promise<PromotionV3Response> {
    try {
      const { API_CONFIG, getCommonHeaders } = await import('./api-config');
      
      const response = await fetch(
        `${API_CONFIG.baseURL}/Promotion/SoftDeletePromotionByUID?PromotionUID=${promotionId}`,
        {
          method: 'PUT',
          headers: getCommonHeaders()
        }
      );

      const result = await response.json();
      
      return {
        success: result.Data?.Success || false,
        data: {} as any
      };
      
    } catch (error: any) {
      return {
        success: false,
        error: error.message || 'Failed to delete promotion'
      };
    }
  }

  // Check if promotion code is unique (global uniqueness)
  async validatePromotionCode(code: string, promotionUID?: string): Promise<{ isUnique: boolean; message?: string }> {
    try {
      const { API_CONFIG, getCommonHeaders } = await import('./api-config');
      
      // Build the API endpoint (orgUID is optional since validation is global)
      const url = new URL(`${API_CONFIG.baseURL}/Promotion/ValidatePromotionCode`);
      url.searchParams.append('code', code);
      
      // Try to get organization UID (optional for global validation)
      try {
        const { useAuthStore } = await import('@/stores/auth-store');
        const user = useAuthStore.getState().getCurrentUser();
        const orgUID = user?.currentOrganization?.uid;
        if (orgUID) {
          url.searchParams.append('orgUID', orgUID);
        }
      } catch (error) {
        console.log('Auth store not available, using global validation without orgUID');
      }
      
      if (promotionUID) {
        url.searchParams.append('promotionUID', promotionUID);
      }
      
      const response = await fetch(url.toString(), {
        method: 'GET',
        headers: getCommonHeaders()
      });

      const result = await response.json();
      
      if (result.Data?.IsValid === false) {
        let message = 'Promotion code already exists.';
        if (result.Data?.ConflictType === 1) { // Promotions.Code
          message = 'This promotion code is already in use. Please choose a different code.';
        }
        return {
          isUnique: false,
          message: message
        };
      }
      
      return {
        isUnique: true
      };
      
    } catch (error: any) {
      console.error('Error validating promotion code:', error);
      // In case of API error, assume code might be valid to not block user
      return {
        isUnique: true,
        message: 'Unable to validate code uniqueness. Please ensure the code is unique.'
      };
    }
  }

  // Generate a unique promotion code based on name
  generatePromotionCode(name: string): string {
    const cleanName = name.replace(/[^a-zA-Z0-9]/g, '').toUpperCase();
    const namePrefix = cleanName.slice(0, 6);
    const timestamp = Date.now().toString().slice(-6);
    return `${namePrefix}${timestamp}`;
  }
}

export const promotionV3Service = new PromotionV3Service();