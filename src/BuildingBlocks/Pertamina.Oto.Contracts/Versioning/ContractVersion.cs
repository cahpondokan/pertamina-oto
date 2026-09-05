using System;

namespace Pertamina.Oto.Contracts.Versioning;

/// <summary>
/// Carries the canonical version token of a cross-boundary contract.
/// </summary>
/// <remarks>
/// The token is intentionally opaque. The canonical documentation requires
/// versioned contracts but does not yet lock a semantic-versioning scheme.
/// Consumers must compare the exact token and must not infer ordering.
/// </remarks>
public readonly record struct ContractVersion
{
    private ContractVersion(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the exact, case-sensitive version token supplied by its authority.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Creates a version from an authoritative token without normalizing it.
    /// </summary>
    /// <exception cref="FormatException">
    /// Thrown when the token is empty or contains surrounding whitespace.
    /// </exception>
    public static ContractVersion Parse(string? value)
    {
        if (!TryParse(value, out var version))
        {
            throw new FormatException(
                "A contract version token must be non-empty and must not contain surrounding whitespace.");
        }

        return version;
    }

    /// <summary>
    /// Attempts to preserve a non-empty authoritative version token exactly.
    /// </summary>
    public static bool TryParse(string? value, out ContractVersion version)
    {
        version = default;

        if (string.IsNullOrWhiteSpace(value) || !string.Equals(value, value.Trim(), StringComparison.Ordinal))
        {
            return false;
        }

        version = new ContractVersion(value);
        return true;
    }

    public override string ToString() => Value ?? string.Empty;
}
