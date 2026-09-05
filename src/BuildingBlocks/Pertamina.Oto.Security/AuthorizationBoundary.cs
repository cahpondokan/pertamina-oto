namespace Pertamina.Oto.Security;

public enum AuthorizationGate { Unknown = 0, Satisfied = 1, Denied = 2 }

/// <summary>
/// Trusted application-side results for one action/object at time of use. Never bind from
/// UI, IPC or request data. Satisfied includes a trusted policy determination that a
/// conditional gate does not apply. This context is not a reusable authorization grant.
/// </summary>
public sealed record AuthorizationContext(
    AuthorizationGate Authentication,
    AuthorizationGate Session,
    AuthorizationGate Permission,
    AuthorizationGate OwnershipOrScope,
    AuthorizationGate Policy,
    AuthorizationGate ResourceState,
    AuthorizationGate Consent,
    AuthorizationGate Reauthentication);

/// <summary>Default and missing decisions deny access.</summary>
public readonly record struct AuthorizationDecision
{
    private AuthorizationDecision(bool allowed) => IsAllowed = allowed;
    public bool IsAllowed { get; }
    internal static AuthorizationDecision Allow => new(true);
}

public interface IAuthorizationBoundary
{
    AuthorizationDecision Evaluate(AuthorizationContext? context);
}

/// <summary>Combines trusted gates; does not authenticate or resolve resource ownership.</summary>
public sealed class DenyByDefaultAuthorizationBoundary : IAuthorizationBoundary
{
    public AuthorizationDecision Evaluate(AuthorizationContext? context) =>
        context is not null &&
        context.Authentication == AuthorizationGate.Satisfied &&
        context.Session == AuthorizationGate.Satisfied &&
        context.Permission == AuthorizationGate.Satisfied &&
        context.OwnershipOrScope == AuthorizationGate.Satisfied &&
        context.Policy == AuthorizationGate.Satisfied &&
        context.ResourceState == AuthorizationGate.Satisfied &&
        context.Consent == AuthorizationGate.Satisfied &&
        context.Reauthentication == AuthorizationGate.Satisfied
            ? AuthorizationDecision.Allow : default;
}
