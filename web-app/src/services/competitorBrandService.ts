import { apiService } from '@/services/api';

export interface CompetitorBrandMapping {
  id?: number;
  uid?: string;
  categoryName?: string;
  categoryCode?: string;
  brandCode?: string;
  brandName?: string;
  competitorCompany?: string;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
}

export interface CompetitorBrandMappingDto {
  categoryCode: string;
  brandCode: string;
  competitorCode: string;
  createdBy?: string;
}

export interface CompetitorBrandUpdateDto {
  uid: string;
  competitorCode: string;
  modifiedBy?: string;
}

export interface DropdownOption {
  value: string;
  label: string;
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

class CompetitorBrandService {

  async getMappings(
    pageNumber: number = 1,
    pageSize: number = 10,
    categoryCode?: string,
    brandCode?: string
  ): Promise<PagedResponse<CompetitorBrandMapping>> {
    try {
      const params = new URLSearchParams({
        pageNumber: pageNumber.toString(),
        pageSize: pageSize.toString(),
      });
      
      if (categoryCode) params.append('categoryCode', categoryCode);
      if (brandCode) params.append('brandCode', brandCode);

      const response = await apiService.get<any>(
        `/CompetitorBrand/GetMappings?${params}`
      );

      // Transform the data to match the frontend interface
      const transformedData = (response.Data || response.data || []).map((item: any) => ({
        id: item.id,
        uid: item.uid,
        categoryName: item.categoryname,
        categoryCode: item.categorycode,
        brandCode: item.brandcode,
        brandName: item.brandname,
        competitorCompany: item.competitorcompany,
        createdBy: item.createdby,
        createdTime: item.createdtime,
        modifiedBy: item.modifiedby,
        modifiedTime: item.modifiedtime,
      }));

      return {
        data: transformedData,
        totalCount: response.TotalCount || response.totalCount || 0,
        pageNumber: response.PageNumber || response.pageNumber || pageNumber,
        pageSize: response.PageSize || response.pageSize || pageSize,
      };
    } catch (error) {
      console.error('Error fetching competitor brand mappings:', error);
      throw error;
    }
  }

  async createMapping(data: CompetitorBrandMappingDto): Promise<any> {
    try {
      const response = await apiService.post<any>(
        '/CompetitorBrand/Create',
        data
      );
      return response;
    } catch (error) {
      console.error('Error creating competitor brand mapping:', error);
      throw error;
    }
  }

  async updateMapping(data: CompetitorBrandUpdateDto): Promise<any> {
    try {
      const response = await apiService.put<any>(
        '/CompetitorBrand/Update',
        data
      );
      return response;
    } catch (error) {
      console.error('Error updating competitor brand mapping:', error);
      throw error;
    }
  }

  async deleteMapping(uid: string, deletedBy?: string): Promise<any> {
    try {
      const params = deletedBy ? `?deletedBy=${deletedBy}` : '';
      const response = await apiService.delete<any>(
        `/CompetitorBrand/Delete/${uid}${params}`
      );
      return response;
    } catch (error) {
      console.error('Error deleting competitor brand mapping:', error);
      throw error;
    }
  }

  async getCategories(): Promise<DropdownOption[]> {
    try {
      const response = await apiService.get<DropdownOption[]>(
        '/CompetitorBrand/GetCategories'
      );
      return response || [];
    } catch (error) {
      console.error('Error fetching categories:', error);
      throw error;
    }
  }

  async getBrandsByCategory(categoryCode: string): Promise<DropdownOption[]> {
    try {
      const response = await apiService.get<DropdownOption[]>(
        `/CompetitorBrand/GetBrandsByCategory/${categoryCode}`
      );
      return response || [];
    } catch (error) {
      console.error('Error fetching brands:', error);
      throw error;
    }
  }

  async getCompetitors(): Promise<DropdownOption[]> {
    try {
      const response = await apiService.get<DropdownOption[]>(
        '/CompetitorBrand/GetCompetitors'
      );
      // Filter out empty or dummy competitors and ensure unique values
      const filteredCompetitors = (response || []).filter((comp: DropdownOption) => 
        comp.value && 
        comp.value.trim() !== '' && 
        comp.value.toLowerCase() !== 'na' &&
        comp.value.toLowerCase() !== 'n/a' &&
        comp.value.toLowerCase() !== 'none' &&
        comp.value.toLowerCase() !== 'test'
      );
      
      // Remove duplicates based on value
      const uniqueCompetitors = filteredCompetitors.reduce((acc: DropdownOption[], current: DropdownOption) => {
        const exists = acc.find(item => item.value === current.value);
        if (!exists) {
          acc.push(current);
        }
        return acc;
      }, []);
      
      // Sort alphabetically
      return uniqueCompetitors.sort((a, b) => a.label.localeCompare(b.label));
    } catch (error) {
      console.error('Error fetching competitors:', error);
      throw error;
    }
  }

  async bulkDelete(uids: string[], deletedBy?: string): Promise<any> {
    try {
      const promises = uids.map(uid => this.deleteMapping(uid, deletedBy));
      const results = await Promise.all(promises);
      return results;
    } catch (error) {
      console.error('Error bulk deleting competitor brand mappings:', error);
      throw error;
    }
  }
}

export default new CompetitorBrandService();