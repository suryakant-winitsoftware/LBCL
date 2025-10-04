import * as React from "react"
import { cva, type VariantProps } from "class-variance-authority"
import { Loader2 } from "lucide-react"
import { cn } from "@/lib/utils"

const loaderVariants = cva(
  "flex items-center justify-center",
  {
    variants: {
      variant: {
        default: "text-blue-600 dark:text-blue-400",
        primary: "text-gray-900 dark:text-gray-100",
        secondary: "text-gray-600 dark:text-gray-400",
        destructive: "text-destructive",
        outline: "text-foreground",
        ghost: "text-accent-foreground",
        link: "text-primary",
        white: "text-white",
      },
      size: {
        default: "h-8 w-8",
        sm: "h-4 w-4",
        md: "h-6 w-6",
        lg: "h-12 w-12",
        xl: "h-16 w-16",
      },
      type: {
        page: "min-h-screen",
        inline: "",
        button: "",
        card: "py-8",
      },
    },
    defaultVariants: {
      variant: "default",
      size: "default",
      type: "inline",
    },
  }
)

export interface LoaderProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof loaderVariants> {
  text?: string
  showText?: boolean
  showBackground?: boolean
}

const Loader = React.forwardRef<HTMLDivElement, LoaderProps>(
  ({ className, variant, size, type, text, showText = true, showBackground = false, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn(loaderVariants({ type }), className)}
        {...props}
      >
        <div className="relative text-center">
          {/* Main loader container */}
          <div className="relative">
            {/* Outer spinning ring for larger loaders */}
            {(size === "lg" || size === "xl" || type === "page") && (
              <div className="absolute inset-0 flex items-center justify-center">
                <div className={cn(
                  "rounded-full border-2 border-transparent",
                  "border-t-current border-r-current animate-spin",
                  size === "lg" && "h-12 w-12",
                  size === "xl" && "h-16 w-16",
                  type === "page" && size !== "sm" && size !== "md" && "h-12 w-12",
                  "opacity-20"
                )} />
              </div>
            )}
            
            {/* Main spinning icon */}
            <Loader2 
              className={cn(
                "animate-spin mx-auto relative z-10",
                loaderVariants({ variant, size }),
                showText && text && "mb-3",
                // Professional smooth spin animation
                "animate-[spin_0.75s_cubic-bezier(0.4,0,0.6,1)_infinite]"
              )} 
            />
          </div>
          
          {/* Loading text with animation */}
          {showText && text && (
            <div className="space-y-2">
              <p className={cn(
                "font-medium",
                size === "sm" && "text-xs",
                size === "md" && "text-sm",
                size === "default" && "text-sm",
                size === "lg" && "text-base",
                size === "xl" && "text-lg",
                variant === "white" ? "text-white" : "text-gray-700 dark:text-gray-300"
              )}>
                {text}
              </p>
              
              {/* Loading dots animation for page loader */}
              {type === "page" && (
                <div className="flex justify-center space-x-1">
                  <div className="w-1.5 h-1.5 bg-current rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                  <div className="w-1.5 h-1.5 bg-current rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                  <div className="w-1.5 h-1.5 bg-current rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    )
  }
)
Loader.displayName = "Loader"

// Professional page loader with backdrop
export const PageLoader = React.forwardRef<
  HTMLDivElement,
  Omit<LoaderProps, "type">
>(({ text = "Loading...", className, variant = "primary", ...props }, ref) => (
  <div className={cn("fixed inset-0 z-50 flex items-center justify-center", className)}>
    {/* Professional backdrop */}
    <div className="absolute inset-0 bg-white/95 dark:bg-gray-950/95 backdrop-blur-sm" />
    
    {/* Loader content without card */}
    <div className="relative">
      <Loader 
        ref={ref} 
        type="page" 
        text={text} 
        size="lg"
        variant={variant}
        showBackground={false}
        {...props} 
      />
    </div>
  </div>
))
PageLoader.displayName = "PageLoader"

// Compact button loader
export const ButtonLoader = React.forwardRef<
  HTMLDivElement,
  Omit<LoaderProps, "type" | "size">
>(({ showText = false, className, ...props }, ref) => (
  <Loader 
    ref={ref} 
    type="button" 
    size="sm" 
    showText={showText} 
    className={cn("inline-flex", className)}
    {...props} 
  />
))
ButtonLoader.displayName = "ButtonLoader"

// Card content loader
export const CardLoader = React.forwardRef<
  HTMLDivElement,
  Omit<LoaderProps, "type">
>(({ text = "Loading data...", ...props }, ref) => (
  <Loader ref={ref} type="card" text={text} {...props} />
))
CardLoader.displayName = "CardLoader"

// Modern skeleton loader with shimmer effect
export const SkeletonLoader = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement>
>(({ className, ...props }, ref) => {
  return (
    <div
      ref={ref}
      className={cn(
        "relative overflow-hidden rounded-md bg-gray-200 dark:bg-gray-800",
        "before:absolute before:inset-0",
        "before:-translate-x-full",
        "before:animate-[shimmer_2s_infinite]",
        "before:bg-gradient-to-r",
        "before:from-transparent before:via-white/60 dark:before:via-white/10 before:to-transparent",
        className
      )}
      {...props}
    />
  )
})
SkeletonLoader.displayName = "SkeletonLoader"

// Inline text loader for loading states in text
export const InlineLoader = React.forwardRef<
  HTMLDivElement,
  Omit<LoaderProps, "type" | "size">
>(({ className, ...props }, ref) => (
  <Loader 
    ref={ref} 
    type="inline" 
    size="sm" 
    showText={false}
    className={cn("inline-flex mx-1", className)}
    {...props} 
  />
))
InlineLoader.displayName = "InlineLoader"

export { Loader, loaderVariants }