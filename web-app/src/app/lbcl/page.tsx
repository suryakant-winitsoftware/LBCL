"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function DeliveryRootPage() {
  const router = useRouter();

  useEffect(() => {
    router.push("/delivery/login");
  }, [router]);

  return null;
}
