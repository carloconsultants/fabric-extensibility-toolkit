using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Models.Constants;

/// <summary>
/// Constants for theme entities. All configurable values are retrieved from environment variables.
/// </summary>
public static class ThemeConstants
{
    /// <summary>
    /// Gets the table name for themes from environment variables.
    /// </summary>
    public static string TableName => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_THEME_TABLE");

    public const string DataColorsPartition = "dataColors";
    public const string VisualStylesPartitionPrefix = "visualStyles_";
}