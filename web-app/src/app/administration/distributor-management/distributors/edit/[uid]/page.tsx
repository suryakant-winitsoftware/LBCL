'use client';

import React, { useState, useEffect } from 'react';
import { useRouter, useParams } from 'next/navigation';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { distributorService, IDistributorMasterView } from '@/services/distributor.service';

export default function EditDistributorPage() {
  const router = useRouter();
  const params = useParams();
  const uid = params.uid as string;

  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [distributorData, setDistributorData] = useState<IDistributorMasterView | null>(null);

  useEffect(() => {
    const fetchDistributor = async () => {
      try {
        setLoading(true);
        const data = await distributorService.getDistributorByUID(uid);
        setDistributorData(data);
      } catch (error) {
        console.error('Error fetching distributor:', error);
        toast.error('Failed to load distributor details');
        router.push('/administration/distributor-management/distributors');
      } finally {
        setLoading(false);
      }
    };

    if (uid) {
      fetchDistributor();
    }
  }, [uid, router]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!distributorData) return;

    setSaving(true);
    try {
      const now = new Date().toISOString();
      const updatedData = {
        ...distributorData,
        Org: {
          ...distributorData.Org,
          ModifiedTime: now,
        },
        Store: {
          ...distributorData.Store,
          ModifiedTime: now,
        },
      };

      await distributorService.createDistributor(updatedData);
      toast.success('Distributor updated successfully');
      router.push('/administration/distributor-management/distributors');
    } catch (error: any) {
      console.error('Error updating distributor:', error);
      toast.error(error?.message || 'Failed to update distributor');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  if (!distributorData) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <p className="text-muted-foreground">Distributor not found</p>
      </div>
    );
  }

  return (
    <div className="space-y-6 p-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" onClick={() => router.back()}>
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Edit Distributor</h1>
            <p className="text-muted-foreground">Update distributor information</p>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => router.back()}>
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={saving}>
            <Save className="mr-2 h-4 w-4" />
            {saving ? 'Saving...' : 'Save Changes'}
          </Button>
        </div>
      </div>

      <div className="rounded-lg border p-8 bg-card">
        <p className="text-center text-muted-foreground">
          Edit functionality will be implemented with full form similar to create page
        </p>
        <div className="mt-4 space-y-2">
          <p><strong>UID:</strong> {distributorData.Org.UID}</p>
          <p><strong>Name:</strong> {distributorData.Org.Name}</p>
          <p><strong>Code:</strong> {distributorData.Org.Code}</p>
        </div>
      </div>
    </div>
  );
}
