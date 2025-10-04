import { apiService } from './api';
import {
  ApprovalRequest,
  ApprovalHierarchy,
  ApprovalLog,
  AllApprovalRequest,
  ViewChangeRequestApproval,
  ApprovalActionRequest,
  ApprovalPagingRequest,
  PagedApprovalResponse,
  ApprovalRuleMapping,
  ApprovalStatistics,
  ApprovalDetail
} from '@/types/approval.types';

/**
 * Approval Service
 * Handles all approval-related API calls
 */
export const approvalService = {
  /**
   * Get approval log/history for a specific request
   * @param requestId - The approval request ID
   * @returns List of approval logs
   */
  getApprovalLog: async (requestId: string): Promise<ApprovalLog[]> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetApprovalLog?requestId=${requestId}`);
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching approval log:', error);
      throw error;
    }
  },

  /**
   * Get approval hierarchy for a rule
   * @param ruleId - The rule ID
   * @returns Approval hierarchy chain
   */
  getApprovalHierarchy: async (ruleId: string): Promise<ApprovalHierarchy[]> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetApprovalHierarchy?ruleId=${ruleId}`);
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching approval hierarchy:', error);
      throw error;
    }
  },

  /**
   * Get all change requests (pending approvals)
   * @returns List of change requests
   */
  getAllChangeRequests: async (): Promise<ViewChangeRequestApproval[]> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetAllChangeRequest`);
      console.log('GetAllChangeRequest response:', response);

      let rawData: any[] = [];

      // Handle different response formats
      if (Array.isArray(response)) {
        rawData = response;
      } else if (response.Data && Array.isArray(response.Data)) {
        rawData = response.Data;
      } else if (response.data && Array.isArray(response.data)) {
        rawData = response.data;
      } else {
        console.warn('Unexpected response format:', response);
        return [];
      }

      // Map API response (PascalCase) to frontend format (camelCase)
      const mappedData = rawData.map(item => {
        // Parse ChangedRecord JSON to extract description
        let itemDescription = '';
        try {
          if (item.ChangedRecord) {
            const changedRecord = typeof item.ChangedRecord === 'string'
              ? JSON.parse(item.ChangedRecord)
              : item.ChangedRecord;

            // Extract meaningful description from ChangeRecords array
            if (changedRecord.ChangeRecords && Array.isArray(changedRecord.ChangeRecords)) {
              const changes = changedRecord.ChangeRecords
                .map((change: any) => `${change.FieldName}: ${change.OldValue} → ${change.NewValue}`)
                .join(', ');
              itemDescription = changes || changedRecord.Action || 'No description';
            } else {
              itemDescription = changedRecord.Action || changedRecord.reason || 'Change request';
            }
          }
        } catch (e) {
          console.warn('Failed to parse ChangedRecord:', e);
          itemDescription = 'Change request';
        }

        return {
          uid: item.UID || item.uid,
          linkedItemType: item.LinkedItemType || item.linkedItemType,
          linkedItemUID: item.LinkedItemUid || item.linkedItemUID,
          requestID: item.UID || item.uid || item.requestID, // Use UID as requestID
          status: item.Status || item.status,
          requesterName: item.RequestedBy || item.requesterName || 'Unknown',
          requesterId: item.EmpUid || item.requesterId,
          createdOn: item.RequestDate || item.createdOn || item.request_date,
          modifiedOn: item.ApprovedDate || item.modifiedOn || item.approved_date,
          itemDescription: itemDescription || item.itemDescription,
          currentApproverId: item.EmpUid || item.currentApproverId,
          currentApproverName: item.RequestedBy || item.currentApproverName,
          // Pass through all other fields including raw ChangedRecord
          ...item
        };
      });

      console.log('Mapped approvals:', mappedData, 'Count:', mappedData.length);
      return mappedData;
    } catch (error) {
      console.error('Error fetching change requests:', error);
      throw error;
    }
  },

  /**
   * Get paginated change request data with filtering and sorting
   * @param pagingRequest - Paging, sorting, and filtering parameters
   * @returns Paginated response with approval requests
   */
  getChangeRequestData: async (
    pagingRequest: ApprovalPagingRequest
  ): Promise<PagedApprovalResponse<ViewChangeRequestApproval>> => {
    try {
      const response = await apiService.post<any>(
        `/ApprovalEngine/GetChangeRequestData`,
        pagingRequest
      );
      return response.Data || response.data || { data: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 };
    } catch (error) {
      console.error('Error fetching paginated change requests:', error);
      throw error;
    }
  },

  /**
   * Get specific change request by UID
   * @param requestUid - The unique identifier of the request
   * @returns Change request details
   */
  getChangeRequestByUid: async (requestUid: string): Promise<ViewChangeRequestApproval> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetChangeRequestDataByUid?requestUid=${requestUid}`);
      const rawData = response.Data || response.data;

      // Map API response to frontend format
      const mappedData = {
        uid: rawData.UID || rawData.uid,
        linkedItemType: rawData.LinkedItemType || rawData.linkedItemType,
        linkedItemUID: rawData.LinkedItemUid || rawData.linkedItemUID,
        requestID: rawData.UID || rawData.requestID,
        status: rawData.Status || rawData.status,
        requesterName: rawData.RequestedBy || rawData.requesterName || 'Unknown',
        requesterId: rawData.EmpUid || rawData.requesterId,
        createdOn: rawData.RequestDate || rawData.createdOn,
        modifiedOn: rawData.ApprovedDate || rawData.modifiedOn,
        itemDescription: rawData.ChangedRecord || rawData.itemDescription,
        currentApproverId: rawData.EmpUid || rawData.currentApproverId,
        currentApproverName: rawData.RequestedBy || rawData.currentApproverName,
        ...rawData
      };

      console.log('Mapped change request detail:', mappedData);
      return mappedData;
    } catch (error) {
      console.error('Error fetching change request by UID:', error);
      throw error;
    }
  },

  /**
   * Get approval details by linked item UID
   * @param requestUid - The linked item UID
   * @returns All approval request details
   */
  getApprovalDetailsByLinkedItemUid: async (requestUid: string): Promise<AllApprovalRequest> => {
    try {
      const response = await apiService.get<any>(
        `/ApprovalEngine/GetApprovalDetailsByLinkedItemUid?requestUid=${requestUid}`
      );
      const rawData = response.Data || response.data;

      // Map API response to frontend format
      const mappedData = {
        id: rawData.Id || rawData.id,
        linkedItemType: rawData.LinkedItemType || rawData.linkedItemType,
        linkedItemUID: requestUid, // Use the parameter as it's the linked item UID
        requestID: rawData.RequestID || rawData.requestID,
        approvalUserDetail: rawData.ApprovalUserDetail || rawData.approvalUserDetail,
        status: rawData.Status || rawData.status,
        createdOn: rawData.CreatedOn || rawData.createdOn,
        requesterName: rawData.RequesterName || rawData.requesterName,
        currentApproverName: rawData.CurrentApproverName || rawData.currentApproverName,
        ...rawData
      };

      console.log('Mapped approval details:', mappedData);
      return mappedData;
    } catch (error) {
      console.error('Error fetching approval details:', error);
      throw error;
    }
  },

  /**
   * Update/Apply approved changes to main table
   * @param approvalData - The approval data to update
   * @returns Success indicator
   */
  updateChangesInMainTable: async (approvalData: ViewChangeRequestApproval): Promise<number> => {
    try {
      const response = await apiService.put<any>(
        `/ApprovalEngine/UpdateChangesInMainTable`,
        approvalData
      );
      return response.Data || response.data;
    } catch (error) {
      console.error('Error updating changes:', error);
      throw error;
    }
  },

  /**
   * Delete/Cancel approval request
   * @param requestId - The request ID to delete
   * @returns Success indicator
   */
  deleteApprovalRequest: async (requestId: string): Promise<number> => {
    try {
      const response = await apiService.delete<any>(`/ApprovalEngine/DeleteApprovalRequest?requestId=${requestId}`);
      return response.Data || response.data;
    } catch (error) {
      console.error('Error deleting approval request:', error);
      throw error;
    }
  },

  /**
   * Get available role names for approval assignment
   * @returns List of roles
   */
  getRoleNames: async (): Promise<Array<{ key: string; value: string }>> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetRoleNames`);
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching role names:', error);
      throw error;
    }
  },

  /**
   * Get rule ID for a specific type and type code
   * @param type - The approval type (e.g., "ReturnOrder", "Initiative")
   * @param typeCode - The type code
   * @returns Rule ID
   */
  getRuleId: async (type: string, typeCode: string): Promise<number> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetRuleId?type=${type}&typeCode=${typeCode}`);
      return response.Data || response.data;
    } catch (error) {
      console.error('Error fetching rule ID:', error);
      throw error;
    }
  },

  /**
   * Get dropdown data for approval mapping
   * @returns Approval rule map options
   */
  getDropdownsForApprovalMapping: async (): Promise<any[]> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/DropDownsForApprovalMapping`);
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching approval mapping dropdowns:', error);
      throw error;
    }
  },

  /**
   * Integrate/Create approval rule mapping
   * @param approvalRuleMapping - Rule mapping data
   * @returns Success indicator
   */
  integrateRule: async (approvalRuleMapping: ApprovalRuleMapping): Promise<number> => {
    try {
      const response = await apiService.post<any>(
        `/ApprovalEngine/IntegrateRule`,
        approvalRuleMapping
      );
      return response.Data || response.data;
    } catch (error) {
      console.error('Error integrating approval rule:', error);
      throw error;
    }
  },

  /**
   * Get approval rule master data
   * @returns List of approval rules
   */
  getApprovalRuleMasterData: async (): Promise<any[]> => {
    try {
      const response = await apiService.get<any>(`/ApprovalEngine/GetApprovalRuleMasterData`);
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching approval rule master data:', error);
      throw error;
    }
  },

  /**
   * Get user hierarchy for a specific rule
   * @param hierarchyType - Type of hierarchy
   * @param hierarchyUID - Hierarchy UID
   * @param ruleId - Rule ID
   * @returns User hierarchy data
   */
  getUserHierarchyForRule: async (
    hierarchyType: string,
    hierarchyUID: string,
    ruleId: number
  ): Promise<any[]> => {
    try {
      const response = await apiService.get<any>(
        `/ApprovalEngine/GetUserHierarchyForRule/${hierarchyType}/${hierarchyUID}/${ruleId}`
      );
      return response.Data || response.data || [];
    } catch (error) {
      console.error('Error fetching user hierarchy for rule:', error);
      throw error;
    }
  },

  /**
   * Approve an approval request
   * @param actionRequest - Approval action details
   * @returns Success indicator
   */
  approveRequest: async (actionRequest: ApprovalActionRequest): Promise<any> => {
    try {
      // Get the change request first to update it
      const changeRequest = await approvalService.getChangeRequestByUid(actionRequest.requestId);

      // Parse the ChangedRecord to get field changes
      let changedRecord = changeRequest.itemDescription || changeRequest.ChangedRecord;
      if (typeof changedRecord === 'string') {
        try {
          changedRecord = JSON.parse(changedRecord);
        } catch (e) {
          console.warn('Failed to parse ChangedRecord:', e);
        }
      }

      // Convert to ChangeRecordDTO format expected by backend
      const changeRecordDTO = [{
        LinkedItemUID: changeRequest.linkedItemUID,
        Action: "update",
        ScreenModelName: changeRequest.linkedItemType || "Store",
        UID: changeRequest.uid,
        ChangeRecords: [
          {
            FieldName: changedRecord.field || "address",
            OldValue: changedRecord.oldValue || "",
            NewValue: changedRecord.newValue || ""
          }
        ]
      }];

      // Update the request with proper format
      const updatedRequest = {
        ...changeRequest,
        status: 'Approved',
        ApprovedDate: new Date().toISOString(),
        comments: actionRequest.comments,
        ChangedRecord: JSON.stringify(changeRecordDTO) // Backend expects JSON string of array
      };

      console.log('Sending approval request:', updatedRequest);

      // Call UpdateChangesInMainTable to apply the changes
      const response = await apiService.put<any>(
        `/ApprovalEngine/UpdateChangesInMainTable`,
        updatedRequest
      );

      return response.Data || response.data || response;
    } catch (error) {
      console.error('Error approving request:', error);
      throw error;
    }
  },

  /**
   * Reject an approval request
   * @param actionRequest - Rejection action details
   * @returns Success indicator
   */
  rejectRequest: async (actionRequest: ApprovalActionRequest): Promise<any> => {
    try {
      // For now, just log - rejection logic needs to be implemented in backend
      console.warn('Reject functionality needs backend implementation');
      // Temporary: Just delete the approval request
      const response = await apiService.delete<any>(
        `/ApprovalEngine/DeleteApprovalRequest?requestId=${actionRequest.requestId}`
      );
      return response.Data || response.data || response;
    } catch (error) {
      console.error('Error rejecting request:', error);
      throw error;
    }
  },

  /**
   * Reassign approval request to another user
   * @param actionRequest - Reassignment details
   * @returns Success indicator
   */
  reassignRequest: async (actionRequest: ApprovalActionRequest): Promise<any> => {
    try {
      // This endpoint might need to be created in backend
      const response = await apiService.post<any>(
        `/ApprovalEngine/ReassignRequest`,
        actionRequest
      );
      return response.Data || response.data || response;
    } catch (error) {
      console.error('Error reassigning request:', error);
      throw error;
    }
  },

  /**
   * Get approval statistics for dashboard
   * @param userId - Optional user ID to filter by approver
   * @returns Approval statistics
   */
  getApprovalStatistics: async (userId?: string): Promise<ApprovalStatistics> => {
    try {
      // This is a custom aggregation - might need backend support
      const requests = await approvalService.getAllChangeRequests();

      const stats: ApprovalStatistics = {
        totalPending: 0,
        totalApproved: 0,
        totalRejected: 0,
        byType: {}
      };

      requests.forEach(req => {
        if (req.status === 'Pending') stats.totalPending++;
        if (req.status === 'Approved') stats.totalApproved++;
        if (req.status === 'Rejected') stats.totalRejected++;

        // Count by type
        if (!stats.byType[req.linkedItemType as any]) {
          stats.byType[req.linkedItemType as any] = {
            pending: 0,
            approved: 0,
            rejected: 0
          };
        }

        if (req.status === 'Pending') stats.byType[req.linkedItemType as any]!.pending++;
        if (req.status === 'Approved') stats.byType[req.linkedItemType as any]!.approved++;
        if (req.status === 'Rejected') stats.byType[req.linkedItemType as any]!.rejected++;
      });

      return stats;
    } catch (error) {
      console.error('Error fetching approval statistics:', error);
      throw error;
    }
  },

  /**
   * Get complete approval detail including hierarchy and logs
   * @param requestUid - The change request UID
   * @returns Complete approval detail
   */
  getCompleteApprovalDetail: async (requestUid: string): Promise<ApprovalDetail> => {
    try {
      // First get the change request data by UID
      const request = await approvalService.getChangeRequestByUid(requestUid);

      // Get the linked approval request to find the numeric ID for logs/hierarchy
      const approvalDetails = await approvalService.getApprovalDetailsByLinkedItemUid(requestUid);

      let logs: ApprovalLog[] = [];
      let hierarchy: ApprovalHierarchy[] = [];

      console.log('Change request data:', request);
      console.log('Approval details for logs/hierarchy:', approvalDetails);

      // If we have approval details with a numeric ID, fetch logs and hierarchy
      if (approvalDetails && approvalDetails.id) {
        const numericRequestId = String(approvalDetails.id);
        console.log('Using numeric request ID for logs:', numericRequestId);

        try {
          logs = await approvalService.getApprovalLog(numericRequestId);
        } catch (e) {
          console.warn('Failed to load approval logs:', e);
        }

        // Check for ruleId in the approval details
        const ruleId = (approvalDetails as any).ruleId || (request as any).ruleId;
        if (ruleId) {
          try {
            hierarchy = await approvalService.getApprovalHierarchy(String(ruleId));
          } catch (e) {
            console.warn('Failed to load approval hierarchy:', e);
          }
        }
      } else {
        console.warn('No numeric ID found in approval details, skipping logs/hierarchy');
      }

      // Merge change request data with approval details for complete linkedItem
      const mergedLinkedItem = {
        ...approvalDetails,
        ...request, // Override with request data which has more details
        linkedItemUID: request.linkedItemUID || approvalDetails?.linkedItemUID || requestUid,
        status: request.status || approvalDetails?.status,
        requesterName: request.requesterName || approvalDetails?.requesterName,
        createdOn: request.createdOn || approvalDetails?.createdOn,
        linkedItemType: request.linkedItemType || approvalDetails?.linkedItemType
      };

      console.log('Merged linked item:', mergedLinkedItem);

      return {
        request: request as any,
        linkedItem: mergedLinkedItem as any,
        hierarchy,
        logs
      };
    } catch (error) {
      console.error('Error fetching complete approval detail:', error);
      throw error;
    }
  }
};

export default approvalService;
