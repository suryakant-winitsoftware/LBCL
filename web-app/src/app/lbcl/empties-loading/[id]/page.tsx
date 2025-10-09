"use client";

import { use } from "react";
import { EmptiesLoadingDetail } from "@/app/lbcl/components/empties-loading-detail";

export default function EmptiesLoadingDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);

  return <EmptiesLoadingDetail deliveryId={id} />;
}
