"use client";

import React from "react";
import { MapPin } from "lucide-react";
import StoreHierarchy from "../store-groups/components/StoreHierarchy";

export default function StoreHierarchyPage() {
  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <MapPin className="h-6 w-6" />
          Store Hierarchy
        </h1>
        <p className="text-gray-600 mt-1">
          View and manage store hierarchy structure
        </p>
      </div>

      {/* Store Hierarchy Content */}
      <StoreHierarchy />
    </div>
  );
}