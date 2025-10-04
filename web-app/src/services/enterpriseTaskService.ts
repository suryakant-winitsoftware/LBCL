import { apiService } from '@/services/api';

// Enterprise Task interfaces
export interface EnterpriseTask {
  id?: number;
  taskType: string;
  taskSubType?: string;
  taskName: string;
  salesOrganization: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  taskDescription?: string;
  status: string;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
}

export interface EnterpriseTaskCreateRequest {
  taskType: string;
  taskSubType?: string;
  salesOrganization: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  taskDescription?: string;
}

export interface TaskAssignment {
  id?: number;
  enterpriseTaskId: number;
  userGroupType: string;
  userGroupId: string;
  userGroupName?: string;
  status: string;
  assignedDate: string;
  assignedBy?: string;
  startedDate?: string;
  completedDate?: string;
  progressPercentage: number;
  notes?: string;
}

export interface TaskAssignmentRequest {
  enterpriseTaskId: number;
  userGroupType: string;
  userGroupId: string;
  userGroupName?: string;
  notes?: string;
}

export interface TaskType {
  id: string;
  name: string;
  subTypes: string[];
}

export interface SalesOrganization {
  id: string;
  name: string;
  code: string;
}

export interface UserGroup {
  id: string;
  name: string;
  description: string;
}

export interface SpecificUserGroup {
  id: string;
  name: string;
}

class EnterpriseTaskService {
  private baseUrl = '/api/EnterpriseTask';

  // Get all enterprise tasks
  async getAllTasks() {
    return apiService.get<EnterpriseTask[]>(this.baseUrl);
  }

  // Get enterprise task by ID
  async getTaskById(id: number) {
    return apiService.get<EnterpriseTask>(`${this.baseUrl}/${id}`);
  }

  // Create new enterprise task
  async createTask(task: EnterpriseTaskCreateRequest) {
    return apiService.post<any>(this.baseUrl, task);
  }

  // Update enterprise task
  async updateTask(id: number, task: EnterpriseTaskCreateRequest) {
    return apiService.put<any>(`${this.baseUrl}/${id}`, task);
  }

  // Delete enterprise task
  async deleteTask(id: number) {
    return apiService.delete(`${this.baseUrl}/${id}`);
  }

  // Assign task to user groups
  async assignTask(id: number, assignment: TaskAssignmentRequest) {
    return apiService.post<any>(`${this.baseUrl}/${id}/assign`, assignment);
  }

  // Get task assignments for a specific enterprise task
  async getTaskAssignments(id: number) {
    return apiService.get<TaskAssignment[]>(`${this.baseUrl}/${id}/assignments`);
  }

  // Get available task types
  async getTaskTypes() {
    return apiService.get<TaskType[]>(`${this.baseUrl}/task-types`);
  }

  // Get available sales organizations
  async getSalesOrganizations() {
    return apiService.get<SalesOrganization[]>(`${this.baseUrl}/sales-organizations`);
  }

  // Get available user groups
  async getUserGroups() {
    return apiService.get<UserGroup[]>(`${this.baseUrl}/user-groups`);
  }

  // Get specific user groups by type
  async getSpecificUserGroups(groupType: string) {
    return apiService.get<SpecificUserGroup[]>(`${this.baseUrl}/user-groups/${groupType}/specific`);
  }
}

const enterpriseTaskService = new EnterpriseTaskService();
export default enterpriseTaskService;