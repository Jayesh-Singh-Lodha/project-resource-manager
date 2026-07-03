import { QueryClient } from '@tanstack/react-query';

/**
 * Shared TanStack Query client.
 * - 5 minute stale time to reduce unnecessary refetches.
 * - 1 retry on failure.
 * - Refetch on window focus for fresh data.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 5 * 60 * 1000, // 5 minutes
      retry: 1,
      refetchOnWindowFocus: true,
    },
    mutations: {
      retry: 0,
    },
  },
});
