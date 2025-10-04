// Common types used across the application

export interface ApiResponse<T = any> {
  success: boolean
  data?: T
  message?: string
  error?: string
}

export interface FilterCriteria {
  Name: string
  Value: any
  Type?: number  // FilterType: 0=Equal, 1=Like, etc.
  DataType?: string
  FilterGroup?: number
  FilterType?: number
}

export interface SortCriteria {
  SortParameter: string
  Direction: 'Asc' | 'Desc'
}

export interface PagingRequest {
  PageNumber: number
  PageSize: number
  SortCriterias: SortCriteria[]
  FilterCriterias: FilterCriteria[]
  IsCountRequired: boolean
}

export interface PagedResponse<T> {
  PagedData: T[]
  TotalCount: number
  PageNumber: number
  PageSize: number
  TotalPages: number
}