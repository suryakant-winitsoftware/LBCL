"use client"

import { usePathname } from "next/navigation"
import { ReactNode, useEffect, useState } from "react"
import { MainLayout } from "./main-layout"

interface AutoLayoutProps {
  children: ReactNode
}

// Pages that don't need the sidebar/header layout
const PUBLIC_ROUTES = [
  "/login",
  "/auth/forgot-password",
  "/lbcl", // LBCL delivery routes have their own layout
  // Add other auth-related routes here
]

// Check if current path should have layout
function shouldHaveLayout(pathname: string, isNotFoundPage: boolean): boolean {
  // Exclude 404 not-found page
  if (isNotFoundPage) {
    return false
  }

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
  const [isNotFoundPage, setIsNotFoundPage] = useState(false)

  // Check if body has the not-found marker
  useEffect(() => {
    const checkNotFound = () => {
      setIsNotFoundPage(document.body.hasAttribute("data-not-found"))
    }

    checkNotFound()

    // Use MutationObserver to detect when the attribute is added
    const observer = new MutationObserver(checkNotFound)
    observer.observe(document.body, { attributes: true, attributeFilter: ["data-not-found"] })

    return () => observer.disconnect()
  }, [])

  if (shouldHaveLayout(pathname, isNotFoundPage)) {
    return <MainLayout>{children}</MainLayout>
  }

  return <>{children}</>
}