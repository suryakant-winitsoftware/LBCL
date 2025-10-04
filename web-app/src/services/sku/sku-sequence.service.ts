import { apiService } from '../api';

export interface SkuSequence {
  id?: number;
  uid: string;
  createdBy?: string;
  createdTime?: string;
  modifiedBy?: string;
  modifiedTime?: string;
  serverAddTime?: string;
  serverModifiedTime?: string;
  buOrgUID?: string;
  franchiseeOrgUID?: string;
  seqType: string; // 'General', 'Route', 'Customer', etc.
  skuUID: string;
  serialNo: number;
  actionType?: 'Add' | 'Delete';
}

export interface SkuSequenceUI extends SkuSequence {
  skuCode: string;
  skuName: string;
  isSelected?: boolean;
}

export interface SkuSequenceFilter {
  buOrgUID?: string;
  franchiseeOrgUID?: string;
  seqType?: string;
  skuUID?: string;
}

export interface PagedSkuSequenceResponse {
  pagedData: SkuSequenceUI[];
  totalCount: number;
}

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias?: Array<{
    sortParameter: string;
    direction: 'Asc' | 'Desc';
  }>;
  filterCriterias?: Array<{
    propertyName: string;
    value: string;
    operation: string;
  }>;
  isCountRequired: boolean;
}

class SkuSequenceService {
  /**
   * Get all SKU sequences with pagination and filtering
   */
  async getSkuSequences(
    seqType: string,
    pagingRequest: PagingRequest
  ): Promise<PagedSkuSequenceResponse> {
    try {
      const response: any = await apiService.post(
        `/SkuSequence/SelectAllSkuSequenceDetails?SeqType=${seqType}`,
        pagingRequest
      );

      console.log('📥 SKU Sequence API Response:', response);

      // Backend returns: { Data: { PagedData: [...], TotalCount: 0 }, StatusCode: 200, IsSuccess: true }
      const pagedData = response.Data?.PagedData || [];

      // Map backend response to UI format
      const mappedData: SkuSequenceUI[] = pagedData.map((item: any) => ({
        id: item.Id,
        uid: item.UID,
        createdBy: item.CreatedBy,
        createdTime: item.CreatedTime,
        modifiedBy: item.ModifiedBy,
        modifiedTime: item.ModifiedTime,
        serverAddTime: item.ServerAddTime,
        serverModifiedTime: item.ServerModifiedTime,
        buOrgUID: item.BUOrgUID,
        franchiseeOrgUID: item.FranchiseeOrgUID,
        seqType: item.SeqType,
        skuUID: item.SKUUID,
        serialNo: item.SerialNo,
        skuCode: item.SKUCode || 'N/A',
        skuName: item.SKUName || 'Unknown Product',
      }));

      console.log('✅ Mapped Data:', mappedData);

      return {
        pagedData: mappedData,
        totalCount: response.Data?.TotalCount || 0
      };
    } catch (error: any) {
      console.error('Error fetching SKU sequences:', error);
      throw new Error(
        error.message || 'Failed to fetch SKU sequences'
      );
    }
  }

  /**
   * Create, Update, or Delete SKU sequences
   */
  async saveSkuSequences(sequences: SkuSequence[]): Promise<number> {
    try {
      const response: any = await apiService.post('/SkuSequence/CUDSkuSequence', sequences);
      // Backend returns: { Data: number, StatusCode: 200, IsSuccess: true }
      return response.Data || 0;
    } catch (error: any) {
      console.error('Error saving SKU sequences:', error);
      throw new Error(
        error.message || 'Failed to save SKU sequences'
      );
    }
  }

  /**
   * Create general sequence for a new SKU
   */
  async createGeneralSequenceForSKU(
    buOrgUID: string,
    skuUID: string
  ): Promise<number> {
    try {
      const response: any = await apiService.post(
        `/SkuSequence/CreateGeneralSKUSequenceForSKU?BUOrgUID=${buOrgUID}&SKUUID=${skuUID}`,
        {}
      );
      // Backend returns: { Data: number, StatusCode: 200, IsSuccess: true }
      return response.Data || 0;
    } catch (error: any) {
      console.error('Error creating general sequence:', error);
      throw new Error(
        error.message || 'Failed to create general sequence'
      );
    }
  }

  /**
   * Reorder SKU sequences - updates serial numbers
   */
  async reorderSequences(
    sequences: SkuSequenceUI[],
    seqType: string
  ): Promise<SkuSequence[]> {
    const currentTime = new Date().toISOString();
    const updates: SkuSequence[] = sequences.map((seq, index) => ({
      uid: seq.uid,
      skuUID: seq.skuUID,
      seqType: seqType,
      serialNo: index + 1, // New position (1-based)
      buOrgUID: seq.buOrgUID,
      franchiseeOrgUID: seq.franchiseeOrgUID,
      modifiedBy: 'ADMIN', // Will be set from session
      modifiedTime: currentTime,
      serverModifiedTime: currentTime,
      createdBy: seq.createdBy || 'ADMIN',
      createdTime: seq.createdTime || currentTime,
      serverAddTime: seq.serverAddTime || currentTime,
      actionType: 'Add' as const // Add means update if exists
    }));

    return updates;
  }

  /**
   * Delete SKU sequences
   */
  async deleteSkuSequences(uids: string[]): Promise<number> {
    try {
      const deletions: SkuSequence[] = uids.map(uid => ({
        uid: uid,
        skuUID: '',
        seqType: '',
        serialNo: 0,
        actionType: 'Delete' as const
      }));

      const response: any = await apiService.post('/SkuSequence/CUDSkuSequence', deletions);
      return response.Data || 0;
    } catch (error: any) {
      console.error('Error deleting SKU sequences:', error);
      throw new Error(
        error.message || 'Failed to delete SKU sequences'
      );
    }
  }
}

export default new SkuSequenceService();
