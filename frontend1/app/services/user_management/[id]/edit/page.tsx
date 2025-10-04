"use client"

import { use } from "react"
import { useRouter } from "next/navigation"
import { ArrowLeft } from "lucide-react"
import { Button } from "../../../../../components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../../../components/ui/card"
import { RouteUserForm } from "../../../../../components/admin/employees/route-user-form"
import { useToast } from "../../../../../components/ui/use-toast"

interface EditEmployeePageProps {
  params: Promise<{
    id: string
  }>
}

export default function EditEmployeePage({ params }: EditEmployeePageProps) {
  const { id } = use(params)
  const router = useRouter()
  const { toast } = useToast()

  const handleSuccess = () => {
    toast({
      title: "Success",
      description: "Employee updated successfully!",
    })
    router.push("/user/admin/dashboard")
  }

  const handleCancel = () => {
    router.push(`/services/user_management/${id}`)
  }

  const handleBack = () => {
    router.push("/user/admin/dashboard")
  }

  return (
    <div style={{ padding: '24px', maxWidth: '800px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <div>
          <h1 style={{ fontSize: '24px', fontWeight: 'bold', color: '#000', marginBottom: '8px' }}>Edit Employee</h1>
          <p style={{ color: '#666' }}>
            Update employee information and settings
          </p>
        </div>
        <Button variant="outline" onClick={handleBack}>
          <ArrowLeft style={{ width: '16px', height: '16px', marginRight: '8px' }} />
          Back to Dashboard
        </Button>
      </div>

      {/* Edit Form */}
      <Card>
        <CardHeader>
          <CardTitle>Employee Information</CardTitle>
          <CardDescription>
            Update the employee details below. All changes will be saved when you click Update.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <RouteUserForm
            employeeId={id}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
            isModal={false}
          />
        </CardContent>
      </Card>
    </div>
  )
}