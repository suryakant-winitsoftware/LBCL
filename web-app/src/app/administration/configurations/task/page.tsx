'use client';

import React, { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Card,
  CardContent,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu';
import { Badge } from '@/components/ui/badge';
import { Checkbox } from '@/components/ui/checkbox';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/use-toast';
import { PaginationControls } from '@/components/ui/pagination-controls';
import {
  Plus,
  Edit,
  Trash2,
  Search,
  Calendar,
  Camera,
  Target,
  Package,
  CheckCircle,
  Clock,
  FileDown,
  Upload,
  X,
  ChevronDown,
  Filter,
  MoreVertical,
  ListTodo,
  Eye,
  UserPlus,
} from 'lucide-react';
import taskService, { Task, TaskFilter } from '@/services/taskService';
import { format } from 'date-fns';

const TASK_STATUSES = ['Draft', 'Active', 'Completed', 'Cancelled', 'OnHold'] as const;
const TASK_PRIORITIES = ['Low', 'Medium', 'High', 'Critical'] as const;

export default function TaskManagementPage() {
  const router = useRouter();
  const { toast } = useToast();
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [filteredTasks, setFilteredTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedTasks, setSelectedTasks] = useState<string[]>([]);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalCount, setTotalCount] = useState(0);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatuses, setSelectedStatuses] = useState<string[]>([]);
  const [selectedPriorities, setSelectedPriorities] = useState<string[]>([]);

  useEffect(() => {
    // Check for success message from URL params or session
    const urlParams = new URLSearchParams(window.location.search);
    const message = urlParams.get('message');
    if (message) {
      toast({
        title: 'Success',
        description: decodeURIComponent(message),
      });
      // Clean URL
      window.history.replaceState({}, '', window.location.pathname);
    }
    
    loadTasks();
  }, []);

  // Add keyboard shortcut for Ctrl+F / Cmd+F
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === 'f') {
        e.preventDefault();
        searchInputRef.current?.focus();
        searchInputRef.current?.select();
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  // Filter data based on search and filters
  useEffect(() => {
    let filtered = [...tasks];

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(task => 
        task.title?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        task.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        task.taskTypeName?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply status filter
    if (selectedStatuses.length > 0) {
      filtered = filtered.filter(task => 
        selectedStatuses.includes(task.status)
      );
    }

    // Apply priority filter
    if (selectedPriorities.length > 0) {
      filtered = filtered.filter(task => 
        selectedPriorities.includes(task.priority)
      );
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredTasks(paginatedData);
    setTotalCount(filtered.length);
  }, [tasks, searchTerm, selectedStatuses, selectedPriorities, currentPage, pageSize]);

  const loadTasks = async () => {
    try {
      setLoading(true);
      const paginatedFilter = {
        pageNumber: 1,
        pageSize: 1000, // Get all for client-side filtering
      };
      const response = await taskService.getPagedTasks(paginatedFilter);
      
      console.log('Task response:', response); // Debug log
      console.log('Task response.data:', response?.data); // Debug log
      
      let tasksData = [];
      
      // Check for the deeply nested structure: response.data.Data.Data
      if (response?.data?.Data?.Data && Array.isArray(response.data.Data.Data)) {
        tasksData = response.data.Data.Data;
        console.log('Using response.data.Data.Data path');
      }
      // Check for response.Data first (direct API response)
      else if (response?.Data && Array.isArray(response.Data)) {
        tasksData = response.Data;
        console.log('Using response.Data path');
      } 
      // Then check for response.data.Data (wrapped response)
      else if (response?.data?.Data && Array.isArray(response.data.Data)) {
        tasksData = response.data.Data;
        console.log('Using response.data.Data path');
      } 
      // Then check for response.data (simple array)
      else if (response?.data && Array.isArray(response.data)) {
        tasksData = response.data;
        console.log('Using response.data path');
      } 
      // Finally check if response itself is an array
      else if (Array.isArray(response)) {
        tasksData = response;
        console.log('Using response path');
      } else {
        console.log('No matching response structure found, response:', response);
        console.log('response keys:', Object.keys(response || {}));
        console.log('response.data keys:', Object.keys(response?.data || {}));
      }
      
      // Normalize field names from backend (PascalCase) to frontend (camelCase)
      const normalizedTasks = tasksData.map((task: any) => ({
        id: task.Id || task.id || 0,
        uid: task.UID || task.uid || '',
        code: task.Code || task.code || '',
        title: task.Title || task.title || '',
        description: task.Description || task.description || '',
        taskTypeId: task.TaskTypeId || task.taskTypeId || 0,
        taskTypeName: task.TaskTypeName || task.taskTypeName || '',
        taskSubTypeId: task.TaskSubTypeId || task.taskSubTypeId,
        taskSubTypeName: task.TaskSubTypeName || task.taskSubTypeName || '',
        salesOrgId: task.SalesOrgId || task.salesOrgId || 0,
        salesOrgName: task.SalesOrgName || task.salesOrgName || '',
        startDate: task.StartDate || task.startDate || '',
        endDate: task.EndDate || task.endDate || '',
        isActive: task.IsActive !== undefined ? task.IsActive : task.isActive !== undefined ? task.isActive : true,
        priority: task.Priority || task.priority || 'Medium',
        status: task.Status || task.status || 'Draft',
        taskData: task.TaskData || task.taskData || '',
        createdTime: task.CreatedTime || task.createdTime || '',
        createdBy: task.CreatedBy || task.createdBy || '',
        modifiedTime: task.ModifiedTime || task.modifiedTime || '',
        modifiedBy: task.ModifiedBy || task.modifiedBy || ''
      }));
      
      console.log('Parsed tasks data:', tasksData); // Debug log
      console.log('Normalized tasks data:', normalizedTasks); // Debug log
      setTasks(normalizedTasks);
    } catch (error) {
      console.error('Error loading tasks:', error);
      setTasks([]);
      toast({
        title: 'Error',
        description: 'Failed to load tasks',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const handleSelectAll = () => {
    if (selectedTasks.length === filteredTasks.length && filteredTasks.length > 0) {
      setSelectedTasks([]);
    } else {
      setSelectedTasks(filteredTasks.map(t => t.uid!).filter(uid => uid !== undefined));
    }
  };

  const handleSelectTask = (taskUid: string) => {
    if (selectedTasks.includes(taskUid)) {
      setSelectedTasks(selectedTasks.filter(uid => uid !== taskUid));
    } else {
      setSelectedTasks([...selectedTasks, taskUid]);
    }
  };

  const handleDeleteSelected = async () => {
    if (selectedTasks.length === 0) {
      toast({
        title: 'Warning',
        description: 'Please select tasks to delete',
        variant: 'destructive',
      });
      return;
    }

    setDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    try {
      // Delete tasks one by one using UID
      for (const taskUid of selectedTasks) {
        await taskService.deleteTask(taskUid);
      }
      
      toast({
        title: 'Success',
        description: `${selectedTasks.length} task(s) deleted successfully`,
      });
      
      setSelectedTasks([]);
      loadTasks();
    } catch (error) {
      console.error('Error deleting tasks:', error);
      toast({
        title: 'Error',
        description: 'Failed to delete tasks',
        variant: 'destructive',
      });
    } finally {
      setDeleteDialogOpen(false);
    }
  };

  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  const handleExport = () => {
    const csvContent = [
      ["Task Title", "Type", "Priority", "Status", "Start Date", "End Date"],
      ...filteredTasks.map(task => [
        task.title,
        task.taskTypeName || 'N/A',
        task.priority,
        task.status,
        task.startDate ? format(new Date(task.startDate), 'MMM d, yyyy') : '',
        task.endDate ? format(new Date(task.endDate), 'MMM d, yyyy') : ''
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `tasks_${new Date().toISOString()}.csv`;
    a.click();
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Draft':
        return <Badge variant="secondary">Draft</Badge>;
      case 'Active':
        return (
          <Badge variant="default" className="bg-blue-500 hover:bg-blue-600">
            <Clock className="mr-1 h-3 w-3" />
            Active
          </Badge>
        );
      case 'Completed':
        return (
          <Badge variant="default" className="bg-green-500 hover:bg-green-600">
            <CheckCircle className="mr-1 h-3 w-3" />
            Completed
          </Badge>
        );
      case 'Cancelled':
        return <Badge variant="destructive">Cancelled</Badge>;
      case 'OnHold':
        return (
          <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">
            On Hold
          </Badge>
        );
      default:
        return <Badge variant="outline">{status}</Badge>;
    }
  };

  const getPriorityBadge = (priority: string) => {
    switch (priority) {
      case 'Low':
        return <Badge variant="secondary">Low</Badge>;
      case 'Medium':
        return <Badge variant="default">Medium</Badge>;
      case 'High':
        return (
          <Badge variant="default" className="bg-orange-500 hover:bg-orange-600">
            High
          </Badge>
        );
      case 'Critical':
        return <Badge variant="destructive">Critical</Badge>;
      default:
        return <Badge variant="outline">{priority}</Badge>;
    }
  };

  const getTaskTypeIcons = (task: Task) => {
    const icons = [];
    if (task.taskData) {
      try {
        const data = JSON.parse(task.taskData);
        if (data.requiresPhotoCapture) {
          icons.push(<Camera key="photo" className="h-3 w-3 text-blue-500" title="Photo Capture" />);
        }
        if (data.requiresPromotionSetup) {
          icons.push(<Target key="promotion" className="h-3 w-3 text-orange-500" title="Competitor Promotions" />);
        }
        if (data.requiresStockCheck) {
          icons.push(<Package key="stock" className="h-3 w-3 text-green-500" title="Stock Check" />);
        }
      } catch (e) {
        // Handle invalid JSON gracefully
      }
    }
    return icons;
  };

  const formatDate = (dateString?: string) => {
    if (!dateString) return '-';
    try {
      // Handle both ISO date strings and date objects
      const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
      return format(date, 'MMM d, yyyy');
    } catch {
      return dateString || '-';
    }
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Task Management</h2>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <FileDown className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button
            onClick={() => router.push('/administration/configurations/task/create')}
            size="sm"
          >
            <Plus className="h-4 w-4 mr-2" />
            Add Task
          </Button>
        </div>
      </div>

      {/* Search Bar */}
      <Card className="shadow-sm border-gray-200 mb-4">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by title, description or type... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Status Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Status
                  {selectedStatuses.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedStatuses.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {TASK_STATUSES.map(status => (
                  <DropdownMenuCheckboxItem
                    key={status}
                    checked={selectedStatuses.includes(status)}
                    onCheckedChange={(checked) => {
                      setSelectedStatuses(prev => 
                        checked 
                          ? [...prev, status]
                          : prev.filter(s => s !== status)
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {status}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Priority Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Priority
                  {selectedPriorities.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedPriorities.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Priority</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {TASK_PRIORITIES.map(priority => (
                  <DropdownMenuCheckboxItem
                    key={priority}
                    checked={selectedPriorities.includes(priority)}
                    onCheckedChange={(checked) => {
                      setSelectedPriorities(prev => 
                        checked 
                          ? [...prev, priority]
                          : prev.filter(p => p !== priority)
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {priority}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedStatuses.length > 0 || selectedPriorities.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedStatuses([]);
                  setSelectedPriorities([]);
                  setCurrentPage(1);
                }}
              >
                <X className="h-4 w-4 mr-2" />
                Clear All
              </Button>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Selected Actions */}
      {selectedTasks.length > 0 && (
        <Card className="shadow-sm border-orange-200 bg-orange-50 mb-4">
          <CardContent className="py-3">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="bg-orange-100 text-orange-800">
                  {selectedTasks.length} selected
                </Badge>
                <span className="text-sm text-orange-700">
                  Select actions to apply to selected tasks
                </span>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setSelectedTasks([])}
                >
                  Clear Selection
                </Button>
                <Button
                  variant="destructive"
                  size="sm"
                  onClick={handleDeleteSelected}
                >
                  <Trash2 className="h-4 w-4 mr-2" />
                  Delete Selected
                </Button>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-12 pl-6">
                  <Checkbox
                    checked={selectedTasks.length === filteredTasks.length && filteredTasks.length > 0}
                    onCheckedChange={handleSelectAll}
                  />
                </TableHead>
                <TableHead>Task Title</TableHead>
                <TableHead>Type</TableHead>
                <TableHead className="text-center">Priority</TableHead>
                <TableHead className="text-center">Status</TableHead>
                <TableHead>Start Date</TableHead>
                <TableHead>End Date</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, index) => (
                  <TableRow key={index}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-4 w-4" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-40" />
                      <Skeleton className="h-3 w-20 mt-1" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-6 w-16 mx-auto rounded-full" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-6 w-20 mx-auto rounded-full" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-24" />
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <div className="flex justify-end">
                        <Skeleton className="h-8 w-8 rounded" />
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              ) : filteredTasks.length === 0 ? (
                <TableRow key="no-tasks">
                  <TableCell colSpan={8} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <ListTodo className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No tasks found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm || selectedStatuses.length > 0 || selectedPriorities.length > 0
                          ? "Try adjusting your search or filters" 
                          : "Click 'Add Task' to create your first task"}
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                filteredTasks.map((task) => (
                  <TableRow key={task.uid || task.id}>
                    <TableCell className="pl-6">
                      <Checkbox
                        checked={selectedTasks.includes(task.uid || task.id?.toString() || '')}
                        onCheckedChange={() => handleSelectTask(task.uid || task.id?.toString() || '')}
                      />
                    </TableCell>
                    <TableCell>
                      <div>
                        <div className="font-medium">{task.title}</div>
                        <div className="flex gap-1 mt-1">
                          {getTaskTypeIcons(task)}
                        </div>
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="text-sm">{task.taskTypeName || 'N/A'}</span>
                    </TableCell>
                    <TableCell className="text-center">
                      {getPriorityBadge(task.priority)}
                    </TableCell>
                    <TableCell className="text-center">
                      {getStatusBadge(task.status)}
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Calendar className="h-3 w-3 text-gray-400" />
                        {formatDate(task.startDate)}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="flex items-center gap-1">
                        <Calendar className="h-3 w-3 text-gray-400" />
                        {formatDate(task.endDate)}
                      </div>
                    </TableCell>
                    <TableCell className="text-right pr-6">
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" className="h-8 w-8 p-0">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuLabel>Actions</DropdownMenuLabel>
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/configurations/task/view/${task.uid || task.id}`)}
                          >
                            <Eye className="mr-2 h-4 w-4" />
                            View
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/configurations/task/edit/${task.uid || task.id}`)}
                          >
                            <Edit className="mr-2 h-4 w-4" />
                            Edit
                          </DropdownMenuItem>
                          <DropdownMenuItem
                            onClick={() => router.push(`/administration/configurations/task/assign/${task.uid || task.id}`)}
                          >
                            <UserPlus className="mr-2 h-4 w-4" />
                            Assign
                          </DropdownMenuItem>
                          <DropdownMenuSeparator />
                          <DropdownMenuItem
                            onClick={() => {
                              setSelectedTasks([task.uid || task.id?.toString() || '']);
                              setDeleteDialogOpen(true);
                            }}
                            className="text-red-600 focus:text-red-600"
                          >
                            <Trash2 className="mr-2 h-4 w-4" />
                            Delete
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>

          {/* Pagination */}
          {totalCount > 0 && (
            <div className="px-6 py-4 border-t bg-gray-50/30">
              <PaginationControls
                currentPage={currentPage}
                totalCount={totalCount}
                pageSize={pageSize}
                onPageChange={handlePageChange}
                onPageSizeChange={handlePageSizeChange}
                itemName="tasks"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Delete</DialogTitle>
            <DialogDescription>
              Are you sure you want to delete {selectedTasks.length} selected task(s)? This action cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setDeleteDialogOpen(false)}
            >
              Cancel
            </Button>
            <Button
              variant="destructive"
              onClick={confirmDelete}
            >
              Delete
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}