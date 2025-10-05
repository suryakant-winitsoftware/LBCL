import { apiRequest, buildPagingRequest } from '../lib/api-config';

class PurchaseOrderService {

  /**
   * Get authentication token dynamically
   */
  getAuthToken() {
    // First try auth_token (used by product management)
    const authToken = typeof window !== 'undefined' ? localStorage.getItem('auth_token') : null;

    if (authToken && authToken !== 'null' && authToken !== 'undefined') {
      console.log('🔑 Using auth_token from localStorage');
      return authToken;
    }

    // Try authToken as fallback
    const authTokenAlt = typeof window !== 'undefined' ? localStorage.getItem('authToken') : null;
    if (authTokenAlt && authTokenAlt !== 'null' && authTokenAlt !== 'undefined') {
      console.log('🔑 Using authToken from localStorage');
      return authTokenAlt;
    }

    // Fallback to get from currentUser session
    const currentUser = typeof window !== 'undefined' ? localStorage.getItem('currentUser') : null;

    if (currentUser) {
      try {
        const user = JSON.parse(currentUser);
        if (user.token && user.token !== 'null' && user.token !== 'undefined') {
          console.log('🔑 Using token from currentUser session');
          return user.token;
        }
      } catch (e) {
        console.error('Error parsing currentUser:', e);
      }
    }

    console.warn('⚠️ No valid auth token found');
    return null;
  }

  /**
   * Get Purchase Order Headers
   * Endpoint: POST /api/PurchaseOrder/GetPurchaseOrderHeaders
   */
  async getPurchaseOrderHeaders(filters = {}) {
    try {
      // Get fresh token dynamically
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      console.log('🔑 Using token for API call:', token.substring(0, 30) + '...');

      const requestBody = buildPagingRequest(
        filters.page || 1,
        filters.pageSize || 50,
        filters.searchQuery,
        {
          Status: filters.status,
          StartDate: filters.startDate,
          EndDate: filters.endDate,
          PurchaseOrderNo: filters.purchaseOrderNo,
          SAPStatus: filters.sapStatus
        },
        filters.sortField,
        filters.sortDirection
      );

      console.log('📤 Request body:', requestBody);

      const response = await apiRequest(
        '/PurchaseOrder/GetPurchaseOrderHeaders',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        let headers = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        // Debug: Log the first order to see what fields are available
        if (headers && headers.length > 0) {
          console.log('📦 Purchase Order Header Sample:', headers[0]);
          console.log('🏢 Warehouse fields in PO:', {
            warehouse: headers[0].warehouse,
            Warehouse: headers[0].Warehouse,
            warehouse_uid: headers[0].warehouse_uid,
            wareHouseUID: headers[0].wareHouseUID,
            WareHouseUID: headers[0].WareHouseUID,
            warehouseUID: headers[0].warehouseUID
          });
        }

        // Ensure warehouse fields and UID are properly mapped for each header
        if (Array.isArray(headers)) {
          headers = headers.map(header => {
            if (header) {
              const warehouseIdField = header.warehouse_uid || header.WareHouseUID || header.wareHouseUID;
              const warehouseNameField = header.warehouse || header.Warehouse || header.warehouseName || header.WarehouseName;
              const uidField = header.UID || header.uid || header.Uid;
              return {
                ...header,
                uid: uidField,
                UID: uidField,
                wareHouseUID: warehouseIdField,
                warehouse_uid: warehouseIdField,
                warehouse: warehouseNameField,
                Warehouse: warehouseNameField
              };
            }
            return header;
          });
        }

        return {
          success: true,
          data: response.data,
          headers: Array.isArray(headers) ? headers : [],
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch purchase order headers');
      }
    } catch (error) {
      console.error('Error fetching purchase order headers:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch purchase order headers',
        headers: [],
        totalCount: 0
      };
    }
  }

  /**
   * Get Purchase Order Master by UID
   * Endpoint: GET /api/PurchaseOrder/GetPurchaseOrderMasterByUID
   */
  async getPurchaseOrderMasterByUID(uid) {
    try {
      const token = this.getAuthToken();

      if (!uid) {
        throw new Error('Purchase Order UID is required');
      }

      const response = await apiRequest(
        `/PurchaseOrder/GetPurchaseOrderMasterByUID?uid=${encodeURIComponent(uid)}`,
        {
          method: 'GET'
        },
        token
      );

      if (response.success) {
        const masterData = response.data?.Data || response.data;
        // Ensure warehouse_uid is properly mapped
        if (masterData && (masterData.warehouse_uid || masterData.WareHouseUID || masterData.wareHouseUID)) {
          masterData.wareHouseUID = masterData.warehouse_uid || masterData.WareHouseUID || masterData.wareHouseUID;
          masterData.warehouse_uid = masterData.warehouse_uid || masterData.WareHouseUID || masterData.wareHouseUID;
        }
        return {
          success: true,
          data: masterData,
          master: masterData
        };
      } else {
        throw new Error(response.error || 'Failed to fetch purchase order master');
      }
    } catch (error) {
      console.error('Error fetching purchase order master:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch purchase order master',
        master: null
      };
    }
  }

  /**
   * Create, Update, or Delete Purchase Order
   * Endpoint: POST /api/PurchaseOrder/CUD_PurchaseOrder
   */
  async cudPurchaseOrder(actionType, purchaseOrderHeader, purchaseOrderLines = [], purchaseOrderLineProvisions = []) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      // Map actionType to numeric value
      // 0 = CREATE, 1 = UPDATE, 2 = DELETE
      const actionTypeMap = {
        'CREATE': 0,
        'UPDATE': 1,
        'DELETE': 2,
        0: 0,
        1: 1,
        2: 2
      };

      const numericActionType = actionTypeMap[actionType] ?? actionTypeMap[actionType.toUpperCase()];

      if (numericActionType === undefined) {
        throw new Error('Valid action type (CREATE/0, UPDATE/1, DELETE/2) is required');
      }

      // Generate UIDs and add required base model fields
      const generateUID = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
      };

