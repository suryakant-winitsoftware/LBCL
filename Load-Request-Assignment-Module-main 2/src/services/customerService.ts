import { apiService } from './api';

export interface Customer {
  UID: string;
  Code: string;
  Name: string;
  Address?: string;
  AddressLine1?: string;
  AddressLine2?: string;
  City?: string;
  State?: string;
  Country?: string;
  PinCode?: string;
  ContactNo?: string;
  ContactPerson?: string;
  Email?: string;
  Type?: string;
  Status?: string;
  IsActive?: boolean;
  OrgUID?: string;
  CompanyUID?: string;
  LocationUID?: string;
  Latitude?: string;
  Longitude?: string;
  CreditLimit?: number;
  CreditDays?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  isCountRequired: boolean;
  sortCriterias?: Array<{
    columnName: string;
    sortDirection: 'ASC' | 'DESC';
  }>;
  filterCriterias?: Array<{
    columnName: string;
    filterValue: string;
    filterType: 'Equals' | 'Contains' | 'GreaterThan' | 'LessThan';
  }>;
}

export const customerService = {
  /**
   * Get all customers
   */
  async getAllCustomers(): Promise<Customer[]> {
    try {
      const response = await apiService.get('/api/Customer/SelectAllCustomers');
      return response?.data || [];
    } catch (error) {
      console.error('Error fetching customers:', error);
      return [];
    }
  },

  /**
   * Get customers with pagination and filters
   */
  async getCustomersPaged(params: PagingRequest) {
    try {
      const response = await apiService.get('/api/Customer/GetCustomersPaged', {
        params: {
          pageNumber: params.pageNumber,
          pageSize: params.pageSize,
          isCountRequired: params.isCountRequired
        }
      });
      return response?.data || { pagedData: [], totalCount: 0 };
    } catch (error) {
      console.error('Error fetching paged customers:', error);
      return { pagedData: [], totalCount: 0 };
    }
  },

  /**
   * Get customers filtered
   */
  async getCustomersFiltered(filter: string): Promise<Customer[]> {
    try {
      const response = await apiService.get('/api/Customer/GetCustomersFiltered', {
        params: { filter }
      });
      return response?.data || [];
    } catch (error) {
      console.error('Error fetching filtered customers:', error);
      return [];
    }
  },

  /**
   * Get customer by UID
   */
  async getCustomerByUID(uid: string): Promise<Customer | null> {
    try {
      const response = await apiService.get('/api/Customer/SelectCustomerByUID', {
        params: { UID: uid }
      });
      return response?.data || null;
    } catch (error) {
      console.error('Error fetching customer:', error);
      return null;
    }
  },

  /**
   * Get customers by organization
   */
  async getCustomersByOrg(orgUID: string): Promise<Customer[]> {
    try {
      // First try with filter
      const response = await apiService.get('/api/Customer/GetCustomersFiltered', {
        params: { filter: `OrgUID:${orgUID}` }
      });
      
      if (response?.data) {
        return response.data;
      }
      
      // Fallback to all customers and filter locally
      const allCustomers = await this.getAllCustomers();
      return allCustomers.filter(c => c.OrgUID === orgUID);
    } catch (error) {
      console.error('Error fetching customers by org:', error);
      return [];
    }
  },

  /**
   * Get stores (customers of type 'Store' or 'Retail')
   */
  async getStores(orgUID: string): Promise<Customer[]> {
    try {
      const customers = await this.getCustomersByOrg(orgUID);
      // Filter for store types
      return customers.filter(c => 
        c.IsActive && 
        (c.Type === 'Store' || c.Type === 'Retail' || c.Type === 'Wholesale' || !c.Type)
      );
    } catch (error) {
      console.error('Error fetching stores:', error);
      return [];
    }
  },

  /**
   * Create customer
   */
  async createCustomer(customer: Partial<Customer>) {
    try {
      const response = await apiService.post('/api/Customer/CreateCustomer', customer);
      return response?.data;
    } catch (error) {
      console.error('Error creating customer:', error);
      throw error;
    }
  },

  /**
   * Update customer
   */
  async updateCustomer(customer: Customer) {
    try {
      const response = await apiService.put('/api/Customer/UpdateCustomer', customer);
      return response?.data;
    } catch (error) {
      console.error('Error updating customer:', error);
      throw error;
    }
  },

  /**
   * Delete customer
   */
  async deleteCustomer(uid: string) {
    try {
      const response = await apiService.delete('/api/Customer/DeleteCustomer', {
        params: { UID: uid }
      });
      return response?.data;
    } catch (error) {
      console.error('Error deleting customer:', error);
      throw error;
    }
  },

  /**
   * Search customers by name or code
   */
  async searchCustomers(searchTerm: string, orgUID?: string): Promise<Customer[]> {
    try {
      const allCustomers = orgUID 
        ? await this.getCustomersByOrg(orgUID)
        : await this.getAllCustomers();
      
      const searchLower = searchTerm.toLowerCase();
      return allCustomers.filter(c => 
        c.Name?.toLowerCase().includes(searchLower) ||
        c.Code?.toLowerCase().includes(searchLower) ||
        c.Address?.toLowerCase().includes(searchLower) ||
        c.ContactNo?.includes(searchTerm)
      );
    } catch (error) {
      console.error('Error searching customers:', error);
      return [];
    }
  }
};