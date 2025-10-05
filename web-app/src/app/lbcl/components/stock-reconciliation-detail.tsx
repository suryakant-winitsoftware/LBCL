"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Bell, Search, ScanBarcode } from "lucide-react";
import { Button } from "@/components/ui/button";
import Image from "next/image";

interface StockItem {
  id: string;
  name: string;
  code: string;
  image: string;
  openBalance: number;
  stockReceipt: number;
  deliveries: number;
  credits: number;
  adjustments: number;
  closingBalance: number;
  totalStockCount: number;
  variance: number;
}

const mockStockData: StockItem[] = [
  {
    id: "1",
    name: "Lion Lager Beer can 625ml",
    code: "5213",
    image: "/beer-can.png",
    openBalance: 200,
    stockReceipt: 100,
    deliveries: 100,
    credits: 20,
    adjustments: -20,
    closingBalance: 200,
    totalStockCount: 190,
    variance: 10
  },
  {
    id: "2",
    name: "Lion Lager Beer can 330ml",
    code: "5214",
    image: "/beer-can.png",
    openBalance: 200,
    stockReceipt: 100,
    deliveries: 100,
    credits: 20,
    adjustments: 0,
    closingBalance: 220,
    totalStockCount: 220,
    variance: 0
  },
  {
    id: "3",
    name: "Lion Lager Beer bottle 625ml",
    code: "5216",
    image: "/amber-beer-bottle.png",
    openBalance: 200,
    stockReceipt: 100,
    deliveries: 100,
    credits: 20,
    adjustments: -20,
    closingBalance: 200,
    totalStockCount: 190,
    variance: 10
  },
  {
    id: "4",
    name: "Lion Lager Beer bottle 330ml",
    code: "5210",
    image: "/amber-beer-bottle.png",
    openBalance: 200,
    stockReceipt: 100,
    deliveries: 100,
    credits: 20,
    adjustments: 0,
    closingBalance: 220,
    totalStockCount: 220,
    variance: 0
  },
  {
    id: "5",
    name: "Lion Lager Beer bottle 625ml",
    code: "5216",
    image: "/amber-beer-bottle.png",
    openBalance: 200,
    stockReceipt: 100,
    deliveries: 100,
    credits: 20,
    adjustments: 0,
    closingBalance: 220,
    totalStockCount: 220,
    variance: 0
  }
];

export function StockReconciliationDetail() {
  const router = useRouter();
  const [searchQuery, setSearchQuery] = useState("");
  const [stockCounts, setStockCounts] = useState<Record<string, number>>(
    mockStockData.reduce(
      (acc, item) => ({ ...acc, [item.id]: item.totalStockCount }),
      {}
    )
  );

  const handleCancel = () => {
    router.back();
  };

  const handleSubmit = () => {
    // Handle submit logic
    console.log("[v0] Submitting stock reconciliation", stockCounts);
  };

  const handleStockCountChange = (itemId: string, value: string) => {
    const numValue = value === "" ? 0 : Number.parseInt(value, 10);
    if (!isNaN(numValue) && numValue >= 0) {
      setStockCounts((prev) => ({ ...prev, [itemId]: numValue }));
    }
  };

  const calculateVariance = (closingBalance: number, stockCount: number) => {
    return closingBalance - stockCount;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Info Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-[1600px] mx-auto flex flex-wrap items-center justify-between gap-4">
          <div className="flex flex-wrap items-center gap-6 flex-1">
            <div>
              <span className="text-sm text-gray-600">Audit No</span>
              <p className="text-base font-bold text-gray-900">85444127121</p>
            </div>
            <div>
              <span className="text-sm text-gray-600">Warehouse</span>
              <p className="text-base font-bold text-gray-900">
                [5844] LBCL WAREHOUSE
              </p>
            </div>
            <div>
              <span className="text-sm text-gray-600">Date</span>
              <p className="text-base font-bold text-gray-900">24 NOV 2024</p>
            </div>
          </div>
          <div className="flex gap-3">
            <Button
              variant="outline"
              onClick={handleCancel}
              className="px-8 bg-transparent"
            >
              Cancel
            </Button>
            <Button
              onClick={handleSubmit}
              className="bg-[#A08B5C] hover:bg-[#8A7549] text-white px-8"
            >
              Submit
            </Button>
          </div>
        </div>
      </div>

      {/* Search Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-[1600px] mx-auto flex flex-wrap items-center gap-4">
          <div className="flex items-center gap-2 flex-1 min-w-[300px]">
            <div className="relative flex-1">
              <input
                type="text"
                placeholder="Search by Item Code/Description"
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-full px-4 py-2 pr-10 border border-gray-300 rounded-lg text-sm"
              />
              <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
            </div>
          </div>
          <div className="flex gap-3">
            <Button variant="outline" className="gap-2 bg-transparent">
              <Search className="w-4 h-4" />
              Search
            </Button>
            <Button variant="outline" className="gap-2 bg-transparent">
              <ScanBarcode className="w-4 h-4" />
              Scan
            </Button>
          </div>
        </div>
      </div>

      {/* Table */}
      <main className="max-w-[1600px] mx-auto px-4 py-6">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="bg-[#FFF8E7] border-b border-gray-200">
                <th className="px-4 py-3 text-left text-sm font-semibold text-gray-900 min-w-[250px]">
                  Item Code/Description
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[100px]">
                  Open Balance
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[100px]">
                  Stock Receipt
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[100px]">
                  Deliveries
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[120px]">
                  Credits
                  <br />
                  <span className="text-xs font-normal">
                    (Warehouse & Customer)
                  </span>
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[120px]">
                  Adjustments
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[120px]">
                  Closing Balance
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[120px]">
                  Total Stock Count
                </th>
                <th className="px-4 py-3 text-center text-sm font-semibold text-gray-900 min-w-[100px]">
                  Variance
                </th>
              </tr>
            </thead>
            <tbody>
              {mockStockData.map((item) => {
                const currentStockCount = stockCounts[item.id] || 0;
                const variance = calculateVariance(
                  item.closingBalance,
                  currentStockCount
                );

                return (
                  <tr
                    key={item.id}
                    className="border-b border-gray-200 hover:bg-gray-50"
                  >
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-3">
                        <div className="relative w-12 h-12 flex-shrink-0">
                          <Image
                            src={item.image || "/placeholder.svg"}
                            alt={item.name}
                            fill
                            className="object-contain"
                            sizes="48px"
                          />
                        </div>
                        <div>
                          <p className="font-semibold text-gray-900 text-sm">
                            {item.name}
                          </p>
                          <p className="text-xs text-gray-500">{item.code}</p>
                        </div>
                      </div>
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.openBalance}
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.stockReceipt}
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.deliveries}
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.credits}
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.adjustments}
                    </td>
                    <td className="px-4 py-4 text-center text-sm text-gray-900">
                      {item.closingBalance}
                    </td>
                    <td className="px-4 py-4">
                      <div className="flex justify-center">
                        <input
                          type="number"
                          min="0"
                          value={currentStockCount}
                          onChange={(e) =>
                            handleStockCountChange(item.id, e.target.value)
                          }
                          className="w-20 px-2 py-1 text-center text-sm border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-[#A08B5C] focus:border-transparent"
                        />
                      </div>
                    </td>
                    <td className="px-4 py-4 text-center text-sm font-semibold text-gray-900">
                      {variance}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </main>
    </div>
  );
}
