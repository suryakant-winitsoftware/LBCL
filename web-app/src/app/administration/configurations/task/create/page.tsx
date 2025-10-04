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
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Checkbox } from '@/components/ui/checkbox';
import { Switch } from '@/components/ui/switch';
import { useToast } from '@/components/ui/use-toast';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { format } from 'date-fns';
import { cn } from '@/lib/utils';
import {
  ArrowLeft,
  Save,
  CalendarIcon,
  Loader2,
  AlertCircle,
  CheckCircle2,
  ClipboardList,
  Building2,
} from 'lucide-react';
import taskService, { 
  TaskCreateRequest
} from '@/services/taskService';
import { organizationService } from '@/services/organizationService';
import { listItemService, ListItem } from '@/services/listItemService';

// Priority options
const PRIORITY_OPTIONS = [
  { value: 'Low', label: 'Low', color: 'text-green-600' },
  { value: 'Medium', label: 'Medium', color: 'text-yellow-600' },
  { value: 'High', label: 'High', color: 'text-orange-600' },
  { value: 'Critical', label: 'Critical', color: 'text-red-600' },
];

export default function CreateTaskPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  
  // Form data  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    taskTypeUID: '', // Changed to use UID instead of ID
    salesOrgId: 0,
    startDate: format(new Date(), 'yyyy-MM-dd'),
    endDate: format(new Date(Date.now() + 24 * 60 * 60 * 1000), 'yyyy-MM-dd'), // Default to tomorrow
    isActive: true,
    priority: 'Medium',
    taskData: '',
  });
  
  // Dropdown data
  const [taskTypes, setTaskTypes] = useState<ListItem[]>([]);
  const [salesOrgs, setSalesOrgs] = useState<any[]>([]);
  
  // UI State
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [createdTaskId, setCreatedTaskId] = useState<string | null>(null);
  const [startDateOpen, setStartDateOpen] = useState(false);
  const [endDateOpen, setEndDateOpen] = useState(false);

  // Load initial data
  useEffect(() => {
    loadInitialData();
  }, []);

  // No sub types needed with list_item structure

  const loadInitialData = async () => {
    setLoadingData(true);
    try {
      // Load task types from list_item
      const taskTypesData = await listItemService.getTaskTypes();
      console.log('Task types response:', taskTypesData);
      setTaskTypes(taskTypesData);
      
      // Load sales organizations
      const orgsResponse = await organizationService.getOrganizations();
      let orgs = [];
      if (Array.isArray(orgsResponse)) {
        orgs = orgsResponse;
      } else if (orgsResponse?.data?.Data && Array.isArray(orgsResponse.data.Data)) {
        orgs = orgsResponse.data.Data;
      } else if (orgsResponse?.Data && Array.isArray(orgsResponse.Data)) {
        orgs = orgsResponse.Data;
      } else if (orgsResponse?.data && Array.isArray(orgsResponse.data)) {
        orgs = orgsResponse.data;
      }
      
      // Normalize organization field names
      orgs = orgs.map(org => ({
        id: org.id || org.Id || org.ID || 0,
        uid: org.uid || org.UID || org.Uid || '',
        code: org.code || org.Code || '',
        name: org.name || org.Name || '',
        description: org.description || org.Description || ''
      }));
      
      setSalesOrgs(orgs);
    } catch (error) {
      console.error('Error loading initial data:', error);
      toast({
        title: 'Error',
        description: 'Failed to load data. Please refresh the page.',
        variant: 'destructive',
      });
    } finally {
      setLoadingData(false);
    }
  };

  // No sub types loading needed with list_item structure

  const handleInputChange = (field: string, value: any) => {
    setFormData(prev => {
      const newData = {
        ...prev,
        [field]: value,
      };
      
      // If start date is changed and it's >= end date, set end date to start date + 1 day
      if (field === 'startDate' && value) {
        const startDate = new Date(value);
        const endDate = new Date(prev.endDate);
        if (endDate <= startDate) {
          const nextDay = new Date(startDate);
          nextDay.setDate(nextDay.getDate() + 1);
          newData.endDate = format(nextDay, 'yyyy-MM-dd');
        }
      }
      
      return newData;
    });
  };

  const validateForm = (): boolean => {
    const errors: string[] = [];
    
    if (!formData.title?.trim()) {
      errors.push('Task Title is required');
    }
    
    if (!formData.taskTypeUID?.trim()) {
      errors.push('Task Type is required');
    }
    
    if (formData.salesOrgId === 0) {
      errors.push('Sales Organization is required');
    }
    
    if (!formData.startDate) {
      errors.push('Start Date is required');
    }
    
    if (!formData.endDate) {
      errors.push('End Date is required');
    }
    
    // Validate date range
    if (formData.startDate && formData.endDate) {
      const start = new Date(formData.startDate);
      const end = new Date(formData.endDate);
      if (end <= start) {
        errors.push('End Date must be after Start Date');
      }
    }
    
    if (errors.length > 0) {
      toast({
        title: 'Validation Error',
        description: errors.join(', '),
        variant: 'destructive',
      });
      return false;
    }
    
    return true;
  };

  const handleSave = async () => {
    if (!validateForm()) {
      return;
    }

    setLoading(true);
    try {
      // Prepare the data - convert UID to ID for backend
      const selectedTaskType = taskTypes.find(type => type.uid === formData.taskTypeUID);
      const dataToSend = {
        ...formData,
        // Use the numeric ID for backend compatibility
        taskTypeId: selectedTaskType?.id || 0,
        // Remove UID fields as backend expects numeric IDs
        taskTypeUID: undefined,
        // Generate a unique code
        code: `TASK_${Date.now()}`,
        // Ensure dates are in correct format
        startDate: formData.startDate,
        endDate: formData.endDate,
        // Add any additional task data as JSON
        taskData: JSON.stringify({
          createdFrom: 'Web Portal',
          additionalInfo: formData.description,
          taskTypeCode: selectedTaskType?.code,
          taskTypeName: selectedTaskType?.name
        }),
      };

      const response = await taskService.createTask(dataToSend);
      
      // Get the created task ID/UID
      const taskId = response?.data?.uid || response?.uid || response?.data?.id || response?.id;
      setCreatedTaskId(taskId);
      setShowConfirmation(true);
      
      toast({
        title: 'Success',
        description: 'Task created successfully',
      });
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

  const handleAddNew = () => {
    // Reset form
    setFormData({
      title: '',
      description: '',
      taskTypeUID: '',
      salesOrgId: 0,
      startDate: format(new Date(), 'yyyy-MM-dd'),
      endDate: format(new Date(Date.now() + 24 * 60 * 60 * 1000), 'yyyy-MM-dd'), // Default to tomorrow
      isActive: true,
      priority: 'Medium',
      taskData: '',
    });
    setShowConfirmation(false);
    setCreatedTaskId(null);
  };

  const handleEdit = () => {
    if (createdTaskId) {
      router.push(`/administration/configurations/task/edit/${createdTaskId}`);
    }
  };

  // Show loading state
  if (loadingData) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="flex items-center justify-center">
              <Loader2 className="h-8 w-8 animate-spin" />
              <span className="ml-2">Loading...</span>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Show confirmation screen
  if (showConfirmation) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="flex flex-col items-center">
              <CheckCircle2 className="h-16 w-16 text-green-600 mb-4" />
              <h2 className="text-xl font-semibold text-green-600 mb-2">
                Task Created Successfully!
              </h2>
              <p className="text-gray-600 mb-8">
                Your task has been created and assigned.
              </p>
              <div className="flex gap-4">
                <Button
                  onClick={() => router.push('/administration/configurations/task')}
                  variant="outline"
                >
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Back to Tasks
                </Button>
                <Button onClick={handleEdit}>
                  Edit Task
                </Button>
                <Button onClick={handleAddNew}>
                  Create Another
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 max-w-5xl">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <ClipboardList className="h-6 w-6 text-primary" />
              <div>
                <CardTitle>Create New Task</CardTitle>
                <CardDescription className="mt-1">
                  Define task details and assign to team members
                </CardDescription>
              </div>
            </div>
            <Button
              variant="outline"
              onClick={() => router.push('/administration/configurations/task')}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back
            </Button>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          <div className="text-sm text-gray-500 text-right">
            (<span className="text-red-500">*</span>) indicates required fields
          </div>

          {/* Task Title */}
          <div className="space-y-2">
            <Label htmlFor="title">
              Task Title <span className="text-red-500">*</span>
            </Label>
            <Input
              id="title"
              placeholder="Enter task title"
              value={formData.title}
              onChange={(e) => handleInputChange('title', e.target.value)}
              maxLength={200}
            />
          </div>

          {/* Task Description */}
          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              placeholder="Enter task description"
              value={formData.description}
              onChange={(e) => handleInputChange('description', e.target.value)}
              rows={4}
            />
          </div>

          {/* Task Type */}
          <div className="space-y-2">
            <Label htmlFor="taskType">
              Task Type <span className="text-red-500">*</span>
            </Label>
            <Select
              value={formData.taskTypeUID}
              onValueChange={(value) => {
                handleInputChange('taskTypeUID', value);
              }}
            >
              <SelectTrigger id="taskType">
                <SelectValue placeholder="Select task type" />
              </SelectTrigger>
              <SelectContent>
                {taskTypes.map((type) => (
                  <SelectItem 
                    key={type.uid} 
                    value={type.uid}
                  >
                    {type.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Sales Organization */}
          <div className="space-y-2">
            <Label htmlFor="salesOrg">
              Sales Organization <span className="text-red-500">*</span>
            </Label>
            <Select
              value={formData.salesOrgId.toString()}
              onValueChange={(value) => handleInputChange('salesOrgId', parseInt(value))}
            >
              <SelectTrigger id="salesOrg">
                <SelectValue placeholder="Select sales organization" />
              </SelectTrigger>
              <SelectContent>
                {salesOrgs.map((org) => (
                  <SelectItem key={org.uid || org.id} value={(org.id || 0).toString()}>
                    <div className="flex items-center gap-2">
                      <Building2 className="h-4 w-4" />
                      {org.name || 'Unknown'}
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Dates */}
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>
                Start Date <span className="text-red-500">*</span>
              </Label>
              <Popover open={startDateOpen} onOpenChange={setStartDateOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className={cn(
                      "w-full justify-start text-left font-normal",
                      !formData.startDate && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {formData.startDate ? format(new Date(formData.startDate), 'PPP') : "Select date"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={formData.startDate ? new Date(formData.startDate) : undefined}
                    onSelect={(date) => {
                      if (date) {
                        handleInputChange('startDate', format(date, 'yyyy-MM-dd'));
                        setStartDateOpen(false);
                      }
                    }}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>

            <div className="space-y-2">
              <Label>
                End Date <span className="text-red-500">*</span>
              </Label>
              <Popover open={endDateOpen} onOpenChange={setEndDateOpen}>
                <PopoverTrigger asChild>
                  <Button
                    variant="outline"
                    className={cn(
                      "w-full justify-start text-left font-normal",
                      !formData.endDate && "text-muted-foreground"
                    )}
                  >
                    <CalendarIcon className="mr-2 h-4 w-4" />
                    {formData.endDate ? format(new Date(formData.endDate), 'PPP') : "Select date"}
                  </Button>
                </PopoverTrigger>
                <PopoverContent className="w-auto p-0">
                  <Calendar
                    mode="single"
                    selected={formData.endDate ? new Date(formData.endDate) : undefined}
                    onSelect={(date) => {
                      if (date) {
                        handleInputChange('endDate', format(date, 'yyyy-MM-dd'));
                        setEndDateOpen(false);
                      }
                    }}
                    disabled={(date) => {
                      // Disable dates before or equal to start date
                      if (formData.startDate) {
                        return date <= new Date(formData.startDate);
                      }
                      return false;
                    }}
                    initialFocus
                  />
                </PopoverContent>
              </Popover>
            </div>
          </div>

          {/* Priority and Status */}
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="priority">Priority</Label>
              <Select
                value={formData.priority}
                onValueChange={(value) => handleInputChange('priority', value)}
              >
                <SelectTrigger id="priority">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {PRIORITY_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      <span className={option.color}>{option.label}</span>
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="isActive">Status</Label>
              <div className="flex items-center space-x-2 pt-2">
                <Switch
                  id="isActive"
                  checked={formData.isActive}
                  onCheckedChange={(checked) => handleInputChange('isActive', checked)}
                />
                <Label htmlFor="isActive" className="cursor-pointer">
                  {formData.isActive ? 'Active' : 'Inactive'}
                </Label>
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-4 pt-6 border-t">
            <Button
              variant="outline"
              onClick={() => router.push('/administration/configurations/task')}
              disabled={loading}
            >
              Cancel
            </Button>
            <Button onClick={handleSave} disabled={loading}>
              {loading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Creating...
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Create Task
                </>
              )}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}