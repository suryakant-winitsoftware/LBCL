"use client";

import { use } from "react";
import { ActivityLogPage } from "@/app/lbcl/components/activity-log-page";

interface PageProps {
  params: Promise<{
    id: string;
  }>;
}

export default function ActivityLog({ params }: PageProps) {
  const { id } = use(params);
  return <ActivityLogPage deliveryPlanId={id} />;
}
