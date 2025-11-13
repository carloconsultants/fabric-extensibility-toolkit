using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PowerBITips.Api.Models.Analytics;
using PowerBITips.Api.Utilities.Helpers;
using System.Text;
using System.Text.Json;

namespace PowerBITips.Api.Services;

public interface IAnalyticsManagementService
{
    Task<PostGoogleAnalyticsResponse> PostGoogleAnalyticsAsync(PostGoogleAnalyticsRequest request);
    Task<PostGoogleAnalyticsResponse> TrackEventAsync(EventTrackingRequest request);
    Task<PostGoogleAnalyticsResponse> TrackPageViewAsync(PageViewTrackingRequest request);
    Task<PostGoogleAnalyticsResponse> TrackLoginEventAsync(LoginEventRequest request);
}

public class AnalyticsManagementService : IAnalyticsManagementService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AnalyticsManagementService> _logger;

    public AnalyticsManagementService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AnalyticsManagementService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PostGoogleAnalyticsResponse> PostGoogleAnalyticsAsync(PostGoogleAnalyticsRequest request)
    {
        try
        {
            _logger.LogInformation("Posting Google Analytics event");

            var measurementId = ConfigurationHelper.GetRequiredEnvironmentVariable("GA_MEASUREMENT_ID");
            var apiSecret = ConfigurationHelper.GetRequiredEnvironmentVariable("GA_API_SECRET");

            var url = $"https://www.google-analytics.com/mp/collect?measurement_id={measurementId}&api_secret={apiSecret}";
            var jsonContent = JsonSerializer.Serialize(request.EventData, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, httpContent);

            var responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully posted Google Analytics event");
                return new PostGoogleAnalyticsResponse
                {
                    Success = true,
                    Message = "Event posted successfully"
                };
            }
            else
            {
                _logger.LogWarning("Failed to post Google Analytics event. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseText);
                return new PostGoogleAnalyticsResponse
                {
                    Success = false,
                    Message = $"Failed to post event: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error posting Google Analytics event");
            return new PostGoogleAnalyticsResponse
            {
                Success = false,
                Message = $"Error posting event: {ex.Message}"
            };
        }
    }

    public async Task<PostGoogleAnalyticsResponse> TrackEventAsync(EventTrackingRequest request)
    {
        try
        {
            _logger.LogInformation("Tracking custom event: {Action} in category {Category}",
                request.Action, request.Category);

            var clientId = GenerateClientId();
            var eventData = new GoogleAnalyticsEventData
            {
                ClientId = clientId,
                Events = new List<GoogleAnalyticsEvent>
                {
                    new GoogleAnalyticsEvent
                    {
                        Name = request.Action,
                        Params = new Dictionary<string, object>
                        {
                            ["event_category"] = request.Category
                        }
                    }
                }
            };

            // Add optional parameters
            if (!string.IsNullOrEmpty(request.Label))
                eventData.Events[0].Params["event_label"] = request.Label;

            if (request.Value.HasValue)
                eventData.Events[0].Params["value"] = request.Value.Value;

            // Add custom parameters
            if (request.Params != null)
            {
                foreach (var param in request.Params)
                {
                    eventData.Events[0].Params[param.Key] = param.Value;
                }
            }

            var analyticsRequest = new PostGoogleAnalyticsRequest { EventData = eventData };
            return await PostGoogleAnalyticsAsync(analyticsRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking custom event");
            return new PostGoogleAnalyticsResponse
            {
                Success = false,
                Message = $"Error tracking event: {ex.Message}"
            };
        }
    }

    public async Task<PostGoogleAnalyticsResponse> TrackPageViewAsync(PageViewTrackingRequest request)
    {
        try
        {
            _logger.LogInformation("Tracking page view for page: {Page}", request.Page);

            var clientId = GenerateClientId();
            var eventData = new GoogleAnalyticsEventData
            {
                ClientId = clientId,
                Events = new List<GoogleAnalyticsEvent>
                {
                    new GoogleAnalyticsEvent
                    {
                        Name = "page_view",
                        Params = new Dictionary<string, object>
                        {
                            ["page_location"] = request.Page
                        }
                    }
                }
            };

            // Add optional parameters
            if (!string.IsNullOrEmpty(request.Title))
                eventData.Events[0].Params["page_title"] = request.Title;

            if (!string.IsNullOrEmpty(request.Location))
                eventData.Events[0].Params["page_location"] = request.Location;

            var analyticsRequest = new PostGoogleAnalyticsRequest { EventData = eventData };
            return await PostGoogleAnalyticsAsync(analyticsRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking page view");
            return new PostGoogleAnalyticsResponse
            {
                Success = false,
                Message = $"Error tracking page view: {ex.Message}"
            };
        }
    }

    public async Task<PostGoogleAnalyticsResponse> TrackLoginEventAsync(LoginEventRequest request)
    {
        try
        {
            _logger.LogInformation("Tracking login event for user");

            var eventData = new GoogleAnalyticsEventData
            {
                ClientId = request.ClientId,
                Events = new List<GoogleAnalyticsEvent>
                {
                    new GoogleAnalyticsEvent
                    {
                        Name = "login",
                        Params = new Dictionary<string, object>
                        {
                            ["method"] = "PowerBI Tips Authentication",
                            ["session_id"] = request.SessionId
                        }
                    }
                }
            };

            var analyticsRequest = new PostGoogleAnalyticsRequest { EventData = eventData };
            return await PostGoogleAnalyticsAsync(analyticsRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking login event");
            return new PostGoogleAnalyticsResponse
            {
                Success = false,
                Message = $"Error tracking login event: {ex.Message}"
            };
        }
    }

    private string GenerateClientId()
    {
        // Generate a simple client ID for analytics
        return Guid.NewGuid().ToString();
    }
}