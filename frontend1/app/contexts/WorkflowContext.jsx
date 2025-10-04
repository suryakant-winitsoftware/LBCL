"use client"
import { createContext, useContext, useState, useEffect } from 'react'

const WorkflowContext = createContext()

export const useWorkflow = () => {
  const context = useContext(WorkflowContext)
  if (!context) {
    throw new Error('useWorkflow must be used within a WorkflowProvider')
  }
  return context
}

export const WorkflowProvider = ({ children }) => {
  const [completedSteps, setCompletedSteps] = useState(() => {
    // Load from localStorage on initial load
    if (typeof window !== 'undefined') {
      const saved = localStorage.getItem('workflowCompletedSteps')
      return saved ? JSON.parse(saved) : {}
    }
    return {}
  })

  const [isSubmissionSuccessful, setIsSubmissionSuccessful] = useState(false)

  useEffect(() => {
    // Save to localStorage whenever completedSteps changes
    if (typeof window !== 'undefined') {
      localStorage.setItem('workflowCompletedSteps', JSON.stringify(completedSteps))
    }
  }, [completedSteps])

  const markStepCompleted = (stepId, planId = 'default') => {
    setCompletedSteps(prev => ({
      ...prev,
      [`${planId}_${stepId}`]: true
    }))
  }

  const isStepCompleted = (stepId, planId = 'default') => {
    return !!completedSteps[`${planId}_${stepId}`]
  }

  const resetWorkflow = (planId = 'default') => {
    const newCompletedSteps = { ...completedSteps }
    Object.keys(newCompletedSteps).forEach(key => {
      if (key.startsWith(`${planId}_`)) {
        delete newCompletedSteps[key]
      }
    })
    setCompletedSteps(newCompletedSteps)
    setIsSubmissionSuccessful(false)
  }

  const markSubmissionSuccessful = () => {
    setIsSubmissionSuccessful(true)
  }

  const value = {
    completedSteps,
    markStepCompleted,
    isStepCompleted,
    resetWorkflow,
    isSubmissionSuccessful,
    markSubmissionSuccessful,
    setIsSubmissionSuccessful
  }

  return (
    <WorkflowContext.Provider value={value}>
      {children}
    </WorkflowContext.Provider>
  )
}