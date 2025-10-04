'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import {
  Card,
  CardContent,
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
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/use-toast';
import {
  Search,
  RotateCcw,
  Eye,
  Calendar,
  Filter,
  ChevronDown,
  ChevronUp,
} from 'lucide-react';
import taskService from '@/services/taskService';

interface TaskReportItem {
  taskId: number;
  siteName: string;
  salesman: string;
  taskName: string;
  status: 'P' | 'C';
  taskAssignOn: string;
  statusOn?: string;
  completedOn?: string;
}

// Mock data for salesmen and customers - replace with actual API calls
const mockSalesmen = [
  { id: 'SALES001', name: 'John Doe' },
  { id: 'SALES002', name: 'Jane Smith' },
  { id: 'SALES003', name: 'Bob Johnson' },
];

const mockCustomers = [
  { id: 'CUST001', name: 'Store A - Downtown' },
  { id: 'CUST002', name: 'Store B - Mall' },
  { id: 'CUST003', name: 'Store C - Airport' },
  { id: 'CUST004', name: 'Store D - Central' },
  { id: 'CUST005', name: 'Store E - North' },
  { id: 'CUST006', name: 'Store F - South' },
  { id: 'CUST007', name: 'Store G - East' },
];

export default function TaskReportPage() {
  const router = useRouter();
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [showFilters, setShowFilters] = useState(false);
  const [tasks, setTasks] = useState<TaskReportItem[]>([]);
  const [filteredTasks, setFilteredTasks] = useState<TaskReportItem[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize] = useState(10);

  // Filter states
  const [filters, setFilters] = useState({
    taskName: '',
    taskAssignDate: '',
    selectedSalesmen: [] as string[],
    selectedCustomers: [] as string[],
  });

  useEffect(() => {
    // Check for success message from URL params
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('s') === '1') {
      toast({
        title: 'Success',
        description: 'Successfully reviewed the task.',
      });
      // Clean URL
      window.history.replaceState({}, '', window.location.pathname);
    }
    
    loadTasks();
  }, []);

  useEffect(() => {
    applyFilters();
  }, [tasks, filters]);

  const loadTasks = async () => {
    setLoading(true);
    try {
      // Mock data for now - replace with actual API call
      const mockTasks: TaskReportItem[] = [
        {
          taskId: 1,
          siteName: 'Store A - Downtown',
          salesman: 'John Doe',
          taskName: 'Capture Shelf Photo | Stock Check',
          status: 'C',
          taskAssignOn: '2025-01-20',
          completedOn: '2025-01-21T10:30:00',
        },
        {
          taskId: 2,
          siteName: 'Store B - Mall',
          salesman: 'Jane Smith',
          taskName: 'Competitor Promotions',
          status: 'P',
          taskAssignOn: '2025-01-22',
        },
        {
          taskId: 3,
          siteName: 'Store C - Airport',
          salesman: 'Bob Johnson',
          taskName: 'Capture Shelf Photo | Competitor Promotions',
          status: 'C',
          taskAssignOn: '2025-01-19',
          completedOn: '2025-01-20T14:15:00',
        },
      ];
      
      setTasks(mockTasks);
      setTotalCount(mockTasks.length);
    } catch (error: any) {
      console.error('Error loading tasks:', error);
      toast({
        title: 'Error',
        description: 'Failed to load task reports',
        variant: 'destructive',
      });
    } finally {
      setLoading(false);
    }
  };

  const applyFilters = () => {
    let filtered = [...tasks];

    // Filter by task name
    if (filters.taskName) {
      filtered = filtered.filter(task =>
        task.taskName.toLowerCase().includes(filters.taskName.toLowerCase())
      );
    }

    // Filter by task assign date
    if (filters.taskAssignDate) {
      filtered = filtered.filter(task =>
        task.taskAssignOn === filters.taskAssignDate
      );
    }

    // Filter by selected salesmen
    if (filters.selectedSalesmen.length > 0) {
      filtered = filtered.filter(task =>
        filters.selectedSalesmen.includes(task.salesman)
      );
    }

    // Filter by selected customers
    if (filters.selectedCustomers.length > 0) {
      filtered = filtered.filter(task =>
        filters.selectedCustomers.some(customer =>
          task.siteName.includes(customer)
        )
      );
    }

    setFilteredTasks(filtered);
  };

  const handleSearch = () => {
    applyFilters();
    setCurrentPage(1);
  };

  const handleReset = () => {
    setFilters({
      taskName: '',
      taskAssignDate: '',
      selectedSalesmen: [],
      selectedCustomers: [],
    });
    setCurrentPage(1);
  };

  const handleViewReport = (taskId: number) => {
    router.push(`/administration/configurations/task/view/${taskId}`);
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  };

  const formatDateTime = (dateTimeString: string) => {
    if (!dateTimeString) return '';
    const date = new Date(dateTimeString);
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      hour12: true
    });
  };

  const getPaginatedTasks = () => {
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    return filteredTasks.slice(startIndex, endIndex);
  };

  const totalPages = Math.ceil(filteredTasks.length / pageSize);
  const startRecord = filteredTasks.length > 0 ? (currentPage - 1) * pageSize + 1 : 0;
  const endRecord = Math.min(currentPage * pageSize, filteredTasks.length);

  return (
    <div className="container mx-auto py-6 max-w-7xl">
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Maintain Task Report</CardTitle>
              <div className="flex items-center gap-2 text-sm text-gray-500 mt-2">
                <a href="/administration/configurations/task" className="hover:underline">Dashboard</a>
                <span>»</span>
                <a href="/administration/reports" className="hover:underline">Reports</a>
                <span>»</span>
                <span>Task Report</span>
              </div>
            </div>
            <Button
              variant="outline"
              size="sm"
              onClick={() => setShowFilters(!showFilters)}
              className="flex items-center gap-2"
            >
              <Filter className="h-4 w-4" />
              {showFilters ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
            </Button>
          </div>
        </CardHeader>

        <CardContent>
          {/* Search Filters */}
          {showFilters && (
            <div className="mb-6 p-4 border rounded-lg bg-gray-50">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="taskName">Task Name</Label>
                  <Input
                    id="taskName"
                    placeholder="Enter task name"
                    value={filters.taskName}
                    onChange={(e) => setFilters(prev => ({ ...prev, taskName: e.target.value }))}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="taskAssignDate">Task Assign For</Label>
                  <Input
                    id="taskAssignDate"
                    type="date"
                    value={filters.taskAssignDate}
                    onChange={(e) => setFilters(prev => ({ ...prev, taskAssignDate: e.target.value }))}
                  />
                </div>

                <div className="space-y-2">
                  <Label>Site Name</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select Site Name" />
                    </SelectTrigger>
                    <SelectContent>
                      {mockCustomers.map((customer) => (
                        <SelectItem key={customer.id} value={customer.id}>
                          {customer.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div className="space-y-2">
                  <Label>Salesman</Label>
                  <Select>
                    <SelectTrigger>
                      <SelectValue placeholder="Select Salesman" />
                    </SelectTrigger>
                    <SelectContent>
                      {mockSalesmen.map((salesman) => (
                        <SelectItem key={salesman.id} value={salesman.id}>
                          {salesman.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
              </div>

              <div className="flex gap-2 mt-4">
                <Button onClick={handleSearch} size="sm">
                  <Search className="mr-2 h-4 w-4" />
                  Search
                </Button>
                <Button onClick={handleReset} variant="outline" size="sm">
                  <RotateCcw className="mr-2 h-4 w-4" />
                  Reset
                </Button>
              </div>
            </div>
          )}

          {/* Record Count */}
          <div className="flex justify-end mb-4 text-sm text-gray-600">
            You are viewing {startRecord} to {endRecord} of {filteredTasks.length}
          </div>

          {/* Tasks Table */}
          <div className="border rounded-lg">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[300px]">Site Name</TableHead>
                  <TableHead className="w-[250px]">Salesman</TableHead>
                  <TableHead className="w-[400px]">Task Name</TableHead>
                  <TableHead className="w-[200px]">Task Assign For</TableHead>
                  <TableHead className="w-[200px]">Completed On</TableHead>
                  <TableHead className="w-[100px] text-center">Action</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-8">
                      Loading...
                    </TableCell>
                  </TableRow>
                ) : getPaginatedTasks().length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-8 text-gray-500">
                      No records available.
                    </TableCell>
                  </TableRow>
                ) : (
                  getPaginatedTasks().map((task) => (
                    <TableRow key={task.taskId}>
                      <TableCell className="font-medium">
                        <span title={task.siteName}>
                          {task.siteName.length > 30 ? `${task.siteName.substring(0, 30)}...` : task.siteName}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span title={task.salesman}>
                          {task.salesman.length > 20 ? `${task.salesman.substring(0, 20)}...` : task.salesman}
                        </span>
                      </TableCell>
                      <TableCell>
                        <span title={task.taskName}>
                          {task.taskName}
                        </span>
                      </TableCell>
                      <TableCell>{formatDate(task.taskAssignOn)}</TableCell>
                      <TableCell>
                        {task.completedOn ? formatDateTime(task.completedOn) : ''}
                      </TableCell>
                      <TableCell className="text-center">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleViewReport(task.taskId)}
                          className="flex items-center gap-1"
                        >
                          <Eye className="h-3 w-3" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 mt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={currentPage === 1}
                onClick={() => setCurrentPage(currentPage - 1)}
              >
                Previous
              </Button>
              
              <span className="text-sm">
                Page {currentPage} of {totalPages}
              </span>
              
              <Button
                variant="outline"
                size="sm"
                disabled={currentPage === totalPages}
                onClick={() => setCurrentPage(currentPage + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}