import StockReceivingActivityLog from "@/components/stock-receiving-activity-log"

export default async function ActivityLogPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params
  return <StockReceivingActivityLog deliveryId={id} />
}
