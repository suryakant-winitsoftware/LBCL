"use client"
import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { 
  Shield, Users, Settings, BarChart3, Package, Truck, 
  FileText, LogOut, Search, Plus, RefreshCw,
  Edit, Trash2, Lock, Unlock, User, MapPin, X,
  Check, AlertCircle, ChevronRight, Activity, Database,
  Bell, Menu, Home, ChevronDown
} from 'lucide-react'
import { useAuth } from '../../../contexts/AuthContext'
import ProtectedRoute from '../../../components/ProtectedRoute'
import { employeeService } from '../../../services/user_management'
import { Button } from '../../../../components/ui/button'
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '../../../../components/ui/card'
import { Input } from '../../../../components/ui/input'

const AdminDashboard = () => {
  const { user, logout } = useAuth()
  const router = useRouter()
  const [activeSection, setActiveSection] = useState('overview')
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [showEditModal, setShowEditModal] = useState(false)
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false)
  const [selectedUser, setSelectedUser] = useState(null)
  const [notification, setNotification] = useState(null)
  const [stats, setStats] = useState({ total: 0, active: 0, inactive: 0, pending: 0 })
  const [sidebarOpen, setSidebarOpen] = useState(false)
  
  // Form states for create/edit user
  const [formData, setFormData] = useState({
    name: '',
    loginId: '',
    password: '',
    email: '',
    phone: '',
    role: 'stock_manager',
    status: 'Active',
    authType: 'Regular'
  })

  // Fetch users from API
  const fetchUsers = async () => {
    setLoading(true)
    try {
      const pagingRequest = employeeService.buildPagingRequest(1, 100, searchTerm)
      const response = await employeeService.getEmployees(pagingRequest)
      
      const formattedUsers = response.pagedData.map(emp => ({
        uid: emp.UID || emp.uid,
        name: emp.Name || emp.name,
        username: emp.LoginId || emp.loginId,
        email: emp.Email || emp.email || '',
        phone: emp.Phone || emp.phone || '',
        role: mapRole(emp),
        system: mapSystem(emp),
        status: emp.Status || emp.status || 'Active',
        startDate: emp.StartDate || emp.startDate,
        modifiedTime: emp.ModifiedTime || emp.modifiedTime,
        canHandleStock: emp.CanHandleStock || emp.canHandleStock || false
      }))
      
      setUsers(formattedUsers)
      
      // Fetch stats
      const statsData = await employeeService.getEmployeeStats()
      setStats(statsData)
    } catch (error) {
      console.error('Failed to fetch users:', error)
      showNotification('Failed to fetch users', 'error')
    } finally {
      setLoading(false)
    }
  }

  // Map employee data to role
  const mapRole = (emp) => {
    const authType = emp.AuthType || emp.authType || ''
    if (authType.toLowerCase().includes('admin')) return 'super_admin'
    if (authType.toLowerCase().includes('delivery')) return 'delivery_manager'
    if (authType.toLowerCase().includes('itinerary')) return 'itinerary_manager'
    return 'stock_manager'
  }

  // Map role to system
  const mapSystem = (role) => {
    const roleStr = typeof role === 'object' ? mapRole(role) : role
    switch(roleStr) {
      case 'super_admin': return 'admin'
      case 'delivery_manager': return 'delivery'
      case 'itinerary_manager': return 'itinerary'
      case 'stock_manager': return 'stock'
      default: return 'stock'
    }
  }

  useEffect(() => {
    if (activeSection === 'users') {
      fetchUsers()
    }
  }, [activeSection, searchTerm])

  // Show notification
  const showNotification = (message, type = 'success') => {
    setNotification({ message, type })
    setTimeout(() => setNotification(null), 3000)
  }

  // Handle create user
  const handleCreateUser = async () => {
    try {
      const timestamp = Date.now();
      const empUID = `EMP_${timestamp}`;
      const empInfoUID = `EI_${timestamp}`;
      const jobPositionUID = `JP_${timestamp}`;
      const currentDateTime = new Date().toISOString();
      
      const empDTO = {
        Emp: {
          UID: empUID,
          Code: formData.loginId.toUpperCase(),
          Name: formData.name,
          LoginId: formData.loginId,
          Status: 'Active',
          AuthType: 'Basic',
          Password: formData.password,
          ActionType: 1,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
          CreatedTime: currentDateTime,
          ModifiedTime: currentDateTime,
          ServerAddTime: currentDateTime,
          ServerModifiedTime: currentDateTime
        },
        EmpInfo: {
          UID: empInfoUID,
          EmpUID: empUID,
          Email: formData.email || '',
          Phone: formData.phone || '',
          StartDate: currentDateTime,
          CanHandleStock: false,
          ActionType: 1,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
          CreatedTime: currentDateTime,
          ModifiedTime: currentDateTime,
          ServerAddTime: currentDateTime,
          ServerModifiedTime: currentDateTime
        },
        JobPosition: {
          UID: jobPositionUID,
          EmpUID: empUID,
          CompanyUID: '',
          OrgUID: '',
          Department: '',
          UserRoleUID: '',
          HasEOT: false,
          CollectionLimit: 0,
          ActionType: 1,
          CreatedBy: 'ADMIN',
          ModifiedBy: 'ADMIN',
          CreatedTime: currentDateTime,
          ModifiedTime: currentDateTime,
          ServerAddTime: currentDateTime,
          ServerModifiedTime: currentDateTime
        },
        EmpOrgMapping: []
      }

      await employeeService.createEmployee(empDTO)
      showNotification('User created successfully')
      setShowCreateModal(false)
      resetForm()
      fetchUsers()
    } catch (error) {
      console.error('Failed to create user:', error)
      showNotification('Failed to create user', 'error')
    }
  }

  // Handle update user
  const handleUpdateUser = async () => {
    try {
      const empDTO = {
        emp: {
          ...selectedUser,
          name: formData.name,
          loginId: formData.loginId,
          status: formData.status,
          authType: formData.role,
          actionType: 'Update',
          modifiedBy: 'ADMIN',
          modifiedTime: new Date().toISOString()
        }
      }

      await employeeService.updateEmployee(empDTO)
      
      // Update password if provided
      if (formData.password) {
        await employeeService.resetPassword(selectedUser.uid, formData.password)
      }
      
      showNotification('User updated successfully')
      setShowEditModal(false)
      resetForm()
      fetchUsers()
    } catch (error) {
      console.error('Failed to update user:', error)
      showNotification('Failed to update user', 'error')
    }
  }

  // Handle delete user
  const handleDeleteUser = async () => {
    try {
      await employeeService.deleteEmployee(selectedUser.uid)
      showNotification('User deleted successfully')
      setShowDeleteConfirm(false)
      setSelectedUser(null)
      fetchUsers()
    } catch (error) {
      // Try deactivate if delete fails
      try {
        await employeeService.deactivateEmployee(selectedUser.uid)
        showNotification('User deactivated successfully')
        setShowDeleteConfirm(false)
        setSelectedUser(null)
        fetchUsers()
      } catch (deactivateError) {
        console.error('Failed to delete/deactivate user:', deactivateError)
        showNotification('Failed to delete user', 'error')
      }
    }
  }

  // Handle toggle user status
  const handleToggleStatus = async (user) => {
    try {
      if (user.status === 'Active') {
        await employeeService.deactivateEmployee(user.uid)
        showNotification('User deactivated successfully')
      } else {
        await employeeService.activateEmployee(user.uid)
        showNotification('User activated successfully')
      }
      fetchUsers()
    } catch (error) {
      console.error('Failed to toggle user status:', error)
      showNotification('Failed to update user status', 'error')
    }
  }

  // Reset form
  const resetForm = () => {
    setFormData({
      name: '',
      loginId: '',
      password: '',
      email: '',
      phone: '',
      role: 'stock_manager',
      status: 'Active',
      authType: 'Regular'
    })
    setSelectedUser(null)
  }

  // Open edit modal
  const openEditModal = (user) => {
    setSelectedUser(user)
    setFormData({
      name: user.name,
      loginId: user.username,
      password: '',
      email: user.email || '',
      phone: '',
      role: user.role,
      status: user.status,
      authType: 'Regular'
    })
    setShowEditModal(true)
  }

  const handleLogout = () => {
    logout()
    router.push('/user/unified-login')
  }

  const navItems = [
    { id: 'overview', label: 'Dashboard', icon: Home },
    { id: 'users', label: 'User Management', icon: Users },
    { id: 'stock', label: 'Stock Control', icon: Package },
    { id: 'delivery', label: 'Delivery Control', icon: Truck },
    { id: 'analytics', label: 'Analytics', icon: BarChart3 },
    { id: 'system', label: 'System Settings', icon: Settings },
    { id: 'logs', label: 'Activity Logs', icon: FileText }
  ]

  const renderSystemOverview = () => (
    <div className="min-h-screen bg-[var(--background)] p-4 sm:p-6">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-[var(--foreground)] mb-2">System Overview</h1>
        <p className="text-[var(--muted-foreground)]">Monitor and manage your entire system from one place</p>
      </div>
      
      {/* Stats Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 sm:gap-6 mb-8">
        <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
          <div className="flex items-center">
            <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--primary-light)] rounded-lg flex items-center justify-center">
              <Users className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
            </div>
            <div className="ml-3 sm:ml-4">
              <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Total Users</p>
              <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{stats.total}</p>
            </div>
          </div>
        </div>

        <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
          <div className="flex items-center">
            <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--success-light)] rounded-lg flex items-center justify-center">
              <Check className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
            </div>
            <div className="ml-3 sm:ml-4">
              <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Active Users</p>
              <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{stats.active}</p>
            </div>
          </div>
        </div>

        <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
          <div className="flex items-center">
            <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--muted)] rounded-lg flex items-center justify-center">
              <Lock className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--muted-foreground)]" />
            </div>
            <div className="ml-3 sm:ml-4">
              <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">Inactive Users</p>
              <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">{stats.inactive}</p>
            </div>
          </div>
        </div>

        <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
          <div className="flex items-center">
            <div className="w-10 h-10 sm:w-12 sm:h-12 bg-[var(--primary-light)] rounded-lg flex items-center justify-center">
              <Activity className="w-5 h-5 sm:w-6 sm:h-6 text-[var(--primary)]" />
            </div>
            <div className="ml-3 sm:ml-4">
              <p className="text-xs sm:text-sm font-medium text-[var(--muted-foreground)]">System Load</p>
              <p className="text-xl sm:text-2xl font-bold text-[var(--foreground)]">94%</p>
            </div>
          </div>
        </div>
      </div>

      {/* Quick Access Panel */}
      <div className="bg-[var(--card)] rounded-lg shadow-sm border border-[var(--border)] p-4 sm:p-6">
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-[var(--foreground)] mb-2">Quick Access</h2>
          <p className="text-[var(--muted-foreground)]">Navigate to frequently used sections</p>
        </div>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <button
            onClick={() => router.push('/user/manager/stock-receiving-dashboard')}
            className="bg-[var(--card)] border border-[var(--border)] rounded-lg p-4 sm:p-6 text-center hover:border-[var(--primary)] hover:bg-[var(--primary-light)] group transition-all duration-200"
          >
            <Package className="w-6 h-6 sm:w-8 sm:h-8 mb-3 text-[var(--primary)] mx-auto group-hover:scale-110 transition-transform duration-200" />
            <div className="font-semibold text-[var(--foreground)] mb-1 text-sm sm:text-base">Stock Portal</div>
            <div className="text-xs sm:text-sm text-[var(--muted-foreground)]">Manage inventory</div>
          </button>
          
          <button
            onClick={() => router.push('/user/delivery/delivery-dashboard')}
            className="bg-[var(--card)] border border-[var(--border)] rounded-lg p-4 sm:p-6 text-center hover:border-[var(--primary)] hover:bg-[var(--primary-light)] group transition-all duration-200"
          >
            <Truck className="w-6 h-6 sm:w-8 sm:h-8 mb-3 text-[var(--primary)] mx-auto group-hover:scale-110 transition-transform duration-200" />
            <div className="font-semibold text-[var(--foreground)] mb-1 text-sm sm:text-base">Delivery Portal</div>
            <div className="text-xs sm:text-sm text-[var(--muted-foreground)]">Track shipments</div>
          </button>
          
          <button
            onClick={() => router.push('/user/itinerary/dashboard')}
            className="bg-[var(--card)] border border-[var(--border)] rounded-lg p-4 sm:p-6 text-center hover:border-[var(--primary)] hover:bg-[var(--primary-light)] group transition-all duration-200"
          >
            <MapPin className="w-6 h-6 sm:w-8 sm:h-8 mb-3 text-[var(--primary)] mx-auto group-hover:scale-110 transition-transform duration-200" />
            <div className="font-semibold text-[var(--foreground)] mb-1 text-sm sm:text-base">Sales Itinerary</div>
            <div className="text-xs sm:text-sm text-[var(--muted-foreground)]">Plan routes</div>
          </button>
          
          <button
            onClick={() => setActiveSection('users')}
            className="bg-[var(--card)] border border-[var(--border)] rounded-lg p-4 sm:p-6 text-center hover:border-[var(--primary)] hover:bg-[var(--primary-light)] group transition-all duration-200"
          >
            <Users className="w-6 h-6 sm:w-8 sm:h-8 mb-3 text-[var(--primary)] mx-auto group-hover:scale-110 transition-transform duration-200" />
            <div className="font-semibold text-[var(--foreground)] mb-1 text-sm sm:text-base">Manage Users</div>
            <div className="text-xs sm:text-sm text-[var(--muted-foreground)]">User administration</div>
          </button>
        </div>
      </div>
    </div>
  )

  const renderUserManagement = () => (
    <div className="animate-fadeIn">
      <div className="mb-6">
        <h1 className="text-2xl font-semibold text-[var(--foreground)] mb-2">User Management</h1>
        <p className="text-[var(--muted-foreground)]">Create, edit, and manage system users</p>
      </div>
      
      <Card className="border-0 shadow-sm bg-white">
        <CardHeader className="border-b border-[var(--border)] bg-[var(--muted)]">
          <div className="flex flex-col lg:flex-row lg:items-center lg:justify-between gap-4">
            <CardTitle className="text-lg">All Users</CardTitle>
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="relative flex-1 sm:flex-initial">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--muted-foreground)]" />
                <input 
                  type="text" 
                  placeholder="Search users..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10 pr-4 py-2.5 border border-[var(--input-border)] rounded-lg bg-[var(--input)] text-[var(--foreground)] placeholder:text-[var(--muted-foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--primary)] focus:border-transparent transition-all duration-200 w-full sm:w-64 text-sm"
                />
              </div>
              <Button 
                onClick={fetchUsers}
                variant="outline"
                size="sm"
                className="border-[var(--input-border)]"
              >
                <RefreshCw className="w-4 h-4" />
              </Button>
              <Button 
                onClick={() => setShowCreateModal(true)}
                variant="default"
                size="sm"
                className="bg-[var(--primary)] hover:bg-[var(--primary-hover)]"
              >
                <Plus className="w-4 h-4 mr-2" />
                Add User
              </Button>
            </div>
          </div>
        </CardHeader>
        
        <CardContent className="p-0">
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <div className="animate-pulse text-[var(--muted-foreground)]">Loading users...</div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full">
                <thead className="bg-[var(--muted)]">
                  <tr>
                    <th className="text-left py-4 px-6 font-medium text-xs uppercase tracking-wider text-[var(--muted-foreground)]">User</th>
                    <th className="text-left py-4 px-6 font-medium text-xs uppercase tracking-wider text-[var(--muted-foreground)]">Role</th>
                    <th className="text-left py-4 px-6 font-medium text-xs uppercase tracking-wider text-[var(--muted-foreground)]">System</th>
                    <th className="text-left py-4 px-6 font-medium text-xs uppercase tracking-wider text-[var(--muted-foreground)]">Status</th>
                    <th className="text-left py-4 px-6 font-medium text-xs uppercase tracking-wider text-[var(--muted-foreground)]">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-[var(--border)]">
                  {users.map((user, index) => (
                    <tr key={user.uid || index} className="hover:bg-[var(--muted)] transition-colors">
                      <td className="py-4 px-6">
                        <div className="flex items-center gap-3">
                          <div className="w-10 h-10 rounded-full bg-[var(--primary)] flex items-center justify-center text-white font-semibold text-sm">
                            {user.name.charAt(0).toUpperCase()}
                          </div>
                          <div>
                            <p className="font-medium text-[var(--foreground)] text-sm">{user.name}</p>
                            <p className="text-sm text-[var(--muted-foreground)]">{user.username}</p>
                          </div>
                        </div>
                      </td>
                      <td className="py-4 px-6">
                        <span className="text-sm text-[var(--foreground)] capitalize">{user.role.replace(/_/g, ' ')}</span>
                      </td>
                      <td className="py-4 px-6">
                        <span className="text-sm text-[var(--foreground)] capitalize">{user.system}</span>
                      </td>
                      <td className="py-4 px-6">
                        <span className={`
                          inline-flex items-center px-2.5 py-1 rounded-full text-xs font-medium
                          ${user.status === 'Active' 
                            ? 'bg-[var(--success-light)] text-[var(--success)]' 
                            : 'bg-[var(--muted)] text-[var(--muted-foreground)]'}
                        `}>
                          {user.status}
                        </span>
                      </td>
                      <td className="py-4 px-6">
                        <div className="flex items-center gap-1">
                          <Button 
                            onClick={() => openEditModal(user)}
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 hover:bg-[var(--secondary-light)]"
                          >
                            <Edit className="w-4 h-4" />
                          </Button>
                          <Button 
                            onClick={() => handleToggleStatus(user)}
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 hover:bg-[var(--secondary-light)]"
                          >
                            {user.status === 'Active' ? 
                              <Lock className="w-4 h-4" /> :
                              <Unlock className="w-4 h-4" />
                            }
                          </Button>
                          <Button 
                            onClick={() => { setSelectedUser(user); setShowDeleteConfirm(true); }}
                            variant="ghost"
                            size="sm"
                            className="h-8 w-8 p-0 hover:bg-[var(--destructive-light)] hover:text-[var(--destructive)]"
                          >
                            <Trash2 className="w-4 h-4" />
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  )

  const renderContent = () => {
    switch(activeSection) {
      case 'overview': return renderSystemOverview()
      case 'users': return renderUserManagement()
      default: return renderSystemOverview()
    }
  }

  return (
    <ProtectedRoute requiredSystem="admin">
      <div className="min-h-screen bg-[var(--background)] flex">
        {/* Mobile Menu Overlay */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 bg-black/50 z-40 lg:hidden"
            onClick={() => setSidebarOpen(false)}
          />
        )}

        {/* Sidebar */}
        <div className={`
          fixed lg:relative z-50 lg:z-auto
          w-64 h-full bg-[var(--sidebar)] border-r border-[var(--sidebar-border)]
          transform transition-transform duration-300 ease-in-out
          ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
          lg:translate-x-0 flex flex-col
        `}>
          {/* Logo Section */}
          <div className="p-6 border-b border-[var(--sidebar-border)]">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-lg bg-[var(--primary)] flex items-center justify-center">
                  <Shield className="w-6 h-6 text-white" />
                </div>
                <div>
                  <h2 className="font-semibold text-[var(--primary)] text-sm">Admin Portal</h2>
                  <p className="text-xs text-[var(--muted-foreground)]">System Control</p>
                </div>
              </div>
              <button
                onClick={() => setSidebarOpen(false)}
                className="lg:hidden p-2 rounded-md text-[var(--sidebar-foreground)] hover:bg-[var(--sidebar-hover)] transition-colors"
              >
                <X className="w-5 h-5" />
              </button>
            </div>
          </div>

          {/* User Info */}
          <div className="p-4 mx-4 mt-4 rounded-lg bg-[var(--sidebar-active)]">
            <div className="flex items-center gap-3">
              <div className="w-9 h-9 rounded-full bg-[var(--primary)] flex items-center justify-center">
                <User className="w-4 h-4 text-white" />
              </div>
              <div>
                <p className="font-medium text-[var(--foreground)] text-sm">Administrator</p>
                <p className="text-xs text-[var(--muted-foreground)]">System Admin</p>
              </div>
            </div>
          </div>

          {/* Navigation */}
          <nav className="flex-1 p-4 mt-2 space-y-1">
            {navItems.map((item) => {
              const Icon = item.icon
              return (
                <button
                  key={item.id}
                  onClick={() => {
                    setActiveSection(item.id)
                    setSidebarOpen(false)
                  }}
                  className={`
                    w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm
                    transition-all duration-200
                    ${activeSection === item.id 
                      ? 'bg-[var(--sidebar-active)] text-[var(--primary)] font-medium' 
                      : 'text-[var(--sidebar-foreground)] hover:bg-[var(--sidebar-hover)] hover:text-[var(--primary)]'}
                  `}
                >
                  <Icon className="w-4 h-4 flex-shrink-0" />
                  <span className="flex-1 text-left">{item.label}</span>
                  {activeSection === item.id && (
                    <ChevronRight className="w-4 h-4" />
                  )}
                </button>
              )
            })}
          </nav>

          {/* Logout Button */}
          <div className="p-4 border-t border-[var(--sidebar-border)]">
            <Button 
              onClick={handleLogout}
              variant="outline"
              size="sm"
              className="w-full justify-start text-sm border-[var(--sidebar-border)] hover:bg-[var(--sidebar-hover)]"
            >
              <LogOut className="w-4 h-4 mr-2" />
              Logout
            </Button>
          </div>
        </div>

        {/* Main Content */}
        <div className="flex-1 overflow-auto bg-[var(--muted)] lg:ml-0">
          {/* Mobile Header */}
          <div className="lg:hidden bg-[var(--sidebar)] border-b border-[var(--sidebar-border)] p-4 flex items-center justify-between">
            <button
              onClick={() => setSidebarOpen(true)}
              className="p-2 rounded-md text-[var(--sidebar-foreground)] hover:bg-[var(--sidebar-hover)] transition-colors"
            >
              <Menu className="w-6 h-6" />
            </button>
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 rounded-lg bg-[var(--primary)] flex items-center justify-center">
                <Shield className="w-5 h-5 text-white" />
              </div>
              <h2 className="font-semibold text-[var(--primary)] text-sm">Admin Portal</h2>
            </div>
          </div>

          <div className="p-4 sm:p-6 lg:p-8">
            {renderContent()}
          </div>
        </div>

        {/* Notification */}
        {notification && (
          <div className={`
            fixed top-6 right-6 p-4 rounded-lg shadow-lg
            flex items-center gap-3 animate-slideInUp z-50
            ${notification.type === 'error' 
              ? 'bg-[var(--destructive)] text-[var(--destructive-foreground)]' 
              : 'bg-[var(--success)] text-[var(--success-foreground)]'}
          `}>
            {notification.type === 'error' ? 
              <AlertCircle className="w-5 h-5" /> :
              <Check className="w-5 h-5" />
            }
            <span className="font-medium">{notification.message}</span>
          </div>
        )}

        {/* Create/Edit User Modal */}
        {(showCreateModal || showEditModal) && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
            <Card className="w-full max-w-md shadow-xl">
              <CardHeader className="border-b border-[var(--border)]">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-lg">{showCreateModal ? 'Create New User' : 'Edit User'}</CardTitle>
                  <Button
                    onClick={() => { 
                      setShowCreateModal(false); 
                      setShowEditModal(false); 
                      resetForm(); 
                    }}
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0"
                  >
                    <X className="w-4 h-4" />
                  </Button>
                </div>
              </CardHeader>
              <CardContent className="p-6 space-y-4">
                <Input
                  label="Name"
                  value={formData.name}
                  onChange={(e) => setFormData({...formData, name: e.target.value})}
                  placeholder="Enter user's full name"
                />
                
                <Input
                  label="Login ID"
                  value={formData.loginId}
                  onChange={(e) => setFormData({...formData, loginId: e.target.value})}
                  placeholder="Enter unique login ID"
                />
                
                <Input
                  label={showEditModal ? "New Password (leave empty to keep current)" : "Password"}
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({...formData, password: e.target.value})}
                  placeholder={showEditModal ? "Enter new password to change" : "Enter password"}
                />
                
                <Input
                  label="Email"
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({...formData, email: e.target.value})}
                  placeholder="user@example.com"
                />
                
                <Input
                  label="Phone"
                  value={formData.phone}
                  onChange={(e) => setFormData({...formData, phone: e.target.value})}
                  placeholder="+1234567890"
                />
                
                <div>
                  <label className="block text-sm font-medium text-[var(--foreground)] mb-2">Role</label>
                  <select
                    value={formData.role}
                    onChange={(e) => setFormData({...formData, role: e.target.value})}
                    className="w-full px-4 py-2.5 border border-[var(--input-border)] rounded-lg bg-[var(--input)] text-[var(--foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--primary)] transition-all duration-200 text-sm"
                  >
                    <option value="stock_manager">Stock Manager</option>
                    <option value="delivery_manager">Delivery Manager</option>
                    <option value="itinerary_manager">Sales Itinerary Manager</option>
                    <option value="super_admin">System Administrator</option>
                  </select>
                </div>
                
                <div>
                  <label className="block text-sm font-medium text-[var(--foreground)] mb-2">Status</label>
                  <select
                    value={formData.status}
                    onChange={(e) => setFormData({...formData, status: e.target.value})}
                    className="w-full px-4 py-2.5 border border-[var(--input-border)] rounded-lg bg-[var(--input)] text-[var(--foreground)] focus:outline-none focus:ring-2 focus:ring-[var(--primary)] transition-all duration-200 text-sm"
                  >
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </div>
                
                <div className="flex gap-3 pt-4">
                  <Button
                    onClick={showCreateModal ? handleCreateUser : handleUpdateUser}
                    className="flex-1"
                    size="sm"
                  >
                    {showCreateModal ? 'Create User' : 'Update User'}
                  </Button>
                  <Button
                    onClick={() => { 
                      setShowCreateModal(false); 
                      setShowEditModal(false); 
                      resetForm(); 
                    }}
                    variant="outline"
                    className="flex-1"
                    size="sm"
                  >
                    Cancel
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        )}

        {/* Delete Confirmation Modal */}
        {showDeleteConfirm && (
          <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
            <Card className="w-full max-w-sm shadow-xl">
              <CardHeader className="border-b border-[var(--border)]">
                <CardTitle className="text-lg">Confirm Delete</CardTitle>
              </CardHeader>
              <CardContent className="p-6">
                <p className="text-[var(--muted-foreground)] mb-6 text-sm">
                  Are you sure you want to delete user <strong className="text-[var(--foreground)]">{selectedUser?.name}</strong>? 
                  This action cannot be undone.
                </p>
                <div className="flex gap-3">
                  <Button
                    onClick={handleDeleteUser}
                    variant="destructive"
                    className="flex-1"
                    size="sm"
                  >
                    Delete User
                  </Button>
                  <Button
                    onClick={() => { 
                      setShowDeleteConfirm(false); 
                      setSelectedUser(null); 
                    }}
                    variant="outline"
                    className="flex-1"
                    size="sm"
                  >
                    Cancel
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>
        )}
      </div>
    </ProtectedRoute>
  )
}

export default AdminDashboard