"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Calendar, RefreshCw, ChevronRight } from "lucide-react";

type LoadRequest = {
  id: string;
  loadRequestNo: string;
  distributor: string;
  date: string;
  collectedQty: number;
  approvalStatus: "Pending" | "Approved" | "Shipped";
};

export function TruckLoadingList() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<
    "ALL" | "PENDING" | "APPROVED" | "SHIPPED"
  >("ALL");
  const [fromDate, setFromDate] = useState("04 May 2025");
  const [toDate, setToDate] = useState("07 May 2025");

  // Mock data
  const loadRequests: LoadRequest[] = [
    {
      id: "1",
      loadRequestNo: "RHTT15E0000001",
      distributor: "R.T DISTRIBUTORS",
      date: "23 MAY 2025",
      collectedQty: 45,
      approvalStatus: "Pending"
    }
  ];

  const filteredRequests = loadRequests.filter((request) => {
    if (activeTab === "ALL") return true;
    return request.approvalStatus.toUpperCase() === activeTab;
  });

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Pending":
        return "text-orange-600";
      case "Approved":
        return "text-green-600";
      case "Shipped":
        return "text-blue-600";
      default:
        return "text-gray-600";
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Date Filters */}
      <div className="bg-white border-b border-gray-200 px-4 py-4">
        <div className="flex items-center gap-4 justify-between max-w-4xl mx-auto">
          <div className="flex items-center gap-2 flex-1">
            <span className="text-sm font-medium text-gray-700 whitespace-nowrap">
              From Date
            </span>
            <div className="flex items-center gap-2 bg-gray-50 px-3 py-2 rounded-lg border border-gray-200 flex-1">
              <span className="text-sm text-gray-600">{fromDate}</span>
              <Calendar className="w-4 h-4 text-[#A08B5C]" />
            </div>
          </div>

          <div className="flex items-center gap-2 flex-1">
            <span className="text-sm font-medium text-gray-700 whitespace-nowrap">
              To Date
            </span>
            <div className="flex items-center gap-2 bg-gray-50 px-3 py-2 rounded-lg border border-gray-200 flex-1">
              <span className="text-sm text-gray-600">{toDate}</span>
              <Calendar className="w-4 h-4 text-[#A08B5C]" />
            </div>
          </div>

          <button className="p-2 hover:bg-gray-100 rounded-lg">
            <RefreshCw className="w-5 h-5 text-[#A08B5C]" />
          </button>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex max-w-4xl mx-auto">
          {(["ALL", "PENDING", "APPROVED", "SHIPPED"] as const).map((tab) => (
            <button
              key={tab}
              onClick={() => setActiveTab(tab)}
              className={`flex-1 py-4 text-sm font-medium transition-colors relative ${
                activeTab === tab
                  ? "text-[#A08B5C]"
                  : "text-gray-500 hover:text-gray-700"
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

      {/* Load Requests List */}
      <div className="p-4 max-w-4xl mx-auto space-y-4">
        {filteredRequests.map((request) => (
          <div
            key={request.id}
            onClick={() => router.push(`/lbcl/truck-loading/${request.id}`)}
            className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
          >
            {/* Request Header */}
            <div className="p-4 flex items-center justify-between">
              <div className="flex-1 grid grid-cols-3 gap-4">
                <div>
                  <div className="text-xs text-gray-500 mb-1">
                    Load Request No
                  </div>
                  <div className="font-bold text-gray-900">
                    {request.loadRequestNo}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-gray-500 mb-1">Distributor</div>
                  <div className="font-bold text-gray-900">
                    {request.distributor}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-gray-500 mb-1">Date</div>
                  <div className="font-bold text-gray-900">{request.date}</div>
                </div>
              </div>
              <ChevronRight className="w-5 h-5 text-gray-400 ml-4" />
            </div>

            {/* Status Bar */}
            <div className="bg-[#FFF8E7] px-4 py-3 flex items-center justify-between border-t border-gray-100">
              <div>
                <div className="text-xs text-gray-600 mb-1">Collected Qty</div>
                <div className="text-lg font-bold text-gray-900">
                  {request.collectedQty}
                </div>
              </div>
              <div className="h-12 w-px bg-gray-300" />
              <div className="text-right">
                <div className="text-xs text-gray-600 mb-1">
                  Approval Status
                </div>
                <div
                  className={`text-lg font-bold ${getStatusColor(
                    request.approvalStatus
                  )}`}
                >
                  {request.approvalStatus}
                </div>
              </div>
            </div>
          </div>
        ))}

        {filteredRequests.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500">No load requests found</p>
          </div>
        )}
      </div>

      {/* Floating Action Button for creating new requests */}
      <button
        onClick={() => router.push("/lbcl/truck-loading/new")}
        className="fixed bottom-6 right-6 w-14 h-14 bg-[#A08B5C] text-white rounded-full shadow-lg hover:bg-[#8F7A4D] transition-colors flex items-center justify-center"
      >
        <svg
          className="w-6 h-6"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M12 4v16m8-8H4"
          />
        </svg>
      </button>
    </div>
  );
}
