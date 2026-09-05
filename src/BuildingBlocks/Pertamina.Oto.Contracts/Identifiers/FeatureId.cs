using System;

namespace Pertamina.Oto.Contracts.Identifiers;

/// <summary>
/// Identifies one canonical product capability using the catalog form
/// <c>FEAT-{CATEGORY}-{NNN}</c>.
/// </summary>
public readonly record struct FeatureId
{
    private const string Prefix = "FEAT-";
    private const int OrdinalLength = 3;

    private FeatureId(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the canonical, case-sensitive identifier.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a feature identifier from its canonical representation.
    /// </summary>
    /// <exception cref="FormatException">
    /// Thrown when <paramref name="value"/> is not in canonical form.
    /// </exception>
    public static FeatureId Parse(string? value)
    {
        if (!TryParse(value, out var featureId))
        {
            throw new FormatException(
                "A canonical feature identifier must use FEAT-{CATEGORY}-{NNN} with an uppercase alphabetic category.");
        }

        return featureId;
    }

    /// <summary>
    /// Attempts to create a feature identifier without normalizing or changing
    /// the caller-supplied catalog value.
    /// </summary>
    public static bool TryParse(string? value, out FeatureId featureId)
    {
        featureId = default;

        if (value is null || !value.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var remainder = value.AsSpan(Prefix.Length);
        var categorySeparator = remainder.LastIndexOf('-');

        if (categorySeparator <= 0 || remainder.Length - categorySeparator - 1 != OrdinalLength)
        {
            return false;
        }

        var category = remainder[..categorySeparator];
        foreach (var character in category)
        {
            if (character is < 'A' or > 'Z')
            {
                return false;
            }
        }

        var ordinal = remainder[(categorySeparator + 1)..];
        foreach (var character in ordinal)
        {
            if (character is < '0' or > '9')
            {
                return false;
            }
        }

        featureId = new FeatureId(value);
        return true;
    }

    public override string ToString() => Value ?? string.Empty;
}