      const currentTime = new Date().toISOString();
      // Use empty string to bypass FK constraint - backend will handle it
      const userUID = typeof window !== 'undefined' ? localStorage.getItem('userUID') : '';
      // Use empty string if invalid to let backend handle FK constraint
      const validUserUID = (userUID && userUID !== 'default-user' && userUID !== 'system-user' && userUID !== 'null') ? userUID : '';

      // Ensure header has proper structure with base model fields
      const headerUID = purchaseOrderHeader.uid || generateUID();
      const formattedHeader = {
        // Base model fields
        id: purchaseOrderHeader.id || 0,
        uid: headerUID,
        ss: purchaseOrderHeader.ss || 1,
        createdBy: validUserUID,
        createdTime: currentTime,
        modifiedBy: validUserUID,
        modifiedTime: currentTime,
        serverAddTime: currentTime,
        serverModifiedTime: currentTime,
        keyUID: purchaseOrderHeader.keyUID || generateUID(),
        isSelected: false,

        // Purchase order header fields
        orgUID: purchaseOrderHeader.orgUID,
        divisionUID: purchaseOrderHeader.divisionUID || null,
        hasTemplate: purchaseOrderHeader.hasTemplate || false,
        purchaseOrderTemplateHeaderUID: purchaseOrderHeader.purchaseOrderTemplateHeaderUID || null,
        wareHouseUID: purchaseOrderHeader.wareHouseUID || purchaseOrderHeader.warehouse_uid,
        orderDate: purchaseOrderHeader.orderDate,
        orderNumber: purchaseOrderHeader.orderNumber || null,
        draftOrderNumber: purchaseOrderHeader.draftOrderNumber || null,
        dmsOrderNumber: purchaseOrderHeader.dmsOrderNumber || null,
        expectedDeliveryDate: purchaseOrderHeader.expectedDeliveryDate,
        shippingAddressUID: purchaseOrderHeader.shippingAddressUID || '',  // Empty string to avoid FK constraint
        billingAddressUID: purchaseOrderHeader.billingAddressUID || '',    // Empty string to avoid FK constraint
        status: purchaseOrderHeader.status || 'DRAFT',
        sapStatus: purchaseOrderHeader.sapStatus || 'Pending',
        qtyCount: purchaseOrderHeader.qtyCount || 0,
        lineCount: purchaseOrderHeader.lineCount || 0,
        totalAmount: purchaseOrderHeader.totalAmount || 0,
        totalDiscount: purchaseOrderHeader.totalDiscount || 0,
        lineDiscount: purchaseOrderHeader.lineDiscount || 0,
        headerDiscount: purchaseOrderHeader.headerDiscount || 0,
        totalTaxAmount: purchaseOrderHeader.totalTaxAmount || 0,
        lineTaxAmount: purchaseOrderHeader.lineTaxAmount || 0,
        headerTaxAmount: purchaseOrderHeader.headerTaxAmount || 0,
        netAmount: purchaseOrderHeader.netAmount || 0,
        availableCreditLimit: purchaseOrderHeader.availableCreditLimit || 0,
        taxData: purchaseOrderHeader.taxData || '{}',  // Must be valid JSON string, not null
        branchUID: purchaseOrderHeader.branchUID || null,
        hoOrgUID: purchaseOrderHeader.hoOrgUID || null,
        orgUnitUID: purchaseOrderHeader.orgUnitUID || null,
        reportingEmpUID: purchaseOrderHeader.reportingEmpUID || null,
        sourceWareHouseUID: purchaseOrderHeader.sourceWareHouseUID || null,
        reportingEmpName: purchaseOrderHeader.reportingEmpName || '',
        reportingEmpCode: purchaseOrderHeader.reportingEmpCode || '',
        createdByEmpCode: purchaseOrderHeader.createdByEmpCode || '',
        createdByEmpName: purchaseOrderHeader.createdByEmpName || ''
      };

