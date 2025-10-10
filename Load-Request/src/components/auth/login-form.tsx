"use client";

import { useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import * as z from "zod";
import Image from "next/image";
import { Eye, EyeOff, User, AlertCircle, LogIn } from "lucide-react";
import { SkeletonLoader } from "@/components/ui/loader";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { toast } from "sonner";
import { useAuth } from "@/providers/auth-provider";

const loginSchema = z.object({
  loginId: z.string().min(1, "Login ID is required"),
  password: z.string().min(1, "Password is required"),
  rememberMe: z.boolean().default(false),
});

type LoginForm = z.infer<typeof loginSchema>;

interface LoginFormProps {
  onSuccess?: () => void;
}

export function LoginForm({ onSuccess }: LoginFormProps) {
  const [showPassword, setShowPassword] = useState(false);
  const router = useRouter();
  const searchParams = useSearchParams();
  const { login, error, isLoading } = useAuth();

  const reason = searchParams.get("reason");
  const from = searchParams.get("from");

  const form = useForm({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      loginId: "",
      password: "",
      rememberMe: false,
    },
  });

  const onSubmit = async (values: LoginForm) => {
    try {
      const success = await login(values);

      if (success) {
        toast.success("Login successful!", {
          description: "Welcome back to WINIT Access Control System",
        });

        // Redirect to intended page or dashboard
        router.push(from || "/dashboard");
        onSuccess?.();
      } else {
        toast.error("Login failed", {
          description: error || "Invalid credentials. Please try again.",
        });
      }
    } catch {
      toast.error("Login failed", {
        description: "Authentication service unavailable. Please try again.",
      });
    }
  };

  return (
    <div className="min-h-screen flex">
      {/* Left Side - Image */}
      <div className="hidden lg:flex lg:w-3/5 relative">
        <Image
          src="/images/login.png"
          alt="WINIT Access Control System"
          fill
          className="object-cover"
          priority
        />
      </div>

      {/* Right Side - Login Form */}
      <div className="w-full lg:w-2/5 flex items-center justify-center relative overflow-hidden" style={{ backgroundColor: 'rgba(182, 196, 205, 0.15)' }}
        // style={{
        //   backgroundColor: '#b6c4cd',
        //   backgroundImage: `
        //     repeating-linear-gradient(
        //       0deg,
        //       transparent,
        //       transparent 19px,
        //       #d9e3e5 19px,
        //       #d9e3e5 20px
        //     ),
        //     repeating-linear-gradient(
        //       90deg,
        //       transparent,
        //       transparent 19px,
        //       #d9e3e5 19px,
        //       #d9e3e5 20px
        //     )
        //   `,
        //   backgroundSize: '20px 20px'
        // }}
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
                <div className="h-1 w-8 bg-blue-600 rounded-full" />
                <div className="h-1 w-1 bg-gray-300 rounded-full" />
                <div className="h-1 w-1 bg-gray-300 rounded-full" />
                <div className="h-1 w-1 bg-gray-300 rounded-full" />
              </div>
            </div>
          </div>
          
          {/* Login Form */}
          <Card className="border border-gray-300 bg-white shadow-none rounded-lg">
            <CardHeader className="space-y-3 p-8 pb-6">
              <CardTitle className="text-xl font-bold text-center text-gray-800">
                Sign In
              </CardTitle>
            </CardHeader>
            <CardContent className="px-8 pb-8">
              {/* Show alert if redirected due to expired session or unauthorized access */}
              {reason && (
                <Alert
                  className="mb-4"
                  variant={reason === "expired" ? "default" : "destructive"}
                >
                  <AlertCircle className="h-4 w-4" />
                  <AlertDescription>
                    {reason === "expired"
                      ? "Your session has expired. Please sign in again to continue."
                      : reason === "unauthorized"
                      ? "You are not authorized to access that resource. Please sign in with appropriate credentials."
                      : "Please sign in to continue."}
                  </AlertDescription>
                </Alert>
              )}

              <Form {...form}>
                <form
                  onSubmit={form.handleSubmit(onSubmit)}
                  className="space-y-8"
                >
                  {/* Login ID Field */}
                  <FormField
                    control={form.control}
                    name="loginId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-sm font-semibold text-gray-700">
                          Login ID
                        </FormLabel>
                        <FormControl>
                          <div className="relative">
                            <User className="absolute left-3 top-4 h-5 w-5 text-gray-400" />
                            <Input
                              {...field}
                              type="text"
                              placeholder="Enter your login ID"
                              className="pl-10 h-12 bg-white border border-gray-300 rounded-md focus:border-[#4a5568] focus:ring-2 focus:ring-[#4a5568]/20 transition-all duration-200 text-gray-800 font-medium"
                              disabled={isLoading}
                            />
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Password Field */}
                  <FormField
                    control={form.control}
                    name="password"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel className="text-sm font-semibold text-gray-700">
                          Password
                        </FormLabel>
                        <FormControl>
                          <div className="relative">
                            <Input
                              {...field}
                              type={showPassword ? "text" : "password"}
                              placeholder="Enter your password"
                              className="pr-10 h-12 bg-white border border-gray-300 rounded-md focus:border-[#4a5568] focus:ring-2 focus:ring-[#4a5568]/20 transition-all duration-200 text-gray-800 font-medium"
                              disabled={isLoading}
                            />
                            <button
                              type="button"
                              className="absolute right-3 top-4 h-5 w-5 text-gray-400 hover:text-gray-600 transition-colors"
                              onClick={() => setShowPassword(!showPassword)}
                              disabled={isLoading}
                            >
                              {showPassword ? <EyeOff /> : <Eye />}
                            </button>
                          </div>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  {/* Remember Me & Forgot Password */}
                  <div className="flex items-center justify-between">
                    <FormField
                      control={form.control}
                      name="rememberMe"
                      render={({ field }) => (
                        <FormItem className="flex flex-row items-start space-x-3 space-y-0">
                          <FormControl>
                            <Checkbox
                              checked={field.value}
                              onCheckedChange={field.onChange}
                              disabled={isLoading}
                            />
                          </FormControl>
                          <div className="space-y-1 leading-none">
                            <FormLabel className="text-sm text-gray-600">
                              Remember me
                            </FormLabel>
                          </div>
                        </FormItem>
                      )}
                    />
                    <Button
                      variant="link"
                      className="px-0 font-medium text-sm text-[#4a5568] hover:text-[#2d3748] hover:underline"
                      type="button"
                      disabled={isLoading}
                      onClick={() => router.push("/auth/forgot-password")}
                    >
                      Forgot password?
                    </Button>
                  </div>

                  {/* Submit Button */}
                  <Button
                    type="submit"
                    className="w-full h-10 bg-blue-600 hover:bg-blue-700 text-white font-medium text-sm rounded-md transition-colors duration-200"
                    disabled={isLoading}
                  >
                    {isLoading ? (
                      <div className="flex items-center justify-center">
                        <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                        <span>Signing in...</span>
                      </div>
                    ) : (
                      <div className="flex items-center justify-center">
                        <LogIn className="h-4 w-4 mr-2" />
                        <span>Sign In</span>
                      </div>
                    )}
                  </Button>
                </form>
              </Form>
            </CardContent>
          </Card>

          {/* Footer */}
          <div className="text-center">
            <p className="text-xs text-gray-500">
              Protected by WINIT Security System
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
