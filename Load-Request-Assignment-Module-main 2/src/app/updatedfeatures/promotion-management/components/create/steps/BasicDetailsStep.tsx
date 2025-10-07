"use client";

import React, { useState, useEffect, useCallback } from 'react';
import { ChevronDown, ChevronUp, RefreshCw, Calendar, Info, CheckCircle, XCircle, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Card, CardContent } from '@/components/ui/card';
import { PromotionV3FormData } from '../../../types/promotionV3.types';
import { promotionV3Service } from '../../../services/promotionV3.service';

interface BasicDetailsStepProps {
  formData: PromotionV3FormData;
  onFormDataChange: (field: string, value: any) => void;
  onNameChange: (name: string) => void;
  onGenerateCode: () => void;
}

export default function BasicDetailsStep({
  formData,
  onFormDataChange,
  onNameChange,
  onGenerateCode
}: BasicDetailsStepProps) {
  const [showOptionalFields, setShowOptionalFields] = useState(false);
  
  // State for promotion code validation
  const [codeValidation, setCodeValidation] = useState<{
    isValidating: boolean;
    isValid: boolean | null;
    message: string;
  }>({
    isValidating: false,
    isValid: null,
    message: ''
  });

  // Validate promotion code with debouncing
  const validatePromotionCode = useCallback(async (code: string) => {
    if (!code || code.length < 3) {
      setCodeValidation({
        isValidating: false,
        isValid: null,
        message: ''
      });
      return;
    }

    setCodeValidation(prev => ({ ...prev, isValidating: true }));

    try {
      const result = await promotionV3Service.validatePromotionCode(code);
      
      setCodeValidation({
        isValidating: false,
        isValid: result.isUnique,
        message: result.isUnique 
          ? 'Code is available' 
          : (result.message || 'This code is already in use')
      });
    } catch (error) {
      setCodeValidation({
        isValidating: false,
        isValid: null,
        message: 'Unable to validate code'
      });
    }
  }, []);

  // Debounced validation effect
  useEffect(() => {
    const timeoutId = setTimeout(() => {
      if (formData.promotionCode) {
        validatePromotionCode(formData.promotionCode);
      }
    }, 500); // 500ms debounce

    return () => clearTimeout(timeoutId);
  }, [formData.promotionCode, validatePromotionCode]);

  // Calculate min date for validUpto (must be after validFrom)
  const getMinEndDate = () => {
    if (formData.validFrom) {
      const date = new Date(formData.validFrom);
      date.setDate(date.getDate() + 1);
      return date.toISOString().split('T')[0];
    }
    return new Date().toISOString().split('T')[0];
  };

  // Calculate default end date (30 days from start date)
  const getDefaultEndDate = () => {
    const startDate = formData.validFrom ? new Date(formData.validFrom) : new Date();
    startDate.setDate(startDate.getDate() + 30);
    return startDate.toISOString().split('T')[0];
  };

  return (
    <div className="max-w-[75%] space-y-4">
      <div className="mb-6">
        <h2 className="text-xl font-medium text-gray-900">Basic Details</h2>
      </div>
      
      {/* Basic Information Section */}
      <div className="border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 mb-4">
          Basic Information
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Promotion Name - Full Width */}
          <div className="md:col-span-2">
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Promotion Name <span className="text-red-500">*</span>
            </Label>
            <Input
              id="promotionName"
              type="text"
              value={formData.promotionName}
              onChange={(e) => onNameChange(e.target.value)}
              placeholder="e.g., Summer Special Offer"
              className="h-10"
            />
          </div>

          {/* Promotion Code */}
          <div>
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Promotion Code <span className="text-red-500">*</span>
            </Label>
            <div className="flex gap-2">
              <div className="relative flex-1">
                <Input
                  id="promotionCode"
                  type="text"
                  value={formData.promotionCode}
                  onChange={(e) =>
                    onFormDataChange("promotionCode", e.target.value.toUpperCase())
                  }
                  placeholder="Auto-generated"
                  className={`h-10 font-mono text-sm pr-8 ${
                    codeValidation.isValid === false 
                      ? 'border-red-500 focus:border-red-500' 
                      : codeValidation.isValid === true 
                      ? 'border-green-500 focus:border-green-500' 
                      : ''
                  }`}
                />
                {/* Validation icon */}
                <div className="absolute right-2 top-1/2 transform -translate-y-1/2">
                  {codeValidation.isValidating && (
                    <Loader2 className="h-4 w-4 animate-spin text-gray-400" />
                  )}
                  {!codeValidation.isValidating && codeValidation.isValid === true && (
                    <CheckCircle className="h-4 w-4 text-green-500" />
                  )}
                  {!codeValidation.isValidating && codeValidation.isValid === false && (
                    <XCircle className="h-4 w-4 text-red-500" />
                  )}
                </div>
              </div>
              <Button
                type="button"
                variant="outline"
                size="icon"
                onClick={onGenerateCode}
                disabled={!formData.promotionName.trim()}
                className="h-10 w-10"
              >
                <RefreshCw className="h-4 w-4" />
              </Button>
            </div>
            
            {/* Validation message */}
            {codeValidation.message && (
              <p className={`text-xs mt-1 ${
                codeValidation.isValid === false 
                  ? 'text-red-600' 
                  : codeValidation.isValid === true 
                  ? 'text-green-600' 
                  : 'text-gray-500'
              }`}>
                {codeValidation.message}
              </p>
            )}
            
            {/* Default help text when no validation message */}
            {!codeValidation.message && (
              <p className="text-xs text-gray-500 mt-1">
                Code auto-generates when you enter a name, or click Generate for a new one
              </p>
            )}
          </div>

          {/* Remarks */}
          <div>
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Remarks
            </Label>
            <textarea
              id="remarks"
              value={formData.remarks || ''}
              onChange={(e) =>
                onFormDataChange("remarks", e.target.value)
              }
              placeholder="Brief promotion remarks or notes"
              rows={2}
              className="w-full h-10 px-3 py-2 text-sm border border-gray-300 rounded-md focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none resize-none"
            />
          </div>
        </div>
      </div>

      {/* Validity Section */}
      <div className="border border-gray-200 rounded-lg p-5">
        <h3 className="text-sm font-semibold text-gray-900 mb-4">
          Validity & Settings
        </h3>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          {/* Valid From */}
          <div>
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Valid From <span className="text-red-500">*</span>
            </Label>
            <Input
              id="validFrom"
              type="date"
              value={formData.validFrom}
              onChange={(e) => {
                onFormDataChange("validFrom", e.target.value);
                if (formData.validUpto && e.target.value > formData.validUpto) {
                  onFormDataChange("validUpto", getDefaultEndDate());
                }
              }}
              min={new Date().toISOString().split('T')[0]}
              className="h-10"
            />
            <p className="text-xs text-gray-500 mt-1">Start date of the promotion</p>
          </div>

          {/* Valid Until */}
          <div>
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Valid Until <span className="text-red-500">*</span>
            </Label>
            <Input
              id="validUpto"
              type="date"
              value={formData.validUpto}
              onChange={(e) =>
                onFormDataChange("validUpto", e.target.value)
              }
              min={getMinEndDate()}
              className="h-10"
            />
            <p className="text-xs text-gray-500 mt-1">End date of the promotion</p>
          </div>

          {/* Priority */}
          <div>
            <Label className="text-sm font-medium text-gray-700 mb-1.5">
              Priority
            </Label>
            <Input
              id="priority"
              type="number"
              value={formData.priority}
              onChange={(e) =>
                onFormDataChange("priority", parseInt(e.target.value) || 1)
              }
              min="1"
              max="100"
              className="h-10"
              placeholder="1"
            />
            <p className="text-xs text-gray-500 mt-1">Higher priority promotions are applied first (1-100)</p>
          </div>
        </div>
      </div>

      {/* Optional Fields Section */}
      <div className="border border-gray-200 rounded-lg">
        <button
          type="button"
          onClick={() => setShowOptionalFields(!showOptionalFields)}
          className="w-full flex items-center justify-between p-5 text-left hover:bg-gray-50 rounded-lg transition-colors"
        >
          <div>
            <h3 className="text-sm font-semibold text-gray-900">Optional Fields</h3>
            <p className="text-sm text-gray-500 mt-1">Customer-facing content</p>
          </div>
          {showOptionalFields ? (
            <ChevronUp className="w-5 h-5 text-gray-400" />
          ) : (
            <ChevronDown className="w-5 h-5 text-gray-400" />
          )}
        </button>
        
        {showOptionalFields && (
          <div className="border-t border-gray-200 p-5 space-y-4">
            <div className="space-y-4">
              {/* Promotion Title */}
              <div>
                <Label className="text-sm font-medium text-gray-700 mb-1.5">
                  Promotion Title (Customer Facing)
                </Label>
                <Input
                  id="promoTitle"
                  type="text"
                  value={formData.promoTitle || ''}
                  onChange={(e) =>
                    onFormDataChange("promoTitle", e.target.value)
                  }
                  placeholder="e.g., Summer Special - Get 20% Off!"
                  className="h-10"
                />
                <p className="text-xs text-gray-500 mt-1">
                  This title will be visible to customers
                </p>
              </div>

              {/* Promotion Message */}
              <div>
                <Label className="text-sm font-medium text-gray-700 mb-1.5">
                  Promotion Message (Customer Facing)
                </Label>
                <textarea
                  id="promoMessage"
                  value={formData.promoMessage || ''}
                  onChange={(e) =>
                    onFormDataChange("promoMessage", e.target.value)
                  }
                  placeholder="Enter a message that customers will see"
                  rows={3}
                  className="w-full px-3 py-2 text-sm border border-gray-300 rounded-md focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:outline-none resize-none"
                />
                <p className="text-xs text-gray-500 mt-1">
                  This message will appear on invoices and receipts
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}