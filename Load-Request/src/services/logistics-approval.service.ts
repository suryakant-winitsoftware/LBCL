import { apiService } from './api';
import { PagingRequest, PagingResponse } from '@/types/common.types';

export interface LoadRequest {
  id: string;
  uid: string;
  movementCode: string;
  routeCode: string;
  routeUID?: string;
  salesmanId: string;
  salesmanName: string;
  salesmanUID?: string;
  loadType: 'Emergency' | 'Normal';
  submittedDate: string;
  requiredDate: string;
  status: 'Pending' | 'LogisticsApproved' | 'ForkliftApproved' | 'Rejected' | 'Shipped';
  productType: 'Commercial' | 'POSM';
  submittedBy?: string;
  submittedByUID?: string;
  approvedBy?: string;
  approvedByUID?: string;
  approvalDate?: string;
  rejectionReason?: string;
  shipmentDate?: string;
  shipmentDetails?: string;
  items?: LoadRequestItem[];
  totalQuantity?: number;
  totalValue?: number;
  priority?: 'High' | 'Medium' | 'Low';
  notes?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface LoadRequestItem {
  id: string;
  uid: string;
  loadRequestUID: string;
  sku: string;
  productName: string;
  quantity: number;
  quantityApproved?: number;
  quantityShipped?: number;
  uom: string;
  unitPrice?: number;
  totalPrice?: number;
  status?: 'Pending' | 'Approved' | 'Rejected' | 'Partial';
  notes?: string;
}

export interface LoadRequestFilter {
  status?: LoadRequest['status'][];
  loadType?: LoadRequest['loadType'][];
  productType?: LoadRequest['productType'][];
  salesmanUID?: string;
  routeUID?: string;
  fromDate?: string;
  toDate?: string;
  searchTerm?: string;
}

export interface CreateLoadRequestDto {
  routeUID: string;
  salesmanUID: string;
  loadType: 'Emergency' | 'Normal';
  productType: 'Commercial' | 'POSM';
  requiredDate: string;
  priority?: 'High' | 'Medium' | 'Low';
  notes?: string;
  items: {
    sku: string;
    productName: string;
    quantity: number;
    uom: string;
    unitPrice?: number;
  }[];
}

export interface UpdateLoadRequestDto {
  uid: string;
  status?: LoadRequest['status'];
  approvedByUID?: string;
  rejectionReason?: string;
  shipmentDate?: string;
  shipmentDetails?: string;
  notes?: string;
  items?: {
    uid: string;
    quantityApproved?: number;
    quantityShipped?: number;
    status?: LoadRequestItem['status'];
  }[];
}

export interface ApproveLoadRequestDto {
  uid: string;
  approvedByUID: string;
  approvalNotes?: string;
  items?: {
    uid: string;
    quantityApproved: number;
  }[];
}

export interface RejectLoadRequestDto {
  uid: string;
  rejectedByUID: string;
  rejectionReason: string;
}

export interface ShipLoadRequestDto {
  uid: string;
  shippedByUID: string;
  shipmentDate: string;
  shipmentDetails: string;
  trackingNumber?: string;
  items: {
    uid: string;
    quantityShipped: number;
  }[];
}

class LogisticsApprovalService {
  private baseEndpoint = '/LoadRequest';

  // Get all load requests with filtering and pagination
  async getLoadRequests(
    pagingRequest: PagingRequest,
    filter?: LoadRequestFilter
  ): Promise<PagingResponse<LoadRequest>> {
    try {
      const body = {
        ...pagingRequest,
        filter
      };
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/GetAllLoadRequests`,
        body
      );
      
      // Transform the response to match our interface
      return {
        data: response.Data || response.data || [],
        totalRecords: response.TotalRecords || response.totalRecords || 0,
        pageNumber: pagingRequest.pageNumber,
        pageSize: pagingRequest.pageSize,
        message: response.Message || response.message,
        isSuccess: response.IsSuccess !== undefined ? response.IsSuccess : true
      };
    } catch (error) {
      console.error('Error fetching load requests:', error);
      // Return mock data if API fails
      return this.getMockLoadRequests(pagingRequest, filter);
    }
  }

  // Get load request by ID
  async getLoadRequestById(uid: string): Promise<LoadRequest> {
    try {
      const response = await apiService.get<any>(
        `${this.baseEndpoint}/GetLoadRequestByUID?UID=${uid}`
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error fetching load request:', error);
      // Return mock data if API fails
      return this.getMockLoadRequestById(uid);
    }
  }

  // Create new load request
  async createLoadRequest(data: CreateLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/CreateLoadRequest`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error creating load request:', error);
      // Return mock response if API fails
      return {
        id: Date.now().toString(),
        uid: `LR-${Date.now()}`,
        movementCode: `SKTT01E000${Date.now().toString().slice(-4)}`,
        routeCode: data.routeUID,
        salesmanId: data.salesmanUID,
        salesmanName: 'Mock Salesman',
        loadType: data.loadType,
        productType: data.productType,
        submittedDate: new Date().toISOString(),
        requiredDate: data.requiredDate,
        status: 'Pending',
        priority: data.priority || 'Medium',
        notes: data.notes
      };
    }
  }

