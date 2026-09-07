import type { VisaCompliance } from '@/shared/types';
import { NotificationList, type AppNotification } from '@/features/notifications/components/NotificationList';
import { StatBar } from '@/shared/components/StatBar';
import { Badge } from '@/shared/ui/badge';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/shared/ui/card';
import { ShieldAlert } from 'lucide-react';

export function VisaComplianceCard({ compliance }: { compliance: VisaCompliance }) {
  if (!compliance.applies) return null;

  const notifications: AppNotification[] = [];
  if (compliance.isOverLimit) {
    const overBy = Math.round((compliance.fortnightHoursWorked - compliance.fortnightHourLimit) * 10) / 10;
    notifications.push({
      type: 'visa_limit_exceeded',
      severity: 'error',
      message:
        `Student visa hour limit hit: ${compliance.fortnightHoursWorked.toFixed(1)}/${compliance.fortnightHourLimit} hrs in the last 14 days. ` +
        `You are ${overBy} hrs over — reduce shifts or wait for older hours to roll off.`,
    });
  } else if (compliance.isNearLimit) {
    notifications.push({
      type: 'visa_limit_warning',
      severity: 'warning',
      message:
        `Approaching student visa limit: ${compliance.fortnightHoursWorked.toFixed(1)}/${compliance.fortnightHourLimit} hrs this fortnight. ` +
        `${compliance.remainingHours.toFixed(1)} hrs remaining.`,
    });
  }

  return (
    <Card className={compliance.isOverLimit ? 'border-destructive/50' : compliance.isNearLimit ? 'border-amber-500/50' : undefined}>
      <CardHeader className="pb-2">
        <CardTitle className="flex items-center gap-2 text-base">
          <ShieldAlert className="h-4 w-4" />
          Visa work limit
          <Badge variant={compliance.isOverLimit ? 'default' : compliance.isNearLimit ? 'outline' : 'secondary'}>
            {compliance.visaStatus} · {compliance.workingRights}
          </Badge>
        </CardTitle>
        <CardDescription>
          Rolling 14-day window ({compliance.fortnightStart} → {compliance.fortnightEnd}) · Home Affairs study-period rule
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <StatBar
          label="Fortnight hours"
          value={compliance.fortnightHoursWorked}
          max={compliance.fortnightHourLimit}
          display={`${compliance.fortnightHoursWorked.toFixed(1)} / ${compliance.fortnightHourLimit} hrs`}
        />
        <p className="text-sm text-muted-foreground">
          {compliance.isOverLimit
            ? 'You cannot add more shifts in this fortnight without breaching your visa.'
            : `${compliance.remainingHours.toFixed(1)} hours left before you hit the 48 hr/fortnight cap.`}
        </p>
        <NotificationList items={notifications} />
      </CardContent>
    </Card>
  );
}
