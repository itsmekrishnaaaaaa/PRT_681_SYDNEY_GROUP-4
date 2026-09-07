import { useCallback, useEffect, useMemo, useState } from 'react';
import { getJobs } from '@/features/jobs/api';
import { getMyAnalytics } from '@/features/dashboard/api';
import { clockIn, clockOut, createShift, deleteShift, endBreak, getActiveSession, getShifts, getShiftSummary, startBreak, updateShift } from '@/features/shifts/api';
import { getErrorMessage } from '@/shared/api/client';
import { ShiftCalendar } from '@/features/shifts/components/ShiftCalendar';
import { ShiftModal } from '@/features/shifts/components/ShiftModal';
import { EmptyState } from '@/shared/components/EmptyState';
import { FeatureTip } from '@/shared/components/FeatureTip';
import { PageHeader } from '@/shared/components/PageHeader';
import { StatCard } from '@/shared/components/StatCard';
import { VisaComplianceCard } from '@/features/shifts/components/VisaComplianceCard';
import { useShiftHub } from '@/features/shifts/hooks/useShiftHub';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/shared/ui/select';
import type { ActiveShiftSession, Job, Shift, ShiftSummary, UserAnalytics } from '@/shared/types';
import { money, startOfMonth } from '@/shared/utils/dates';
import { toShiftPayload } from '@/features/shifts/utils/shiftForm';
import { CalendarDays, Clock, Coffee, DollarSign, LogIn, LogOut as LogOutIcon, Play, Timer, Briefcase } from 'lucide-react';

