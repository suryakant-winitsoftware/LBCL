import { apiService } from '@/services/api';
import { authService } from '@/lib/auth-service';

// Updated Task interfaces to match new backend
export interface Task {
  id?: number;
  uid?: string;
  code?: string;
  title: string;
  description?: string;
  taskTypeId: number;
  taskTypeName?: string;
  taskSubTypeId?: number;
  taskSubTypeName?: string;
  salesOrgId: number;
  salesOrgName?: string;
  startDate: string;
  endDate: string;
  isActive?: boolean;
  priority: string;
  status: string;
  taskData?: string;
  createdTime?: string;
  createdBy?: string;
  modifiedTime?: string;
  modifiedBy?: string;
  assignments?: TaskAssignment[];
}

export interface TaskType {
  id: number;
  uid: string;
  code: string;
  name: string;
  description?: string;
  isActive: boolean;
  sortOrder: number;
}

export interface TaskSubType {
  id: number;
  uid: string;
  code: string;
  name: string;
  description?: string;
  taskTypeId: number;
  isActive: boolean;
  sortOrder: number;
}

export interface TaskAssignment {
  id?: number;
  uid?: string;
  taskId: number;
  assignedToType: string;
  userId?: number;
  userName?: string;
  userGroupId?: number;
  userGroupName?: string;
  status: string;
  assignedDate?: string;
  startedDate?: string;
  completedDate?: string;
  notes?: string;
  progress?: number;
}

export interface TaskFilter {
  taskTypeId?: number;
  taskSubTypeId?: number;
  salesOrgId?: number;
  status?: string;
  startDate?: string;
  endDate?: string;
  assignedUserId?: number;
  assignedUserGroupId?: number;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface TaskCreateRequest {
  title: string;
  description?: string;
  taskTypeId: number;
  taskSubTypeId?: number;
  salesOrgId: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  priority: string;
  taskData?: string;
}

export interface AssignTaskRequest {
  taskId: number;
  assignedToType: string;
  userIds?: number[];
  userGroupIds?: number[];
  notes?: string;
}

export interface TaskStatusUpdateRequest {
  uid: string;
  status: string;
  startedDate?: string;
  completedDate?: string;
  notes?: string;
  progress?: number;
}

export interface TaskStatusUpdateRequest {
  taskId: number;
  status: string;
  comments?: string;
  actualDurationMinutes?: number;
  rating?: number;
  ratingNotes?: string;
}

export interface TaskSummary {
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  overdueTasks: number;
  averageRating?: number;
  completionRate?: number;
}

export interface TaskPerformance {
  userId: string;
  userName?: string;
  totalTasks: number;
  completedTasks: number;
  pendingTasks: number;
  overdueTasks: number;
  averageRating?: number;
  averageDuration?: number;
  completionPercentage?: number;
}

class TaskService {
  private baseUrl = 'Task';

  // Get all tasks with filtering
  async getAllTasks(filter?: TaskFilter) {
    return this.getPagedTasks(filter);
  }

  // Get paged tasks using new backend API
  async getPagedTasks(filter?: TaskFilter) {
    try {
      const response = await apiService.post<any>(`${this.baseUrl}/GetTasksByFilter`, filter || {});
      return { 
        data: { 
          Data: response?.data || response || [], 
          TotalCount: response?.data?.length || 0 
        } 
      };
    } catch (error) {
      console.error('Error getting paged tasks:', error);
      return { data: { Data: [], TotalCount: 0 } };
    }
  }

  // Get task by UID (our backend uses UID)
  async getTaskById(id: number) {
    // Note: This would need the UID, but we'll try with the ID first
    return apiService.get<Task>(`${this.baseUrl}/GetTaskByUID/${id}`);
  }

  async getTaskByUID(uid: string) {
    return apiService.get<Task>(`${this.baseUrl}/GetTaskByUID/${uid}`);
  }

  // Create new task
  async createTask(task: TaskCreateRequest) {
    return apiService.post<any>(`${this.baseUrl}/CreateTask`, task);
  }

  // Update task
  async updateTask(task: Task) {
    return apiService.put<any>(`${this.baseUrl}/UpdateTask`, task);
  }

  // Delete task
  async deleteTask(uid: string) {
    return apiService.delete(`${this.baseUrl}/DeleteTask/${uid}`);
  }

