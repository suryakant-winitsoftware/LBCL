import React, { useState, useEffect, useMemo } from 'react';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { SearchBar } from './SearchBar';
import { FilterOptions, FilterConfig, FilterValue } from './FilterOptions';
import { RefreshCw, Download, Upload } from 'lucide-react';
import { cn } from '@/lib/utils';

interface TableWithFiltersProps<T> {
  data: T[];
  columns: any[];
  filters?: FilterConfig[];
  searchKeys?: (keyof T)[];
  searchPlaceholder?: string;
  title?: string;
  description?: string;
  className?: string;
  cardClassName?: string;
  showRefresh?: boolean;
  showExport?: boolean;
  showImport?: boolean;
  onRefresh?: () => void;
  onExport?: () => void;
  onImport?: () => void;
  renderTable: (filteredData: T[]) => React.ReactNode;
  customActions?: React.ReactNode;
  enableSearch?: boolean;
  enableFilters?: boolean;
  filterPosition?: 'top' | 'side';
  debounceSearchMs?: number;
}

export function TableWithFilters<T extends Record<string, any>>({
  data,
  columns,
  filters = [],
  searchKeys = [],
  searchPlaceholder = 'Search...',
  title,
  description,
  className,
  cardClassName,
  showRefresh = false,
  showExport = false,
  showImport = false,
  onRefresh,
  onExport,
  onImport,
  renderTable,
  customActions,
  enableSearch = true,
  enableFilters = true,
  filterPosition = 'top',
  debounceSearchMs = 300,
}: TableWithFiltersProps<T>) {
  const [searchTerm, setSearchTerm] = useState('');
  const [filterValues, setFilterValues] = useState<FilterValue>({});
  const [isLoading, setIsLoading] = useState(false);

  const handleRefresh = async () => {
    if (onRefresh) {
      setIsLoading(true);
      await onRefresh();
      setIsLoading(false);
    }
  };

  const handleResetFilters = () => {
    setSearchTerm('');
    setFilterValues({});
  };

  const filteredData = useMemo(() => {
    let result = [...data];

    // Apply search filter
    if (enableSearch && searchTerm && searchKeys.length > 0) {
      result = result.filter((item) =>
        searchKeys.some((key) => {
          const value = item[key];
          if (value === null || value === undefined) return false;
          return String(value).toLowerCase().includes(searchTerm.toLowerCase());
        })
      );
    }

    // Apply custom filters
    if (enableFilters && filters.length > 0) {
      Object.keys(filterValues).forEach((filterId) => {
        const filterValue = filterValues[filterId];
        if (filterValue === undefined || filterValue === '' || filterValue === null) return;

        const filter = filters.find(f => f.id === filterId);
        if (!filter) return;

        switch (filter.type) {
          case 'select':
          case 'status':
            if (filterValue !== 'all') {
              result = result.filter((item) => item[filterId] === filterValue);
            }
            break;

          case 'multiselect':
            if (Array.isArray(filterValue) && filterValue.length > 0) {
              result = result.filter((item) => filterValue.includes(item[filterId]));
            }
            break;

          case 'boolean':
            result = result.filter((item) => item[filterId] === filterValue);
            break;

          case 'date':
            if (filterValue) {
              const filterDate = new Date(filterValue).setHours(0, 0, 0, 0);
              result = result.filter((item) => {
                const itemDate = new Date(item[filterId]).setHours(0, 0, 0, 0);
                return itemDate === filterDate;
              });
            }
            break;

          case 'dateRange':
            if (filterValue?.start && filterValue?.end) {
              const startDate = new Date(filterValue.start).setHours(0, 0, 0, 0);
              const endDate = new Date(filterValue.end).setHours(23, 59, 59, 999);
              result = result.filter((item) => {
                const itemDate = new Date(item[filterId]).getTime();
                return itemDate >= startDate && itemDate <= endDate;
              });
            }
            break;
        }
      });
    }

    return result;
  }, [data, searchTerm, filterValues, searchKeys, filters, enableSearch, enableFilters]);

  const renderFiltersSection = () => (
    <div className="space-y-4">
      {enableSearch && (
        <div className="flex items-center gap-4">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder={searchPlaceholder}
            className="flex-1 max-w-md"
            debounceMs={debounceSearchMs}
          />
          {customActions}
        </div>
      )}
      
      {enableFilters && filters.length > 0 && (
        <FilterOptions
          filters={filters}
          values={filterValues}
          onChange={setFilterValues}
          onReset={handleResetFilters}
        />
      )}
    </div>
  );

  const renderActionButtons = () => (
    <div className="flex items-center gap-2">
      {showRefresh && (
        <Button
          variant="outline"
          size="sm"
          onClick={handleRefresh}
          disabled={isLoading}
        >
          <RefreshCw className={cn('h-4 w-4', isLoading && 'animate-spin')} />
          <span className="ml-2 hidden sm:inline">Refresh</span>
        </Button>
      )}
      {showImport && (
        <Button
          variant="outline"
          size="sm"
          onClick={onImport}
        >
          <Upload className="h-4 w-4" />
          <span className="ml-2 hidden sm:inline">Import</span>
        </Button>
      )}
      {showExport && (
        <Button
          variant="outline"
          size="sm"
          onClick={onExport}
        >
          <Download className="h-4 w-4" />
          <span className="ml-2 hidden sm:inline">Export</span>
        </Button>
      )}
    </div>
  );

  if (filterPosition === 'side') {
    return (
      <div className={cn('flex gap-6', className)}>
        <aside className="w-64 space-y-4">
          {enableSearch && (
            <Card>
              <CardHeader className="pb-3">
                <h3 className="text-sm font-medium">Search</h3>
              </CardHeader>
              <CardContent>
                <SearchBar
                  value={searchTerm}
                  onChange={setSearchTerm}
                  placeholder={searchPlaceholder}
                  className="w-full"
                  debounceMs={debounceSearchMs}
                />
              </CardContent>
            </Card>
          )}

          {enableFilters && filters.length > 0 && (
            <Card>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-medium">Filters</h3>
                  {Object.keys(filterValues).length > 0 && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={handleResetFilters}
                      className="h-auto p-0 text-xs"
                    >
                      Clear
                    </Button>
                  )}
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {filters.map((filter) => (
                    <div key={filter.id}>
                      <FilterOptions
                        filters={[filter]}
                        values={filterValues}
                        onChange={setFilterValues}
                        showActiveCount={false}
                      />
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          )}
        </aside>

        <div className="flex-1">
          <Card className={cardClassName}>
            {title && (
              <CardHeader>
                <div className="flex items-center justify-between">
                  <div>
                    <h2 className="text-2xl font-bold tracking-tight">{title}</h2>
                    {description && (
                      <p className="text-muted-foreground">{description}</p>
                    )}
                  </div>
                  <div className="flex items-center gap-2">
                    {customActions}
                    {renderActionButtons()}
                  </div>
                </div>
              </CardHeader>
            )}
            <CardContent className="p-0">
              {renderTable(filteredData)}
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <Card className={cn(cardClassName, className)}>
      {(title || description || enableSearch || enableFilters) && (
        <CardHeader>
          {title && (
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-2xl font-bold tracking-tight">{title}</h2>
                {description && (
                  <p className="text-muted-foreground">{description}</p>
                )}
              </div>
              {renderActionButtons()}
            </div>
          )}
          {(enableSearch || enableFilters) && (
            <>
              {title && <Separator className="mb-4" />}
              {renderFiltersSection()}
            </>
          )}
        </CardHeader>
      )}
      <CardContent className="p-0">
        <div className="flex items-center justify-between px-6 py-3 border-b">
          <p className="text-sm text-muted-foreground">
            Showing {filteredData.length} of {data.length} results
          </p>
        </div>
        {renderTable(filteredData)}
      </CardContent>
    </Card>
  );
}