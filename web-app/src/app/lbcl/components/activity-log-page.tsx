"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { ChevronRight, ChevronDown, FileText, Check, RefreshCw } from "lucide-react";
import { ShareDialog } from "@/app/lbcl/components/share-dialog";
import { SignatureDialog } from "@/app/lbcl/components/signature-dialog";
import { inventoryService } from "@/services/inventory/inventory.service";
import { vehicleService, Vehicle } from "@/services/vehicleService";
import { organizationService } from "@/services/organizationService";
import { employeeService } from "@/services/admin/employee.service";
import { roleService } from "@/services/admin/role.service";
import { deliveryLoadingService } from "@/services/deliveryLoadingService";
import { useAuth } from "@/providers/auth-provider";
import { openPickListPDFInNewTab } from "@/utils/pickListPDF";
import { openDeliveryNotePDFInNewTab, getDeliveryNotePDFBlob, getDeliveryNoteNumber } from "@/utils/deliveryNotePDF";

interface ActivityLogPageProps {
  deliveryPlanId: string;
  readOnly?: boolean;
}

export function ActivityLogPage({ deliveryPlanId, readOnly = false }: ActivityLogPageProps) {
  const router = useRouter();
  const { user } = useAuth();

  // Check if user has PRINCIPLE role or belongs to PRINCIPLE organization
  const isPrincipalRole = user?.roles?.some(role => role.isPrincipalRole === true);
  const isPrincipalOrg = user?.currentOrganization?.type?.toUpperCase() === "PRINCIPAL";
  const isPrincipalUser = isPrincipalRole || isPrincipalOrg;

  // Check if user is an OPERATOR
  const isOperator = user?.roles?.some(role =>
    role.roleNameEn?.toUpperCase().includes("OPERATOR") ||
    role.code?.toUpperCase().includes("OPERATOR")
  );

  // Check if user is a SECURITY OFFICER
  const isSecurityOfficer = user?.roles?.some(role =>
    role.roleNameEn?.toUpperCase().includes("SECURITY") ||
    role.code?.toUpperCase().includes("SECURITY") ||
    role.uid?.toUpperCase() === "SECURITYOFFICER" ||
    role.code?.toUpperCase() === "SECURITYOFFICER"
  );

  console.log("🔐 Activity Log - User Role Check:");
  console.log("   - Is Operator:", isOperator);
  console.log("   - Is Security Officer:", isSecurityOfficer);
  console.log("   - Is Principal User:", isPrincipalUser);
  console.log("   - User roles:", user?.roles);

  const [expandedSections, setExpandedSections] = useState<number[]>([3, 6]);
  const [showShareDialog, setShowShareDialog] = useState(false);
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);
  const [showSuccessMessage, setShowSuccessMessage] = useState(false);

  const handleSignatureSave = async (logisticsSignature: string, driverSignature: string, signatureNotes: string) => {
    try {
      // Helper function to convert empty strings to null for GUIDs
      const sanitizeGuid = (value: string | null | undefined) => {
        if (!value || value.trim() === "") return null;
        return value;
      };

      // Load existing delivery loading data to preserve fields the current user cannot modify
      let existingData: any = null;
      try {
        existingData = await deliveryLoadingService.getByWHStockRequestUID(purchaseOrder.UID || purchaseOrder.uid);
        if (existingData) {
          console.log("📋 Existing delivery loading data:", existingData);
        }
      } catch (error) {
        console.log("ℹ️ No existing data found, creating new record");
      }

      // Create delivery loading tracking data
      const now = new Date();
      const todayDate = now.toISOString().split('T')[0]; // YYYY-MM-DD

      // For OPERATOR from PRINCIPLE: Only save Fork Lift Operator & Load Times, preserve other fields
      // For SECURITY OFFICER from PRINCIPLE: Only save Security Officer & Departure Time, preserve other fields
      // For PRINCIPLE (non-OPERATOR/non-SECURITY): Only save Vehicle & Driver, preserve other fields
      // For other users: Save all fields
      const isOperatorFromPrincipal = isOperator && isPrincipalUser;
      const isSecurityOfficerFromPrincipal = isSecurityOfficer && isPrincipalUser;
      const isPrincipalNonOperator = isPrincipalUser && !isOperator && !isSecurityOfficer;

      // OPERATOR can edit Load Times, SECURITY OFFICER cannot, PRINCIPLE (non-OPERATOR/non-SECURITY) cannot
      const loadingStartTime = (isPrincipalNonOperator || isSecurityOfficerFromPrincipal)
        ? (existingData?.LoadingStartTime || existingData?.loadingStartTime || null)
        : `${todayDate}T${loadingStartHour.padStart(2, '0')}:${loadingStartMin.padStart(2, '0')}:00`;

      const loadingEndTime = (isPrincipalNonOperator || isSecurityOfficerFromPrincipal)
        ? (existingData?.LoadingEndTime || existingData?.loadingEndTime || null)
        : `${todayDate}T${loadingEndHour.padStart(2, '0')}:${loadingEndMin.padStart(2, '0')}:00`;

      // SECURITY OFFICER can edit Departure Time, OPERATOR and PRINCIPLE (non-SECURITY) cannot
      const departureTime = (isPrincipalNonOperator || isOperatorFromPrincipal)
        ? (existingData?.DepartureTime || existingData?.departureTime || null)
        : `${todayDate}T${departureHour.padStart(2, '0')}:${departureMin.padStart(2, '0')}:00`;

      // Auto-generate delivery note number based on order number and timestamp
      const autoDeliveryNoteNumber = existingData?.DeliveryNoteNumber || existingData?.deliveryNoteNumber || getDeliveryNoteNumber(purchaseOrder);

      // Generate and upload delivery note PDF
      let deliveryNoteFilePath = existingData?.DeliveryNoteFilePath || existingData?.deliveryNoteFilePath || null;

      if (!deliveryNoteFilePath) {
        try {
          console.log("📄 Generating delivery note PDF...");
          const pdfBlob = getDeliveryNotePDFBlob(purchaseOrder, orderLines);
          const pdfFileName = `${autoDeliveryNoteNumber}.pdf`;

          // Create FormData to upload PDF
          const formData = new FormData();
          formData.append('file', pdfBlob, pdfFileName);
          formData.append('folderPath', 'DeliveryNotes');

          // Upload PDF to server
          const token = typeof window !== "undefined" ? localStorage.getItem("auth_token") : null;
          const uploadResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api"}/FileUpload/UploadFile`, {
            method: 'POST',
            headers: {
              Authorization: token ? `Bearer ${token}` : "",
            },
            body: formData
          });

          if (uploadResponse.ok) {
            const uploadResult = await uploadResponse.json();
            console.log("📤 Upload response:", uploadResult);
            // Check both possible property names (SavedImgsPath with capital letters)
            const savedPaths = uploadResult.SavedImgsPath || uploadResult.savedImgsPath;
            if (savedPaths && savedPaths.length > 0) {
              deliveryNoteFilePath = savedPaths[0];
              console.log("✅ Delivery note PDF saved:", deliveryNoteFilePath);
            } else {
              console.warn("⚠️ No file path returned from upload");
            }
          } else {
            console.error("❌ Upload failed with status:", uploadResponse.status);
            const errorText = await uploadResponse.text();
            console.error("Error details:", errorText);
          }
        } catch (pdfError) {
          console.error("❌ Error saving delivery note PDF:", pdfError);
          // Continue even if PDF save fails
        }
      }

      const deliveryLoadingData = {
        WHStockRequestUID: purchaseOrder.UID || purchaseOrder.uid,
        // For OPERATOR/SECURITY: preserve existing Vehicle/Driver, For PRINCIPLE (non-OPERATOR/non-SECURITY): save new value, For others: save new value
        VehicleUID: (isOperatorFromPrincipal || isSecurityOfficerFromPrincipal)
          ? sanitizeGuid(existingData?.VehicleUID || existingData?.vehicleUID)
          : sanitizeGuid(selectedVehicle),
        DriverEmployeeUID: (isOperatorFromPrincipal || isSecurityOfficerFromPrincipal)
          ? sanitizeGuid(existingData?.DriverEmployeeUID || existingData?.driverEmployeeUID)
          : sanitizeGuid(selectedDriver),
        // For OPERATOR: save new value, For PRINCIPLE (non-OPERATOR): preserve existing, For others: save new value
        ForkLiftOperatorUID: (isPrincipalNonOperator || isSecurityOfficerFromPrincipal)
          ? sanitizeGuid(existingData?.ForkLiftOperatorUID || existingData?.forkLiftOperatorUID)
          : sanitizeGuid(selectedOperator),
        // For SECURITY OFFICER: save new value, For PRINCIPLE (non-SECURITY): preserve existing, For others: save new value
        SecurityOfficerUID: (isPrincipalNonOperator || isOperatorFromPrincipal)
          ? sanitizeGuid(existingData?.SecurityOfficerUID || existingData?.securityOfficerUID)
          : sanitizeGuid(selectedSecurityOfficer),
        ArrivalTime: arrivalTime || existingData?.ArrivalTime || existingData?.arrivalTime || null,
        LoadingStartTime: loadingStartTime,
        LoadingEndTime: loadingEndTime,
        DepartureTime: departureTime,
        // For SECURITY OFFICER: preserve existing signatures, For others: save new signatures
        LogisticsSignature: isSecurityOfficerFromPrincipal
          ? (existingData?.LogisticsSignature || existingData?.logisticsSignature || null)
          : (logisticsSignature || existingData?.LogisticsSignature || existingData?.logisticsSignature || null),
        DriverSignature: isSecurityOfficerFromPrincipal
          ? (existingData?.DriverSignature || existingData?.driverSignature || null)
          : (driverSignature || existingData?.DriverSignature || existingData?.driverSignature || null),
        Notes: signatureNotes || notes || existingData?.Notes || existingData?.notes || null,
        DeliveryNoteNumber: autoDeliveryNoteNumber,
        DeliveryNoteFilePath: deliveryNoteFilePath,
        Status: "SHIPPED",
        IsActive: true
      };

      console.log("📦 Delivery Loading Data being sent:", {
        isPrincipalUser,
        isOperator,
        isSecurityOfficer,
        isOperatorFromPrincipal,
        isSecurityOfficerFromPrincipal,
        isPrincipalNonOperator,
        existingData: existingData ? 'Found' : 'Not found',
        VehicleUID: deliveryLoadingData.VehicleUID,
        DriverEmployeeUID: deliveryLoadingData.DriverEmployeeUID,
        ForkLiftOperatorUID: deliveryLoadingData.ForkLiftOperatorUID,
        SecurityOfficerUID: deliveryLoadingData.SecurityOfficerUID,
        ArrivalTime: arrivalTime,
        LoadingStartTime: loadingStartTime,
        LoadingEndTime: loadingEndTime,
        DepartureTime: departureTime
      });

      // Save delivery loading tracking
      await deliveryLoadingService.saveDeliveryLoadingTracking(deliveryLoadingData);
      console.log("✅ Delivery loading tracking saved successfully");

      // Show success message
      setShowSuccessMessage(true);

      // Redirect to delivery plans after 2 seconds
      setTimeout(() => {
        router.push("/lbcl/delivery-plans");
      }, 2000);
    } catch (error) {
      console.error("Error saving delivery plan:", error);
      setError("Failed to confirm delivery plan");
    }
  };

  // Data states
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [purchaseOrder, setPurchaseOrder] = useState<any>(null);
  const [orderLines, setOrderLines] = useState<any[]>([]);

  // Vehicle states
  const [vehicles, setVehicles] = useState<Vehicle[]>([]);
  const [selectedVehicle, setSelectedVehicle] = useState<string>("");
  const [loadingVehicles, setLoadingVehicles] = useState(false);

  // Driver/Employee states
  const [drivers, setDrivers] = useState<any[]>([]);
  const [selectedDriver, setSelectedDriver] = useState<string>("");
  const [loadingDrivers, setLoadingDrivers] = useState(false);

  // Operator states
  const [operators, setOperators] = useState<any[]>([]);
  const [selectedOperator, setSelectedOperator] = useState<string>("");
  const [loadingOperators, setLoadingOperators] = useState(false);

  // Security Officer states
  const [securityOfficers, setSecurityOfficers] = useState<any[]>([]);
  const [selectedSecurityOfficer, setSelectedSecurityOfficer] = useState<string>("");
  const [loadingSecurityOfficers, setLoadingSecurityOfficers] = useState(false);

  // Time tracking states
  const [arrivalTime, setArrivalTime] = useState<string>("");
  const [loadingStartHour, setLoadingStartHour] = useState<string>("11");
  const [loadingStartMin, setLoadingStartMin] = useState<string>("14");
  const [loadingEndHour, setLoadingEndHour] = useState<string>("12");
  const [loadingEndMin, setLoadingEndMin] = useState<string>("25");
  const [departureHour, setDepartureHour] = useState<string>("13");
  const [departureMin, setDepartureMin] = useState<string>("26");
  const [notes, setNotes] = useState<string>("");

  // Signature states
  const [logisticsSignature, setLogisticsSignature] = useState<string>("");
  const [driverSignature, setDriverSignature] = useState<string>("");


  useEffect(() => {
    fetchPurchaseOrderData();
    loadVehicles();
    loadDrivers();
    loadOperators();
    loadSecurityOfficers();
  }, [deliveryPlanId]);

  const loadVehicles = async () => {
    try {
      setLoadingVehicles(true);

      // Get organizations to find the Principal org
      const orgsResult = await organizationService.getOrganizations(1, 1000);
      const activeOrgs = orgsResult.data.filter((org: any) => org.ShowInTemplate === true);

      if (activeOrgs.length > 0) {
        // Use the first organization (Principal)
        const principalOrgUID = activeOrgs[0].UID;

        const result = await vehicleService.getVehicles(1, 100, principalOrgUID, []);

        if (result.data && result.data.length > 0) {
          console.log("🚗 First vehicle from API:", result.data[0]);
          console.log("🔑 Vehicle UID being set:", result.data[0].UID);
          setVehicles(result.data);
          // Auto-select the first vehicle
          setSelectedVehicle(result.data[0].UID);
        }
      }
    } catch (error) {
      console.error("Error loading vehicles:", error);
    } finally {
      setLoadingVehicles(false);
    }
  };

  const loadEmployeesByRole = async (
    roleIdentifier: string,
    roleName: string,
    setEmployees: (employees: any[]) => void,
    setSelected: (uid: string) => void
  ) => {
    // Get organizations to find the Principal org
    const orgsResult = await organizationService.getOrganizations(1, 1000);
    const activeOrgs = orgsResult.data.filter((org: any) => org.ShowInTemplate === true);

    let roles: any[] = [];

    if (activeOrgs.length > 0) {
      // Use the first organization (Principal)
      const principalOrgUID = activeOrgs[0].UID;

      // Get roles for the Principal organization
      const orgRoles = await roleService.getRolesByOrg(principalOrgUID, false);

      console.log(`🔍 Roles for org (${roleName}):`, principalOrgUID, orgRoles);

      if (orgRoles && orgRoles.length > 0) {
        roles = orgRoles;
      } else {
        // If no roles found for org, load all roles
        console.log(`⚠️ No roles found for org, loading all roles (${roleName})`);
        const allRolesResult = await roleService.getRoles({
          pageNumber: 1,
          pageSize: 1000,
          isCountRequired: false
        });

        roles = allRolesResult.pagedData || [];
        console.log(`📋 All roles loaded (${roleName}):`, roles);
      }
    }

    if (roles && roles.length > 0) {
      // Find the role - first try by exact UID, then by name
      const targetRole = roles.find((role: any) =>
        role.UID === roleIdentifier ||
        role.uid === roleIdentifier ||
        role.Code === roleIdentifier ||
        role.code === roleIdentifier ||
        role.RoleNameEn?.toLowerCase().includes(roleName.toLowerCase()) ||
        role.roleNameEn?.toLowerCase().includes(roleName.toLowerCase())
      );

      console.log(`🎯 Found ${roleName} role:`, targetRole);

      if (targetRole) {
        const roleUID = targetRole.UID;
        const principalOrgUID = activeOrgs.length > 0 ? activeOrgs[0].UID : '';

        // Load employees with the role
        console.log(`📋 Loading employees for org: ${principalOrgUID}, role: ${roleUID}`);
        const employees = await employeeService.getEmployeesSelectionItemByRoleUID(
          principalOrgUID,
          roleUID
        );

        console.log(`👥 ${roleName} employees:`, employees);
        console.log(`🔍 First ${roleName} employee raw:`, employees[0]);

        if (employees && employees.length > 0) {
          const mappedEmployees = employees.map((emp: any) => ({
            UID: emp.UID || emp.Value,
            Name: emp.Label || emp.Name || `[${emp.Code}] ${emp.Name}`,
            Code: emp.Code
          }));

          console.log(`✅ Mapped ${roleName}:`, mappedEmployees);
          console.log(`🔑 ${roleName} UID being set:`, mappedEmployees[0].UID);

          setEmployees(mappedEmployees);
          // Auto-select the first employee
          setSelected(mappedEmployees[0].UID);
        } else {
          console.log(`⚠️ No ${roleName} employees found`);
        }
      } else {
        console.log(`⚠️ ${roleName} role not found in roles list`);
      }
    } else {
      console.log(`⚠️ No roles available for ${roleName}`);
    }
  };

  const loadDrivers = async () => {
    try {
      setLoadingDrivers(true);
      await loadEmployeesByRole('DRIVER', 'driver', setDrivers, setSelectedDriver);
    } catch (error) {
      console.error("❌ Error loading drivers:", error);
    } finally {
      setLoadingDrivers(false);
    }
  };

  const loadOperators = async () => {
    try {
      setLoadingOperators(true);
      await loadEmployeesByRole('OPERATOR', 'operator', setOperators, setSelectedOperator);
    } catch (error) {
      console.error("❌ Error loading operators:", error);
    } finally {
      setLoadingOperators(false);
    }
  };

  const loadSecurityOfficers = async () => {
    try {
      setLoadingSecurityOfficers(true);
      await loadEmployeesByRole('SECURITYOFFICER', 'security', setSecurityOfficers, setSelectedSecurityOfficer);
    } catch (error) {
      console.error("❌ Error loading security officers:", error);
    } finally {
      setLoadingSecurityOfficers(false);
    }
  };

  const fetchPurchaseOrderData = async () => {
    try {
      setLoading(true);
      setError("");

      console.log('🔍 Fetching load request data for:', deliveryPlanId);
      const response = await inventoryService.selectLoadRequestDataByUID(deliveryPlanId);

      console.log('📦 Load Request Response:', response);

      if (response && response.WHStockRequest) {
        const header = response.WHStockRequest;
        const lines = response.WHStockRequestLines || [];

        console.log('✅ Load Request Header:', header);
        console.log('📋 Load Request Lines:', lines);

        // Transform WH Stock Request to match expected purchaseOrder format
        const transformedHeader = {
          UID: header.UID,
          uid: header.UID,
          Code: header.RequestCode,
          RequestCode: header.RequestCode,
          OrderNumber: header.RequestCode, // For "Delivery Plan No"
          DraftOrderNumber: header.RequestCode,
          OrgName: header.TargetOrgName, // Target Organization Name
          orgName: header.TargetOrgName,
          WarehouseName: header.TargetWHName, // Target Warehouse Name
          warehouseName: header.TargetWHName,
          Status: header.Status,
          status: header.Status,
          RequiredByDate: header.RequiredByDate,
          RequestedDeliveryDate: header.RequiredByDate, // For "Date"
          requestedDeliveryDate: header.RequiredByDate,
          ExpectedDeliveryDate: header.RequiredByDate,
          expectedDeliveryDate: header.RequiredByDate,
          OrderDate: header.RequestedTime,
          orderDate: header.RequestedTime,
          RequestedTime: header.RequestedTime,
          SourceOrgUID: header.SourceOrgUID,
          SourceOrgName: header.SourceOrgName,
          SourceWHUID: header.SourceWHUID,
          SourceWHName: header.SourceWHName,
          TargetOrgUID: header.TargetOrgUID,
          TargetOrgName: header.TargetOrgName,
          TargetWHUID: header.TargetWHUID,
          TargetWHName: header.TargetWHName,
          Remarks: header.Remarks
        };

        // Debug: Check if SKUName is present in raw lines data
        console.log("📦 Raw lines from API:", lines);
        console.log("🏷️ First line SKUName:", lines[0]?.SKUName);

        // Transform lines to match expected format
        const transformedLines = lines.map((line: any) => ({
          UID: line.UID,
          SKUCode: line.SKUCode,
          skuCode: line.SKUCode,
          SKUUID: line.SKUUID,
          SKUName: line.SKUName, // Add SKU Name for PDF
          skuName: line.SKUName,
          ProductName: line.SKUName, // Fallback field name
          RequestedQty: line.RequestedQty,
          requestedQty: line.RequestedQty,
          ApprovedQty: line.ApprovedQty,
          UOM: line.UOM,
          uom: line.UOM,
          Remarks: line.Remarks
        }));

        console.log("✅ Transformed lines:", transformedLines);
        console.log("🏷️ First transformed line SKUName:", transformedLines[0]?.SKUName);

        setPurchaseOrder(transformedHeader);
        setOrderLines(transformedLines);

        // Load existing delivery loading data if available
        await loadExistingDeliveryLoadingData(header.UID);
      } else {
        setError("Failed to fetch load request data");
      }
    } catch (error) {
      console.error("Error fetching load request:", error);
      setError("Failed to load load request data");
    } finally {
      setLoading(false);
    }
  };

  const loadExistingDeliveryLoadingData = async (whStockRequestUID: string) => {
    try {
      console.log("🔍 Loading existing delivery loading data for WH Stock Request:", whStockRequestUID);
      const response = await deliveryLoadingService.getDeliveryLoadingByWHStockRequest(whStockRequestUID);

      if (response && response.data) {
        const existingData = response.data;
        console.log("✅ Found existing delivery loading data:", existingData);

        // Populate all fields with existing data
        if (existingData.VehicleUID || existingData.vehicleUID) {
          setSelectedVehicle(existingData.VehicleUID || existingData.vehicleUID);
        }
        if (existingData.DriverEmployeeUID || existingData.driverEmployeeUID) {
          setSelectedDriver(existingData.DriverEmployeeUID || existingData.driverEmployeeUID);
        }
        if (existingData.ForkLiftOperatorUID || existingData.forkLiftOperatorUID) {
          setSelectedOperator(existingData.ForkLiftOperatorUID || existingData.forkLiftOperatorUID);
        }
        if (existingData.SecurityOfficerUID || existingData.securityOfficerUID) {
          setSelectedSecurityOfficer(existingData.SecurityOfficerUID || existingData.securityOfficerUID);
        }

        // Populate time fields
        const loadingStartTime = existingData.LoadingStartTime || existingData.loadingStartTime;
        if (loadingStartTime) {
          const startTime = new Date(loadingStartTime);
          setLoadingStartHour(startTime.getHours().toString().padStart(2, '0'));
          setLoadingStartMin(startTime.getMinutes().toString().padStart(2, '0'));
        }

        const loadingEndTime = existingData.LoadingEndTime || existingData.loadingEndTime;
        if (loadingEndTime) {
          const endTime = new Date(loadingEndTime);
          setLoadingEndHour(endTime.getHours().toString().padStart(2, '0'));
          setLoadingEndMin(endTime.getMinutes().toString().padStart(2, '0'));
        }

        const departureTime = existingData.DepartureTime || existingData.departureTime;
        if (departureTime) {
          const departure = new Date(departureTime);
          setDepartureHour(departure.getHours().toString().padStart(2, '0'));
          setDepartureMin(departure.getMinutes().toString().padStart(2, '0'));
        }

        const notes = existingData.Notes || existingData.notes;
        if (notes) setNotes(notes);

        const deliveryNotePath = existingData.DeliveryNoteFilePath || existingData.deliveryNoteFilePath;
        if (deliveryNotePath) {
          console.log("📄 Delivery Note File Path loaded:", deliveryNotePath);
        }

        console.log("✅ All existing data loaded successfully");
      } else {
        console.log("ℹ️ No existing delivery loading data found");
      }
    } catch (error) {
      console.error("Error loading existing delivery loading data:", error);
    }
  };

  const toggleSection = (section: number) => {
    setExpandedSections((prev) =>
      prev.includes(section)
        ? prev.filter((s) => s !== section)
        : [...prev, section]
    );
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
          <p className="text-gray-600">Loading delivery plan...</p>
        </div>
      </div>
    );
  }

  if (error || !purchaseOrder) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 max-w-md w-full text-center">
          <p className="text-red-700 mb-4">{error || "Delivery plan not found"}</p>
          <Button
            onClick={() => router.push("/lbcl/delivery-plans")}
            className="bg-[#A08B5C] hover:bg-[#8F7A4B]"
          >
            Back to Delivery Plans
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Delivery Info */}
      <div className="px-4 sm:px-6 lg:px-8 py-3 sm:py-4 bg-white border-b">
        <div className="flex items-center justify-end gap-2 mb-4">
          <Button
            variant="outline"
            onClick={() => router.push("/lbcl/delivery-plans")}
            className="text-[#A08B5C] border-[#A08B5C] text-xs sm:text-sm px-3 sm:px-6 bg-transparent"
          >
            Back
          </Button>
          {!readOnly && (
            <Button
              onClick={() => {
                // Show signature dialog only when current user is a Security Officer
                console.log("🔍 Submit button clicked:");
                console.log("   - Is Security Officer:", isSecurityOfficer);
                console.log("   - User roles:", user?.roles);

                if (isSecurityOfficer) {
                  // Current user is Security Officer - show signature dialog
                  console.log("📝 User is Security Officer - Showing signature dialog");
                  setShowSignatureDialog(true);
                } else {
                  // Current user is NOT Security Officer - skip signature dialog
                  console.log("✅ User is not Security Officer - Skipping signature dialog - submitting directly");
                  handleSignatureSave("", "", "");
                }
              }}
              className="bg-[#A08B5C] hover:bg-[#8F7A4B] text-white text-xs sm:text-sm px-3 sm:px-6"
            >
              Submit
            </Button>
          )}
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 sm:gap-4 text-sm text-center">
          <div>
            <div className="font-medium text-gray-600 mb-1">
              Delivery Plan No
            </div>
            <div className="font-bold break-all">
              {purchaseOrder.OrderNumber || purchaseOrder.orderNumber || purchaseOrder.DraftOrderNumber || purchaseOrder.draftOrderNumber || 'N/A'}
            </div>
          </div>
          <div>
            <div className="font-medium text-gray-600 mb-1">Distributor / Agent</div>
            <div className="font-bold break-words">
              {purchaseOrder.OrgName || purchaseOrder.orgName || purchaseOrder.OrgUID || purchaseOrder.orgUID || 'N/A'}
            </div>
          </div>
          <div>
            <div className="font-medium text-gray-600 mb-1">Date</div>
            <div className="font-bold">
              {formatDate(purchaseOrder.OrderDate || purchaseOrder.orderDate || '')}
            </div>
          </div>
        </div>
      </div>

      {/* Activity Steps */}
      <main className="px-4 sm:px-6 lg:px-8 py-4 sm:py-6">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 max-w-7xl mx-auto">
          {/* Step 1: Share Delivery Plan */}
          <button
            onClick={() => !readOnly && setShowShareDialog(true)}
            className={`bg-white rounded-lg p-4 sm:p-6 shadow-sm transition-shadow ${!readOnly ? 'hover:shadow-md' : 'cursor-default opacity-60'}`}
            disabled={readOnly}
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">1</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Share Delivery Plan
              </span>
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 2: View/Generate Pick List */}
          <button
            onClick={() => openPickListPDFInNewTab(purchaseOrder, orderLines)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">2</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                View / Generate Pick List ({orderLines.length} items)
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 3: Loading */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(3)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">3</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Loading
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(3) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(3) && (
              <div className="mt-4 sm:mt-6 space-y-4">
                {/* Show all fields, but enable/disable based on user role */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Vehicle / Prime Mover
                    </label>
                    <Select
                      value={selectedVehicle}
                      onValueChange={setSelectedVehicle}
                      disabled={loadingVehicles || readOnly || ((isOperator || isSecurityOfficer) && isPrincipalUser)}
                    >
                      <SelectTrigger className={readOnly || ((isOperator || isSecurityOfficer) && isPrincipalUser) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}>
                        <SelectValue placeholder={loadingVehicles ? "Loading vehicles..." : "Select vehicle"} />
                      </SelectTrigger>
                      <SelectContent>
                        {vehicles.map((vehicle) => (
                          <SelectItem key={vehicle.UID} value={vehicle.UID}>
                            {vehicle.VehicleNo} - {vehicle.RegistrationNo}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Driver
                    </label>
                    <Select
                      value={selectedDriver}
                      onValueChange={setSelectedDriver}
                      disabled={loadingDrivers || readOnly || ((isOperator || isSecurityOfficer) && isPrincipalUser)}
                    >
                      <SelectTrigger className={readOnly || ((isOperator || isSecurityOfficer) && isPrincipalUser) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}>
                        <SelectValue placeholder={loadingDrivers ? "Loading drivers..." : "Select driver"} />
                      </SelectTrigger>
                      <SelectContent>
                        {drivers.map((driver) => (
                          <SelectItem key={driver.UID} value={driver.UID}>
                            {driver.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Fork Lift Operator
                    </label>
                    <Select
                      value={selectedOperator}
                      onValueChange={setSelectedOperator}
                      disabled={loadingOperators || readOnly || (isPrincipalUser && !isOperator)}
                    >
                      <SelectTrigger className={readOnly || (isPrincipalUser && !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}>
                        <SelectValue placeholder={loadingOperators ? "Loading operators..." : "Select operator"} />
                      </SelectTrigger>
                      <SelectContent>
                        {operators.map((operator) => (
                          <SelectItem key={operator.UID} value={operator.UID}>
                            {operator.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="grid grid-cols-2 gap-2">
                    <div>
                      <label className="text-xs sm:text-sm font-medium mb-2 block">
                        Load Start Time
                      </label>
                      <div className="flex gap-2">
                        <Input
                          value={loadingStartHour}
                          onChange={(e) => setLoadingStartHour(e.target.value)}
                          className={`text-center ${readOnly || (isPrincipalUser && !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                          disabled={readOnly || (isPrincipalUser && !isOperator)}
                        />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input
                          value={loadingStartMin}
                          onChange={(e) => setLoadingStartMin(e.target.value)}
                          className={`text-center ${readOnly || (isPrincipalUser && !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                          disabled={readOnly || (isPrincipalUser && !isOperator)}
                        />
                        <span className="text-xs text-gray-500 self-center">
                          Min
                        </span>
                      </div>
                    </div>
                    <div>
                      <label className="text-xs sm:text-sm font-medium mb-2 block">
                        Load End Time
                      </label>
                      <div className="flex gap-2">
                        <Input
                          value={loadingEndHour}
                          onChange={(e) => setLoadingEndHour(e.target.value)}
                          className={`text-center ${readOnly || (isPrincipalUser && !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                          disabled={readOnly || (isPrincipalUser && !isOperator)}
                        />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input
                          value={loadingEndMin}
                          onChange={(e) => setLoadingEndMin(e.target.value)}
                          className={`text-center ${readOnly || (isPrincipalUser && !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                          disabled={readOnly || (isPrincipalUser && !isOperator)}
                        />
                        <span className="text-xs text-gray-500 self-center">
                          Min
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 4: View/Generate Delivery Note */}
          <button
            onClick={() => openDeliveryNotePDFInNewTab(purchaseOrder, orderLines)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">4</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                View / Generate Delivery Note ({orderLines.length} items)
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 5: Receive Stock */}
          <button
            onClick={() => !readOnly && setShowSignatureDialog(true)}
            className={`bg-white rounded-lg p-4 sm:p-6 shadow-sm transition-shadow ${!readOnly ? 'hover:shadow-md' : 'cursor-default opacity-60'}`}
            disabled={readOnly}
          >
            <div className="flex items-center gap-3 sm:gap-4">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">5</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Receive Stock
              </span>
              <FileText className="w-4 h-4 text-red-500 flex-shrink-0" />
              <ChevronRight className="w-5 h-5 text-gray-400 flex-shrink-0" />
            </div>
          </button>

          {/* Step 6: Gate Pass */}
          <div className="bg-white rounded-lg p-4 sm:p-6 shadow-sm lg:col-span-2">
            <button
              onClick={() => toggleSection(6)}
              className="flex items-center gap-3 sm:gap-4 w-full"
            >
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-[#F5E6C8] flex items-center justify-center flex-shrink-0">
                <span className="font-bold text-sm sm:text-base">6</span>
              </div>
              <span className="text-sm sm:text-base font-medium flex-1 text-left">
                Gate Pass
              </span>
              <ChevronDown
                className={`w-5 h-5 text-gray-400 transition-transform flex-shrink-0 ${
                  expandedSections.includes(6) ? "rotate-180" : ""
                }`}
              />
            </button>

            {expandedSections.includes(6) && (
              <div className="mt-4 sm:mt-6 space-y-4">
                {/* Enable Security Officer and Prime Mover Departure for SECURITY OFFICER role */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Getpass Employee (Security Officer)
                    </label>
                    <Select
                      value={selectedSecurityOfficer}
                      onValueChange={setSelectedSecurityOfficer}
                      disabled={loadingSecurityOfficers || readOnly || (isOperator && isPrincipalUser) || (isPrincipalUser && !isSecurityOfficer)}
                    >
                      <SelectTrigger className={`${(readOnly || (isOperator && isPrincipalUser) || (isPrincipalUser && !isSecurityOfficer)) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                        <SelectValue placeholder={loadingSecurityOfficers ? "Loading security officers..." : "Select security officer"} />
                      </SelectTrigger>
                      <SelectContent>
                        {securityOfficers.map((officer) => (
                          <SelectItem key={officer.UID} value={officer.UID}>
                            {officer.Name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Prime Mover Departure
                    </label>
                    <div className="flex gap-2">
                      <Input
                        value={departureHour}
                        onChange={(e) => setDepartureHour(e.target.value)}
                        className={`text-center flex-1 ${readOnly || (isPrincipalUser && !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        disabled={readOnly || (isPrincipalUser && !isSecurityOfficer)}
                      />
                      <span className="text-xs text-gray-500 self-center">
                        HH
                      </span>
                      <Input
                        value={departureMin}
                        onChange={(e) => setDepartureMin(e.target.value)}
                        className={`text-center flex-1 ${readOnly || (isPrincipalUser && !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        disabled={readOnly || (isPrincipalUser && !isSecurityOfficer)}
                      />
                      <span className="text-xs text-gray-500 self-center">
                        Min
                      </span>
                    </div>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <Check className="w-5 h-5 text-green-600" />
                  <span className="text-sm text-gray-700">Notify Agency</span>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Progress Tracker */}
        <div className="mt-6 sm:mt-8 bg-white rounded-lg p-4 sm:p-6 shadow-sm max-w-7xl mx-auto overflow-x-auto">
          <div className="flex items-center gap-2 sm:gap-4 min-w-max pb-2">
            {/* Checkpoint 1 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-green-500 flex items-center justify-center mb-2">
                <Check className="w-4 h-4 sm:w-5 sm:h-5 text-white" />
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  LBCL Warehouse
                </div>
                <div className="text-xs text-green-600">Completed</div>
                <div className="text-xs text-gray-500">Arrived: 08:00 AM</div>
                <div className="text-xs text-gray-500">Distance: 0 km</div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 25min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-green-500 min-w-[40px]" />

            {/* Checkpoint 2 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-green-500 flex items-center justify-center mb-2">
                <Check className="w-4 h-4 sm:w-5 sm:h-5 text-white" />
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Checkpoint Alpha
                </div>
                <div className="text-xs text-green-600">Completed</div>
                <div className="text-xs text-gray-500">
                  Arrived: 09:40 AM (+5 min)
                </div>
                <div className="text-xs text-gray-500">
                  Distance: 48 km (+3 km)
                </div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 35min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-yellow-500 min-w-[40px]" />

            {/* Checkpoint 3 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-blue-500 flex items-center justify-center mb-2">
                <span className="text-white text-xs sm:text-sm font-bold">
                  3
                </span>
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Checkpoint Beta
                </div>
                <div className="text-xs text-blue-600">In Progress</div>
                <div className="text-xs text-gray-500">
                  Arrived: 11:40 AM (+30 min)
                </div>
                <div className="text-xs text-gray-500">Distance: 33 km</div>
                <div className="text-xs text-gray-500">
                  Travel time: 1h 15min
                </div>
              </div>
            </div>

            <div className="h-1 flex-1 bg-gray-300 min-w-[40px]" />

            {/* Checkpoint 4 */}
            <div className="flex flex-col items-center flex-shrink-0">
              <div className="w-8 h-8 sm:w-10 sm:h-10 rounded-full bg-gray-300 flex items-center justify-center mb-2">
                <span className="text-gray-600 text-xs sm:text-sm font-bold">
                  4
                </span>
              </div>
              <div className="text-center">
                <div className="text-xs sm:text-sm font-medium">
                  Distributor Agent Warehouse
                </div>
                <div className="text-xs text-gray-500">Pending</div>
                <div className="text-xs text-gray-500">
                  Distance: 40 km (planned)
                </div>
              </div>
            </div>
          </div>

          <div className="mt-4 p-3 bg-red-50 rounded-lg">
            <p className="text-xs sm:text-sm text-red-600 flex items-center gap-2">
              <span className="flex-shrink-0">⚠️</span>
              <span>
                Current pace suggests arrival at destination may be delayed by
                ~15 minutes
              </span>
            </p>
          </div>
        </div>
      </main>

      {/* Dialogs */}
      <ShareDialog
        open={showShareDialog}
        onOpenChange={setShowShareDialog}
        purchaseOrder={purchaseOrder}
        orderLines={orderLines}
      />
      <SignatureDialog
        open={showSignatureDialog}
        onOpenChange={setShowSignatureDialog}
        selectedDriverName={drivers.find(d => d.UID === selectedDriver)?.Name || "N/A"}
        organizationName="LBCL Logistics"
        onSave={handleSignatureSave}
      />

      {/* Success Message */}
      {showSuccessMessage && (
        <div className="fixed top-4 right-4 z-50 bg-green-500 text-white px-6 py-4 rounded-lg shadow-lg flex items-center gap-3 animate-in fade-in slide-in-from-top-5">
          <Check className="w-5 h-5" />
          <div>
            <p className="font-semibold">Success!</p>
            <p className="text-sm">Delivery plan submitted successfully</p>
          </div>
        </div>
      )}
    </div>
  );
}