  // Get task types
  async getTaskTypes() {
    return apiService.get<TaskType[]>(`${this.baseUrl}/GetAllTaskTypes`);
  }

  // Get task sub types by task type
  async getTaskSubTypes(taskTypeId: number) {
    return apiService.get<TaskSubType[]>(`${this.baseUrl}/GetTaskSubTypesByTaskType/${taskTypeId}`);
  }

  // Task Type Management
  async createTaskType(taskType: Omit<TaskType, 'id'>) {
    return apiService.post<any>(`${this.baseUrl}/CreateTaskType`, taskType);
  }

  async updateTaskType(taskType: TaskType) {
    return apiService.put<any>(`${this.baseUrl}/UpdateTaskType`, taskType);
  }

  async deleteTaskType(uid: string) {
    return apiService.delete(`${this.baseUrl}/DeleteTaskType/${uid}`);
  }

  async getTaskTypeByUID(uid: string) {
    return apiService.get<TaskType>(`${this.baseUrl}/GetTaskTypeByUID/${uid}`);
  }

  // Task Sub Type Management
  async createTaskSubType(taskSubType: Omit<TaskSubType, 'id'>) {
    return apiService.post<any>(`${this.baseUrl}/CreateTaskSubType`, taskSubType);
  }

  async updateTaskSubType(taskSubType: TaskSubType) {
    return apiService.put<any>(`${this.baseUrl}/UpdateTaskSubType`, taskSubType);
  }

  async deleteTaskSubType(uid: string) {
    return apiService.delete(`${this.baseUrl}/DeleteTaskSubType/${uid}`);
  }

  async getTaskSubTypeByUID(uid: string) {
    return apiService.get<TaskSubType>(`${this.baseUrl}/GetTaskSubTypeByUID/${uid}`);
  }

  // Assign task to users/groups
  async assignTask(assignment: AssignTaskRequest) {
    return apiService.post<any>(`${this.baseUrl}/AssignTask`, assignment);
  }

  // Get task assignments
  async getTaskAssignments(taskId: number) {
    return apiService.get<TaskAssignment[]>(`${this.baseUrl}/GetTaskAssignments/${taskId}`);
  }

  // Get user task assignments
  async getUserTaskAssignments(userId: number) {
    return apiService.get<TaskAssignment[]>(`${this.baseUrl}/GetUserTaskAssignments/${userId}`);
  }

  // Update task assignment status
  async updateTaskAssignment(assignment: TaskStatusUpdateRequest) {
    return apiService.put<any>(`${this.baseUrl}/UpdateTaskAssignment`, assignment);
  }

  // Get tasks dashboard
  async getTasksDashboard(userId?: number, userGroupId?: number, salesOrgId?: number) {
    const params = new URLSearchParams();
    if (userId) params.append('userId', userId.toString());
    if (userGroupId) params.append('userGroupId', userGroupId.toString());
    if (salesOrgId) params.append('salesOrgId', salesOrgId.toString());
    const queryString = params.toString();
    const url = queryString ? `${this.baseUrl}/GetTasksDashboard?${queryString}` : `${this.baseUrl}/GetTasksDashboard`;
    return apiService.get<Task[]>(url);
  }

  // Get task status counts
  async getTaskStatusCounts(userId?: number, salesOrgId?: number) {
    const params = new URLSearchParams();
    if (userId) params.append('userId', userId.toString());
    if (salesOrgId) params.append('salesOrgId', salesOrgId.toString());
    const queryString = params.toString();
    const url = queryString ? `${this.baseUrl}/GetTaskStatusCounts?${queryString}` : `${this.baseUrl}/GetTaskStatusCounts`;
    return apiService.get<any>(url);
  }

  // Validate task dates
  async validateTaskDates(startDate: string, endDate: string) {
    return apiService.post<{ IsValid: boolean }>(`${this.baseUrl}/ValidateTaskDates?startDate=${startDate}&endDate=${endDate}`, null);
  }

  // Check user access to task
  async canUserAccessTask(taskId: number, userId: number) {
    return apiService.get<{ CanAccess: boolean }>(`${this.baseUrl}/CanUserAccessTask/${taskId}/${userId}`);
  }
}

const taskService = new TaskService();
export default taskService;