export function ShiftsPage() {
  const [viewDate, setViewDate] = useState(() => startOfMonth(new Date()));
  const [jobs, setJobs] = useState<Job[]>([]);
  const [shifts, setShifts] = useState<Shift[]>([]);
  const [summary, setSummary] = useState<ShiftSummary | null>(null);
  const [activeSession, setActiveSession] = useState<ActiveShiftSession | null>(null);
  const [clockInJobId, setClockInJobId] = useState('');
  const [modalOpen, setModalOpen] = useState(false);
  const [selectedDate, setSelectedDate] = useState<Date | null>(null);
  const [editingShift, setEditingShift] = useState<Shift | null>(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [analytics, setAnalytics] = useState<UserAnalytics | null>(null);

  const monthRange = useMemo(() => {
    const from = new Date(viewDate.getFullYear(), viewDate.getMonth(), 1);
    const to = new Date(viewDate.getFullYear(), viewDate.getMonth() + 1, 0);
    return { from: from.toISOString().slice(0, 10), to: to.toISOString().slice(0, 10) };
  }, [viewDate]);

  const load = useCallback(async () => {
    const [jobList, shiftList, shiftSummary, userAnalytics] = await Promise.all([
      getJobs(), getShifts(monthRange.from, monthRange.to), getShiftSummary(monthRange.from, monthRange.to),
      getMyAnalytics(),
    ]);
    setJobs(jobList);
    setShifts(shiftList);
    setSummary(shiftSummary);
    setAnalytics(userAnalytics);
    setClockInJobId((current) => current || jobList[0]?.id || '');
    try { setActiveSession(await getActiveSession()); } catch { setActiveSession(null); }
  }, [monthRange.from, monthRange.to]);

  useEffect(() => { void load(); }, [load]);
  useShiftHub({ onCreated: () => void load(), onUpdated: () => void load(), onDeleted: () => void load() });

  return (
    <div className="space-y-6">
      <PageHeader
        icon={CalendarDays}
        title="Shift calendar"
        description="Log hours on the calendar, clock in live, or edit past shifts — pay is calculated from your job rate."
        badge={<Badge variant="secondary" className="font-normal">Step 2 of 4</Badge>}
      />

      <FeatureTip title="Student visa?" variant="accent">
        Hours in the last 14 days count toward your fortnight limit. We warn you in notifications if you&apos;re near or over 48 hrs.
      </FeatureTip>

      {error && !modalOpen && (
        <div className="rounded-lg border border-destructive/30 bg-destructive/5 px-4 py-3 text-destructive text-sm" role="alert">
          {error}
        </div>
      )}

      {analytics?.visaCompliance?.applies && (
        <VisaComplianceCard compliance={analytics.visaCompliance} />
      )}

      <Card className="border-violet-200/60 bg-gradient-to-br from-violet-50/50 to-white">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Timer className="h-5 w-5 text-violet-600" />
            Live shift session
            {activeSession && <Badge className="bg-emerald-600">On shift</Badge>}
          </CardTitle>
          <CardDescription>Clock in when you start work — breaks and clock-out create an accurate shift record.</CardDescription>
        </CardHeader>
        <CardContent>
          {activeSession ? (
            <div className="flex flex-wrap items-center gap-3">
              <div className="flex items-center gap-2 rounded-lg bg-white border px-3 py-2 text-sm">
                <Play className="h-4 w-4 text-emerald-600" />
                <span><strong>{activeSession.jobTitle}</strong> · started {activeSession.startTime.slice(0, 5)}</span>
              </div>
              {activeSession.breakStartedAt ? (
                <Button variant="secondary" className="gap-2" onClick={() => void endBreak().then(setActiveSession)}>
                  <Play className="h-4 w-4" /> End break
                </Button>
              ) : (
                <Button variant="secondary" className="gap-2" onClick={() => void startBreak().then(setActiveSession)}>
                  <Coffee className="h-4 w-4" /> Start break
                </Button>
              )}
              <Button className="gap-2" onClick={() => void clockOut().then(() => { setActiveSession(null); return load(); })}>
                <LogOutIcon className="h-4 w-4" /> Clock out
              </Button>
            </div>
          ) : jobs.length === 0 ? (
            <EmptyState
              icon={Briefcase}
              title="Add a job first"
              description="Create a job with your hourly rate, then come back to clock in or log shifts."
              action={<Button variant="outline" size="sm" onClick={() => window.location.assign('/jobs')}>Go to Jobs</Button>}
            />
          ) : (
            <div className="flex flex-wrap items-center gap-3">
              <Select value={clockInJobId} onValueChange={setClockInJobId}>
                <SelectTrigger className="w-[220px]"><SelectValue placeholder="Select job" /></SelectTrigger>
                <SelectContent>{jobs.map((j) => <SelectItem key={j.id} value={j.id}>{j.title}</SelectItem>)}</SelectContent>
              </Select>
              <Button className="gap-2" disabled={!clockInJobId} onClick={() => void clockIn(clockInJobId).then(setActiveSession).catch((e) => setError(getErrorMessage(e)))}>
                <LogIn className="h-4 w-4" /> Clock in
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      <div className="grid gap-4 sm:grid-cols-3">
        <StatCard icon={CalendarDays} label="Shifts this month" value={summary?.shiftCount ?? 0} hint="Logged in calendar" tone="violet" />
        <StatCard icon={Clock} label="Hours worked" value={`${summary?.totalHours.toFixed(1) ?? '0.0'}h`} hint="After breaks deducted" tone="blue" />
        <StatCard icon={DollarSign} label="Estimated pay" value={money(summary?.totalPay ?? 0)} hint="From job rates" tone="emerald" />
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2"><CalendarDays className="h-4 w-4" /> Monthly calendar</CardTitle>
          <CardDescription>Click any day to add a shift · click a shift block to edit or delete</CardDescription>
        </CardHeader>
        <CardContent>
          <ShiftCalendar viewDate={viewDate} shifts={shifts} activeSession={activeSession} onViewDateChange={setViewDate}
            onDayClick={(date) => { setSelectedDate(date); setEditingShift(null); setError(''); setModalOpen(true); }}
            onShiftClick={(shift) => { setSelectedDate(new Date(`${shift.workDate.slice(0, 10)}T12:00:00`)); setEditingShift(shift); setError(''); setModalOpen(true); }}
          />
        </CardContent>
      </Card>

      <ShiftModal open={modalOpen} selectedDate={selectedDate} shift={editingShift} jobs={jobs} saving={saving} error={error}
        onClose={() => { if (!saving) { setModalOpen(false); setSelectedDate(null); setEditingShift(null); setError(''); } }}
        onSubmit={async (values) => {
          setSaving(true);
          setError('');
          try {
            const payload = toShiftPayload(values);
            if (editingShift) {
              await updateShift(editingShift.id, payload);
            } else {
              await createShift(payload);
            }
            await load();
            setModalOpen(false);
            setSelectedDate(null);
            setEditingShift(null);
            setError('');
          } catch (err) {
            setError(getErrorMessage(err, 'Could not save shift.'));
          } finally {
            setSaving(false);
          }
        }}
        onDelete={editingShift ? async () => {
          setSaving(true);
          setError('');
          try {
            await deleteShift(editingShift.id);
            await load();
            setModalOpen(false);
            setSelectedDate(null);
            setEditingShift(null);
          } catch (err) {
            setError(getErrorMessage(err, 'Could not delete shift.'));
          } finally {
            setSaving(false);
          }
        } : undefined}
      />
    </div>
  );
}
