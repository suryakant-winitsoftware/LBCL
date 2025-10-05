"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { Button } from '../../../components/ui/button'
import { Input } from '../../../components/ui/input'
import { User, Lock, ArrowLeft, AlertCircle, Eye, EyeOff, Loader2 } from 'lucide-react'

const StockReceivingLogin = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [rememberMe, setRememberMe] = useState(false)
  const router = useRouter()


  const handleLogin = async (e) => {
    e.preventDefault()
    if (!username.trim() || !password.trim()) {
      setError('Please fill in all fields')
      return
    }

    setLoading(true)
    setError('')

    try {
      // TODO: Implement actual authentication
      // For now, just navigate to the dashboard
      router.push('/manager/stock-receiving-dashboard')
    } catch (err) {
      setError('An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }


  return (
    <div style={{backgroundColor: '#ffffff'}} className="min-h-screen">
      {/* Background Pattern */}
      <div className="absolute inset-0"></div>
      
      <div className="relative min-h-screen flex">
        {/* Left Section - Branding */}
        <div style={{backgroundColor: '#375AE6'}} className="hidden lg:flex lg:w-1/2 p-12 items-center justify-center">
          <div className="max-w-md text-center">
            <div className="mb-8">
              <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="w-16 h-16 flex items-center justify-center mx-auto mb-4">
                <span style={{color: '#000000'}} className="text-3xl font-bold">L</span>
              </div>
              <h1 style={{color: '#ffffff'}} className="text-4xl font-bold mb-4">LION</h1>
              <p style={{color: '#ffffff'}} className="text-xl">Business Management System</p>
            </div>
            <div style={{color: '#ffffff'}} className="space-y-4">
              <p className="text-lg">Stock Management Portal</p>
              <div className="space-y-2 text-sm">
                <div className="flex items-center justify-center space-x-2">
                  <div style={{backgroundColor: '#ffffff'}} className="w-2 h-2"></div>
                  <span>Manage agent stock receiving</span>
                </div>
                <div className="flex items-center justify-center space-x-2">
                  <div style={{backgroundColor: '#ffffff'}} className="w-2 h-2"></div>
                  <span>Track inventory and physical counts</span>
                </div>
                <div className="flex items-center justify-center space-x-2">
                  <div style={{backgroundColor: '#ffffff'}} className="w-2 h-2"></div>
                  <span>Generate comprehensive reports</span>
                </div>
              </div>
            </div>
            <div style={{borderTop: '1px solid #ffffff'}} className="mt-8 pt-8">
              <p style={{color: '#ffffff'}} className="text-sm">Powered by</p>
              <p style={{color: '#ffffff'}} className="text-lg font-semibold">WINIT thinking mobile</p>
            </div>
          </div>
        </div>

        {/* Right Section - Login Form */}
        <div className="flex-1 flex items-center justify-center p-6 lg:p-12">
          <div className="w-full max-w-md">
            {/* Mobile Logo */}
            <div className="lg:hidden text-center mb-8">
              <div style={{backgroundColor: '#375AE6', border: '1px solid #000000'}} className="w-12 h-12 flex items-center justify-center mx-auto mb-4">
                <span style={{color: '#ffffff'}} className="text-2xl font-bold">L</span>
              </div>
              <h2 style={{color: '#000000'}} className="text-2xl font-bold">LION</h2>
              <p style={{color: '#000000'}} className="text-sm">Stock Management</p>
            </div>

            <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="p-6">
              {/* Header */}
              <div className="text-center mb-4">
                <h2 style={{color: '#000000'}} className="text-2xl font-bold mb-2">Stock Management</h2>
                <p style={{color: '#000000'}} className="text-sm">Sign in to your stock receiving account</p>
              </div>


              {/* Error Alert */}
              {error && (
                <div style={{backgroundColor: '#ffffff', border: '1px solid #000000'}} className="mb-4 p-3 flex items-center space-x-2">
                  <AlertCircle style={{color: '#000000'}} className="w-5 h-5 flex-shrink-0" />
                  <span style={{color: '#000000'}} className="text-sm">{error}</span>
                </div>
              )}

              {/* Login Form */}
              <form onSubmit={handleLogin} className="space-y-4">
                {/* Username Field */}
                <div>
                  <label style={{color: '#000000'}} className="block text-sm font-medium mb-1">Username</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-2 flex items-center pointer-events-none">
                      <User style={{color: '#000000'}} className="w-4 h-4" />
                    </div>
                    <Input
                      type="text"
                      placeholder="Enter your username"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      style={{backgroundColor: '#ffffff', color: '#000000', border: '1px solid #000000'}}
                      className="pl-8 pr-2 py-2 w-full"
                      required
                    />
                  </div>
                </div>

                {/* Password Field */}
                <div>
                  <label style={{color: '#000000'}} className="block text-sm font-medium mb-1">Password</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-2 flex items-center pointer-events-none">
                      <Lock style={{color: '#000000'}} className="w-4 h-4" />
                    </div>
                    <Input
                      type={showPassword ? "text" : "password"}
                      placeholder="Enter your password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      style={{backgroundColor: '#ffffff', color: '#000000', border: '1px solid #000000'}}
                      className="pl-8 pr-10 py-2 w-full"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute inset-y-0 right-0 pr-2 flex items-center"
                    >
                      {showPassword ? 
                        <EyeOff style={{color: '#000000'}} className="w-4 h-4" /> : 
                        <Eye style={{color: '#000000'}} className="w-4 h-4" />
                      }
                    </button>
                  </div>
                </div>

                {/* Remember Me */}
                <div className="flex items-center justify-between">
                  <label className="flex items-center">
                    <input
                      type="checkbox"
                      checked={rememberMe}
                      onChange={(e) => setRememberMe(e.target.checked)}
                      style={{accentColor: '#375AE6'}}
                      className="border-black"
                    />
                    <span style={{color: '#000000'}} className="ml-2 text-sm">Remember me</span>
                  </label>
                  <button
                    type="button"
                    style={{color: '#375AE6'}}
                    className="text-sm font-medium"
                  >
                    Forgot password?
                  </button>
                </div>

                {/* Login Button */}
                <Button
                  type="submit"
                  disabled={loading}
                  style={{backgroundColor: '#375AE6', color: '#ffffff', border: '1px solid #000000'}}
                  className="w-full py-2 font-semibold disabled:opacity-50"
                >
                  {loading ? (
                    <div className="flex items-center justify-center space-x-2">
                      <Loader2 className="w-4 h-4 animate-spin" />
                      <span>Signing In...</span>
                    </div>
                  ) : (
                    'Sign In'
                  )}
                </Button>
              </form>

              {/* Footer Links */}
              <div style={{borderTop: '1px solid #000000'}} className="mt-6 pt-4 text-center">
                <Link 
                  href="/" 
                  style={{color: '#000000'}}
                  className="inline-flex items-center space-x-2 text-sm font-medium"
                >
                  <ArrowLeft className="w-4 h-4" />
                  <span>Back to Home</span>
                </Link>
              </div>
            </div>

            {/* System Info */}
            <div className="mt-4 text-center text-xs" style={{color: '#000000'}}>
              <p>© 2025 LION Business Management System</p>
              <p>Powered by WINIT thinking mobile</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default StockReceivingLogin