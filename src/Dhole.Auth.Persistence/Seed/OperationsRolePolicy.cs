namespace Dhole.Auth.Persistence.Seed;

/// <summary>
/// Exact role names authorized to populate the Operations team membership pool.
/// Scope grants are independent: membership in either role never grants new scopes.
/// </summary>
public static class OperationsRolePolicy
{
    public const string Operator = "Operativo Logístico";
    public const string Supervisor = "Supervisor Operativo";
    public static readonly string[] AssignmentEligible = [Operator, Supervisor];

    // Only these seven formerly auto-seeded roles may be retired.
    // Keep their scopes and all other manually configured roles unchanged.
    public static readonly string[] DeprecatedAutoCreated =
    [
        "Dynamics",
        "Dynamics Seguridad",
        "Operaciones · Administrador de equipos",
        "Operaciones · Validador de Aperturas",
        "Operaciones · Coordinación Origen",
        "Vendedor Jefe",
        "Vendedor Supervisor"
    ];
}
