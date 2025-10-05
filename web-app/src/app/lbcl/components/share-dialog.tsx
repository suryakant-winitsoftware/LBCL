"use client"

import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { MessageSquare, Mail, MessageCircle } from "lucide-react"

interface ShareDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function ShareDialog({ open, onOpenChange }: ShareDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">Share Delivery Plan</DialogTitle>
        </DialogHeader>

        <div className="py-4 sm:py-6">
          <p className="text-sm text-gray-600 mb-6">
            The delivery plan will be automatically sent to a group of recipients who are part of the list
          </p>

          <div className="mb-6">
            <h3 className="font-medium mb-4">Share Files Via</h3>
            <div className="flex justify-center gap-6 sm:gap-12">
              <button className="flex flex-col items-center gap-2 hover:opacity-70 transition-opacity">
                <div className="w-12 h-12 sm:w-16 sm:h-16 flex items-center justify-center">
                  <MessageSquare className="w-8 h-8 sm:w-12 sm:h-12" />
                </div>
              </button>
              <button className="flex flex-col items-center gap-2 hover:opacity-70 transition-opacity">
                <div className="w-12 h-12 sm:w-16 sm:h-16 flex items-center justify-center">
                  <Mail className="w-8 h-8 sm:w-12 sm:h-12" />
                </div>
              </button>
              <button className="flex flex-col items-center gap-2 hover:opacity-70 transition-opacity">
                <div className="w-12 h-12 sm:w-16 sm:h-16 flex items-center justify-center">
                  <MessageCircle className="w-8 h-8 sm:w-12 sm:h-12" />
                </div>
              </button>
            </div>
          </div>
        </div>

        <Button onClick={() => onOpenChange(false)} className="w-full bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-12">
          DONE
        </Button>
      </DialogContent>
    </Dialog>
  )
}
