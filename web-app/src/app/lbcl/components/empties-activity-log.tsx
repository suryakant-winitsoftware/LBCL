"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ChevronDown, ChevronRight, FileText, Check } from "lucide-react";
import { RateAgentModal } from "@/app/lbcl/components/rate-agent-modal";
import { SignatureDialog } from "@/app/lbcl/components/signature-dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { openDeliveryNotePDFInNewTab } from "@/utils/deliveryNotePDF";

interface ActivityStep {
  id: number;
  title: string;
  completed: boolean;
  expandable?: boolean;
}

export function EmptiesActivityLog() {
  const router = useRouter();
  const [expandedStep, setExpandedStep] = useState<number | null>(2);
  const [showRateAgent, setShowRateAgent] = useState(false);
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);
  const [showSuccessMessage, setShowSuccessMessage] = useState(false);
  const [purchaseOrder, setPurchaseOrder] = useState<any>(null);
  const [orderLines, setOrderLines] = useState<any[]>([]);
  const [deliveryLoading, setDeliveryLoading] = useState<any>(null);

  // Initialize mock data for empties delivery
  useEffect(() => {
    // Get today's date
    const today = new Date();
    const todayISO = today.toISOString();

    // Mock purchase order data
    setPurchaseOrder({
      UID: "EMT85444127121",
      Code: "EMT85444127121",
      OrderNumber: "EMT85444127121",
      OrgUID: "DIST001",
      WarehouseName: "Main Warehouse",
      TargetOrgName: "R.T DISTRIBUTORS",
      RequiredByDate: todayISO,
      RequestedTime: todayISO,
      OrderDate: todayISO,
      DeliveryDate: todayISO
    });

    // Mock order lines
    setOrderLines([
      {
        SKUCode: "5213",
        SKUName: "Short Quarter Keg 7.75 Galon Beers",
        RequestedQty: 25,
        ApprovedQty: 25,
        Unit: "Dozen"
      },
      {
        SKUCode: "5214",
        SKUName: "Slim Quarter Keg 7.75 Galon",
        RequestedQty: 10,
        ApprovedQty: 10,
        Unit: "Dozen"
      },
      {
        SKUCode: "5216",
        SKUName: "Lion Large Beer bottle 625ml",
        RequestedQty: 20,
        ApprovedQty: 20,
        Unit: "Dozen"
      }
    ]);

    // Mock delivery loading data
    setDeliveryLoading({
      DeliveryNoteNumber: "EMT85444127121",
      OrgName: "R.T DISTRIBUTORS",
      VehicleUID: "LK1673",
      DepartureTime: todayISO,
      DeliveryDate: todayISO
    });
  }, []);

  const steps: ActivityStep[] = [
    { id: 1, title: "View Empties Delivery Note", completed: false },
    { id: 2, title: "Gate Entry", completed: false, expandable: true },
    { id: 3, title: "Physical Count & Audit Empties", completed: false },
    { id: 4, title: "View Empties Damage Result", completed: false },
    { id: 5, title: "Rate Agent", completed: false },
    { id: 6, title: "Unloading", completed: false, expandable: true },
    {
      id: 7,
      title: "Send Empty Detail Acknowledgement Note to Agent",
      completed: false
    }
  ];

  // Handle delivery note view
  const handleViewDeliveryNote = () => {
    const deliveryNotePath = deliveryLoading?.DeliveryNoteFilePath;

    if (deliveryNotePath) {
      // Open saved PDF file
      console.log("📄 Opening saved empties delivery note:", deliveryNotePath);
      const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:8000/api";
      const fileUrl = `${apiUrl.replace('/api', '')}/${deliveryNotePath}`;
      window.open(fileUrl, "_blank");
    } else {
      // Generate PDF on-the-fly with empties delivery note format
      console.log("📄 Generating empties delivery note PDF");
      openDeliveryNotePDFInNewTab(purchaseOrder, orderLines, true);
    }
  };

  const handleStepClick = (stepId: number) => {
    if (stepId === 1) {
      handleViewDeliveryNote();
    } else if (stepId === 3) {
      router.push("/lbcl/empties-receiving/physical-count");
    } else if (stepId === 4) {
      router.push("/lbcl/empties-receiving/damage-results");
    } else if (stepId === 5) {
      setShowRateAgent(true);
    } else if (steps.find((s) => s.id === stepId)?.expandable) {
      setExpandedStep(expandedStep === stepId ? null : stepId);
    }
  };

  const handleSubmit = () => {
    setShowSignatureDialog(true);
  };

  const handleSignatureSave = (logisticsSignature: string, driverSignature: string, signatureNotes: string) => {
    console.log("Signatures saved:", { logisticsSignature, driverSignature, signatureNotes });
    setShowSignatureDialog(false);
    setShowSuccessMessage(true);

    // Hide success message after 3 seconds
    setTimeout(() => {
      setShowSuccessMessage(false);
      router.push("/lbcl/empties-receiving");
    }, 3000);
  };

  const leftColumnSteps = steps.slice(0, 4);
  const rightColumnSteps = steps.slice(4, 7);

  // Format date function
  const formatDate = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
  };

  // Format time function
  const formatTime = (dateString: string) => {
    if (!dateString) return "N/A";
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
      hour12: false
    }) + ' HRS';
  };

  return (
    <>
      <div className="min-h-screen bg-gray-50">
        {/* Info Section */}
        <div className="bg-white p-6 border-b border-gray-200">
          <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-6">
            <div>
              <div className="text-xs text-gray-600 mb-2">
                Empties Delivery No
              </div>
              <div className="font-bold text-base text-gray-900">EMT85444127121</div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-2">Distributor</div>
              <div className="font-bold text-base text-gray-900">
                [5844] R.T DISTRIBUTOR
              </div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-2">Prime Mover</div>
              <div className="font-bold text-base text-gray-900">{deliveryLoading?.VehicleUID || 'LK1673'}</div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-2">Date</div>
              <div className="font-bold text-base text-gray-900">
                {deliveryLoading?.DepartureTime ? formatDate(deliveryLoading.DepartureTime) : formatDate(new Date().toISOString())}
              </div>
            </div>
            <div>
              <div className="text-xs text-gray-600 mb-2">
                Agent Departure Time
              </div>
              <div className="font-bold text-base text-gray-900">
                {deliveryLoading?.DepartureTime ? formatTime(deliveryLoading.DepartureTime) : formatTime(new Date().toISOString())}
              </div>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="p-6 flex gap-3 justify-end bg-white border-b border-gray-200">
          <button
            onClick={() => router.back()}
            className="py-2.5 px-6 bg-white border-2 border-[#A08B5C] text-[#A08B5C] rounded-lg text-sm font-semibold hover:bg-gray-50 transition-colors"
          >
            Back
          </button>
          <button
            onClick={handleSubmit}
            className="py-2.5 px-6 bg-[#A08B5C] text-white rounded-lg text-sm font-semibold hover:bg-[#8F7A4D] transition-colors"
          >
            Submit
          </button>
        </div>

        <div className="p-6 grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left Column - Steps 1-4 */}
          <div className="space-y-4">
            {leftColumnSteps.map((step) => (
              <div
                key={step.id}
                className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
              >
                <button
                  onClick={() => handleStepClick(step.id)}
                  className="w-full p-4 flex items-center gap-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="w-10 h-10 rounded-full bg-[#FFF8E7] flex items-center justify-center flex-shrink-0">
                    <span className="font-bold text-base text-[#A08B5C]">{step.id}</span>
                  </div>
                  <div className="flex-1 text-left">
                    <div className="font-semibold text-base text-gray-900">
                      {step.title}
                    </div>
                  </div>
                  {step.id === 1 && (
                    <FileText className="w-6 h-6 text-red-600" />
                  )}
                  {step.expandable && (
                    <ChevronDown
                      className={`w-6 h-6 text-gray-400 transition-transform ${
                        expandedStep === step.id ? "rotate-180" : ""
                      }`}
                    />
                  )}
                  {!step.expandable && step.id !== 1 && (
                    <ChevronRight className="w-6 h-6 text-gray-400" />
                  )}
                </button>

                {/* Expanded Content for Gate Entry */}
                {step.id === 2 && expandedStep === 2 && (
                  <div className="px-5 pb-5 space-y-5 border-t border-gray-200 pt-5 bg-gray-50">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 items-start">
                      <div className="flex flex-col">
                        <label className="text-sm font-medium text-gray-700 mb-2">
                          Select Security Officer
                        </label>
                        <Select defaultValue="vasanth">
                          <SelectTrigger className="h-[42px]">
                            <SelectValue placeholder="Select security officer" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="vasanth">Vasanth Kumar</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                      <div className="flex flex-col">
                        <label className="text-sm font-medium text-gray-700 mb-2">
                          Prime Mover Arrival
                        </label>
                        <div className="flex gap-3">
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="19"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              HH
                            </span>
                          </div>
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="00"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              Min
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <div className="relative inline-flex items-center">
                        <input
                          type="checkbox"
                          id="notify"
                          defaultChecked
                          className="peer w-5 h-5 rounded border-2 border-[#A08B5C] appearance-none cursor-pointer checked:bg-[#A08B5C] checked:border-[#A08B5C] transition-colors"
                        />
                        <svg
                          className="absolute w-3 h-3 left-1 top-1 text-white pointer-events-none hidden peer-checked:block"
                          fill="none"
                          stroke="currentColor"
                          viewBox="0 0 24 24"
                        >
                          <path
                            strokeLinecap="round"
                            strokeLinejoin="round"
                            strokeWidth={4}
                            d="M5 13l4 4L19 7"
                          />
                        </svg>
                      </div>
                      <label
                        htmlFor="notify"
                        className="text-sm font-medium text-gray-700 cursor-pointer"
                      >
                        Notify LBCL Logistics
                      </label>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>

          {/* Right Column - Steps 5-7 */}
          <div className="space-y-4">
            {rightColumnSteps.map((step) => (
              <div
                key={step.id}
                className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow"
              >
                <button
                  onClick={() => handleStepClick(step.id)}
                  className="w-full p-4 flex items-center gap-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="w-10 h-10 rounded-full bg-[#FFF8E7] flex items-center justify-center flex-shrink-0">
                    <span className="font-bold text-base text-[#A08B5C]">{step.id}</span>
                  </div>
                  <div className="flex-1 text-left">
                    <div className="font-semibold text-base text-gray-900">
                      {step.title}
                    </div>
                  </div>
                  {step.expandable && (
                    <ChevronDown
                      className={`w-6 h-6 text-gray-400 transition-transform ${
                        expandedStep === step.id ? "rotate-180" : ""
                      }`}
                    />
                  )}
                  {!step.expandable && (
                    <ChevronRight className="w-6 h-6 text-gray-400" />
                  )}
                </button>

                {/* Expanded Content for Unloading */}
                {step.id === 6 && expandedStep === 6 && (
                  <div className="px-5 pb-5 space-y-5 border-t border-gray-200 pt-5 bg-gray-50">
                    <div className="flex flex-col">
                      <label className="text-sm font-medium text-gray-700 mb-2">
                        LBCL Fork Lift Operator
                      </label>
                      <Select defaultValue="tarun">
                        <SelectTrigger className="h-[42px]">
                          <SelectValue placeholder="Select operator" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="tarun">Tarun Prasad</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5 items-start">
                      <div className="flex flex-col">
                        <label className="text-sm font-medium text-gray-700 mb-2">
                          Unload Start Time
                        </label>
                        <div className="flex gap-3">
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="19"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              HH
                            </span>
                          </div>
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="02"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              Min
                            </span>
                          </div>
                        </div>
                      </div>
                      <div className="flex flex-col">
                        <label className="text-sm font-medium text-gray-700 mb-2">
                          Unload End Time
                        </label>
                        <div className="flex gap-3">
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="16"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              HH
                            </span>
                          </div>
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full h-[42px] px-4 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                              defaultValue="58"
                            />
                            <span className="absolute right-4 top-1/2 -translate-y-1/2 text-xs font-medium text-gray-500 pointer-events-none">
                              Min
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Signature Dialog */}
      <SignatureDialog
        open={showSignatureDialog}
        onOpenChange={setShowSignatureDialog}
        selectedDriverName="R.M.K.P. Rathnayake (U KUMAR)"
        organizationName="R.T DISTRIBUTORS"
        onSave={handleSignatureSave}
      />

      {/* Success Message */}
      {showSuccessMessage && (
        <div className="fixed top-4 right-4 z-50 bg-green-500 text-white px-6 py-4 rounded-lg shadow-lg flex items-center gap-3 animate-in fade-in slide-in-from-top-5">
          <Check className="w-5 h-5" />
          <div>
            <p className="font-semibold">Success!</p>
            <p className="text-sm">Empties receiving submitted successfully</p>
          </div>
        </div>
      )}

      {showRateAgent && (
        <RateAgentModal onClose={() => setShowRateAgent(false)} />
      )}
    </>
  );
}
