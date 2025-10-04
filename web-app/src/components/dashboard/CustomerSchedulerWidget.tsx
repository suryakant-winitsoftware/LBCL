import React from 'react';
import CustomerScheduler from '@/components/route/route_shedule';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Calendar, Clock } from 'lucide-react';

interface CustomerSchedulerWidgetProps {
  className?: string;
  title?: string;
}

const CustomerSchedulerWidget: React.FC<CustomerSchedulerWidgetProps> = ({ 
  className = "",
  title = "Customer Schedule Manager" 
}) => {
  return (
    <Card className={className}>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Calendar className="h-5 w-5" />
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        <CustomerScheduler />
      </CardContent>
    </Card>
  );
};

export default CustomerSchedulerWidget;