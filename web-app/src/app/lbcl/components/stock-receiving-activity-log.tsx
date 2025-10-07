"use client";

import { useState, useEffect } from "react";
import {
  ChevronDown,
  ChevronRight,
  Clock,
  Bell,
  RefreshCw,
  FileText
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { useRouter } from "next/navigation";
import { inventoryService } from "@/services/inventory/inventory.service";
import { stockReceivingService } from "@/services/stockReceivingService";
import { deliveryLoadingService } from "@/services/deliveryLoadingService";
import { DeliveryNoteDialog } from "@/app/lbcl/components/delivery-note-dialog";
import { roleService } from "@/services/admin/role.service";
import { employeeService } from "@/services/admin/employee.service";
import { openDeliveryNotePDFInNewTab } from "@/utils/deliveryNotePDF";
import { SuccessDialog } from "@/components/dialogs/success-dialog";
import { useAuth } from "@/providers/auth-provider";

export default function StockReceivingActivityLog({
  deliveryId,
  readOnly = false
}: {
  deliveryId: string;
  readOnly?: boolean;
}) {
  const router = useRouter();
  const { user } = useAuth();

  // Check user roles for field visibility
  const isSecurityOfficer = user?.roles?.some(role =>
    role.roleNameEn?.toUpperCase().includes("SECURITY") ||
    role.code?.toUpperCase().includes("SECURITY") ||
    role.uid?.toUpperCase() === "SECURITYOFFICER" ||
    role.code?.toUpperCase() === "SECURITYOFFICER"
  );

  const isOperator = user?.roles?.some(role =>
    role.roleNameEn?.toUpperCase().includes("OPERATOR") ||
    role.code?.toUpperCase().includes("OPERATOR")
  );

  const isAgent = user?.roles?.some(role =>
    role.roleNameEn?.toUpperCase().includes("AGENT") ||
    role.code?.toUpperCase().includes("AGENT")
  );

  console.log("🔐 Stock Receiving Activity Log - User Role Check:");
  console.log("   - Is Security Officer:", isSecurityOfficer);
  console.log("   - Is Operator:", isOperator);
  console.log("   - Is Agent:", isAgent);
  const [expandedSections, setExpandedSections] = useState<
    Record<number, boolean>
  >({
    2: true,
    4: true,
    6: true
  });
  const [showDeliveryNote, setShowDeliveryNote] = useState(false);
  const [purchaseOrder, setPurchaseOrder] = useState<any>(null);
  const [deliveryLoading, setDeliveryLoading] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [orderLines, setOrderLines] = useState<any[]>([]);

  // Security Officer states
  const [securityOfficers, setSecurityOfficers] = useState<any[]>([]);
  const [loadingSecurityOfficers, setLoadingSecurityOfficers] = useState(false);

  // Fork Lift Operator states
  const [forkLiftOperators, setForkLiftOperators] = useState<any[]>([]);
  const [loadingForkLiftOperators, setLoadingForkLiftOperators] =
    useState(false);

  // Form state
  const [securityOfficer, setSecurityOfficer] = useState("");
  const [arrivalHour, setArrivalHour] = useState("");
  const [arrivalMin, setArrivalMin] = useState("");
  const [forkLiftOperator, setForkLiftOperator] = useState("");
  const [unloadStartHour, setUnloadStartHour] = useState("");
  const [unloadStartMin, setUnloadStartMin] = useState("");
  const [unloadEndHour, setUnloadEndHour] = useState("");
  const [unloadEndMin, setUnloadEndMin] = useState("");
  const [loadEmptyStartHour, setLoadEmptyStartHour] = useState("");
  const [loadEmptyStartMin, setLoadEmptyStartMin] = useState("");
  const [loadEmptyEndHour, setLoadEmptyEndHour] = useState("");
  const [loadEmptyEndMin, setLoadEmptyEndMin] = useState("");
  const [getpassEmployee, setGetpassEmployee] = useState("");
  const [getpassHour, setGetpassHour] = useState("");
  const [getpassMin, setGetpassMin] = useState("");
  const [notifyLBCL, setNotifyLBCL] = useState(true);
  const [showSuccessDialog, setShowSuccessDialog] = useState(false);

  useEffect(() => {
    fetchData();
  }, [deliveryId]);

  // Load employees when purchase order is loaded
  useEffect(() => {
    console.log("🔄 useEffect triggered for loading employees");
    console.log("📦 Purchase Order state:", purchaseOrder);

    if (purchaseOrder) {
      console.log("✅ Purchase Order exists, checking for OrgUID");
      const orgUID =
        purchaseOrder?.OrgUID ||
        purchaseOrder?.org_uid ||
        purchaseOrder?.orgUID;
      console.log("🔍 Extracted OrgUID:", orgUID);

      if (orgUID) {
        console.log("✅ OrgUID found, loading employees...");
        loadSecurityOfficers();
        loadForkLiftOperators();
      } else {
        console.warn("⚠️ No OrgUID found in purchase order");
        console.log(
          "📋 Available purchase order fields:",
          Object.keys(purchaseOrder)
        );
      }
    } else {
      console.log("⏳ Purchase order not loaded yet");
    }
  }, [purchaseOrder]);

  const loadSecurityOfficers = async () => {
    try {
      setLoadingSecurityOfficers(true);

      // Get the distributor org UID from purchase order
      const distributorOrgUID =
        purchaseOrder?.OrgUID ||
        purchaseOrder?.org_uid ||
        purchaseOrder?.orgUID;
      if (!distributorOrgUID) {
        console.log("⚠️ No distributor org UID found in purchase order");
        console.log("📦 Available fields:", Object.keys(purchaseOrder || {}));
        return;
      }

      console.log(
        "🏢 Loading security officers for distributor org:",
        distributorOrgUID
      );

      // Get roles for the distributor organization
      const orgRoles = await roleService.getRolesByOrg(
        distributorOrgUID,
        false
      );
      console.log("🔍 Roles for distributor org:", orgRoles);

      let roles: any[] = [];
      if (orgRoles && orgRoles.length > 0) {
        roles = orgRoles;
      } else {
        // If no roles found for org, load all roles
        console.log("⚠️ No roles found for distributor org, loading all roles");
        const allRolesResult = await roleService.getRoles({
          pageNumber: 1,
          pageSize: 1000,
          isCountRequired: false
        });

        console.log("📊 All roles result:", allRolesResult);

        // Extract roles from pagedData or items array
        roles =
          allRolesResult.pagedData ||
          allRolesResult.data?.items ||
          allRolesResult.data ||
          [];
        console.log("📋 Extracted roles array:", roles);
        console.log("📋 Number of roles:", roles.length);
      }

      // Find the SECURITYOFFICER role - check multiple fields
      const securityRole = roles.find(
        (r: any) =>
          r.UID === "SECURITYOFFICER" ||
          r.uid === "SECURITYOFFICER" ||
          r.Code === "SECURITYOFFICER" ||
          r.code === "SECURITYOFFICER" ||
          r.RoleNameEn?.toLowerCase().includes("security") ||
          r.roleNameEn?.toLowerCase().includes("security")
      );

      if (securityRole) {
        const roleUID = securityRole.UID || securityRole.uid;
        console.log("🔑 Security Officer role UID:", roleUID);

        // Get employees with SECURITYOFFICER role for the distributor org
        console.log(
          `📋 Loading employees for org: ${distributorOrgUID}, role: ${roleUID}`
        );
        const employees =
          await employeeService.getEmployeesSelectionItemByRoleUID(
            distributorOrgUID,
            roleUID
          );
        console.log("👥 Security officers:", employees);

        if (employees && employees.length > 0) {
          const mappedEmployees = employees.map((emp: any) => ({
            UID: emp.UID || emp.Value,
            Name: emp.Label || emp.Name || `[${emp.Code}] ${emp.Name}`,
            Code: emp.Code
          }));

          console.log("✅ Mapped security officers:", mappedEmployees);
          setSecurityOfficers(mappedEmployees);

          // Auto-select the first officer
          if (mappedEmployees.length > 0) {
            setSecurityOfficer(mappedEmployees[0].UID);
          }
        } else {
          console.log("⚠️ No security officers found");
        }
      } else {
        console.log("⚠️ SECURITYOFFICER role not found");
      }
    } catch (error) {
      console.error("❌ Error loading security officers:", error);
    } finally {
      setLoadingSecurityOfficers(false);
    }
  };

  const loadForkLiftOperators = async () => {
    try {
      setLoadingForkLiftOperators(true);

      // Get the distributor org UID from purchase order
      const distributorOrgUID =
        purchaseOrder?.OrgUID ||
        purchaseOrder?.org_uid ||
        purchaseOrder?.orgUID;
      if (!distributorOrgUID) {
        console.log("⚠️ No distributor org UID found in purchase order");
        return;
      }

      console.log(
        "🏢 Loading fork lift operators for distributor org:",
        distributorOrgUID
      );

      // Get roles for the distributor organization
      const orgRoles = await roleService.getRolesByOrg(
        distributorOrgUID,
        false
      );
      console.log("🔍 Roles for distributor org:", orgRoles);

      let roles: any[] = [];
      if (orgRoles && orgRoles.length > 0) {
        roles = orgRoles;
      } else {
        // If no roles found for org, load all roles
        console.log("⚠️ No roles found for distributor org, loading all roles");
        const allRolesResult = await roleService.getRoles({
          pageNumber: 1,
          pageSize: 1000,
          isCountRequired: false
        });

        // Extract roles from pagedData or items array
        roles =
          allRolesResult.pagedData ||
          allRolesResult.data?.items ||
          allRolesResult.data ||
          [];
        console.log("📋 Extracted roles array:", roles);
      }

      // Find the OPERATOR role - check multiple fields
      const operatorRole = roles.find(
        (r: any) =>
          r.UID === "OPERATOR" ||
          r.uid === "OPERATOR" ||
          r.Code === "OPERATOR" ||
          r.code === "OPERATOR" ||
          r.RoleNameEn?.toLowerCase().includes("operator") ||
          r.roleNameEn?.toLowerCase().includes("operator")
      );

      if (operatorRole) {
        const roleUID = operatorRole.UID || operatorRole.uid;
        console.log("🔑 Operator role UID:", roleUID);

        // Get employees with OPERATOR role for the distributor org
        console.log(
          `📋 Loading employees for org: ${distributorOrgUID}, role: ${roleUID}`
        );
        const employees =
          await employeeService.getEmployeesSelectionItemByRoleUID(
            distributorOrgUID,
            roleUID
          );
        console.log("👥 Fork lift operators:", employees);

        if (employees && employees.length > 0) {
          const mappedEmployees = employees.map((emp: any) => ({
            UID: emp.UID || emp.Value,
            Name: emp.Label || emp.Name || `[${emp.Code}] ${emp.Name}`,
            Code: emp.Code
          }));

          console.log("✅ Mapped fork lift operators:", mappedEmployees);
          setForkLiftOperators(mappedEmployees);

          // Auto-select the first operator
          if (mappedEmployees.length > 0) {
            setForkLiftOperator(mappedEmployees[0].UID);
          }
        } else {
          console.log("⚠️ No fork lift operators found");
        }
      } else {
        console.log("⚠️ OPERATOR role not found");
      }
    } catch (error) {
      console.error("❌ Error loading fork lift operators:", error);
    } finally {
      setLoadingForkLiftOperators(false);
    }
  };

  const fetchData = async () => {
    try {
      setLoading(true);
      setError("");

      // Fetch both WH stock request and delivery loading tracking in parallel
      const [whResponse, dlResponse] = await Promise.all([
        inventoryService.selectLoadRequestDataByUID(deliveryId),
        deliveryLoadingService.getByWHStockRequestUID(deliveryId)
      ]);

      console.log("🔍 WH Stock Request Response:", whResponse);
      console.log("🔍 Delivery Loading Response:", dlResponse);

      if (whResponse) {
        const header = whResponse.WHStockRequest;
        const lines = whResponse.WHStockRequestLines || [];

        console.log("📦 WH Stock Request Header:", header);
        console.log("📦 WH Stock Request Lines:", lines);

        // Transform to match expected format
        const transformedHeader = {
          UID: header.UID,
          Code: header.Code,
          OrderNumber: header.Code,
          OrgUID: header.TargetOrgUID,
          orgUID: header.TargetOrgUID,
          WarehouseName: header.TargetWHName,
          TargetOrgName: header.TargetOrgName,
          TargetWHName: header.TargetWHName,
          RequiredByDate: header.RequiredByDate,
          RequestedTime: header.RequestedTime
        };

        setPurchaseOrder(transformedHeader);
        setOrderLines(lines);

        console.log("🏢 WH Stock Request TargetOrgUID:", header.TargetOrgUID);
      } else {
        setError("Failed to load WH stock request");
      }

      // Delivery loading might not exist yet (it's created during delivery loading activity log)
      if (dlResponse) {
        console.log("🚚 Delivery Loading Data:", dlResponse);
        console.log(
          "🚚 DL Keys:",
          dlResponse ? Object.keys(dlResponse) : "null"
        );
        setDeliveryLoading(dlResponse);
      } else {
        console.log(
          "⚠️ No delivery loading data found - this is normal if delivery hasn't been loaded yet"
        );
      }
    } catch (error) {
      console.error("Error fetching data:", error);
      setError("Failed to load data");
    } finally {
      setLoading(false);
    }
  };

  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date
      .toLocaleDateString("en-GB", {
        day: "2-digit",
        month: "short",
        year: "numeric"
      })
      .toUpperCase();
  };

  const formatTime = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return (
      date.toLocaleTimeString("en-GB", {
        hour: "2-digit",
        minute: "2-digit",
        hour12: false
      }) + " HRS"
    );
  };

  const handleSubmit = async () => {
    try {
      const now = new Date();
      const todayDate = now.toISOString().split("T")[0];

      const arrivalTime =
        arrivalHour && arrivalMin
          ? `${todayDate}T${arrivalHour.padStart(2, "0")}:${arrivalMin.padStart(
              2,
              "0"
            )}:00`
          : null;

      const unloadingStartTime =
        unloadStartHour && unloadStartMin
          ? `${todayDate}T${unloadStartHour.padStart(
              2,
              "0"
            )}:${unloadStartMin.padStart(2, "0")}:00`
          : null;

      const unloadingEndTime =
        unloadEndHour && unloadEndMin
          ? `${todayDate}T${unloadEndHour.padStart(
              2,
              "0"
            )}:${unloadEndMin.padStart(2, "0")}:00`
          : null;

      const loadEmptyStockTime =
        loadEmptyStartHour && loadEmptyStartMin
          ? `${todayDate}T${loadEmptyStartHour.padStart(
              2,
              "0"
            )}:${loadEmptyStartMin.padStart(2, "0")}:00`
          : null;

      const getpassTime =
        getpassHour && getpassMin
          ? `${todayDate}T${getpassHour.padStart(2, "0")}:${getpassMin.padStart(
              2,
              "0"
            )}:00`
          : null;

      const stockReceivingData = {
        WHStockRequestUID: deliveryId,
        ReceiverName: null,
        ReceiverEmployeeCode: securityOfficer || null,
        ForkLiftOperatorUID: forkLiftOperator || null,
        LoadEmptyStockEmployeeUID: forkLiftOperator || null,
        GetpassEmployeeUID: getpassEmployee || null,
        ArrivalTime: arrivalTime,
        UnloadingStartTime: unloadingStartTime,
        UnloadingEndTime: unloadingEndTime,
        LoadEmptyStockTime: loadEmptyStockTime,
        GetpassTime: getpassTime,
        PhysicalCountStartTime: null,
        PhysicalCountEndTime: null,
        ReceiverSignature: null,
        Notes: null,
        Status: "COMPLETED",
        IsActive: true
      };

      console.log("💾 Saving stock receiving data:", stockReceivingData);

      await stockReceivingService.saveStockReceivingTracking(
        stockReceivingData
      );

      // Calculate completion time
      let completionTime = "N/A";
      if (unloadingStartTime && unloadingEndTime) {
        const start = new Date(unloadingStartTime);
        const end = new Date(unloadingEndTime);
        const diffMs = end.getTime() - start.getTime();
        const diffMins = Math.round(diffMs / 60000);
        completionTime = `${diffMins} Min`;
      }

      // Format times for display
      const formatTime = (timeStr: string | null) => {
        if (!timeStr) return "N/A";
        const date = new Date(timeStr);
        return date.toLocaleTimeString("en-US", {
          hour: "2-digit",
          minute: "2-digit",
          hour12: true
        });
      };

      // Show success dialog
      setShowSuccessDialog(true);
    } catch (error) {
      console.error("Error saving stock receiving:", error);
      alert("Failed to save stock receiving activity log");
    }
  };

  const toggleSection = (section: number) => {
    setExpandedSections((prev) => ({ ...prev, [section]: !prev[section] }));
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="w-8 h-8 text-[#A08B5C] animate-spin mx-auto mb-2" />
          <p className="text-gray-600">Loading purchase order...</p>
        </div>
      </div>
    );
  }

  if (error || !purchaseOrder) {
    return (
      <div className="min-h-screen bg-white flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 text-center max-w-md">
          <p className="text-red-700">{error || "Purchase order not found"}</p>
          <button
            onClick={fetchData}
            className="mt-2 text-sm text-red-600 hover:text-red-800 underline"
          >
            Try again
          </button>
        </div>
      </div>
    );
  }

  // Debug: Log the purchase order data when rendering
  console.log("🎨 Rendering with purchaseOrder:", purchaseOrder);
  console.log(
    "🎨 Purchase Order Keys:",
    purchaseOrder ? Object.keys(purchaseOrder) : "no PO"
  );
  console.log("🎨 order_number:", purchaseOrder?.order_number);
  console.log("🎨 OrderNumber:", purchaseOrder?.OrderNumber);
  console.log("🎨 order_date:", purchaseOrder?.order_date);

  return (
    <div className="min-h-screen bg-white">
      {/* Header */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-between sticky top-0 z-10">
        <h1 className="text-base sm:text-lg md:text-xl font-bold text-center flex-1 px-2">
          Agent Stock Receiving Activity Log Report
        </h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            className="border-[#A08B5C] text-[#A08B5C] bg-transparent"
            onClick={() => router.back()}
          >
            Back
          </Button>
          {!readOnly && (
            <Button
              className="bg-[#A08B5C] hover:bg-[#8A7549] text-white"
              onClick={handleSubmit}
            >
              Submit
            </Button>
          )}
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 text-sm">
          <div>
            <div className="text-gray-600 mb-1">Agent Stock Receiving No</div>
            <div className="font-bold">
              ASR
              {purchaseOrder.order_number ||
                purchaseOrder.OrderNumber ||
                purchaseOrder.orderNumber ||
                deliveryId}
            </div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">SAP Delivery Note No</div>
            <div className="font-bold">
              {deliveryLoading?.DeliveryNoteNumber ||
                deliveryLoading?.deliveryNoteNumber ||
                "N/A"}
            </div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Distributor</div>
            <div className="font-bold">
              {deliveryLoading?.org_name ||
                deliveryLoading?.OrgName ||
                deliveryLoading?.orgName ||
                purchaseOrder.org_name ||
                purchaseOrder.OrgName ||
                purchaseOrder.orgName ||
                "N/A"}
            </div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Prime Mover</div>
            <div className="font-bold">
              {deliveryLoading?.VehicleUID ||
                deliveryLoading?.vehicleUID ||
                "N/A"}
            </div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">Date</div>
            <div className="font-bold">
              {formatDate(
                deliveryLoading?.order_date ||
                  purchaseOrder.order_date ||
                  purchaseOrder.OrderDate ||
                  purchaseOrder.orderDate ||
                  ""
              )}
            </div>
          </div>
          <div>
            <div className="text-gray-600 mb-1">LBCL Departure Time</div>
            <div className="font-bold">
              {formatTime(
                deliveryLoading?.DepartureTime ||
                  deliveryLoading?.departureTime ||
                  ""
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Activity Steps */}
      <div className="p-4 grid grid-cols-1 lg:grid-cols-2 gap-4">
        {/* Left Column */}
        <div className="space-y-4">
          {/* Step 1: View Delivery Note */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => openDeliveryNotePDFInNewTab(purchaseOrder, orderLines)}
              className="flex items-center justify-between w-full"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  1
                </div>
                <span className="font-semibold">View Delivery Note</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-red-600 text-sm">PDF</span>
                <ChevronRight className="w-5 h-5" />
              </div>
            </button>
          </div>

          {/* Step 2: Gate Entry */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => toggleSection(2)}
              className="flex items-center justify-between w-full mb-4"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  2
                </div>
                <span className="font-semibold">Gate Entry</span>
              </div>
              {expandedSections[2] ? (
                <ChevronDown className="w-5 h-5" />
              ) : (
                <ChevronRight className="w-5 h-5" />
              )}
            </button>

            {expandedSections[2] && (
              <div className="space-y-4 pl-13">
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Select Security Officer
                    </label>
                    <Select
                      value={securityOfficer}
                      onValueChange={setSecurityOfficer}
                      disabled={loadingSecurityOfficers || readOnly || !isSecurityOfficer}
                    >
                      <SelectTrigger className={`${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                        <SelectValue
                          placeholder={
                            loadingSecurityOfficers
                              ? "Loading security officers..."
                              : "Select security officer"
                          }
                        />
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
                    <label className="text-sm font-medium mb-2 block">
                      Prime Mover Arrival
                    </label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="15"
                        className={`w-20 ${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={arrivalHour}
                        onChange={(e) => setArrivalHour(e.target.value)}
                        disabled={readOnly || !isSecurityOfficer}
                      />
                      <span className="text-gray-400 self-center">HH</span>
                      <Input
                        type="number"
                        placeholder="00"
                        className={`w-20 ${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={arrivalMin}
                        onChange={(e) => setArrivalMin(e.target.value)}
                        disabled={readOnly || !isSecurityOfficer}
                      />
                      <span className="text-gray-400 self-center">Min</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox
                    id="notify-lbcl"
                    checked={notifyLBCL}
                    onCheckedChange={(checked) =>
                      setNotifyLBCL(checked as boolean)
                    }
                    disabled={readOnly || !isSecurityOfficer}
                  />
                  <label htmlFor="notify-lbcl" className="text-sm font-medium">
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>

          {/* Step 3: Physical Count */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => router.push(`/lbcl/stock-receiving/${deliveryId}`)}
              className="flex items-center justify-between w-full"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  3
                </div>
                <span className="font-semibold">
                  Physical Count & Perform Stock Receiving
                </span>
              </div>
              <ChevronRight className="w-5 h-5" />
            </button>
          </div>
        </div>

        {/* Right Column */}
        <div className="space-y-4">
          {/* Step 4: Unloading */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => toggleSection(4)}
              className="flex items-center justify-between w-full mb-4"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  4
                </div>
                <span className="font-semibold">Unloading</span>
              </div>
              {expandedSections[4] ? (
                <ChevronDown className="w-5 h-5" />
              ) : (
                <ChevronRight className="w-5 h-5" />
              )}
            </button>

            {expandedSections[4] && (
              <div className="space-y-4 pl-13">
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Agent Fork Lift Operator
                  </label>
                  <Select
                    value={forkLiftOperator}
                    onValueChange={setForkLiftOperator}
                    disabled={loadingForkLiftOperators || readOnly || !isOperator}
                  >
                    <SelectTrigger className={`${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                      <SelectValue
                        placeholder={
                          loadingForkLiftOperators
                            ? "Loading operators..."
                            : "Select fork lift operator"
                        }
                      />
                    </SelectTrigger>
                    <SelectContent>
                      {forkLiftOperators.map((operator) => (
                        <SelectItem key={operator.UID} value={operator.UID}>
                          {operator.Name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Unload Start Time
                    </label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="16"
                        className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={unloadStartHour}
                        onChange={(e) => setUnloadStartHour(e.target.value)}
                        disabled={readOnly || !isOperator}
                      />
                      <span className="text-gray-400 self-center text-xs">
                        HH
                      </span>
                      <Input
                        type="number"
                        placeholder="02"
                        className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={unloadStartMin}
                        onChange={(e) => setUnloadStartMin(e.target.value)}
                        disabled={readOnly || !isOperator}
                      />
                      <span className="text-gray-400 self-center text-xs">
                        Min
                      </span>
                    </div>
                  </div>
                  <div>
                    <label className="text-sm font-medium mb-2 block">
                      Unload End Time
                    </label>
                    <div className="flex gap-2">
                      <Input
                        type="number"
                        placeholder="16"
                        className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={unloadEndHour}
                        onChange={(e) => setUnloadEndHour(e.target.value)}
                        disabled={readOnly || !isOperator}
                      />
                      <span className="text-gray-400 self-center text-xs">
                        HH
                      </span>
                      <Input
                        type="number"
                        placeholder="58"
                        className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                        value={unloadEndMin}
                        onChange={(e) => setUnloadEndMin(e.target.value)}
                        disabled={readOnly || !isOperator}
                      />
                      <span className="text-gray-400 self-center text-xs">
                        Min
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>

          {/* Step 5: Load Empty Stock */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button className="flex items-center justify-between w-full mb-4">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  5
                </div>
                <span className="font-semibold">Load Empty Stock</span>
              </div>
              <ChevronRight className="w-5 h-5" />
            </button>

            <div className="space-y-4 pl-13">
              <div>
                <label className="text-sm font-medium mb-2 block">
                  Agent Fork Lift Operator
                </label>
                <Select
                  value={forkLiftOperator}
                  onValueChange={setForkLiftOperator}
                  disabled={loadingForkLiftOperators || readOnly || !isOperator}
                >
                  <SelectTrigger className={`${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                    <SelectValue
                      placeholder={
                        loadingForkLiftOperators
                          ? "Loading operators..."
                          : "Select fork lift operator"
                      }
                    />
                  </SelectTrigger>
                  <SelectContent>
                    {forkLiftOperators.map((operator) => (
                      <SelectItem key={operator.UID} value={operator.UID}>
                        {operator.Name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Load Start Time
                  </label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="17"
                      className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={loadEmptyStartHour}
                      onChange={(e) => setLoadEmptyStartHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled={readOnly || !isOperator}
                    />
                    <span className="text-gray-400 self-center text-xs">
                      HH
                    </span>
                    <Input
                      type="number"
                      placeholder="25"
                      className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={loadEmptyStartMin}
                      onChange={(e) => setLoadEmptyStartMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled={readOnly || !isOperator}
                    />
                    <span className="text-gray-400 self-center text-xs">
                      Min
                    </span>
                  </div>
                </div>
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Load End Time
                  </label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="18"
                      className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={loadEmptyEndHour}
                      onChange={(e) => setLoadEmptyEndHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled={readOnly || !isOperator}
                    />
                    <span className="text-gray-400 self-center text-xs">
                      HH
                    </span>
                    <Input
                      type="number"
                      placeholder="08"
                      className={`w-16 ${(readOnly || !isOperator) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={loadEmptyEndMin}
                      onChange={(e) => setLoadEmptyEndMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled={readOnly || !isOperator}
                    />
                    <span className="text-gray-400 self-center text-xs">
                      Min
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Step 6: Gate Pass */}
          <div className="bg-white border border-gray-200 rounded-lg p-4">
            <button
              onClick={() => toggleSection(6)}
              className="flex items-center justify-between w-full mb-4"
            >
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 bg-[#F5E6D3] rounded-full flex items-center justify-center font-bold">
                  6
                </div>
                <span className="font-semibold">Gate Pass (Empties)</span>
              </div>
              {expandedSections[6] ? (
                <ChevronDown className="w-5 h-5" />
              ) : (
                <ChevronRight className="w-5 h-5" />
              )}
            </button>

            {expandedSections[6] && (
              <div className="space-y-4 pl-13">
                <div>
                  <label className="text-sm font-medium mb-2 block">
                    Getpass Employee (Security Officer)
                  </label>
                  <Select
                    value={getpassEmployee}
                    onValueChange={setGetpassEmployee}
                    disabled={loadingSecurityOfficers || readOnly || !isSecurityOfficer}
                  >
                    <SelectTrigger className={`${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}>
                      <SelectValue
                        placeholder={
                          loadingSecurityOfficers
                            ? "Loading security officers..."
                            : "Select getpass employee"
                        }
                      />
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
                  <label className="text-sm font-medium mb-2 block">
                    Getpass Time (Prime Mover Departure)
                  </label>
                  <div className="flex gap-2">
                    <Input
                      type="number"
                      placeholder="18"
                      className={`w-20 ${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={getpassHour}
                      onChange={(e) => setGetpassHour(e.target.value)}
                      min="0"
                      max="23"
                      disabled={readOnly || !isSecurityOfficer}
                    />
                    <span className="text-gray-400 self-center">HH</span>
                    <Input
                      type="number"
                      placeholder="14"
                      className={`w-20 ${(readOnly || !isSecurityOfficer) ? 'bg-gray-50 text-gray-900 font-medium cursor-default opacity-100' : ''}`}
                      value={getpassMin}
                      onChange={(e) => setGetpassMin(e.target.value)}
                      min="0"
                      max="59"
                      disabled={readOnly || !isSecurityOfficer}
                    />
                    <span className="text-gray-400 self-center">Min</span>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Checkbox id="notify-lbcl-2" defaultChecked disabled={readOnly || !isSecurityOfficer} />
                  <label
                    htmlFor="notify-lbcl-2"
                    className="text-sm font-medium"
                  >
                    Notify LBCL Logistics
                  </label>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Delivery Note Dialog */}
      <DeliveryNoteDialog
        open={showDeliveryNote}
        onOpenChange={setShowDeliveryNote}
        orderLines={orderLines}
        purchaseOrder={purchaseOrder}
      />

      {/* Success Dialog */}
      <SuccessDialog
        open={showSuccessDialog}
        onOpenChange={setShowSuccessDialog}
        title="Success"
        message="Stock receiving activity completed successfully"
        showPrintButton={false}
        onDone={() => {
          setShowSuccessDialog(false);
          router.push("/lbcl/stock-receiving");
        }}
      />
    </div>
  );
}
