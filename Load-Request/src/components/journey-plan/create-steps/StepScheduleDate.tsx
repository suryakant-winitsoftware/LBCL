import React from 'react';
import { UseFormReturn } from 'react-hook-form';
import { motion } from 'framer-motion';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import {
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form';
import { HolidayCalendar } from '@/components/journey-plan/HolidayCalendar';
import { RouteScheduleSelector } from '@/components/journey-plan/RouteScheduleSelector';

interface StepScheduleDateProps {
  form: UseFormReturn<any>;
  selectedSchedule: any;
  setSelectedSchedule: (schedule: any) => void;
  setShowBulkGenerator: (show: boolean) => void;
}

export const StepScheduleDate: React.FC<StepScheduleDateProps> = ({
  form,
  selectedSchedule,
  setSelectedSchedule,
  setShowBulkGenerator,
}) => {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      className="space-y-6"
    >
      <Card>
        <CardHeader>
          <CardTitle>Schedule & Date</CardTitle>
          <CardDescription>
            Select visit date and configure schedule settings
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Visit Date with Holiday Calendar */}
          <FormField
            control={form.control}
            name="visitDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Visit Date *</FormLabel>
                <HolidayCalendar
                  orgUID={form.watch("orgUID") || ""}
                  selectedDate={field.value}
                  onDateSelect={(date) => field.onChange(date)}
                  showHolidayList={true}
                />
                <FormMessage />
              </FormItem>
            )}
          />

          {/* Route Schedule Selector */}
          {form.watch("routeUID") && (
            <RouteScheduleSelector
              routeUID={form.watch("routeUID")}
              onScheduleSelect={(schedule) => {
                setSelectedSchedule(schedule);
                if (schedule) {
                  form.setValue("routeScheduleUID", schedule.UID);
                }
              }}
              onGenerateFromSchedule={(schedule) => {
                setSelectedSchedule(schedule);
                setShowBulkGenerator(true);
              }}
              showGenerateButton={true}
            />
          )}

          {/* Time Configuration */}
          <div className="grid grid-cols-2 gap-4">
            <FormField
              control={form.control}
              name="dayStartsAt"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Day Starts At *</FormLabel>
                  <FormControl>
                    <Input type="time" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="dayEndsBy"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Day Ends By *</FormLabel>
                  <FormControl>
                    <Input type="time" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="defaultDuration"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Default Visit Duration (min) *</FormLabel>
                  <FormControl>
                    <Input 
                      type="number" 
                      min="5"
                      max="480"
                      {...field} 
                      onChange={e => field.onChange(parseInt(e.target.value) || 30)}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="defaultTravelTime"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Default Travel Time (min) *</FormLabel>
                  <FormControl>
                    <Input 
                      type="number" 
                      min="0"
                      max="240"
                      {...field}
                      onChange={e => field.onChange(parseInt(e.target.value) || 30)}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>

          {/* Multi-day Journey Plan Option */}
          <FormField
            control={form.control}
            name="endDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>End Date (Optional - For Multi-Day Plans)</FormLabel>
                <FormControl>
                  <Input 
                    type="date" 
                    {...field}
                    value={field.value ? new Date(field.value).toISOString().split('T')[0] : ''}
                    onChange={e => field.onChange(e.target.value ? new Date(e.target.value) : undefined)}
                    min={form.watch("visitDate") ? new Date(form.watch("visitDate")).toISOString().split('T')[0] : undefined}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </CardContent>
      </Card>
    </motion.div>
  );
};