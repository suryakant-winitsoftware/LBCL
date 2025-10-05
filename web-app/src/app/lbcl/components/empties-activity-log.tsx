"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ChevronDown, ChevronRight, FileText, Check } from "lucide-react";
import { PhysicalCountModal } from "@/app/lbcl/components/physical-count-modal";
import { RateAgentModal } from "@/app/lbcl/components/rate-agent-modal";

interface ActivityStep {
  id: number;
  title: string;
  completed: boolean;
  expandable?: boolean;
}

export function EmptiesActivityLog() {
  const router = useRouter();
  const [expandedStep, setExpandedStep] = useState<number | null>(2);
  const [showPhysicalCount, setShowPhysicalCount] = useState(false);
  const [showRateAgent, setShowRateAgent] = useState(false);

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

  const handleStepClick = (stepId: number) => {
    if (stepId === 3) {
      setShowPhysicalCount(true);
    } else if (stepId === 5) {
      setShowRateAgent(true);
    } else if (steps.find((s) => s.id === stepId)?.expandable) {
      setExpandedStep(expandedStep === stepId ? null : stepId);
    }
  };

  const leftColumnSteps = steps.slice(0, 4);
  const rightColumnSteps = steps.slice(4, 7);

  return (
    <>
      <div className="min-h-screen bg-gray-50">
        {/* Info Section */}
        <div className="bg-white p-4 border-b border-gray-200">
          <div className="grid grid-cols-5 gap-4">
            <div>
              <div className="text-xs text-gray-500 mb-1">
                Empties Delivery No
              </div>
              <div className="font-bold text-gray-900">EMT85444127121</div>
            </div>
            <div>
              <div className="text-xs text-gray-500 mb-1">Distributor</div>
              <div className="font-bold text-gray-900">
                [5844] R.T DISTRIBUTOR
              </div>
            </div>
            <div>
              <div className="text-xs text-gray-500 mb-1">Prime Mover</div>
              <div className="font-bold text-gray-900">LK1673</div>
            </div>
            <div>
              <div className="text-xs text-gray-500 mb-1">Date</div>
              <div className="font-bold text-gray-900">20 MAY 2025</div>
            </div>
            <div>
              <div className="text-xs text-gray-500 mb-1">
                Agent Departure Time
              </div>
              <div className="font-bold text-gray-900">18:14 HRS</div>
            </div>
          </div>
        </div>

        {/* Action Buttons */}
        <div className="p-4 flex gap-3 justify-end">
          <button
            onClick={() => router.back()}
            className="py-3 px-8 bg-white border-2 border-[#A08B5C] text-[#A08B5C] rounded-lg font-semibold hover:bg-gray-50 transition-colors"
          >
            Back
          </button>
          <button className="py-3 px-8 bg-[#A08B5C] text-white rounded-lg font-semibold hover:bg-[#8F7A4D] transition-colors">
            Submit
          </button>
        </div>

        <div className="p-4 grid grid-cols-2 gap-4">
          {/* Left Column - Steps 1-4 */}
          <div className="space-y-3">
            {leftColumnSteps.map((step) => (
              <div
                key={step.id}
                className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden"
              >
                <button
                  onClick={() => handleStepClick(step.id)}
                  className="w-full p-4 flex items-center gap-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="w-10 h-10 rounded-full bg-[#FFF8E7] flex items-center justify-center flex-shrink-0">
                    <span className="font-bold text-[#A08B5C]">{step.id}</span>
                  </div>
                  <div className="flex-1 text-left">
                    <div className="font-semibold text-gray-900">
                      {step.title}
                    </div>
                  </div>
                  {step.id === 1 && (
                    <FileText className="w-5 h-5 text-red-600" />
                  )}
                  {step.expandable && (
                    <ChevronDown
                      className={`w-5 h-5 text-gray-400 transition-transform ${
                        expandedStep === step.id ? "rotate-180" : ""
                      }`}
                    />
                  )}
                  {!step.expandable && step.id !== 1 && (
                    <ChevronRight className="w-5 h-5 text-gray-400" />
                  )}
                </button>

                {/* Expanded Content for Gate Entry */}
                {step.id === 2 && expandedStep === 2 && (
                  <div className="px-4 pb-4 space-y-4 border-t border-gray-200 pt-4">
                    <div className="grid grid-cols-2 gap-4">
                      <div>
                        <label className="text-sm text-gray-600 mb-2 block">
                          Select Security Officer
                        </label>
                        <select className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white">
                          <option>Vasanth Kumar</option>
                        </select>
                      </div>
                      <div>
                        <label className="text-sm text-gray-600 mb-2 block">
                          Prime Mover Arrival
                        </label>
                        <div className="flex gap-2">
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                              defaultValue="19"
                            />
                            <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                              HH
                            </span>
                          </div>
                          <div className="relative flex-1">
                            <input
                              type="number"
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                              defaultValue="00"
                            />
                            <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                              Min
                            </span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="relative">
                        <input
                          type="checkbox"
                          id="notify"
                          className="peer sr-only"
                          defaultChecked
                        />
                        <label
                          htmlFor="notify"
                          className="flex items-center justify-center w-5 h-5 border-2 border-gray-300 rounded cursor-pointer peer-checked:bg-green-500 peer-checked:border-green-500"
                        >
                          <Check className="w-3 h-3 text-white hidden peer-checked:block" />
                        </label>
                      </div>
                      <label
                        htmlFor="notify"
                        className="text-sm text-gray-700 cursor-pointer"
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
          <div className="space-y-3">
            {rightColumnSteps.map((step) => (
              <div
                key={step.id}
                className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden"
              >
                <button
                  onClick={() => handleStepClick(step.id)}
                  className="w-full p-4 flex items-center gap-4 hover:bg-gray-50 transition-colors"
                >
                  <div className="w-10 h-10 rounded-full bg-[#FFF8E7] flex items-center justify-center flex-shrink-0">
                    <span className="font-bold text-[#A08B5C]">{step.id}</span>
                  </div>
                  <div className="flex-1 text-left">
                    <div className="font-semibold text-gray-900">
                      {step.title}
                    </div>
                  </div>
                  {step.expandable && (
                    <ChevronDown
                      className={`w-5 h-5 text-gray-400 transition-transform ${
                        expandedStep === step.id ? "rotate-180" : ""
                      }`}
                    />
                  )}
                  {!step.expandable && (
                    <ChevronRight className="w-5 h-5 text-gray-400" />
                  )}
                </button>

                {/* Expanded Content for Unloading */}
                {step.id === 6 && expandedStep === 6 && (
                  <div className="px-4 pb-4 space-y-4 border-t border-gray-200 pt-4">
                    <div className="space-y-4">
                      <div>
                        <label className="text-sm text-gray-600 mb-2 block">
                          LBCL Fork Lift Operator
                        </label>
                        <select className="w-full px-3 py-2 border border-gray-300 rounded-lg bg-white">
                          <option>Tarun Prasad</option>
                        </select>
                      </div>
                      <div className="grid grid-cols-2 gap-4">
                        <div>
                          <label className="text-sm text-gray-600 mb-2 block">
                            Unload Start Time
                          </label>
                          <div className="flex gap-2">
                            <div className="relative flex-1">
                              <input
                                type="number"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                                defaultValue="19"
                              />
                              <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                                HH
                              </span>
                            </div>
                            <div className="relative flex-1">
                              <input
                                type="number"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                                defaultValue="02"
                              />
                              <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                                Min
                              </span>
                            </div>
                          </div>
                        </div>
                        <div>
                          <label className="text-sm text-gray-600 mb-2 block">
                            Unload End Time
                          </label>
                          <div className="flex gap-2">
                            <div className="relative flex-1">
                              <input
                                type="number"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                                defaultValue="16"
                              />
                              <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                                HH
                              </span>
                            </div>
                            <div className="relative flex-1">
                              <input
                                type="number"
                                className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                                defaultValue="58"
                              />
                              <span className="absolute right-3 top-1/2 -translate-y-1/2 text-xs text-gray-400">
                                Min
                              </span>
                            </div>
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

      {showPhysicalCount && (
        <PhysicalCountModal onClose={() => setShowPhysicalCount(false)} />
      )}
      {showRateAgent && (
        <RateAgentModal onClose={() => setShowRateAgent(false)} />
      )}
    </>
  );
}
