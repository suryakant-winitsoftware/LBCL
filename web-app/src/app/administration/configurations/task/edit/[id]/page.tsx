'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
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
  CheckCircle2,
  ClipboardList,
  Building2,
} from 'lucide-react';
import taskService, { Task } from '@/services/taskService';
import { organizationService } from '@/services/organizationService';
import { listItemService, ListItem } from '@/services/listItemService';

// Priority options
const PRIORITY_OPTIONS = [
  { value: 'Low', label: 'Low', color: 'text-green-600' },
  { value: 'Medium', label: 'Medium', color: 'text-yellow-600' },
  { value: 'High', label: 'High', color: 'text-orange-600' },
  { value: 'Critical', label: 'Critical', color: 'text-red-600' },
];

export default function EditTaskPage() {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(true);
  
  // Form data  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    taskTypeUID: '',
    salesOrgId: 0,
    startDate: format(new Date(), 'yyyy-MM-dd'),
    endDate: format(new Date(Date.now() + 24 * 60 * 60 * 1000), 'yyyy-MM-dd'),
    isActive: true,
    priority: 'Medium',
    taskData: '',
  });
  
  // Dropdown data
  const [taskTypes, setTaskTypes] = useState<ListItem[]>([]);
  const [salesOrgs, setSalesOrgs] = useState<any[]>([]);
  
  // UI State
  const [showConfirmation, setShowConfirmation] = useState(false);
  const [startDateOpen, setStartDateOpen] = useState(false);
  const [endDateOpen, setEndDateOpen] = useState(false);

  // Load initial data first
  useEffect(() => {
    if (params.id) {
      loadInitialData();
    }
  }, [params.id]);

  // Load task data after task types are loaded
  useEffect(() => {
    if (params.id && taskTypes.length > 0) {
      loadTask(params.id as string);
    }
  }, [params.id, taskTypes]);

  const loadInitialData = async () => {
    try {
      // Load task types from list_item
      const taskTypesData = await listItemService.getTaskTypes();
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
    }
  };

  const loadTask = async (taskId: string) => {
    try {
      setLoadingData(true);
      
      // Use getTaskById with numeric ID if taskId is numeric, otherwise use getTaskByUID
      let response;
      const numericId = parseInt(taskId);
      if (!isNaN(numericId) && numericId.toString() === taskId) {
        // It's a numeric ID
        response = await taskService.getTaskById(numericId);
      } else {
        // It's a UID
        response = await taskService.getTaskByUID(taskId);
      }
      
      console.log('Task response:', response); // Debug log
      
      let taskData;
      // Handle different response structures
      if (response?.Data) {
        taskData = response.Data;
      } else if (response?.data?.Data) {
        taskData = response.data.Data;
      } else if (response?.data) {
        taskData = response.data;
      } else {
        taskData = response;
      }
      
      console.log('Raw task data from API:', taskData); // Debug log
      console.log('Task data keys:', Object.keys(taskData || {})); // Debug log
      
      if (taskData) {
        // Parse TaskData JSON to extract task type information
        let taskDataJson = null;
        const rawTaskData = taskData.TaskData || taskData.taskData || '';
        if (rawTaskData) {
          try {
            taskDataJson = JSON.parse(rawTaskData);
            console.log('Parsed TaskData JSON:', taskDataJson);
          } catch (e) {
            console.warn('Failed to parse TaskData JSON:', e);
          }
        }

        // Normalize field names (backend uses PascalCase)
        const normalizedTask = {
          id: taskData.Id || taskData.id || 0,
          uid: taskData.UID || taskData.uid || '',
          title: taskData.Title || taskData.title || '',
          description: taskData.Description || taskData.description || '',
          taskTypeId: taskData.TaskTypeId || taskData.taskTypeId || 0,
          taskTypeUID: taskData.TaskTypeUID || taskData.taskTypeUID || taskDataJson?.taskTypeCode || '', // Try TaskData JSON too
          taskTypeName: taskData.TaskTypeName || taskData.taskTypeName || taskDataJson?.taskTypeName || '',
          salesOrgId: taskData.SalesOrgId || taskData.salesOrgId || 0,
          startDate: taskData.StartDate || taskData.startDate || '',
          endDate: taskData.EndDate || taskData.endDate || '',
          isActive: taskData.IsActive !== undefined ? taskData.IsActive : taskData.isActive !== undefined ? taskData.isActive : true,
          priority: taskData.Priority || taskData.priority || 'Medium',
          status: taskData.Status || taskData.status || 'Draft',
          taskData: rawTaskData,
        };
        
        console.log('Normalized task:', normalizedTask); // Debug log
        console.log('Task type info from normalized:', {
          taskTypeId: normalizedTask.taskTypeId,
          taskTypeUID: normalizedTask.taskTypeUID,
          taskTypeName: normalizedTask.taskTypeName
        }); // Debug log
        
        // Find matching task type UID
        console.log('Task type ID from backend:', normalizedTask.taskTypeId);
        console.log('Task type UID from backend:', normalizedTask.taskTypeUID);
        console.log('Available task types:', taskTypes.map(t => ({id: t.id, uid: t.uid, name: t.name}))); // Debug log
        
        let taskTypeUID = '';
        
        // First try to use the direct UID from backend if available
        if (normalizedTask.taskTypeUID) {
          // Verify it exists in our task types
          const directMatch = taskTypes.find(t => t.uid === normalizedTask.taskTypeUID);
          if (directMatch) {
            taskTypeUID = normalizedTask.taskTypeUID;
            console.log('Using direct UID match:', taskTypeUID);
          }
        }
        
        // If no direct UID match, try by ID
        if (!taskTypeUID && normalizedTask.taskTypeId) {
          const idMatch = taskTypes.find(t => t.id === normalizedTask.taskTypeId);
          if (idMatch) {
            taskTypeUID = idMatch.uid;
            console.log('Using ID match:', normalizedTask.taskTypeId, '->', taskTypeUID);
          }
        }
        
        // If still no match, try by name as fallback
        if (!taskTypeUID && normalizedTask.taskTypeName) {
          const nameMatch = taskTypes.find(t => t.name.toLowerCase() === normalizedTask.taskTypeName.toLowerCase());
          if (nameMatch) {
            taskTypeUID = nameMatch.uid;
            console.log('Using name match:', normalizedTask.taskTypeName, '->', taskTypeUID);
          }
        }
        
        // If still no match, try to match by code from TaskData JSON 
        if (!taskTypeUID && taskDataJson?.taskTypeCode) {
          const codeMatch = taskTypes.find(t => t.code?.toLowerCase() === taskDataJson.taskTypeCode.toLowerCase());
          if (codeMatch) {
            taskTypeUID = codeMatch.uid;
            console.log('Using TaskData code match:', taskDataJson.taskTypeCode, '->', taskTypeUID);
          }
        }
        
        // If still no match found and we have invalid data (task_type_id = 0), default to Survey
        if (!taskTypeUID && (normalizedTask.taskTypeId === 0 || !normalizedTask.taskTypeId)) {
          const defaultMatch = taskTypes.find(t => t.name === 'Survey');
          if (defaultMatch) {
            taskTypeUID = defaultMatch.uid;
            console.log('Using default Survey type for task with invalid task_type_id:', taskTypeUID);
          }
        }
        
        console.log('Final task type UID:', taskTypeUID);
        
        // Parse dates and ensure end date is after start date
        let startDate = normalizedTask.startDate ? format(new Date(normalizedTask.startDate), 'yyyy-MM-dd') : format(new Date(), 'yyyy-MM-dd');
        let endDate = normalizedTask.endDate ? format(new Date(normalizedTask.endDate), 'yyyy-MM-dd') : format(new Date(Date.now() + 24 * 60 * 60 * 1000), 'yyyy-MM-dd');
        
        // Ensure end date is after start date
        const startDateObj = new Date(startDate);
        const endDateObj = new Date(endDate);
        if (endDateObj <= startDateObj) {
          const nextDay = new Date(startDateObj);
          nextDay.setDate(nextDay.getDate() + 1);
          endDate = format(nextDay, 'yyyy-MM-dd');
        }
        
        // Populate form with existing task data
        const formDataToSet = {
          title: normalizedTask.title,
          description: normalizedTask.description,
          taskTypeUID: taskTypeUID,
          salesOrgId: normalizedTask.salesOrgId,
          startDate: startDate,
          endDate: endDate,
          isActive: normalizedTask.isActive,
          priority: normalizedTask.priority,
          taskData: normalizedTask.taskData,
        };
        
        console.log('Form data to set:', formDataToSet); // Debug log
        setFormData(formDataToSet);
        
        // Force a re-render to ensure dropdowns update
        setTimeout(() => {
          console.log('Current form data after timeout:', formData); // Debug log
        }, 100);
      }
    } catch (error) {
      console.error('Error loading task:', error);
      toast({
        title: 'Error',
        description: 'Failed to load task details',
        variant: 'destructive',
      });
      router.push('/administration/configurations/task');
    } finally {
      setLoadingData(false);
    }
  };

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
      // Find the selected task type to get its ID
      const selectedTaskType = taskTypes.find(type => type.uid === formData.taskTypeUID);
      
      // Prepare the data for update
      const dataToSend = {
        uid: params.id as string,
        title: formData.title,
        description: formData.description,
        taskTypeId: selectedTaskType?.id || 0,
        salesOrgId: formData.salesOrgId,
        startDate: formData.startDate,
        endDate: formData.endDate,
        isActive: formData.isActive,
        priority: formData.priority,
        taskData: JSON.stringify({
          updatedFrom: 'Web Portal',
          additionalInfo: formData.description,
          taskTypeCode: selectedTaskType?.code,
          taskTypeName: selectedTaskType?.name
        }),
      };

      const response = await taskService.updateTask(dataToSend);
      
      setShowConfirmation(true);
      
      toast({
        title: 'Success',
        description: 'Task updated successfully',
      });
    } catch (error: any) {
      console.error('Error updating task:', error);
      toast({
        title: 'Error',
        description: error.response?.data?.message || 'Failed to update task',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
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
              <span className="ml-2">Loading task details...</span>
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
                Task Updated Successfully!
              </h2>
              <p className="text-gray-600 mb-8">
                Your task has been updated with the new details.
              </p>
              <div className="flex gap-4">
                <Button
                  onClick={() => router.push('/administration/configurations/task')}
                  variant="outline"
                >
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Back to Tasks
                </Button>
                <Button onClick={() => setShowConfirmation(false)}>
                  Edit Again
                </Button>
                <Button onClick={() => router.push('/administration/configurations/task/create')}>
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
                <CardTitle>Edit Task</CardTitle>
                <CardDescription className="mt-1">
                  Update task details and assignments
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
          
          {/* Debug info - remove in production */}
          <div className="bg-gray-100 p-3 rounded text-xs font-mono">
            <div>Task ID: {params.id}</div>
            <div>Task Type UID: {formData.taskTypeUID}</div>
            <div>Selected Task Type: {formData.taskTypeUID ? taskTypes.find(t => t.uid === formData.taskTypeUID)?.name : 'None'}</div>
            <div>Sales Org ID: {formData.salesOrgId}</div>
            <div>Selected Sales Org: {formData.salesOrgId ? salesOrgs.find(o => o.id === formData.salesOrgId)?.name : 'None'}</div>
            <div>Task Types Loaded: {taskTypes.length}</div>
            <div>Sales Orgs Loaded: {salesOrgs.length}</div>
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
              value={formData.taskTypeUID || ''}
              onValueChange={(value) => {
                console.log('Task type selected:', value); // Debug log
                handleInputChange('taskTypeUID', value);
              }}
            >
              <SelectTrigger id="taskType">
                <SelectValue placeholder="Select task type">
                  {formData.taskTypeUID ? 
                    taskTypes.find(t => t.uid === formData.taskTypeUID)?.name || 'Select task type'
                    : 'Select task type'
                  }
                </SelectValue>
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
              value={formData.salesOrgId > 0 ? formData.salesOrgId.toString() : ''}
              onValueChange={(value) => {
                console.log('Sales org selected:', value); // Debug log
                handleInputChange('salesOrgId', parseInt(value));
              }}
            >
              <SelectTrigger id="salesOrg">
                <SelectValue placeholder="Select sales organization">
                  {formData.salesOrgId > 0 ? 
                    salesOrgs.find(o => o.id === formData.salesOrgId)?.name || 'Select sales organization'
                    : 'Select sales organization'
                  }
                </SelectValue>
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
                  Updating...
                </>
              ) : (
                <>
                  <Save className="mr-2 h-4 w-4" />
                  Update Task
                </>
              )}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}