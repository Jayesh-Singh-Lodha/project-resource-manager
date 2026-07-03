// Common types

export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  errors: string[];
}
