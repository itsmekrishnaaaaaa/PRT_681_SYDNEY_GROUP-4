export type AccountApprovalStatus = 'Pending' | 'Approved' | 'Rejected';

export type IdentityDocumentType = 'Passport' | 'DrivingLicence' | 'NationalIdCard' | 'VisaGrantNotice';

export const identityDocumentOptions: { value: IdentityDocumentType; label: string; hint: string }[] = [
  { value: 'Passport', label: 'Australian passport', hint: 'Upload the photo page (100-point ID — 70 points).' },
  { value: 'DrivingLicence', label: 'Australian driving licence', hint: 'Upload the front of your valid licence (100-point ID — 40 points).' },
  { value: 'NationalIdCard', label: 'Photo ID card', hint: 'Upload a government-issued photo ID (e.g. Proof of Age).' },
  { value: 'VisaGrantNotice', label: 'VEVO / visa grant notice', hint: 'Upload your Department of Home Affairs visa grant or VEVO check.' },
];

export const identityDocumentLabel = (type: IdentityDocumentType) =>
  identityDocumentOptions.find((o) => o.value === type)?.label ?? type;

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  approvalStatus: AccountApprovalStatus;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserProfile;
}

export interface RegisterResponse {
  message: string;
  userId: string;
  approvalStatus: AccountApprovalStatus;
}

export interface Job {
  id: string;
  title: string;
  hourlyRate: number;
  dateOfJoin: string;
}

export interface Shift {
  id: string;
  jobId: string;
  jobTitle: string;
  workDate: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
  totalHours: number;
  totalEarned: number;
}

export interface ShiftSummary {
  totalHours: number;
  totalPay: number;
  shiftCount: number;
}

export interface ActiveShiftSession {
  id: string;
  jobId: string;
  jobTitle: string;
  shiftDate: string;
  startTime: string;
  breakMinutesAccumulated: number;
  breakStartedAt: string | null;
  hourlyRate: number;
}

export type ExpenseKind = 'Fixed' | 'Variable' | 'OneOff';

export interface ExpenseCategory {
  id: string;
  title: string;
}

export interface Expense {
  id: string;
  categoryId: string;
  categoryTitle: string;
  amount: number;
  date: string;
  expenseKind: ExpenseKind | number;
  paymentMethod: string;
}

export interface ExpenseSummary {
  totalExpenses: number;
  expenseCount: number;
  byCategory: Record<string, number>;
}

export interface ExpenseCalculatorResult {
  totalExpenses: number;
  remainingBalance: number;
  savingsRatePercent: number;
  byCategory: Record<string, number>;
}

export type SavingsGoalStatus = 'Pending' | 'InProgress' | 'Completed';

export interface SavingsGoal {
  id: string;
  title: string;
  targetAmount: number;
  currentAmount: number;
  allocationPercentage: number;
  targetDate: string | null;
  status: SavingsGoalStatus;
  progressPercent: number;
  remainingAmount: number;
}

export interface SavingsGoalAnalyticsItem {
  goalId: string;
  title: string;
  status: SavingsGoalStatus;
  targetAmount: number;
  currentAmount: number;
  remainingAmount: number;
  progressPercent: number;
  allocationPercentage: number;
  avgHourlyRate: number;
  hoursNeeded: number;
  avgHoursPerWeek: number;
  estimatedMonthlySavings: number;
  targetDate: string | null;
  estimatedFinishDate: string | null;
  weeksToComplete: number | null;
}

export interface SavingsNotification {
  type: string;
  message: string;
  severity: 'error' | 'warning' | 'info';
}

export interface SavingsGoalAnalytics {
  monthlyIncomeEstimate: number;
  netBalance: number;
  totalAllocationPercent: number;
  goals: SavingsGoalAnalyticsItem[];
  notifications: SavingsNotification[];
}

export interface AdvisorChatResponse {
  reply: string;
  suggestions: string[];
}

export interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  createdAt: string;
  approvalStatus: AccountApprovalStatus;
}

export interface PendingRegistration {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  dateOfBirth: string | null;
  nationality: string;
  visaType: string;
  visaStatus: string;
  workingRights: string;
  visaExpiryDate: string | null;
  identityDocumentType: IdentityDocumentType;
  visaDocumentFileName: string | null;
  hasVisaDocument: boolean;
  approvalStatus: AccountApprovalStatus;
  aiReviewSummary: string | null;
  createdAt: string;
}

export interface RegistrationAiReview {
  recommendation: string;
  summary: string;
  checks: string[];
  flags: string[];
  suggestApprove: boolean;
}

export interface VisaCompliance {
  applies: boolean;
  visaStatus: string;
  workingRights: string;
  fortnightHoursWorked: number;
  fortnightHourLimit: number;
  remainingHours: number;
  isOverLimit: boolean;
  isNearLimit: boolean;
  fortnightStart: string;
  fortnightEnd: string;
}

export interface AppNotification {
  id: string;
  type: string;
  title: string;
  message: string;
  severity: 'error' | 'warning' | 'info';
  category: string;
  createdAt: string;
  isRead: boolean;
}

export interface NotificationInbox {
  items: AppNotification[];
  unreadCount: number;
}

export type AnalyticsPeriodKey = '7d' | '30d' | '12m';

export interface WeekdayAnalytics {
  dayLabel: string;
  date: string;
  shiftHours: number;
  shiftCount: number;
  expenseAmount: number;
  expenseCount: number;
}

export interface UserAnalytics {
  jobCount: number;
  shiftCount: number;
  totalHours: number;
  totalEarnings: number;
  expenseCount: number;
  totalExpenses: number;
  netBalance: number;
  savingsGoalCount: number;
  totalSavingsTarget: number;
  totalAllocationPercent: number;
  expensesByCategory: Record<string, number>;
  expensesByKind: Record<string, number>;
  visaCompliance: VisaCompliance | null;
  notifications: AppNotification[];
  timelineBreakdown: WeekdayAnalytics[];
  period: string;
  periodLabel: string;
  dateFrom: string;
  dateTo: string;
  unreadNotificationCount: number;
}

export interface AdminUserStats {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
  createdAt: string;
  jobCount: number;
  shiftCount: number;
  totalEarnings: number;
  expenseCount: number;
  totalExpenses: number;
  savingsGoalCount: number;
}

export interface AdminAnalytics {
  totalUsers: number;
  totalAdmins: number;
  totalJobs: number;
  totalShifts: number;
  totalHours: number;
  totalEarnings: number;
  totalExpenses: number;
  totalExpenseAmount: number;
  totalSavingsGoals: number;
  users: AdminUserStats[];
}

export const expenseKindOptions: { value: ExpenseKind; label: string }[] = [
  { value: 'Fixed', label: 'Fixed' },
  { value: 'Variable', label: 'Variable' },
  { value: 'OneOff', label: 'One-off' },
];

export const expenseKindLabel = (kind: ExpenseKind | number) => {
  if (typeof kind === 'number') {
    return expenseKindOptions[kind]?.label ?? 'Unknown';
  }
  return kind;
};
