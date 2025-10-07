"use client";

import { useState, useCallback } from "react";
import { useRouter } from "next/navigation";
import { motion, AnimatePresence } from "framer-motion";
import { toast } from "sonner";

// Import components
import CreatePromotionHeader from "./CreatePromotionHeader";
import ProgressBar from "./ProgressBar";
import PromotionTypeStep from "./steps/PromotionTypeStep";
import BasicDetailsStep from "./steps/BasicDetailsStep";
import ConfigurationStep from "./steps/ConfigurationStep";
import FootprintStep from "./steps/FootprintStep";
import VolumeCapStep from "./steps/VolumeCapStep";
import ReviewStep from "./steps/ReviewStep";
import StepNavigation from "./StepNavigation";

// Import hooks
import { usePromotionValidation } from "../../hooks/usePromotionValidation";
import { usePromotionCode } from "../../hooks/usePromotionCode";

// Import services and types
import { promotionV3Service } from "../../services/promotionV3.service";
import { PromotionV3FormData } from "../../types/promotionV3.types";

// Import promotion configuration
import { PROMOTION_LEVELS } from "../../utils/promotionConfig";

const progressSteps = [
  { num: 1, label: "Choose Type", mobileLabel: "Type" },
  { num: 2, label: "Basic Details", mobileLabel: "Details" },
  { num: 3, label: "Configuration", mobileLabel: "Config" },
  { num: 4, label: "Footprint", mobileLabel: "Footprint" },
  { num: 5, label: "Volume Caps", mobileLabel: "Caps" },
  { num: 6, label: "Review", mobileLabel: "Review" }
];

