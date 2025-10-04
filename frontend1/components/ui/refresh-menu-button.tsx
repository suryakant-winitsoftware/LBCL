'use client'

import { useState } from 'react'
import { RefreshCw } from 'lucide-react'
import { Button } from '@/components/ui/button'
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from '@/components/ui/tooltip'
import { usePermissions } from '@/providers/permission-provider'
import { useToast } from '@/components/ui/use-toast'

interface RefreshMenuButtonProps {
  className?: string
  variant?: 'default' | 'outline' | 'ghost' | 'secondary'
  size?: 'default' | 'sm' | 'lg' | 'icon'
}

export function RefreshMenuButton({ 
  className,
  variant = 'ghost',
  size = 'icon' 
}: RefreshMenuButtonProps) {
  const { refreshMenu } = usePermissions()
  const { toast } = useToast()
  const [isRefreshing, setIsRefreshing] = useState(false)

  const handleRefresh = async () => {
    setIsRefreshing(true)
    try {
      await refreshMenu()
      toast({
        title: 'Menu Refreshed',
        description: 'The menu has been updated with the latest changes.',
      })
    } catch (error) {
      toast({
        title: 'Refresh Failed',
        description: 'Failed to refresh the menu. Please try again.',
        variant: 'destructive'
      })
    } finally {
      setIsRefreshing(false)
    }
  }

  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <Button
          variant={variant}
          size={size}
          className={className}
          onClick={handleRefresh}
          disabled={isRefreshing}
        >
          <RefreshCw 
            className={`h-4 w-4 ${isRefreshing ? 'animate-spin' : ''}`} 
          />
          {size !== 'icon' && <span className="ml-2">Refresh Menu</span>}
        </Button>
      </TooltipTrigger>
      <TooltipContent>
        <p>Refresh menu to see latest changes</p>
      </TooltipContent>
    </Tooltip>
  )
}