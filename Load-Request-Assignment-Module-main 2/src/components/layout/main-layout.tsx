"use client"

import { ReactNode, useEffect } from "react"
import { useAuth } from "@/providers/auth-provider"
import { useRouter } from "next/navigation"
import { SidebarProvider, SidebarInset } from "@/components/ui/sidebar"
import { TooltipProvider } from "@/components/ui/tooltip"
import { ProfessionalSidebar } from "./professional-sidebar"
import { Header } from "./header"
import { SkeletonLoader } from "@/components/ui/loader"

interface MainLayoutProps {
  children: ReactNode
}

export function MainLayout({ children }: MainLayoutProps) {
  const { isAuthenticated, isLoading } = useAuth()
  const router = useRouter()

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login")
    }
  }, [isAuthenticated, isLoading, router])

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex">
        {/* Sidebar skeleton */}
        <div className="w-64 bg-white dark:bg-gray-950 border-r border-gray-200 dark:border-gray-800 p-4">
          <div className="space-y-4">
            <SkeletonLoader className="h-10 w-full rounded-lg" />
            <div className="space-y-2">
              {Array.from({ length: 6 }).map((_, i) => (
                <SkeletonLoader key={i} className="h-10 w-full rounded" />
              ))}
            </div>
          </div>
        </div>
        
        {/* Main content skeleton */}
        <div className="flex-1 p-6 space-y-6">
          <SkeletonLoader className="h-16 w-full rounded-lg" />
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <SkeletonLoader className="h-64 w-full rounded-lg" />
            <SkeletonLoader className="h-64 w-full rounded-lg" />
          </div>
        </div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen bg-gray-50 dark:bg-gray-900 flex items-center justify-center">
        <div className="text-center space-y-4">
          <SkeletonLoader className="h-8 w-48 rounded mx-auto" />
          <SkeletonLoader className="h-4 w-32 rounded mx-auto" />
        </div>
      </div>
    )
  }

  return (
    <TooltipProvider>
      <SidebarProvider defaultOpen={true}>
        <ProfessionalSidebar />
        <SidebarInset>
          <Header />
          <main className="flex flex-1 flex-col gap-4 p-4 pt-6">
            {children}
          </main>
        </SidebarInset>
      </SidebarProvider>
    </TooltipProvider>
  )
}