export default function CreatePromotionWizard() {
  const router = useRouter();
  const { validateStep, getValidationErrors } = usePromotionValidation();
  const { generatePromotionCode } = usePromotionCode();

  const [currentStep, setCurrentStep] = useState(1);
  const [selectedLevel, setSelectedLevel] = useState("");
  const [selectedFormat, setSelectedFormat] = useState("");
  const [loading] = useState(false);
  const [saving, setSaving] = useState(false);

  // Get today's date in YYYY-MM-DD format
  const today = new Date().toISOString().split('T')[0];
  
  const [formData, setFormData] = useState<PromotionV3FormData>({
    promotionName: "",
    promotionCode: "",
    level: "",
    format: "",
    promoTitle: "",
    promoMessage: "",
    remarks: "",
    hasSlabs: false,
    slabs: [],
    orderType: "LINE",
    productSelection: {
      selectionType: "all" as "all" | "hierarchy" | "specific",
      hierarchySelections: {} as { [key: string]: string[] },
      specificProducts: [] as string[],
      excludedProducts: [] as string[],
      type: "all",
      brand: [],
      category: [],
      subCategory: [],
      skuGroup: [],
      sku: []
    },
    compulsoryItems: [],
    discountAmount: 0,
    discountPercent: 0,
    buyQty: 0,
    getQty: 0,
    minValue: 0,
    minQty: 0,
    minLineCount: 0,
    minBrandCount: 0,
    selectionModel: "any",
    focItems: {
      guaranteed: [],
      choice: {
        maxSelections: 1,
        items: []
      }
    },
    validFrom: today,
    validUpto: "",
    priority: 1,
    volumeCaps: {
      enabled: false,
      overallCap: {
        type: "value" as "value" | "quantity" | "count",
        value: 0,
        consumed: 0
      },
      invoiceCap: {
        maxDiscountValue: 0,
        maxQuantity: 0,
        maxApplications: 0
      },
      periodCaps: [],
      hierarchyCaps: []
    },
    footprint: {
      type: "all" as "all" | "hierarchy" | "specific",
      dynamicHierarchy: {},
      selectedOrgs: [],
      finalOrgUID: undefined as string | undefined,
      companyUID: undefined as string | undefined,
      organizationUID: undefined as string | undefined,
      selectedStoreGroups: [],
      selectedBranches: [],
      selectedStores: [],
      selectedCustomers: [],
      selectedSalesmen: [],
      specificStores: [],
      organization: [],
      location: [],
      branch: [],
      storeGroup: [],
      route: [],
      salesPerson: [],
      stores: [],
      selectedCountries: [],
      selectedDivisions: [],
      locationHierarchy: {
        countries: [],
        divisions: []
      }
    },
    productConfigs: [],
    excludedItems: [],
    applyForExcludedItems: false,
    isApprovalCreated: false,
    approvalStatus: "",
    maxDiscountAmount: 0,
    maxDealCount: 1,
    isActive: true
  });

  // Form update functions
  const updateFormData = useCallback((field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value,
    }));
  }, []);

  const updateNestedFormData = useCallback((path: string[], value: any) => {
    setFormData((prev) => {
      const newData = { ...prev };
      let current: any = newData;
      
      for (let i = 0; i < path.length - 1; i++) {
        if (current[path[i]] === undefined) {
          current[path[i]] = {};
        }
        current = current[path[i]];
      }
      
      current[path[path.length - 1]] = value;
      return newData;
    });
  }, []);

  // Submit promotion - defined before handleNext to avoid hoisting issues
  const handleSubmit = useCallback(async () => {
    setSaving(true);
    try {
      // Validate promotion code uniqueness before submission
      if (formData.promotionCode) {
        console.log('Validating promotion code:', formData.promotionCode);
        const validation = await promotionV3Service.validatePromotionCode(formData.promotionCode);
        
        if (!validation.isUnique) {
          toast.error(validation.message || 'Promotion code already exists. Please choose a different code.');
          setSaving(false);
          return;
        }
      }
      
      // Prepare data for submission
      const submitData = {
        ...formData,
        level: selectedLevel,
        format: selectedFormat,
      };

      console.log('Submitting promotion data:', submitData);

      // Call API to create promotion
      const response = await promotionV3Service.createPromotion(submitData);
      
      if (response.success) {
        toast.success("Promotion created successfully!");
        router.push("/updatedfeatures/promotion-management/promotions/manage");
      } else {
        throw new Error(response.error || "Failed to create promotion");
      }
    } catch (error) {
      console.error('Error creating promotion:', error);
      toast.error(error instanceof Error ? error.message : "Failed to create promotion. Please try again.");
    } finally {
      setSaving(false);
    }
  }, [formData, selectedLevel, selectedFormat, router]);

  // Step navigation
  const handleNext = useCallback(async () => {
    const isValid = validateStep(currentStep, formData, selectedLevel, selectedFormat);
    
    if (!isValid) {
      const errors = getValidationErrors(currentStep, formData, selectedLevel, selectedFormat);
      errors.forEach((error) => toast.error(error));
      return;
    }

    if (currentStep === 1) {
      // Update formData with selected level and format
      setFormData((prev) => ({
        ...prev,
        level: selectedLevel as 'instant' | 'invoice',
        format: selectedFormat,
        orderType: selectedLevel === "invoice" ? "INVOICE" : "LINE",
      }));
    }

    if (currentStep === 6) {
      // Last step - submit the promotion
      await handleSubmit();
    } else if (currentStep < 6) {
      setCurrentStep((prev) => prev + 1);
    }
  }, [currentStep, formData, selectedLevel, selectedFormat, validateStep, getValidationErrors, handleSubmit]);

  const handleBack = useCallback(() => {
    if (currentStep > 1) {
      setCurrentStep((prev) => prev - 1);
    } else {
      router.push("/updatedfeatures/promotion-management/promotions/manage");
    }
  }, [currentStep, router]);

  const handleStepClick = useCallback((step: number) => {
    // Allow navigation to previous steps only
    if (step < currentStep) {
      setCurrentStep(step);
    } else if (step === currentStep + 1) {
      // Allow going to next step if current step is valid
      handleNext();
    }
  }, [currentStep, handleNext]);

  // Name change handler (generates code automatically)
  const handleNameChange = useCallback((name: string) => {
    updateFormData("promotionName", name);
    if (!formData.promotionCode || formData.promotionCode.length === 0) {
      const generatedCode = generatePromotionCode(name);
      updateFormData("promotionCode", generatedCode);
    }
  }, [formData.promotionCode, updateFormData, generatePromotionCode]);

  // Generate new code
  const handleGenerateCode = useCallback(() => {
    const generatedCode = generatePromotionCode(formData.promotionName);
    updateFormData("promotionCode", generatedCode);
  }, [formData.promotionName, updateFormData, generatePromotionCode]);

  // Render current step
  const renderStep = () => {
    const stepVariants = {
      initial: { opacity: 0, x: 50 },
      animate: { opacity: 1, x: 0 },
      exit: { opacity: 0, x: -50 },
    };

    return (
      <AnimatePresence mode="wait">
        <motion.div
          key={currentStep}
          variants={stepVariants}
          initial="initial"
          animate="animate"
          exit="exit"
          transition={{ duration: 0.3 }}
          className="min-h-[400px]"
        >
          {currentStep === 1 && (
            <PromotionTypeStep
              selectedLevel={selectedLevel}
              selectedFormat={selectedFormat}
              onLevelSelect={setSelectedLevel}
              onFormatSelect={setSelectedFormat}
            />
          )}
          {currentStep === 2 && (
            <BasicDetailsStep
              formData={formData}
              onFormDataChange={updateFormData}
              onNameChange={handleNameChange}
              onGenerateCode={handleGenerateCode}
            />
          )}
          {currentStep === 3 && (
            <ConfigurationStep
              formData={formData}
              selectedLevel={selectedLevel}
              selectedFormat={selectedFormat}
              onFormDataChange={updateFormData}
            />
          )}
          {currentStep === 4 && (
            <FootprintStep
              footprint={formData.footprint}
              onFootprintUpdate={(field, value) => {
                if (field === "_batch") {
                  // Handle batch update - value is the complete footprint object
                  console.log('Batch updating footprint:', value);
                  updateFormData('footprint', value);
                } else {
                  // Handle single field update
                  const updatedFootprint = {
                    ...formData.footprint,
                    [field]: value
                  };
                  console.log('Updating footprint field:', field, 'with value:', value);
                  console.log('Updated footprint:', updatedFootprint);
                  updateFormData('footprint', updatedFootprint);
                }
              }}
            />
          )}
          {currentStep === 5 && (
            <VolumeCapStep
              volumeCaps={formData.volumeCaps}
              onVolumeCapsUpdate={(field, value) => {
                updateFormData('volumeCaps', {
                  ...formData.volumeCaps,
                  [field]: value
                });
              }}
            />
          )}
          {currentStep === 6 && (
            <ReviewStep
              formData={formData}
              selectedLevel={selectedLevel}
              selectedFormat={selectedFormat}
              promotionLevels={PROMOTION_LEVELS}
            />
          )}
        </motion.div>
      </AnimatePresence>
    );
  };

  return (
    <div className="h-full flex flex-col overflow-hidden">
      {/* Header and Progress Bar - Fixed at top */}
      <div className="flex-shrink-0 border-b border-gray-200">
        <CreatePromotionHeader 
          onBack={handleBack} 
          selectedLevel={selectedLevel}
          selectedFormat={selectedFormat}
        />
        <div>
          <ProgressBar
            steps={progressSteps}
            currentStep={currentStep}
            onStepClick={handleStepClick}
          />
        </div>
      </div>
      
      {/* Main Content - Only this scrolls */}
      <div className="flex-1 overflow-y-auto">
        <div className="px-6 py-6">
          {renderStep()}
        </div>
      </div>
      
      {/* Navigation - Fixed at bottom */}
      <div className="flex-shrink-0 border-t border-gray-200">
        <div className="px-6">
          <StepNavigation
            currentStep={currentStep}
            totalSteps={6}
            onNext={handleNext}
            onBack={handleBack}
            loading={loading || saving}
            canProceed={validateStep(currentStep, formData, selectedLevel, selectedFormat)}
          />
        </div>
      </div>
    </div>
  );
}