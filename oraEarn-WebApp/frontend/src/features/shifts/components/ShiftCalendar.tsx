import type { ActiveShiftSession, Shift } from '@/shared/types';
import { addMonths, buildCalendarDays, formatDateKey, money, weekdayLabels } from '@/shared/utils/dates';
import { Button } from '@/shared/ui/button';
import { cn } from '@/shared/lib/utils';

type ShiftCalendarProps = {
  viewDate: Date;
  shifts: Shift[];
  activeSession?: ActiveShiftSession | null;
  onViewDateChange: (date: Date) => void;
  onDayClick: (date: Date) => void;
  onShiftClick: (shift: Shift) => void;
};

function getShiftEndDateTime(shift: Shift): Date {
  const [y, m, d] = shift.workDate.slice(0, 10).split('-').map(Number);
  const [sh, sm] = shift.startTime.slice(0, 5).split(':').map(Number);
  const [eh, em] = shift.endTime.slice(0, 5).split(':').map(Number);

  const start = new Date(y, m - 1, d, sh, sm);
  const end = new Date(y, m - 1, d, eh, em);
  if (end <= start) {
    end.setDate(end.getDate() + 1);
  }
  return end;
}

/** Shift is done once its end time has passed. */
export function isShiftDone(shift: Shift): boolean {
  return getShiftEndDateTime(shift) <= new Date();
}

export function ShiftCalendar({ viewDate, shifts, activeSession, onViewDateChange, onDayClick, onShiftClick }: ShiftCalendarProps) {
  const days = buildCalendarDays(viewDate);
  const currentMonth = viewDate.getMonth();
  const shiftsByDay = shifts.reduce<Record<string, Shift[]>>((acc, shift) => {
    const key = shift.workDate.slice(0, 10);
    acc[key] ??= [];
    acc[key].push(shift);
    return acc;
  }, {});

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <Button variant="outline" onClick={() => onViewDateChange(addMonths(viewDate, -1))}>Previous</Button>
        <h3 className="text-lg font-semibold">{viewDate.toLocaleDateString(undefined, { month: 'long', year: 'numeric' })}</h3>
        <Button variant="outline" onClick={() => onViewDateChange(addMonths(viewDate, 1))}>Next</Button>
      </div>

      <div className="flex flex-wrap gap-4 text-xs text-muted-foreground">
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-4 w-4 rounded bg-emerald-100 ring-1 ring-emerald-200" />
          Day with completed shift(s)
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-4 w-4 rounded bg-red-100 ring-1 ring-red-200" />
          Day with shift not finished yet
        </span>
      </div>
      <div className="grid grid-cols-7 gap-2">
        {weekdayLabels.map((label) => (
          <div key={label} className="text-center text-xs font-medium text-muted-foreground py-1">{label}</div>
        ))}

        {days.map((day) => {
          const key = formatDateKey(day);
          const dayShifts = shiftsByDay[key] ?? [];
          const outsideMonth = day.getMonth() !== currentMonth;
          const today = formatDateKey(new Date()) === key;
          const liveSessionToday =
            activeSession && activeSession.shiftDate.slice(0, 10) === key;
          const hasShifts = dayShifts.length > 0 || liveSessionToday;
          const allShiftsDone =
            dayShifts.length > 0 && dayShifts.every(isShiftDone) && !liveSessionToday;
          const dayNotDone = liveSessionToday || dayShifts.some((s) => !isShiftDone(s));

          return (
            <button
              key={key}
              type="button"
              onClick={() => onDayClick(day)}
              className={cn(
                'min-h-24 rounded-lg border p-2 text-left transition',
                !hasShifts && 'hover:border-primary/50 hover:bg-accent/40',
                hasShifts && allShiftsDone && 'border-emerald-300 bg-emerald-100 hover:bg-emerald-100/90',
                hasShifts && dayNotDone && 'border-red-300 bg-red-100 hover:bg-red-100/90',
                outsideMonth && 'opacity-50',
                today && !hasShifts && 'border-primary ring-1 ring-primary/30',
                today && allShiftsDone && 'ring-2 ring-emerald-400/50',
                today && dayNotDone && 'ring-2 ring-red-400/50',
              )}
            >
              <div className={cn(
                'mb-1 text-sm font-semibold',
                allShiftsDone && 'text-emerald-900',
                dayNotDone && hasShifts && 'text-red-900',
              )}>
                {day.getDate()}
              </div>
              <div className="space-y-1">
                {liveSessionToday && (
                  <div className="truncate rounded bg-white/60 px-1.5 py-0.5 font-medium text-[10px] text-red-900 leading-tight">
                    {activeSession.startTime.slice(0, 5)} · Live
                  </div>
                )}
                {dayShifts.map((shift) => (
                  <div
                    key={shift.id}
                    onClick={(e) => { e.stopPropagation(); onShiftClick(shift); }}
                    className="truncate rounded bg-white/60 px-1.5 py-0.5 text-[10px] font-medium leading-tight text-foreground/90"
                    title={isShiftDone(shift) ? 'Shift completed' : 'Shift not finished yet'}
                  >
                    {shift.startTime.slice(0, 5)} · {money(shift.totalEarned)}
                  </div>
                ))}
              </div>
            </button>
          );
        })}
      </div>
    </div>
  );
}
