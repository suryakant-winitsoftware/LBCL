"use client";

import type React from "react";

import { Bell, User } from "lucide-react";
import Image from "next/image";

interface PageHeaderProps {
  title: string;
  showBell?: boolean;
  showProfile?: boolean;
  rightContent?: React.ReactNode;
}

export function PageHeader({
  title,
  showBell = true,
  showProfile = true,
  rightContent
}: PageHeaderProps) {
  return (
    <header className="bg-white border-b border-gray-200 px-4 py-4 sticky top-0 z-50 shadow-sm">
      <div className="flex items-center justify-between gap-4 max-w-full">
        {/* Left: Logo */}
        <div className="flex items-center gap-4 flex-shrink-0">
          <div className="relative w-16 h-8 sm:w-20 sm:h-10 md:w-24 md:h-12">
            <Image
              src="/images/lion-logo.png"
              alt="LION Logo"
              fill
              className="object-contain"
              priority
            />
          </div>
        </div>

        {/* Center: Page Title */}
        <h1 className="text-base sm:text-lg md:text-xl lg:text-2xl font-bold text-gray-900 flex-1 px-2 truncate">
          {title}
        </h1>

        {/* Right: Icons or Custom Content */}
        <div className="flex items-center gap-2 flex-shrink-0">
          {rightContent ? (
            rightContent
          ) : (
            <>
              {showBell && (
                <button
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                  aria-label="Notifications"
                >
                  <Bell className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
                </button>
              )}
              {showProfile && (
                <button
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                  aria-label="Profile"
                >
                  <User className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
                </button>
              )}
            </>
          )}
        </div>
      </div>
    </header>
  );
}