      // Format lines with base model fields
      const formattedLines = (purchaseOrderLines || []).map((line, index) => ({
        // Base model fields
        id: line.id || 0,
        uid: line.uid || generateUID(),
        ss: line.ss || 1,
        createdBy: validUserUID,
        createdTime: currentTime,
        modifiedBy: validUserUID,
        modifiedTime: currentTime,
        serverAddTime: currentTime,
        serverModifiedTime: currentTime,
        keyUID: line.keyUID || generateUID(),
        isSelected: false,

        // Purchase order line fields
        purchaseOrderHeaderUID: headerUID,
        lineNumber: line.lineNumber || (index + 1),
        skuuid: line.skuuid,
        skuCode: line.skuCode || '',
        skuType: line.skuType || '',
        uom: line.uom || 'OU',
        baseUOM: line.baseUOM || line.uom || 'OU',
        uomConversionToBU: line.uomConversionToBU || 1,
        availableQty: line.availableQty || 0,
        modelQty: line.modelQty || 0,
        inTransitQty: line.inTransitQty || 0,
        suggestedQty: line.suggestedQty || 0,
        past3MonthAvg: line.past3MonthAvg || 0,
        requestedQty: line.requestedQty || 0,
        finalQty: line.finalQty || line.requestedQty || 0,
        finalQtyBU: line.finalQtyBU || line.finalQty || line.requestedQty || 0,
        unitPrice: line.unitPrice || 0,
        basePrice: line.basePrice || line.unitPrice || 0,
        totalAmount: line.totalAmount || 0,
        totalDiscount: line.totalDiscount || 0,
        lineDiscount: line.lineDiscount || 0,
        headerDiscount: line.headerDiscount || 0,
        totalTaxAmount: line.totalTaxAmount || 0,
        lineTaxAmount: line.lineTaxAmount || 0,
        headerTaxAmount: line.headerTaxAmount || 0,
        netAmount: line.netAmount || 0,
        taxData: line.taxData || '{}',  // Must be valid JSON string, not null
        standingSchemeData: line.standingSchemeData || '{}',  // Must be valid JSON string, not null
        mrp: line.mrp || 0,
        dpPrice: line.dpPrice || 0
      }));

      const requestBody = [{
        actionType: numericActionType,
        purchaseOrderHeader: formattedHeader,
        purchaseOrderLines: formattedLines,
        purchaseOrderLineProvisions: purchaseOrderLineProvisions || []
      }];

