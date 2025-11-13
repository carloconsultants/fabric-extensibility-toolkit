using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Core.Azure;

/// <summary>
/// Central constants for Azure storage resources. All values are retrieved from environment variables.
/// </summary>
public static class AzureConstants
{
    /// <summary>
    /// Gets the theme container name from environment variables.
    /// </summary>
    public static string ThemeContainer => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_THEME_CONTAINER");

    /// <summary>
    /// Gets the project container name from environment variables.
    /// </summary>
    public static string ProjectContainer => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PROJECT_CONTAINER");

    /// <summary>
    /// Gets the project images container name from environment variables.
    /// </summary>
    public static string ProjectImagesContainer => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PROJECT_IMAGES_CONTAINER");

    /// <summary>
    /// Gets the published images container name from environment variables.
    /// </summary>
    public static string PublishedImagesContainer => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PUBLISHED_IMAGES_CONTAINER");

    /// <summary>
    /// Gets the project folder name from environment variables.
    /// </summary>
    public static string ProjectFolder => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PROJECT_FOLDER");

    /// <summary>
    /// Gets the project layout folder name from environment variables.
    /// </summary>
    public static string ProjectLayoutFolder => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PROJECT_LAYOUT_FOLDER");

    /// <summary>
    /// Gets the theme folder name from environment variables.
    /// </summary>
    public static string ThemeFolder => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_THEME_FOLDER");

    /// <summary>
    /// Gets the project images folder name from environment variables.
    /// </summary>
    public static string ProjectImagesFolder => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PROJECT_IMAGES_FOLDER");

    /// <summary>
    /// Gets the published images folder name from environment variables.
    /// Note: This handles 'undefined' values from configuration by throwing an error.
    /// </summary>
    public static string PublishedImagesFolder
    {
        get
        {
            var value = ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PUBLISHED_IMAGES_FOLDER");
            if (value == "undefined")
            {
                throw new ArgumentException("STORAGE_PUBLISHED_IMAGES_FOLDER is set to 'undefined'. Please provide a valid folder name in local.settings.json.");
            }
            return value;
        }
    }

    /// <summary>
    /// Gets the published folder name from environment variables.
    /// Note: Allows 'undefined' as a valid value to match old Node.js API behavior.
    /// </summary>
    public static string PublishedFolder => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_PUBLISHED_FOLDER");

    /// <summary>
    /// Gets the user login event table name from environment variables.
    /// </summary>
    public static string UserLoginEventTable => ConfigurationHelper.GetRequiredEnvironmentVariable("STORAGE_USER_LOGIN_EVENT_TABLE");

    // Hard-coded container for license (if this should be configurable, add it to local.settings.json)
    public const string LicenseContainer = "license";
}
