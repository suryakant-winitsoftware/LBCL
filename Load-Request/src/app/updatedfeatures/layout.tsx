import { ReactNode } from "react"

interface UpdatedFeaturesLayoutProps {
  children: ReactNode
}

export default function UpdatedFeaturesLayout({ children }: UpdatedFeaturesLayoutProps) {
  return (
    <div className="h-full overflow-hidden">
      <main className="w-full h-full">
        {children}
      </main>
    </div>
  )
}

export const metadata = {
  title: "Updated Features - MULTIPLEX",
  description: "Enhanced features for MULTIPLEX system",
}