import PhysicalCountPage from "@/components/physical-count-page"

export default function StockReceivingDetailsPage({
  params,
}: {
  params: { id: string }
}) {
  return <PhysicalCountPage deliveryId={params.id} />
}
