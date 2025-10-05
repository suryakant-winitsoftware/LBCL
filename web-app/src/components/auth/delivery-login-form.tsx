"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import * as z from "zod";
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
import { useDeliveryAuth } from "@/providers/delivery-auth-provider";

const deliveryLoginSchema = z.object({
  loginId: z.string().min(1, "Login ID is required"),
  password: z.string().min(1, "Password is required"),
  rememberMe: z.boolean().default(false),
});

type DeliveryLoginForm = z.infer<typeof deliveryLoginSchema>;

interface DeliveryLoginFormProps {
  onSuccess?: () => void;
}

export function DeliveryLoginForm({ onSuccess }: DeliveryLoginFormProps) {
  const [showPassword, setShowPassword] = useState(false);
  const router = useRouter();
  const { login, error, isLoading } = useDeliveryAuth();

  const form = useForm({
    resolver: zodResolver(deliveryLoginSchema),
    defaultValues: {
      loginId: "",
      password: "",
      rememberMe: false,
    },
  });

  const onSubmit = async (values: DeliveryLoginForm) => {
    try {
      const success = await login(values);

      if (success) {
        toast.success("Login successful!", {
          description: "Welcome to Delivery Management System",
        });

        // Redirect to delivery dashboard
        router.push("/delivery/dashboard");
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
    <Card className="border border-gray-300 bg-white shadow-none rounded-lg">
      <CardHeader className="space-y-3 p-8 pb-6">
        <CardTitle className="text-xl font-bold text-center text-gray-800">
          Delivery Management Sign In
        </CardTitle>
      </CardHeader>
      <CardContent className="px-8 pb-8">
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

            {/* Remember Me */}
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
                  <span>Sign In to Delivery</span>
                </div>
              )}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
