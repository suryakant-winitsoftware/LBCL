import { FilterCriteria, SortCriteria } from '../constants/filterConstants';

export const PAGINATION_CONFIG = {
  DEFAULT_PAGE_SIZE: 50,
  MAX_PAGE_SIZE: 1000,
  MIN_PAGE_SIZE: 10,
  DEFAULT_PAGE_NUMBER: 1
};

export interface PagingRequest {
  pageNumber: number;
  pageSize: number;
  sortCriterias: SortCriteria[];
  filterCriterias: FilterCriteria[];
  isCountRequired: boolean;
}

export const createPagingRequest = (
  pageNumber = PAGINATION_CONFIG.DEFAULT_PAGE_NUMBER,
  pageSize = PAGINATION_CONFIG.DEFAULT_PAGE_SIZE,
  filters: FilterCriteria[] = [],
  sorts: SortCriteria[] = [],
  isCountRequired = true
): PagingRequest => {
  return {
    pageNumber,
    pageSize,
    sortCriterias: sorts,
    filterCriterias: filters,
    isCountRequired
  };
};