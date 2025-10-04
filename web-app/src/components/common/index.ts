// Common reusable components
export { default as RouteSchedule } from './RouteSchedule';

// Re-export types if needed
export interface RouteScheduleProps {
  form: any;
  className?: string;
}

// Common schedule-related types
export interface SchedulePattern {
  type: 'Daily' | 'Weekly' | 'MultiplePerWeeks' | 'Fortnightly';
  days?: {
    monday: boolean;
    tuesday: boolean;
    wednesday: boolean;
    thursday: boolean;
    friday: boolean;
    saturday: boolean;
    sunday: boolean;
  };
  fortnightlyWeeks?: number;
}