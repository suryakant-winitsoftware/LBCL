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
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { useToast } from '@/components/ui/use-toast';
import {
  ArrowLeft,
  Save,
  Users,
  Calendar,
  Building,
} from 'lucide-react';

// Mock data - replace with actual API calls
const userGroups = [
  { id: '1', name: 'Sales Team Alpha', description: 'Primary sales representatives' },
  { id: '2', name: 'Sales Team Beta', description: 'Secondary sales representatives' },
  { id: '3', name: 'Audit Team', description: 'Store audit specialists' },
  { id: '4', name: 'Training Team', description: 'Training coordinators' },
  { id: '5', name: 'Regional Managers', description: 'Regional management team' },
];

const specificUserGroups = {
  '1': [
    { id: '1', name: 'North Region Sales' },
    { id: '2', name: 'South Region Sales' },
    { id: '3', name: 'Central Region Sales' },
  ],
  '2': [
    { id: '4', name: 'East Region Sales' },
    { id: '5', name: 'West Region Sales' },
  ],
  '3': [
    { id: '6', name: 'Compliance Auditors' },
    { id: '7', name: 'Inventory Auditors' },
  ],
  '4': [
    { id: '8', name: 'Product Trainers' },
    { id: '9', name: 'System Trainers' },
  ],
  '5': [
    { id: '10', name: 'Area Managers' },
    { id: '11', name: 'District Managers' },
  ],
};

interface TaskDetails {
  id: number;
  taskName: string;
  subCategory: string;
  salesOrganization: string;
  startDate: string;
  endDate: string;
}

