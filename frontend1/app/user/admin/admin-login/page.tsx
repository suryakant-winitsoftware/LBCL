"use client"
import { useEffect } from 'react'
import { useRouter } from 'next/navigation'

export default function AdminLoginPage() {
  const router = useRouter()
  
  useEffect(() => {
    router.push('/user/unified-login')
  }, [])
  
  return null
}