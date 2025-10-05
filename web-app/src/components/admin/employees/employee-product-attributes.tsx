"use client";

import { useState, useEffect } from "react";
import { useToast } from "@/components/ui/use-toast";
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
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import ProductAttributesMultiDropdown from "@/components/sku/ProductAttributesMultiDropdown";
import { employeeService } from "@/services/admin/employee.service";
import { apiService } from "@/services/api";
import {
  Search,
  Users,
  UserCheck,
  Briefcase,
  MapPin,
  Building2,
  Package,
  X,
  Save,
  RefreshCw,
  CheckCircle2
} from "lucide-react";

interface SelectionCriteria {
  hasEmployee: boolean;
  hasSalesTeam: boolean;
  hasLocation: boolean;
  hasOrganization: boolean;
}

interface Employee {
  uid: string;
  code: string;
  name: string;
  email?: string;
  role?: string;
  designation?: string;
  isActive: boolean;
}

export function EmployeeProductAttributes() {
  const { toast } = useToast();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);

  // Product attributes state
  const [selectedAttributes, setSelectedAttributes] = useState<any[]>([]);

  // Selection criteria state - default to Sales Team
  const [selectionCriteria, setSelectionCriteria] = useState<SelectionCriteria>(
    {
      hasEmployee: false,
      hasSalesTeam: true, // Default to sales team
      hasLocation: false,
      hasOrganization: false
    }
  );

  // Selected items state
  const [selectedEmployees, setSelectedEmployees] = useState<string[]>([]);
  const [selectedRole, setSelectedRole] = useState<string>("");
  const [selectedSalesTeams, setSelectedSalesTeams] = useState<string[]>([]);
  const [selectedLocations, setSelectedLocations] = useState<string[]>([]);
  const [selectedOrganizations, setSelectedOrganizations] = useState<string[]>(
    []
  );

  // Data state
  const [employees, setEmployees] = useState<Employee[]>([]);
  const [allEmployees, setAllEmployees] = useState<Employee[]>([]);
  const [availableRoles, setAvailableRoles] = useState<
    { value: string; label: string }[]
  >([]);
  const [loadingRoles, setLoadingRoles] = useState(false);

  // Search state
  const [employeeSearchTerm, setEmployeeSearchTerm] = useState("");

  // Load all employees initially
  const loadAllEmployees = async () => {
    try {
      setLoading(true);
      console.log("Loading all employees...");

      const response = await employeeService.getEmployees({
        pageNumber: 1,
        pageSize: 1000,
        isCountRequired: true,
        filterCriterias: [],
        sortCriterias: []
      });

      console.log("Employee response:", response);

      if (response && response.pagedData) {
        const employeeData = response.pagedData.map((emp: any) => ({
          uid: emp.UID || emp.uid,
          code: emp.Code || emp.code || emp.EmployeeCode,
          name: emp.Name || emp.name || emp.EmployeeName,
          email: emp.Email || emp.email,
          role: emp.Role || emp.role || emp.UserRole,
          designation: emp.Designation || emp.designation,
          isActive: emp.IsActive !== false
        }));

        console.log(`Loaded ${employeeData.length} employees`);
        setAllEmployees(employeeData);
      } else {
        console.log("No employee data in response");
        toast({
          title: "Warning",
          description: "No employees found in the system",
          variant: "default"
        });
      }
    } catch (error) {
      console.error("Error loading employees:", error);
      toast({
        title: "Error",
        description: "Failed to load employees. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  // Load roles from API - same as store-linking
  const loadRoles = async () => {
    if (loadingRoles || availableRoles.length > 0) return;

    try {
      setLoadingRoles(true);
      console.log("Loading roles from API...");

      const roleData = await apiService.post("/Role/SelectAllRoles", {
        pageNumber: 0,
        pageSize: 100,
        isCountRequired: false,
        sortCriterias: [],
        filterCriterias: []
      });

      if (roleData.IsSuccess && roleData.Data?.PagedData) {
        const roles = roleData.Data.PagedData.map((role: any) => ({
          value: role.UID,
          label: role.RoleNameEn || role.Code || role.Name
        }));
        setAvailableRoles(roles);
        console.log(
          `Found ${roles.length} roles:`,
          roles.map((r: any) => r.label)
        );
      } else {
        console.log("No roles found in response");
      }
    } catch (error) {
      console.error("Error loading roles:", error);
      toast({
        title: "Error",
        description: "Failed to load roles. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoadingRoles(false);
    }
  };

  // Load employees based on criteria
  const loadEmployees = async () => {
    if (employees.length > 0 || loading) return;

    // Load all employees if not already loaded
    if (allEmployees.length === 0) {
      await loadAllEmployees();
    }

    // Set employees for display
    setEmployees(allEmployees);
  };

  // Load employees for selected role/sales team - same as store-linking
  const loadEmployeesForRole = async (roleUID: string, orgUID?: string) => {
    if (!roleUID) return;

    setLoading(true);
    try {
      // Use EPIC01 as default orgUID if not provided (same as store-linking)
      const effectiveOrgUID = orgUID || "EPIC01";
      console.log(
        `[SALES_TEAMS] Loading employees for role: ${roleUID}, org: ${effectiveOrgUID}`
      );

      let employees: Employee[] = [];

      if (effectiveOrgUID && roleUID) {
        console.log(
          "🔍 Loading employees for org + role:",
          effectiveOrgUID,
          roleUID
        );

        // Primary: Use organization + role API first (most accurate)
        try {
          console.log("🎯 Using org + role combination API");
          const orgRoleEmployees =
            await employeeService.getEmployeesSelectionItemByRoleUID(
              effectiveOrgUID,
              roleUID
            );

          console.log("🔍 Raw API response:", orgRoleEmployees);

          if (orgRoleEmployees && orgRoleEmployees.length > 0) {
            console.log("🔍 First employee raw data:", orgRoleEmployees[0]);
            employees = orgRoleEmployees.map((item: any) => ({
              uid: item.UID || item.Value || item.uid,
              name: item.Name || item.name || item.Text || item.Label,
              code: item.Code || item.code,
              email: item.Email || item.email,
              role: roleUID,
              designation: item.Designation || item.designation,
              isActive: true
            }));
            console.log(
              `✅ Found ${employees.length} employees for org '${effectiveOrgUID}' + role '${roleUID}'`
            );
          } else {
            // Secondary: Try role-based API
            console.log("🔄 Trying role-based API");
            const roleBasedEmployees =
              await employeeService.getReportsToEmployeesByRoleUID(roleUID);

            if (roleBasedEmployees && roleBasedEmployees.length > 0) {
              employees = roleBasedEmployees.map((emp: any) => ({
                uid: emp.UID || emp.uid,
                name: emp.Name || emp.name || emp.Label,
                code: emp.Code || emp.code,
                email: emp.Email || emp.email,
                role: roleUID,
                designation: emp.Designation || emp.designation,
                isActive: true
              }));
              console.log(`✅ Found ${employees.length} role-based employees`);
            }
          }
        } catch (innerError) {
          console.error(
            "Failed to load employees by org+role, trying fallback:",
            innerError
          );

          // Fallback: Try general employee API with role filter
          const response = await employeeService.getEmployees({
            pageNumber: 1,
            pageSize: 1000,
            isCountRequired: false,
            filterCriterias: [
              {
                Name: "RoleUID",
                Value: roleUID
              }
            ],
            sortCriterias: []
          });

          if (response && response.pagedData) {
            employees = response.pagedData.map((emp: any) => ({
              uid: emp.UID || emp.uid,
              code: emp.Code || emp.code || emp.EmployeeCode,
              name: emp.Name || emp.name || emp.EmployeeName,
              email: emp.Email || emp.email,
              role: emp.Role || emp.role || emp.UserRole,
              designation: emp.Designation || emp.designation,
              isActive: emp.IsActive !== false
            }));
            console.log(
              `✅ Found ${employees.length} employees using fallback`
            );
          }
        }
      }

      setEmployees(employees);
      console.log(`[SALES_TEAMS] Final employee count: ${employees.length}`);
    } catch (error) {
      console.error("[SALES_TEAMS] Failed to load employees:", error);
      setEmployees([]);
      toast({
        title: "Error",
        description: "Failed to load team members. Please try again.",
        variant: "destructive"
      });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    // Load employees and roles on mount since we default to Sales Team
    loadAllEmployees();
    loadRoles();
  }, []);

  // Handle product attributes selection - updated for ProductAttributesMultiDropdown
  const handleAttributesChange = (attributes: any[]) => {
    console.log("Product attributes selected:", attributes);
    setSelectedAttributes(attributes);
  };

  /**
   * Optimize hierarchical selection:
   * - If children have same parent: save only children
   * - If children have different parents: save both parent and children
   */
  const optimizeHierarchicalSelection = (selectedLevels: any[]) => {
    if (selectedLevels.length === 0) return [];

    // Group by level
    const levelGroups: { [level: number]: any[] } = {};
    selectedLevels.forEach((attr) => {
      if (!levelGroups[attr.level]) {
        levelGroups[attr.level] = [];
      }
      levelGroups[attr.level].push(attr);
    });

    const levels = Object.keys(levelGroups).map(Number).sort();
    const result: any[] = [];

    console.log("Level groups:", levelGroups);

    // Process each level
    for (let i = 0; i < levels.length; i++) {
      const currentLevel = levels[i];
      const currentItems = levelGroups[currentLevel];

      if (currentLevel === 1) {
        // First level - always include
        result.push(...currentItems);
        console.log(`Level ${currentLevel}: Added all items (root level)`);
      } else {
        // Check if all items at this level have the same parent
        const parentCodes = [
          ...new Set(currentItems.map((item) => item.parentCode))
        ];

        if (parentCodes.length === 1 && parentCodes[0]) {
          // Same parent - check if parent is already selected at previous level
          const parentCode = parentCodes[0];
          const parentInResult = result.find(
            (item) => item.code === parentCode
          );

          if (parentInResult) {
            // Parent already in result - replace with children (more specific)
            const parentIndex = result.findIndex(
              (item) => item.code === parentCode
            );
            result.splice(parentIndex, 1); // Remove parent
            result.push(...currentItems); // Add children
            console.log(
              `Level ${currentLevel}: Same parent (${parentCode}) - replaced parent with children`
            );
          } else {
            // Parent not in result - add children only
            result.push(...currentItems);
            console.log(
              `Level ${currentLevel}: Same parent (${parentCode}) - added children only`
            );
          }
        } else {
          // Different parents or no parent - add all items
          result.push(...currentItems);
          console.log(
            `Level ${currentLevel}: Different parents - added all items`
          );

          // Also ensure parents are included if not already present
          parentCodes.forEach((parentCode) => {
            if (
              parentCode &&
              !result.find((item) => item.code === parentCode)
            ) {
              // Find parent item from previous levels
              const parentItem = selectedLevels.find(
                (item) => item.code === parentCode
              );
              if (parentItem) {
                result.push(parentItem);
                console.log(
                  `Level ${currentLevel}: Added missing parent ${parentCode}`
                );
              }
            }
          });
        }
      }
    }

    console.log("Optimization result:", result);
    return result;
  };

  // Handle save
  const handleSave = async () => {
    // Validate product attributes - at least one level should have a selection
    const hasAnySelection =
      selectedAttributes.length > 0 &&
      selectedAttributes.some((attr) => attr.code && attr.code !== "");
    if (!hasAnySelection) {
      toast({
        title: "Missing Product Attributes",
        description:
          "Please select at least one product attribute level from the hierarchy above",
        variant: "destructive"
      });
      return;
    }

    // Validate role selection
    if (!selectedRole) {
      toast({
        title: "No Role Selected",
        description: "Please select a sales team role from the dropdown",
        variant: "destructive"
      });
      return;
    }

    // Validate employee selection
    if (!selectedSalesTeams || selectedSalesTeams.length === 0) {
      toast({
        title: "No Team Members Selected",
        description:
          "Please select at least one team member from the selected role",
        variant: "destructive"
      });
      return;
    }

    try {
      setSaving(true);

      // Build linked item UIDs from selected product attributes (Multi-dropdown format)
      // selectedAttributes is now an array of SelectedAttribute objects from ProductAttributesMultiDropdown
      const selectedLevels = selectedAttributes.filter(
        (attr) => attr.code && attr.code !== ""
      );

      // Optimize selection: only save children if parents are same, otherwise save both
      const optimizedAttributes = optimizeHierarchicalSelection(selectedLevels);
      const linkedItemUIDs = selectedSalesTeams.join(","); // Users linked to these attributes

      console.log("Selected levels:", selectedLevels);
      console.log("Optimized attributes to save:", optimizedAttributes);
      console.log("Users linked to attributes:", linkedItemUIDs);

      // Build SelectionMapCriteria following database structure
      // Add a unique UID for this criteria to avoid conflicts
      const criteriaUID = `SMAP_${Date.now()}_${Math.random()
        .toString(36)
        .substring(2, 9)}`;
      const currentDateTime = new Date().toISOString();

      const selectionMapCriteria = {
        uid: criteriaUID, // Add UID to avoid transaction issues
        linkedItemUID: selectedSalesTeams.join(","), // Link to selected users/employees
        linkedItemType: "User", // Changed to User type for employee relations
        hasOrganization: false,
        hasLocation: false,
        hasCustomer: false,
        hasSalesTeam: false, // Changed to false since we're using hasItem now
        hasItem: true, // Changed to true - we're storing items (product attributes)
        orgCount: 0,
        locationCount: 0,
        customerCount: 0,
        salesTeamCount: 0, // Set to 0 since hasSalesTeam is false
        itemCount: optimizedAttributes.length, // Count of optimized product attributes
        actionType: 0, // 0=Add, 1=Update, 2=Delete
        isActive: true,
        createdTime: currentDateTime,
        modifiedTime: currentDateTime,
        SS: 0
      };

      // Build SelectionMapDetails for each optimized product attribute (items)
      // Changed to save as SKUGroup instead of ProductAttribute
      const selectionMapDetails = optimizedAttributes.map(
        (attribute, index) => ({
          uid: `SDET_${criteriaUID}_${index}`, // Add UID for each detail
          selectionMapCriteriaUID: criteriaUID, // Link to the criteria
          selectionGroup: "Item", // Item group for SKU Groups
          typeUID: "SKUGroup", // Changed from "ProductAttribute" to "SKUGroup"
          selectionValue: attribute.uid || attribute.code, // Use attribute UID/code
          isExcluded: false,
          actionType: 0, // 0=Add, 1=Update, 2=Delete
          isActive: true,
          createdTime: currentDateTime,
          modifiedTime: currentDateTime,
          SS: 0
        })
      );

      // Complete request payload
      const requestPayload = {
        selectionMapCriteria,
        selectionMapDetails
      };

      console.log("Saving user-SKUGroup mapping:", requestPayload);
      console.log("Linked Users (Employees):", selectedSalesTeams);
      console.log(
        "Selected SKU Groups (Product Attributes):",
        selectedAttributes
      );

      // Call the SelectionMap API using the correct endpoint from store-linking service
      const response = await fetch(
        `${
          process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"
        }/Mapping/CUDSelectiomMapMaster`,
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${localStorage.getItem("auth_token")}`
          },
          body: JSON.stringify(requestPayload)
        }
      );

      console.log("Response status:", response.status);
      console.log("Response headers:", response.headers);

      // Check if response has content
      const contentType = response.headers.get("content-type");
      const contentLength = response.headers.get("content-length");

      console.log("Content-Type:", contentType);
      console.log("Content-Length:", contentLength);

      // Handle empty response
      if (!contentType || !contentType.includes("application/json")) {
        if (response.ok) {
          // If status is OK but no JSON response, assume success
          toast({
            title: "Success",
            description: "SKU Groups assigned to sales team successfully"
          });
          console.log("Save successful (no JSON response)");
          handleReset();
          return;
        } else {
          throw new Error(
            `Server returned status ${response.status} without JSON response`
          );
        }
      }

      // Try to parse JSON response
      let result;
      const responseText = await response.text();
      console.log("Response text:", responseText);

      if (responseText) {
        try {
          result = JSON.parse(responseText);
        } catch (parseError) {
          console.error("Failed to parse response:", parseError);
          console.error("Response text was:", responseText);

          if (response.ok) {
            // If parsing fails but status is OK, assume success
            toast({
              title: "Success",
              description: "SKU Groups assigned to sales team successfully"
            });
            handleReset();
            return;
          } else {
            throw new Error("Invalid response from server");
          }
        }
      } else if (response.ok) {
        // Empty response but OK status
        toast({
          title: "Success",
          description: "SKU Groups assigned to users successfully"
        });
        handleReset();
        return;
      }

      if (!response.ok) {
        throw new Error(
          result?.Message ||
            `Failed to save mapping (status: ${response.status})`
        );
      }

      if (result && result.IsSuccess) {
        toast({
          title: "Success",
          description: "SKU Groups assigned to users successfully"
        });

        // Log for debugging
        console.log("Save successful:", result);

        // Reset form after successful save
        handleReset();
      } else if (result) {
        throw new Error(result.Message || "Failed to save mapping");
      }
    } catch (error) {
      console.error("Error saving product attribute assignment:", error);
      toast({
        title: "Error",
        description:
          error instanceof Error
            ? error.message
            : "Failed to save product attribute assignment",
        variant: "destructive"
      });
    } finally {
      setSaving(false);
    }
  };

  // Handle reset
  const handleReset = () => {
    setSelectedAttributes([]);
    setSelectionCriteria({
      hasEmployee: false,
      hasSalesTeam: false,
      hasLocation: false,
      hasOrganization: false
    });
    setSelectedEmployees([]);
    setSelectedRole("");
    setSelectedSalesTeams([]);
    setSelectedLocations([]);
    setSelectedOrganizations([]);
    setEmployees([]);
  };

  // Filter employees based on search
  const filteredEmployees = employees.filter(
    (emp) =>
      emp.name.toLowerCase().includes(employeeSearchTerm.toLowerCase()) ||
      emp.code.toLowerCase().includes(employeeSearchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      {/* Sales Team Selection - Always visible */}
      <Card>
        <CardHeader className="border-b">
          <div className="flex items-center justify-between">
            <CardTitle className="flex items-center gap-3 text-xl">
              <div className="p-2 bg-purple-50 rounded-lg">
                <Users className="h-5 w-5 text-purple-600" />
              </div>
              Sales Team Selection
            </CardTitle>
            <div className="flex items-center gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  if (selectionCriteria.hasEmployee) {
                    setSelectedEmployees([]);
                  } else if (selectionCriteria.hasSalesTeam) {
                    setSelectedSalesTeams([]);
                    setSelectedRole("");
                  } else if (selectionCriteria.hasLocation) {
                    setSelectedLocations([]);
                  } else if (selectionCriteria.hasOrganization) {
                    setSelectedOrganizations([]);
                  }
                }}
              >
                <X className="h-4 w-4 mr-2" />
                Clear Selection
              </Button>
            </div>
          </div>
          <p className="text-sm text-muted-foreground mt-2">
            Select a role and assign team members to product attributes
          </p>
        </CardHeader>
        <CardContent className="p-6">
          <div className="space-y-8">
            {/* Sales Team Selection - Always shown */}
            <div className="space-y-6">
              {/* Role Selection Dropdown */}
              <div className="space-y-4">
                <Label className="text-base font-semibold flex items-center gap-2">
                  <Briefcase className="h-4 w-4 text-purple-600" />
                  Select Sales Team Role
                </Label>
                <Select
                  value={selectedRole}
                  onValueChange={(value) => {
                    setSelectedRole(value);
                    setSelectedSalesTeams([]); // Clear previous selections
                    loadEmployeesForRole(value); // Load employees for this role
                  }}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Choose a role..." />
                  </SelectTrigger>
                  <SelectContent>
                    {availableRoles.map((role) => (
                      <SelectItem key={role.value} value={role.value}>
                        {role.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* Show loading or no roles message if needed */}
              {loadingRoles && (
                <div className="text-center py-4">
                  <RefreshCw className="h-6 w-6 animate-spin mx-auto mb-2" />
                  <p className="text-sm text-gray-500">Loading roles...</p>
                </div>
              )}

              {!loadingRoles && availableRoles.length === 0 && (
                <div className="text-center py-4 text-gray-500">
                  <p>
                    No roles found. Please ensure roles are configured in the
                    system.
                  </p>
                </div>
              )}

              {/* Employee Selection for the selected role */}
              {selectedRole && (
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <Label className="text-base font-semibold flex items-center gap-2">
                      <Users className="h-4 w-4 text-purple-600" />
                      Select Team Members
                    </Label>
                    <Badge variant="outline" className="bg-purple-50">
                      {selectedSalesTeams.length} selected
                    </Badge>
                  </div>

                  {/* Employee Search */}
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
                    <Input
                      placeholder="Search team members..."
                      value={employeeSearchTerm}
                      onChange={(e) => setEmployeeSearchTerm(e.target.value)}
                      className="pl-10"
                    />
                  </div>

                  {/* Employee List for selected role */}
                  <ScrollArea className="h-[300px] border rounded-lg p-4">
                    {filteredEmployees.length === 0 ? (
                      <div className="text-center py-8 text-gray-500">
                        No employees found for role: {selectedRole}
                      </div>
                    ) : (
                      <div className="space-y-2">
                        {filteredEmployees.map((employee) => (
                          <div
                            key={employee.uid}
                            className="flex items-center space-x-3 p-2 hover:bg-gray-50 rounded"
                          >
                            <Checkbox
                              checked={selectedSalesTeams.includes(
                                employee.uid
                              )}
                              onCheckedChange={(checked) => {
                                if (checked) {
                                  setSelectedSalesTeams([
                                    ...selectedSalesTeams,
                                    employee.uid
                                  ]);
                                } else {
                                  setSelectedSalesTeams(
                                    selectedSalesTeams.filter(
                                      (id) => id !== employee.uid
                                    )
                                  );
                                }
                              }}
                            />
                            <div className="flex-1">
                              <div className="font-medium">
                                {employee.name} ({employee.code})
                              </div>
                              <div className="text-sm text-gray-500">
                                {employee.designation}
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </ScrollArea>
                </div>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Product Attributes Selection */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-3 text-xl">
            <div className="p-2 bg-green-50 rounded-lg">
              <Package className="h-5 w-5 text-green-600" />
            </div>
            Product Attributes
          </CardTitle>
          <CardDescription>Configure product hierarchy</CardDescription>
        </CardHeader>
        <CardContent>
          <ProductAttributesMultiDropdown
            onSelectionsChange={handleAttributesChange}
            fieldNamePattern="L{n}"
            showLevelNumbers={true}
            disabled={loading || saving}
            enableSearch={true}
            gridColumns={{ default: 1, md: 2, lg: 3 }}
          />
        </CardContent>
      </Card>

      {/* Action Buttons - Moved to Bottom */}
      <div className="flex justify-end gap-4">
        <Button variant="outline" onClick={handleReset} disabled={saving}>
          <X className="h-4 w-4 mr-2" />
          Reset
        </Button>
        <Button
          onClick={handleSave}
          disabled={
            loading ||
            saving ||
            !(
              selectedAttributes.length > 0 &&
              selectedAttributes.some((attr) => attr.code)
            ) ||
            !selectedRole ||
            selectedSalesTeams.length === 0
          }
          title={
            !(
              selectedAttributes.length > 0 &&
              selectedAttributes.some((attr) => attr.code)
            )
              ? "Please select at least one product attribute level"
              : !selectedRole
              ? "Please select a role"
              : selectedSalesTeams.length === 0
              ? "Please select team members"
              : "Save assignment"
          }
        >
          {saving ? (
            <>
              <RefreshCw className="h-4 w-4 mr-2 animate-spin" />
              Saving...
            </>
          ) : (
            <>
              <Save className="h-4 w-4 mr-2" />
              Save Assignment
            </>
          )}
        </Button>
      </div>
    </div>
  );
}
