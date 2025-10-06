"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { User, Lock } from "lucide-react"
import { useRouter } from "next/navigation"
import Image from "next/image"

export function LoginPage() {
  const router = useRouter()
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")

  const handleLogin = (e: React.FormEvent) => {
    e.preventDefault()
    router.push("/dashboard")
  }

  return (
    <div className="min-h-screen bg-white flex flex-col">
      {/* Login Form with Logo - Centered */}
      <div className="flex-1 flex items-center justify-center px-4 sm:px-6 lg:px-8">
        <div className="w-full max-w-md">
          {/* Logo */}
          <div className="w-full flex justify-center mb-8 sm:mb-10 md:mb-12">
            <div className="relative w-40 h-20 sm:w-52 sm:h-26 md:w-64 md:h-32">
              <Image
                src="/images/lion-logo.png"
                alt="LION Logo"
                fill
                className="object-contain"
                priority
              />
            </div>
          </div>
          {/* Login Form */}
          <form onSubmit={handleLogin} className="w-full space-y-4 sm:space-y-6">
          {/* Username Input */}
          <div className="relative">
            <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-600">
              <User className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <Input
              type="text"
              placeholder="User Name"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="pl-12 sm:pl-14 h-12 sm:h-14 text-base sm:text-lg bg-white border-none shadow-md"
            />
          </div>

          {/* Password Input */}
          <div className="relative">
            <div className="absolute left-4 top-1/2 -translate-y-1/2 text-gray-600">
              <Lock className="w-5 h-5 sm:w-6 sm:h-6" />
            </div>
            <Input
              type="password"
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="pl-12 sm:pl-14 h-12 sm:h-14 text-base sm:text-lg bg-white border-none shadow-md"
            />
          </div>

          {/* Login Button */}
          <Button
            type="submit"
            className="w-full h-12 sm:h-14 text-base sm:text-lg font-semibold bg-[#A08B5C] hover:bg-[#8F7A4B] text-white rounded-xl shadow-md"
          >
            LOGIN
          </Button>
          </form>
        </div>
      </div>

      {/* Bottom Banner Image */}
      <div className="relative w-full h-32 sm:h-40 md:h-48 lg:h-56">
        <Image
          src="/images/login_down.png"
          alt=""
          fill
          className="object-cover"
          priority
        />
      </div>
    </div>
  )
}
