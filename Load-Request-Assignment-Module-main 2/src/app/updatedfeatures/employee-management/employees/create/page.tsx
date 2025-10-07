"use client";

import React from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useToast } from "@/components/ui/use-toast";
import { RouteUserForm } from "@/components/admin/employees/route-user-form";

export default function CreateEmployeePage() {
  const router = useRouter();
  const { toast } = useToast();

  const handleSuccess = () => {
    toast({
      title: "Success",
      description: "Employee created successfully!",
    });
    router.push("/updatedfeatures/employee-management/employees/manage");
  };

  const handleCancel = () => {
    router.push("/updatedfeatures/employee-management/employees/manage");
  };

  return (
    <div className="container mx-auto py-6 max-w-6xl">
      {/* Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-3">
            <Button
              variant="ghost"
              size="icon"
              onClick={handleCancel}
              className="hover:bg-gray-100"
            >
              <ArrowLeft className="h-5 w-5" />
            </Button>
            <div>
              <h1 className="text-2xl font-semibold text-gray-900">
                Create Route User
              </h1>
              <p className="text-sm text-gray-500 mt-1">
                Add a new route user to the system with location assignments
              </p>
            </div>
          </div>
        </div>
      </div>

      {/* Route User Form */}
      <Card>
        <CardContent className="pt-6">
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