using System.ComponentModel.DataAnnotations;

namespace PowerBITips.Api.Models.DTOs.Validation;

/// <summary>
/// Validates that a string represents a valid GUID
/// </summary>
public class ValidGuidAttribute : ValidationAttribute
{
    public ValidGuidAttribute() : base("The {0} field must be a valid GUID.") { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true; // Let [Required] handle null validation
        
        if (value is string stringValue)
        {
            return Guid.TryParse(stringValue, out _);
        }
        
        return value is Guid;
    }
}

/// <summary>
/// Validates that a date is not in the past
/// </summary>
public class NotInPastAttribute : ValidationAttribute
{
    public NotInPastAttribute() : base("The {0} field cannot be in the past.") { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (value is DateTime dateTime)
        {
            return dateTime > DateTime.UtcNow;
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a string is a valid email address
/// </summary>
public class ValidEmailAttribute : ValidationAttribute
{
    public ValidEmailAttribute() : base("The {0} field must be a valid email address.") { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (value is string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a string represents a valid URL
/// </summary>
public class ValidUrlAttribute : ValidationAttribute
{
    public ValidUrlAttribute() : base("The {0} field must be a valid URL.") { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (value is string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) 
                   && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a numeric value is within a specified range
/// </summary>
public class NumericRangeAttribute : ValidationAttribute
{
    public double Minimum { get; set; }
    public double Maximum { get; set; }

    public NumericRangeAttribute(double minimum, double maximum) 
        : base("The {0} field must be between {1} and {2}.")
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name, Minimum, Maximum);
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (double.TryParse(value.ToString(), out double numericValue))
        {
            return numericValue >= Minimum && numericValue <= Maximum;
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a collection has a minimum and maximum number of items
/// </summary>
public class CollectionSizeAttribute : ValidationAttribute
{
    public int MinimumSize { get; set; }
    public int MaximumSize { get; set; } = int.MaxValue;

    public CollectionSizeAttribute(int minimumSize) 
        : base("The {0} field must contain at least {1} item(s).")
    {
        MinimumSize = minimumSize;
    }

    public CollectionSizeAttribute(int minimumSize, int maximumSize) 
        : base("The {0} field must contain between {1} and {2} item(s).")
    {
        MinimumSize = minimumSize;
        MaximumSize = maximumSize;
    }

    public override string FormatErrorMessage(string name)
    {
        if (MaximumSize == int.MaxValue)
        {
            return string.Format("The {0} field must contain at least {1} item(s).", name, MinimumSize);
        }
        return string.Format("The {0} field must contain between {1} and {2} item(s).", name, MinimumSize, MaximumSize);
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return MinimumSize == 0;
        
        if (value is System.Collections.ICollection collection)
        {
            return collection.Count >= MinimumSize && collection.Count <= MaximumSize;
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a string contains only allowed characters
/// </summary>
public class AllowedCharactersAttribute : ValidationAttribute
{
    public string AllowedCharacters { get; set; }

    public AllowedCharactersAttribute(string allowedCharacters) 
        : base("The {0} field contains invalid characters. Allowed characters: {1}")
    {
        AllowedCharacters = allowedCharacters;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString, name, AllowedCharacters);
    }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (value is string stringValue)
        {
            return stringValue.All(c => AllowedCharacters.Contains(c));
        }
        
        return false;
    }
}

/// <summary>
/// Validates that a string represents a valid JSON
/// </summary>
public class ValidJsonAttribute : ValidationAttribute
{
    public ValidJsonAttribute() : base("The {0} field must contain valid JSON.") { }

    public override bool IsValid(object? value)
    {
        if (value == null) return true;
        
        if (value is string jsonString)
        {
            try
            {
                System.Text.Json.JsonDocument.Parse(jsonString);
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }
        
        return false;
    }
}