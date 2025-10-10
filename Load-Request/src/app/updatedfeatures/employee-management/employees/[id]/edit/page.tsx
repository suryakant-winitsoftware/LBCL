"use client"

import { use } from "react"
import { useRouter } from "next/navigation"
import { ArrowLeft } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { RouteUserForm } from "@/components/admin/employees/route-user-form"
import { useToast } from "@/components/ui/use-toast"

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
    router.push(`/updatedfeatures/employee-management/employees/${id}`)
  }

  const handleCancel = () => {
    router.push(`/updatedfeatures/employee-management/employees/${id}`)
  }

  const handleBack = () => {
    router.push("/updatedfeatures/employee-management/employees/manage")
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div className="flex items-center gap-4">
          <Button variant="outline" onClick={handleBack}>
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to List
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Edit Employee</h1>
            <p className="text-muted-foreground">
              Update employee information and settings
            </p>
          </div>
        </div>
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