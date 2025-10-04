import { Suspense } from "react"
import { LoginForm } from "@/components/auth/login-form"

export default function LoginPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <LoginForm />
    </Suspense>
  )
}

export const metadata = {
  title: "Login - WINIT Access Control",
  description: "Sign in to WINIT Access Control System",
}