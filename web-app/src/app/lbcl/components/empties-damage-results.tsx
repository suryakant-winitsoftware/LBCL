"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";

type DamageProduct = {
  id: string;
  code: string;
  name: string;
  goodCollected: number;
  damageCollected: number;
  sampleGood: number;
  totalInHand: number;
};

export function EmptiesDamageResults() {
  const router = useRouter();
  const [currentDate, setCurrentDate] = useState("");

  useEffect(() => {
    const date = new Date();
    const formatted = date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
    setCurrentDate(formatted);
  }, []);

  const damageProducts: DamageProduct[] = [
    {
      id: "1",
      code: "5213",
      name: "Short Quarter Keg 7.75 Galon Beers",
      goodCollected: 25,
      damageCollected: 3,
      sampleGood: 5,
      totalInHand: 23
    },
    {
      id: "2",
      code: "5214",
      name: "Slim Quarter Keg 7.75 Galon",
      goodCollected: 10,
      damageCollected: 2,
      sampleGood: 2,
      totalInHand: 10
    },
    {
      id: "3",
      code: "5216",
      name: "Lion Large Beer bottle 625ml",
      goodCollected: 20,
      damageCollected: 2,
      sampleGood: 3,
      totalInHand: 19
    },
    {
      id: "4",
      code: "5210",
      name: "Lion Large Beer bottle 330ml",
      goodCollected: 5,
      damageCollected: 1,
      sampleGood: 1,
      totalInHand: 5
    }
  ];

  const totalGoodCollected = damageProducts.reduce((sum, p) => sum + p.goodCollected, 0);
  const totalDamageCollected = damageProducts.reduce((sum, p) => sum + p.damageCollected, 0);
  const totalSampleGood = damageProducts.reduce((sum, p) => sum + p.sampleGood, 0);
  const totalInHand = damageProducts.reduce((sum, p) => sum + p.totalInHand, 0);

  return (
    <div className="min-h-screen bg-white">
      {/* Info Section */}
      <div className="bg-gray-50 p-4 border-b border-gray-200">
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-4">
          <div>
            <div className="text-xs text-gray-600 mb-1">Agent Name</div>
            <div className="font-bold text-sm">R.T DISTRIBUTORS</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Empties Delivery No</div>
            <div className="font-bold text-sm">EMT85444127121</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Prime Mover</div>
            <div className="font-bold text-sm">LK1673 (U KUMAR)</div>
          </div>
          <div>
            <div className="text-xs text-gray-600 mb-1">Date</div>
            <div className="font-bold text-sm">{currentDate}</div>
          </div>
        </div>
        <button
          onClick={() => router.back()}
          className="flex items-center gap-2 text-[#A08B5C] hover:text-[#8A7549] font-medium transition-colors"
        >
          <ArrowLeft className="w-4 h-4" />
          Back to Activity Log
        </button>
      </div>

      {/* Results Table */}
      <div className="p-6">
        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead className="bg-[#F5E6D3]">
              <tr>
                <th className="text-left p-4 font-semibold text-sm border border-gray-300">
                  Product Code/Description
                </th>
                <th className="text-center p-4 font-semibold text-sm border border-gray-300">
                  Good Empties<br />Collected Qty
                </th>
                <th className="text-center p-4 font-semibold text-sm border border-gray-300">
                  Damage Empties<br />Collected Qty
                </th>
                <th className="text-center p-4 font-semibold text-sm border border-gray-300">
                  Sample<br />Good Qty
                </th>
                <th className="text-center p-4 font-semibold text-sm border border-gray-300">
                  Total In Hand
                </th>
              </tr>
            </thead>
            <tbody>
              {damageProducts.map((product) => (
                <tr key={product.id} className="hover:bg-gray-50">
                  <td className="p-4 border border-gray-300">
                    <div className="flex items-center gap-3">
                      <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center">
                        <span className="text-2xl">🍺</span>
                      </div>
                      <div>
                        <div className="font-semibold text-gray-900">{product.name}</div>
                        <div className="text-sm text-gray-600">{product.code}</div>
                      </div>
                    </div>
                  </td>
                  <td className="text-center p-4 border border-gray-300">
                    <div className="font-medium text-gray-900">{product.goodCollected}</div>
                  </td>
                  <td className="text-center p-4 border border-gray-300">
                    <div className="font-medium text-red-600">{product.damageCollected}</div>
                  </td>
                  <td className="text-center p-4 border border-gray-300">
                    <div className="font-medium text-blue-600">{product.sampleGood}</div>
                  </td>
                  <td className="text-center p-4 border border-gray-300">
                    <div className="font-bold text-green-700">{product.totalInHand}</div>
                  </td>
                </tr>
              ))}
              {/* Total Row */}
              <tr className="bg-[#FFF8E7] font-bold">
                <td className="p-4 border border-gray-300 text-right">
                  <div className="font-bold text-gray-900">TOTAL</div>
                </td>
                <td className="text-center p-4 border border-gray-300">
                  <div className="font-bold text-gray-900">{totalGoodCollected}</div>
                </td>
                <td className="text-center p-4 border border-gray-300">
                  <div className="font-bold text-red-600">{totalDamageCollected}</div>
                </td>
                <td className="text-center p-4 border border-gray-300">
                  <div className="font-bold text-blue-600">{totalSampleGood}</div>
                </td>
                <td className="text-center p-4 border border-gray-300">
                  <div className="font-bold text-green-700">{totalInHand}</div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mt-6">
          <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
            <div className="text-sm text-blue-700 font-medium mb-1">Good Empties</div>
            <div className="text-2xl font-bold text-blue-900">{totalGoodCollected}</div>
          </div>
          <div className="bg-red-50 border border-red-200 rounded-lg p-4">
            <div className="text-sm text-red-700 font-medium mb-1">Damage Empties</div>
            <div className="text-2xl font-bold text-red-900">{totalDamageCollected}</div>
          </div>
          <div className="bg-purple-50 border border-purple-200 rounded-lg p-4">
            <div className="text-sm text-purple-700 font-medium mb-1">Sample Good</div>
            <div className="text-2xl font-bold text-purple-900">{totalSampleGood}</div>
          </div>
          <div className="bg-green-50 border border-green-200 rounded-lg p-4">
            <div className="text-sm text-green-700 font-medium mb-1">Total In Hand</div>
            <div className="text-2xl font-bold text-green-900">{totalInHand}</div>
          </div>
        </div>

        {/* Notes Section */}
        <div className="mt-6 bg-amber-50 border border-amber-200 rounded-lg p-4">
          <h3 className="font-semibold text-amber-900 mb-2 flex items-center gap-2">
            <span className="text-lg">ℹ️</span>
            Damage Analysis Notes
          </h3>
          <ul className="text-sm text-amber-800 space-y-1 ml-6 list-disc">
            <li>Total of {totalDamageCollected} damaged empties were collected from this delivery</li>
            <li>Damage rate: {((totalDamageCollected / (totalGoodCollected + totalDamageCollected)) * 100).toFixed(1)}% of total empties collected</li>
            <li>Sample goods have been taken from both good and damaged empties for quality inspection</li>
            <li>All damaged empties will be segregated and processed according to company policy</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
