"use client";

import type React from "react";
import { useRouter } from "next/navigation";
import { Bell, User, LogOut } from "lucide-react";
import Image from "next/image";
import { useAuth } from "@/providers/auth-provider";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";

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
  const router = useRouter();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    router.push("/lbcl/login");
  };

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

        {/* Spacer */}
        <div className="flex-1"></div>

        {/* Right: Icons or Custom Content */}
        <div className="flex items-center gap-2 flex-shrink-0">
          {rightContent ? (
            rightContent
          ) : (
            <>
              {showBell && (
                <button
                  className="p-2 hover:bg-gray-100 rounded-lg transition-colors relative"
                  aria-label="Notifications"
                >
                  <Bell className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
                  {/* Optional notification badge */}
                  {/* <span className="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span> */}
                </button>
              )}
              {showProfile && (
                <DropdownMenu>
                  <DropdownMenuTrigger asChild>
                    <button
                      className="p-2 hover:bg-gray-100 rounded-lg transition-colors"
                      aria-label="Profile"
                    >
                      <User className="w-5 h-5 sm:w-6 sm:h-6 text-gray-700" />
                    </button>
                  </DropdownMenuTrigger>
                  <DropdownMenuContent align="end" className="w-64">
                    {/* User Info Section */}
                    <div className="px-4 py-3 border-b">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-full bg-gray-100 flex items-center justify-center">
                          <User className="w-5 h-5 text-gray-600" />
                        </div>
                        <div className="flex-1 min-w-0">
                          <p className="text-sm font-semibold text-gray-900 truncate">
                            {user?.name || user?.loginId || "User"}
                          </p>
                          {user?.email && (
                            <p className="text-xs text-gray-500 truncate">
                              {user.email}
                            </p>
                          )}
                        </div>
                      </div>
                    </div>

                    {/* Organization & Role Info */}
                    <div className="px-4 py-3 border-b">
                      {user?.currentOrganization?.name && (
                        <div className="mb-2">
                          <p className="text-xs font-medium text-gray-500 mb-0.5">Organization</p>
                          <p className="text-sm text-gray-900">
                            {user.currentOrganization.name}
                          </p>
                        </div>
                      )}
                      {user?.roles && user.roles.length > 0 && (
                        <div>
                          <p className="text-xs font-medium text-gray-500 mb-0.5">Role</p>
                          <p className="text-sm text-gray-900">
                            {user.roles.map((r: any) => r.roleNameEn || r.RoleNameEn).join(", ")}
                          </p>
                        </div>
                      )}
                    </div>

                    {/* Logout Button */}
                    <div className="p-2">
                      <button
                        onClick={handleLogout}
                        className="w-full flex items-center gap-2 px-3 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-md transition-colors"
                      >
                        <LogOut className="w-4 h-4" />
                        <span>Log out</span>
                      </button>
                    </div>
                  </DropdownMenuContent>
                </DropdownMenu>
              )}
            </>
          )}
        </div>
      </div>
    </header>
  );
}