export default function AssignTaskPage() {
  const router = useRouter();
  const params = useParams();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  
  const [taskDetails, setTaskDetails] = useState<TaskDetails | null>(null);
  const [selectedUserGroup, setSelectedUserGroup] = useState('');
  const [selectedSpecificGroup, setSelectedSpecificGroup] = useState('');
  const [availableSpecificGroups, setAvailableSpecificGroups] = useState<any[]>([]);

  const taskId = parseInt(params.id as string);

  useEffect(() => {
    loadTaskDetails();
  }, [taskId]);

  useEffect(() => {
    // Update specific groups when user group changes
    if (selectedUserGroup) {
      const groups = specificUserGroups[selectedUserGroup as keyof typeof specificUserGroups] || [];
      setAvailableSpecificGroups(groups);
      setSelectedSpecificGroup(''); // Reset selection
    } else {
      setAvailableSpecificGroups([]);
    }
  }, [selectedUserGroup]);

  const loadTaskDetails = async () => {
    setLoading(true);
    try {
      // Mock API call - replace with actual API
      await new Promise(resolve => setTimeout(resolve, 500));
      
      // Mock task details
      const mockTask: TaskDetails = {
        id: taskId,
        taskName: 'Lock Minimum Check',
        subCategory: 'Store Monitoring',
        salesOrganization: '[EPIC01] admin',
        startDate: 'Sep 23, 2025',
        endDate: 'Sep 23, 2025',
      };
      
      setTaskDetails(mockTask);
    } catch (error: any) {
      console.error('Error loading task details:', error);
      toast({
        title: 'Error',
        description: 'Failed to load task details',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const validateForm = (): boolean => {
    if (!selectedUserGroup) {
      toast({
        title: 'Validation Error',
        description: 'Please select a User Group',
        variant: 'destructive',
      });
      return false;
    }

    if (!selectedSpecificGroup) {
      toast({
        title: 'Validation Error',
        description: 'Please select a specific User Group',
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

    setSaving(true);
    try {
      // Mock save - replace with actual API call
      await new Promise(resolve => setTimeout(resolve, 1000));
      
      toast({
        title: 'Success',
        description: 'Task assigned successfully',
      });
      
      // Redirect back to task management
      router.push('/administration/configurations/task?message=Task assigned successfully');
    } catch (error: any) {
      console.error('Error assigning task:', error);
      toast({
        title: 'Error',
        description: 'Failed to assign task',
        variant: 'destructive',
      });
    } finally {
      setSaving(false);
    }
  };

  const handleBack = () => {
    router.back();
  };

  if (loading) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="text-center">Loading task details...</div>
          </CardContent>
        </Card>
      </div>
    );
  }

  if (!taskDetails) {
    return (
      <div className="container mx-auto py-6 max-w-4xl">
        <Card>
          <CardContent className="py-8">
            <div className="text-center text-gray-500">Task not found</div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="container mx-auto py-6 max-w-4xl">
      <Card>
        <CardHeader>
          <CardTitle className="text-xl font-bold text-blue-600">ASSIGN TASK</CardTitle>
          <CardDescription>
            <div className="flex items-center gap-2 text-sm text-gray-500">
              <a href="/administration/configurations/task" className="hover:underline">Dashboard</a>
              <span>»</span>
              <a href="/administration/configurations/task" className="hover:underline">Manage Task</a>
              <span>»</span>
              <span>Assign Users</span>
            </div>
          </CardDescription>
        </CardHeader>

        <CardContent>
          {/* Task Details Section */}
          <div className="mb-8">
            <h3 className="text-lg font-semibold mb-4">{taskDetails.taskName}</h3>
            
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 p-4 bg-gray-50 rounded-lg">
              <div className="space-y-3">
                <div className="flex items-start gap-3">
                  <span className="font-medium w-32">Sub Category :</span>
                  <span>{taskDetails.subCategory}</span>
                </div>
                <div className="flex items-start gap-3">
                  <span className="font-medium w-32">Sales Organization :</span>
                  <span>{taskDetails.salesOrganization}</span>
                </div>
              </div>
              
              <div className="space-y-3">
                <div className="flex items-center gap-3">
                  <Calendar className="h-4 w-4 text-gray-500" />
                  <span className="font-medium">Start Date :</span>
                  <span>{taskDetails.startDate}</span>
                </div>
                <div className="flex items-center gap-3">
                  <Calendar className="h-4 w-4 text-gray-500" />
                  <span className="font-medium">End Date :</span>
                  <span>{taskDetails.endDate}</span>
                </div>
              </div>
            </div>
          </div>

          {/* Assignment Section */}
          <div className="space-y-6">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
              {/* Assign To */}
              <div className="space-y-2">
                <Label htmlFor="assignTo" className="flex items-center gap-2">
                  <Users className="h-4 w-4" />
                  <strong>Assign To:</strong>
                </Label>
                <Select value={selectedUserGroup} onValueChange={setSelectedUserGroup}>
                  <SelectTrigger id="assignTo">
                    <SelectValue placeholder="User Group" />
                  </SelectTrigger>
                  <SelectContent>
                    {userGroups.map((group) => (
                      <SelectItem key={group.id} value={group.id}>
                        {group.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Select User Group */}
              <div className="space-y-2">
                <Label htmlFor="selectUserGroup" className="flex items-center gap-2">
                  <Building className="h-4 w-4" />
                  <strong>Select User Group:</strong>
                </Label>
                <Select 
                  value={selectedSpecificGroup} 
                  onValueChange={setSelectedSpecificGroup}
                  disabled={!selectedUserGroup}
                >
                  <SelectTrigger id="selectUserGroup">
                    <SelectValue placeholder={selectedUserGroup ? "Select" : "Select User Group first"} />
                  </SelectTrigger>
                  <SelectContent>
                    {availableSpecificGroups.map((group) => (
                      <SelectItem key={group.id} value={group.id}>
                        {group.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>

            {/* Selected Groups Summary */}
            {selectedUserGroup && selectedSpecificGroup && (
              <div className="p-4 bg-blue-50 rounded-lg border border-blue-200">
                <h4 className="font-medium text-blue-800 mb-2">Assignment Summary:</h4>
                <div className="text-sm text-blue-700">
                  <p><strong>User Group:</strong> {userGroups.find(g => g.id === selectedUserGroup)?.name}</p>
                  <p><strong>Specific Group:</strong> {availableSpecificGroups.find(g => g.id === selectedSpecificGroup)?.name}</p>
                </div>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex justify-center gap-3 mt-8 pt-6 border-t">
            <Button
              onClick={handleSave}
              disabled={saving}
              className="bg-green-600 hover:bg-green-700 text-white px-8"
            >
              <Save className="mr-2 h-4 w-4" />
              {saving ? 'Saving...' : 'Save'}
            </Button>
            <Button
              onClick={handleBack}
              variant="secondary"
              disabled={saving}
              className="px-8"
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Back
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}