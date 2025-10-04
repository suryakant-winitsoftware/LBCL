/**
 * Store Group Types
 * Defines all interfaces and types for Store Group management
 */

export interface IStoreGroup {
  Id?: number;
  UID: string;
  StoreGroupTypeUID: string;
  Code: string;
  Name: string;
  ParentUID?: string | null;
  ItemLevel: number;
  HasChild: boolean;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export interface IStoreGroupType {
  Id?: number;
  UID: string;
  Code: string;
  Name: string;
  Description?: string;
  ParentUID?: string;
  LevelNo?: number;
  CreatedBy?: string;
  CreatedTime?: string;
  ModifiedBy?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsActive?: boolean;
  IsSelected?: boolean;
}

export interface StoreGroupFormData {
  UID?: string;
  StoreGroupTypeUID: string;
  Code: string;
  Name: string;
  ParentUID?: string | null;
  ItemLevel?: number;
  Description?: string;
}

export interface StoreGroupListRequest {
  PageNumber: number;
  PageSize: number;
  FilterCriterias?: FilterCriteria[];
  SortCriterias?: SortCriteria[];
  IsCountRequired: boolean;
}

export interface FilterCriteria {
  PropertyName: string;
  Value: any;
  Operator: string;
  Name?: string;
}

export interface SortCriteria {
  PropertyName: string;
  SortOrder: 'ASC' | 'DESC';
}

export interface PagedResponse<T> {
  PagedData: T[];
  TotalCount: number;
}

export interface ApiResponse<T = any> {
  Data?: T;
  StatusCode?: number;
  IsSuccess?: boolean;
  Message?: string;
  Error?: string;
}

export interface StoreGroupSearchParams {
  searchTerm?: string;
  storeGroupTypeUID?: string;
  parentUID?: string;
  itemLevel?: number;
  hasChild?: boolean;
  isActive?: boolean;
}

export interface StoreGroupHierarchy {
  UID: string;
  Name: string;
  Code: string;
  Level: number;
  Children?: StoreGroupHierarchy[];
  ParentUID?: string | null;
  StoreGroupTypeUID: string;
}