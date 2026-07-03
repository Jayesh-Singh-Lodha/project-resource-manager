import { type ClassValue, clsx } from 'clsx';
import { twMerge } from 'tailwind-merge';
import { format, parseISO, startOfWeek, subWeeks } from 'date-fns';

/**
 * Merge Tailwind CSS classes with conflict resolution.
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Format an ISO date string to DD-MM-YYYY display format.
 */
export function formatDate(isoDate: string): string {
  try {
    return format(parseISO(isoDate), 'dd-MM-yyyy');
  } catch {
    return isoDate;
  }
}

/**
 * Format an ISO date string to a readable display format.
 */
export function formatDateTime(isoDate: string): string {
  try {
    return format(parseISO(isoDate), 'dd-MM-yyyy HH:mm');
  } catch {
    return isoDate;
  }
}

/**
 * Get the start of the current week (Monday).
 */
export function getCurrentWeekStart(): Date {
  return startOfWeek(new Date(), { weekStartsOn: 1 });
}

/**
 * Get the start of last week (Monday).
 */
export function getLastWeekStart(): Date {
  return subWeeks(getCurrentWeekStart(), 1);
}

/**
 * Format a date to ISO string for API requests (YYYY-MM-DD).
 */
export function toApiDate(date: Date): string {
  return format(date, 'yyyy-MM-dd');
}

/**
 * Get health status color classes.
 */
export function getHealthColor(status: string): string {
  switch (status?.toLowerCase()) {
    case 'ontrack':
    case 'on_track':
      return 'text-emerald-400';
    case 'attention':
      return 'text-amber-400';
    case 'atrisk':
    case 'at_risk':
      return 'text-red-400';
    default:
      return 'text-slate-400';
  }
}

/**
 * Get health status emoji.
 */
export function getHealthEmoji(status: string): string {
  switch (status?.toLowerCase()) {
    case 'ontrack':
    case 'on_track':
      return '🟢';
    case 'attention':
      return '🟡';
    case 'atrisk':
    case 'at_risk':
      return '🔴';
    default:
      return '⚪';
  }
}
