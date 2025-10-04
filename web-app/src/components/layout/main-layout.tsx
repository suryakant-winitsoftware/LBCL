"use client"

import { ReactNode, useEffect, useRef, useState, createContext, useContext } from "react"
import { useAuth } from "@/providers/auth-provider"
import { useRouter } from "next/navigation"
import { SidebarProvider, SidebarInset, useSidebar } from "@/components/ui/sidebar"
import { TooltipProvider } from "@/components/ui/tooltip"
import { ProfessionalSidebar } from "./professional-sidebar"
import { Header } from "./header"
import { SkeletonLoader } from "@/components/ui/loader"

interface MainLayoutProps {
  children: ReactNode
}

interface AutoCollapseContextType {
  autoCollapseEnabled: boolean
  setAutoCollapseEnabled: (enabled: boolean) => void
}

const AutoCollapseContext = createContext<AutoCollapseContextType>({
  autoCollapseEnabled: true,
  setAutoCollapseEnabled: () => {}
})

export const useAutoCollapse = () => useContext(AutoCollapseContext)

function AutoCollapseSidebarWrapper({ children }: { children: ReactNode }) {
  const { setOpen, state } = useSidebar()
  const { autoCollapseEnabled } = useAutoCollapse()
  const sidebarRef = useRef<HTMLDivElement>(null)
  const isHoveringRef = useRef(false)
  const isInteractingRef = useRef(false)

  useEffect(() => {
    // Only apply auto-collapse behavior if enabled
    if (!autoCollapseEnabled) return

    const handleMainContentClick = (e: MouseEvent) => {
      const target = e.target as HTMLElement
      
      // Check if click is outside sidebar
      if (sidebarRef.current && !sidebarRef.current.contains(target)) {
        // Only collapse if not hovering and not interacting
        if (!isHoveringRef.current && !isInteractingRef.current && state === "expanded") {
          setOpen(false)
        }
      }
    }

    const handleSidebarMouseEnter = () => {
      isHoveringRef.current = true
      if (state === "collapsed") {
        setOpen(true)
      }
    }

    const handleSidebarMouseLeave = () => {
      isHoveringRef.current = false
      // Auto-collapse after mouse leaves (with delay)
      setTimeout(() => {
        if (!isHoveringRef.current && !isInteractingRef.current && autoCollapseEnabled) {
          setOpen(false)
        }
      }, 300)
    }

    const handleSidebarInteractionStart = () => {
      isInteractingRef.current = true
    }

    const handleSidebarInteractionEnd = () => {
      isInteractingRef.current = false
    }

    // Add event listeners
    document.addEventListener("click", handleMainContentClick)
    
    const sidebarElement = sidebarRef.current
    if (sidebarElement) {
      sidebarElement.addEventListener("mouseenter", handleSidebarMouseEnter)
      sidebarElement.addEventListener("mouseleave", handleSidebarMouseLeave)
      sidebarElement.addEventListener("mousedown", handleSidebarInteractionStart)
      sidebarElement.addEventListener("mouseup", handleSidebarInteractionEnd)
    }

    return () => {
      document.removeEventListener("click", handleMainContentClick)
      if (sidebarElement) {
        sidebarElement.removeEventListener("mouseenter", handleSidebarMouseEnter)
        sidebarElement.removeEventListener("mouseleave", handleSidebarMouseLeave)
        sidebarElement.removeEventListener("mousedown", handleSidebarInteractionStart)
        sidebarElement.removeEventListener("mouseup", handleSidebarInteractionEnd)
      }
    }
  }, [setOpen, state, autoCollapseEnabled])

  return (
    <>
      <div ref={sidebarRef}>
        <ProfessionalSidebar />
      </div>
      <SidebarInset>
        <Header />
        <main className="flex flex-1 flex-col gap-4 p-4 pt-6">
          {children}
        </main>
      </SidebarInset>
    </>
  )
}

export function MainLayout({ children }: MainLayoutProps) {
  const { isAuthenticated, isLoading } = useAuth()
  const router = useRouter()
  const [autoCollapseEnabled, setAutoCollapseEnabled] = useState(false)

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
      <AutoCollapseContext.Provider value={{ autoCollapseEnabled, setAutoCollapseEnabled }}>
        <SidebarProvider defaultOpen={true}>
          <AutoCollapseSidebarWrapper>
            {children}
          </AutoCollapseSidebarWrapper>
        </SidebarProvider>
      </AutoCollapseContext.Provider>
    </TooltipProvider>
  )
}