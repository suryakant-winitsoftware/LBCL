"use client"

import { toast as sonnerToast } from "sonner"

export interface ToastProps {
  title?: string
  description?: string | React.ReactNode
  variant?: "default" | "destructive"
}

export function useToast() {
  const toast = ({ title, description, variant }: ToastProps) => {
    const message = title || ""
    const options = {
      description: description as string | React.ReactNode,
    }

    if (variant === "destructive") {
      sonnerToast.error(message, options)
    } else {
      sonnerToast.success(message, options)
    }
  }

  return { toast }
}