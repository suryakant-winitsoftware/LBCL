"use client"

import { useEffect, useState } from "react"
import { usePathname } from "next/navigation"
import { cn } from "@/lib/utils"

interface NavigationLoaderProps {
  className?: string
}

export function NavigationLoader({ className }: NavigationLoaderProps) {
  const [isLoading, setIsLoading] = useState(false)
  const [progress, setProgress] = useState(0)
  const pathname = usePathname()

  useEffect(() => {
    let progressTimer: NodeJS.Timeout
    let hideTimer: NodeJS.Timeout

    const handleStart = () => {
      setIsLoading(true)
      setProgress(0)

      // Simulate progress
      progressTimer = setInterval(() => {
        setProgress(prev => {
          if (prev >= 90) return prev
          return prev + Math.random() * 10
        })
      }, 100)
    }

    const handleComplete = () => {
      setProgress(100)
      hideTimer = setTimeout(() => {
        setIsLoading(false)
        setProgress(0)
      }, 200)
      clearInterval(progressTimer)
    }

    // Start loading on pathname change
    handleStart()
    
    // Complete loading after a short delay to simulate navigation
    const completeTimer = setTimeout(handleComplete, 300)

    return () => {
      clearInterval(progressTimer)
      clearTimeout(hideTimer)
      clearTimeout(completeTimer)
    }
  }, [pathname])

  if (!isLoading) return null

  return (
    <div
      className={cn(
        "fixed top-0 left-0 right-0 z-50 h-1 bg-gray-100",
        className
      )}
    >
      <div
        className="h-full bg-blue-600 transition-all duration-300 ease-out"
        style={{ width: `${progress}%` }}
      />
    </div>
  )
}

export function PageLoader() {
  const [isLoading, setIsLoading] = useState(false)
  const pathname = usePathname()

  useEffect(() => {
    setIsLoading(true)
    
    const timer = setTimeout(() => {
      setIsLoading(false)
    }, 100)

    return () => clearTimeout(timer)
  }, [pathname])

  if (!isLoading) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-white/80 backdrop-blur-sm">
      <div className="flex flex-col items-center space-y-3">
        {/* Simple spinning loader */}
        <div className="h-8 w-8 animate-spin rounded-full border-4 border-gray-200 border-t-blue-600" />
        <p className="text-sm font-medium text-gray-600">Loading...</p>
      </div>
    </div>
  )
}

export function LoadingSpinner({ size = "default", className }: { 
  size?: "sm" | "default" | "lg"
  className?: string 
}) {
  const sizeClasses = {
    sm: "h-4 w-4 border-2",
    default: "h-6 w-6 border-2", 
    lg: "h-8 w-8 border-4"
  }

  return (
    <div 
      className={cn(
        "animate-spin rounded-full border-gray-200 border-t-blue-600",
        sizeClasses[size],
        className
      )} 
    />
  )
}