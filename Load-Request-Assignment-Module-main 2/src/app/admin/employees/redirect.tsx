"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"

export default function EmployeesRedirect() {
  const router = useRouter()
  
  useEffect(() => {
    router.replace("/updatedfeatures/employee-management/employees/manage")
  }, [router])
  
  return null
}