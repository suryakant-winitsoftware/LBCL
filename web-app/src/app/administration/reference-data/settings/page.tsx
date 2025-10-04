"use client";

import React, { useState, useEffect, useRef } from 'react';
import {
  Card,
  CardContent,
} from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Switch } from '@/components/ui/switch';
import { Label } from '@/components/ui/label';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuCheckboxItem,
} from '@/components/ui/dropdown-menu';
import { Skeleton } from '@/components/ui/skeleton';
import { PaginationControls } from '@/components/ui/pagination-controls';
import {
  Plus,
  Edit,
  Trash2,
  Search,
  Settings as SettingsIcon,
  Eye,
  FileDown,
  Upload,
  X,
  ChevronDown,
  Filter,
  MoreVertical,
  Shield,
} from 'lucide-react';

import {
  settingsService,
  Setting,
  CreateSettingRequest,
  UpdateSettingRequest,
  PagingRequest,
  FilterCriteria,
  SortCriteria
} from '@/services/settings.service';
import { SettingModal } from './components/setting-modal';

const SETTING_TYPES = ['Global', 'UI', 'FR', 'Test'] as const;
const DATA_TYPES = ['String', 'Int', 'Boolean'] as const;

export default function SettingsManagementPage() {
  const searchInputRef = useRef<HTMLInputElement>(null);
  const [settings, setSettings] = useState<Setting[]>([]);
  const [filteredSettings, setFilteredSettings] = useState<Setting[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Pagination
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Filters and Search
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTypes, setSelectedTypes] = useState<string[]>([]);
  const [selectedDataTypes, setSelectedDataTypes] = useState<string[]>([]);
  const [selectedEditableStatus, setSelectedEditableStatus] = useState<string[]>([]);

  // Modal states
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isViewModalOpen, setIsViewModalOpen] = useState(false);
  const [selectedSetting, setSelectedSetting] = useState<Setting | null>(null);
  
  // Delete confirmation
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [settingToDelete, setSettingToDelete] = useState<Setting | null>(null);

  const [adminMode, setAdminMode] = useState(false);

  useEffect(() => {
    fetchSettings();
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
    let filtered = [...settings];

    // Apply search filter
    if (searchTerm) {
      filtered = filtered.filter(setting => 
        setting.name?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        setting.value?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        setting.uid?.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Apply type filter
    if (selectedTypes.length > 0) {
      filtered = filtered.filter(setting => 
        selectedTypes.includes(setting.type)
      );
    }

    // Apply data type filter
    if (selectedDataTypes.length > 0) {
      filtered = filtered.filter(setting => 
        selectedDataTypes.includes(setting.dataType)
      );
    }

    // Apply editable status filter
    if (selectedEditableStatus.length > 0) {
      filtered = filtered.filter(setting => {
        if (selectedEditableStatus.includes('Editable') && setting.isEditable) return true;
        if (selectedEditableStatus.includes('Read Only') && !setting.isEditable) return true;
        return false;
      });
    }

    // Apply pagination
    const startIndex = (currentPage - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedData = filtered.slice(startIndex, endIndex);
    
    setFilteredSettings(paginatedData);
    setTotalCount(filtered.length);
  }, [settings, searchTerm, selectedTypes, selectedDataTypes, selectedEditableStatus, currentPage, pageSize]);

  const fetchSettings = async () => {
    try {
      setLoading(true);
      setError(null);

      const request: PagingRequest = {
        pageNumber: 1,
        pageSize: 1000, // Get all for client-side filtering
        sortCriterias: [
          { sortParameter: 'Type', direction: 'Asc' },
          { sortParameter: 'Name', direction: 'Asc' }
        ],
        filterCriterias: [],
        isCountRequired: true
      };

      const response = await settingsService.getAllSettings(request);
      setSettings(response.pagedData);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch settings');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateSetting = async (settingData: CreateSettingRequest) => {
    try {
      await settingsService.createSetting(settingData);
      setIsCreateModalOpen(false);
      await fetchSettings();
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to create setting');
    }
  };

  const handleUpdateSetting = async (settingData: UpdateSettingRequest) => {
    try {
      await settingsService.updateSetting(settingData);
      setIsEditModalOpen(false);
      setSelectedSetting(null);
      await fetchSettings();
    } catch (err) {
      throw new Error(err instanceof Error ? err.message : 'Failed to update setting');
    }
  };

  const handleDeleteSetting = async (setting: Setting) => {
    try {
      await settingsService.deleteSetting(setting.uid);
      setDeleteConfirmOpen(false);
      setSettingToDelete(null);
      await fetchSettings();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete setting');
    }
  };

  const openDeleteConfirm = (setting: Setting) => {
    setSettingToDelete(setting);
    setDeleteConfirmOpen(true);
  };

  const openEditModal = (setting: Setting) => {
    setSelectedSetting(setting);
    setIsEditModalOpen(true);
  };

  const openViewModal = (setting: Setting) => {
    setSelectedSetting(setting);
    setIsViewModalOpen(true);
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
      ["Name", "Type", "Value", "Data Type", "Editable", "UID"],
      ...filteredSettings.map(setting => [
        setting.name,
        setting.type,
        setting.value,
        setting.dataType,
        setting.isEditable ? 'Yes' : 'No',
        setting.uid
      ])
    ].map(row => row.join(",")).join("\n");

    const blob = new Blob([csvContent], { type: "text/csv" });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `settings_${new Date().toISOString()}.csv`;
    a.click();
  };

  const getDataTypeBadgeVariant = (dataType: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (dataType) {
      case 'Boolean': return 'outline';
      case 'Int': return 'secondary';
      case 'String': return 'default';
      default: return 'outline';
    }
  };

  const getTypeBadgeVariant = (type: string): "default" | "secondary" | "destructive" | "outline" => {
    switch (type) {
      case 'Global': return 'default';
      case 'UI': return 'secondary';
      case 'FR': return 'outline';
      case 'Test': return 'outline';
      default: return 'outline';
    }
  };

  return (
    <div className="container mx-auto p-6">
      {/* Header with actions */}
      <div className="flex justify-between items-center mb-4">
        <h2 className="text-2xl font-bold">Settings Management</h2>
        <div className="flex gap-2">
          <div className="flex items-center space-x-2 border rounded-lg px-3 py-1.5 bg-orange-50 border-orange-200">
            <Switch
              id="admin-mode"
              checked={adminMode}
              onCheckedChange={setAdminMode}
              className="data-[state=checked]:bg-orange-500 h-4 w-8"
            />
            <Label 
              htmlFor="admin-mode" 
              className="text-sm font-medium cursor-pointer text-orange-700"
            >
              Admin Mode
            </Label>
          </div>
          <Button variant="outline" size="sm">
            <Upload className="h-4 w-4 mr-2" />
            Import
          </Button>
          <Button variant="outline" size="sm" onClick={handleExport}>
            <FileDown className="h-4 w-4 mr-2" />
            Export
          </Button>
          <Button onClick={() => setIsCreateModalOpen(true)} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Setting
          </Button>
        </div>
      </div>

      {error && (
        <Card className="border-red-200 bg-red-50 mb-4">
          <CardContent className="pt-4">
            <p className="text-red-600 text-sm">{error}</p>
          </CardContent>
        </Card>
      )}

      {adminMode && (
        <Card className="border-orange-300 bg-orange-50 mb-4">
          <CardContent className="pt-4">
            <div className="flex items-start gap-2">
              <Shield className="h-5 w-5 text-orange-600 mt-0.5" />
              <div>
                <p className="text-orange-800 font-medium text-sm">Admin Mode Active</p>
                <p className="text-orange-700 text-sm mt-1">
                  You can now edit system-protected settings. Please be careful as changes may affect system behavior.
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Search Bar */}
      <Card className="shadow-sm border-gray-200 mb-4">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchInputRef}
                placeholder="Search by name, value or UID... (Ctrl+F)"
                value={searchTerm}
                onChange={(e) => {
                  setSearchTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>
            
            {/* Type Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Type
                  {selectedTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {SETTING_TYPES.map(type => (
                  <DropdownMenuCheckboxItem
                    key={type}
                    checked={selectedTypes.includes(type)}
                    onCheckedChange={(checked) => {
                      setSelectedTypes(prev => 
                        checked 
                          ? [...prev, type]
                          : prev.filter(t => t !== type)
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {type}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Data Type Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Data Type
                  {selectedDataTypes.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedDataTypes.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Data Type</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {DATA_TYPES.map(dataType => (
                  <DropdownMenuCheckboxItem
                    key={dataType}
                    checked={selectedDataTypes.includes(dataType)}
                    onCheckedChange={(checked) => {
                      setSelectedDataTypes(prev => 
                        checked 
                          ? [...prev, dataType]
                          : prev.filter(dt => dt !== dataType)
                      );
                      setCurrentPage(1);
                    }}
                  >
                    {dataType}
                  </DropdownMenuCheckboxItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Editable Status Filter */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Shield className="h-4 w-4 mr-2" />
                  Editable
                  {selectedEditableStatus.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedEditableStatus.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <DropdownMenuLabel>Filter by Editable Status</DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuCheckboxItem
                  checked={selectedEditableStatus.includes("Editable")}
                  onCheckedChange={(checked) => {
                    setSelectedEditableStatus(prev => 
                      checked 
                        ? [...prev, "Editable"]
                        : prev.filter(s => s !== "Editable")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Editable
                </DropdownMenuCheckboxItem>
                <DropdownMenuCheckboxItem
                  checked={selectedEditableStatus.includes("Read Only")}
                  onCheckedChange={(checked) => {
                    setSelectedEditableStatus(prev => 
                      checked 
                        ? [...prev, "Read Only"]
                        : prev.filter(s => s !== "Read Only")
                    );
                    setCurrentPage(1);
                  }}
                >
                  Read Only
                </DropdownMenuCheckboxItem>
              </DropdownMenuContent>
            </DropdownMenu>
            
            {/* Clear All Button */}
            {(searchTerm || selectedTypes.length > 0 || selectedDataTypes.length > 0 || selectedEditableStatus.length > 0) && (
              <Button
                variant="outline"
                onClick={() => {
                  setSearchTerm("");
                  setSelectedTypes([]);
                  setSelectedDataTypes([]);
                  setSelectedEditableStatus([]);
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
                <TableHead className="pl-6">Name</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Value</TableHead>
                <TableHead>Data Type</TableHead>
                <TableHead className="text-center">Editable</TableHead>
                <TableHead>Modified</TableHead>
                <TableHead className="text-right pr-6">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                Array.from({ length: 10 }).map((_, i) => (
                  <TableRow key={i}>
                    <TableCell className="pl-6">
                      <Skeleton className="h-5 w-32" />
                      <Skeleton className="h-3 w-24 mt-1" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-6 w-16 rounded-full" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-5 w-40" />
                    </TableCell>
                    <TableCell>
                      <Skeleton className="h-6 w-20 rounded-full" />
                    </TableCell>
                    <TableCell className="text-center">
                      <Skeleton className="h-6 w-12 mx-auto rounded-full" />
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
              ) : filteredSettings.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={7} className="h-24 text-center">
                    <div className="flex flex-col items-center justify-center py-8">
                      <SettingsIcon className="h-12 w-12 text-gray-400 mb-3" />
                      <p className="text-sm font-medium text-gray-900">No settings found</p>
                      <p className="text-sm text-gray-500 mt-1">
                        {searchTerm || selectedTypes.length > 0 || selectedDataTypes.length > 0 || selectedEditableStatus.length > 0
                          ? "Try adjusting your search or filters" 
                          : "Click 'Add Setting' to create your first setting"}
                      </p>
                    </div>
                  </TableCell>
                </TableRow>
              ) : (
                filteredSettings.map((setting, index) => (
                  <TableRow key={`${setting.uid}-${setting.id}-${index}`}>
                    <TableCell className="font-medium pl-6">
                      <div>
                        <div>{setting.name}</div>
                        <div className="text-xs text-gray-500">{setting.uid}</div>
                      </div>
                    </TableCell>
                    <TableCell>
                      {setting.type === 'Global' ? (
                        <Badge className="bg-blue-100 text-blue-800 hover:bg-blue-100">
                          {setting.type}
                        </Badge>
                      ) : setting.type === 'UI' ? (
                        <Badge className="bg-green-100 text-green-800 hover:bg-green-100">
                          {setting.type}
                        </Badge>
                      ) : setting.type === 'FR' ? (
                        <Badge className="bg-purple-100 text-purple-800 hover:bg-purple-100">
                          {setting.type}
                        </Badge>
                      ) : setting.type === 'Test' ? (
                        <Badge className="bg-orange-100 text-orange-800 hover:bg-orange-100">
                          {setting.type}
                        </Badge>
                      ) : (
                        <Badge variant="outline">{setting.type}</Badge>
                      )}
                    </TableCell>
                    <TableCell className="max-w-xs">
                      <div className="truncate" title={setting.value}>
                        {setting.value}
                      </div>
                    </TableCell>
                    <TableCell>
                      {setting.dataType === 'String' ? (
                        <Badge className="bg-indigo-100 text-indigo-800 hover:bg-indigo-100">
                          {setting.dataType}
                        </Badge>
                      ) : setting.dataType === 'Int' ? (
                        <Badge className="bg-cyan-100 text-cyan-800 hover:bg-cyan-100">
                          {setting.dataType}
                        </Badge>
                      ) : setting.dataType === 'Boolean' ? (
                        <Badge className="bg-amber-100 text-amber-800 hover:bg-amber-100">
                          {setting.dataType}
                        </Badge>
                      ) : (
                        <Badge variant="outline">{setting.dataType}</Badge>
                      )}
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-2">
                        <Badge variant={setting.isEditable ? 'default' : 'secondary'}>
                          {setting.isEditable ? 'Yes' : 'No'}
                        </Badge>
                        {adminMode && !setting.isEditable && (
                          <Badge variant="outline" className="text-orange-600 border-orange-300 text-xs">
                            Admin
                          </Badge>
                        )}
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="text-sm">
                        {new Date(setting.serverModifiedTime).toLocaleDateString()}
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
                          <DropdownMenuItem onClick={() => openViewModal(setting)}>
                            <Eye className="mr-2 h-4 w-4" />
                            View
                          </DropdownMenuItem>
                          {(setting.isEditable || adminMode) && (
                            <>
                              <DropdownMenuItem 
                                onClick={() => openEditModal(setting)}
                                className={adminMode && !setting.isEditable ? "text-orange-600" : ""}
                              >
                                <Edit className="mr-2 h-4 w-4" />
                                {adminMode && !setting.isEditable ? "Edit (Admin)" : "Edit"}
                              </DropdownMenuItem>
                            </>
                          )}
                          {adminMode && (
                            <>
                              <DropdownMenuSeparator />
                              <DropdownMenuItem
                                onClick={() => openDeleteConfirm(setting)}
                                className="text-red-600 focus:text-red-600"
                              >
                                <Trash2 className="mr-2 h-4 w-4" />
                                Delete
                              </DropdownMenuItem>
                            </>
                          )}
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
                itemName="settings"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create Modal */}
      <SettingModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onSubmit={handleCreateSetting}
        mode="create"
        title="Create New Setting"
      />

      {/* Edit Modal */}
      <SettingModal
        isOpen={isEditModalOpen}
        onClose={() => {
          setIsEditModalOpen(false);
          setSelectedSetting(null);
        }}
        onSubmit={handleUpdateSetting}
        mode="edit"
        title="Edit Setting"
        initialData={selectedSetting}
      />

      {/* View Modal */}
      <SettingModal
        isOpen={isViewModalOpen}
        onClose={() => {
          setIsViewModalOpen(false);
          setSelectedSetting(null);
        }}
        mode="view"
        title="View Setting Details"
        initialData={selectedSetting}
      />

      {/* Delete Confirmation */}
      <AlertDialog open={deleteConfirmOpen} onOpenChange={setDeleteConfirmOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Setting</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete the setting "{settingToDelete?.name}"?
              This action cannot be undone.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => settingToDelete && handleDeleteSetting(settingToDelete)}
              className="bg-red-600 hover:bg-red-700"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}