      // Log specific problematic fields
      console.log('🔍 TaxData in header:', requestBody[0].purchaseOrderHeader.taxData);
      console.log('🔍 TaxData type:', typeof requestBody[0].purchaseOrderHeader.taxData);
      if (requestBody[0].purchaseOrderLines.length > 0) {
        console.log('🔍 First line TaxData:', requestBody[0].purchaseOrderLines[0].taxData);
        console.log('🔍 First line TaxData type:', typeof requestBody[0].purchaseOrderLines[0].taxData);
      }
      console.log('📤 CUD Purchase Order Request:', JSON.stringify(requestBody, null, 2));

      const response = await apiRequest(
        '/PurchaseOrder/CUD_PurchaseOrder',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        return {
          success: true,
          data: response.data,
          message: this.getOperationMessage(actionType)
        };
      } else {
        throw new Error(response.error || `Failed to ${actionType.toLowerCase()} purchase order`);
      }
    } catch (error) {
      console.error(`Error ${actionType.toLowerCase()}ing purchase order:`, error);
      return {
        success: false,
        error: error.message || `Failed to ${actionType.toLowerCase()} purchase order`
      };
    }
  }

  /**
   * Create Purchase Order
   */
  async createPurchaseOrder(purchaseOrderHeader, purchaseOrderLines = [], purchaseOrderLineProvisions = []) {
    return await this.cudPurchaseOrder('CREATE', purchaseOrderHeader, purchaseOrderLines, purchaseOrderLineProvisions);
  }

  /**
   * Update Purchase Order
   */
  async updatePurchaseOrder(purchaseOrderHeader, purchaseOrderLines = [], purchaseOrderLineProvisions = []) {
    return await this.cudPurchaseOrder('UPDATE', purchaseOrderHeader, purchaseOrderLines, purchaseOrderLineProvisions);
  }

  /**
   * Delete Purchase Order
   */
  async deletePurchaseOrder(purchaseOrderUID) {
    const header = { uid: purchaseOrderUID };
    return await this.cudPurchaseOrder('DELETE', header, [], []);
  }

  /**
   * Save Purchase Order as Draft
   */
  async savePurchaseOrderAsDraft(purchaseOrderHeader, purchaseOrderLines = [], purchaseOrderLineProvisions = []) {
    const draftHeader = {
      ...purchaseOrderHeader,
      status: 'DRAFT'
    };
    return await this.createPurchaseOrder(draftHeader, purchaseOrderLines, purchaseOrderLineProvisions);
  }

  /**
   * Confirm Purchase Order
   */
  async confirmPurchaseOrder(purchaseOrderHeader, purchaseOrderLines = [], purchaseOrderLineProvisions = []) {
    const confirmedHeader = {
      ...purchaseOrderHeader,
      status: 'CONFIRMED'
    };
    const actionType = purchaseOrderHeader.uid ? 'UPDATE' : 'CREATE';
    return await this.cudPurchaseOrder(actionType, confirmedHeader, purchaseOrderLines, purchaseOrderLineProvisions);
  }

  /**
   * Get All Purchase Order Template Headers
   * Endpoint: POST /api/PurchaseOrderTemplateHeader/GetAllPurchaseOrderTemplateHeaders
   */
  async getAllPurchaseOrderTemplateHeaders(filters = {}) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      const requestBody = {
        pageNumber: filters.page || 1,
        pageSize: filters.pageSize || 10,
        filterCriterias: [],
        isCountRequired: true
      };

      if (filters.searchQuery) {
        requestBody.filterCriterias.push({
          name: "templateName",
          value: filters.searchQuery,
          type: 1,
          filterGroup: 0,
          filterMode: 0
        });
      }

      const response = await apiRequest(
        '/PurchaseOrderTemplateHeader/GetAllPurchaseOrderTemplateHeaders',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        let headers = response.data?.Data?.PagedData || response.data?.PagedData || response.data?.data || [];
        const totalCount = response.data?.Data?.TotalCount || response.data?.TotalCount || response.data?.totalCount || 0;

        // Debug: Log the first template to see what fields are available
        if (headers && headers.length > 0) {
          console.log('📋 Template Header Sample:', headers[0]);
          console.log('🏢 Warehouse fields available:', {
            warehouse: headers[0].warehouse,
            Warehouse: headers[0].Warehouse,
            warehouse_uid: headers[0].warehouse_uid,
            wareHouseUID: headers[0].wareHouseUID,
            WareHouseUID: headers[0].WareHouseUID,
            warehouseUID: headers[0].warehouseUID
          });
        }

        // Ensure warehouse fields are properly mapped for each template header
        if (Array.isArray(headers)) {
          headers = headers.map(header => {
            if (header) {
              // Map warehouse fields to multiple variations for compatibility
              const warehouseIdField = header.warehouse_uid || header.wareHouseUID || header.WareHouseUID || header.warehouseUID;
              const warehouseNameField = header.warehouse || header.Warehouse || header.warehouseName || header.WarehouseName;
              return {
                ...header,
                warehouse_uid: warehouseIdField,
                wareHouseUID: warehouseIdField,
                WareHouseUID: warehouseIdField,
                warehouse: warehouseNameField,
                Warehouse: warehouseNameField
              };
            }
            return header;
          });
        }

        return {
          success: true,
          data: response.data,
          headers: Array.isArray(headers) ? headers : [],
          totalCount: totalCount || 0
        };
      } else {
        throw new Error(response.error || 'Failed to fetch template headers');
      }
    } catch (error) {
      console.error('Error fetching template headers:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch template headers',
        headers: [],
        totalCount: 0
      };
    }
  }

  /**
   * Get Purchase Order Template Master by UID
   * Endpoint: GET /api/PurchaseOrderTemplateHeader/GetPurchaseOrderTemplateMasterByUID
   */
  async getPurchaseOrderTemplateMasterByUID(uid) {
    try {
      const token = this.getAuthToken();

      if (!uid) {
        throw new Error('Template UID is required');
      }

      const response = await apiRequest(
        `/PurchaseOrderTemplateHeader/GetPurchaseOrderTemplateMasterByUID?uid=${encodeURIComponent(uid)}`,
        {
          method: 'GET'
        },
        token
      );

      if (response.success) {
        const masterData = response.data?.Data || response.data;
        // Ensure warehouse_uid is properly mapped for template master
        if (masterData && (masterData.warehouse_uid || masterData.wareHouseUID || masterData.WareHouseUID)) {
          masterData.wareHouseUID = masterData.warehouse_uid || masterData.wareHouseUID || masterData.WareHouseUID;
          masterData.warehouse_uid = masterData.warehouse_uid || masterData.wareHouseUID || masterData.WareHouseUID;
        }
        return {
          success: true,
          data: masterData,
          master: masterData
        };
      } else {
        throw new Error(response.error || 'Failed to fetch template master');
      }
    } catch (error) {
      console.error('Error fetching template master:', error);
      return {
        success: false,
        error: error.message || 'Failed to fetch template master',
        master: null
      };
    }
  }

  /**
   * Create, Update, or Delete Purchase Order Template
   * Endpoint: POST /api/PurchaseOrderTemplateHeader/CUD_PurchaseOrderTemplate
   */
  async cudPurchaseOrderTemplate(actionType, purchaseOrderTemplateHeader, purchaseOrderTemplateLines = []) {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.error('❌ No auth token found. User needs to login.');
        throw new Error('Authentication required. Please login.');
      }

      // Map action types to enum values (Add=0, Update=1, Delete=2)
      const actionTypeMap = {
        'CREATE': 0,  // Add
        'UPDATE': 1,  // Update
        'DELETE': 2,  // Delete
        0: 0,
        1: 1,
        2: 2
      };

      const numericActionType = actionTypeMap[actionType] ?? actionTypeMap[actionType.toUpperCase()];

      if (numericActionType === undefined) {
        throw new Error('Valid action type (CREATE/0, UPDATE/1, DELETE/2) is required');
      }

      // Generate UID for new template (similar to backend Guid.NewGuid().ToString())
      const generateUID = () => {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
          var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
          return v.toString(16);
        });
      };

      const currentTime = new Date().toISOString();
      const userUID = localStorage.getItem('userUID') || 'system-user';
      const orgUID = localStorage.getItem('orgUID') || 'system-org';

      // Generate template header UID first
      const templateHeaderUID = purchaseOrderTemplateHeader.uid || generateUID();

      // Structure the request body to match IPurchaseOrderTemplateMaster
      const requestBody = {
        actionType: numericActionType,
        purchaseOrderTemplateHeader: {
          // IBaseModel fields - all required by database schema
          id: purchaseOrderTemplateHeader.id || 0,
          uid: templateHeaderUID,
          ss: purchaseOrderTemplateHeader.ss || 1,
          createdBy: userUID,
          createdTime: currentTime,
          modifiedBy: userUID,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime,
          keyUID: purchaseOrderTemplateHeader.keyUID || generateUID(),
          isSelected: false,

          // IPurchaseOrderTemplateHeader specific fields - match database column names exactly
          orgUID: purchaseOrderTemplateHeader.orgUID || orgUID,
          templateName: purchaseOrderTemplateHeader.templateName,
          storeUid: purchaseOrderTemplateHeader.storeUid || purchaseOrderTemplateHeader.wareHouseUID || purchaseOrderTemplateHeader.warehouse_uid, // Maps to store_uid column
          isCreatedByStore: purchaseOrderTemplateHeader.isCreatedByStore !== undefined ? purchaseOrderTemplateHeader.isCreatedByStore : true,
          isActive: purchaseOrderTemplateHeader.isActive !== undefined ? purchaseOrderTemplateHeader.isActive : true
        },
        purchaseOrderTemplateLines: (purchaseOrderTemplateLines || []).map((line, index) => ({
          // IBaseModel fields - all required by database schema
          id: line.id || 0,
          uid: line.uid || generateUID(), // Generate new UID for each line
          ss: line.ss || 1,
          createdBy: userUID,
          createdTime: currentTime,
          modifiedBy: userUID,
          modifiedTime: currentTime,
          serverAddTime: currentTime,
          serverModifiedTime: currentTime,
          keyUID: line.keyUID || generateUID(),
          isSelected: false,

          // IPurchaseOrderTemplateLine specific fields
          purchaseOrderTemplateHeaderUID: templateHeaderUID, // Link to parent template
          lineNumber: line.lineNumber || (index + 1),
          skuuid: line.skuuid || line.SKUUID,
          skuCode: line.skuCode || line.SKUCode,
          uom: line.uom || line.UOM || 'OU',
          qty: parseFloat(line.qty || line.requestedQty || 0),
          inputQty: parseInt(line.inputQty || line.requestedQty || 0)
        }))
      };

      console.log('📤 CUD Template Request:', requestBody);

      const response = await apiRequest(
        '/PurchaseOrderTemplateHeader/CUD_PurchaseOrderTemplate',
        {
          method: 'POST',
          body: JSON.stringify(requestBody)
        },
        token
      );

      if (response.success) {
        return {
          success: true,
          data: response.data,
          message: this.getTemplateOperationMessage(actionType)
        };
      } else {
        throw new Error(response.error || `Failed to ${actionType.toLowerCase()} template`);
      }
    } catch (error) {
      console.error(`Error ${actionType.toLowerCase()}ing template:`, error);
      return {
        success: false,
        error: error.message || `Failed to ${actionType.toLowerCase()} template`
      };
    }
  }

  /**
   * Create Purchase Order Template
   */
  async createPurchaseOrderTemplate(templateHeader, templateLines = []) {
    return await this.cudPurchaseOrderTemplate('CREATE', templateHeader, templateLines);
  }

  /**
   * Update Purchase Order Template
   */
  async updatePurchaseOrderTemplate(templateHeader, templateLines = []) {
    return await this.cudPurchaseOrderTemplate('UPDATE', templateHeader, templateLines);
  }

  /**
   * Delete Purchase Order Template
   */
  async deletePurchaseOrderTemplate(templateUID) {
    const header = { uid: templateUID };
    return await this.cudPurchaseOrderTemplate('DELETE', header, []);
  }

  /**
   * Get Templates (legacy method for backward compatibility)
   */
  async getPurchaseOrderTemplates(filters = {}) {
    return await this.getAllPurchaseOrderTemplateHeaders(filters);
  }

  /**
   * Get Purchase Orders by Status
   */
  async getPurchaseOrdersByStatus(status, filters = {}) {
    const statusFilters = {
      ...filters,
      status: status
    };
    return await this.getPurchaseOrderHeaders(statusFilters);
  }

  /**
   * Search Purchase Orders
   */
  async searchPurchaseOrders(searchQuery, filters = {}) {
    const searchFilters = {
      ...filters,
      searchQuery: searchQuery
    };
    return await this.getPurchaseOrderHeaders(searchFilters);
  }

  /**
   * Helper method to get operation success message
   */
  getOperationMessage(operationType) {
    switch (operationType.toUpperCase()) {
      case 'CREATE':
        return 'Purchase order created successfully';
      case 'UPDATE':
        return 'Purchase order updated successfully';
      case 'DELETE':
        return 'Purchase order deleted successfully';
      default:
        return 'Operation completed successfully';
    }
  }

  /**
   * Helper method to get template operation success message
   */
  getTemplateOperationMessage(operationType) {
    switch (operationType.toUpperCase()) {
      case 'CREATE':
        return 'Template created successfully';
      case 'UPDATE':
        return 'Template updated successfully';
      case 'DELETE':
        return 'Template deleted successfully';
      default:
        return 'Template operation completed successfully';
    }
  }

  /**
   * Format purchase order data for API
   */
  formatPurchaseOrderData(orderData, products = []) {
    const currentDate = new Date().toISOString();

    // Calculate totals
    const totalQuantity = products.reduce((sum, product) => sum + (product.orderQty || 0), 0);
    const totalAmount = products.reduce((sum, product) => sum + (product.value || 0), 0);

    // Format header
    const purchaseOrderHeader = {
      orgUID: orderData.orgUID || localStorage.getItem('orgUID') || '',
      divisionUID: orderData.divisionUID || localStorage.getItem('divisionUID') || '',
      hasTemplate: orderData.hasTemplate || false,
      purchaseOrderTemplateHeaderUID: orderData.templateUID || '',
      wareHouseUID: orderData.warehouse_uid || orderData.wareHouseUID || orderData.plant || 'N002',
      warehouse_uid: orderData.warehouse_uid || orderData.wareHouseUID || orderData.plant || 'N002',
      orderDate: orderData.orderDate ? new Date(orderData.orderDate).toISOString() : currentDate,
      orderNumber: orderData.orderNumber || '',
      draftOrderNumber: orderData.draftOrderNumber || '',
      expectedDeliveryDate: orderData.requestedDeliveryDate ? new Date(orderData.requestedDeliveryDate).toISOString() : currentDate,
      shippingAddressUID: orderData.shippingAddressUID || '',
      billingAddressUID: orderData.billingAddressUID || '',
      status: orderData.status || 'DRAFT',
      qtyCount: totalQuantity,
      lineCount: products.length,
      totalAmount: totalAmount,
      totalDiscount: orderData.totalDiscount || 0,
      lineDiscount: orderData.lineDiscount || 0,
      headerDiscount: orderData.headerDiscount || 0,
      totalTaxAmount: orderData.totalTaxAmount || 0,
      lineTaxAmount: orderData.lineTaxAmount || 0,
      headerTaxAmount: orderData.headerTaxAmount || 0,
      netAmount: totalAmount,
      availableCreditLimit: orderData.availableCreditLimit || 0,
      taxData: orderData.taxData || '',
      branchUID: orderData.branchUID || '',
      hoOrgUID: orderData.hoOrgUID || '',
      orgUnitUID: orderData.orgUnitUID || '',
      reportingEmpUID: orderData.reportingEmpUID || '',
      sourceWareHouseUID: orderData.sourceWareHouseUID || '',
      reportingEmpName: orderData.requestedBy || '',
      reportingEmpCode: orderData.requestedByCode || '',
      createdByEmpCode: orderData.preparedByCode || '',
      createdByEmpName: orderData.preparedBy || '',
      createdBy: orderData.createdBy || localStorage.getItem('userUID') || '',
      createdTime: currentDate,
      modifiedBy: orderData.modifiedBy || localStorage.getItem('userUID') || '',
      modifiedTime: currentDate
    };

    // Format lines
    const purchaseOrderLines = products.map((product, index) => ({
      purchaseOrderHeaderUID: orderData.uid || '',
      lineNumber: index + 1,
      skuuid: product.skuuid || product.uid || '',
      skuCode: product.skuCode || '',
      skuType: product.skuType || '',
      uom: product.uom || 'OU',
      baseUOM: product.baseUOM || product.uom || 'OU',
      uomConversionToBU: product.uomConversionToBU || 1,
      availableQty: product.availQty || 0,
      modelQty: product.modelQty || 0,
      inTransitQty: product.inTransit || 0,
      suggestedQty: product.suggestedOrderQty || 0,
      past3MonthAvg: product.past3MonthAverage || 0,
      requestedQty: product.orderQty || 0,
      finalQty: product.orderQty || 0,
      finalQtyBU: product.orderQty || 0,
      unitPrice: product.unitPrice || 0,
      basePrice: product.basePrice || product.unitPrice || 0,
      totalAmount: product.value || (product.orderQty * product.unitPrice) || 0,
      totalDiscount: product.totalDiscount || 0,
      lineDiscount: product.lineDiscount || 0,
      headerDiscount: product.headerDiscount || 0,
      totalTaxAmount: product.totalTaxAmount || 0,
      lineTaxAmount: product.lineTaxAmount || 0,
      headerTaxAmount: product.headerTaxAmount || 0,
      netAmount: product.value || (product.orderQty * product.unitPrice) || 0,
      taxData: product.taxData || '',
      mrp: product.mrp || 0,
      dpPrice: product.dpPrice || 0,
      createdBy: localStorage.getItem('userUID') || '',
      createdTime: currentDate,
      modifiedBy: localStorage.getItem('userUID') || '',
      modifiedTime: currentDate
    }));

    return {
      purchaseOrderHeader,
      purchaseOrderLines,
      purchaseOrderLineProvisions: []
    };
  }

  /**
   * Validate purchase order data
   */
  validatePurchaseOrderData(orderData, products = []) {
    const errors = [];

    if (!orderData.plant || orderData.plant === 'Select') {
      errors.push('Plant is required');
    }

    if (!orderData.requestedBy) {
      errors.push('Requested by is required');
    }

    if (!orderData.requestedDeliveryDate) {
      errors.push('Requested delivery date is required');
    }

    if (!products || products.length === 0) {
      errors.push('At least one product is required');
    }

    const validProducts = products.filter(product =>
      product.orderQty && product.orderQty > 0
    );

    if (validProducts.length === 0) {
      errors.push('At least one product must have a valid order quantity');
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  /**
   * Get employee dropdown options for Requested By field
   */
  async getEmployeeDropdownOptions() {
    try {
      const token = this.getAuthToken();

      if (!token) {
        console.warn('⚠️ No auth token found for employee dropdown');
        return this.getFallbackEmployeeOptions();
      }

      const response = await apiRequest('/MaintainUser/SelectAllMaintainUserDetails', {
        method: 'POST',
        body: JSON.stringify({
          pageNumber: 1,
          pageSize: 100,
          isCountRequired: false,
          sortCriterias: [],
          filterCriterias: []
        })
      });

      if (response.success && response.data) {
        const employees = response.data.PagedData || response.data.Data?.PagedData || response.data.Data || [];

        const options = employees.map(emp => ({
          value: emp.UID || emp.uid,
          label: `[${emp.Code || emp.code}] ${emp.Name || emp.name}`,
          code: emp.Code || emp.code,
          name: emp.Name || emp.name
        }));

        return {
          success: true,
          employees: options
        };
      }

      return this.getFallbackEmployeeOptions();
    } catch (error) {
      console.error('Error fetching employees:', error);
      return this.getFallbackEmployeeOptions();
    }
  }

  /**
   * Get fallback employee options when API fails
   */
  getFallbackEmployeeOptions() {
    return {
      success: true,
      employees: [
        { value: 'emp001', label: '[13010435] G & M Paterson', code: '13010435', name: 'G & M Paterson' },
        { value: 'emp002', label: '[EMP001] Admin User', code: 'EMP001', name: 'Admin User' },
        { value: 'emp003', label: '[EMP002] Manager', code: 'EMP002', name: 'Manager' }
      ]
    };
  }
}

export default new PurchaseOrderService();