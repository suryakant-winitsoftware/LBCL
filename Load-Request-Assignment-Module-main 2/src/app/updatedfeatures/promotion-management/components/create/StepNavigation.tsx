"use client";

import React from "react";
import { ArrowLeft, ArrowRight, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";

interface StepNavigationProps {
  currentStep: number;
  totalSteps: number;
  onNext: () => void;
  onBack: () => void;
  loading?: boolean;
  canProceed?: boolean;
}

export default function StepNavigation({
  currentStep,
  totalSteps,
  onNext,
  onBack,
  loading = false,
  canProceed = true,
}: StepNavigationProps) {
  const isLastStep = currentStep === totalSteps;
  const isFirstStep = currentStep === 1;

  return (
    <div className="py-4">
      <div className="flex items-center justify-between">
        {/* Left Side - Previous Button */}
        <div>
          <Button
            variant="outline"
            onClick={onBack}
            disabled={loading}
            className="flex items-center gap-2 h-10 px-5 border-gray-300 hover:bg-gray-50 hover:border-gray-400 rounded-lg font-medium transition-all"
          >
            <ArrowLeft className="h-4 w-4" />
            <span>Previous</span>
          </Button>
        </div>

        {/* Right Side - Next/Submit Button */}
        <div>
          <Button
            onClick={onNext}
            disabled={loading || !canProceed}
            className={`flex items-center gap-2 h-10 px-5 rounded-lg font-medium transition-all shadow-sm ${
              isLastStep 
                ? "bg-green-600 hover:bg-green-700 hover:shadow-md" 
                : "bg-blue-600 hover:bg-blue-700 hover:shadow-md"
            } text-white disabled:opacity-50 disabled:cursor-not-allowed`}
          >
            {loading ? (
              <>
                <Loader2 className="h-4 w-4 animate-spin" />
                <span>Processing...</span>
              </>
            ) : (
              <>
                <span>{isLastStep ? "Create Promotion" : "Next Step"}</span>
                {!isLastStep && <ArrowRight className="h-4 w-4" />}
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}