"use client"

import { useEffect } from "react"
import { useRouter, useSearchParams } from "next/navigation"

export default function PermissionsPage() {
  const router = useRouter()
  const searchParams = useSearchParams()
  
  useEffect(() => {
    const roleParam = searchParams.get('role')
    const newUrl = roleParam 
      ? `/updatedfeatures/role-management/roles/permissions?role=${roleParam}`
      : "/updatedfeatures/role-management/roles/permissions"
    router.replace(newUrl)
  }, [router, searchParams])
  
  return (
    <div className="flex items-center justify-center min-h-screen">
      <p>Redirecting...</p>
    </div>
  )
}