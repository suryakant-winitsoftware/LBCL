export interface DashboardKPI {
  todaySales: number
  todayOrders: number
  todayCustomers: number
  averageOrderValue: number
  conversionRate: number
  mtdSales: number
  ytdSales: number
  growthPercentage: number
}

export interface SalesTrendData {
  date: string
  sales: number
  orders: number
  customers: number
  averageOrderValue: number
}

export interface FilterOptions {
  startDate?: Date
  endDate?: Date
  routeCode?: string
  customerType?: string
  productCategory?: string
  searchTerm?: string
}

export interface CustomerScorecardProps {
  customer?: any
  onClose?: () => void
}

export interface ProductScorecardProps {
  product?: any
  onClose?: () => void
}

export interface JourneyComplianceProps {
  salesmen: any[]
  selectedSalesman: string
  date: string
}

export interface TimeMotionAnalysisProps {
  salesmen: any[]
  selectedSalesman: string
  date: string
}

export * from './action-type.enum'
export * from './admin.types'
export * from './audit.types'
export * from './auth.types'
export * from './commission.types'
export * from './common.types'
export * from './permission.types'
export * from './product.types'
export * from './security.types'
export * from './session.types'
export * from './store-check.types'
export * from './store.types'
export * from './storeGroup.types'
export * from './survey-create.types'
export * from './survey.types'