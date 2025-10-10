"use client";

import * as React from "react";
import { ChevronsUpDown, X, Search } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Badge } from "@/components/ui/badge";

export interface MultiSelectOption {
  value: string;
  label: string;
  code?: string;
  isAssigned?: boolean;
}

interface MultiSelectProps {
  options: MultiSelectOption[];
  selected: string[];
  onChange: (values: string[]) => void;
  placeholder?: string;
  disabled?: boolean;
  className?: string;
  maxItems?: number;
  hideSelectedInButton?: boolean;
}

export function MultiSelect({
  options,
  selected,
  onChange,
  placeholder = "Select items...",
  disabled = false,
  className,
  maxItems,
  hideSelectedInButton = false,
}: MultiSelectProps) {
  const [open, setOpen] = React.useState(false);
  const [searchValue, setSearchValue] = React.useState("");
  const triggerRef = React.useRef<HTMLButtonElement>(null);

  // Ensure selected is always an array
  const selectedArray = React.useMemo(() => {
    if (!selected) return [];
    return Array.isArray(selected) ? selected : [selected];
  }, [selected]);

  const selectedOptions = React.useMemo(() => 
    options.filter((option) => selectedArray.includes(option.value)),
    [options, selectedArray]
  );

  const filteredOptions = React.useMemo(() => {
    if (!searchValue) return options;
    const searchLower = searchValue.toLowerCase();
    return options.filter(
      (option) =>
        option.label.toLowerCase().includes(searchLower) ||
        option.value.toLowerCase().includes(searchLower) ||
        (option.code && option.code.toLowerCase().includes(searchLower))
    );
  }, [options, searchValue]);

  const handleToggle = React.useCallback((value: string) => {
    const currentSelected = [...selectedArray];
    const index = currentSelected.indexOf(value);
    
    if (index > -1) {
      // Item is selected, remove it
      currentSelected.splice(index, 1);
    } else {
      // Item is not selected, add it
      if (!maxItems || currentSelected.length < maxItems) {
        currentSelected.push(value);
      }
    }
    
    onChange(currentSelected);
  }, [selectedArray, onChange, maxItems]);

  const handleSelectAll = React.useCallback(() => {
    const allValues = filteredOptions.map(o => o.value);
    const uniqueValues = Array.from(new Set([...selectedArray, ...allValues]));
    onChange(maxItems ? uniqueValues.slice(0, maxItems) : uniqueValues);
  }, [filteredOptions, selectedArray, onChange, maxItems]);

  const handleClearAll = React.useCallback(() => {
    const filteredValues = new Set(filteredOptions.map(o => o.value));
    const remaining = selectedArray.filter(s => !filteredValues.has(s));
    onChange(remaining);
  }, [filteredOptions, selectedArray, onChange]);

  const handleRemove = React.useCallback((value: string, e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(selectedArray.filter((item) => item !== value));
  }, [selectedArray, onChange]);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          ref={triggerRef}
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className={cn(
            "w-full justify-between",
            !selectedArray.length && "text-muted-foreground",
            className
          )}
          disabled={disabled}
        >
          <div className="flex flex-wrap gap-1 flex-1">
            {hideSelectedInButton || selectedArray.length === 0 ? (
              <span className={selectedArray.length > 0 ? "text-gray-900" : ""}>
                {selectedArray.length > 0 ? `${selectedArray.length} selected` : placeholder}
              </span>
            ) : (
              selectedOptions.map((option) => (
                <Badge
                  key={option.value}
                  variant="secondary"
                  className="mr-1"
                  onClick={(e) => handleRemove(option.value, e)}
                >
                  {option.code ? `${option.code} - ${option.label}` : option.label}
                  <X className="ml-1 h-3 w-3 cursor-pointer" />
                </Badge>
              ))
            )}
          </div>
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent 
        className="p-2" 
        align="start" 
        sideOffset={4}
        style={{ 
          width: Math.max(triggerRef.current?.offsetWidth || 300, 300),
          maxWidth: '400px',
          zIndex: 10000,
          pointerEvents: 'auto'
        }}
      >
        <div className="space-y-2">
          {/* Header with Close Button */}
          <div className="flex items-center justify-between pb-2 border-b">
            <span className="font-medium text-sm">Select Options</span>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setOpen(false)}
              className="h-8 w-8 p-0 hover:bg-gray-100"
            >
              <X className="h-4 w-4" />
            </Button>
          </div>
          
          {/* Search Input */}
          <div className="flex items-center border-b pb-2">
            <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
            <Input
              placeholder="Search..."
              value={searchValue}
              onChange={(e) => setSearchValue(e.target.value)}
              className="h-8 border-0 focus:ring-0 p-0 text-sm"
            />
            {searchValue && (
              <Button
                variant="ghost"
                size="sm"
                onClick={() => setSearchValue("")}
                className="h-6 w-6 p-0"
              >
                <X className="h-3 w-3" />
              </Button>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex gap-2 pb-2 border-b">
            <Button
              variant="outline"
              size="sm"
              onClick={handleSelectAll}
              className="text-xs h-7"
            >
              Select All ({filteredOptions.length})
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={handleClearAll}
              className="text-xs h-7"
            >
              Clear All
            </Button>
            {selectedArray.length > 0 && (
              <Badge variant="secondary" className="ml-auto">
                {selectedArray.length} selected
              </Badge>
            )}
          </div>

          {/* Options List with Checkboxes */}
          <ScrollArea className="h-[300px]">
            <div className="space-y-1 pr-3">
              {filteredOptions.length === 0 ? (
                <p className="text-sm text-muted-foreground text-center py-4">
                  No items found
                </p>
              ) : (
                filteredOptions.map((option) => {
                  const isSelected = selectedArray.includes(option.value);
                  
                  return (
                    <div
                      key={option.value}
                      className="flex items-center space-x-2 py-1.5 px-2 hover:bg-gray-50 rounded cursor-pointer"
                      onClick={() => handleToggle(option.value)}
                    >
                      <Checkbox
                        checked={isSelected}
                        onCheckedChange={() => handleToggle(option.value)}
                        onClick={(e) => e.stopPropagation()}
                      />
                      <span className="flex-1 text-sm">
                        {option.code ? `${option.code} - ${option.label}` : option.label}
                      </span>
                      {option.isAssigned && (
                        <Badge variant="outline" className="ml-2 text-xs">
                          Assigned
                        </Badge>
                      )}
                    </div>
                  );
                })
              )}
            </div>
          </ScrollArea>
        </div>
      </PopoverContent>
    </Popover>
  );
}