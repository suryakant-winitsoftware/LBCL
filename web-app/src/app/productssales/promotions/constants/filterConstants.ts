// Filter Type Enums
export enum FilterType {
  Equal = 1,
  NotEqual = 2,
  GreaterThan = 3,
  LessThan = 4,
  GreaterThanOrEqual = 5,
  LessThanOrEqual = 6,
  Contains = 7,
  NotContains = 8,
  StartsWith = 9,
  EndsWith = 10,
  In = 11,
  NotIn = 12,
  Between = 13,
  IsNull = 14,
  IsNotNull = 15
}

// Filter Group Type
export enum FilterGroupType {
  Field = 0,
  Group = 1,
  Custom = 2
}

// Filter Mode
export enum FilterMode {
  And = 0,
  Or = 1
}

// Sort Direction
export enum SortDirection {
  Ascending = 0,
  Descending = 1
}

// Filter Criteria Interface
export interface FilterCriteria {
  Name: string;
  Value: any;
  Type: FilterType;
  DataType: string;
  FilterGroup: FilterGroupType;
  FilterMode: FilterMode;
}

// Sort Criteria Interface
export interface SortCriteria {
  FieldName: string;
  SortDirection: SortDirection;
}

// Paging Request Interface
export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias: SortCriteria[];
  filterCriterias: FilterCriteria[];
  isCountRequired: boolean;
}