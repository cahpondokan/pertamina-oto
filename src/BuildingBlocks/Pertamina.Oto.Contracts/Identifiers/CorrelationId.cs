namespace Pertamina.Oto.Contracts.Identifiers;

/// <summary>Opaque random correlation token, never an authorization credential.</summary>
public readonly record struct CorrelationId
{
    private readonly Guid value;
    private CorrelationId(Guid value) => this.value = value;
    public bool IsValid => value != Guid.Empty;
    public static CorrelationId New() => new(Guid.NewGuid());
    public static bool TryParse(string? candidate, out CorrelationId result)
    {
        result = default;
        if (candidate is null || candidate.Length != 32 ||
            !Guid.TryParseExact(candidate, "N", out Guid parsed) || parsed == Guid.Empty)
        {
            return false;
        }

        result = new CorrelationId(parsed);
        return true;
    }

    public static CorrelationId Parse(string? candidate) => TryParse(candidate, out var result)
        ? result : throw new FormatException("Invalid correlation identifier.");
    public override string ToString() => value.ToString("N");
}
