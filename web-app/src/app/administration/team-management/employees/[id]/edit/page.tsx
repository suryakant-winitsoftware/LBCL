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
    router.push("/administration/team-management/employees")
  }

  const handleCancel = () => {
    router.push(`/administration/team-management/employees/${id}`)
  }

  const handleBack = () => {
    router.push("/administration/team-management/employees")
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">Edit Employee</h1>
          <p className="text-muted-foreground text-base">
            Update employee information and settings
          </p>
        </div>
        <Button variant="outline" onClick={handleBack} className="flex-shrink-0">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to List
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