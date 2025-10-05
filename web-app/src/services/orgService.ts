/**
 * Organization Service
 * Handles all Organization related API calls
 */

import { apiService } from "./api";

export interface IOrganization {
  UID: string;
  Code: string;
  Name: string;
  OrgTypeUID?: string;
  ParentUID?: string;
  Status?: string;
  IsActive?: boolean;
}

class OrgService {
  /**
   * Get organizations by org type UID
   */
  async getOrgByOrgTypeUID(
    orgTypeUID: string,
    parentUID?: string,
    branchUID?: string
  ): Promise<{ success: boolean; data: IOrganization[] }> {
    try {
      const params = new URLSearchParams();
      params.append("OrgTypeUID", orgTypeUID);
      if (parentUID) params.append("parentUID", parentUID);
      if (branchUID) params.append("branchUID", branchUID);

      const response = await apiService.get(
        `/Org/GetOrgByOrgTypeUID?${params.toString()}`
      );

      if (response && response.Data) {
        return {
          success: true,
          data: response.Data,
        };
      }

      return {
        success: false,
        data: [],
      };
    } catch (error) {
      console.error("Error fetching organizations by type:", error);
      return {
        success: false,
        data: [],
      };
    }
  }
}

export const orgService = new OrgService();
