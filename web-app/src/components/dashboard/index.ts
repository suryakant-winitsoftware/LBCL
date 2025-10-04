// Dashboard components exports
export { default as CustomerSchedulerWidget } from './CustomerSchedulerWidget';
export { default as RouteScheduleWidget } from './RouteScheduleWidget';

// Component types
export interface DashboardWidgetProps {
  className?: string;
  title?: string;
}