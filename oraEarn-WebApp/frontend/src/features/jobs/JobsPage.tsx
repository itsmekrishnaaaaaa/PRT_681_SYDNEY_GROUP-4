import { type FormEvent, useEffect, useState } from 'react';
import { createJob, deleteJob, getJobs } from '@/features/jobs/api';
import { getErrorMessage } from '@/shared/api/client';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import {
  FormErrorSummary,
  FormField,
  fieldInvalidClass,
  pickErrors,
  useFormValidation,
  validators,
} from '@/shared/form-field';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatCard } from '@/shared/components/StatCard';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/shared/ui/table';
import type { Job } from '@/shared/types';
import { money } from '@/shared/utils/dates';
import { Briefcase, Calendar, DollarSign, Plus, Trash2, TrendingUp } from 'lucide-react';

type JobFields = 'title' | 'hourlyRate' | 'dateOfJoin' | '_form';

export function JobsPage() {
  const [jobs, setJobs] = useState<Job[]>([]);
  const [form, setForm] = useState({ title: '', hourlyRate: '25', dateOfJoin: new Date().toISOString().slice(0, 10) });
  const { errors, clearError, validate, setFormError } = useFormValidation<JobFields>();

  const load = async () => setJobs(await getJobs());
  useEffect(() => { void load(); }, []);

  const avgRate = jobs.length > 0 ? jobs.reduce((s, j) => s + j.hourlyRate, 0) / jobs.length : 0;

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (
      !validate(() =>
        pickErrors([
          ['title', validators.required(form.title, 'Job title is required.')],
          ['hourlyRate', validators.minNumber(form.hourlyRate, 0.01, 'Enter a valid hourly rate.')],
          ['dateOfJoin', validators.dateRequired(form.dateOfJoin, 'Date of join is required.')],
        ]),
      )
    ) {
      return;
    }

    try {
      await createJob({ title: form.title, hourlyRate: Number(form.hourlyRate), dateOfJoin: form.dateOfJoin });
      setForm({ title: '', hourlyRate: '25', dateOfJoin: new Date().toISOString().slice(0, 10) });
      await load();
    } catch (err) {
      setFormError(getErrorMessage(err));
    }
  };

  return (
    <div className="space-y-6">
      <PageHeader
        icon={Briefcase}
        title="Jobs & employers"
        description="Add where you work and your hourly rate — every shift uses this to calculate pay automatically."
        badge={<Badge variant="secondary" className="font-normal">Step 1 of 4</Badge>}
      />

      <FeatureTip variant="accent" title="Why add jobs first?">
        Shifts, earnings, savings estimates, and Aura advice all use your job rate. One setup here powers the whole app.
      </FeatureTip>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard icon={Briefcase} label="Active jobs" value={jobs.length} hint="Employers you track" tone="violet" />
        <StatCard icon={DollarSign} label="Average rate" value={jobs.length ? `${money(avgRate)}/hr` : '—'} hint="Across all jobs" tone="emerald" />
        <StatCard icon={TrendingUp} label="Highest rate" value={jobs.length ? `${money(Math.max(...jobs.map((j) => j.hourlyRate)))}/hr` : '—'} hint="Best-paying role" tone="amber" />
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><Plus className="h-4 w-4" /> Add a job</CardTitle>
            <CardDescription>Job title, pay rate, and when you started — used for all future shifts.</CardDescription>
          </CardHeader>
          <CardContent>
            <FormErrorSummary errors={errors} />
            <form className="space-y-4" noValidate onSubmit={handleSubmit}>
              <FormField label="Job title" error={errors.title} required hint="e.g. Café barista, Warehouse picker">
                <Input
                  placeholder="Where do you work?"
                  className={fieldInvalidClass(errors.title)}
                  value={form.title}
                  onChange={(e) => {
                    setForm({ ...form, title: e.target.value });
                    clearError('title');
                  }}
                />
              </FormField>
              <FormField label="Hourly rate ($)" error={errors.hourlyRate} required hint="Before tax — used to calculate shift pay">
                <Input
                  type="number"
                  min="0"
                  step="0.01"
                  className={fieldInvalidClass(errors.hourlyRate)}
                  value={form.hourlyRate}
                  onChange={(e) => {
                    setForm({ ...form, hourlyRate: e.target.value });
                    clearError('hourlyRate');
                  }}
                />
              </FormField>
              <FormField label="Start date" error={errors.dateOfJoin} required hint="When you began this role">
                <Input
                  type="date"
                  className={fieldInvalidClass(errors.dateOfJoin)}
                  value={form.dateOfJoin}
                  onChange={(e) => {
                    setForm({ ...form, dateOfJoin: e.target.value });
                    clearError('dateOfJoin');
                  }}
                />
              </FormField>
              <Button type="submit" className="gap-2"><Plus className="h-4 w-4" /> Save job</Button>
            </form>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2"><Briefcase className="h-4 w-4" /> Your jobs</CardTitle>
            <CardDescription>Tap delete to remove — existing shifts keep their recorded pay.</CardDescription>
          </CardHeader>
          <CardContent>
            {jobs.length === 0 ? (
              <EmptyState
                icon={Briefcase}
                title="No jobs yet"
                description="Add your first employer above, then head to Shifts to log hours on the calendar."
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
                        <Button variant="ghost" size="icon" title="Remove job" onClick={() => void deleteJob(job.id).then(load)}>
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
