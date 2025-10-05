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
import { PickListDialog } from "@/app/lbcl/components/pick-list-dialog";
import { DeliveryNoteDialog } from "@/app/lbcl/components/delivery-note-dialog";
import { SignatureDialog } from "@/app/lbcl/components/signature-dialog";
import purchaseOrderService from "@/services/purchaseOrder";
import { vehicleService, Vehicle } from "@/services/vehicleService";
import { organizationService } from "@/services/organizationService";
import { employeeService } from "@/services/admin/employee.service";
import { roleService } from "@/services/admin/role.service";
import { deliveryLoadingService } from "@/services/deliveryLoadingService";

interface ActivityLogPageProps {
  deliveryPlanId: string;
}

export function ActivityLogPage({ deliveryPlanId }: ActivityLogPageProps) {
  const router = useRouter();
  const [expandedSections, setExpandedSections] = useState<number[]>([3, 6]);
  const [showShareDialog, setShowShareDialog] = useState(false);
  const [showPickListDialog, setShowPickListDialog] = useState(false);
  const [showDeliveryNoteDialog, setShowDeliveryNoteDialog] = useState(false);
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);
  const [showSuccessMessage, setShowSuccessMessage] = useState(false);

  const handleSignatureSave = async (logisticsSignature: string, driverSignature: string, signatureNotes: string) => {
    try {
      // Create delivery loading tracking data
      const now = new Date();
      const todayDate = now.toISOString().split('T')[0]; // YYYY-MM-DD

      // Combine hour and minute for times
      const loadingStartTime = `${todayDate}T${loadingStartHour.padStart(2, '0')}:${loadingStartMin.padStart(2, '0')}:00`;
      const loadingEndTime = `${todayDate}T${loadingEndHour.padStart(2, '0')}:${loadingEndMin.padStart(2, '0')}:00`;
      const departureTime = `${todayDate}T${departureHour.padStart(2, '0')}:${departureMin.padStart(2, '0')}:00`;

      const deliveryLoadingData = {
        PurchaseOrderUID: purchaseOrder.UID || purchaseOrder.uid,
        VehicleUID: selectedVehicle || null,
        DriverEmployeeUID: selectedDriver || null,
        ForkLiftOperatorUID: selectedOperator || null,
        SecurityOfficerUID: selectedSecurityOfficer || null,
        ArrivalTime: arrivalTime || null,
        LoadingStartTime: loadingStartTime,
        LoadingEndTime: loadingEndTime,
        DepartureTime: departureTime,
        LogisticsSignature: logisticsSignature || null,
        DriverSignature: driverSignature || null,
        Notes: signatureNotes || notes || null,
        IsActive: true
      };

      console.log("📦 Delivery Loading Data being sent:", {
        VehicleUID: selectedVehicle,
        DriverEmployeeUID: selectedDriver,
        ForkLiftOperatorUID: selectedOperator,
        SecurityOfficerUID: selectedSecurityOfficer,
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

      const response = await purchaseOrderService.getPurchaseOrderMasterByUID(deliveryPlanId);

      if (response.success && response.master) {
        const header = response.master.PurchaseOrderHeader || response.master.purchaseOrderHeader;
        console.log('🔍 Purchase Order Header:', header);
        console.log('🏢 OrgName:', header?.OrgName, header?.orgName);
        console.log('🏭 WarehouseName:', header?.WarehouseName, header?.warehouseName);

        setPurchaseOrder(header);
        setOrderLines(response.master.PurchaseOrderLines || response.master.purchaseOrderLines || []);
      } else {
        setError(response.error || "Failed to fetch delivery plan data");
      }
    } catch (error) {
      console.error("Error fetching purchase order:", error);
      setError("Failed to load delivery plan data");
    } finally {
      setLoading(false);
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
          <Button
            onClick={() => setShowSignatureDialog(true)}
            className="bg-[#A08B5C] hover:bg-[#8F7A4B] text-white text-xs sm:text-sm px-3 sm:px-6"
          >
            Submit
          </Button>
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
            onClick={() => setShowShareDialog(true)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
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
            onClick={() => setShowPickListDialog(true)}
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
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Vehicle / Prime Mover
                    </label>
                    <Select
                      value={selectedVehicle}
                      onValueChange={setSelectedVehicle}
                      disabled={loadingVehicles}
                    >
                      <SelectTrigger>
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
                      disabled={loadingDrivers}
                    >
                      <SelectTrigger>
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
                      disabled={loadingOperators}
                    >
                      <SelectTrigger>
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
                          className="text-center"
                        />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input
                          value={loadingStartMin}
                          onChange={(e) => setLoadingStartMin(e.target.value)}
                          className="text-center"
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
                          className="text-center"
                        />
                        <span className="text-xs text-gray-500 self-center">
                          HH
                        </span>
                        <Input
                          value={loadingEndMin}
                          onChange={(e) => setLoadingEndMin(e.target.value)}
                          className="text-center"
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
            onClick={() => setShowDeliveryNoteDialog(true)}
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
            onClick={() => setShowSignatureDialog(true)}
            className="bg-white rounded-lg p-4 sm:p-6 shadow-sm hover:shadow-md transition-shadow"
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
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <div>
                    <label className="text-xs sm:text-sm font-medium mb-2 block">
                      Select Security Officer
                    </label>
                    <Select
                      value={selectedSecurityOfficer}
                      onValueChange={setSelectedSecurityOfficer}
                      disabled={loadingSecurityOfficers}
                    >
                      <SelectTrigger>
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
                        className="text-center flex-1"
                      />
                      <span className="text-xs text-gray-500 self-center">
                        HH
                      </span>
                      <Input
                        value={departureMin}
                        onChange={(e) => setDepartureMin(e.target.value)}
                        className="text-center flex-1"
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
      <ShareDialog open={showShareDialog} onOpenChange={setShowShareDialog} />
      <PickListDialog
        open={showPickListDialog}
        onOpenChange={setShowPickListDialog}
        orderLines={orderLines}
        purchaseOrder={purchaseOrder}
      />
      <DeliveryNoteDialog
        open={showDeliveryNoteDialog}
        onOpenChange={setShowDeliveryNoteDialog}
        orderLines={orderLines}
        purchaseOrder={purchaseOrder}
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
