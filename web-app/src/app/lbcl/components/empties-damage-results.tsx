"use client";

import { useState, useEffect, Fragment } from "react";
import { useRouter } from "next/navigation";
import { ArrowLeft, ChevronDown, ChevronUp, FileText } from "lucide-react";

type DamageProduct = {
  id: string;
  code: string;
  name: string;
  goodCollected: number;
  damageCollected: number;
  sampleGood: number;
  totalInHand: number;
};

type DamageItem = {
  damageIndex: number;
  imageCount: number;
  textNote: string;
  documentCount: number;
};

export function EmptiesDamageResults() {
  const router = useRouter();

  const [currentDate, setCurrentDate] = useState("");
  const [expandedProducts, setExpandedProducts] = useState<Set<string>>(new Set());
  const [damageItems, setDamageItems] = useState<Record<string, DamageItem[]>>({});

  useEffect(() => {
    const date = new Date();
    const formatted = date.toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).toUpperCase();
    setCurrentDate(formatted);

    // Initialize damage items with dummy data
    const initialDamageItems: Record<string, DamageItem[]> = {
      "1": [
        {
          damageIndex: 1,
          imageCount: 2,
          textNote: "Severe crack on the top rim of the keg, extends approximately 3 inches. Metal is slightly bent near the handle area.",
          documentCount: 2
        },
        {
          damageIndex: 2,
          imageCount: 1,
          textNote: "Dent on the side panel, approximately 2 inches deep. No visible cracks but structural integrity may be compromised.",
          documentCount: 1
        },
        {
          damageIndex: 3,
          imageCount: 2,
          textNote: "Rust spots visible on bottom section, approximately 30% surface area affected. Requires immediate attention.",
          documentCount: 0
        }
      ],
      "2": [
        {
          damageIndex: 1,
          imageCount: 1,
          textNote: "Label completely torn off, container is intact but not suitable for distribution without relabeling.",
          documentCount: 1
        },
        {
          damageIndex: 2,
          imageCount: 1,
          textNote: "Minor crack on handle, still functional but needs replacement before next use.",
          documentCount: 0
        }
      ],
      "3": [
        {
          damageIndex: 1,
          imageCount: 1,
          textNote: "Broken glass bottle, shattered at neck. Contents leaked completely.",
          documentCount: 1
        },
        {
          damageIndex: 2,
          imageCount: 1,
          textNote: "Cracked bottle bottom, visible hairline fracture extending vertically.",
          documentCount: 0
        }
      ],
      "4": [
        {
          damageIndex: 1,
          imageCount: 1,
          textNote: "Bottle cap damaged and bent, seal broken. Product not suitable for sale.",
          documentCount: 1
        }
      ]
    };
    setDamageItems(initialDamageItems);

    // Auto-expand all products with damage
    const expandedSet = new Set<string>();
    damageProducts.forEach(product => {
      if (product.damageCollected > 0) {
        expandedSet.add(product.id);
      }
    });
    setExpandedProducts(expandedSet);
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

  const toggleProductExpansion = (productId: string) => {
    setExpandedProducts(prev => {
      const newSet = new Set(prev);
      if (newSet.has(productId)) {
        newSet.delete(productId);
      } else {
        newSet.add(productId);
      }
      return newSet;
    });
  };

  const totalGoodCollected = damageProducts.reduce((sum, p) => sum + p.goodCollected, 0);
  const totalDamageCollected = damageProducts.reduce((sum, p) => sum + p.damageCollected, 0);
  const totalSampleGood = damageProducts.reduce((sum, p) => sum + p.sampleGood, 0);

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
        <h2 className="text-xl font-bold text-gray-900 mb-4">Damage Empties Documentation</h2>

        <div className="overflow-x-auto">
          <table className="w-full border-collapse">
            <thead className="bg-[#F5E6D3] sticky top-0 z-20">
              <tr>
                <th className="text-left p-3 font-semibold text-sm border border-gray-300">
                  Product Code/Description
                </th>
                <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                  Good Empties<br />Collected Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                  Damage Empties<br />Collected Qty
                </th>
                <th className="text-center p-3 font-semibold text-sm border border-gray-300">
                  Sample<br />Good Qty
                </th>
              </tr>
            </thead>
            <tbody>
              {damageProducts.map((product) => {
                const hasDamage = product.damageCollected > 0;
                const isExpanded = expandedProducts.has(product.id);
                const damages = damageItems[product.id] || [];

                return (
                  <Fragment key={product.id}>
                    {/* Main Product Row */}
                    <tr className="border-b border-gray-300 hover:bg-gray-50">
                      <td className="p-3 border border-gray-300">
                        <div className="flex items-center gap-3">
                          {hasDamage && (
                            <button
                              onClick={() => toggleProductExpansion(product.id)}
                              className="p-1 hover:bg-gray-200 rounded transition-colors"
                            >
                              {isExpanded ? (
                                <ChevronUp className="h-4 w-4 text-gray-600" />
                              ) : (
                                <ChevronDown className="h-4 w-4 text-gray-600" />
                              )}
                            </button>
                          )}
                          <div className="w-12 h-12 bg-gray-200 rounded flex items-center justify-center flex-shrink-0">
                            <span className="text-2xl">🍺</span>
                          </div>
                          <div>
                            <div className="font-semibold text-sm">{product.name}</div>
                            <div className="text-xs text-gray-600">{product.code}</div>
                          </div>
                        </div>
                      </td>
                      <td className="text-center p-3 border border-gray-300">
                        <div className="font-medium">{product.goodCollected}</div>
                      </td>
                      <td className="text-center p-3 border border-gray-300">
                        <div className="font-medium text-red-600">{product.damageCollected}</div>
                      </td>
                      <td className="text-center p-3 border border-gray-300">
                        <div className="font-medium text-blue-600">{product.sampleGood}</div>
                      </td>
                    </tr>

                    {/* Damage Item Child Rows */}
                    {hasDamage && isExpanded && damages.map((damage) => {
                      const key = `${product.id}-${damage.damageIndex}`;

                      return (
                        <tr key={key} className="bg-red-50 border-b border-gray-200">
                          <td colSpan={4} className="p-4 border border-gray-300">
                            <div className="bg-white rounded-lg p-4 shadow-sm">
                              {/* Header */}
                              <div className="flex items-center justify-between mb-4 pb-3 border-b border-gray-200">
                                <div className="flex items-center gap-3">
                                  <div className="bg-red-600 text-white px-3 py-1 rounded-md text-sm font-semibold">
                                    Damage Item {damage.damageIndex}
                                  </div>
                                  <div className="text-sm text-gray-600">
                                    <span className="font-medium">{product.code}</span> - {product.name}
                                  </div>
                                </div>
                              </div>

                              {/* Content Grid */}
                              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                                {/* Images Section */}
                                <div className="space-y-2">
                                  <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                    🖼️ Images ({damage.imageCount})
                                  </label>
                                  <div className="flex flex-wrap gap-2">
                                    {damage.imageCount > 0 ? (
                                      Array.from({ length: damage.imageCount }).map((_, imgIndex) => (
                                        <div
                                          key={imgIndex}
                                          className="h-24 w-24 rounded-md bg-gradient-to-br from-amber-100 to-amber-200 border-2 border-amber-300 flex items-center justify-center hover:border-red-600 transition-colors"
                                        >
                                          <div className="text-center">
                                            <div className="text-3xl mb-1">🍺</div>
                                            <div className="text-xs text-amber-700 font-semibold">IMG {imgIndex + 1}</div>
                                          </div>
                                        </div>
                                      ))
                                    ) : (
                                      <div className="text-sm text-gray-500 italic">No images available</div>
                                    )}
                                  </div>
                                </div>

                                {/* Notes Section */}
                                <div className="space-y-2">
                                  <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                    <FileText className="h-4 w-4" />
                                    Damage Notes
                                  </label>
                                  <div className="bg-gray-50 rounded-md p-3 border border-gray-200 min-h-[96px]">
                                    {damage.textNote ? (
                                      <p className="text-sm text-gray-700 leading-relaxed">{damage.textNote}</p>
                                    ) : (
                                      <p className="text-sm text-gray-500 italic">No notes available</p>
                                    )}
                                  </div>
                                </div>

                                {/* Documents Section */}
                                <div className="space-y-2">
                                  <label className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                                    📄 Documents ({damage.documentCount})
                                  </label>
                                  <div className="flex flex-wrap gap-2">
                                    {damage.documentCount > 0 ? (
                                      Array.from({ length: damage.documentCount }).map((_, docIndex) => (
                                        <div
                                          key={docIndex}
                                          className="h-24 w-24 px-2 rounded-md bg-blue-50 border-2 border-blue-200 flex flex-col items-center justify-center gap-1 hover:bg-blue-100 transition-colors"
                                        >
                                          <FileText className="h-6 w-6 text-blue-600" />
                                          <span className="text-[9px] text-blue-600 font-bold">DOC {docIndex + 1}</span>
                                        </div>
                                      ))
                                    ) : (
                                      <div className="text-sm text-gray-500 italic">No documents available</div>
                                    )}
                                  </div>
                                </div>
                              </div>
                            </div>
                          </td>
                        </tr>
                      );
                    })}
                  </Fragment>
                );
              })}

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
              </tr>
            </tbody>
          </table>
        </div>

        {/* Summary Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 mt-6">
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
        </div>
      </div>
    </div>
  );
}
