export interface ApiResponse<T> {
    succeeded: boolean;
    message: string;
    errors: any;
    data: T; 
}