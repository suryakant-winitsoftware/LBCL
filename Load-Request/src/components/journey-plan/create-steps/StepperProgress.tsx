import React from 'react';
import { motion } from 'framer-motion';
import { cn } from '@/lib/utils';
import { CheckCircle2, LucideIcon } from 'lucide-react';

export interface StepConfig {
  num: number;
  label: string;
  mobileLabel?: string;
  icon: LucideIcon;
}

interface StepperProgressProps {
  steps: StepConfig[];
  currentStep: number;
  completedSteps?: number[];
  onStepClick?: (step: number) => void;
  allowNavigation?: boolean;
}

export const StepperProgress: React.FC<StepperProgressProps> = ({
  steps,
  currentStep,
  completedSteps = [],
  onStepClick,
  allowNavigation = true,
}) => {
  const handleStepClick = (stepNum: number) => {
    if (allowNavigation && onStepClick && (completedSteps.includes(stepNum) || stepNum <= currentStep)) {
      onStepClick(stepNum);
    }
  };

  return (
    <div className="w-full">
      {/* Desktop Stepper */}
      <div className="hidden md:block">
        <div className="flex items-center justify-between relative">
          {/* Progress Line Background */}
          <div className="absolute left-0 right-0 top-1/2 -translate-y-1/2 h-[2px] bg-muted-foreground/20" />
          
          {/* Active Progress Line */}
          <motion.div
            className="absolute left-0 top-1/2 -translate-y-1/2 h-[2px] bg-primary"
            initial={{ width: '0%' }}
            animate={{
              width: `${((currentStep - 1) / (steps.length - 1)) * 100}%`
            }}
            transition={{ duration: 0.3, ease: 'easeInOut' }}
          />

          {/* Steps */}
          {steps.map((step) => {
            const isActive = step.num === currentStep;
            const isCompleted = completedSteps.includes(step.num) || step.num < currentStep;
            const isClickable = allowNavigation && (isCompleted || step.num <= currentStep);
            const Icon = step.icon;

            return (
              <div
                key={step.num}
                className={cn(
                  "relative z-10 flex flex-col items-center",
                  isClickable && "cursor-pointer"
                )}
                onClick={() => handleStepClick(step.num)}
              >
                <motion.div
                  className={cn(
                    "w-12 h-12 rounded-full flex items-center justify-center border-2 transition-all",
                    isActive && "bg-primary border-primary text-primary-foreground shadow-lg scale-110",
                    isCompleted && !isActive && "bg-primary/10 border-primary text-primary",
                    !isActive && !isCompleted && "bg-background border-muted-foreground/30 text-muted-foreground"
                  )}
                  whileHover={isClickable ? { scale: 1.05 } : {}}
                  whileTap={isClickable ? { scale: 0.95 } : {}}
                >
                  {isCompleted && !isActive ? (
                    <CheckCircle2 className="h-6 w-6" />
                  ) : (
                    <Icon className="h-5 w-5" />
                  )}
                </motion.div>
                <div className="mt-2 text-center">
                  <p className={cn(
                    "text-sm font-medium",
                    isActive && "text-primary",
                    isCompleted && !isActive && "text-primary",
                    !isActive && !isCompleted && "text-muted-foreground"
                  )}>
                    {step.label}
                  </p>
                  <p className={cn(
                    "text-xs mt-0.5",
                    isActive && "text-primary/80",
                    !isActive && "text-muted-foreground"
                  )}>
                    Step {step.num}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      {/* Mobile Stepper */}
      <div className="md:hidden">
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            {steps.map((step) => {
              const isActive = step.num === currentStep;
              const isCompleted = completedSteps.includes(step.num) || step.num < currentStep;
              const isClickable = allowNavigation && (isCompleted || step.num <= currentStep);

              return (
                <motion.div
                  key={step.num}
                  className={cn(
                    "w-2 h-2 rounded-full transition-all",
                    isActive && "w-8 bg-primary",
                    isCompleted && !isActive && "bg-primary/50",
                    !isActive && !isCompleted && "bg-muted-foreground/30",
                    isClickable && "cursor-pointer"
                  )}
                  onClick={() => handleStepClick(step.num)}
                  whileTap={isClickable ? { scale: 0.9 } : {}}
                />
              );
            })}
          </div>
          <span className="text-sm text-muted-foreground">
            {currentStep} / {steps.length}
          </span>
        </div>
        
        <div className="bg-muted/50 rounded-lg p-3">
          <div className="flex items-center gap-3">
            {(() => {
              const currentStepConfig = steps.find(s => s.num === currentStep);
              const Icon = currentStepConfig?.icon;
              return Icon ? (
                <div className="w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
                  <Icon className="h-5 w-5 text-primary" />
                </div>
              ) : null;
            })()}
            <div>
              <p className="font-medium text-sm">
                {steps.find(s => s.num === currentStep)?.mobileLabel || steps.find(s => s.num === currentStep)?.label}
              </p>
              <p className="text-xs text-muted-foreground">
                Step {currentStep} of {steps.length}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};