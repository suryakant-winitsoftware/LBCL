"use client";

import React, { createContext, useContext, useState, useCallback } from "react";

interface DeliveryUser {
  id: string;
  loginId: string;
  name?: string;
  role?: string;
}

interface DeliveryAuthContextType {
  user: DeliveryUser | null;
  isLoading: boolean;
  error: string | null;
  login: (credentials: { loginId: string; password: string; rememberMe?: boolean }) => Promise<boolean>;
  logout: () => void;
}

const DeliveryAuthContext = createContext<DeliveryAuthContextType | undefined>(undefined);

export function DeliveryAuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<DeliveryUser | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const login = useCallback(async (credentials: { loginId: string; password: string; rememberMe?: boolean }) => {
    setIsLoading(true);
    setError(null);

    try {
      // TODO: Replace with actual delivery API endpoint
      const response = await fetch("/api/delivery/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        const data = await response.json();
        setError(data.message || "Login failed");
        return false;
      }

      const data = await response.json();

      // Store delivery user data
      const deliveryUser: DeliveryUser = {
        id: data.userId || data.id,
        loginId: credentials.loginId,
        name: data.name,
        role: data.role,
      };

      setUser(deliveryUser);

      // Store token in localStorage for delivery app
      if (data.token) {
        localStorage.setItem("delivery_token", data.token);
      }

      if (credentials.rememberMe) {
        localStorage.setItem("delivery_user", JSON.stringify(deliveryUser));
      }

      return true;
    } catch (err) {
      setError("Network error. Please try again.");
      return false;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const logout = useCallback(() => {
    setUser(null);
    localStorage.removeItem("delivery_token");
    localStorage.removeItem("delivery_user");
  }, []);

  return (
    <DeliveryAuthContext.Provider
      value={{
        user,
        isLoading,
        error,
        login,
        logout,
      }}
    >
      {children}
    </DeliveryAuthContext.Provider>
  );
}

export function useDeliveryAuth() {
  const context = useContext(DeliveryAuthContext);
  if (context === undefined) {
    throw new Error("useDeliveryAuth must be used within a DeliveryAuthProvider");
  }
  return context;
}
