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
      <div className="flex items-center justify-between gap-2">
        <Button variant="outline" size="sm" className="px-3" onClick={() => onViewDateChange(addMonths(viewDate, -1))}>
          Prev
        </Button>
        <h3 className="min-w-0 truncate text-center text-base font-semibold sm:text-lg">
          {viewDate.toLocaleDateString(undefined, { month: 'long', year: 'numeric' })}
        </h3>
        <Button variant="outline" size="sm" className="px-3" onClick={() => onViewDateChange(addMonths(viewDate, 1))}>
          Next
        </Button>
      </div>

      <div className="flex flex-wrap gap-x-4 gap-y-1 text-xs text-muted-foreground">
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-3.5 w-3.5 rounded bg-foreground ring-1 ring-foreground" />
          Completed
        </span>
        <span className="flex items-center gap-1.5">
          <span className="inline-block h-3.5 w-3.5 rounded bg-muted ring-1 ring-border" />
          Not finished
        </span>
      </div>
      <div className="-mx-1 overflow-x-auto px-1">
        <div className="grid min-w-[20rem] grid-cols-7 gap-0.5 sm:min-w-0 sm:gap-2">
        {weekdayLabels.map((label) => (
          <div key={label} className="py-1 text-center text-[10px] font-medium text-muted-foreground sm:text-xs">
            <span className="sm:hidden">{label[0]}</span>
            <span className="hidden sm:inline">{label}</span>
          </div>
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
          const extraCount = dayShifts.length + (liveSessionToday ? 1 : 0);

          return (
            <button
              key={key}
              type="button"
              onClick={() => onDayClick(day)}
              className={cn(
                'min-h-11 rounded-md border p-1 text-left transition sm:min-h-20 sm:rounded-lg sm:p-2 md:min-h-24',
                !hasShifts && 'hover:border-primary/50 hover:bg-accent/40',
                hasShifts && allShiftsDone && 'border-foreground bg-foreground text-background hover:bg-foreground/90',
                hasShifts && dayNotDone && 'border-border bg-muted hover:bg-accent',
                outsideMonth && 'opacity-50',
                today && !hasShifts && 'border-primary ring-1 ring-primary/30',
                today && allShiftsDone && 'ring-2 ring-foreground/40',
                today && dayNotDone && 'ring-2 ring-muted-foreground/40',
              )}
            >
              <div className={cn(
                'text-xs font-semibold sm:mb-1 sm:text-sm',
                allShiftsDone && 'text-background',
                dayNotDone && hasShifts && 'text-foreground',
              )}>
                {day.getDate()}
              </div>
              <div className="hidden space-y-1 sm:block">
                {liveSessionToday && (
                  <div className="truncate rounded bg-background/70 px-1.5 py-0.5 font-medium text-[10px] text-foreground leading-tight">
                    {activeSession.startTime.slice(0, 5)} · Live
                  </div>
                )}
                {dayShifts.map((shift) => (
                  <div
                    key={shift.id}
                    onClick={(e) => { e.stopPropagation(); onShiftClick(shift); }}
                    className={cn(
                      'truncate rounded px-1.5 py-0.5 text-[10px] font-medium leading-tight',
                      allShiftsDone ? 'bg-background/20 text-background' : 'bg-background/80 text-foreground',
                    )}
                    title={isShiftDone(shift) ? 'Shift completed' : 'Shift not finished yet'}
                  >
                    {shift.startTime.slice(0, 5)} · {money(shift.totalEarned)}
                  </div>
                ))}
              </div>
              {hasShifts && (
                <div className={cn(
                  'mt-0.5 text-[9px] font-medium sm:hidden',
                  allShiftsDone ? 'text-background/80' : 'text-muted-foreground',
                )}>
                  {extraCount}
                </div>
              )}
            </button>
          );
        })}
        </div>
      </div>
    </div>
  );
}
