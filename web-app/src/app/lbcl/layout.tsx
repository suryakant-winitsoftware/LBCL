"use client";

import { DeliveryLayout as SharedDeliveryLayout } from "@/app/lbcl/components/delivery-layout";

export default function LBCLLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <SharedDeliveryLayout>{children}</SharedDeliveryLayout>;
}
