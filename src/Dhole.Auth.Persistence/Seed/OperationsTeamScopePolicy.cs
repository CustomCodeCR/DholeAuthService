namespace Dhole.Auth.Persistence.Seed;

/// <summary>Allowlisted operational scopes derived from active memberships. Roles
/// are validated independently of the caller's supplied scope claims.</summary>
public static class OperationsTeamScopePolicy
{
    public static bool Supports(string department) =>
        department is "APERTURAS" or "COORDINACION_ORIGEN" or "DOCUMENTACION" or "TRANSITO" or "PANAMA" or "COSTA_RICA";

    public static bool SupportsMembership(Membership member) =>
        Supports(member.DepartmentCode) && (member.RoleCode switch
        {
            null or "OPERATOR" or "TEAM_SUPERVISOR" or "LOGISTICS_MANAGER" or "OPERATIONS_HEAD" => true,
            "ORIGIN_SUPERVISOR" => member.DepartmentCode == "COORDINACION_ORIGEN",
            _ => false
        });

    public static string[] Scopes(string department, bool supervisor, string? roleCode = null, string? teamName = null)
    {
        if (!SupportsMembership(new(Guid.Empty, department, supervisor, roleCode))) return [];
        if (roleCode is "LOGISTICS_MANAGER" or "OPERATIONS_HEAD")
            return ["operations.case.read", "operations.case.read-all", "operations.case.create", "operations.realtime.read"];
        if (roleCode == "ORIGIN_SUPERVISOR")
            return ["operations.case.read", "operations.origin.read", "operations.origin.read-all", "operations.realtime.read"];
        string[] scopes = department switch
        {
            "APERTURAS" => ["operations.case.read", "operations.case.create", "operations.case.update",
                "operations.aperture.update", "operations.aperture.validation.review", "operations.realtime.read"],
            "COORDINACION_ORIGEN" => ["operations.case.read", "operations.origin.read", "operations.origin.update",
                "operations.origin.departure.confirm", "operations.internal-note.read",
                "operations.internal-note.create", "operations.document.manage", "operations.realtime.read"],
            "DOCUMENTACION" => ["operations.case.read", "operations.document.manage", "operations.realtime.read"],
            "TRANSITO" => ["operations.case.read", "operations.transit.manage", "operations.realtime.read"],
            "PANAMA" => ["operations.case.read", "operations.panama.manage", "operations.realtime.read"],
            "COSTA_RICA" => ["operations.case.read", "operations.costa-rica.manage", "operations.realtime.read"],
            _ => []
        };
        if (department == "COORDINACION_ORIGEN")
        {
            if (teamName == "Origen · Centroamérica") scopes = [.. scopes, "operations.central-america.manage"];
            if (teamName is "Origen · Transporte terrestre" or "Origen · FTL" or "Origen · LTL") scopes = [.. scopes, "operations.land.manage"];
        }
        return (roleCode == "TEAM_SUPERVISOR" || (roleCode is null && supervisor))
            ? [.. scopes, "operations.audit.read"] : scopes;
    }

    public sealed record Membership(Guid UserId, string DepartmentCode, bool IsSupervisor, string? RoleCode = null, string? TeamName = null);
    public sealed record Grant(Guid UserId, string ScopeCode);

    public static HashSet<Grant> Resolve(IEnumerable<Membership> memberships, IReadOnlySet<Guid> eligibleUsers)
        => memberships.Where(m => eligibleUsers.Contains(m.UserId) && SupportsMembership(m))
            .SelectMany(m => Scopes(m.DepartmentCode, m.IsSupervisor, m.RoleCode, m.TeamName).Select(code => new Grant(m.UserId, code)))
            .ToHashSet();
}
