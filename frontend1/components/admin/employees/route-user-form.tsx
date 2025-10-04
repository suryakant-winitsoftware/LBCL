"use client"

import * as React from "react"
import { useState, useEffect } from "react"
import { Button } from "../../ui/button"
import { employeeService } from "../../../app/services/user_management"

interface RouteUserFormProps {
  user?: any | null
  employeeId?: string
  onSuccess: () => void
  onCancel: () => void
  isModal?: boolean
}

export function RouteUserForm({ user, employeeId, onSuccess, onCancel, isModal = true }: RouteUserFormProps) {
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
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [employee, setEmployee] = useState<any>(null)

  useEffect(() => {
    if (employeeId) {
      fetchEmployee()
    } else if (user) {
      setFormData({
        name: user.name || '',
        loginId: user.username || '',
        password: '',
        email: user.email || '',
        phone: user.phone || '',
        role: user.role || 'stock_manager',
        status: user.status || 'Active',
        authType: 'Regular'
      })
    }
  }, [employeeId, user])

  const fetchEmployee = async () => {
    if (!employeeId) return
    
    try {
      setLoading(true)
      const data = await employeeService.getEmployeeById(employeeId)
      setEmployee(data)
      
      const emp = data.Emp || data.emp
      const empInfo = data.EmpInfo || data.empInfo
      
      if (emp) {
        setFormData({
          name: emp.Name || emp.name || '',
          loginId: emp.LoginId || emp.loginId || '',
          password: '',
          email: empInfo?.Email || empInfo?.email || '',
          phone: empInfo?.Phone || empInfo?.phone || '',
          role: emp.AuthType || emp.authType || 'stock_manager',
          status: emp.Status || emp.status || 'Active',
          authType: 'Regular'
        })
      }
    } catch (err) {
      console.error('Failed to fetch employee:', err)
      setError('Failed to load employee data')
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!formData.name || !formData.loginId) {
      setError('Please fill in all required fields')
      return
    }

    if (!employeeId && !formData.password) {
      setError('Password is required for new users')
      return
    }

    setLoading(true)
    setError('')

    try {
      if (employeeId) {
        // Update existing employee
        const empDTO = {
          emp: {
            ...employee?.emp,
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
          await employeeService.resetPassword(employeeId, formData.password)
        }
      } else {
        // Create new employee
        const empUID = `EMP_${Date.now()}`
        const empDTO = {
          emp: {
            uid: empUID,
            code: formData.loginId.toUpperCase(),
            name: formData.name,
            loginId: formData.loginId,
            status: formData.status,
            authType: formData.role,
            password: formData.password,
            actionType: 'Insert',
            createdBy: 'ADMIN',
            modifiedBy: 'ADMIN',
            createdTime: new Date().toISOString(),
            modifiedTime: new Date().toISOString()
          },
          empInfo: {
            uid: `EI_${Date.now()}`,
            empUID: empUID,
            email: formData.email,
            phone: formData.phone,
            startDate: new Date().toISOString(),
            canHandleStock: formData.role === 'stock_manager',
            actionType: 'Insert'
          },
          jobPosition: {
            uid: `JP_${Date.now()}`,
            empUID: empUID,
            department: mapSystem(formData.role),
            userRoleUID: formData.role,
            hasEOT: false,
            collectionLimit: 0,
            actionType: 'Insert'
          }
        }

        await employeeService.createEmployee(empDTO)
      }

      onSuccess()
    } catch (err: any) {
      console.error('Failed to save employee:', err)
      setError(err.message || 'Failed to save employee')
    } finally {
      setLoading(false)
    }
  }

  const mapSystem = (role: string) => {
    switch(role) {
      case 'super_admin': return 'admin'
      case 'delivery_manager': return 'delivery'
      case 'itinerary_manager': return 'itinerary'
      case 'stock_manager': return 'stock'
      default: return 'stock'
    }
  }

  if (loading && employeeId) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        Loading employee data...
      </div>
    )
  }

  return (
    <div>
      {error && (
        <div style={{
          marginBottom: '16px',
          padding: '12px',
          backgroundColor: '#ffffff',
          border: '1px solid #000000',
          borderRadius: '4px',
          color: '#000000'
        }}>
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Name *
          </label>
          <input
            type="text"
            value={formData.name}
            onChange={(e) => setFormData({...formData, name: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
            required
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Login ID *
          </label>
          <input
            type="text"
            value={formData.loginId}
            onChange={(e) => setFormData({...formData, loginId: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
            required
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            {employeeId ? 'New Password (leave empty to keep current)' : 'Password *'}
          </label>
          <input
            type="password"
            value={formData.password}
            onChange={(e) => setFormData({...formData, password: e.target.value})}
            placeholder={employeeId ? "Enter new password if you want to change it" : "Enter password"}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
            required={!employeeId}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Email
          </label>
          <input
            type="email"
            value={formData.email}
            onChange={(e) => setFormData({...formData, email: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Phone
          </label>
          <input
            type="text"
            value={formData.phone}
            onChange={(e) => setFormData({...formData, phone: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Role *
          </label>
          <select
            value={formData.role}
            onChange={(e) => setFormData({...formData, role: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
            required
          >
            <option value="stock_manager">Stock Manager</option>
            <option value="delivery_manager">Delivery Manager</option>
            <option value="itinerary_manager">Sales Itinerary Manager</option>
            <option value="super_admin">System Administrator</option>
          </select>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '4px', fontSize: '14px', fontWeight: '600', color: '#000000' }}>
            Status
          </label>
          <select
            value={formData.status}
            onChange={(e) => setFormData({...formData, status: e.target.value})}
            style={{
              width: '100%',
              padding: '8px',
              border: '1px solid #000000',
              borderRadius: '4px',
              fontSize: '14px'
            }}
          >
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
        </div>

        <div style={{ display: 'flex', gap: '8px', marginTop: '16px' }}>
          <Button type="submit" disabled={loading}>
            {loading ? 'Saving...' : (employeeId ? 'Update User' : 'Create User')}
          </Button>
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
        </div>
      </form>
    </div>
  )
}