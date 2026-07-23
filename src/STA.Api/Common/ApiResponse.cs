namespace STA.Api.Common;

public record ApiResponse<T>(bool Success, T? Data, string? Message = null);

public static class PaginationHelper
{
    public static (int Page, int PageSize) Normalize(int page, int pageSize, int maxPageSize = 200)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > maxPageSize) pageSize = maxPageSize;
        return (page, pageSize);
    }
}

public record ApiErrorResponse(bool Success, IEnumerable<ApiError> Errors)
{
    public ApiErrorResponse(params ApiError[] errors) : this(false, errors) { }
}

public record ApiError(string Field, string Message);
