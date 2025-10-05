"use client";

import type React from "react";

import { Bell, User } from "lucide-react";
import { NavigationMenu } from "@/app/lbcl/components/navigation-menu";

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
        {/* Left: Hamburger Menu */}
        <div className="flex-shrink-0">
          <NavigationMenu />
        </div>

        {/* Center: Page Title */}
        <h1 className="text-base sm:text-lg md:text-xl lg:text-2xl font-bold text-gray-900 text-center flex-1 px-2 truncate">
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
