import React from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Calendar } from '@/components/ui/calendar';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { CalendarIcon, Filter, X } from 'lucide-react';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';

export type FilterType = 
  | 'select' 
  | 'multiselect' 
  | 'date' 
  | 'dateRange' 
  | 'boolean'
  | 'status';

export interface FilterOption {
  label: string;
  value: string | number;
}

export interface FilterConfig {
  id: string;
  label: string;
  type: FilterType;
  options?: FilterOption[];
  placeholder?: string;
  icon?: React.ComponentType<{ className?: string }>;
}

export interface FilterValue {
  [key: string]: any;
}

interface FilterOptionsProps {
  filters: FilterConfig[];
  values: FilterValue;
  onChange: (values: FilterValue) => void;
  onReset?: () => void;
  className?: string;
  showActiveCount?: boolean;
}

export const FilterOptions: React.FC<FilterOptionsProps> = ({
  filters,
  values,
  onChange,
  onReset,
  className,
  showActiveCount = true,
}) => {
  const activeFiltersCount = Object.keys(values).filter(
    key => values[key] !== undefined && values[key] !== '' && values[key] !== null
  ).length;

  const handleFilterChange = (filterId: string, value: any) => {
    onChange({
      ...values,
      [filterId]: value,
    });
  };

  const handleRemoveFilter = (filterId: string) => {
    const newValues = { ...values };
    delete newValues[filterId];
    onChange(newValues);
  };

  const renderFilter = (filter: FilterConfig) => {
    const currentValue = values[filter.id];

    switch (filter.type) {
      case 'select':
        return (
          <Select
            value={currentValue || ''}
            onValueChange={(value) => handleFilterChange(filter.id, value)}
          >
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder={filter.placeholder || `Select ${filter.label}`} />
            </SelectTrigger>
            <SelectContent>
              {filter.options?.map((option) => (
                <SelectItem key={option.value} value={String(option.value)}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        );

      case 'multiselect':
        return (
          <Popover>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className="w-[200px] justify-start"
              >
                {currentValue && currentValue.length > 0 ? (
                  <>
                    <span className="truncate">
                      {currentValue.length} selected
                    </span>
                  </>
                ) : (
                  <span className="text-muted-foreground">
                    {filter.placeholder || `Select ${filter.label}`}
                  </span>
                )}
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-[200px] p-0">
              <div className="p-2 space-y-2 max-h-[300px] overflow-auto">
                {filter.options?.map((option) => {
                  const isChecked = currentValue?.includes(option.value);
                  return (
                    <div key={option.value} className="flex items-center space-x-2">
                      <Checkbox
                        id={`${filter.id}-${option.value}`}
                        checked={isChecked}
                        onCheckedChange={(checked) => {
                          const newValue = currentValue || [];
                          if (checked) {
                            handleFilterChange(filter.id, [...newValue, option.value]);
                          } else {
                            handleFilterChange(
                              filter.id,
                              newValue.filter((v: any) => v !== option.value)
                            );
                          }
                        }}
                      />
                      <Label
                        htmlFor={`${filter.id}-${option.value}`}
                        className="text-sm font-normal cursor-pointer"
                      >
                        {option.label}
                      </Label>
                    </div>
                  );
                })}
              </div>
            </PopoverContent>
          </Popover>
        );

      case 'date':
        return (
          <Popover>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className={cn(
                  'w-[180px] justify-start text-left font-normal',
                  !currentValue && 'text-muted-foreground'
                )}
              >
                <CalendarIcon className="mr-2 h-4 w-4" />
                {currentValue ? format(new Date(currentValue), 'PPP') : <span>{filter.placeholder || 'Pick a date'}</span>}
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0">
              <Calendar
                mode="single"
                selected={currentValue ? new Date(currentValue) : undefined}
                onSelect={(date) => handleFilterChange(filter.id, date?.toISOString())}
                initialFocus
              />
            </PopoverContent>
          </Popover>
        );

      case 'boolean':
        return (
          <div className="flex items-center space-x-2">
            <Switch
              id={filter.id}
              checked={currentValue || false}
              onCheckedChange={(checked) => handleFilterChange(filter.id, checked)}
            />
            <Label htmlFor={filter.id}>{filter.label}</Label>
          </div>
        );

      case 'status':
        return (
          <Select
            value={currentValue || ''}
            onValueChange={(value) => handleFilterChange(filter.id, value)}
          >
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder={filter.placeholder || 'All Status'} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
        );

      default:
        return null;
    }
  };

  return (
    <div className={cn('flex flex-wrap items-center gap-3', className)}>
      <div className="flex items-center gap-2">
        <Filter className="h-4 w-4 text-muted-foreground" />
        <span className="text-sm font-medium">Filters</span>
        {showActiveCount && activeFiltersCount > 0 && (
          <Badge variant="secondary" className="ml-1">
            {activeFiltersCount}
          </Badge>
        )}
      </div>

      {filters.map((filter) => (
        <div key={filter.id} className="flex items-center gap-2">
          {filter.icon && <filter.icon className="h-4 w-4 text-muted-foreground" />}
          {renderFilter(filter)}
          {values[filter.id] !== undefined && values[filter.id] !== '' && values[filter.id] !== null && (
            <Button
              variant="ghost"
              size="sm"
              className="h-6 w-6 p-0"
              onClick={() => handleRemoveFilter(filter.id)}
            >
              <X className="h-3 w-3" />
            </Button>
          )}
        </div>
      ))}

      {activeFiltersCount > 0 && onReset && (
        <Button
          variant="ghost"
          size="sm"
          onClick={onReset}
          className="text-destructive hover:text-destructive"
        >
          Clear all
        </Button>
      )}
    </div>
  );
};