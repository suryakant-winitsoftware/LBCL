import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Switch } from '@/components/ui/switch';
import { CalendarDays, CalendarClock, CalendarRange, Calendar } from 'lucide-react';

interface RouteScheduleProps {
  form: UseFormReturn<any>;
  className?: string;
}

const RouteSchedule: React.FC<RouteScheduleProps> = ({ form, className }) => {
  const { control, watch, setValue } = form;
  const scheduleType = watch('scheduleType');
  const dailyWeeklyDays = watch('dailyWeeklyDays');

  const scheduleOptions = [
    {
      value: 'Daily',
      label: 'Daily',
      description: 'Visit customers on selected days every week',
      icon: CalendarDays,
    },
    {
      value: 'Weekly',
      label: 'Weekly',
      description: 'Visit customers once per week on a specific day',
      icon: Calendar,
    },
    {
      value: 'MultiplePerWeeks',
      label: 'Multiple per Week',
      description: 'Visit customers multiple times per week',
      icon: CalendarRange,
    },
    {
      value: 'Fortnightly',
      label: 'Fortnightly',
      description: 'Visit customers every 2 weeks',
      icon: CalendarClock,
    },
  ];

  const dayLabels = [
    { key: 'monday', label: 'Monday', short: 'Mon' },
    { key: 'tuesday', label: 'Tuesday', short: 'Tue' },
    { key: 'wednesday', label: 'Wednesday', short: 'Wed' },
    { key: 'thursday', label: 'Thursday', short: 'Thu' },
    { key: 'friday', label: 'Friday', short: 'Fri' },
    { key: 'saturday', label: 'Saturday', short: 'Sat' },
    { key: 'sunday', label: 'Sunday', short: 'Sun' },
  ];

  const handleDayToggle = (dayKey: string, checked: boolean) => {
    setValue(`dailyWeeklyDays.${dayKey}`, checked);
  };

  const handleScheduleTypeChange = (value: string) => {
    setValue('scheduleType', value);
    
    // Reset day selections when changing schedule type
    if (value === 'Weekly') {
      // For weekly, allow only one day selection
      const currentDays = dailyWeeklyDays || {};
      const selectedDays = Object.entries(currentDays).filter(([_, selected]) => selected);
      
      if (selectedDays.length > 1) {
        // Reset all days and select Monday as default
        dayLabels.forEach(day => {
          setValue(`dailyWeeklyDays.${day.key}`, day.key === 'monday');
        });
      }
    }
  };

  const shouldShowDaySelection = () => {
    return ['Daily', 'Weekly', 'MultiplePerWeeks'].includes(scheduleType);
  };

  const getSelectedDaysCount = () => {
    if (!dailyWeeklyDays) return 0;
    return Object.values(dailyWeeklyDays).filter(Boolean).length;
  };

  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <CalendarDays className="h-5 w-5" />
          Route Schedule Configuration
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Schedule Type Selection */}
        <div className="space-y-3">
          <Label className="text-base font-semibold">
            Schedule Pattern
          </Label>
          <RadioGroup
            value={scheduleType}
            onValueChange={handleScheduleTypeChange}
            className="grid grid-cols-1 md:grid-cols-2 gap-4"
          >
            {scheduleOptions.map((option) => {
              const IconComponent = option.icon;
              return (
                <div key={option.value} className="flex items-start space-x-3">
                  <RadioGroupItem
                    value={option.value}
                    id={option.value}
                    className="mt-1"
                  />
                  <div className="flex-1">
                    <Label
                      htmlFor={option.value}
                      className="flex items-center gap-2 cursor-pointer"
                    >
                      <IconComponent className="h-4 w-4" />
                      <span className="font-medium">{option.label}</span>
                    </Label>
                    <p className="text-sm text-muted-foreground mt-1">
                      {option.description}
                    </p>
                  </div>
                </div>
              );
            })}
          </RadioGroup>
        </div>

        {/* Day Selection for applicable schedule types */}
        {shouldShowDaySelection() && (
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <Label className="text-base font-semibold">
                Select Days
                {scheduleType === 'Weekly' && (
                  <span className="text-sm font-normal text-muted-foreground ml-2">
                    (Select exactly one day)
                  </span>
                )}
              </Label>
              <div className="text-sm text-muted-foreground">
                {getSelectedDaysCount()} day{getSelectedDaysCount() !== 1 ? 's' : ''} selected
              </div>
            </div>

            <div className="grid grid-cols-7 gap-2">
              {dayLabels.map((day) => (
                <div
                  key={day.key}
                  className="flex flex-col items-center space-y-2"
                >
                  <Label
                    htmlFor={`day-${day.key}`}
                    className="text-xs font-medium text-center"
                  >
                    {day.short}
                  </Label>
                  <Switch
                    id={`day-${day.key}`}
                    checked={dailyWeeklyDays?.[day.key] || false}
                    onCheckedChange={(checked) => {
                      if (scheduleType === 'Weekly' && checked) {
                        // For weekly, only allow one day to be selected
                        dayLabels.forEach(otherDay => {
                          setValue(`dailyWeeklyDays.${otherDay.key}`, otherDay.key === day.key);
                        });
                      } else {
                        handleDayToggle(day.key, checked);
                      }
                    }}
                    disabled={
                      scheduleType === 'Weekly' && 
                      getSelectedDaysCount() >= 1 && 
                      !(dailyWeeklyDays?.[day.key])
                    }
                  />
                </div>
              ))}
            </div>

            {/* Validation messages */}
            {scheduleType === 'Weekly' && getSelectedDaysCount() !== 1 && (
              <p className="text-sm text-amber-600">
                Please select exactly one day for weekly schedule
              </p>
            )}
            {(scheduleType === 'Daily' || scheduleType === 'MultiplePerWeeks') && getSelectedDaysCount() === 0 && (
              <p className="text-sm text-red-600">
                Please select at least one day
              </p>
            )}
          </div>
        )}

        {/* Fortnightly specific info */}
        {scheduleType === 'Fortnightly' && (
          <div className="p-4 bg-blue-50 rounded-lg border border-blue-200">
            <div className="flex items-center gap-2">
              <CalendarClock className="h-4 w-4 text-blue-600" />
              <p className="text-sm text-blue-800">
                Routes will be executed every 2 weeks based on the pattern configuration.
              </p>
            </div>
          </div>
        )}

        {/* Summary */}
        <div className="p-4 bg-gray-50 rounded-lg">
          <h4 className="font-medium mb-2">Schedule Summary</h4>
          <div className="text-sm text-muted-foreground">
            {scheduleType === 'Daily' && getSelectedDaysCount() > 0 && (
              <p>
                Route will run daily on: {' '}
                {dayLabels
                  .filter(day => dailyWeeklyDays?.[day.key])
                  .map(day => day.label)
                  .join(', ')
                }
              </p>
            )}
            {scheduleType === 'Weekly' && getSelectedDaysCount() === 1 && (
              <p>
                Route will run weekly on: {' '}
                {dayLabels
                  .filter(day => dailyWeeklyDays?.[day.key])
                  .map(day => day.label)
                  .join('')
                }
              </p>
            )}
            {scheduleType === 'MultiplePerWeeks' && getSelectedDaysCount() > 0 && (
              <p>
                Route will run multiple times per week on: {' '}
                {dayLabels
                  .filter(day => dailyWeeklyDays?.[day.key])
                  .map(day => day.label)
                  .join(', ')
                }
              </p>
            )}
            {scheduleType === 'Fortnightly' && (
              <p>Route will run every 2 weeks</p>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default RouteSchedule;