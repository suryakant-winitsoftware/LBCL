"use client";

import { useState, useEffect, useRef } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from "@/components/ui/dialog";
import { useToast } from "@/components/ui/use-toast";
import {
  Plus,
  Search,
  Edit,
  Trash2,
  RefreshCw,
  Tags,
  Database,
  Filter,
  ChevronDown,
  X,
  MoreHorizontal,
  Eye
} from "lucide-react";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuItem
} from "@/components/ui/dropdown-menu";
import { DataTable } from "@/components/ui/data-table";
import { PaginationControls } from "@/components/ui/pagination-controls";
import { Textarea } from "@/components/ui/textarea";
import { formatDateToDayMonthYear } from "@/utils/date-formatter";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from "@/components/ui/alert-dialog";

interface SKUClass {
  Id?: number;
  UID: string;
  CompanyUID: string;
  ClassName: string;
  Description: string;
  ClassLabel: string;
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedTime?: string;
  ModifiedTime?: string;
  ServerAddTime?: string;
  ServerModifiedTime?: string;
  IsSelected?: boolean;
}

export default function ClassManagementPage() {
  const { toast } = useToast();
  const searchRef = useRef<HTMLDivElement>(null);
  const [showSearchHighlight, setShowSearchHighlight] = useState(false);

  // Data states for SKU Classes
  const [skuClasses, setSKUClasses] = useState<SKUClass[]>([]);
  const [loadingClasses, setLoadingClasses] = useState(false);
  const [searchClassTerm, setSearchClassTerm] = useState("");
  const [classDialogOpen, setClassDialogOpen] = useState(false);
  const [deleteClassDialogOpen, setDeleteClassDialogOpen] = useState(false);
  const [selectedClass, setSelectedClass] = useState<SKUClass | null>(null);
  const [isEditClassMode, setIsEditClassMode] = useState(false);
  const [classFormData, setClassFormData] = useState<Partial<SKUClass>>({
    CompanyUID: "EPIC01",
    ClassName: "",
    Description: "",
    ClassLabel: "",
    CreatedBy: "ADMIN",
    ModifiedBy: "ADMIN"
  });

  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  // Filter states
  const [selectedCompanies, setSelectedCompanies] = useState<string[]>([]);

  useEffect(() => {
    loadSKUClasses();
  }, []);

  // Ctrl+F search functionality
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "f") {
        e.preventDefault();
        searchRef.current?.querySelector("input")?.focus();
        setShowSearchHighlight(true);
        setTimeout(() => setShowSearchHighlight(false), 2000);
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  // SKU Class Functions
  const loadSKUClasses = async () => {
    if (loadingClasses) return; // Prevent duplicate calls
    setLoadingClasses(true);
    try {
      // Use the SelectAllSKUClassDetails endpoint
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKUClass/SelectAllSKUClassDetails`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            pageNumber: 1,
            pageSize: 1000,
            filterCriterias: [],
            isCountRequired: true
          })
        }
      );

      if (response.ok) {
        const result = await response.json();
        if (result.IsSuccess && result.Data?.PagedData) {
          setSKUClasses(result.Data.PagedData);
          return;
        } else if (
          result.IsSuccess &&
          result.Data &&
          Array.isArray(result.Data)
        ) {
          setSKUClasses(result.Data);
          return;
        }
      }

      // Fallback: Try to get classes from SKUClassGroups
      const groupsResponse = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKUClassGroup/SelectAllSKUClassGroupDetails`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json"
          },
          body: JSON.stringify({
            pageNumber: 1,
            pageSize: 1000,
            filterCriterias: [],
            isCountRequired: true
          })
        }
      );

      if (groupsResponse.ok) {
        const groupsResult = await groupsResponse.json();
        if (groupsResult.IsSuccess && groupsResult.Data?.PagedData) {
          const uniqueClassUIDs = [
            ...new Set(
              groupsResult.Data.PagedData.map((g: any) => g.SKUClassUID).filter(
                Boolean
              )
            )
          ];

          const fetchedClasses: SKUClass[] = [];
          for (const uid of uniqueClassUIDs) {
            try {
              const classResponse = await fetch(
                `${
                  process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
                }/SKUClass/GetSKUClassByUID?UID=${uid}`,
                {
                  method: "GET",
                  headers: {
                    Authorization: `Bearer ${localStorage.getItem(
                      "auth_token"
                    )}`,
                    "Content-Type": "application/json"
                  }
                }
              );

              if (classResponse.ok) {
                const classResult = await classResponse.json();
                if (classResult.IsSuccess && classResult.Data) {
                  fetchedClasses.push(classResult.Data);
                }
              }
            } catch (error) {
              console.error(`Failed to fetch class ${uid}:`, error);
            }
          }

          const storedClassUIDs = JSON.parse(
            localStorage.getItem("skuClassUIDs") || "[]"
          );
          for (const uid of storedClassUIDs) {
            if (!fetchedClasses.find((c) => c.UID === uid)) {
              try {
                const classResponse = await fetch(
                  `${
                    process.env.NEXT_PUBLIC_API_URL ||
                    "http://localhost:8000/api"
                  }/SKUClass/GetSKUClassByUID?UID=${uid}`,
                  {
                    method: "GET",
                    headers: {
                      Authorization: `Bearer ${localStorage.getItem(
                        "auth_token"
                      )}`,
                      "Content-Type": "application/json"
                    }
                  }
                );

                if (classResponse.ok) {
                  const classResult = await classResponse.json();
                  if (classResult.IsSuccess && classResult.Data) {
                    fetchedClasses.push(classResult.Data);
                  }
                }
              } catch (error) {
                console.error(`Failed to fetch stored class ${uid}:`, error);
              }
            }
          }

          setSKUClasses(fetchedClasses);
        }
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to fetch SKU Classes",
        variant: "destructive"
      });
    } finally {
      setLoadingClasses(false);
    }
  };

  const handleCreateClass = async () => {
    try {
      const uid = `CLASS_${Date.now()}`;
      const now = new Date().toISOString();
      const payload = {
        ...classFormData,
        UID: uid,
        CreatedTime: now,
        ModifiedTime: now,
        CreatedBy:
          classFormData.CreatedBy ||
          localStorage.getItem("username") ||
          "ADMIN",
        ModifiedBy:
          classFormData.ModifiedBy ||
          localStorage.getItem("username") ||
          "ADMIN"
      };

      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKUClass/CreateSKUClass`,
        {
          method: "POST",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json"
          },
          body: JSON.stringify(payload)
        }
      );

      if (response.ok) {
        const result = await response.json();
        if (result.IsSuccess) {
          toast({
            title: "Success",
            description: "SKU Class created successfully"
          });
          setClassDialogOpen(false);
          resetClassForm();

          const newClassResponse = await fetch(
            `${
              process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
            }/SKUClass/GetSKUClassByUID?UID=${uid}`,
            {
              headers: {
                Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
                "Content-Type": "application/json"
              }
            }
          );
          if (newClassResponse.ok) {
            const newClassResult = await newClassResponse.json();
            if (newClassResult.IsSuccess && newClassResult.Data) {
              setSKUClasses([...skuClasses, newClassResult.Data]);

              const storedUIDs = JSON.parse(
                localStorage.getItem("skuClassUIDs") || "[]"
              );
              if (!storedUIDs.includes(uid)) {
                storedUIDs.push(uid);
                localStorage.setItem(
                  "skuClassUIDs",
                  JSON.stringify(storedUIDs)
                );
              }
            }
          }
        }
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to create SKU Class",
        variant: "destructive"
      });
    }
  };

  const handleUpdateClass = async () => {
    if (!selectedClass) return;

    try {
      const payload = {
        ...selectedClass,
        ...classFormData,
        UID: selectedClass.UID,
        ModifiedTime: new Date().toISOString(),
        ModifiedBy: localStorage.getItem("username") || "ADMIN",
        CreatedTime: selectedClass.CreatedTime,
        CreatedBy: selectedClass.CreatedBy
      };

      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKUClass/UpdateSKUClass`,
        {
          method: "PUT",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json"
          },
          body: JSON.stringify(payload)
        }
      );

      if (response.ok) {
        const result = await response.json();
        if (result.IsSuccess) {
          toast({
            title: "Success",
            description: "SKU Class updated successfully"
          });
          setClassDialogOpen(false);
          resetClassForm();
          setSKUClasses(
            skuClasses.map((c) => (c.UID === selectedClass.UID ? payload : c))
          );
        }
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to update SKU Class",
        variant: "destructive"
      });
    }
  };

  const handleDeleteClass = async () => {
    if (!selectedClass) return;

    try {
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/SKUClass/DeleteSKUClass?UID=${selectedClass.UID}`,
        {
          method: "DELETE",
          headers: {
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`,
            "Content-Type": "application/json"
          }
        }
      );

      if (response.ok) {
        const result = await response.json();
        if (result.IsSuccess) {
          toast({
            title: "Success",
            description: "SKU Class deleted successfully"
          });
          setSKUClasses(skuClasses.filter((c) => c.UID !== selectedClass.UID));
          setDeleteClassDialogOpen(false);
          setSelectedClass(null);
        }
      }
    } catch (error) {
      toast({
        title: "Error",
        description: "Failed to delete SKU Class",
        variant: "destructive"
      });
    }
  };

  const resetClassForm = () => {
    setClassFormData({
      CompanyUID: "EPIC01",
      ClassName: "",
      Description: "",
      ClassLabel: "",
      CreatedBy: "ADMIN",
      ModifiedBy: "ADMIN"
    });
    setSelectedClass(null);
    setIsEditClassMode(false);
  };

  const openCreateClassDialog = () => {
    resetClassForm();
    setIsEditClassMode(false);
    setClassDialogOpen(true);
  };

  const openEditClassDialog = (skuClass: SKUClass) => {
    setSelectedClass(skuClass);
    setClassFormData({
      CompanyUID: skuClass.CompanyUID,
      ClassName: skuClass.ClassName,
      Description: skuClass.Description,
      ClassLabel: skuClass.ClassLabel,
      ModifiedBy: "ADMIN"
    });
    setIsEditClassMode(true);
    setClassDialogOpen(true);
  };

  const openDeleteClassDialog = (skuClass: SKUClass) => {
    setSelectedClass(skuClass);
    setDeleteClassDialogOpen(true);
  };

  // Get unique values for filters
  const uniqueCompanies = Array.from(
    new Set(skuClasses.map((c) => c.CompanyUID).filter(Boolean))
  );

  // Filter and paginate data
  const filteredClasses = skuClasses.filter((c) => {
    const matchesSearch =
      !searchClassTerm ||
      c.ClassName?.toLowerCase().includes(searchClassTerm.toLowerCase()) ||
      c.Description?.toLowerCase().includes(searchClassTerm.toLowerCase()) ||
      c.ClassLabel?.toLowerCase().includes(searchClassTerm.toLowerCase());

    const matchesCompany =
      selectedCompanies.length === 0 ||
      selectedCompanies.includes(c.CompanyUID);

    return matchesSearch && matchesCompany;
  });

  const totalPages = Math.ceil(filteredClasses.length / pageSize);
  const paginatedClasses = filteredClasses.slice(
    (currentPage - 1) * pageSize,
    currentPage * pageSize
  );

  // Table columns
  const columns = [
    {
      accessorKey: "ClassName",
      header: () => <div className="pl-6">Class Name</div>,
      cell: ({ row }: any) => (
        <div className="pl-6">
          <span className="font-medium text-sm">{row.original.ClassName}</span>
        </div>
      )
    },
    {
      accessorKey: "ClassLabel",
      header: "Label",
      cell: ({ row }: any) => (
        <Badge
          variant="secondary"
          className="bg-blue-100 text-blue-800 hover:bg-blue-100 text-xs"
        >
          {row.original.ClassLabel}
        </Badge>
      )
    },
    {
      accessorKey: "Description",
      header: "Description",
      cell: ({ row }: any) => (
        <span className="text-sm">{row.original.Description}</span>
      )
    },
    {
      accessorKey: "CompanyUID",
      header: "Company",
      cell: ({ row }: any) => (
        <Badge variant="outline" className="text-xs">
          {row.original.CompanyUID}
        </Badge>
      )
    },
    {
      accessorKey: "CreatedTime",
      header: "Created",
      cell: ({ row }: any) => (
        <span className="text-xs text-gray-500">
          {formatDateToDayMonthYear(row.original.CreatedTime)}
        </span>
      )
    },
    {
      id: "actions",
      header: () => <div className="text-right pr-6">Actions</div>,
      cell: ({ row }: any) => (
        <div className="text-right pr-6">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <span className="sr-only">Open menu</span>
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuLabel>Actions</DropdownMenuLabel>
              <DropdownMenuItem
                onClick={() => {
                  toast({
                    title: "Class Details",
                    description: `${row.original.ClassName} (${row.original.ClassLabel}) - ${row.original.CompanyUID}`
                  });
                }}
                className="cursor-pointer"
              >
                <Eye className="mr-2 h-4 w-4" />
                View Details
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => openEditClassDialog(row.original)}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" />
                Edit
              </DropdownMenuItem>
              <DropdownMenuSeparator />
              <DropdownMenuItem
                onClick={() => openDeleteClassDialog(row.original)}
                className="cursor-pointer text-red-600"
              >
                <Trash2 className="mr-2 h-4 w-4" />
                Delete
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      )
    }
  ];

  return (
    <div className="container mx-auto py-4 space-y-4">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-2xl font-bold">Product Class Management</h1>
        <div className="flex gap-2">
          <Button variant="outline" onClick={loadSKUClasses} size="sm">
            <RefreshCw className="h-4 w-4 mr-2" />
            Refresh
          </Button>
          <Button onClick={openCreateClassDialog} size="sm">
            <Plus className="h-4 w-4 mr-2" />
            Add Class
          </Button>
        </div>
      </div>

      {/* Search and Filters */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="py-3">
          <div className="flex gap-3">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
              <Input
                ref={searchRef}
                placeholder="Search by name, description, or label... (Ctrl+F)"
                value={searchClassTerm}
                onChange={(e) => {
                  setSearchClassTerm(e.target.value);
                  setCurrentPage(1);
                }}
                className="pl-10 border-gray-200 focus:border-primary focus:ring-1 focus:ring-primary/20"
              />
            </div>

            {/* Company Filter Dropdown */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="outline">
                  <Filter className="h-4 w-4 mr-2" />
                  Company
                  {selectedCompanies.length > 0 && (
                    <Badge variant="secondary" className="ml-2">
                      {selectedCompanies.length}
                    </Badge>
                  )}
                  <ChevronDown className="h-4 w-4 ml-2" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56">
                <DropdownMenuLabel>Filter by Company</DropdownMenuLabel>
                <DropdownMenuSeparator />
                {uniqueCompanies.map((company) => (
                  <DropdownMenuCheckboxItem
                    key={company}
                    checked={selectedCompanies.includes(company)}
                    onCheckedChange={(checked) => {
                      if (checked) {
                        setSelectedCompanies([...selectedCompanies, company]);
                      } else {
                        setSelectedCompanies(
                          selectedCompanies.filter((c) => c !== company)
                        );
                      }
                      setCurrentPage(1);
                    }}
                  >
                    {company}
                  </DropdownMenuCheckboxItem>
                ))}
                {selectedCompanies.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem
                      onClick={() => {
                        setSelectedCompanies([]);
                        setCurrentPage(1);
                      }}
                    >
                      <X className="h-4 w-4 mr-2" />
                      Clear Filter
                    </DropdownMenuItem>
                  </>
                )}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </CardContent>
      </Card>

      {/* Table */}
      <Card className="shadow-sm border-gray-200">
        <CardContent className="p-0">
          <DataTable
            columns={columns}
            data={paginatedClasses}
            loading={loadingClasses}
            searchable={false}
            pagination={false}
            noWrapper={true}
          />

          {filteredClasses.length > pageSize && (
            <div className="p-4 border-t">
              <PaginationControls
                currentPage={currentPage}
                totalPages={totalPages}
                pageSize={pageSize}
                totalItems={filteredClasses.length}
                onPageChange={setCurrentPage}
                onPageSizeChange={(newSize) => {
                  setPageSize(newSize);
                  setCurrentPage(1);
                }}
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* SKU Class Create/Edit Dialog */}
      <Dialog open={classDialogOpen} onOpenChange={setClassDialogOpen}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>
              {isEditClassMode ? "Edit SKU Class" : "Create New SKU Class"}
            </DialogTitle>
            <DialogDescription>
              {isEditClassMode
                ? "Update the SKU class information below."
                : "Enter the details for the new SKU class."}
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="className">Class Name *</Label>
              <Input
                id="className"
                value={classFormData.ClassName || ""}
                onChange={(e) =>
                  setClassFormData({
                    ...classFormData,
                    ClassName: e.target.value
                  })
                }
                placeholder="e.g., FOCUS, Premium, Standard"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="classLabel">Class Label *</Label>
              <Input
                id="classLabel"
                value={classFormData.ClassLabel || ""}
                onChange={(e) =>
                  setClassFormData({
                    ...classFormData,
                    ClassLabel: e.target.value
                  })
                }
                placeholder="Display label for the class"
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                value={classFormData.Description || ""}
                onChange={(e) =>
                  setClassFormData({
                    ...classFormData,
                    Description: e.target.value
                  })
                }
                placeholder="Detailed description of the class"
                rows={3}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="companyUID">Company UID</Label>
              <Input
                id="companyUID"
                value={classFormData.CompanyUID || ""}
                onChange={(e) =>
                  setClassFormData({
                    ...classFormData,
                    CompanyUID: e.target.value
                  })
                }
                placeholder="Company identifier"
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setClassDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={isEditClassMode ? handleUpdateClass : handleCreateClass}
              disabled={
                !classFormData.ClassName ||
                !classFormData.ClassLabel ||
                !classFormData.Description
              }
            >
              {isEditClassMode ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* SKU Class Delete Confirmation Dialog */}
      <AlertDialog
        open={deleteClassDialogOpen}
        onOpenChange={setDeleteClassDialogOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the SKU
              Class
              {selectedClass && (
                <>
                  {" "}
                  <strong>{selectedClass.ClassName}</strong> (
                  {selectedClass.ClassLabel})
                </>
              )}
              .
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteClass}
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
