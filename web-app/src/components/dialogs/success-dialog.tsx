"use client"

import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { CheckCircle2, X, Printer } from "lucide-react"

interface SuccessDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  message?: string
  completionTime?: string
  date?: string
  startTime?: string
  endTime?: string
  showPrintButton?: boolean
  onPrint?: () => void
  onDone?: () => void
}

export function SuccessDialog({
  open,
  onOpenChange,
  title,
  message,
  completionTime,
  date,
  startTime,
  endTime,
  showPrintButton = false,
  onPrint,
  onDone
}: SuccessDialogProps) {
  const handleDone = () => {
    onOpenChange(false)
    if (onDone) {
      onDone()
    }
  }

  const handlePrint = () => {
    if (onPrint) {
      onPrint()
    } else {
      window.print()
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl p-0 overflow-hidden">
        {/* Header */}
        <DialogTitle className="flex items-center justify-between px-6 py-4 border-b">
          <h2 className="text-2xl font-bold">Success</h2>
          <button
            onClick={() => onOpenChange(false)}
            className="p-2 hover:bg-gray-100 rounded-full transition-colors"
          >
            <X className="w-6 h-6" />
          </button>
        </DialogTitle>

        {/* Content */}
        <div className="px-6 py-8">
          {/* Success Icon */}
          <div className="flex justify-center mb-6">
            <div className="w-32 h-32 bg-green-500 rounded-full flex items-center justify-center">
              <CheckCircle2 className="w-20 h-20 text-white" strokeWidth={3} />
            </div>
          </div>

          {/* Title */}
          <h3 className="text-2xl font-bold text-center mb-4">
            {title}
          </h3>

          {/* Message */}
          {message && (
            <p className="text-center text-gray-600 mb-6 text-lg">
              {message}
            </p>
          )}

          {/* Completion Time */}
          {completionTime && (
            <p className="text-center text-gray-700 font-medium mb-6">
              Completed in <span className="font-bold">{completionTime}</span>
            </p>
          )}

          {/* Time Details */}
          {(date || startTime || endTime) && (
            <div className="flex items-center justify-center gap-4 text-gray-600 mb-8">
              {date && (
                <>
                  <span>Date: <span className="font-semibold text-gray-900">{date}</span></span>
                  <span className="text-gray-400">●</span>
                </>
              )}
              {startTime && (
                <>
                  <span>Start Time: <span className="font-semibold text-gray-900">{startTime}</span></span>
                  <span className="text-gray-400">●</span>
                </>
              )}
              {endTime && (
                <span>End Time: <span className="font-semibold text-gray-900">{endTime}</span></span>
              )}
            </div>
          )}
        </div>

        {/* Footer Buttons */}
        <div className="grid grid-cols-2 border-t">
          {showPrintButton && (
            <Button
              variant="ghost"
              onClick={handlePrint}
              className="h-16 rounded-none border-r text-base font-semibold hover:bg-gray-100"
            >
              <Printer className="w-5 h-5 mr-2" />
              PRINT
            </Button>
          )}
          <Button
            onClick={handleDone}
            className={`h-16 rounded-none text-base font-semibold bg-[#A08B5C] hover:bg-[#8F7A4B] text-white ${
              showPrintButton ? '' : 'col-span-2'
            }`}
          >
            DONE
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
