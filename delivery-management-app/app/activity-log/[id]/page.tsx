import { ActivityLogPage } from "@/components/activity-log-page"

export default function ActivityLog({ params }: { params: { id: string } }) {
  return <ActivityLogPage deliveryPlanId={params.id} />
}