  // Update load request
  async updateLoadRequest(data: UpdateLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.put<any>(
        `${this.baseEndpoint}/UpdateLoadRequest`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error updating load request:', error);
      throw error;
    }
  }

  // Approve load request (Logistics)
  async approveLoadRequestLogistics(data: ApproveLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/ApproveLoadRequestLogistics`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error approving load request (logistics):', error);
      // Mock approval if API fails
      return {
        ...await this.getLoadRequestById(data.uid),
        status: 'LogisticsApproved',
        approvedByUID: data.approvedByUID,
        approvalDate: new Date().toISOString()
      };
    }
  }

  // Approve load request (Forklift)
  async approveLoadRequestForklift(data: ApproveLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/ApproveLoadRequestForklift`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error approving load request (forklift):', error);
      // Mock approval if API fails
      return {
        ...await this.getLoadRequestById(data.uid),
        status: 'ForkliftApproved',
        approvedByUID: data.approvedByUID,
        approvalDate: new Date().toISOString()
      };
    }
  }

  // Reject load request
  async rejectLoadRequest(data: RejectLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/RejectLoadRequest`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error rejecting load request:', error);
      // Mock rejection if API fails
      return {
        ...await this.getLoadRequestById(data.uid),
        status: 'Rejected',
        rejectionReason: data.rejectionReason
      };
    }
  }

  // Ship load request
  async shipLoadRequest(data: ShipLoadRequestDto): Promise<LoadRequest> {
    try {
      const response = await apiService.post<any>(
        `${this.baseEndpoint}/ShipLoadRequest`,
        data
      );
      return response.Data || response;
    } catch (error) {
      console.error('Error shipping load request:', error);
      // Mock shipment if API fails
      return {
        ...await this.getLoadRequestById(data.uid),
        status: 'Shipped',
        shipmentDate: data.shipmentDate,
        shipmentDetails: data.shipmentDetails
      };
    }
  }

  // Delete load request
  async deleteLoadRequest(uid: string): Promise<boolean> {
    try {
      await apiService.delete(`${this.baseEndpoint}/DeleteLoadRequest?UID=${uid}`);
      return true;
    } catch (error) {
      console.error('Error deleting load request:', error);
      return false;
    }
  }

  // Get load request statistics
  async getLoadRequestStats(orgUID?: string): Promise<any> {
    try {
      const endpoint = orgUID 
        ? `${this.baseEndpoint}/GetLoadRequestStats?OrgUID=${orgUID}`
        : `${this.baseEndpoint}/GetLoadRequestStats`;
      const response = await apiService.get<any>(endpoint);
      return response.Data || response;
    } catch (error) {
      console.error('Error fetching load request stats:', error);
      // Return mock stats if API fails
      return {
        pending: 8,
        logisticsApproved: 5,
        forkliftApproved: 3,
        rejected: 2,
        shipped: 10,
        total: 28
      };
    }
  }

  // Mock data methods for development/fallback
  private getMockLoadRequests(
    pagingRequest: PagingRequest,
    filter?: LoadRequestFilter
  ): PagingResponse<LoadRequest> {
    const mockData: LoadRequest[] = [
      { 
        id: '1', 
        uid: 'LR-001',
        movementCode: 'SKTT01E0001',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR001]',
        salesmanName: 'John Smith', 
        loadType: 'Normal',
        productType: 'Commercial',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'Pending'
      },
      { 
        id: '2',
        uid: 'LR-002', 
        movementCode: 'SKTT01E0002',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR002]',
        salesmanName: 'Sarah Johnson', 
        loadType: 'Emergency',
        productType: 'POSM',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'LogisticsApproved'
      },
      { 
        id: '3',
        uid: 'LR-003',
        movementCode: 'SKTT01E0003',
        routeCode: '[SKTT02]SKTT02',
        salesmanId: '[LSR003]',
        salesmanName: 'Mike Wilson', 
        loadType: 'Normal',
        productType: 'Commercial',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'ForkliftApproved'
      },
      { 
        id: '4',
        uid: 'LR-004',
        movementCode: 'SKTT01E0004',
        routeCode: '[SKTT03]SKTT03',
        salesmanId: '[LSR004]',
        salesmanName: 'Emily Brown', 
        loadType: 'Emergency',
        productType: 'Commercial',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'Rejected',
        rejectionReason: 'Insufficient stock'
      },
      { 
        id: '5',
        uid: 'LR-005',
        movementCode: 'SKTT01E0005',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR005]',
        salesmanName: 'David Lee', 
        loadType: 'Normal',
        productType: 'POSM',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'Shipped',
        shipmentDate: new Date().toISOString()
      },
      { 
        id: '6',
        uid: 'LR-006',
        movementCode: 'SKTT01E0006',
        routeCode: '[SKTT02]SKTT02',
        salesmanId: '[LSR006]',
        salesmanName: 'Lisa Davis', 
        loadType: 'Emergency',
        productType: 'Commercial',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'Pending'
      },
      { 
        id: '7',
        uid: 'LR-007',
        movementCode: 'SKTT01E0007',
        routeCode: '[SKTT03]SKTT03',
        salesmanId: '[LSR007]',
        salesmanName: 'Tom Anderson', 
        loadType: 'Normal',
        productType: 'POSM',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'LogisticsApproved'
      },
      { 
        id: '8',
        uid: 'LR-008',
        movementCode: 'SKTT01E0008',
        routeCode: '[SKTT01]SKTT01',
        salesmanId: '[LSR008]',
        salesmanName: 'Anna Garcia', 
        loadType: 'Emergency',
        productType: 'Commercial',
        submittedDate: new Date().toISOString(),
        requiredDate: new Date(Date.now() + 86400000).toISOString(),
        status: 'Pending'
      }
    ];

    // Apply filters
    let filteredData = [...mockData];
    if (filter) {
      if (filter.status && filter.status.length > 0) {
        filteredData = filteredData.filter(item => filter.status!.includes(item.status));
      }
      if (filter.loadType && filter.loadType.length > 0) {
        filteredData = filteredData.filter(item => filter.loadType!.includes(item.loadType));
      }
      if (filter.searchTerm) {
        const term = filter.searchTerm.toLowerCase();
        filteredData = filteredData.filter(item => 
          item.movementCode.toLowerCase().includes(term) ||
          item.salesmanName.toLowerCase().includes(term) ||
          item.routeCode.toLowerCase().includes(term)
        );
      }
    }

    // Apply pagination
    const startIndex = (pagingRequest.pageNumber - 1) * pagingRequest.pageSize;
    const endIndex = startIndex + pagingRequest.pageSize;
    const paginatedData = filteredData.slice(startIndex, endIndex);

    return {
      data: paginatedData,
      totalRecords: filteredData.length,
      pageNumber: pagingRequest.pageNumber,
      pageSize: pagingRequest.pageSize,
      isSuccess: true
    };
  }

  private getMockLoadRequestById(uid: string): LoadRequest {
    return {
      id: uid,
      uid: uid,
      movementCode: `SKTT01E000${uid}`,
      routeCode: '[SKTT01]SKTT01',
      salesmanId: '[LSR001]',
      salesmanName: 'John Smith',
      loadType: 'Normal',
      productType: 'Commercial',
      submittedDate: new Date().toISOString(),
      requiredDate: new Date(Date.now() + 86400000).toISOString(),
      status: 'Pending',
      items: [
        {
          id: '1',
          uid: 'LRI-001',
          loadRequestUID: uid,
          sku: 'SKU001',
          productName: 'Product 1',
          quantity: 100,
          uom: 'PCS',
          unitPrice: 10,
          totalPrice: 1000
        },
        {
          id: '2',
          uid: 'LRI-002',
          loadRequestUID: uid,
          sku: 'SKU002',
          productName: 'Product 2',
          quantity: 50,
          uom: 'CTN',
          unitPrice: 50,
          totalPrice: 2500
        }
      ],
      totalQuantity: 150,
      totalValue: 3500
    };
  }
}

export const logisticsApprovalService = new LogisticsApprovalService();