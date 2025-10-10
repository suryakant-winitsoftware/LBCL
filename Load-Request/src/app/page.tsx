"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuth } from "@/providers/auth-provider"
import { SkeletonLoader } from "@/components/ui/loader"

export default function HomePage() {
  const router = useRouter()
  const { isAuthenticated, isLoading } = useAuth()
  
  useEffect(() => {
    if (!isLoading) {
      if (isAuthenticated) {
        router.push("/dashboard")
      } else {
        router.push("/login")
      }
    }
  }, [isAuthenticated, isLoading, router])
  
  return (
    <div className="min-h-screen bg-gradient-to-b from-blue-50 to-white dark:from-gray-900 dark:to-gray-800 flex items-center justify-center">
      <div className="text-center space-y-6 p-8">
        <div className="space-y-4">
          <SkeletonLoader className="h-12 w-64 rounded-lg mx-auto" />
          <SkeletonLoader className="h-6 w-48 rounded mx-auto" />
        </div>
        <div className="space-y-3">
          <SkeletonLoader className="h-4 w-32 rounded mx-auto" />
          <SkeletonLoader className="h-10 w-40 rounded-lg mx-auto" />
        </div>
      </div>
    </div>
  )
}
