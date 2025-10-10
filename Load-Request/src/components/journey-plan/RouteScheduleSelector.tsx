import React, { useEffect, useState } from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Calendar, Clock, Route, CalendarDays, Info } from 'lucide-react';
import { api } from '@/services/api';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import moment from 'moment';

interface RouteScheduleSelectorProps {
  routeUID: string;
  onScheduleSelect?: (schedule: any | null) => void;
  onGenerateFromSchedule?: (schedule: any) => void;
  showGenerateButton?: boolean;
}

export const RouteScheduleSelector: React.FC<RouteScheduleSelectorProps> = ({
  routeUID,
  onScheduleSelect,
  onGenerateFromSchedule,
  showGenerateButton = true,
}) => {
  const [selectedSchedule, setSelectedSchedule] = useState<any | null>(null);
  const [daywise, setDaywise] = useState<any | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (routeUID) {
      loadRouteSchedule();
    }
  }, [routeUID]);

  const loadRouteSchedule = async () => {
    setLoading(true);
    try {
      // Get RouteSchedule from Route Master data (if exists)
      const masterData = await api.route.getMasterByUID(routeUID);
      if (masterData?.IsSuccess && masterData?.Data) {
        const routeMaster = masterData.Data;
        console.log("Route master data loaded for schedule selector:", routeMaster);
        
        // Extract RouteSchedule if available
        if (routeMaster.RouteSchedule) {
          console.log("RouteSchedule found:", routeMaster.RouteSchedule);
          setSelectedSchedule(routeMaster.RouteSchedule);
          onScheduleSelect?.(routeMaster.RouteSchedule);
          
          // Also get daywise config if available
          if (routeMaster.RouteScheduleDaywise) {
            setDaywise(routeMaster.RouteScheduleDaywise);
          }
        } else {
          console.log("No RouteSchedule found for this route - this is normal for manual journey planning");
          setSelectedSchedule(null);
          onScheduleSelect?.(null);
        }
      } else {
        console.log("Could not load route master data");
        setSelectedSchedule(null);
        onScheduleSelect?.(null);
      }
    } catch (error) {
      console.error('Error loading route schedule:', error);
      // Don't treat this as an error - routes often don't have schedules
      setSelectedSchedule(null);
      onScheduleSelect?.(null);
    } finally {
      setLoading(false);
    }
  };

  const handleScheduleSelect = (schedule: any | null) => {
    setSelectedSchedule(schedule);
    onScheduleSelect?.(schedule);
  };

  const getScheduleStatusBadge = (schedule: any) => {
    if (schedule.Status === 0) {
      return <Badge variant="destructive">Inactive</Badge>;
    }
    
    const today = moment();
    const fromDate = schedule.FromDate ? moment(schedule.FromDate) : null;
    const toDate = schedule.ToDate ? moment(schedule.ToDate) : null;
    
    if (fromDate && fromDate.isAfter(today)) {
      return <Badge variant="secondary">Upcoming</Badge>;
    }
    
    if (toDate && toDate.isBefore(today)) {
      return <Badge variant="outline">Expired</Badge>;
    }
    
    return <Badge variant="default">Active</Badge>;
  };

  const getDaysDisplay = () => {
    if (!daywise) return 'All days';
    
    const activeDays: string[] = [];
    if (daywise.Monday === 1) activeDays.push('Mon');
    if (daywise.Tuesday === 1) activeDays.push('Tue');
    if (daywise.Wednesday === 1) activeDays.push('Wed');
    if (daywise.Thursday === 1) activeDays.push('Thu');
    if (daywise.Friday === 1) activeDays.push('Fri');
    if (daywise.Saturday === 1) activeDays.push('Sat');
    if (daywise.Sunday === 1) activeDays.push('Sun');
    
    if (activeDays.length === 0) return 'No days configured';
    if (activeDays.length === 7) return 'All days';
    
    return activeDays.join(', ');
  };

  if (loading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-10 w-full" />
        </CardContent>
      </Card>
    );
  }

  if (!loading && !selectedSchedule) {
    return (
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <CalendarDays className="h-5 w-5" />
            Route Schedule
          </CardTitle>
          <CardDescription>
            No automatic schedule configured for this route. You can still create journey plans manually or use bulk generation.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {showGenerateButton && onGenerateFromSchedule && (
            <Button
              onClick={() => onGenerateFromSchedule(null)}
              className="w-full"
              variant="outline"
            >
              <CalendarDays className="mr-2 h-4 w-4" />
              Generate Journey Plans (Manual Pattern)
            </Button>
          )}
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <CalendarDays className="h-5 w-5" />
          Route Schedule
        </CardTitle>
        <CardDescription>
          Select a schedule pattern to auto-generate journey plans
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <label className="text-sm font-medium">Schedule Pattern</label>
          <div className="p-3 bg-muted/30 rounded-lg">
            <div className="flex items-center justify-between">
              <span className="font-medium">{selectedSchedule.Name || 'Route Schedule'}</span>
              <div className="flex items-center gap-2">
                <Badge variant="outline" className="text-xs">
                  {selectedSchedule.Type || 'Manual'}
                </Badge>
                {getScheduleStatusBadge(selectedSchedule)}
              </div>
            </div>
          </div>
        </div>

        {selectedSchedule && (
          <div className="space-y-3 p-3 bg-muted/50 rounded-lg">
            <div className="flex items-center justify-between">
              <span className="text-sm font-medium">Schedule Details</span>
              <TooltipProvider>
                <Tooltip>
                  <TooltipTrigger>
                    <Info className="h-4 w-4 text-muted-foreground" />
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>This schedule defines when journey plans should be created</p>
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>
            </div>

            <div className="grid grid-cols-2 gap-3 text-sm">
              <div className="flex items-center gap-2">
                <Calendar className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Type:</span>
                <span className="font-medium">
                  {selectedSchedule.Type || 'Manual'}
                </span>
              </div>

              <div className="flex items-center gap-2">
                <Clock className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Time:</span>
                <span className="font-medium">
                  {selectedSchedule.StartTime || '00:00'} - {selectedSchedule.EndTime || '00:00'}
                </span>
              </div>

              {selectedSchedule.FromDate && (
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">From:</span>
                  <span className="font-medium">
                    {moment(selectedSchedule.FromDate).format('DD MMM YYYY')}
                  </span>
                </div>
              )}

              {selectedSchedule.ToDate && (
                <div className="flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">To:</span>
                  <span className="font-medium">
                    {moment(selectedSchedule.ToDate).format('DD MMM YYYY')}
                  </span>
                </div>
              )}

              <div className="flex items-center gap-2">
                <Route className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Visit Duration:</span>
                <span className="font-medium">{selectedSchedule.VisitDurationInMinutes} min</span>
              </div>

              <div className="flex items-center gap-2">
                <Route className="h-4 w-4 text-muted-foreground" />
                <span className="text-muted-foreground">Travel Time:</span>
                <span className="font-medium">{selectedSchedule.TravelTimeInMinutes} min</span>
              </div>

              {daywise && (
                <div className="col-span-2 flex items-center gap-2">
                  <CalendarDays className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Active Days:</span>
                  <span className="font-medium">{getDaysDisplay()}</span>
                </div>
              )}

              {selectedSchedule.LastBeatDate && (
                <div className="col-span-2 flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Last Generated:</span>
                  <span className="font-medium">
                    {moment(selectedSchedule.LastBeatDate).format('DD MMM YYYY')}
                  </span>
                </div>
              )}

              {selectedSchedule.NextBeatDate && (
                <div className="col-span-2 flex items-center gap-2">
                  <Calendar className="h-4 w-4 text-muted-foreground" />
                  <span className="text-muted-foreground">Next Due:</span>
                  <span className="font-medium">
                    {moment(selectedSchedule.NextBeatDate).format('DD MMM YYYY')}
                  </span>
                </div>
              )}
            </div>

            {showGenerateButton && onGenerateFromSchedule && (
              <Button
                onClick={() => onGenerateFromSchedule(selectedSchedule)}
                className="w-full"
                variant="outline"
              >
                <CalendarDays className="mr-2 h-4 w-4" />
                Generate Journey Plans from Schedule
              </Button>
            )}
          </div>
        )}
      </CardContent>
    </Card>
  );
};