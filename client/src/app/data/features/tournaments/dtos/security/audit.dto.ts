export interface AuditLogResponseDto {
  id: number;
  userId?: number;
  username?: string;
  actionType: string;
  message: string;
  ipAddress?: string;
  createdAt: Date;
  totalRecords: number; 
}