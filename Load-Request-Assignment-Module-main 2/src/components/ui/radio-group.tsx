"use client"

import * as React from "react"
import { cn } from "@/lib/utils"

interface RadioGroupProps extends React.HTMLAttributes<HTMLDivElement> {
  value?: string;
  onValueChange?: (value: string) => void;
  defaultValue?: string;
  disabled?: boolean;
  name?: string;
}

const RadioGroupContext = React.createContext<{
  value?: string;
  onValueChange?: (value: string) => void;
  name?: string;
}>({});

const RadioGroup = React.forwardRef<HTMLDivElement, RadioGroupProps>(
  ({ className, value, onValueChange, defaultValue, children, ...props }, ref) => {
    const [internalValue, setInternalValue] = React.useState(defaultValue);
    const actualValue = value !== undefined ? value : internalValue;
    const name = React.useId();

    const handleValueChange = React.useCallback((newValue: string) => {
      if (value === undefined) {
        setInternalValue(newValue);
      }
      onValueChange?.(newValue);
    }, [value, onValueChange]);

    return (
      <RadioGroupContext.Provider value={{ value: actualValue, onValueChange: handleValueChange, name }}>
        <div
          ref={ref}
          className={cn("grid gap-2", className)}
          {...props}
        >
          {children}
        </div>
      </RadioGroupContext.Provider>
    )
  }
)
RadioGroup.displayName = "RadioGroup"

interface RadioGroupItemProps extends React.InputHTMLAttributes<HTMLInputElement> {
  value: string;
}

const RadioGroupItem = React.forwardRef<HTMLInputElement, RadioGroupItemProps>(
  ({ className, value, id, ...props }, ref) => {
    const context = React.useContext(RadioGroupContext);
    const isChecked = context.value === value;
    const actualId = id || `${context.name}-${value}`;

    return (
      <div className="flex items-center">
        <input
          ref={ref}
          type="radio"
          id={actualId}
          name={context.name}
          value={value}
          checked={isChecked}
          onChange={() => context.onValueChange?.(value)}
          className={cn(
            "h-4 w-4 rounded-full border border-gray-300 text-purple-600 focus:ring-2 focus:ring-purple-500 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50",
            className
          )}
          {...props}
        />
      </div>
    )
  }
)
RadioGroupItem.displayName = "RadioGroupItem"

export { RadioGroup, RadioGroupItem }