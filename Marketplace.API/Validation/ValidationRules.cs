using System.Text.RegularExpressions;

namespace Marketplace.API.Validation;

public static partial class ValidationRules
{
    private const int MinPhoneDigits = 10;
    private const int MaxPhoneDigits = 15;

    [GeneratedRegex(@"^(?=(?:\D*\d){10,15}\D*$)\+?[0-9()\-\s]+$")]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"^[\p{L}\p{M}'’`\-\s]{2,100}$")]
    private static partial Regex PersonNameRegex();

    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var trimmed = phoneNumber.Trim();
        if (!PhoneRegex().IsMatch(trimmed))
            return false;

        var digits = trimmed.Count(char.IsDigit);
        return digits >= MinPhoneDigits && digits <= MaxPhoneDigits;
    }

    public static string NormalizePhoneNumber(string phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        return phoneNumber.Trim();
    }

    public static bool IsValidPersonName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return PersonNameRegex().IsMatch(value.Trim());
    }
}
