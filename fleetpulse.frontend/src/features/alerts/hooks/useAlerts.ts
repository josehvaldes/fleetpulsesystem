import { getAlerts } from '@/services/alertService';
import type { Alert } from '@/types/alert';
import {
  useQuery,
} from '@tanstack/react-query'

interface AlertsState {
  isLoading: boolean;
  error: string | null;
  alerts: Alert[] | null;
  totalCount: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
  pageNumber: number;
  pageSize: number;
}

export function useAlerts(
    pageNumber: number,
    pageSize: number,
    riskLevel?: string,
    status?: string,
    fromDate?: string,
    toDate?: string):AlertsState {

  const { isLoading, error, data} = useQuery({
    queryKey: ['alerts', pageNumber, pageSize, riskLevel, status, fromDate, toDate],
    queryFn: async () => getAlerts(pageNumber, pageSize, riskLevel, status, fromDate, toDate),
    retryDelay: (attemptIndex: number) => Math.min(1000 * 2 ** attemptIndex, 30000),
  });
  
  return {
    isLoading: isLoading,
    error: error ? (error as Error).message : null,
    alerts: data ? data.data : null,
    totalCount: data ? data.totalCount : 0,
    totalPages: data ? data.totalPages : 0,
    hasNextPage: data ? data.hasNextPage : false,
    hasPreviousPage: data ? data.hasPreviousPage : false,
    pageNumber: data ? data.pageNumber : 0,
    pageSize: data ? data.pageSize : 0,
  };
}