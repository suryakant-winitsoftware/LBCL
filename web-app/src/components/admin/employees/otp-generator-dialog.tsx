"use client"

import { useState, useEffect, useCallback } from "react"
import { generateTOTP } from '@/utils/totp'
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogFooter } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Card, CardContent } from "@/components/ui/card"
import { Copy, Clock, Shield } from "lucide-react"
import { useToast } from "@/components/ui/use-toast"

interface OTPGeneratorDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  employeeName: string
  encryptedPassword: string // The encrypted password value from employee data
}

export function OTPGeneratorDialog({
  open,
  onOpenChange,
  employeeName,
  encryptedPassword,
}: OTPGeneratorDialogProps) {
  const { toast } = useToast()
  const [currentOTP, setCurrentOTP] = useState("")
  const [timeRemaining, setTimeRemaining] = useState(120) // 2 minutes = 120 seconds

  // Use ENCRIPTED_PASSWORD from emp table as the secret for OTP generation
  const secret = encryptedPassword

  // Function to generate OTP using custom algorithm (matches mobile app)
  const generateOTP = useCallback(() => {
    try {
      if (!secret) {
        console.error("No secret provided for OTP generation")
        return
      }

      // Generate TOTP with 2 minute (120 second) time window
      // Uses ENCRIPTED_PASSWORD from emp table as ASCII string
      const otp = generateTOTP(secret, Date.now(), 120)
      setCurrentOTP(otp)

      // Reset timer
      const now = Math.floor(Date.now() / 1000)
      const remaining = 120 - (now % 120)
      setTimeRemaining(remaining)
    } catch (error) {
      console.error("Failed to generate OTP:", error)
      toast({
        title: "Error",
        description: "Failed to generate OTP. Please try again.",
        variant: "destructive",
      })
    }
  }, [secret, toast])

  // Generate OTP when dialog opens
  useEffect(() => {
    if (open && secret) {
      generateOTP()
    }
  }, [open, secret, generateOTP])

  // Update countdown timer every second
  useEffect(() => {
    if (!open) return

    const interval = setInterval(() => {
      const now = Math.floor(Date.now() / 1000)
      const remaining = 120 - (now % 120)
      setTimeRemaining(remaining)

      // Regenerate OTP when time expires (at the start of new 2-minute window)
      if (remaining === 120) {
        generateOTP()
      }
    }, 1000)

    return () => clearInterval(interval)
  }, [open, generateOTP])

  const handleCopyOTP = () => {
    navigator.clipboard.writeText(currentOTP)
    toast({
      title: "Copied",
      description: "OTP copied to clipboard",
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            OTP Generator
          </DialogTitle>
          <DialogDescription>
            Generate time-based one-time password for <strong>{employeeName}</strong>
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-4 py-4">
          {/* Current OTP Display */}
          <Card className="bg-gradient-to-br from-blue-50 to-indigo-50 border-blue-200">
            <CardContent className="pt-6">
              <div className="text-center space-y-2">
                <Label className="text-sm text-gray-600">Current OTP</Label>
                <div className="flex items-center justify-center gap-2">
                  <div className="text-4xl font-mono font-bold tracking-wider text-blue-600">
                    {currentOTP || "------"}
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={handleCopyOTP}
                    className="h-8 w-8 p-0"
                  >
                    <Copy className="h-4 w-4" />
                  </Button>
                </div>

                {/* Time Remaining */}
                <div className="flex items-center justify-center gap-2 text-sm">
                  <Clock className="h-4 w-4 text-gray-500" />
                  <span className="text-gray-600">
                    Valid for: <strong className={timeRemaining <= 30 ? "text-red-500" : "text-green-600"}>
                      {Math.floor(timeRemaining / 60)}:{String(timeRemaining % 60).padStart(2, '0')}
                    </strong>
                  </span>
                  <div className="ml-2 flex-1 max-w-[100px] bg-gray-200 rounded-full h-2">
                    <div
                      className={`h-2 rounded-full transition-all ${
                        timeRemaining <= 30 ? "bg-red-500" : "bg-green-500"
                      }`}
                      style={{ width: `${(timeRemaining / 120) * 100}%` }}
                    />
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

        </div>

        <DialogFooter>
          <Button
            variant="default"
            onClick={() => onOpenChange(false)}
          >
            Close
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
