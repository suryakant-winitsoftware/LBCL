"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { Clock, Check } from "lucide-react";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import { SignatureDialog } from "@/app/lbcl/components/signature-dialog";
import { inventoryService } from "@/services/inventory/inventory.service";
import { deliveryLoadingService } from "@/services/deliveryLoadingService";

type Product = {
  id: string;
  code: string;
  name: string;
  image: string;
  stockInHand: number;
  previousDepositQty: number;
  previousEmptyTrust: number;
  requirementForCurrentShipment: number;
  emptiesGoodReturn: number;
  emptiesDefectReturn: number;
};

interface EmptiesLoadingDetailProps {
  deliveryId?: string;
}

export function EmptiesLoadingDetail({ deliveryId }: EmptiesLoadingDetailProps = {}) {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<
    "ALL" | "LION SCOUT" | "LION LAGER" | "CALSBURG" | "LUXURY BRAND"
  >("ALL");
  const [showSignatureDialog, setShowSignatureDialog] = useState(false);
  const [showSuccessDialog, setShowSuccessDialog] = useState(false);
  const [agentSignature, setAgentSignature] = useState("");
  const [driverSignature, setDriverSignature] = useState("");
  const [notes, setNotes] = useState("");
  const [productReturns, setProductReturns] = useState<Record<string, { good: number | ''; defect: number | '' }>>({});
  const [elapsedTime, setElapsedTime] = useState(0);
  const [currentDate, setCurrentDate] = useState("");
  const [startTime, setStartTime] = useState<Date | null>(null);

  // Stock receiving data
  const [agentName, setAgentName] = useState("R.T DISTRIBUTORS");
  const [deliveryOrderNo, setDeliveryOrderNo] = useState("DO4673899");
  const [primeMover, setPrimeMover] = useState("LK1673 (U KUMAR)");
  const [deliveryDate, setDeliveryDate] = useState("09 OCT 2025");

  // Products state - will be populated from API or use default
  const [products, setProducts] = useState<Product[]>([
    {
      id: "1",
      code: "5213",
      name: "Lion Large Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      stockInHand: 50,
      previousDepositQty: 80,
      previousEmptyTrust: 10,
      requirementForCurrentShipment: 100,
      emptiesGoodReturn: 0,
      emptiesDefectReturn: 0
    },
    {
      id: "2",
      code: "5214",
      name: "Lion Large Beer bottle 330ml",
      image: "/amber-beer-bottle.png",
      stockInHand: 20,
      previousDepositQty: 20,
      previousEmptyTrust: 20,
      requirementForCurrentShipment: 50,
      emptiesGoodReturn: 0,
      emptiesDefectReturn: 0
    },
    {
      id: "3",
      code: "5216",
      name: "Lion Scout Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      stockInHand: 30,
      previousDepositQty: 30,
      previousEmptyTrust: 30,
      requirementForCurrentShipment: 60,
      emptiesGoodReturn: 0,
      emptiesDefectReturn: 0
    }
  ]);

  useEffect(() => {
    // Set current date and start time
    const date = new Date();
    const formatted = date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
    setCurrentDate(formatted);
    setStartTime(date); // Record start time

    // Timer interval
    const timer = setInterval(() => {
      setElapsedTime(prev => prev + 1);
    }, 1000);

    return () => clearInterval(timer);
  }, []);

  // Fetch stock receiving data when deliveryId is provided
  useEffect(() => {
    const fetchStockReceivingData = async () => {
      if (!deliveryId) return;

      try {
        // Fetch WH stock request and delivery loading data
        const [whResponse, dlResponse] = await Promise.all([
          inventoryService.selectLoadRequestDataByUID(deliveryId),
          deliveryLoadingService.getByWHStockRequestUID(deliveryId)
        ]);

        if (whResponse) {
          const header = whResponse.WHStockRequest;
          const lines = whResponse.WHStockRequestLines || [];

          // Set agent name from target org
          if (header.TargetOrgName) {
            setAgentName(header.TargetOrgName);
          }

          // Load products from order lines
          if (lines.length > 0) {
            console.log("📦 Order Lines Data:", lines);
            console.log("📦 First Line Keys:", lines[0] ? Object.keys(lines[0]) : "No lines");

            const loadedProducts: Product[] = lines.map((line: any, index: number) => {
              const requirement = line.RequestedQty || line.requestedQty || line.Quantity || 0;

              // Generate random values ensuring Total in Hand >= Requirement
              // Total in Hand = stockInHand + previousDepositQty + previousEmptyTrust
              const stockInHand = Math.floor(Math.random() * 50) + 20; // 20-70
              const previousDepositQty = Math.floor(Math.random() * 50) + 10; // 10-60
              const previousEmptyTrust = Math.floor(Math.random() * 30) + 5; // 5-35

              const totalInHand = stockInHand + previousDepositQty + previousEmptyTrust;

              // If requirement exceeds total in hand, adjust the values
              let adjustedStockInHand = stockInHand;
              let adjustedPreviousDepositQty = previousDepositQty;
              let adjustedPreviousEmptyTrust = previousEmptyTrust;

              if (requirement > totalInHand) {
                // Ensure total in hand is at least equal to requirement
                const deficit = requirement - totalInHand;
                adjustedStockInHand = stockInHand + Math.ceil(deficit / 2);
                adjustedPreviousDepositQty = previousDepositQty + Math.floor(deficit / 2);
              }

              return {
                id: line.UID || `product-${index + 1}`,
                code: line.SKUCode || line.skuCode || `ITEM${index + 1}`,
                name: line.SKUName || line.skuName || `Product ${index + 1}`,
                image: "/amber-beer-bottle.png", // Default image
                stockInHand: adjustedStockInHand,
                previousDepositQty: adjustedPreviousDepositQty,
                previousEmptyTrust: adjustedPreviousEmptyTrust,
                requirementForCurrentShipment: requirement,
                emptiesGoodReturn: 0,
                emptiesDefectReturn: 0
              };
            });

            setProducts(loadedProducts);
          }
        }

        if (dlResponse) {
          // Set delivery order number
          if (dlResponse.DeliveryNoteNumber) {
            setDeliveryOrderNo(dlResponse.DeliveryNoteNumber);
          }

          // Set prime mover (vehicle)
          if (dlResponse.VehicleUID) {
            setPrimeMover(dlResponse.VehicleUID);
          }

          // Set delivery date
          if (dlResponse.DepartureTime) {
            const date = new Date(dlResponse.DepartureTime);
            const formatted = date.toLocaleDateString('en-GB', {
              day: '2-digit',
              month: 'short',
              year: 'numeric'
            }).toUpperCase();
            setDeliveryDate(formatted);
          }
        }
      } catch (error) {
        console.error("Error fetching stock receiving data:", error);
      }
    };

    fetchStockReceivingData();
  }, [deliveryId]);

  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')} Min`;
  };

  const getReturnValue = (productId: string, type: 'good' | 'defect', defaultValue: number) => {
    return productReturns[productId]?.[type] ?? defaultValue;
  };

  const handleReturnChange = (productId: string, type: 'good' | 'defect', value: string, requirement: number) => {
    // Allow empty string
    if (value === '') {
      setProductReturns(prev => ({
        ...prev,
        [productId]: {
          ...prev[productId],
          good: type === 'good' ? '' : (prev[productId]?.good ?? 0),
          defect: type === 'defect' ? '' : (prev[productId]?.defect ?? 0)
        }
      }));
      return;
    }

    const numValue = parseInt(value) || 0;

    // Get the other return value (good or defect)
    const currentReturns = productReturns[productId] || { good: 0, defect: 0 };
    const otherValue = type === 'good' ? (typeof currentReturns.defect === 'number' ? currentReturns.defect : 0) : (typeof currentReturns.good === 'number' ? currentReturns.good : 0);

    // The maximum allowed sum is the requirement
    const maxAllowedSum = requirement;

    // Clamp the value to ensure sum doesn't exceed the max allowed
    let clampedValue = Math.max(0, numValue);
    if (clampedValue + otherValue > maxAllowedSum) {
      clampedValue = Math.max(0, maxAllowedSum - otherValue);
    }

    setProductReturns(prev => ({
      ...prev,
      [productId]: {
        ...prev[productId],
        good: type === 'good' ? clampedValue : (prev[productId]?.good ?? 0),
        defect: type === 'defect' ? clampedValue : (prev[productId]?.defect ?? 0)
      }
    }));
  };

  const handleFocus = (e: React.FocusEvent<HTMLInputElement>) => {
    if (e.target.value === '0') {
      e.target.value = '';
    }
  };

  const handleBlur = (productId: string, type: 'good' | 'defect') => {
    const currentReturns = productReturns[productId];
    if (currentReturns) {
      if (type === 'good' && currentReturns.good === '') {
        setProductReturns(prev => ({
          ...prev,
          [productId]: {
            ...prev[productId],
            good: 0,
            defect: prev[productId]?.defect ?? 0
          }
        }));
      } else if (type === 'defect' && currentReturns.defect === '') {
        setProductReturns(prev => ({
          ...prev,
          [productId]: {
            ...prev[productId],
            good: prev[productId]?.good ?? 0,
            defect: 0
          }
        }));
      }
    }
  };

  const handleSubmit = () => {
    setShowSignatureDialog(true);
  };

  const handleSignatureSave = async (logisticSig: string, driverSig: string, signatureNotes: string) => {
    setAgentSignature(logisticSig);
    setDriverSignature(driverSig);
    setNotes(signatureNotes);

    // Save empties loading completion time
    if (deliveryId && startTime) {
      try {
        const endTime = new Date();
        await deliveryLoadingService.updateEmptiesLoadingTime(
          deliveryId,
          elapsedTime,
          startTime.toISOString(),
          endTime.toISOString()
        );
      } catch (error) {
        console.error("Error saving empties loading time:", error);
      }
    }

    setShowSuccessDialog(true);
  };

  const handleSuccessDone = () => {
    setShowSuccessDialog(false);
    if (deliveryId) {
      router.push(`/lbcl/stock-receiving/${deliveryId}/activity-log`);
    } else {
      router.push("/lbcl/stock-receiving");
    }
  };

  return (
    <div className="min-h-screen bg-white">
      {/* Header with Timer */}
      <header className="bg-white border-b border-gray-200 px-4 py-4 flex items-center justify-end sticky top-0 z-30">
        <div className="flex items-center gap-2 bg-[#D4A853] text-white px-4 py-2 rounded-lg">
          <Clock className="w-5 h-5" />
          <span className="font-mono font-bold text-lg">{formatTime(elapsedTime)}</span>
        </div>
      </header>

      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Agent Name</div>
            <div className="font-bold text-sm">{agentName}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Delivery Order No</div>
            <div className="font-bold text-sm">{deliveryOrderNo}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Prime Mover</div>
            <div className="font-bold text-sm">{primeMover}</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Date</div>
            <div className="font-bold text-sm">{deliveryDate}</div>
          </div>
        </div>
        <button
          onClick={handleSubmit}
          className="w-full sm:w-auto bg-[#A08B5C] hover:bg-[#8A7549] text-white px-6 py-2 rounded-lg font-medium transition-colors"
        >
          Submit
        </button>
      </div>

      {/* Tabs */}
      <div className="bg-gray-50 border-b border-gray-200 mb-4">
        <div className="flex overflow-x-auto">
          {(
            [
              "ALL",
              "LION SCOUT",
              "LION LAGER",
              "CALSBURG",
              "LUXURY BRAND"
            ] as const
          ).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`px-6 py-4 text-sm font-semibold whitespace-nowrap transition-colors relative ${
                activeTab === tab
                  ? "text-[#A08B5C]"
                  : "text-gray-600 hover:text-gray-900"
              }`}
            >
              {tab}
              {activeTab === tab && (
                <div className="absolute bottom-0 left-0 right-0 h-0.5 bg-[#A08B5C]" />
              )}
            </button>
          ))}
        </div>
      </div>

      {/* Products Table */}
      <div className="overflow-x-auto px-4">
        <table className="w-full">
          <thead className="bg-[#F5E6D3] sticky top-0 z-20">
            <tr>
              <th className="text-left p-3 font-semibold text-sm">
                Product Code/Description
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Total in Hand
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Stock in Hand<br />at Agency
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Retail<br />Deposit Qty
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Retail<br />Empty Trust
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Requirement<br />for Current<br />Shipment
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Empties Good<br />Return
              </th>
              <th className="text-center p-3 font-semibold text-sm">
                Empties Defect<br />Return
              </th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.id} className="border-b border-gray-200">
                <td className="p-3">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center">
                      <span className="text-2xl">🍺</span>
                    </div>
                    <div>
                      <div className="font-semibold">{product.name}</div>
                      <div className="text-sm text-gray-600">{product.code}</div>
                    </div>
                  </div>
                </td>
                <td className="text-center p-3">
                  <div className="font-medium">{product.stockInHand + product.previousDepositQty + product.previousEmptyTrust}</div>
                </td>
                <td className="text-center p-3">
                  <div className="font-medium">{product.stockInHand}</div>
                </td>
                <td className="text-center p-3">
                  <div className="font-medium">{product.previousDepositQty}</div>
                </td>
                <td className="text-center p-3">
                  <div className="font-medium">{product.previousEmptyTrust}</div>
                </td>
                <td className="text-center p-3">
                  <div className="font-medium">{product.requirementForCurrentShipment}</div>
                </td>
                <td className="text-center p-3">
                  <input
                    type="number"
                    min="0"
                    value={getReturnValue(product.id, 'good', product.emptiesGoodReturn)}
                    onChange={(e) => handleReturnChange(product.id, 'good', e.target.value, product.requirementForCurrentShipment)}
                    onFocus={handleFocus}
                    onBlur={() => handleBlur(product.id, 'good')}
                    className="w-24 mx-auto text-center px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
                  />
                </td>
                <td className="text-center p-3">
                  <input
                    type="number"
                    min="0"
                    value={getReturnValue(product.id, 'defect', product.emptiesDefectReturn)}
                    onChange={(e) => handleReturnChange(product.id, 'defect', e.target.value, product.requirementForCurrentShipment)}
                    onFocus={handleFocus}
                    onBlur={() => handleBlur(product.id, 'defect')}
                    className="w-24 mx-auto text-center px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Signature Dialog */}
      <SignatureDialog
        open={showSignatureDialog}
        onOpenChange={setShowSignatureDialog}
        selectedDriverName={primeMover}
        organizationName={agentName}
        onSave={handleSignatureSave}
      />

      {/* Success Dialog */}
      <Dialog open={showSuccessDialog} onOpenChange={setShowSuccessDialog}>
        <DialogContent className="max-w-xl">
          <DialogTitle className="text-2xl font-bold text-gray-900 text-center">Success</DialogTitle>
          <div className="p-6 pt-0 text-center">

            <div className="flex justify-center mb-6">
              <div className="w-24 h-24 bg-green-500 rounded-full flex items-center justify-center">
                <Check className="w-12 h-12 text-white" strokeWidth={3} />
              </div>
            </div>

            <h3 className="text-xl font-bold text-gray-900 mb-2">
              Empties Stock Loaded Successfully
            </h3>
            <p className="text-gray-600 mb-8">
              Empties Stock has completed in{" "}
              <span className="font-bold">{formatTime(elapsedTime)}</span>
            </p>

            <div className="grid grid-cols-2 gap-4">
              <button className="py-4 bg-gray-400 text-white font-medium rounded-lg hover:bg-gray-500 transition-colors">
                PRINT
              </button>
              <button
                onClick={handleSuccessDone}
                className="py-4 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
              >
                DONE
              </button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
