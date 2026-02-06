namespace Share.Models;
public abstract class BaseFilterQTO:BasePaginationQTO
{
    public string? Created_at_start { get; init; }

    public string? Created_at_end { get; init; }


    public string? Updated_at_start { get; init; }

    public string? Updated_at_end { get; init; }

    public string? Deleted_at_start { get; init; }

    public string? Deleted_at_end { get; init; }

    public bool? Is_deleted { get; init; }


    public Guid? Uuid { get; init; }
}