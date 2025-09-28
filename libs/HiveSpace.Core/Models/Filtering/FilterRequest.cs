namespace HiveSpace.Core.Models.Filtering;

public abstract class FilterRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = "createdAt.desc";

    public string SortField => Sort?.Split('.').FirstOrDefault() ?? "createdAt";
    public string SortDirection => Sort?.Split('.').LastOrDefault() ?? "desc";

    public virtual void Validate()
    {
        if (Page < 1) Page = 1;
        if (PageSize < 10) PageSize = 10;
        if (PageSize > 50) PageSize = 50;
    }
}