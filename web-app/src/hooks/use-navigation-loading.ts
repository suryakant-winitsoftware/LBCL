"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useLoading } from "@/components/providers/loading-provider"

export function useNavigationLoading() {
  const { setLoading } = useLoading()
  const router = useRouter()

  const navigateWithLoading = (url: string) => {
    setLoading(true)
    router.push(url)
  }

  const replaceWithLoading = (url: string) => {
    setLoading(true)
    router.replace(url)
  }

  return {
    navigateWithLoading,
    replaceWithLoading,
    setLoading
  }
}

export function useRouteChangeLoading() {
  const { setLoading } = useLoading()

  useEffect(() => {
    const handleStart = () => setLoading(true)
    const handleComplete = () => setLoading(false)

    // Listen for browser navigation events
    window.addEventListener("beforeunload", handleStart)
    window.addEventListener("load", handleComplete)

    return () => {
      window.removeEventListener("beforeunload", handleStart)
      window.removeEventListener("load", handleComplete)
    }
  }, [setLoading])
}