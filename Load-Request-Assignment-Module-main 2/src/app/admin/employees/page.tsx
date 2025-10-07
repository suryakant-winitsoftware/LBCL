"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"

export default function EmployeesPage() {
  const router = useRouter()
  
  useEffect(() => {
    router.replace("/updatedfeatures/employee-management/employees/manage")
  }, [router])
  
  return (
    <div className="flex items-center justify-center min-h-screen">
      <p>Redirecting...</p>
    </div>
  )
}