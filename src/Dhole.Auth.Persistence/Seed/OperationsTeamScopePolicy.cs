namespace Dhole.Auth.Persistence.Seed;

/// <summary>Only approved operational scopes are derived from active memberships.
/// A service caller cannot send arbitrary scope names or administrative grants.</summary>
public static class OperationsTeamScopePolicy
{
    public static bool Supports(string department) =>
        department is "APERTURAS" or "COORDINACION_ORIGEN";

    public static string[] Scopes(string department, bool supervisor)
    {
        string[] scopes = department switch
        {
            "APERTURAS" => ["operations.case.read", "operations.case.create", "operations.case.update",
                "operations.aperture.update", "operations.aperture.validation.review", "operations.realtime.read"],
            "COORDINACION_ORIGEN" => ["operations.case.read", "operations.origin.read", "operations.origin.update",
                "operations.origin.departure.confirm", "operations.internal-note.read",
                "operations.internal-note.create", "operations.document.manage", "operations.realtime.read"],
            _ => []
        };
        return supervisor && Supports(department) ? [.. scopes, "operations.audit.read"] : scopes;
    }

    public sealed record Membership(Guid UserId, string DepartmentCode, bool IsSupervisor);
    public sealed record Grant(Guid UserId, string ScopeCode);

    public static HashSet<Grant> Resolve(IEnumerable<Membership> memberships, IReadOnlySet<Guid> eligibleUsers)
        => memberships.Where(m => eligibleUsers.Contains(m.UserId))
            .SelectMany(m => Scopes(m.DepartmentCode, m.IsSupervisor).Select(code => new Grant(m.UserId, code)))
            .ToHashSet();
}
