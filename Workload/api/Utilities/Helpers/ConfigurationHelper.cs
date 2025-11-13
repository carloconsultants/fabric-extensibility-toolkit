namespace PowerBITips.Api.Utilities.Helpers;

/// <summary>
/// Helper class for configuration-related utilities.
/// </summary>
public static class ConfigurationHelper
{
    /// <summary>
    /// Gets an environment variable value, throwing an exception if it's not set.
    /// </summary>
    /// <param name="variableName">The name of the environment variable.</param>
    /// <returns>The value of the environment variable.</returns>
    /// <exception cref="ArgumentException">Thrown when the environment variable is not set or is empty.</exception>
    public static string GetRequiredEnvironmentVariable(string variableName)
    {
        var value = Environment.GetEnvironmentVariable(variableName);

        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException($"Environment variable '{variableName}' is not set or is empty.");
        }

        return value;
    }

    /// <summary>
    /// Gets multiple required environment variables, returning them as a dictionary.
    /// </summary>
    /// <param name="variableNames">The names of the environment variables to retrieve.</param>
    /// <returns>A dictionary containing the variable names as keys and their values.</returns>
    /// <exception cref="ArgumentException">Thrown when any of the environment variables are not set or are empty.</exception>
    public static Dictionary<string, string> GetRequiredEnvironmentVariables(params string[] variableNames)
    {
        var result = new Dictionary<string, string>();
        var missingVariables = new List<string>();

        foreach (var variableName in variableNames)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrEmpty(value))
            {
                missingVariables.Add(variableName);
            }
            else
            {
                result[variableName] = value;
            }
        }

        if (missingVariables.Any())
        {
            throw new ArgumentException($"Environment variables are not set or are empty: {string.Join(", ", missingVariables)}");
        }

        return result;
    }
}