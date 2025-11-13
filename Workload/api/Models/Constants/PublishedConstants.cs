using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Models.Constants;

/// <summary>
/// Constants for published items. All configurable values are retrieved from environment variables.
/// </summary>
public static class PublishedConstants
{
    /// <summary>
    /// Gets the table name for published items from environment variables.
    /// </summary>
    public static string TableName => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PUBLISHED_TABLE");

    // Partition keys for different item types
    public static class PartitionKeys
    {
        public const string Themes = "theme";
        public const string Layouts = "layout";
        public const string Projects = "project";
        public const string Scrims = "scrims";
    }

    // Default values
    public static class Defaults
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const string DefaultSortBy = "CreatedDate";
        public const string DefaultSortOrder = "desc";
    }

    // Sort fields
    public static class SortFields
    {
        public const string CreatedDate = "CreatedDate";
        public const string UpdatedDate = "UpdatedDate";
        public const string Name = "Name";
        public const string Downloads = "Downloads";
        public const string FavoriteCount = "FavoriteCount";
    }

    // Sort orders
    public static class SortOrders
    {
        public const string Ascending = "asc";
        public const string Descending = "desc";
    }

    // User roles that can publish (excluding layouts which anyone can publish)
    public static class PublishRoles
    {
        public const string Admin = "Admin";
        public const string Contributor = "Contributor";
    }
}