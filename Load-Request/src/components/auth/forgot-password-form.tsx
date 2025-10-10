"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { zodResolver } from "@hookform/resolvers/zod"
import { useForm } from "react-hook-form"
import * as z from "zod"
import Image from "next/image"
import { ArrowLeft, Mail, Send } from "lucide-react"
import { SkeletonLoader } from "@/components/ui/loader"

import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form"
import { Input } from "@/components/ui/input"
import { toast } from "sonner"

const forgotPasswordSchema = z.object({
  loginId: z.string().min(1, "Login ID is required"),
  email: z.string().email("Please enter a valid email address"),
})

type ForgotPasswordForm = z.infer<typeof forgotPasswordSchema>

export function ForgotPasswordForm() {
  const [isLoading, setIsLoading] = useState(false)
  const [emailSent, setEmailSent] = useState(false)
  const router = useRouter()

  const form = useForm<ForgotPasswordForm>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      loginId: "",
      email: "",
    },
  })

  const onSubmit = async (_values: ForgotPasswordForm) => {
    setIsLoading(true)
    
    try {
      // Simulate API call
      await new Promise(resolve => setTimeout(resolve, 2000))
      
      setEmailSent(true)
      
      toast.success("Reset link sent!", {
        description: "Check your email for password reset instructions.",
      })
      
    } catch {
      toast.error("Reset failed", {
        description: "Unable to send reset email. Please try again.",
      })
    } finally {
      setIsLoading(false)
    }
  }

  if (emailSent) {
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

        {/* Right Side - Success Message */}
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
          <div className="w-full max-w-md space-y-8 p-8 relative z-10">
            {/* Mobile Logo and Brand (only shown on small screens) */}
            <div className="text-center lg:hidden">
              <div className="mx-auto h-16 w-16 bg-gradient-to-br from-green-600 to-emerald-600 rounded-2xl flex items-center justify-center shadow-lg">
                <Mail className="h-8 w-8 text-white" />
              </div>
              <h1 className="mt-6 text-3xl font-bold tracking-tight text-gray-900">
                Check your email
              </h1>
              <p className="mt-2 text-sm text-gray-600">
                We&apos;ve sent password reset instructions to your email address.
              </p>
            </div>

            {/* Desktop Header */}
            <div className="hidden lg:block text-center">
              <h1 className="text-3xl font-bold tracking-tight text-gray-900">
                Check your email
              </h1>
              <p className="mt-2 text-sm text-gray-600">
                Reset instructions sent successfully
              </p>
            </div>

            <Card className="border border-gray-300 bg-white shadow-none rounded-lg">
              <CardContent className="pt-6">
                <div className="text-center space-y-4">
                  <p className="text-sm text-gray-600">
                    Didn&apos;t receive the email? Check your spam folder or request a new one.
                  </p>
                  <div className="space-y-2">
                    <Button
                      variant="outline"
                      className="w-full"
                      onClick={() => setEmailSent(false)}
                    >
                      Try again
                    </Button>
                    <Button
                      variant="link"
                      className="w-full"
                      onClick={() => router.push("/login")}
                    >
                      <ArrowLeft className="mr-2 h-4 w-4" />
                      Back to login
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    )
  }

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
        {/* Border with shadow */}
        <div className="absolute right-0 top-0 bottom-0 w-[1px] bg-gray-300 shadow-[0_0_10px_rgba(0,0,0,0.15)]" />
      </div>

      {/* Right Side - Reset Form */}
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
          
          {/* Reset Form */}
          <Card className="border border-gray-300 bg-white shadow-none rounded-lg">
            <CardHeader className="space-y-3 p-8 pb-6">
              <CardTitle className="text-xl font-bold text-center text-gray-800">
                Password Reset
              </CardTitle>
            </CardHeader>
          <CardContent className="px-8 pb-8">
            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
                {/* Login ID Field */}
                <FormField
                  control={form.control}
                  name="loginId"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-sm font-semibold text-gray-700">Login ID</FormLabel>
                      <FormControl>
                        <Input
                          {...field}
                          type="text"
                          placeholder="Enter your login ID"
                          className="h-12 bg-white border border-gray-300 rounded-md focus:border-[#4a5568] focus:ring-2 focus:ring-[#4a5568]/20 transition-all duration-200 text-gray-800 font-medium"
                          disabled={isLoading}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Email Field */}
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel className="text-sm font-semibold text-gray-700">Email Address</FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Mail className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                          <Input
                            {...field}
                            type="email"
                            placeholder="Enter your email address"
                            className="pl-10 h-12 bg-white border border-gray-300 rounded-md focus:border-[#4a5568] focus:ring-2 focus:ring-[#4a5568]/20 transition-all duration-200 text-gray-800 font-medium"
                            disabled={isLoading}
                          />
                        </div>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Submit Button */}
                <Button
                  type="submit"
                  className="w-full h-10 bg-blue-600 hover:bg-blue-700 text-white font-medium text-sm rounded-md transition-colors duration-200"
                  disabled={isLoading}
                >
                  {isLoading ? (
                    <div className="flex items-center justify-center">
                      <SkeletonLoader className="h-4 w-4 rounded mr-2" />
                      <span>Sending...</span>
                    </div>
                  ) : (
                    <div className="flex items-center justify-center">
                      <Send className="h-4 w-4 mr-2" />
                      <span>Send Reset Link</span>
                    </div>
                  )}
                </Button>

                {/* Back to Login */}
                <Button
                  type="button"
                  variant="link"
                  className="w-full"
                  onClick={() => router.push("/login")}
                  disabled={isLoading}
                >
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Back to login
                </Button>
              </form>
            </Form>
          </CardContent>
        </Card>
        </div>
      </div>
    </div>
  )
}