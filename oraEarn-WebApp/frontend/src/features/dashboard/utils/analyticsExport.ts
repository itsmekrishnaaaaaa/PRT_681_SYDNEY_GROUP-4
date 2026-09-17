import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import type { UserAnalytics } from '@/shared/types';
import { money } from '@/shared/utils/dates';

function buildFilename(analytics: UserAnalytics, ext: string) {
  const safeFrom = analytics.dateFrom.replace(/\//g, '-');
  const safeTo = analytics.dateTo.replace(/\//g, '-');
  return `oraearn-analytics-${analytics.period}-${safeFrom}-to-${safeTo}.${ext}`;
}

function summaryRows(analytics: UserAnalytics): (string | number)[][] {
  const rows: (string | number)[][] = [
    ['Metric', 'Value'],
    ['Net balance', analytics.netBalance],
    ['Total earnings', analytics.totalEarnings],
    ['Total expenses', analytics.totalExpenses],
    ['Shifts logged', analytics.shiftCount],
    ['Hours worked', analytics.totalHours],
    ['Expense transactions', analytics.expenseCount],
    ['Jobs', analytics.jobCount],
    ['Savings goals', analytics.savingsGoalCount],
    ['Savings target total', analytics.totalSavingsTarget],
    ['Income allocated to goals (%)', analytics.totalAllocationPercent],
  ];

  if (analytics.visaCompliance?.applies) {
    const visa = analytics.visaCompliance;
    rows.push(
      ['Visa fortnight hours', visa.fortnightHoursWorked],
      ['Visa hour limit', visa.fortnightHourLimit],
      ['Visa hours remaining', visa.remainingHours],
      ['Visa status', visa.isOverLimit ? 'Over limit' : visa.isNearLimit ? 'Near limit' : 'Compliant'],
    );
  }

  return rows;
}

export function downloadAnalyticsExcel(analytics: UserAnalytics) {
  const wb = XLSX.utils.book_new();

  const overview = XLSX.utils.aoa_to_sheet([
    ['OraEarn Analytics Report'],
    ['Period', analytics.periodLabel],
    ['From', analytics.dateFrom],
    ['To', analytics.dateTo],
    [],
    ...summaryRows(analytics),
  ]);
  XLSX.utils.book_append_sheet(wb, overview, 'Summary');

  const timeline = analytics.timelineBreakdown ?? [];
  if (timeline.length > 0) {
    const timelineSheet = XLSX.utils.aoa_to_sheet([
      ['Date', 'Day', 'Shift hours', 'Shift count', 'Expense amount', 'Expense count'],
      ...timeline.map((row) => [
        row.date,
        row.dayLabel,
        row.shiftHours,
        row.shiftCount,
        row.expenseAmount,
        row.expenseCount,
      ]),
    ]);
    XLSX.utils.book_append_sheet(wb, timelineSheet, 'Timeline');
  }

  const categories = Object.entries(analytics.expensesByCategory);
  if (categories.length > 0) {
    const categorySheet = XLSX.utils.aoa_to_sheet([
      ['Category', 'Amount'],
      ...categories.map(([category, amount]) => [category, amount]),
    ]);
    XLSX.utils.book_append_sheet(wb, categorySheet, 'By category');
  }

  const kinds = Object.entries(analytics.expensesByKind);
  if (kinds.length > 0) {
    const kindSheet = XLSX.utils.aoa_to_sheet([
      ['Expense type', 'Count'],
      ...kinds.map(([kind, count]) => [kind, count]),
    ]);
    XLSX.utils.book_append_sheet(wb, kindSheet, 'By type');
  }

  XLSX.writeFile(wb, buildFilename(analytics, 'xlsx'));
}

export function downloadAnalyticsPdf(analytics: UserAnalytics) {
  const doc = new jsPDF();
  const margin = 14;
  let y = margin;

  doc.setFontSize(18);
  doc.text('OraEarn Analytics Report', margin, y);
  y += 10;

  doc.setFontSize(10);
  doc.setTextColor(80);
  doc.text(`Period: ${analytics.periodLabel}`, margin, y);
  y += 5;
  doc.text(`${analytics.dateFrom} → ${analytics.dateTo}`, margin, y);
  y += 8;

  doc.setTextColor(0);
  autoTable(doc, {
    startY: y,
    head: [['Metric', 'Value']],
    body: summaryRows(analytics).slice(1).map(([metric, value]) => [
      String(metric),
      typeof value === 'number' && String(metric).toLowerCase().includes('hour') && !String(metric).includes('%')
        ? value.toFixed(1)
        : typeof value === 'number' && !String(metric).includes('count') && !String(metric).includes('allocated')
          ? money(value)
          : String(value),
    ]),
    theme: 'striped',
    headStyles: { fillColor: [109, 40, 217] },
    margin: { left: margin, right: margin },
  });

  y = (doc as jsPDF & { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 10;

  const timeline = analytics.timelineBreakdown ?? [];
  if (timeline.length > 0) {
    doc.setFontSize(12);
    doc.text('Timeline', margin, y);
    y += 4;

    autoTable(doc, {
      startY: y,
      head: [['Date', 'Day', 'Hours', 'Shifts', 'Expenses', 'Txns']],
      body: timeline.map((row) => [
        row.date,
        row.dayLabel,
        row.shiftHours.toFixed(1),
        String(row.shiftCount),
        money(row.expenseAmount),
        String(row.expenseCount),
      ]),
      theme: 'striped',
      headStyles: { fillColor: [109, 40, 217] },
      margin: { left: margin, right: margin },
      styles: { fontSize: 8 },
    });

    y = (doc as jsPDF & { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 10;
  }

  const categories = Object.entries(analytics.expensesByCategory);
  if (categories.length > 0) {
    if (y > 240) {
      doc.addPage();
      y = margin;
    }

    doc.setFontSize(12);
    doc.text('Spending by category', margin, y);
    y += 4;

    autoTable(doc, {
      startY: y,
      head: [['Category', 'Amount']],
      body: categories.map(([category, amount]) => [category, money(amount)]),
      theme: 'striped',
      headStyles: { fillColor: [109, 40, 217] },
      margin: { left: margin, right: margin },
    });
  }

  doc.save(buildFilename(analytics, 'pdf'));
}
