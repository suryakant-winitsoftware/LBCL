"use client";

import { useState } from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { LoginForm } from "./login-form";
import { DeliveryLoginForm } from "./delivery-login-form";

export function TabbedLoginForm() {
  const [activeTab, setActiveTab] = useState<"lbcl" | "delivery">("lbcl");

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Image */}
      <div className="hidden lg:flex lg:w-3/5 relative">
        <img
          src="/images/login.png"
          alt="Access Control System"
          className="object-cover w-full h-full"
        />
      </div>

      {/* Right Side - Login Form with Tabs */}
      <div
        className="w-full lg:w-2/5 flex items-center justify-center relative overflow-hidden"
        style={{ backgroundColor: 'rgba(182, 196, 205, 0.15)' }}
      >
        <div className="w-full max-w-lg p-8 relative z-10">
          {/* Multiplex Heading */}
          <div className="text-center mb-10">
            <div className="inline-flex items-center justify-center mb-3">
              <div className="h-[1px] w-12 bg-gradient-to-r from-transparent to-blue-600" />
              <div className="mx-3">
                <h1 className="text-5xl font-light tracking-wider">
                  <span className="text-gray-800">MULTI</span>
                  <span className="font-bold bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-transparent">PLEX</span>
                </h1>
              </div>
              <div className="h-[1px] w-12 bg-gradient-to-l from-transparent to-indigo-600" />
            </div>
            <p className="text-xs uppercase tracking-[0.2em] text-gray-500 font-medium">
              Enterprise Access Control System
            </p>
            <div className="flex items-center justify-center mt-2">
              <div className="flex space-x-1">
                <div className={`h-1 w-8 rounded-full transition-colors ${activeTab === 'lbcl' ? 'bg-blue-600' : 'bg-gray-300'}`} />
                <div className={`h-1 w-8 rounded-full transition-colors ${activeTab === 'delivery' ? 'bg-blue-600' : 'bg-gray-300'}`} />
              </div>
            </div>
          </div>

          {/* Tabbed Login Forms */}
          <Tabs value={activeTab} onValueChange={(value) => setActiveTab(value as "lbcl" | "delivery")} className="w-full">
            <TabsList className="grid w-full grid-cols-2 mb-6 bg-gray-100">
              <TabsTrigger
                value="lbcl"
                className="data-[state=active]:bg-white data-[state=active]:text-blue-600 font-semibold"
              >
                LBCL
              </TabsTrigger>
              <TabsTrigger
                value="delivery"
                className="data-[state=active]:bg-white data-[state=active]:text-blue-600 font-semibold"
              >
                Delivery
              </TabsTrigger>
            </TabsList>

            <TabsContent value="lbcl" className="mt-0">
              <LoginForm variant="card-only" />
            </TabsContent>

            <TabsContent value="delivery" className="mt-0">
              <DeliveryLoginForm />
            </TabsContent>
          </Tabs>

          {/* Footer */}
          <div className="text-center mt-6">
            <p className="text-xs text-gray-500">
              Protected by WINIT Security System
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
