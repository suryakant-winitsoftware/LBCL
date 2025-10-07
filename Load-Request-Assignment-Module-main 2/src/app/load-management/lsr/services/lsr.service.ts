import { api } from '@/services/api';

export interface LSRRequest {
  UID?: string;
  id?: string;
  route?: string;
  requestDate?: string;
  status?: string;
  priority?: string;
  estimatedLoad?: string;
  approvalDate?: string;
  approvedLoad?: string;
  assignedTruck?: string;
  assignedDriver?: string;
  createdBy?: string;
  createdOn?: string;
  modifiedBy?: string;
  modifiedOn?: string;
  isActive?: boolean;
}

export interface LSRStats {
  totalRequests: number;
  pendingRequests: number;
  approvedRequests: number;
  rejectedRequests: number;
}

class LSRService {
  /**
   * Get all load service requests with pagination
   */
  async getAllRequests(body: any = {}, stockType?: string) {
    try {
      // Default pagination if not provided
      const requestBody = {
        PageNumber: 0,
        PageSize: 100,
        IsCountRequired: true,
        SortCriterias: [],
        FilterCriterias: [],
        ...body
      };
      const response = await api.loadRequest.getAll(requestBody, stockType);
      return response;
    } catch (error) {
      console.error('Error fetching all LSR requests:', error);
      throw error;
    }
  }

  /**
   * Get load service request by UID
   */
  async getRequestByUID(uid: string) {
    try {
      const response = await api.loadRequest.getByUID(uid);
      return response;
    } catch (error) {
      console.error('Error fetching LSR request by UID:', error);
      throw error;
    }
  }

  /**
   * Get statistics for LSR dashboard
   * Since there's no stats endpoint, we calculate from all requests
   */
  async getStats(orgUID?: string): Promise<LSRStats> {
    try {
      // Fetch all requests
      const response: any = await this.getAllRequests({
        PageNumber: 0,
        PageSize: 1000,
        IsCountRequired: true
      });

      // Extract data from response
      const requests = response?.Data?.Items || response?.Items || [];
      const totalCount = response?.Data?.TotalCount || response?.TotalCount || requests.length;

      // Calculate stats from the data
      const stats: LSRStats = {
        totalRequests: totalCount,
        pendingRequests: requests.filter((r: any) =>
          r.Status === 'Pending' || r.Status === 'Awaiting Approval' || r.Status === 'Under Review'
        ).length,
        approvedRequests: requests.filter((r: any) =>
          r.Status === 'Approved' || r.Status === 'Load Sheet Generated' || r.Status === 'In Transit'
        ).length,
        rejectedRequests: requests.filter((r: any) =>
          r.Status === 'Rejected' || r.Status === 'Cancelled'
        ).length,
      };

      return stats;
    } catch (error) {
      console.error('Error fetching LSR stats:', error);
      // Return default stats on error
      return {
        totalRequests: 0,
        pendingRequests: 0,
        approvedRequests: 0,
        rejectedRequests: 0,
      };
    }
  }

  /**
   * Get pending requests
   */
  async getPendingRequests(body: any = {}) {
    try {
      const requestBody = {
        PageNumber: 0,
        PageSize: 100,
        IsCountRequired: true,
        SortCriterias: [],
        FilterCriterias: [
          {
            FieldName: 'Status',
            Operator: 'eq',
            Value: 'Pending'
          }
        ],
        ...body
      };
      const response = await this.getAllRequests(requestBody);
      return response;
    } catch (error) {
      console.error('Error fetching pending LSR requests:', error);
      throw error;
    }
  }

  /**
   * Get approved requests
   */
  async getApprovedRequests(body: any = {}) {
    try {
      const requestBody = {
        PageNumber: 0,
        PageSize: 100,
        IsCountRequired: true,
        SortCriterias: [],
        FilterCriterias: [
          {
            FieldName: 'Status',
            Operator: 'eq',
            Value: 'Approved'
          }
        ],
        ...body
      };
      const response = await this.getAllRequests(requestBody);
      return response;
    } catch (error) {
      console.error('Error fetching approved LSR requests:', error);
      throw error;
    }
  }

  /**
   * Create new load service request
   */
  async createRequest(body: any) {
    try {
      const response = await api.loadRequest.create(body);
      return response;
    } catch (error) {
      console.error('Error creating LSR request:', error);
      throw error;
    }
  }

