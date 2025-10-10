"use client";

import React from "react";
import { Check } from "lucide-react";
import { cn } from "@/lib/utils";

interface Step {
  num: number;
  label: string;
  mobileLabel: string;
}

interface ProgressBarProps {
  steps: Step[];
  currentStep: number;
  onStepClick?: (step: number) => void;
}

export default function ProgressBar({ steps, currentStep, onStepClick }: ProgressBarProps) {
  return (
    <div className="relative px-6 py-4">
      {/* Desktop Progress Bar */}
      <div className="hidden sm:block">
        <nav aria-label="Progress">
          <ol className="flex items-center justify-between">
            {steps.map((step, index) => (
              <li key={step.num} className="relative flex-1">
                <div className="flex flex-col items-center group">
                  {/* Step Line - positioned before circle for proper layering */}
                  {index !== steps.length - 1 && (
                    <div
                      className={cn(
                        "absolute top-5 left-1/2 h-0.5 w-full",
                        step.num < currentStep ? "bg-blue-500" : "bg-gray-300"
                      )}
                      style={{ transform: "translateY(-50%)" }}
                    />
                  )}
                  
                  {/* Step Circle and Number */}
                  <button
                    type="button"
                    className={cn(
                      "relative z-10 flex h-10 w-10 items-center justify-center rounded-full border-2 transition-all shadow-sm",
                      step.num < currentStep
                        ? "border-blue-500 bg-blue-500 hover:bg-blue-600 shadow-blue-100"
                        : step.num === currentStep
                        ? "border-blue-500 bg-blue-500 shadow-blue-100"
                        : "border-gray-300 bg-white hover:border-gray-400 shadow-gray-100",
                      onStepClick && step.num <= currentStep ? "cursor-pointer" : "cursor-not-allowed"
                    )}
                    onClick={() => onStepClick && step.num <= currentStep && onStepClick(step.num)}
                    disabled={!onStepClick || step.num > currentStep}
                  >
                    {step.num < currentStep ? (
                      <Check className="h-5 w-5 text-white" />
                    ) : (
                      <span
                        className={cn(
                          "text-sm font-semibold",
                          step.num === currentStep ? "text-white" : "text-gray-600"
                        )}
                      >
                        {step.num}
                      </span>
                    )}
                  </button>
                  
                  {/* Step Label */}
                  <span 
                    className={cn(
                      "mt-2 text-xs font-medium text-center",
                      step.num === currentStep ? "text-blue-600" : "text-gray-500"
                    )}
                  >
                    {step.label}
                  </span>
                </div>
              </li>
            ))}
          </ol>
        </nav>
      </div>

      {/* Mobile Progress Bar */}
      <div className="sm:hidden">
        <div className="flex items-center justify-between">
          <span className="text-sm text-gray-500">
            Step {currentStep} of {steps.length}
          </span>
          <span className="text-sm font-medium text-gray-900">
            {steps[currentStep - 1]?.mobileLabel}
          </span>
        </div>
        <div className="mt-2">
          <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
            <div
              className="h-full bg-blue-500 rounded-full transition-all duration-300"
              style={{ width: `${(currentStep / steps.length) * 100}%` }}
            />
          </div>
        </div>
      </div>
    </div>
  );
}