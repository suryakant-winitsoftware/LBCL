"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import Link from 'next/link'
import { User, Lock, Shield, Eye, EyeOff, Loader2, AlertCircle, Settings, Database, Users, BarChart3, Truck, Package, Activity, Clock, FileText, Zap, ArrowLeft } from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'

const AdminLogin = () => {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const { login } = useAuth()
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
      const result = await login(username, password, 'admin')
      
      if (result.success) {
        router.push('/user/admin/dashboard')
      } else {
        setError(result.error || 'Login failed')
      }
    } catch (err) {
      setError('An unexpected error occurred')
    } finally {
      setLoading(false)
    }
  }


  return (
    <div className="min-h-screen bg-[var(--background)]">
      <div className="min-h-screen flex">
        {/* Left Section - Admin Features Overview */}
        <div className="hidden lg:flex lg:w-3/5 bg-[var(--secondary)] p-12 items-center justify-center">
          <div className="max-w-2xl text-[var(--secondary-foreground)]">
            <div className="text-center mb-12">
              <div className="w-20 h-20 bg-[var(--primary)] border-2 border-[var(--secondary-foreground)] rounded-xl flex items-center justify-center mx-auto mb-6">
                <Shield className="w-10 h-10 text-[var(--primary-foreground)]" />
              </div>
              <h1 className="text-5xl font-bold mb-4">ADMIN PORTAL</h1>
              <p className="text-xl mb-2">LION Business Management System</p>
              <p className="text-sm text-[var(--primary)] font-semibold">MASTER CONTROL CENTER</p>
            </div>

            {/* Features Grid */}
            <div className="grid grid-cols-3 gap-6 mb-8">
              
              {/* User Management */}
              <div className="text-center p-5 border border-[var(--secondary-foreground)] rounded-lg hover:bg-[var(--secondary-foreground)]/10 transition-all duration-200">
                <Users className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">USER MANAGEMENT</h3>
                <ul className="text-xs space-y-1 text-[var(--muted-foreground)]">
                  <li>• All User Access</li>
                  <li>• Role Permissions</li>
                  <li>• Login Monitoring</li>
                  <li>• Security Audit</li>
                </ul>
              </div>

              {/* System Control */}
              <div className="text-center p-5 border border-[var(--secondary-foreground)] rounded-lg hover:bg-[var(--secondary-foreground)]/10 transition-all duration-200">
                <Settings className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">SYSTEM CONTROL</h3>
                <ul className="text-xs space-y-1 text-[var(--muted-foreground)]">
                  <li>• Global Settings</li>
                  <li>• Feature Toggles</li>
                  <li>• System Health</li>
                  <li>• Maintenance Mode</li>
                </ul>
              </div>

              {/* Data Analytics */}
              <div className="text-center p-5 border border-[var(--secondary-foreground)] rounded-lg hover:bg-[var(--secondary-foreground)]/10 transition-all duration-200">
                <BarChart3 className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">DATA ANALYTICS</h3>
                <ul className="text-xs space-y-1 text-[var(--muted-foreground)]">
                  <li>• Performance Metrics</li>
                  <li>• Usage Statistics</li>
                  <li>• Custom Reports</li>
                  <li>• Data Export</li>
                </ul>
              </div>

              {/* Stock Operations */}
              <div className="text-center p-5 border border-[var(--secondary-foreground)] rounded-lg hover:bg-[var(--secondary-foreground)]/10 transition-all duration-200">
                <Package className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">STOCK OPERATIONS</h3>
                <ul className="text-xs space-y-1 text-[var(--muted-foreground)]">
                  <li>• Inventory Overview</li>
                  <li>• Stock Receiving</li>
                  <li>• Physical Counts</li>
                  <li>• Stock Alerts</li>
                </ul>
              </div>

              {/* Delivery Management */}
              <div className="text-center p-5 border border-[var(--secondary-foreground)] rounded-lg hover:bg-[var(--secondary-foreground)]/10 transition-all duration-200">
                <Truck className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">DELIVERY CONTROL</h3>
                <ul className="text-xs space-y-1 text-[var(--muted-foreground)]">
                  <li>• Route Planning</li>
                  <li>• Driver Tracking</li>
                  <li>• Delivery Status</li>
                  <li>• Fleet Management</li>
                </ul>
              </div>

              {/* Advanced Features */}
              <div className="text-center p-5 border border-[var(--primary)] bg-[var(--primary)] rounded-lg">
                <Zap className="w-6 h-6 mx-auto mb-3" />
                <h3 className="text-sm font-semibold mb-2">ADVANCED TOOLS</h3>
                <ul className="text-xs space-y-1 text-[var(--primary-foreground)]">
                  <li>• API Management</li>
                  <li>• Integration Hub</li>
                  <li>• Backup Control</li>
                  <li>• System Logs</li>
                </ul>
              </div>

            </div>

            {/* System Status */}
            <div className="border-t border-[var(--secondary-foreground)] pt-8 text-center">
              <div className="flex justify-center gap-8 mb-4">
                <div className="text-center">
                  <Activity className="w-4 h-4 mx-auto mb-1 text-[var(--primary)]" />
                  <p className="text-xs">System Online</p>
                </div>
                <div className="text-center">
                  <Database className="w-4 h-4 mx-auto mb-1 text-[var(--primary)]" />
                  <p className="text-xs">DB Connected</p>
                </div>
                <div className="text-center">
                  <Clock className="w-4 h-4 mx-auto mb-1 text-[var(--primary)]" />
                  <p className="text-xs">Real-time</p>
                </div>
              </div>
              <p className="text-sm text-[var(--primary)] font-semibold">POWERED BY WINIT THINKING MOBILE</p>
            </div>
          </div>
        </div>

        {/* Right Section - Login Form */}
        <div className="flex-1 flex items-center justify-center p-6 lg:p-12">
          <div className="w-full max-w-md">
            
            {/* Mobile Logo */}
            <div className="lg:hidden text-center mb-8">
              <div className="w-12 h-12 bg-[var(--secondary)] border-2 border-[var(--border)] flex items-center justify-center mx-auto mb-4 rounded-lg">
                <Shield className="text-[var(--secondary-foreground)] text-2xl font-bold" />
              </div>
              <h2 className="text-[var(--foreground)] text-2xl font-bold">ADMIN</h2>
              <p className="text-[var(--muted-foreground)] text-sm">System Control</p>
            </div>

            <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-8">
              {/* Header */}
              <div className="text-center mb-6">
                <div className="w-16 h-16 bg-[var(--secondary)] rounded-lg flex items-center justify-center mx-auto mb-4">
                  <Shield className="w-8 h-8 text-[var(--secondary-foreground)]" />
                </div>
                <h2 className="text-2xl font-bold text-[var(--foreground)] mb-2">ADMIN ACCESS</h2>
                <p className="text-[var(--muted-foreground)]">Master Control Login</p>
              </div>


              {/* Error Alert */}
              {error && (
                <div className="mb-5 p-4 bg-[var(--destructive-light)] border border-[var(--destructive)] rounded-lg flex items-center gap-3">
                  <AlertCircle className="w-5 h-5 text-[var(--destructive)] flex-shrink-0" />
                  <span className="text-sm text-[var(--destructive)] font-medium">{error}</span>
                </div>
              )}

              {/* Login Form */}
              <form onSubmit={handleLogin} className="space-y-5">
                {/* Username Field */}
                <div>
                  <label className="block text-sm font-semibold text-[var(--foreground)] mb-2">ADMIN USERNAME</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <User className="w-5 h-5 text-[var(--muted-foreground)]" />
                    </div>
                    <input
                      type="text"
                      placeholder="Enter admin username"
                      value={username}
                      onChange={(e) => setUsername(e.target.value)}
                      className="w-full pl-10 pr-4 py-3 border-2 border-[var(--input-border)] rounded-lg bg-[var(--input)] text-[var(--foreground)] placeholder:text-[var(--muted-foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--primary)] focus:border-transparent transition-all duration-200 font-medium"
                      required
                    />
                  </div>
                </div>

                {/* Password Field */}
                <div>
                  <label className="block text-sm font-semibold text-[var(--foreground)] mb-2">ADMIN PASSWORD</label>
                  <div className="relative">
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <Lock className="w-5 h-5 text-[var(--muted-foreground)]" />
                    </div>
                    <input
                      type={showPassword ? "text" : "password"}
                      placeholder="Enter admin password"
                      value={password}
                      onChange={(e) => setPassword(e.target.value)}
                      className="w-full pl-10 pr-12 py-3 border-2 border-[var(--input-border)] rounded-lg bg-[var(--input)] text-[var(--foreground)] placeholder:text-[var(--muted-foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--primary)] focus:border-transparent transition-all duration-200 font-medium"
                      required
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute inset-y-0 right-0 pr-3 flex items-center hover:text-[var(--foreground)] transition-colors duration-200"
                    >
                      {showPassword ? 
                        <EyeOff className="w-5 h-5 text-[var(--muted-foreground)]" /> :
                        <Eye className="w-5 h-5 text-[var(--muted-foreground)]" />
                      }
                    </button>
                  </div>
                </div>

                {/* Login Button */}
                <button
                  type="submit"
                  disabled={loading}
                  className="w-full py-4 px-6 bg-[var(--primary)] hover:bg-[var(--primary-hover)] disabled:opacity-50 text-[var(--primary-foreground)] font-bold rounded-lg transition-all duration-200 flex items-center justify-center gap-3 text-lg tracking-wide"
                >
                  {loading ? (
                    <>
                      <Loader2 className="w-5 h-5 animate-spin" />
                      <span>AUTHENTICATING...</span>
                    </>
                  ) : (
                    'ACCESS ADMIN PORTAL'
                  )}
                </button>
              </form>

              {/* Footer */}
              <div className="mt-8 pt-6 border-t border-[var(--border)] text-center">
                <Link
                  href="/"
                  className="inline-flex items-center gap-2 text-sm text-[var(--muted-foreground)] hover:text-[var(--foreground)] font-medium transition-colors duration-200"
                >
                  <ArrowLeft className="w-4 h-4" />
                  <span>Back to Home</span>
                </Link>
              </div>
            </div>

            {/* Security Notice */}
            <div className="mt-6 text-center text-xs text-[var(--muted-foreground)]">
              <div className="bg-[var(--warning-light)] border border-[var(--warning)] rounded-lg p-3 mb-3">
                <p className="font-semibold text-[var(--warning)]">⚡ SECURE ADMIN ACCESS ⚡</p>
                <p className="text-[var(--warning)]">All admin actions are logged and monitored</p>
              </div>
              <p>© 2025 LION Business Management System</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default AdminLogin