namespace PowerBITips.Api.Models.Enums;

/// <summary>
/// Types of items that can be published
/// </summary>
public enum PublishedItemType
{
    Theme = 0,
    Layout = 1,
    Project = 2,
    Scrims = 3
}

/// <summary>
/// Sort order for published items
/// </summary>
public enum PublishedSortOrder
{
    Ascending = 0,
    Descending = 1
}

/// <summary>
/// Sort fields for published items
/// </summary>
public enum PublishedSortBy
{
    CreatedDate = 0,
    UpdatedDate = 1,
    Name = 2,
    Downloads = 3,
    FavoriteCount = 4
}