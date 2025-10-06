"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { User, Lock, ArrowLeft } from "lucide-react"
import { useRouter } from "next/navigation"
import Image from "next/image"
import { useAuth } from "@/providers/auth-provider"
import { toast } from "sonner"
import { organizationService } from "@/services/organizationService"

export function LoginPage() {
  const router = useRouter()
  const { login, isLoading, user } = useAuth()
  const [username, setUsername] = useState("")
  const [password, setPassword] = useState("")

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()

    if (!username || !password) {
      toast.error("Please enter both username and password")
      return
    }

    try {
      const result = await login({
        loginId: username,
        password: password,
        rememberMe: false,
      })

      if (result.success && result.user) {
        toast.success("Login successful!")

        // Log complete user details
        console.log("==================== USER LOGIN DETAILS ====================")
        console.log("📋 Complete User Object:", JSON.stringify(result.user, null, 2))
        console.log("============================================================")

        console.log("👤 User ID:", result.user.uid || result.user.id)
        console.log("👤 Login ID:", result.user.loginId)
        console.log("👤 User Name:", result.user.name)
        console.log("👤 Email:", result.user.email)
        console.log("👤 Mobile:", result.user.mobile)
        console.log("👤 Status:", result.user.status)
        console.log("🏢 Company UID:", result.user.companyUID)

        console.log("\n🏢 CURRENT ORGANIZATION:")
        console.log("   - Full Object:", result.user.currentOrganization)
        console.log("   - UID:", result.user.currentOrganization?.uid)
        console.log("   - Code:", result.user.currentOrganization?.code)
        console.log("   - Name:", result.user.currentOrganization?.name)
        console.log("   - Type:", result.user.currentOrganization?.type)
        console.log("   - Parent UID:", result.user.currentOrganization?.parentUID)
        console.log("   - Is Active:", result.user.currentOrganization?.isActive)

        console.log("\n👥 USER ROLES:")
        result.user.roles?.forEach((role, index) => {
          console.log(`   Role ${index + 1}:`)
          console.log("      - UID:", role.uid || role.id)
          console.log("      - Name (EN):", role.roleNameEn)
          console.log("      - Code:", role.code)
          console.log("      - Is Principal Role:", role.isPrincipalRole)
          console.log("      - Is Distributor Role:", role.isDistributorRole)
          console.log("      - Is Admin:", role.isAdmin)
          console.log("      - Is Web User:", role.isWebUser)
          console.log("      - Is App User:", role.isAppUser)
          console.log("      - Organization UID:", role.organizationUID)
        })

        console.log("\n🏬 AVAILABLE ORGANIZATIONS:")
        result.user.availableOrganizations?.forEach((org, index) => {
          console.log(`   Org ${index + 1}:`)
          console.log("      - UID:", org.uid)
          console.log("      - Code:", org.code)
          console.log("      - Name:", org.name)
          console.log("      - Type:", org.type)
        })

        console.log("============================================================\n")

        // Fetch organization details from CompanyUID if available
        let isFranchiseOrg = false;
        let orgDataFetched = false;

        if (result.user.companyUID) {
          try {
            const { organization, orgType } = await organizationService.getOrganizationWithType(result.user.companyUID);

            console.log("\n📦 ORGANIZATION DETAILS FROM COMPANY UID:")
            console.log("   - Organization Code:", organization.Code)
            console.log("   - Organization Name:", organization.Name)
            console.log("   - Organization UID:", organization.UID)
            console.log("   - Organization Status:", organization.Status)
            console.log("   - Organization IsActive:", organization.IsActive)
            console.log("   - Organization Type UID:", organization.OrgTypeUID)

            if (organization && organization.OrgTypeUID) {
              // Successfully fetched organization with OrgTypeUID
              orgDataFetched = true;

              // Check if organization type is FR (Franchise/Distributor)
              isFranchiseOrg = organization.OrgTypeUID?.toUpperCase() === "FR";

              if (orgType) {
                console.log("\n🏢 ORGANIZATION TYPE DETAILS:")
                console.log("   - Type Name:", orgType.Name)
                console.log("   - Type UID:", orgType.UID)
                console.log("   - Is Company Org:", orgType.IsCompanyOrg)
                console.log("   - Is Franchisee Org:", orgType.IsFranchiseeOrg)
                console.log("   - Is Warehouse:", orgType.IsWh)
              }

              console.log("✅ Is Franchise Org (OrgTypeUID = FR):", isFranchiseOrg)
            } else {
              console.warn("⚠️ Organization has no OrgTypeUID, will use role-based routing")
              orgDataFetched = false;
            }
          } catch (error) {
            console.error("⚠️ Could not fetch organization details, will use role-based routing:", error)
            orgDataFetched = false;
          }
        }

        // Check organization type for routing
        const orgType = result.user.currentOrganization?.type?.toUpperCase()
        const orgCode = result.user.currentOrganization?.code?.toUpperCase()

        // Check if user has DISTRIBUTOR role
        const isDistributor = result.user.roles?.some(role => {
          console.log("👤 Role:", role.roleNameEn, "isDistributorRole:", role.isDistributorRole)
          return role.isDistributorRole === true
        })

        // Check if user belongs to PRINCIPLE organization OR has PRINCIPLE role
        const isPrincipalOrg = orgType === "PRINCIPAL" || orgType === "PRIN"
        const isPrincipalRole = result.user.roles?.some(role => {
          console.log("👤 Role:", role.roleNameEn, "isPrincipalRole:", role.isPrincipalRole)
          return role.isPrincipalRole === true
        })

        const isPrincipal = isPrincipalOrg || isPrincipalRole

        console.log("\n🔀 ROUTING DECISION:")
        console.log("   - Organization Type:", orgType)
        console.log("   - Organization Code:", orgCode)
        console.log("   - Org Data Fetched:", orgDataFetched)
        console.log("   - Is Distributor Role:", isDistributor)
        console.log("   - Is Franchise Org (FR):", isFranchiseOrg)
        console.log("   - Is Principal Org:", isPrincipalOrg)
        console.log("   - Is Principal Role:", isPrincipalRole)
        console.log("   - Is Principal (final):", isPrincipal)

        // Routing logic:
        // 1. If org data fetched successfully with OrgTypeUID:
        //    - If OrgTypeUID = "FR" -> stock-receiving
        //    - If OrgTypeUID != "FR" -> delivery-plans (if principal) or dashboard
        // 2. If org data NOT fetched (no OrgTypeUID):
        //    - Fall back to role-based routing (delivery-plans if principal, dashboard otherwise)

        if (orgDataFetched) {
          // Organization data was successfully fetched
          if (isFranchiseOrg) {
            console.log("🚀 OrgTypeUID = FR -> Redirecting to stock-receiving")
            router.push("/lbcl/stock-receiving")
          } else if (isPrincipal) {
            console.log("🚀 OrgTypeUID != FR and Principal user -> Redirecting to delivery-plans")
            router.push("/lbcl/delivery-plans")
          } else {
            console.log("🚀 OrgTypeUID != FR and not Principal -> Redirecting to dashboard")
            router.push("/lbcl/dashboard")
          }
        } else {
          // Organization data NOT fetched, use role-based routing
          console.log("⚠️ No OrgTypeUID available, using role-based routing")

          if (isDistributor) {
            console.log("🚀 Distributor role -> Redirecting to stock-receiving")
            router.push("/lbcl/stock-receiving")
          } else if (isPrincipal) {
            console.log("🚀 Principal role -> Redirecting to delivery-plans")
            router.push("/lbcl/delivery-plans")
          } else {
            console.log("🚀 Regular user -> Redirecting to dashboard")
            router.push("/lbcl/dashboard")
          }
        }
      } else {
        toast.error("Invalid credentials")
      }
    } catch (error) {
      console.error("Login error:", error)
      toast.error("Login failed")
    }
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
              disabled={isLoading}
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
              disabled={isLoading}
            />
          </div>

          {/* Login Button */}
          <Button
            type="submit"
            disabled={isLoading}
            className="w-full h-12 sm:h-14 text-base sm:text-lg font-semibold bg-[#A08B5C] hover:bg-[#8F7A4B] text-white rounded-xl shadow-md disabled:opacity-50"
          >
            {isLoading ? "LOGGING IN..." : "LOGIN"}
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
