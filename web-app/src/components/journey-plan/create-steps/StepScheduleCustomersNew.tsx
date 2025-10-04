import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { 
  ScheduleCustomersManager, 
  type Customer, 
  type TimeSlot, 
  type ScheduleConfig 
} from '../common/ScheduleCustomersManager';

interface StepScheduleCustomersProps {
  form: UseFormReturn<any>;
  routeCustomers: Customer[];
  dropdownsLoading: any;
}

export const StepScheduleCustomers: React.FC<StepScheduleCustomersProps> = ({
  form,
  routeCustomers,
  dropdownsLoading,
}) => {
  // Handle schedule updates from the common component
  const handleScheduleUpdate = (timeSlots: TimeSlot[], config: ScheduleConfig) => {
    // Additional processing specific to journey plan creation
    console.log('Schedule updated:', { timeSlots: timeSlots.length, config });
    
    // You can add custom logic here for the create page
    // For example, updating specific form fields or validation
  };

  // Handle customer assignment updates
  const handleCustomersAssigned = (selectedCustomers: Set<string>) => {
    console.log('Customers assigned:', selectedCustomers.size);
    
    // Custom logic for tracking customer assignments
    form.setValue('totalSelectedCustomers', selectedCustomers.size);
  };

  // Get initial configuration from form values
  const initialConfig: Partial<ScheduleConfig> = {
    dayStartTime: form.watch('plannedStartTime') || form.watch('dayStartsAt') || '00:00',
    dayEndTime: form.watch('plannedEndTime') || form.watch('dayEndsBy') || '00:00',
    slotDuration: form.watch('defaultDuration') || 30,
    scheduleType: form.watch('scheduleType') || 'daily',
  };

  return (
    <ScheduleCustomersManager
      form={form}
      routeCustomers={routeCustomers}
      dropdownsLoading={dropdownsLoading}
      config={initialConfig}
      onScheduleUpdate={handleScheduleUpdate}
      onCustomersAssigned={handleCustomersAssigned}
      title="Schedule & Customer Assignment"
      description="Configure your visit schedule and assign customers to optimal time slots"
      showTimeTemplates={true}
      showHolidayManager={true}
      showWeekdaySelector={true}
      showAutoDistribute={true}
      enableDragDrop={true}
      enableCustomTiming={true}
    />
  );
};