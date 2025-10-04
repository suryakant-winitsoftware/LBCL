import { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Planogram Setup | Product Management',
  description: 'Manage product planogram configurations for optimal shelf space allocation',
};

export default function PlanogramSetupLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <>{children}</>;
}