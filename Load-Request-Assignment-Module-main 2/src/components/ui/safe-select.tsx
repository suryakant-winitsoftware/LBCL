import React from 'react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

interface SafeSelectProps {
  value?: string;
  onValueChange?: (value: string) => void;
  disabled?: boolean;
  children: React.ReactNode;
  placeholder?: string;
  className?: string;
}

interface SafeSelectItemProps {
  value: string;
  children: React.ReactNode;
  disabled?: boolean;
  className?: string;
}

// Safe Select wrapper that prevents empty string values
export function SafeSelect({ 
  value, 
  onValueChange, 
  disabled, 
  children, 
  placeholder,
  className 
}: SafeSelectProps) {
  // Filter out any SelectItem children with empty values
  const safeChildren = React.Children.map(children, (child) => {
    if (React.isValidElement(child) && child.type === SafeSelectItem) {
      const props = child.props as SafeSelectItemProps;
      // Only render if value is not empty
      if (props.value && props.value.trim() !== '') {
        return child;
      }
      return null;
    }
    return child;
  });

  return (
    <Select
      value={value}
      onValueChange={onValueChange}
      disabled={disabled}
    >
      <SelectTrigger className={className}>
        <SelectValue placeholder={placeholder} />
      </SelectTrigger>
      <SelectContent>
        {safeChildren}
      </SelectContent>
    </Select>
  );
}

// Safe SelectItem wrapper that validates values
export function SafeSelectItem({ 
  value, 
  children, 
  disabled, 
  className 
}: SafeSelectItemProps) {
  // Don't render if value is empty, null, or undefined
  if (!value || value.trim() === '') {
    console.warn('SafeSelectItem: Attempted to render with empty value, skipping');
    return null;
  }

  return (
    <SelectItem value={value} disabled={disabled} className={className}>
      {children}
    </SelectItem>
  );
}

// Utility function to clean options for Select components
export function cleanSelectOptions(options: any[]): string[] {
  return options
    .filter(option => option !== null && option !== undefined)
    .map(option => String(option).trim())
    .filter(option => option.length > 0);
}