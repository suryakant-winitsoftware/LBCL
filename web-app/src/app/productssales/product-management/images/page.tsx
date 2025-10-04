"use client";

import React from "react";
import { ImageIcon } from "lucide-react";
import SKUImageManager from "@/components/sku/SKUImageManager";

export default function ProductImageManagementPage() {
  return (
    <div className="container mx-auto p-6">
      {/* Header */}
      <div className="mb-6">
        <h1 className="text-2xl font-bold flex items-center gap-2">
          <ImageIcon className="h-6 w-6" />
          Product Image Management
        </h1>
        <p className="text-gray-600 mt-1">
          Manage product images and visual content
        </p>
      </div>

      {/* Image Management Content */}
      <SKUImageManager mode="manage" />
    </div>
  );
}