  /**
   * Update load service request
   */
  async updateRequest(body: any) {
    try {
      const response = await api.loadRequest.update(body);
      return response;
    } catch (error) {
      console.error('Error updating LSR request:', error);
      throw error;
    }
  }

  /**
   * Delete load service request
   */
  async deleteRequest(body: any) {
    try {
      const response = await api.loadRequest.delete(body);
      return response;
    } catch (error) {
      console.error('Error deleting LSR request:', error);
      throw error;
    }
  }

  /**
   * Update request lines
   */
  async updateRequestLines(body: any) {
    try {
      const response = await api.loadRequest.updateLines(body);
      return response;
    } catch (error) {
      console.error('Error updating LSR request lines:', error);
      throw error;
    }
  }

  /**
   * Create request from queue
   */
  async createRequestFromQueue(body: any) {
    try {
      const response = await api.loadRequest.createFromQueue(body);
      return response;
    } catch (error) {
      console.error('Error creating LSR request from queue:', error);
      throw error;
    }
  }

  /**
   * Approve load service request
   * Updates status to 'Approved' and sets approved quantities
   */
  async approveRequest(
    requestUID: string,
    approvedLines: any[],
    remarks?: string,
    approverName?: string
  ) {
    try {
      const approvalRemarks = remarks || `Approved by ${approverName || 'Logistics Manager'}`;

      const payload = {
        WHStockRequest: {
          UID: requestUID,
          Status: 'Approved',
          Remarks: approvalRemarks,
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          SS: 2  // Sync status: Modified
        },
        WHStockRequestLines: approvedLines.map(line => ({
          UID: line.UID,
          WHStockRequestUID: requestUID,
          ApprovedQty: line.ApprovedQty || line.CPEApprovedQty || line.RequestedQty,
          ApprovedQty1: line.ApprovedQty1 || line.CPEApprovedQty1 || line.RequestedQty1,
          ApprovedQty2: line.ApprovedQty2 || line.CPEApprovedQty2 || line.RequestedQty2,
          SS: 2
        }))
      };

      const response = await api.loadRequest.update(payload);
      return response;
    } catch (error) {
      console.error('Error approving LSR request:', error);
      throw error;
    }
  }

  /**
   * Reject load service request
   * Updates status to 'Rejected' with reason
   */
  async rejectRequest(
    requestUID: string,
    reason: string,
    rejectorName?: string
  ) {
    try {
      const rejectionRemarks = `Rejected by ${rejectorName || 'Logistics Manager'}: ${reason}`;

      const payload = {
        WHStockRequest: {
          UID: requestUID,
          Status: 'Rejected',
          Remarks: rejectionRemarks,
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          SS: 2
        }
      };

      const response = await api.loadRequest.update(payload);
      return response;
    } catch (error) {
      console.error('Error rejecting LSR request:', error);
      throw error;
    }
  }

  /**
   * Update request status
   * Generic method to update status (e.g., Under Review, Load Sheet Generated, etc.)
   */
  async updateRequestStatus(
    requestUID: string,
    status: string,
    remarks?: string
  ) {
    try {
      const payload = {
        WHStockRequest: {
          UID: requestUID,
          Status: status,
          Remarks: remarks || `Status updated to ${status}`,
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          SS: 2
        }
      };

      const response = await api.loadRequest.update(payload);
      return response;
    } catch (error) {
      console.error('Error updating LSR request status:', error);
      throw error;
    }
  }

  /**
   * CPE Approve quantities (intermediate approval step)
   */
  async cpeApproveQuantities(
    requestUID: string,
    approvedLines: any[],
    remarks?: string
  ) {
    try {
      const payload = {
        WHStockRequest: {
          UID: requestUID,
          Status: 'CPE Approved',
          Remarks: remarks || 'Approved by CPE',
          ModifiedTime: new Date().toISOString(),
          ServerModifiedTime: new Date().toISOString(),
          SS: 2
        },
        WHStockRequestLines: approvedLines.map(line => ({
          UID: line.UID,
          WHStockRequestUID: requestUID,
          CPEApprovedQty: line.CPEApprovedQty || line.RequestedQty,
          CPEApprovedQty1: line.CPEApprovedQty1 || line.RequestedQty1,
          CPEApprovedQty2: line.CPEApprovedQty2 || line.RequestedQty2,
          SS: 2
        }))
      };

      const response = await api.loadRequest.update(payload);
      return response;
    } catch (error) {
      console.error('Error CPE approving LSR request:', error);
      throw error;
    }
  }
}

export const lsrService = new LSRService();
