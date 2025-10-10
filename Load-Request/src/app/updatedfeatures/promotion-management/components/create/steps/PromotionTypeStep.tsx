"use client";

import React from "react";
import { Card, CardContent } from "@/components/ui/card";
import { PROMOTION_LEVELS } from "../../../utils/promotionConfig";
import { cn } from "@/lib/utils";
import { Check, Percent, DollarSign, Gift, Package, Settings } from "lucide-react";

interface PromotionTypeStepProps {
  selectedLevel: string;
  selectedFormat: string;
  onLevelSelect: (level: string) => void;
  onFormatSelect: (format: string) => void;
}

export default function PromotionTypeStep({
  selectedLevel,
  selectedFormat,
  onLevelSelect,
  onFormatSelect
}: PromotionTypeStepProps) {
  // Format icon mapping
  const getFormatIcon = (formatValue: string) => {
    switch (formatValue) {
      case 'IQFD': return DollarSign;
      case 'IQPD': return Percent;
      case 'IQXF': return Gift;
      case 'BQXF': return Package;
      case 'MPROD': return Settings;
      default: return Package;
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h2 className="text-xl font-semibold text-gray-900 mb-2">
          Choose Promotion Type
        </h2>
        <p className="text-sm text-gray-600">
          Select the level and format for your promotion
        </p>
      </div>

      {/* Level Selection */}
      <div>
        <div className="flex items-center gap-3 mb-4">
          <span className="text-sm font-medium text-gray-900">1</span>
          <h3 className="text-base font-medium text-gray-900">
            Promotion Level <span className="text-red-500">*</span>
          </h3>
        </div>
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 mb-6">
          {PROMOTION_LEVELS.map((level) => {
            const Icon = level.icon;
            return (
              <div
                key={level.id}
                className={cn(
                  "cursor-pointer transition-all border rounded-lg hover:border-blue-400 hover:shadow-sm",
                  selectedLevel === level.id
                    ? "border-blue-500 bg-blue-50 shadow-sm"
                    : "border-gray-300 bg-white"
                )}
                onClick={() => {
                  onLevelSelect(level.id);
                  onFormatSelect("");
                }}
              >
                <div className="p-4">
                  <div className="flex items-center gap-4">
                    <div className={cn(
                      "p-2 rounded-lg",
                      selectedLevel === level.id
                        ? "bg-blue-100 text-blue-600"
                        : "bg-gray-100 text-gray-600"
                    )}>
                      <Icon className="w-5 h-5" />
                    </div>
                    <div className="flex-1">
                      <h4 className="text-base font-medium text-gray-900 mb-1">
                        {level.name}
                      </h4>
                      <p className="text-sm text-gray-500">
                        {level.description}
                      </p>
                    </div>
                    {selectedLevel === level.id && (
                      <Check className="w-5 h-5 text-blue-600" />
                    )}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Format Selection */}
      {selectedLevel && (
        <div>
          <div className="flex items-center gap-3 mb-4">
            <span className="text-sm font-medium text-gray-900">2</span>
            <h3 className="text-base font-medium text-gray-900">
              Promotion Format <span className="text-red-500">*</span>
            </h3>
          </div>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            {PROMOTION_LEVELS.find((level) => level.id === selectedLevel)?.formats.map((format) => {
              const FormatIcon = getFormatIcon(format.value);
              return (
                <div
                  key={format.value}
                  className={cn(
                    "cursor-pointer transition-all border rounded-lg hover:border-blue-400 hover:shadow-sm",
                    selectedFormat === format.value
                      ? "border-blue-500 bg-blue-50 shadow-sm"
                      : "border-gray-300 bg-white"
                  )}
                  onClick={() => onFormatSelect(format.value)}
                >
                  <div className="p-4">
                    <div className="flex items-center gap-4">
                      <div className={cn(
                        "p-2 rounded-lg",
                        selectedFormat === format.value
                          ? "bg-blue-100 text-blue-600"
                          : "bg-gray-100 text-gray-600"
                      )}>
                        <FormatIcon className="w-5 h-5" />
                      </div>
                      <div className="flex-1">
                        <h4 className="text-base font-medium text-gray-900 mb-1">
                          {format.label}
                        </h4>
                        <p className="text-sm text-gray-500">
                          {format.description}
                        </p>
                      </div>
                      {selectedFormat === format.value && (
                        <Check className="w-5 h-5 text-blue-600" />
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}