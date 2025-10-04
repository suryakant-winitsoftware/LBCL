'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/components/ui/use-toast';
import {
  ArrowLeft,
  Save,
  UserCheck,
  X,
} from 'lucide-react';
import enterpriseTaskService, { TaskType, SalesOrganization, EnterpriseTaskCreateRequest } from '@/services/enterpriseTaskService';

interface TaskFormData {
  taskType: string;
  taskSubType: string;
  salesOrg: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
}

export default function CreateEnterpriseTaskPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [availableSubTypes, setAvailableSubTypes] = useState<string[]>([]);
  const [taskTypes, setTaskTypes] = useState<TaskType[]>([]);
  const [salesOrganizations, setSalesOrganizations] = useState<SalesOrganization[]>([]);
  
  const [formData, setFormData] = useState<TaskFormData>({
    taskType: '',
    taskSubType: '',
    salesOrg: '',
    startDate: '',
    endDate: '',
    isActive: true,
  });

  const [errors, setErrors] = useState<Partial<TaskFormData>>({});

  useEffect(() => {
    // Set default start and end date to today
    const today = new Date().toISOString().split('T')[0];
    setFormData(prev => ({
      ...prev,
      startDate: today,
      endDate: today,
    }));
    
    // Load task types and sales organizations
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [taskTypesRes, salesOrgsRes] = await Promise.all([
        enterpriseTaskService.getTaskTypes(),
        enterpriseTaskService.getSalesOrganizations(),
      ]);
      
      if (taskTypesRes?.data) {
        setTaskTypes(taskTypesRes.data);
      }
      
      if (salesOrgsRes?.data) {
        setSalesOrganizations(salesOrgsRes.data);
      }
    } catch (error) {
      console.error('Error loading data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load form data',
        variant: 'destructive',
      });
    }
  };

  useEffect(() => {
    // Update sub types when task type changes
    if (formData.taskType) {
      const selectedType = taskTypes.find(t => t.id === formData.taskType);
      setAvailableSubTypes(selectedType?.subTypes || []);
      // Reset sub type selection
      setFormData(prev => ({ ...prev, taskSubType: '' }));
    } else {
      setAvailableSubTypes([]);
    }
  }, [formData.taskType]);

  const validateForm = (): boolean => {
    const newErrors: Partial<TaskFormData> = {};

    if (!formData.taskType) {
      newErrors.taskType = 'Task Type is required';
    }

    if (!formData.salesOrg) {
      newErrors.salesOrg = 'Sales Org is required';
    }

    if (!formData.startDate) {
      newErrors.startDate = 'Start Date is required';
    }

    if (!formData.endDate) {
      newErrors.endDate = 'End Date is required';
    }

    if (formData.startDate && formData.endDate && formData.startDate > formData.endDate) {
      newErrors.endDate = 'End Date must be after Start Date';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleInputChange = (field: keyof TaskFormData, value: string | boolean) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: undefined }));
    }
  };

  const handleSave = async () => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const request: EnterpriseTaskCreateRequest = {
        taskType: formData.taskType,
        taskSubType: formData.taskSubType || undefined,
        salesOrganization: formData.salesOrg,
        startDate: formData.startDate,
        endDate: formData.endDate,
        isActive: formData.isActive,
      };
      
      const response = await enterpriseTaskService.createTask(request);
      
      toast({
        title: 'Success',
        description: 'Task created successfully',
      });
      
      // Redirect back to task management
      router.push('/administration/configurations/task?message=Task created successfully');
    } catch (error: any) {
      console.error('Error creating task:', error);
      toast({
        title: 'Error',
        description: error.response?.data?.message || 'Failed to create task',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSaveAndAssign = async () => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      const request: EnterpriseTaskCreateRequest = {
        taskType: formData.taskType,
        taskSubType: formData.taskSubType || undefined,
        salesOrganization: formData.salesOrg,
        startDate: formData.startDate,
        endDate: formData.endDate,
        isActive: formData.isActive,
      };
      
      const response = await enterpriseTaskService.createTask(request);
      const taskId = response?.data?.TaskId || response?.TaskId;
      
      // Navigate to assign task page with task data
      router.push(`/administration/configurations/task/assign/${taskId}`);
    } catch (error: any) {
      console.error('Error creating task:', error);
      toast({
        title: 'Error',
        description: error.response?.data?.message || 'Failed to create task',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = () => {
    router.push('/administration/configurations/task');
  };

  const selectedTaskType = taskTypes.find(t => t.id === formData.taskType);
  const selectedSalesOrg = salesOrganizations.find(s => s.id === formData.salesOrg);

  return (
    <div className="container mx-auto py-6 max-w-4xl">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl font-bold text-blue-600">ADD / EDIT TASK</CardTitle>
          <CardDescription>
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <a href="/administration/configurations/task" className="hover:underline">Dashboard</a>
              <span>»</span>
              <a href="/administration/configurations/task" className="hover:underline">Manage Task</a>
              <span>»</span>
              <span>Add / Edit Task</span>
            </div>
            <div className="text-right mt-2 text-sm text-gray-500">
              (<span className="text-red-500">*</span>) indicates mandatory fields
            </div>
          </CardDescription>
        </CardHeader>

        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Left Column */}
            <div className="space-y-4">
              {/* Task Type */}
              <div className="space-y-2">
                <Label htmlFor="taskType" className="flex items-center gap-1">
                  <span className="text-red-500">*</span>
                  <strong>Task Type:</strong>
                </Label>
                <Select value={formData.taskType} onValueChange={(value) => handleInputChange('taskType', value)}>
                  <SelectTrigger id="taskType" className={errors.taskType ? 'border-red-500' : ''}>
                    <SelectValue placeholder="Select Category" />
                  </SelectTrigger>
                  <SelectContent>
                    {taskTypes.map((type) => (
                      <SelectItem key={type.id} value={type.id}>
                        {type.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.taskType && <p className="text-sm text-red-500">{errors.taskType}</p>}
              </div>

              {/* Sales Org */}
              <div className="space-y-2">
                <Label htmlFor="salesOrg" className="flex items-center gap-1">
                  <span className="text-red-500">*</span>
                  <strong>Sales Org:</strong>
                </Label>
                <Select value={formData.salesOrg} onValueChange={(value) => handleInputChange('salesOrg', value)}>
                  <SelectTrigger id="salesOrg" className={errors.salesOrg ? 'border-red-500' : ''}>
                    <SelectValue placeholder="--Select--" />
                  </SelectTrigger>
                  <SelectContent>
                    {salesOrganizations.map((org) => (
                      <SelectItem key={org.id} value={org.id}>
                        {org.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                {errors.salesOrg && <p className="text-sm text-red-500">{errors.salesOrg}</p>}
              </div>

              {/* End Date */}
              <div className="space-y-2">
                <Label htmlFor="endDate" className="flex items-center gap-1">
                  <span className="text-red-500">*</span>
                  <strong>End Date:</strong>
                </Label>
                <Input
                  id="endDate"
                  type="date"
                  value={formData.endDate}
                  onChange={(e) => handleInputChange('endDate', e.target.value)}
                  className={errors.endDate ? 'border-red-500' : ''}
                />
                {errors.endDate && <p className="text-sm text-red-500">{errors.endDate}</p>}
              </div>

              {/* Is Active */}
              <div className="flex items-center space-x-2 pt-4">
                <Checkbox
                  id="isActive"
                  checked={formData.isActive}
                  onCheckedChange={(checked) => handleInputChange('isActive', checked as boolean)}
                />
                <Label htmlFor="isActive" className="font-medium">
                  Is Active
                </Label>
              </div>
            </div>

            {/* Right Column */}
            <div className="space-y-4">
              {/* Task Sub Type */}
              <div className="space-y-2">
                <Label htmlFor="taskSubType">
                  <strong>Task Sub Type:</strong>
                </Label>
                <Select 
                  value={formData.taskSubType} 
                  onValueChange={(value) => handleInputChange('taskSubType', value)}
                  disabled={!formData.taskType}
                >
                  <SelectTrigger id="taskSubType">
                    <SelectValue placeholder={formData.taskType ? "Select Sub Type" : "Select Task Type first"} />
                  </SelectTrigger>
                  <SelectContent>
                    {availableSubTypes.map((subType) => (
                      <SelectItem key={subType} value={subType}>
                        {subType}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Start Date */}
              <div className="space-y-2">
                <Label htmlFor="startDate" className="flex items-center gap-1">
                  <span className="text-red-500">*</span>
                  <strong>Start Date:</strong>
                </Label>
                <Input
                  id="startDate"
                  type="date"
                  value={formData.startDate}
                  onChange={(e) => handleInputChange('startDate', e.target.value)}
                  className={errors.startDate ? 'border-red-500' : ''}
                />
                {errors.startDate && <p className="text-sm text-red-500">{errors.startDate}</p>}
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-center gap-3 mt-8 pt-6 border-t">
            <Button
              onClick={handleSave}
              disabled={loading}
              className="bg-green-600 hover:bg-green-700 text-white px-6"
            >
              <Save className="mr-2 h-4 w-4" />
              {loading ? 'Saving...' : 'Save'}
            </Button>
            <Button
              onClick={handleSaveAndAssign}
              disabled={loading}
              className="bg-green-600 hover:bg-green-700 text-white px-6"
            >
              <UserCheck className="mr-2 h-4 w-4" />
              Save & Continue to Assign
            </Button>
            <Button
              onClick={handleCancel}
              variant="secondary"
              disabled={loading}
              className="px-6"
            >
              <X className="mr-2 h-4 w-4" />
              Cancel
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}