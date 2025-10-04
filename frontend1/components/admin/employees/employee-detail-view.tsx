"use client"

import * as React from "react"
import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"
import { ArrowLeft, Edit, Trash2, Lock, Unlock, User } from "lucide-react"
import { Button } from "../../ui/button"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "../../ui/card"
import { employeeService } from "../../../app/services/user_management"

interface EmployeeDetailViewProps {
  employeeId: string
}

export function EmployeeDetailView({ employeeId }: EmployeeDetailViewProps) {
  const router = useRouter()
  const [employee, setEmployee] = useState<any>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    fetchEmployee()
  }, [employeeId])

  const fetchEmployee = async () => {
    try {
      setLoading(true)
      const data = await employeeService.getEmployeeById(employeeId)
      setEmployee(data)
    } catch (err: any) {
      console.error('Failed to fetch employee:', err)
      setError('Failed to load employee details')
    } finally {
      setLoading(false)
    }
  }

  const handleEdit = () => {
    router.push(`/services/user_management/${employeeId}/edit`)
  }

  const handleBack = () => {
    router.push('/user/admin/dashboard')
  }

  const handleToggleStatus = async () => {
    if (!employee) return

    try {
      const emp = employee.Emp || employee.emp
      const isActive = emp.Status === 'Active' || emp.status === 'Active'
      
      if (isActive) {
        await employeeService.deactivateEmployee(employeeId)
      } else {
        await employeeService.activateEmployee(employeeId)
      }
      
      // Refresh employee data
      fetchEmployee()
    } catch (err: any) {
      console.error('Failed to toggle status:', err)
      setError('Failed to update employee status')
    }
  }

  const handleDelete = async () => {
    if (!employee) return
    
    if (confirm('Are you sure you want to delete this employee? This action cannot be undone.')) {
      try {
        await employeeService.deleteEmployee(employeeId)
        router.push('/user/admin/dashboard')
      } catch (err: any) {
        console.error('Failed to delete employee:', err)
        setError('Failed to delete employee')
      }
    }
  }

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        Loading employee details...
      </div>
    )
  }

  if (error) {
    return (
      <div style={{
        textAlign: 'center',
        padding: '40px',
        color: '#000000'
      }}>
        {error}
        <div style={{ marginTop: '16px' }}>
          <Button onClick={handleBack} variant="outline">
            <ArrowLeft style={{ width: '16px', height: '16px', marginRight: '8px' }} />
            Back to Dashboard
          </Button>
        </div>
      </div>
    )
  }

  if (!employee) {
    return (
      <div style={{ textAlign: 'center', padding: '40px' }}>
        Employee not found
      </div>
    )
  }

  const emp = employee.Emp || employee.emp
  const empInfo = employee.EmpInfo || employee.empInfo
  const jobPosition = employee.JobPosition || employee.jobPosition

  return (
    <div style={{ padding: '24px', maxWidth: '800px', margin: '0 auto' }}>
      {/* Header */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '24px' }}>
        <div>
          <h1 style={{ fontSize: '24px', fontWeight: 'bold', color: '#000000', marginBottom: '8px' }}>
            Employee Details
          </h1>
          <p style={{ color: '#000000' }}>
            View and manage employee information
          </p>
        </div>
        <Button variant="outline" onClick={handleBack}>
          <ArrowLeft style={{ width: '16px', height: '16px', marginRight: '8px' }} />
          Back to Dashboard
        </Button>
      </div>

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

      {/* Employee Card */}
      <Card>
        <CardHeader>
          <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
            <div style={{
              width: '48px',
              height: '48px',
              backgroundColor: '#375AE6',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}>
              <User style={{ width: '24px', height: '24px', color: '#ffffff' }} />
            </div>
            <div>
              <CardTitle>{emp?.Name || emp?.name || 'Unknown User'}</CardTitle>
              <CardDescription>@{emp?.LoginId || emp?.loginId || 'unknown'}</CardDescription>
            </div>
            <div style={{ marginLeft: 'auto' }}>
              <span style={{
                padding: '4px 8px',
                backgroundColor: (emp?.Status || emp?.status) === 'Active' ? '#375AE6' : '#000000',
                color: '#ffffff',
                fontSize: '12px',
                fontWeight: '600',
                borderRadius: '2px'
              }}>
                {(emp?.Status || emp?.status || 'Unknown').toUpperCase()}
              </span>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '24px' }}>
            {/* Basic Information */}
            <div>
              <h3 style={{ fontSize: '16px', fontWeight: '600', marginBottom: '12px', color: '#000000' }}>
                Basic Information
              </h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Employee Code
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {emp?.Code || emp?.code || 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Full Name
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {emp?.Name || emp?.name || 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Login ID
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {emp?.LoginId || emp?.loginId || 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Role
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {(emp?.AuthType || emp?.authType || 'N/A').replace(/_/g, ' ')}
                  </p>
                </div>
              </div>
            </div>

            {/* Contact Information */}
            <div>
              <h3 style={{ fontSize: '16px', fontWeight: '600', marginBottom: '12px', color: '#000000' }}>
                Contact Information
              </h3>
              <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Email
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {empInfo?.Email || empInfo?.email || 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Phone
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {empInfo?.Phone || empInfo?.phone || 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Start Date
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {empInfo?.StartDate ? new Date(empInfo.StartDate).toLocaleDateString() : 'N/A'}
                  </p>
                </div>
                <div>
                  <span style={{ fontSize: '12px', color: '#000000', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
                    Department
                  </span>
                  <p style={{ margin: 0, color: '#000000', fontWeight: '500' }}>
                    {jobPosition?.Department || jobPosition?.department || 'N/A'}
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Actions */}
          <div style={{ marginTop: '32px', paddingTop: '24px', borderTop: '1px solid #000000' }}>
            <h3 style={{ fontSize: '16px', fontWeight: '600', marginBottom: '16px', color: '#000000' }}>
              Actions
            </h3>
            <div style={{ display: 'flex', gap: '8px' }}>
              <Button onClick={handleEdit}>
                <Edit style={{ width: '16px', height: '16px', marginRight: '8px' }} />
                Edit Employee
              </Button>
              <Button
                variant="outline"
                onClick={handleToggleStatus}
              >
                {(emp?.Status || emp?.status) === 'Active' ? (
                  <>
                    <Lock style={{ width: '16px', height: '16px', marginRight: '8px' }} />
                    Deactivate
                  </>
                ) : (
                  <>
                    <Unlock style={{ width: '16px', height: '16px', marginRight: '8px' }} />
                    Activate
                  </>
                )}
              </Button>
              <Button
                variant="destructive"
                onClick={handleDelete}
              >
                <Trash2 style={{ width: '16px', height: '16px', marginRight: '8px' }} />
                Delete
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}