"use client";

import React from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useToast } from "@/components/ui/use-toast";
import { RouteUserForm } from "@/components/admin/employees/route-user-form";

export default function CreateEmployeePage() {
  const router = useRouter();
  const { toast } = useToast();

  const handleSuccess = () => {
    toast({
      title: "Success",
      description: "Employee created successfully! They can now access the system.",
    });
    router.push("/administration/team-management/employees");
  };

  const handleCancel = () => {
    router.push("/administration/team-management/employees");
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="space-y-2">
          <h1 className="text-3xl font-bold tracking-tight text-gray-900">Create Employee</h1>
          <p className="text-muted-foreground text-base">
            Add a new team member with roles and permissions
          </p>
        </div>
        <Button variant="outline" onClick={handleCancel} className="flex-shrink-0">
          <ArrowLeft className="mr-2 h-4 w-4" />
          Back to List
        </Button>
      </div>

      {/* Create Form */}
      <Card>
        <CardHeader>
          <CardTitle>Employee Information</CardTitle>
          <CardDescription>
            Create a new employee profile. All required fields must be completed before saving.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <RouteUserForm
            user={null}
            onSuccess={handleSuccess}
            onCancel={handleCancel}
            isModal={false}
          />
        </CardContent>
      </Card>
    </div>
  );
}
