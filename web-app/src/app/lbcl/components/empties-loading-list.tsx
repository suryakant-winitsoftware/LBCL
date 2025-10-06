"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ChevronRight } from "lucide-react";

type EmptiesLoadingRecord = {
  id: string;
  emptiesCode: string;
  agent: string;
  date: string;
  status: "Pending" | "Approved";
};

export function EmptiesLoadingList() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<"ALL" | "PENDING" | "APPROVED">("ALL");

  // Mock data
  const emptiesRecords: EmptiesLoadingRecord[] = [
    {
      id: "1",
      emptiesCode: "64785685678",
      agent: "R.T DISTRIBUTORS",
      date: "20-MAY-2025",
      status: "Pending"
    }
  ];

  const filteredRecords = emptiesRecords.filter((record) => {
    if (activeTab === "ALL") return true;
    return record.status.toUpperCase() === activeTab;
  });

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Tabs */}
      <div className="bg-white border-b border-gray-200">
        <div className="flex">
          {(["ALL", "PENDING", "APPROVED"] as const).map((tab) => (
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

      {/* Empties Loading List */}
      <div className="p-4 space-y-3">
        {filteredRecords.map((record) => (
          <div
            key={record.id}
            onClick={() => router.push(`/lbcl/empties-loading/${record.id}`)}
            className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow cursor-pointer"
          >
            <div className="p-4 flex items-center justify-between">
              <div className="flex-1 grid grid-cols-3 gap-4">
                <div>
                  <div className="text-xs text-gray-500 mb-1">Empties Code</div>
                  <div className="font-bold text-gray-900 text-lg">
                    {record.emptiesCode}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-gray-500 mb-1">Agent</div>
                  <div className="font-bold text-gray-900 text-lg">
                    {record.agent}
                  </div>
                </div>
                <div>
                  <div className="text-xs text-gray-500 mb-1">Date</div>
                  <div className="font-bold text-gray-900 text-lg">{record.date}</div>
                </div>
              </div>
              <ChevronRight className="w-6 h-6 text-gray-400 ml-4" />
            </div>
          </div>
        ))}

        {filteredRecords.length === 0 && (
          <div className="text-center py-12">
            <p className="text-gray-500">No empties loading records found</p>
          </div>
        )}
      </div>
    </div>
  );
}
