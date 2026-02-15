export interface ApiResponse<T> {
  succeeded: boolean;
  message: string;
  errors?: string[];
  data: T;
}