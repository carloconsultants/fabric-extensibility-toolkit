using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Content;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Services.Interfaces;
using PowerBITips.Api.Utilities.Helpers;

namespace PowerBITips.Api.Services;

public interface IContentManagementService
{
    Task<GetYoutubeLinkResponse> GetYoutubeLinkAsync();
    Task<GetSharedResourcesResponse> GetSharedResourcesAsync(GetSharedResourcesRequest request);
}

public class ContentManagementService : IContentManagementService
{
    private readonly IUserService _userService;
    private readonly ThemeService _themeService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ContentManagementService> _logger;

    public ContentManagementService(
        IUserService userService,
        ThemeService themeService,
        IConfiguration configuration,
        ILogger<ContentManagementService> logger)
    {
        _userService = userService;
        _themeService = themeService;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<GetYoutubeLinkResponse> GetYoutubeLinkAsync()
    {
        try
        {
            var youtubeUrl = ConfigurationHelper.GetRequiredEnvironmentVariable("EMBEDDED_YOUTUBE_URL");

            _logger.LogInformation("Retrieved YouTube URL from configuration"); return Task.FromResult(new GetYoutubeLinkResponse
            {
                YoutubeUrl = youtubeUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving YouTube link");
            return Task.FromResult(new GetYoutubeLinkResponse());
        }
    }

    public async Task<GetSharedResourcesResponse> GetSharedResourcesAsync(GetSharedResourcesRequest request)
    {
        try
        {
            _logger.LogInformation("Getting shared resources for: {RequestedResources}",
                string.Join(", ", request.RequestedResources));

            // Get all contributors
            var contributorsResponse = await _userService.GetContributorsAsync();
            if (!contributorsResponse.IsSuccess || contributorsResponse.Data?.Items == null || !contributorsResponse.Data.Items.Any())
            {
                _logger.LogWarning("No contributors found");
                return new GetSharedResourcesResponse();
            }

            // Get all themes from contributors - simplified approach since we don't have the Themes property defined
            // TODO: Update UserEntity to include Themes property or use a different approach
            var allContributorsThemes = new List<dynamic>();

            // Filter to published themes only
            var publishedThemes = allContributorsThemes
                .Where(theme => theme?.PublishOptions != null)
                .ToList();

            if (!publishedThemes.Any())
            {
                _logger.LogWarning("No published themes found");
                return new GetSharedResourcesResponse();
            }

            var sharedResources = new List<SharedResourceItem>();

            foreach (var theme in publishedThemes)
            {
                var themeId = theme?.ThemeId?.ToString() ?? string.Empty;
                if (string.IsNullOrEmpty(themeId)) continue;

                var resourceItem = new SharedResourceItem { ThemeId = themeId };

                // Get theme file if requested
                if (request.RequestedResources.Contains(ContentResourceTypes.Theme) ||
                    request.RequestedResources.Contains(ContentResourceTypes.Palette))
                {
                    resourceItem.Theme = await _themeService.GetThemeFileAsync(themeId);

                    // Parse palette from theme if theme data is available
                    if (!string.IsNullOrEmpty(resourceItem.Theme) &&
                        request.RequestedResources.Contains(ContentResourceTypes.Palette))
                    {
                        try
                        {
                            var themeData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(resourceItem.Theme);
                            // Extract palette data colors if available
                            // Note: This is a simplified version - the actual parsing may need more complex logic
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse theme data for palette extraction");
                        }
                    }
                }

                // Get project file if requested
                if (request.RequestedResources.Contains(ContentResourceTypes.Project))
                {
                    resourceItem.Project = await _themeService.GetProjectFileAsync(themeId);
                }

                // Get images if requested
                if (request.RequestedResources.Contains(ContentResourceTypes.Scrims) ||
                    request.RequestedResources.Contains(ContentResourceTypes.Project))
                {
                    resourceItem.Images = await _themeService.GetProjectImagesAsync(themeId);
                }

                sharedResources.Add(resourceItem);
            }

            _logger.LogInformation("Retrieved {Count} shared resource items", sharedResources.Count);

            return new GetSharedResourcesResponse
            {
                SharedResources = sharedResources
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting shared resources");
            return new GetSharedResourcesResponse();
        }
    }
}