using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Core.Interfaces;
using PowerBITips.Api.Models.Constants;
using PowerBITips.Api.Models.DTOs.Requests;
using PowerBITips.Api.Models.DTOs.Responses;
using PowerBITips.Api.Models.Entities;
using PowerBITips.Api.Models.Enums;
using PowerBITips.Api.Services.Common;
using PowerBITips.Api.Utilities.Helpers;
using System.Net;
using Newtonsoft.Json;
using Azure.Data.Tables;

namespace PowerBITips.Api.Services;

public class ThemeService
{
    private readonly IAzureTableStorage _tableStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ThemeService> _logger;

    public ThemeService(
        IAzureTableStorage tableStorage,
        IConfiguration configuration,
        ILogger<ThemeService> logger)
    {
        _tableStorage = tableStorage;
        _configuration = configuration;
        _logger = logger;

        _logger.LogInformation("✅ ThemeService initialized - table name will be retrieved from ThemeConstants.TableName");
    }

    public async Task<ServiceResponse<ThemeDetailResponse>> GetThemeAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("🔄 GetTheme - Getting theme details for theme {ThemeId}", themeId);

            // Get theme from single 'theme' partition
            var themes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and PartitionKey eq 'theme'"
            );

            if (!themes.Any())
            {
                _logger.LogWarning("⚠️ GetTheme - Theme {ThemeId} not found", themeId);
                return ServiceResponse<ThemeDetailResponse>.NotFound("Theme not found");
            }

            var themeEntity = themes.First();
            _logger.LogInformation("📋 GetTheme - Retrieved theme entity with JSON: {JSON}", themeEntity.JSON);

            // Use the theme JSON directly without parsing to avoid serialization issues
            var themeDataJson = themeEntity.JSON ?? "{}";
            _logger.LogInformation("✅ GetTheme - Retrieved theme JSON: {ThemeJson}", themeDataJson);

            var projectData = new Dictionary<string, object>();
            var images = new List<string>();

            // Parse project data
            if (!string.IsNullOrEmpty(themeEntity.ProjectJSON))
            {
                try
                {
                    var project = JsonConvert.DeserializeObject(themeEntity.ProjectJSON);
                    if (project != null)
                    {
                        if (project is Newtonsoft.Json.Linq.JObject jObject)
                        {
                            projectData = jObject.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
                        }
                        else if (project is Dictionary<string, object> dict)
                        {
                            projectData = dict;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ GetTheme - Error parsing project JSON");
                }
            }

            // Parse images
            if (!string.IsNullOrEmpty(themeEntity.ProjectImages))
            {
                try
                {
                    var imgs = JsonConvert.DeserializeObject<string[]>(themeEntity.ProjectImages);
                    if (imgs != null)
                        images.AddRange(imgs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ GetTheme - Error parsing project images");
                }
            }

            // Parse theme data directly as object to preserve original JSON structure
            object? parsedThemeData = null;
            try
            {
                parsedThemeData = JsonConvert.DeserializeObject(themeDataJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GetTheme - Error parsing theme JSON");
            }

            var response = new ThemeDetailResponse
            {
                Theme = MapToThemeResponse(themeEntity),
                ThemeData = parsedThemeData,
                ProjectData = projectData.Any() ? projectData : null,
                Images = images.Any() ? images.ToArray() : null
            };

            return ServiceResponse<ThemeDetailResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting theme {ThemeId}", themeId);
            return ServiceResponse<ThemeDetailResponse>.InternalServerError($"Error getting theme: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets raw theme JSON without deserialization to avoid corruption issues
    /// </summary>
    public async Task<ServiceResponse<RawThemeResponse>> GetRawThemeJsonAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("🔍 GetRawThemeJson - Retrieving theme {ThemeId}", themeId);
            _logger.LogInformation("🔍 GetRawThemeJson - Connection string exists: {HasConnection}",
                !string.IsNullOrEmpty(_configuration.GetConnectionString("AzureWebJobsStorage")));
            _logger.LogInformation("🔍 GetRawThemeJson - Table storage service: {ServiceType}",
                _tableStorage.GetType().Name);

            // Query using existing table storage service but log what we get back
            var themes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and PartitionKey eq 'theme'"
            );

            if (!themes.Any())
            {
                _logger.LogWarning("⚠️ GetRawThemeJson - Theme {ThemeId} not found", themeId);
                return ServiceResponse<RawThemeResponse>.NotFound("Theme not found");
            }

            var themeEntity = themes.First();

            // Log exactly what we're getting from the entity
            var jsonValue = themeEntity.JSON;
            var projectJsonValue = themeEntity.ProjectJSON;
            var projectImagesValue = themeEntity.ProjectImages;
            var themeNameValue = themeEntity.ThemeName;

            _logger.LogInformation("🎯 Raw entity JSON field: '{RawJson}'", jsonValue);
            _logger.LogInformation("🎯 Raw JSON type: {JsonType}", jsonValue?.GetType().Name ?? "null");

            var response = new RawThemeResponse
            {
                ThemeJson = jsonValue ?? "{}",
                ProjectJson = projectJsonValue ?? "{}",
                ImagesJson = projectImagesValue ?? "[]",
                ThemeName = themeNameValue ?? ""
            };

            _logger.LogInformation("✅ GetRawThemeJson - Retrieved raw theme data for {ThemeId}", themeId);
            return ServiceResponse<RawThemeResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting raw theme JSON {ThemeId}", themeId);
            return ServiceResponse<RawThemeResponse>.InternalServerError($"Error getting raw theme: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<UserThemesResponse>> GetUserThemesAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal)
    {
        try
        {
            var userId = clientPrincipal.UserId ?? string.Empty;
            _logger.LogInformation("Getting themes for user {UserId}", userId);

            // Get user's themes by filtering on UserId and PartitionKey = 'theme'
            var userThemes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"UserId eq '{userId}' and PartitionKey eq 'theme'"
            );

            var themes = userThemes.Select(MapToThemeResponse).ToList();

            var response = new UserThemesResponse
            {
                Items = themes,
                TotalCount = themes.Count
            };

            return ServiceResponse<UserThemesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting themes for user {UserId}", clientPrincipal.UserId);
            return ServiceResponse<UserThemesResponse>.InternalServerError($"Error getting user themes: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<ThemeResponse>> SaveThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, SaveThemeRequest request)
    {
        try
        {
            var userId = clientPrincipal.UserId ?? string.Empty;
            _logger.LogInformation("Saving theme {ThemeId} for user {UserId}", request.Id, userId);

            var themeId = request.Id;
            var timestamp = DateTime.UtcNow;

            // Save the entire theme payload as a single entity instead of parsing it
            var payloadJson = request.Payload ?? "{}";
            _logger.LogInformation("💾 SaveTheme - Storing complete payload: {PayloadJson}", payloadJson);

            var entities = new List<ThemeEntity>();

            // Create single theme entity with complete payload
            var themeEntity = new ThemeEntity
            {
                PartitionKey = "theme",
                RowKey = themeId,
                JSON = payloadJson, // Store the complete theme JSON
                ProjectJSON = request.ProjectPayload ?? string.Empty,
                ProjectImages = request.ProjectImages != null ? JsonConvert.SerializeObject(request.ProjectImages) : string.Empty,
                Type = ThemeType.Theme,
                Created = timestamp,
                ThemeName = request.ThemeName,
                ScrimsFamilyName = request.ScrimsFamilyName ?? string.Empty,
                IsCommunity = request.IsCommunity ?? false,
                UserId = userId,
                DownloadCount = 0
            };
            entities.Add(themeEntity);

            // Save all entities
            foreach (var entity in entities)
            {
                try
                {
                    await _tableStorage.UpdateEntityAsync(ThemeConstants.TableName, entity);
                }
                catch
                {
                    await _tableStorage.CreateEntityAsync(ThemeConstants.TableName, entity);
                }
            }

            var response = entities.FirstOrDefault();
            if (response == null)
            {
                return ServiceResponse<ThemeResponse>.BadRequest("Invalid theme data");
            }

            return ServiceResponse<ThemeResponse>.Success(MapToThemeResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving theme {ThemeId} for user {UserId}", request.Id, clientPrincipal.UserId);
            return ServiceResponse<ThemeResponse>.InternalServerError($"Error saving theme: {ex.Message}");
        }
    }

    public async Task<ServiceResponse<bool>> DeleteThemeAsync(StaticWebAppsAuth.ClientPrincipal clientPrincipal, string themeId)
    {
        try
        {
            var userId = clientPrincipal.UserId ?? string.Empty;
            _logger.LogInformation("Deleting theme {ThemeId} for user {UserId}", themeId, userId);

            // Get theme entity for this theme ID
            var themeEntities = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and UserId eq '{userId}' and PartitionKey eq 'theme'"
            );

            if (!themeEntities.Any())
            {
                return ServiceResponse<bool>.NotFound("Theme not found or access denied");
            }

            // Delete the theme entity
            var themeEntity = themeEntities.First();
            await _tableStorage.DeleteEntityAsync(ThemeConstants.TableName, themeEntity.PartitionKey, themeEntity.RowKey);

            return ServiceResponse<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting theme {ThemeId} for user {UserId}", themeId, clientPrincipal.UserId);
            return ServiceResponse<bool>.InternalServerError($"Error deleting theme: {ex.Message}");
        }
    }

    // Content Management support methods
    public async Task<string> GetThemeFileAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Getting theme file for theme {ThemeId}", themeId);

            // Get theme entity to find the blob URL
            var themes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and PartitionKey eq 'theme'"
            );

            var theme = themes.FirstOrDefault();
            if (theme == null || string.IsNullOrEmpty(theme.Url))
            {
                _logger.LogWarning("Theme file not found for theme {ThemeId}", themeId);
                return string.Empty;
            }

            // For now, return empty string - in full implementation, would fetch from blob storage
            // TODO: Implement blob storage service to fetch actual content from theme.Url
            _logger.LogInformation("Theme file URL found: {Url}", theme.Url);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting theme file for theme {ThemeId}", themeId);
            return string.Empty;
        }
    }

    public async Task<string> GetProjectFileAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Getting project file for theme {ThemeId}", themeId);

            // Get theme entity to find the project blob URL
            var themes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and PartitionKey eq 'theme'"
            );

            var theme = themes.FirstOrDefault();
            if (theme == null || string.IsNullOrEmpty(theme.ProjectUrl))
            {
                _logger.LogWarning("Project file not found for theme {ThemeId}", themeId);
                return string.Empty;
            }

            // For now, return empty string - in full implementation, would fetch from blob storage
            // TODO: Implement blob storage service to fetch actual content from theme.ProjectUrl
            _logger.LogInformation("Project file URL found: {Url}", theme.ProjectUrl);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project file for theme {ThemeId}", themeId);
            return string.Empty;
        }
    }

    public async Task<string> GetProjectImagesAsync(string themeId)
    {
        try
        {
            _logger.LogInformation("Getting project images for theme {ThemeId}", themeId);

            // Get theme entity to find the project images blob URL
            var themes = await _tableStorage.GetEntitiesByFilterAsync<ThemeEntity>(
                ThemeConstants.TableName,
                $"RowKey eq '{themeId}' and PartitionKey eq 'theme'"
            );

            var theme = themes.FirstOrDefault();
            if (theme == null || string.IsNullOrEmpty(theme.ProjectImagesUrl))
            {
                _logger.LogWarning("Project images not found for theme {ThemeId}", themeId);
                return string.Empty;
            }

            // For now, return empty string - in full implementation, would fetch from blob storage
            // TODO: Implement blob storage service to fetch actual content from theme.ProjectImagesUrl
            _logger.LogInformation("Project images URL found: {Url}", theme.ProjectImagesUrl);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project images for theme {ThemeId}", themeId);
            return string.Empty;
        }
    }

    private static ThemeResponse MapToThemeResponse(ThemeEntity entity)
    {
        return new ThemeResponse
        {
            ThemeId = entity.ThemeId,
            ThemeName = entity.ThemeName,
            Type = entity.Type,
            Created = entity.Created,
            DownloadCount = entity.DownloadCount,
            UserId = entity.UserId,
            IsCommunity = entity.IsCommunity,
            ScrimsFamilyName = entity.ScrimsFamilyName,
            Url = entity.Url,
            ProjectUrl = entity.ProjectUrl,
            ProjectImagesUrl = entity.ProjectImagesUrl
        };
    }
}