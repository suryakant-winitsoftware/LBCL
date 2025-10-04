"use client"

import { cn } from "@/lib/utils"
import { LoadingSpinner } from "./navigation-loader"

interface LoadingOverlayProps {
  isLoading?: boolean
  message?: string
  className?: string
  spinnerSize?: "sm" | "default" | "lg"
  blur?: boolean
}

export function LoadingOverlay({ 
  isLoading = false, 
  message = "Loading...",
  className,
  spinnerSize = "default",
  blur = true
}: LoadingOverlayProps) {
  if (!isLoading) return null

  return (
    <div 
      className={cn(
        "absolute inset-0 z-50 flex flex-col items-center justify-center bg-white/90",
        blur && "backdrop-blur-sm",
        className
      )}
    >
      <LoadingSpinner size={spinnerSize} />
      {message && (
        <p className="mt-3 text-sm font-medium text-gray-600">
          {message}
        </p>
      )}
    </div>
  )
}

export function FullPageLoader({ 
  message = "Loading...",
  show = true 
}: { 
  message?: string
  show?: boolean 
}) {
  if (!show) return null

  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center bg-white">
      <div className="flex flex-col items-center space-y-4">
        <LoadingSpinner size="lg" />
        <div className="text-center">
          <p className="text-lg font-semibold text-gray-800">{message}</p>
          <p className="text-sm text-gray-500 mt-1">Please wait...</p>
        </div>
      </div>
    </div>
  )
}

export function InlineLoader({ 
  size = "default",
  message,
  className 
}: {
  size?: "sm" | "default" | "lg"
  message?: string
  className?: string
}) {
  return (
    <div className={cn("flex items-center space-x-2", className)}>
      <LoadingSpinner size={size} />
      {message && (
        <span className="text-sm font-medium text-gray-600">
          {message}
        </span>
      )}
    </div>
  )
}