"use client"

import { use } from "react"
import { EmployeeDetailView } from "../../../../components/admin/employees/employee-detail-view"

interface EmployeeDetailPageProps {
  params: Promise<{
    id: string
  }>
}

export default function EmployeeDetailPage({ params }: EmployeeDetailPageProps) {
  const { id } = use(params)
  return <EmployeeDetailView employeeId={id} />
}