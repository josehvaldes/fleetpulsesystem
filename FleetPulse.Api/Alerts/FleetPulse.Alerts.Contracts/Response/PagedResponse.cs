namespace FleetPulse.Alerts.Contracts.Response
{
    public record PagedResponse<T>(
        IReadOnlyList<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage);
}
