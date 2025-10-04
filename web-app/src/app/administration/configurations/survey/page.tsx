'use client';

import React, { useState, useEffect, useCallback, useMemo, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Plus, Search, FileDown, Upload, Eye, Edit, Trash2, MoreVertical, ToggleLeft, ToggleRight, X, FileText, ChevronDown, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu';
import { PaginationControls } from '@/components/ui/pagination-controls';
import { Badge } from '@/components/ui/badge';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { toast } from 'sonner';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { surveyService } from '@/services/surveyService';
import {
  ISurvey,
  SurveyListRequest,
  PagedResponse
} from '@/types/survey.types';

export default function SurveyManagementPage() {
  const router = useRouter();
  const searchInputRef = useRef<HTMLInputElement>(null);
  
  // Survey Management State
  const [surveys, setSurveys] = useState<ISurvey[]>([]);
  const [filteredSurveys, setFilteredSurveys] = useState<ISurvey[]>([]);
  const [loading, setLoading] = useState(true);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatuses, setSelectedStatuses] = useState<string[]>([]);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [surveyToDelete, setSurveyToDelete] = useState<ISurvey | null>(null);

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

  // Filter data based on search and status
  useEffect(() => {
    let filtered = [...surveys];

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(survey => 
        survey.Code?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        survey.Description?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply status filters
    if (selectedStatuses.length > 0) {
      filtered = filtered.filter(survey => {
        const isActive = survey.IsActive ?? false;
        if (selectedStatuses.includes('Active') && isActive) return true;
        if (selectedStatuses.includes('Inactive') && !isActive) return true;
        return false;
      });
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredSurveys(paginatedData);
    setTotalCount(filtered.length);
  }, [surveys, searchTerm, selectedStatuses, currentPage, pageSize]);

  // Fetch surveys
  const fetchSurveys = useCallback(async () => {
    setLoading(true);
    try {
      const request: SurveyListRequest = {
        pageNumber: 1,
        pageSize: 1000, // Get all for client-side filtering
        filterCriterias: [],
        sortCriterias: [{ sortParameter: 'Code', direction: 0 }],
        isCountRequired: true
      };

      const response: PagedResponse<ISurvey> = await surveyService.getAllSurveys(request);
      setSurveys(response.pagedData || []);
    } catch (error) {
      console.error('Error fetching surveys:', error);
      toast.error('Failed to fetch surveys. Please try again.');
      setSurveys([]);
    } finally {
      setLoading(false);
    }
  }, []);

  // Effect to fetch surveys
  useEffect(() => {
    fetchSurveys();
  }, [fetchSurveys]);

  // Handle delete survey
  const handleDelete = async () => {
    if (!surveyToDelete) return;

    // Check if survey is active
    if (surveyToDelete.IsActive) {
      toast.error('Cannot delete an active survey. Please deactivate it first.');
      return;
    }

    try {
      const uid = surveyToDelete.UID || surveyToDelete.Code;
      if (uid) {
        await surveyService.deleteSurvey(uid);
        toast.success('Survey deleted successfully');
        setDeleteDialogOpen(false);
        setSurveyToDelete(null);
        fetchSurveys();
      }
    } catch (error) {
      console.error('Error deleting survey:', error);
      toast.error('Failed to delete survey. Please try again.');
    }
  };

  // Handle toggle active/inactive status
  const handleToggleStatus = async (survey: ISurvey) => {
    try {
      // Parse the SurveyData if it exists and is a string
      let parsedSurveyData: any = {};
      if (survey.SurveyData) {
        try {
          parsedSurveyData = typeof survey.SurveyData === 'string' 
            ? JSON.parse(survey.SurveyData) 
            : survey.SurveyData;
        } catch (e) {
          console.error('Error parsing survey data:', e);
        }
      }

      // Prepare the survey data for update
      const updatedSurvey = {
        UID: survey.UID,
        Code: survey.Code,
        Title: parsedSurveyData.title || '',
        Description: survey.Description || '',
        StartDate: survey.StartDate,
        EndDate: survey.EndDate,
        IsActive: !survey.IsActive,
        Sections: parsedSurveyData.sections || []
      };
      
      await surveyService.createOrUpdateSurvey(updatedSurvey);
      toast.success(`Survey ${updatedSurvey.IsActive ? 'activated' : 'deactivated'} successfully`);
      fetchSurveys();
    } catch (error) {
      console.error('Error toggling survey status:', error);
      toast.error('Failed to update survey status. Please try again.');
    }
  };

  // Handle export
  const handleExport = useCallback(() => {
    const csvContent = [
      ["Code", "Description", "Start Date", "End Date", "Status"],
      ...filteredSurveys.map(survey => [
        survey.Code,
        survey.Description || '',
        survey.StartDate ? new Date(survey.StartDate).toLocaleDateString() : '',
        survey.EndDate ? new Date(survey.EndDate).toLocaleDateString() : '',
        survey.IsActive ? 'Active' : 'Inactive'
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `surveys_${new Date().toISOString()}.csv`;
    a.click();
    toast.success('Surveys exported successfully');
  }, [filteredSurveys]);

  // Handle page change
  const handlePageChange = (page: number) => {
    setCurrentPage(page);
  };

  // Handle page size change
  const handlePageSizeChange = (size: number) => {
    setPageSize(size);
    setCurrentPage(1);
  };

  // Get status badge variant
  const getStatusBadgeVariant = useCallback((isActive: boolean) => {
    return isActive ? 'default' : 'secondary';
  }, []);

  // Pagination helpers
  const totalPages = useMemo(() => {
    return Math.ceil(totalCount / pageSize);
  }, [totalCount, pageSize]);

  return (
    <div className="container mx-auto p-6">
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Survey Management</h2>
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
            onClick={() => router.push('/administration/configurations/survey/edit/new')}
            size="sm"
          >
            <Plus className="h-4 w-4 mr-2" />
            Create Survey
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
                placeholder="Search by code or description... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Status Filter Dropdown */}
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
                <DropdownMenuCheckboxItem
                  checked={selectedStatuses.includes("Active")}
                  onCheckedChange={(checked) => {
                    setSelectedStatuses(prev => 
                      checked 
                        ? [...prev, "Active"]
                        : prev.filter(s => s !== "Active")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Active
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedStatuses.includes("Inactive")}
                  onCheckedChange={(checked) => {
                    setSelectedStatuses(prev => 
                      checked 
                        ? [...prev, "Inactive"]
                        : prev.filter(s => s !== "Inactive")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Inactive
                </DropdownMenuCheckboxItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedStatuses.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedStatuses([]);
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

      {/* Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="pl-6">Code</TableHead>
                  <TableHead>Description</TableHead>
                  <TableHead>Start Date</TableHead>
                  <TableHead>End Date</TableHead>
                  <TableHead className="text-center">Status</TableHead>
                  <TableHead className="text-right pr-6">Actions</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  Array.from({ length: 10 }).map((_, index) => (
                    <TableRow key={index}>
                      <TableCell className="pl-6">
                        <Skeleton className="h-5 w-24" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-48" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-28" />
                      </TableCell>
                      <TableCell>
                        <Skeleton className="h-5 w-28" />
                      </TableCell>
                      <TableCell className="text-center">
                        <Skeleton className="h-6 w-16 mx-auto rounded-full" />
                      </TableCell>
                      <TableCell className="text-right pr-6">
                        <div className="flex justify-end">
                          <Skeleton className="h-8 w-8 rounded" />
                        </div>
                      </TableCell>
                    </TableRow>
                  ))
                ) : filteredSurveys.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="h-24 text-center">
                      <div className="flex flex-col items-center justify-center py-8">
                        <FileText className="h-12 w-12 text-gray-400 mb-3" />
                        <p className="text-sm font-medium text-gray-900">No surveys found</p>
                        <p className="text-sm text-gray-500 mt-1">
                          {searchTerm || selectedStatuses.length > 0 
                            ? "Try adjusting your search or filters" 
                            : "Click 'Create Survey' to create your first survey"}
                        </p>
                      </div>
                    </TableCell>
                  </TableRow>
                ) : (
                  filteredSurveys.map((survey, index) => (
                    <TableRow key={survey.UID || survey.Code || `survey-${index}`}>
                      <TableCell className="font-medium pl-6">{survey.Code}</TableCell>
                      <TableCell>{survey.Description}</TableCell>
                      <TableCell>{survey.StartDate ? new Date(survey.StartDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell>{survey.EndDate ? new Date(survey.EndDate).toLocaleDateString() : '-'}</TableCell>
                      <TableCell className="text-center">
                        <Badge variant={getStatusBadgeVariant(survey.IsActive ?? false)}>
                          {survey.IsActive ? 'Active' : 'Inactive'}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-right pr-6">
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" className="h-8 w-8 p-0">
                              <span className="sr-only">Open menu</span>
                              <MoreVertical className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuLabel>Actions</DropdownMenuLabel>
                            <DropdownMenuItem
                              onClick={() => {
                                const uid = survey.UID || survey.Code;
                                router.push(`/administration/configurations/survey/view/${uid}`);
                              }}
                              className="cursor-pointer"
                            >
                              <Eye className="mr-2 h-4 w-4" />
                              View Details
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => {
                                const uid = survey.UID || survey.Code;
                                router.push(`/administration/configurations/survey/edit/${uid}`);
                              }}
                              className="cursor-pointer"
                            >
                              <Edit className="mr-2 h-4 w-4" />
                              Edit Survey
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => handleToggleStatus(survey)}
                              className="cursor-pointer"
                            >
                              {survey.IsActive ? (
                                <>
                                  <ToggleLeft className="mr-2 h-4 w-4" />
                                  Deactivate Survey
                                </>
                              ) : (
                                <>
                                  <ToggleRight className="mr-2 h-4 w-4" />
                                  Activate Survey
                                </>
                              )}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              onClick={() => {
                                setSurveyToDelete(survey);
                                setDeleteDialogOpen(true);
                              }}
                              className="cursor-pointer text-red-600 focus:text-red-600"
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete Survey
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
                itemName="surveys"
              />
            </div>
          )}
        </CardContent>
      </Card>


      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>
              {surveyToDelete?.IsActive ? 'Cannot Delete Active Survey' : 'Delete Survey'}
            </AlertDialogTitle>
            <AlertDialogDescription>
              {surveyToDelete?.IsActive ? (
                <div className="space-y-2">
                  <p className="text-amber-600 font-medium">
                    This survey is currently active and cannot be deleted.
                  </p>
                  <p>
                    Please deactivate the survey "{surveyToDelete?.Code}" first before attempting to delete it.
                  </p>
                </div>
              ) : (
                <p>
                  Are you sure you want to delete survey "{surveyToDelete?.Code}"? This action cannot be undone.
                </p>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>
              {surveyToDelete?.IsActive ? 'Close' : 'Cancel'}
            </AlertDialogCancel>
            {!surveyToDelete?.IsActive && (
              <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground">
                Delete
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}