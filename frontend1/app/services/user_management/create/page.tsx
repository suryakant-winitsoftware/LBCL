"use client";

import React from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { Button } from "../../../../components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../../../components/ui/card";
import { useToast } from "../../../../components/ui/use-toast";
import { RouteUserForm } from "../../../../components/admin/employees/route-user-form";

export default function CreateEmployeePage() {
  const router = useRouter();
  const { toast } = useToast();

  const handleSuccess = () => {
    toast({
      title: "Success",
      description: "Employee created successfully! They can now access the system.",
    });
    router.push("/user/admin/dashboard");
  };

  const handleCancel = () => {
    router.push("/user/admin/dashboard");
  };

  return (
    <div style={{ padding: '24px', maxWidth: '800px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <div>
          <h1 style={{ fontSize: '24px', fontWeight: 'bold', color: '#000', marginBottom: '8px' }}>Create Employee</h1>
          <p style={{ color: '#666' }}>
            Add a new team member with roles and permissions
          </p>
        </div>
        <Button variant="outline" onClick={handleCancel}>
          <ArrowLeft style={{ width: '16px', height: '16px', marginRight: '8px' }} />
          Back to Dashboard
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
