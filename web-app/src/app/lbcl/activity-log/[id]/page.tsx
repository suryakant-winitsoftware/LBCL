"use client";

import { useState, useEffect } from "react";
import { use } from "react";
import { ActivityLogPage } from "@/app/lbcl/components/activity-log-page";
import { deliveryLoadingService } from "@/services/deliveryLoadingService";

interface PageProps {
  params: Promise<{
    id: string;
  }>;
}

export default function ActivityLog({ params }: PageProps) {
  const { id } = use(params);
  const [isReadOnly, setIsReadOnly] = useState(false);

  useEffect(() => {
    const checkStatus = async () => {
      try {
        const delivery = await deliveryLoadingService.getByWHStockRequestUID(
          id
        );
        if (delivery && delivery.status === "RECEIVED") {
          setIsReadOnly(true);
        }
      } catch (error) {
        console.error("Error checking delivery status:", error);
      }
    };

    checkStatus();
  }, [id]);

  return <ActivityLogPage deliveryPlanId={id} readOnly={isReadOnly} />;
}
