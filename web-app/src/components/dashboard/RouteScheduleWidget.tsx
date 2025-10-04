import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Form } from '@/components/ui/form';
import { RouteSchedule } from '@/components/common';

// Schema for dashboard schedule widget
const scheduleWidgetSchema = z.object({
  scheduleType: z.enum(['Daily', 'Weekly', 'MultiplePerWeeks', 'Fortnightly']).default('Daily'),
  dailyWeeklyDays: z.object({
    monday: z.boolean().default(true),
    tuesday: z.boolean().default(false),
    wednesday: z.boolean().default(false),
    thursday: z.boolean().default(false),
    friday: z.boolean().default(false),
    saturday: z.boolean().default(false),
    sunday: z.boolean().default(false),
  }).default({
    monday: true,
    tuesday: false,
    wednesday: false,
    thursday: false,
    friday: false,
    saturday: false,
    sunday: false,
  }),
});

type ScheduleWidgetData = z.infer<typeof scheduleWidgetSchema>;

interface RouteScheduleWidgetProps {
  onScheduleChange?: (schedule: ScheduleWidgetData) => void;
  initialSchedule?: Partial<ScheduleWidgetData>;
  className?: string;
}

const RouteScheduleWidget: React.FC<RouteScheduleWidgetProps> = ({
  onScheduleChange,
  initialSchedule,
  className
}) => {
  const form = useForm<ScheduleWidgetData>({
    resolver: zodResolver(scheduleWidgetSchema),
    defaultValues: {
      scheduleType: initialSchedule?.scheduleType || 'Daily',
      dailyWeeklyDays: initialSchedule?.dailyWeeklyDays || {
        monday: true,
        tuesday: false,
        wednesday: false,
        thursday: false,
        friday: false,
        saturday: false,
        sunday: false,
      },
    },
  });

  // Watch for changes and notify parent
  React.useEffect(() => {
    const subscription = form.watch((data) => {
      if (onScheduleChange && data) {
        onScheduleChange(data as ScheduleWidgetData);
      }
    });
    return () => subscription.unsubscribe();
  }, [form, onScheduleChange]);

  return (
    <div className={className}>
      <Form {...form}>
        <form>
          <RouteSchedule form={form} />
        </form>
      </Form>
    </div>
  );
};

export default RouteScheduleWidget;