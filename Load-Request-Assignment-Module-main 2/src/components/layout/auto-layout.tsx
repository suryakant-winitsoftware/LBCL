"use client"

import { usePathname } from "next/navigation"
import { ReactNode } from "react"
import { MainLayout } from "./main-layout"

interface AutoLayoutProps {
  children: ReactNode
}

// Pages that don't need the sidebar/header layout
const PUBLIC_ROUTES = [
  "/login",
  "/auth/forgot-password",
  // Add other auth-related routes here
]

// Check if current path should have layout
function shouldHaveLayout(pathname: string): boolean {
  // Exclude public routes
  if (PUBLIC_ROUTES.some(route => pathname.startsWith(route))) {
    return false
  }
  
  // Exclude root page (it redirects anyway)
  if (pathname === "/") {
    return false
  }
  
  // All other pages should have layout
  return true
}

export function AutoLayout({ children }: AutoLayoutProps) {
  const pathname = usePathname()
  
  if (shouldHaveLayout(pathname)) {
    return <MainLayout>{children}</MainLayout>
  }
  
  return <>{children}</>
}