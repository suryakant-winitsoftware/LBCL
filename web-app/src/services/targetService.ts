import { apiService } from '@/services/api';
import { authService } from '@/lib/auth-service';

export interface Target {
  Id?: number;
  UserLinkedType: string;
  UserLinkedUid: string;
  CustomerLinkedType?: string;
  CustomerLinkedUid?: string;
  ItemLinkedItemType?: string;
  ItemLinkedItemUid?: string;
  TargetMonth: number;
  TargetYear: number;
  TargetAmount: number;
  Status?: string;
  Notes?: string;
  CreatedTime?: string;
  CreatedBy?: string;
  ModifiedTime?: string;
  ModifiedBy?: string;
}

export interface TargetFilter {
  UserLinkedType?: string;
  UserLinkedUid?: string;
  CustomerLinkedType?: string;
  CustomerLinkedUid?: string;
  ItemLinkedItemType?: string;
  TargetMonth?: number;
  TargetYear?: number;
  PageNumber?: number;
  PageSize?: number;
}

export interface TargetSummary {
  UserLinkedUid: string;
  UserName?: string;
  CustomerLinkedType?: string;
  CustomerLinkedUid?: string;
  CustomerName?: string;
  TargetMonth: number;
  TargetYear: number;
  TotalTarget: number;
  CosmeticsTarget: number;
  FmcgNonFoodTarget: number;
  FmcgFoodTarget: number;
}

class TargetService {
  private baseUrl = 'Target';

  async getAllTargets(filter?: TargetFilter) {
    const params = new URLSearchParams();
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString());
        }
      });
    }
    const queryString = params.toString();
    const url = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;
    return apiService.get<{ data: Target[] }>(url).then(res => ({ data: res as any }));
  }

  async getPagedTargets(filter?: TargetFilter) {
    const params = new URLSearchParams();
    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          params.append(key, value.toString());
        }
      });
    }
    const queryString = params.toString();
    const url = queryString ? `${this.baseUrl}/paged?${queryString}` : `${this.baseUrl}/paged`;
    return apiService.get<any>(url).then(res => ({ data: res?.Data || res || [] }));
  }

  async getTargetById(id: number) {
    return apiService.get<any>(`${this.baseUrl}/${id}`).then(res => ({ data: res?.Data || res }));
  }

  async createTarget(target: Target) {
    return apiService.post<any>(this.baseUrl, target).then(res => ({ data: res?.Data || res }));
  }

  async updateTarget(id: number, target: Target) {
    return apiService.put<any>(`${this.baseUrl}/${id}`, target).then(res => ({ data: res?.Data || res }));
  }

  async deleteTarget(id: number) {
    return apiService.delete(`${this.baseUrl}/${id}`);
  }

  async getTargetSummary(userLinkedUid: string, year: number, month: number) {
    const params = new URLSearchParams({
      userLinkedUid,
      year: year.toString(),
      month: month.toString()
    });
    return apiService.get<any>(`${this.baseUrl}/summary?${params}`).then(res => ({ data: res?.Data || res || [] }));
  }

  async bulkCreateTargets(targets: Target[]) {
    return apiService.post<any>(`${this.baseUrl}/bulk`, targets).then(res => ({ data: res?.Data || res }));
  }

  async getTargetsByEmployee(employeeId: string) {
    return apiService.get<any>(`${this.baseUrl}/by-employee/${employeeId}`).then(res => ({ data: res?.Data || res || [] }));
  }

  async getTargetsByCustomer(customerId: string) {
    return apiService.get<any>(`${this.baseUrl}/by-customer/${customerId}`).then(res => ({ data: res?.Data || res || [] }));
  }

}

export default new TargetService();