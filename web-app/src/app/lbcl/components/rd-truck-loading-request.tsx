"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { RefreshCw, Search, Plus, Minus, ChevronDown } from "lucide-react";

type Product = {
  id: string;
  code: string;
  name: string;
  image: string;
  plannedQty: number;
  preOrderQty: number;
  bufferQty: number;
  loadableQty: number;
  balanceQty: number;
};

export function RDTruckLoadingRequest() {
  const router = useRouter();
  const [searchBrand, setSearchBrand] = useState("");
  const [searchItem, setSearchItem] = useState("");
  const [showAddItemDialog, setShowAddItemDialog] = useState(false);

  const products: Product[] = [
    {
      id: "1",
      code: "5213",
      name: "Lion Large Beer can 625ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 75,
      preOrderQty: 25,
      bufferQty: 10,
      loadableQty: 90,
      balanceQty: 10
    },
    {
      id: "2",
      code: "5214",
      name: "Lion Large Beer can 330ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 34,
      preOrderQty: 10,
      bufferQty: 6,
      loadableQty: 6,
      balanceQty: 50
    },
    {
      id: "3",
      code: "5216",
      name: "Lion Large Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 43,
      preOrderQty: 20,
      bufferQty: 4,
      loadableQty: 4,
      balanceQty: 67
    },
    {
      id: "4",
      code: "5210",
      name: "Lion Large Beer bottle 330ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 55,
      preOrderQty: 10,
      bufferQty: 5,
      loadableQty: 5,
      balanceQty: 60
    },
    {
      id: "5",
      code: "5216",
      name: "Lion Large Beer bottle 625ml",
      image: "/amber-beer-bottle.png",
      plannedQty: 25,
      preOrderQty: 0,
      bufferQty: 6,
      loadableQty: 6,
      balanceQty: 31
    }
  ];

  const handleSubmit = () => {
    router.push("/lbcl/truck-loading/activity-log");
  };

  const handleViewActivityLog = () => {
    router.push("/lbcl/truck-loading/activity-log");
  };

  return (
    <div className="min-h-screen bg-white">
      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-5 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Load Request No</div>
            <div className="font-bold text-sm">RHTT15E0000001</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Distributor</div>
            <div className="font-bold text-sm">[5844] R.T DISTRIBUTOR</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Date</div>
            <div className="font-bold text-sm">25 MAY 2025</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Vehicle Capacity Limit</div>
            <div className="font-bold text-sm">45 m3</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Total Number of Litres</div>
            <div className="font-bold text-sm">40 m3</div>
          </div>
        </div>
        <div className="grid grid-cols-2 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Driver Details</div>
            <div className="font-bold text-sm">[3678] VANSANTH KUMAR</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Helper Details</div>
            <div className="font-bold text-sm">[5378] PRASHANTH</div>
          </div>
        </div>
        <button
          onClick={handleSubmit}
          className="w-full sm:w-auto bg-[#A08B5C] hover:bg-[#8A7549] text-white px-6 py-2 rounded-lg font-medium transition-colors"
        >
          Submit
        </button>
      </div>

      {/* Search Section */}
      <div className="bg-gray-50 border-b border-gray-200 p-4">
        <div className="flex flex-col sm:flex-row items-center gap-4">
          <div className="flex-1 relative w-full">
            <input
              type="text"
              placeholder="Search by Brand"
              value={searchBrand}
              onChange={(e) => setSearchBrand(e.target.value)}
              className="w-full pl-4 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <div className="flex-1 relative w-full">
            <input
              type="text"
              placeholder="Search by Item Code/Description"
              value={searchItem}
              onChange={(e) => setSearchItem(e.target.value)}
              className="w-full pl-4 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <button className="px-4 py-2 border-2 border-[#A08B5C] text-[#A08B5C] font-medium rounded-lg hover:bg-[#FFF8E7] transition-colors flex items-center gap-2">
            <svg
              className="w-5 h-5"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
            >
              <rect x="3" y="3" width="18" height="18" rx="2" strokeWidth="2" />
              <path d="M9 3v18M3 9h18M3 15h18" strokeWidth="2" />
            </svg>
            Scan
          </button>
          <button
            onClick={handleViewActivityLog}
            className="px-4 py-2 border-2 border-[#A08B5C] text-[#A08B5C] font-medium rounded-lg hover:bg-[#FFF8E7] transition-colors whitespace-nowrap"
          >
            View Activity Log
          </button>
        </div>
      </div>

      {/* Products Table */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="bg-[#F5E6D3] sticky top-0 z-20">
              <tr>
                <th className="text-left p-3 font-semibold text-sm">
                  Item Code/Description
                </th>
                <th className="text-center p-3 font-semibold text-sm">
                  Planned Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm">
                  Pre Order Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm">
                  Buffer Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm">
                  Loadable Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm">
                  Balance Qty
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
                    <div className="font-medium">{product.plannedQty}</div>
                  </td>
                  <td className="text-center p-3">
                    <div className="font-medium">{product.preOrderQty}</div>
                  </td>
                  <td className="text-center p-3">
                    <div className="flex items-center justify-center gap-2">
                      <button className="w-8 h-8 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-50">
                        {product.bufferQty > 0 ? (
                          <Minus className="w-4 h-4" />
                        ) : (
                          <Plus className="w-4 h-4" />
                        )}
                      </button>
                      <button className="w-8 h-8 border border-gray-300 rounded flex items-center justify-center hover:bg-gray-50">
                        <ChevronDown className="w-4 h-4" />
                      </button>
                      <input
                        type="number"
                        value={product.bufferQty}
                        className="w-20 px-2 py-1 border border-gray-300 rounded text-center focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
                        readOnly
                      />
                    </div>
                  </td>
                  <td className="text-center p-3">
                    <input
                      type="number"
                      value={product.loadableQty}
                      className="w-24 mx-auto text-center px-2 py-1 border border-gray-300 rounded bg-gray-50"
                      readOnly
                    />
                  </td>
                  <td className="text-center p-3">
                    <div className="font-medium">{product.balanceQty}</div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
      </div>

      {/* Floating Action Button */}
      <button
        onClick={() => setShowAddItemDialog(true)}
        className="fixed bottom-6 right-6 w-14 h-14 bg-[#A08B5C] text-white rounded-full shadow-lg hover:bg-[#8F7A4D] transition-colors flex items-center justify-center"
      >
        <Plus className="w-6 h-6" />
      </button>
    </div>
  );
}
