import { useCallback, useEffect, useState } from 'react';
import {
  aiReviewRegistration,
  approveRegistration,
  getPendingRegistrations,
  openRegistrationDocument,
  rejectRegistration,
} from '@/features/admin/api';
import { getErrorMessage } from '@/shared/api/client';
import { Badge } from '@/shared/ui/badge';
import { Button } from '@/shared/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { Input } from '@/shared/ui/input';
import { Label } from '@/shared/ui/label';
import type { PendingRegistration, RegistrationAiReview } from '@/shared/types';
import { identityDocumentLabel } from '@/shared/types';
import { Bot, Check, Eye, FileText, X } from 'lucide-react';

export function RegistrationApprovals() {
  const [pending, setPending] = useState<PendingRegistration[]>([]);
  const [selectedId, setSelectedId] = useState<string | null>(null);
  const [notes, setNotes] = useState('');
  const [aiReview, setAiReview] = useState<RegistrationAiReview | null>(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const load = useCallback(async () => {
    const list = await getPendingRegistrations();
    setPending(list);
    return list;
  }, []);

  // Pre-select the oldest registration on first load only; after a decision the selection stays cleared.
  useEffect(() => {
    void load().then((list) => {
      if (list.length > 0) setSelectedId((current) => current ?? list[0].id);
    });
  }, [load]);

  const selected = pending.find((p) => p.id === selectedId) ?? null;

  const runAi = async (userId: string) => {
    setError('');
    setLoading(true);
    try {
      const review = await aiReviewRegistration(userId);
      setAiReview(review);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  const decide = async (userId: string, action: 'approve' | 'reject') => {
    setError('');
    setLoading(true);
    try {
      if (action === 'approve') await approveRegistration(userId, notes);
      else await rejectRegistration(userId, notes);
      setNotes('');
      setAiReview(null);
      setSelectedId(null);
      await load();
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-4">
      {error && <p className="text-sm text-destructive">{error}</p>}

      {pending.length === 0 ? (
        <Card><CardContent className="pt-6 text-sm text-muted-foreground">No pending registrations.</CardContent></Card>
      ) : (
        <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
          <Card>
            <CardHeader className="pb-3">
              <CardTitle className="text-base">Pending ({pending.length})</CardTitle>
            </CardHeader>
            <CardContent className="space-y-1">
              {pending.map((item) => (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => { setSelectedId(item.id); setAiReview(null); }}
                  className={`w-full rounded-md px-3 py-2 text-left text-sm ${selectedId === item.id ? 'bg-muted font-medium' : 'hover:bg-muted/60'}`}
                >
                  <p>{item.firstName} {item.lastName}</p>
                  <p className="text-xs text-muted-foreground">{item.email}</p>
                </button>
              ))}
            </CardContent>
          </Card>

          {selected && (
            <Card>
              <CardHeader>
                <CardTitle>{selected.firstName} {selected.lastName}</CardTitle>
                <CardDescription>Manual admin approval required · AI can recommend approve/reject</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-3 sm:grid-cols-2 text-sm">
                  <div><span className="text-muted-foreground">Email:</span> {selected.email}</div>
                  <div><span className="text-muted-foreground">Phone:</span> {selected.phoneNumber}</div>
                  <div><span className="text-muted-foreground">DOB:</span> {selected.dateOfBirth ? new Date(selected.dateOfBirth).toLocaleDateString() : '—'}</div>
                  <div><span className="text-muted-foreground">Nationality:</span> {selected.nationality}</div>
                  <div><span className="text-muted-foreground">Visa type:</span> {selected.visaType}</div>
                  <div><span className="text-muted-foreground">Visa status:</span> {selected.visaStatus === 'TR' ? 'TR (Temporary Resident)' : selected.visaStatus}</div>
                  <div><span className="text-muted-foreground">Working rights:</span> {selected.workingRights}</div>
                  <div><span className="text-muted-foreground">Visa expiry:</span> {selected.visaExpiryDate ? new Date(selected.visaExpiryDate).toLocaleDateString() : '—'}</div>
                  <div><span className="text-muted-foreground">ID document:</span> {identityDocumentLabel(selected.identityDocumentType)}</div>
                  <div className="flex items-center gap-2">
                    <span className="text-muted-foreground">Document:</span>
                    {selected.hasVisaDocument ? (
                      <Button type="button" size="sm" variant="outline" onClick={() => void openRegistrationDocument(selected.id)}>
                        <Eye className="h-3 w-3 mr-1" /> {selected.visaDocumentFileName ?? 'View PDF/image'}
                      </Button>
                    ) : <Badge variant="outline">Missing</Badge>}
                  </div>
                </div>

                {selected.aiReviewSummary && (
                  <p className="text-sm border rounded-md p-3 bg-muted/40">{selected.aiReviewSummary}</p>
                )}

                {aiReview && (
                  <div className="rounded-lg border p-4 space-y-3 text-sm">
                    <div className="flex items-center gap-2 font-medium">
                      <Bot className="h-4 w-4" /> AI recommendation: {aiReview.recommendation}
                      {aiReview.suggestApprove ? <Badge>Looks good</Badge> : <Badge variant="outline">Review carefully</Badge>}
                    </div>
                    <p>{aiReview.summary}</p>
                    {aiReview.checks.length > 0 && (
                      <ul className="list-disc pl-5 text-muted-foreground">{aiReview.checks.map((c) => <li key={c}>{c}</li>)}</ul>
                    )}
                    {aiReview.flags.length > 0 && (
                      <ul className="list-disc pl-5 text-destructive">{aiReview.flags.map((f) => <li key={f}>{f}</li>)}</ul>
                    )}
                  </div>
                )}

                <div className="space-y-2">
                  <Label>Admin notes (optional)</Label>
                  <Input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Reason for approve/reject" />
                </div>

                <div className="flex flex-wrap gap-2">
                  <Button type="button" variant="secondary" disabled={loading} onClick={() => void runAi(selected.id)}>
                    <Bot className="h-4 w-4" /> Run AI review
                  </Button>
                  <Button type="button" disabled={loading} onClick={() => void decide(selected.id, 'approve')}>
                    <Check className="h-4 w-4" /> Approve
                  </Button>
                  <Button type="button" variant="destructive" disabled={loading} onClick={() => void decide(selected.id, 'reject')}>
                    <X className="h-4 w-4" /> Reject
                  </Button>
                  {selected.hasVisaDocument && (
                    <Button type="button" variant="outline" onClick={() => void openRegistrationDocument(selected.id)}>
                      <FileText className="h-4 w-4" /> Open visa doc
                    </Button>
                  )}
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      )}
    </div>
  );
}
