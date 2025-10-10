"use client";

import React from "react";
import { X, ChevronRight } from "lucide-react";
import { Button } from "@/components/ui/button";
import { getPromotionLevelName, getPromotionFormatLabel } from "../../utils/promotionConfig";

interface CreatePromotionHeaderProps {
  onBack: () => void;
  selectedLevel?: string;
  selectedFormat?: string;
}

export default function CreatePromotionHeader({ 
  onBack, 
  selectedLevel = "", 
  selectedFormat = "" 
}: CreatePromotionHeaderProps) {

  return (
    <div className="bg-white border-b border-gray-200">
      <div className="px-6 py-4">
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-xl font-semibold text-gray-900">Create a New Promotional Campaign</h1>
            </div>
            {selectedLevel && selectedFormat && (
              <div className="flex items-center gap-2 mt-2">
                <div className="text-sm text-gray-600 bg-gray-50 px-3 py-1 rounded border border-blue-300">
                  {getPromotionLevelName(selectedLevel)}
                </div>
                <ChevronRight className="h-4 w-4 text-gray-400" />
                <div className="text-sm text-gray-600 bg-gray-50 px-3 py-1 rounded border border-blue-300">
                  {getPromotionFormatLabel(selectedLevel, selectedFormat)}
                </div>
              </div>
            )}
          </div>
          <Button
            variant="ghost"
            size="sm"
            onClick={onBack}
            className="text-red-500 hover:text-red-600 hover:bg-red-50 border border-red-200 hover:border-red-300"
          >
            <X className="h-5 w-5" />
          </Button>
        </div>
      </div>
    </div>
  );
}