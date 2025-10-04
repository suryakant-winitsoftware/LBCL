"use client"

import { createContext, useContext, useState, useEffect, ReactNode } from "react"
import { useRouter, usePathname } from "next/navigation"

interface LoadingContextType {
  isLoading: boolean
  setLoading: (loading: boolean) => void
  progress: number
}

const LoadingContext = createContext<LoadingContextType>({
  isLoading: false,
  setLoading: () => {},
  progress: 0
})

export function useLoading() {
  const context = useContext(LoadingContext)
  if (!context) {
    throw new Error("useLoading must be used within a LoadingProvider")
  }
  return context
}

interface LoadingProviderProps {
  children: ReactNode
}

export function LoadingProvider({ children }: LoadingProviderProps) {
  const [isLoading, setIsLoading] = useState(false)
  const [progress, setProgress] = useState(0)
  const pathname = usePathname()
  const router = useRouter()

  const setLoading = (loading: boolean) => {
    setIsLoading(loading)
    if (!loading) {
      setProgress(0)
    }
  }

  // Handle route changes
  useEffect(() => {
    let progressTimer: NodeJS.Timeout
    let hideTimer: NodeJS.Timeout

    if (isLoading) {
      setProgress(0)
      
      // Simulate progress
      progressTimer = setInterval(() => {
        setProgress(prev => {
          if (prev >= 90) return prev
          return prev + Math.random() * 15
        })
      }, 150)

      // Auto-complete after reasonable time
      hideTimer = setTimeout(() => {
        setProgress(100)
        setTimeout(() => setLoading(false), 200)
      }, 1000)
    }

    return () => {
      clearInterval(progressTimer)
      clearTimeout(hideTimer)
    }
  }, [isLoading, setLoading])

  const value = {
    isLoading,
    setLoading,
    progress
  }

  return (
    <LoadingContext.Provider value={value}>
      {children}
      {/* Global loading indicator */}
      {isLoading && (
        <div className="fixed top-0 left-0 right-0 z-50 h-1 bg-gray-100">
          <div
            className="h-full bg-blue-600 transition-all duration-300 ease-out"
            style={{ width: `${progress}%` }}
          />
        </div>
      )}
    </LoadingContext.Provider>
  )
}