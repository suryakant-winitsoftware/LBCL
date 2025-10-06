"use client"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"

interface SignatureDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function SignatureDialog({ open, onOpenChange }: SignatureDialogProps) {
  const [notes, setNotes] = useState("")

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-lg sm:text-xl">Signature</DialogTitle>
        </DialogHeader>

        <div className="py-4 space-y-4 sm:space-y-6">
          {/* Signatures */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-medium">Logistic Signature</label>
                <button className="text-xs sm:text-sm text-[#A08B5C] hover:underline">Clear</button>
              </div>
              <div className="border-2 border-gray-300 rounded-lg h-32 sm:h-40 bg-white flex items-end justify-start p-4">
                <div className="text-xs sm:text-sm text-gray-600 bg-blue-50 px-2 py-1 rounded">LBCL Logistics</div>
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-2">
                <label className="text-sm font-medium">Driver Signature</label>
                <button className="text-xs sm:text-sm text-[#A08B5C] hover:underline">Clear</button>
              </div>
              <div className="border-2 border-gray-300 rounded-lg h-32 sm:h-40 bg-white flex items-end justify-start p-4">
                <div className="text-xs sm:text-sm text-gray-600 bg-blue-50 px-2 py-1 rounded">R.M.K.P. Rathnayake</div>
              </div>
            </div>
          </div>

          {/* Notes */}
          <div>
            <div className="flex items-center justify-between mb-2">
              <label className="text-sm font-medium">Notes</label>
              <button className="text-xs sm:text-sm text-[#A08B5C] hover:underline">Clear</button>
            </div>
            <Textarea
              placeholder="Enter Here"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              className="min-h-[120px] resize-none"
            />
          </div>
        </div>

        <Button onClick={() => onOpenChange(false)} className="w-full bg-[#A08B5C] hover:bg-[#8F7A4B] text-white h-12">
          DONE
        </Button>
      </DialogContent>
    </Dialog>
  )
}
