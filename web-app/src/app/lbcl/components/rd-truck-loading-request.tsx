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
    <div className="min-h-screen bg-gray-50">
      {/* Info Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto grid grid-cols-5 gap-4 text-sm">
          <div>
            <div className="text-xs text-gray-500 mb-1">Load Request No</div>
            <div className="font-bold text-gray-900">RHTT15E0000001</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">Distributor</div>
            <div className="font-bold text-gray-900">
              [5844] R.T DISTRIBUTOR
            </div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">Date</div>
            <div className="font-bold text-gray-900">25 MAY 2025</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">
              Vehicle Capacity Limit
            </div>
            <div className="font-bold text-gray-900">45 m3</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">
              Total Number of Litres
            </div>
            <div className="font-bold text-gray-900">40 m3</div>
          </div>
        </div>
        <div className="max-w-7xl mx-auto grid grid-cols-2 gap-4 text-sm mt-4">
          <div>
            <div className="text-xs text-gray-500 mb-1">Driver Details</div>
            <div className="font-bold text-gray-900">[3678] VANSANTH KUMAR</div>
          </div>
          <div>
            <div className="text-xs text-gray-500 mb-1">Helper Details</div>
            <div className="font-bold text-gray-900">[5378] PRASHANTH</div>
          </div>
        </div>
      </div>

      {/* Search Section */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="max-w-7xl mx-auto flex items-center gap-4">
          <div className="flex-1 relative">
            <input
              type="text"
              placeholder="Search by Brand"
              value={searchBrand}
              onChange={(e) => setSearchBrand(e.target.value)}
              className="w-full pl-4 pr-10 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <div className="flex-1 relative">
            <input
              type="text"
              placeholder="Search by Item Code/Description"
              value={searchItem}
              onChange={(e) => setSearchItem(e.target.value)}
              className="w-full pl-4 pr-10 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
            />
            <Search className="absolute right-3 top-1/2 -translate-y-1/2 w-5 h-5 text-gray-400" />
          </div>
          <button className="px-6 py-3 border-2 border-[#A08B5C] text-[#A08B5C] font-medium rounded-lg hover:bg-[#FFF8E7] transition-colors flex items-center gap-2">
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
            className="px-6 py-3 border-2 border-[#A08B5C] text-[#A08B5C] font-medium rounded-lg hover:bg-[#FFF8E7] transition-colors"
          >
            View Activity Log
          </button>
          <button
            onClick={handleSubmit}
            className="px-8 py-3 bg-[#A08B5C] text-white font-medium rounded-lg hover:bg-[#8F7A4D] transition-colors"
          >
            Submit
          </button>
        </div>
      </div>

      {/* Products Table */}
      <div className="p-4 max-w-7xl mx-auto">
        <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-x-auto">
          <table className="w-full">
            <thead className="bg-[#FFF8E7]">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-700 min-w-[250px]">
                  Item Code/Description
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[100px]">
                  Planned Qty
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[120px]">
                  Pre Order Qty
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[150px]">
                  Buffer Qty
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[120px]">
                  Loadable Qty
                </th>
                <th className="px-4 py-3 text-center text-xs font-medium text-gray-700 min-w-[120px]">
                  Balance Qty
                </th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {products.map((product) => (
                <tr key={product.id} className="hover:bg-gray-50">
                  <td className="px-4 py-4">
                    <div className="flex items-center gap-3">
                      <img
                        src={product.image || "/placeholder.svg"}
                        alt={product.name}
                        className="w-10 h-10 object-contain"
                      />
                      <div>
                        <div className="font-medium text-gray-900">
                          {product.name}
                        </div>
                        <div className="text-sm text-gray-500 flex items-center gap-2">
                          {product.code}
                          <span className="inline-flex items-center justify-center w-5 h-5 bg-[#D4A853] rounded-full">
                            <svg
                              className="w-3 h-3 text-white"
                              viewBox="0 0 20 20"
                              fill="currentColor"
                            >
                              <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                            </svg>
                          </span>
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">
                      {product.plannedQty}
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">
                      {product.preOrderQty}
                    </div>
                  </td>
                  <td className="px-4 py-4">
                    <div className="flex items-center justify-center gap-2">
                      <button className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-50">
                        {product.bufferQty > 0 ? (
                          <Minus className="w-4 h-4" />
                        ) : (
                          <Plus className="w-4 h-4" />
                        )}
                      </button>
                      <button className="w-10 h-10 border border-gray-300 rounded-lg flex items-center justify-center hover:bg-gray-50">
                        <ChevronDown className="w-4 h-4" />
                      </button>
                      <input
                        type="number"
                        value={product.bufferQty}
                        className="w-20 px-3 py-2 border border-gray-300 rounded-lg text-center focus:outline-none focus:ring-2 focus:ring-[#A08B5C]"
                        readOnly
                      />
                    </div>
                  </td>
                  <td className="px-4 py-4">
                    <div className="flex justify-center">
                      <input
                        type="number"
                        value={product.loadableQty}
                        className="w-24 px-3 py-2 border border-gray-300 rounded-lg text-center bg-gray-50"
                        readOnly
                      />
                    </div>
                  </td>
                  <td className="px-4 py-4 text-center">
                    <div className="font-medium text-gray-900">
                      {product.balanceQty}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
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
