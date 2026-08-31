

interface PageResponse<T> {
    data: T[];
    totalCount: number;
    totalPages: number;
    hasNextPage: boolean;
    hasPreviousPage: boolean;
    pageNumber: number;
    pageSize: number;
}

export type {PageResponse };