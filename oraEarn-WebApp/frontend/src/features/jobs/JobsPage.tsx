import { useEffect, useState } from 'react';
import { createJob, deleteJob, getJobs, updateJob } from '@/features/jobs/api';
import { JobModal } from '@/features/jobs/components/JobModal';
import { getErrorMessage } from '@/shared/api/client';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatCard } from '@/shared/components/StatCard';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import type { Job } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { Briefcase, Calendar, DollarSign, Pencil, Plus, Trash2, TrendingUp } from 'lucide-react';

export function JobsPage() {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [modalOpen, setModalOpen] = useState(false);
  const [editing, setEditing] = useState<Job | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const load = async () => setJobs(await getJobs());
  useEffect(() => { void load(); }, []);

  const avgRate = jobs.length > 0 ? jobs.reduce((s, j) => s + j.hourlyRate, 0) / jobs.length : 0;

  const openCreate = () => {
    setEditing(null);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (job: Job) => {
    setEditing(job);
    setError('');
    setModalOpen(true);
  };

  const closeModal = () => {
    if (saving) return;
    setModalOpen(false);
    setEditing(null);
    setError('');
  };

  return (
    <div className="space-y-6">
      <PageHeader
        icon={Briefcase}
        title="Jobs & employers"
        description="Add where you work and your hourly rate — every shift uses this to calculate pay automatically."
        badge={<Badge variant="secondary" className="font-normal">Step 1 of 4</Badge>}
        action={
          <Button className="w-full gap-2 sm:w-auto" onClick={openCreate}>
            <Plus className="h-4 w-4" /> Add job
          </Button>
        }
      />

      <FeatureTip variant="accent" title="Why add jobs first?">
        Shifts, earnings, savings estimates, and Aura advice all use your job rate. One setup here powers the whole app.
      </FeatureTip>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard icon={Briefcase} label="Active jobs" value={jobs.length} hint="Employers you track" tone="violet" />
        <StatCard icon={DollarSign} label="Average rate" value={jobs.length ? `${money(avgRate)}/hr` : '—'} hint="Across all jobs" tone="emerald" />
        <StatCard icon={TrendingUp} label="Highest rate" value={jobs.length ? `${money(Math.max(...jobs.map((j) => j.hourlyRate)))}/hr` : '—'} hint="Best-paying role" tone="amber" />
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><Briefcase className="h-4 w-4" /> Your jobs</CardTitle>
          <CardDescription>Edit a job to change the rate, or delete it — existing shifts keep their recorded pay.</CardDescription>
        </CardHeader>
        <CardContent>
          {jobs.length === 0 ? (
            <EmptyState
              icon={Briefcase}
              title="No jobs yet"
              description="Add your first employer, then head to Shifts to log hours on the calendar."
              action={<Button className="gap-2" onClick={openCreate}><Plus className="h-4 w-4" /> Add job</Button>}
            />
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Title</TableHead>
                  <TableHead>Rate</TableHead>
                  <TableHead className="hidden sm:table-cell"><span className="sr-only">Joined</span><Calendar className="h-3.5 w-3.5 inline" /></TableHead>
                  <TableHead />
                </TableRow>
              </TableHeader>
              <TableBody>
                {jobs.map((job) => (
                  <TableRow key={job.id}>
                    <TableCell className="font-medium">{job.title}</TableCell>
                    <TableCell><Badge variant="secondary">{money(job.hourlyRate)}/hr</Badge></TableCell>
                    <TableCell className="hidden text-muted-foreground text-sm sm:table-cell">{new Date(job.dateOfJoin).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <div className="flex justify-end gap-1">
                        <Button variant="ghost" size="icon" title="Edit job" onClick={() => openEdit(job)}>
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" title="Remove job" onClick={() => void deleteJob(job.id).then(load)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </CardContent>
      </Card>

      <JobModal
        open={modalOpen}
        job={editing}
        saving={saving}
        error={error}
        onClose={closeModal}
        onSubmit={async (values) => {
          setSaving(true);
          setError('');
          try {
            if (editing) await updateJob(editing.id, values);
            else await createJob(values);
            await load();
            setModalOpen(false);
            setEditing(null);
          } catch (err) {
            setError(getErrorMessage(err));
          } finally {
            setSaving(false);
          }
        }}
      />
    </div>
  );
}
