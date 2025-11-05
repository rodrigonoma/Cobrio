export interface ApiError {
  message: string;
  errors?: { [key: string]: string[] };
  statusCode?: number;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface SelectOption {
  label: string;
  value: any;
}
