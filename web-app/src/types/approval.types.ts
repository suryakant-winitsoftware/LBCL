// Approval System Type Definitions

// Approval Status Enum
export enum ApprovalStatus {
  PENDING = 'Pending',
  APPROVED = 'Approved',
  REJECTED = 'Rejected',
  IN_PROGRESS = 'InProgress',
  CANCELLED = 'Cancelled'
}

// Approval Item Types
export enum ApprovalItemType {
  INITIATIVE = 'Initiative',
  RETURN_ORDER = 'ReturnOrder',
  LEAVE_REQUEST = 'LeaveRequest',
  CREDIT_OVERRIDE = 'CreditOverride',
  TASK = 'Task',
  PROMOTION = 'Promotion',
  STORE = 'Store',
  PURCHASE_ORDER = 'PurchaseOrder'
}

// Main Approval Request Interface
export interface ApprovalRequest {
  id: number;
  ruleId: number;
  requesterId: string;
  requesterName?: string;
  status: ApprovalStatus;
  createdOn: string;
  modifiedOn?: string;
  modifiedBy?: string;
  approverId?: string;
  approverName?: string;
  comments?: string;
  currentLevel?: number;
  totalLevels?: number;
}

// Approval Hierarchy Interface
export interface ApprovalHierarchy {
  id: number;
  ruleId: number;
  approverId: string;
  approverName?: string;
  approverEmail?: string;
  level: number;
  nextApproverId?: string;
  approverType?: string;
  roleName?: string;
}

// Approval Log Interface
export interface ApprovalLog {
  id: number;
  requestId: number;
  approverId: string;
  approverName?: string;
  level: number;
  status: ApprovalStatus;
  comments?: string;
  modifiedBy?: string;
  reassignTo?: string;
  reassignToName?: string;
  createdOn: string;
  actionDate?: string;
}

// All Approval Request (Universal Linking)
export interface AllApprovalRequest {
  id: number;
  linkedItemType: ApprovalItemType | string;
  linkedItemUID: string;
  requestID: string;
  approvalUserDetail?: string; // JSON string
  status?: ApprovalStatus;
  createdOn?: string;
  requesterName?: string;
  currentApproverName?: string;
}

// View Change Request Approval (For listing)
export interface ViewChangeRequestApproval {
  uid: string;
  linkedItemType: string;
  linkedItemUID: string;
  requestID: string;
  approvalUserDetail?: string;
  status: ApprovalStatus;
  requesterName?: string;
  requesterId?: string;
  currentApproverId?: string;
  currentApproverName?: string;
  createdOn: string;
  modifiedOn?: string;
  comments?: string;
  itemDescription?: string;
  priority?: string;
  // Additional fields based on item type
  [key: string]: any;
}

// Approval Action Request
export interface ApprovalActionRequest {
  requestId: string;
  approverId: string;
  action: 'approve' | 'reject' | 'reassign';
  comments?: string;
  reassignTo?: string;
  level?: number;
}

// Approval Rule Mapping
export interface ApprovalRuleMapping {
  id: number;
  uid: string;
  ruleId: number;
  type: string;
  typeCode: string;
  createdBy?: string;
  createdTime: string;
  modifiedTime?: string;
}

// Approval Statistics (for dashboard)
export interface ApprovalStatistics {
  totalPending: number;
  totalApproved: number;
  totalRejected: number;
  byType: {
    [key in ApprovalItemType]?: {
      pending: number;
      approved: number;
      rejected: number;
    };
  };
  pendingByPriority?: {
    high: number;
    medium: number;
    low: number;
  };
}

// Approval Detail (Combined view)
export interface ApprovalDetail {
  request: ApprovalRequest;
  linkedItem: AllApprovalRequest;
  hierarchy: ApprovalHierarchy[];
  logs: ApprovalLog[];
  itemData?: any; // Specific data based on item type
}

// Initiative Approval Data
export interface InitiativeApprovalData {
  initiativeId: string;
  initiativeName: string;
  brandName?: string;
  taskType?: string;
  storeName?: string;
  storeCode?: string;
  executedDate?: string;
  imageUrl?: string;
  location?: {
    latitude: number;
    longitude: number;
    address?: string;
  };
  executionComments?: string;
}

// Return Order Approval Data
export interface ReturnOrderApprovalData {
  returnOrderId: string;
  orderNumber?: string;
  customerName?: string;
  customerCode?: string;
  returnType: 'Good' | 'Bad';
  referenceInvoice?: string;
  totalAmount: number;
  totalItems: number;
  items: Array<{
    itemCode: string;
    itemName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    reason: string;
  }>;
  reason: string;
  createdByName?: string;
}

// Leave Request Approval Data
export interface LeaveRequestApprovalData {
  leaveId: string;
  employeeCode: string;
  employeeName: string;
  leaveType: 'Annual' | 'Sick' | 'Emergency' | 'Other';
  startDate: string;
  endDate: string;
  numberOfDays: number;
  reason: string;
  attachments?: Array<{
    fileName: string;
    fileUrl: string;
    fileType: string;
  }>;
  balanceLeaves?: {
    annual: number;
    sick: number;
    emergency: number;
  };
}

// Paging Request for Approvals
export interface ApprovalPagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias?: Array<{
    field: string;
    direction: 'asc' | 'desc';
  }>;
  filterCriterias?: Array<{
    field: string;
    operator: string;
    value: any;
  }>;
  isCountRequired?: boolean;
}

// Paged Response
export interface PagedApprovalResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

// Approval Filter Options
export interface ApprovalFilterOptions {
  status?: ApprovalStatus[];
  itemType?: ApprovalItemType[];
  dateFrom?: string;
  dateTo?: string;
  requesterId?: string;
  priority?: string[];
  searchTerm?: string;
}
