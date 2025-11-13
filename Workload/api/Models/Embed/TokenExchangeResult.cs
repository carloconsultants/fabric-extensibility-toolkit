namespace PowerBITips.Api.Models.Embed;

/// <summary>
/// Result of a token exchange operation with detailed error information
/// </summary>
public class TokenExchangeResult
{
    public bool IsSuccess { get; set; }
    public string? AccessToken { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }
    public string? AzureAdErrorCode { get; set; }
    public string? AzureAdErrorDescription { get; set; }

    public static TokenExchangeResult Success(string accessToken)
    {
        return new TokenExchangeResult
        {
            IsSuccess = true,
            AccessToken = accessToken
        };
    }

    public static TokenExchangeResult Failure(string errorCode, string errorDescription, string? azureAdErrorCode = null, string? azureAdErrorDescription = null)
    {
        return new TokenExchangeResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            ErrorDescription = errorDescription,
            AzureAdErrorCode = azureAdErrorCode,
            AzureAdErrorDescription = azureAdErrorDescription
        };
    }
}

