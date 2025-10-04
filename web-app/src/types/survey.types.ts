/**
 * Survey Types
 * Type definitions for Survey management
 */

export interface ISurvey {
  Id?: number;
  UID?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  SS?: string;
  Code?: string;
  Description?: string;
  StartDate?: string;
  EndDate?: string;
  IsActive?: boolean;
  SurveyData?: string;
}

export interface ISurveyResponse {
  Id?: number;
  UID?: string;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  SS?: string;
  EmpUID?: string;
  LinkedItemUID?: string;
  LinkedItemType?: string;
  ActivityType?: string;
  ResponseData?: string;
  SurveyUID?: string;
}

export interface IActivityModule {
  Date?: string;
  OutletCode?: string;
  OutletName?: string;
  UserCode?: string;
  UserName?: string;
  Relativepath?: string;
  LinkedItemUID?: string;
  QuestionID?: string;
  QuestionLabel?: string;
  Answer?: string;
  LocationValue?: string;
  LocationCode?: string;
  LocationName?: string;
  Role?: string;
}

export interface IViewSurveyResponse {
  SurveyResponseUID?: string;
  CustomerCode?: string;
  CustomerName?: string;
  UserCode?: string;
  UserName?: string;
  CreatedDate?: string;
  CreatedTime?: string;
  CreatedDateTime?: Date;
  ActivityType?: string;
  SurveyName?: string;
  Status?: string;
  SurveyAge?: string;
  StatusExecutedorNot?: string;
}

export interface SurveyListRequest {
  pageNumber: number;
  pageSize: number;
  filterCriterias?: FilterCriteria[];
  sortCriterias?: SortCriteria[];
  isCountRequired: boolean;
}

export interface SurveyResponseListRequest {
  pageNumber: number;
  pageSize: number;
  filterCriterias?: FilterCriteria[];
  sortCriterias?: SortCriteria[];
  isCountRequired: boolean;
}

export interface ActivityModuleListRequest {
  pageNumber: number;
  pageSize: number;
  filterCriterias?: FilterCriteria[];
  sortCriterias?: SortCriteria[];
  isCountRequired: boolean;
}

export interface FilterCriteria {
  name: string;
  value: any;
  operator?: string;
}

export interface SortCriteria {
  sortParameter: string;
  direction: number; // 0 for ASC, 1 for DESC
}

export interface PagedResponse<T> {
  pagedData: T[];
  totalCount: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

export interface SurveySearchParams {
  searchText?: string;
  status?: string;
  dateFrom?: string;
  dateTo?: string;
}

export const SURVEY_STATUS = {
  ACTIVE: 'Active',
  INACTIVE: 'Inactive',
  DRAFT: 'Draft'
} as const;

export const ACTIVITY_TYPES = {
  SHOP_OBSERVATION: 'ShopObservation',
  RAISE_TICKET: 'RaiseTicket',
  OTHERS: 'Others'
} as const;

export const SURVEY_NAMES = {
  PRODUCT_DISPLAY: 'productdisplay',
  HYGIENE: 'hygiene',
  CUSTOMER_SERVICE: 'customer_service',
  BRANDING: 'branding',
  AMBIENCE: 'ambience'
} as const;