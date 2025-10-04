/**
 * Distributor Service
 * Handles all Distributor related API calls
 */

import { apiService } from "./api";
import { PagingRequest, PagedResponse } from "@/types/common.types";

// Distributor interfaces
export interface IDistributor {
  UID?: string;
  Code?: string;
  Name?: string;
  SequenceCode?: string;
  ContactPerson?: string;
  ContactNumber?: string;
  Status?: string;
  OpenAccountDate?: Date | string;
}

export interface IDistributorMasterView {
  Org: IOrg;
  Store: IStore;
  StoreAdditionalInfo: IStoreAdditionalInfo;
  StoreCredit: IStoreCredit;
  Address: IAddress;
  Contacts: IContact[];
  Documents: IStoreDocument[];
}

export interface IOrg {
  UID?: string;
  Code: string;
  Name: string;
  SeqCode?: string;
  OrgTypeUid?: string;
  ParentUid?: string;
  CountryUid?: string;
  CompanyUid?: string;
  TaxGroupUid?: string;
  Status?: string;
  HasEarlyAccess?: boolean;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IStore {
  Id?: number;
  UID?: string;
  Code: string;
  Name: string;
  AliasName?: string;
  LegalName?: string;
  Type?: string;
  Status?: string;
  IsActive?: boolean;
  StoreClass?: string;
  StoreRating?: string;
  IsBlocked?: boolean;
  BlockedReasonCode?: string;
  BlockedReasonDescription?: string;
  CountryUid?: string;
  RegionUid?: string;
  CityUid?: string;
  StateUid?: string;
  FranchiseeOrgUid?: string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IStoreAdditionalInfo {
  UID?: string;
  OrderType?: string;
  IsPromotionsBlock?: boolean;
  CustomerStartDate?: Date | string;
  CustomerEndDate?: Date | string;
  PurchaseOrderNumber?: string;
  PaymentMode?: string;
  PriceType?: string;
  VisitFrequency?: number;
  AgingCycle?: number;
}

export interface IStoreCredit {
  Id?: number;
  UID?: string;
  StoreUid?: string;
  PaymentTermUid?: string;
  CreditType?: string;
  CreditLimit?: number;
  TemporaryCredit?: number;
  OrgUid?: string;
  DistributionChannelUid?: string;
  PreferredPaymentMode?: string;
  IsActive?: boolean;
  IsBlocked?: boolean;
  BlockingReasonCode?: string;
  BlockingReasonDescription?: string;
  OutstandingInvoices?: number;
  PaymentType?: string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IContact {
  Id?: number;
  UID?: string;
  Title?: string;
  Name: string;
  Phone?: string;
  PhoneExtension?: string;
  Description?: string;
  Designation?: string;
  Mobile?: string;
  Mobile2?: string;
  Email?: string;
  Email2?: string;
  Email3?: string;
  Fax?: string;
  LinkedItemUID?: string;
  LinkedItemType?: string;
  IsDefault?: boolean;
  IsEditable?: boolean;
  EnabledForInvoiceEmail?: boolean;
  EnabledForDocketEmail?: boolean;
  EnabledForPromoEmail?: boolean;
  IsEmailCC?: boolean;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IAddress {
  Id?: number;
  UID?: string;
  Type?: string;
  Name?: string;
  Line1?: string;
  Line2?: string;
  Line3?: string;
  Line4?: string;
  Landmark?: string;
  Area?: string;
  SubArea?: string;
  ZipCode?: string;
  City?: string;
  CountryCode?: string;
  RegionCode?: string;
  StateCode?: string;
  TerritoryCode?: string;
  Phone?: string;
  PhoneExtension?: string;
  Mobile1?: string;
  Mobile2?: string;
  Email?: string;
  Fax?: string;
  Latitude?: number;
  Longitude?: number;
  Altitude?: number;
  LinkedItemUID?: string;
  LinkedItemType?: string;
  Status?: string;
  IsEditable?: boolean;
  IsDefault?: boolean;
  Info?: string;
  Depot?: string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IStoreDocument {
  Id?: number;
  UID?: string;
  StoreUID?: string;
  DocumentType?: string;
  DocumentNo?: string;
  ValidFrom?: Date | string;
  ValidUpTo?: Date | string;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IDistributorAdminDTO {
  Emp: IEmp;
  JobPosition: IJobPosition;
  ActionType: DistributorAdminActionType;
}

export interface IEmp {
  UID?: string;
  Code?: string;
  FirstName?: string;
  LastName?: string;
  FullName?: string;
  Email?: string;
  Mobile?: string;
  OrgUID?: string;
  EncryptedPassword?: string;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export interface IJobPosition {
  UID?: string;
  EmpUID?: string;
  OrgUID?: string;
  RoleUID?: string;
  IsActive?: boolean;
  CreatedBy?: string;
  CreatedTime?: Date | string;
  ModifiedBy?: string;
  ModifiedTime?: Date | string;
}

export enum DistributorAdminActionType {
  Add = 0,
  UpdateUserName = 1,
  UpdatePW = 2,
}

class DistributorService {
  /**
   * Get all distributors with pagination and filtering
   */
  async getAllDistributors(
    request: PagingRequest
  ): Promise<PagedResponse<IDistributor>> {
    try {
      const response = await apiService.post(
        "/Distributor/SelectAllDistributors",
        request
      );

      // Handle different response structures from .NET API
      if (response && typeof response === "object") {
        if (response.PagedData !== undefined) {
          return {
            PagedData: response.PagedData,
            TotalCount: response.TotalCount || 0,
          };
        }
        if (response.Data && response.Data.PagedData !== undefined) {
          return {
            PagedData: response.Data.PagedData,
            TotalCount: response.Data.TotalCount || 0,
          };
        }
      }

      return {
        PagedData: [],
        TotalCount: 0,
      };
    } catch (error) {
      console.error("Error fetching distributors:", error);
      throw error;
    }
  }

  /**
   * Get distributor details by UID
   */
  async getDistributorByUID(uid: string): Promise<IDistributorMasterView> {
    try {
      const response = await apiService.get(
        `/Distributor/GetDistributorDetailsByUID?UID=${uid}`
      );

      if (response && response.Data) {
        return response.Data;
      }

      throw new Error("Distributor not found");
    } catch (error) {
      console.error("Error fetching distributor details:", error);
      throw error;
    }
  }

  /**
   * Create a new distributor
   */
  async createDistributor(
    distributorData: IDistributorMasterView
  ): Promise<number> {
    try {
      const response = await apiService.post(
        "/Distributor/CreateDistributor",
        distributorData
      );

      if (response && response.Data) {
        return response.Data;
      }

      return 0;
    } catch (error) {
      console.error("Error creating distributor:", error);
      throw error;
    }
  }

  /**
   * Create/Update/Delete distributor admin user
   */
  async cudDistributorAdmin(adminData: IDistributorAdminDTO): Promise<number> {
    try {
      const response = await apiService.post(
        "/Distributor/CUDDistributorAdmin",
        adminData
      );

      if (response && response.Data !== undefined) {
        return response.Data;
      }

      return 0;
    } catch (error) {
      console.error("Error managing distributor admin:", error);
      throw error;
    }
  }

  /**
   * Get all distributor admin users by organization UID
   */
  async getDistributorAdminsByOrgUID(orgUID: string): Promise<IEmp[]> {
    try {
      const response = await apiService.get(
        `/Distributor/SelectAllDistributorAdminDetailsByOrgUID?OrgUID=${orgUID}`
      );

      if (response && response.Data) {
        return response.Data;
      }

      return [];
    } catch (error) {
      console.error("Error fetching distributor admins:", error);
      throw error;
    }
  }

  /**
   * Export distributors to CSV
   */
  async exportToCSV(searchTerm?: string): Promise<Blob> {
    try {
      const request: PagingRequest = {
        PageNumber: 1,
        PageSize: 10000,
        FilterCriterias: searchTerm
          ? [{ Name: "Name", Value: searchTerm, Type: 1 }]
          : [],
        SortCriterias: [],
        IsCountRequired: true,
      };

      const response = await this.getAllDistributors(request);
      const distributors = response.PagedData || [];

      const headers = [
        "Code",
        "Name",
        "Sequence Code",
        "Contact Person",
        "Contact Number",
        "Status",
        "Open Account Date",
      ];

      const csvContent = [
        headers.join(","),
        ...distributors.map((d) =>
          [
            `"${d.Code || ""}"`,
            `"${d.Name || ""}"`,
            `"${d.SequenceCode || ""}"`,
            `"${d.ContactPerson || ""}"`,
            `"${d.ContactNumber || ""}"`,
            `"${d.Status || ""}"`,
            `"${
              d.OpenAccountDate
                ? new Date(d.OpenAccountDate).toLocaleDateString()
                : ""
            }"`,
          ].join(",")
        ),
      ].join("\n");

      return new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
    } catch (error) {
      console.error("Failed to export distributors:", error);
      throw new Error("Failed to export distributors");
    }
  }

  /**
   * Export distributors to Excel
   */
  async exportToExcel(searchTerm?: string): Promise<Blob> {
    const csvContent = await this.exportToCSV(searchTerm);
    return new Blob([csvContent], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=utf-8;",
    });
  }
}

export const distributorService = new DistributorService();
