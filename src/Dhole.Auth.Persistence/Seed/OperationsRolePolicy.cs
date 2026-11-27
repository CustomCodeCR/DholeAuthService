namespace Dhole.Auth.Persistence.Seed;

/// <summary>
/// Exact role names authorized to populate the Operations team membership pool.
/// A role alone never grants team scopes; only synchronized active team memberships do.
/// </summary>
public static class OperationsRolePolicy
{
    public const string Operator = "Operativo Logístico";
    public const string Supervisor = "Supervisor Operativo";
    // Auth contains older manually created roles with the spelling "Logisitico".
    // These exact legacy aliases refer to the same approved logistics operator role;
    // no other department, Dynamics, sales or generic admin roles are admitted.
    public const string OperatorLegacyTypo = "Operativo Logisitico";
    public const string OperatorWithoutAccent = "Operativo Logistico";
    public static readonly string[] BusinessRoles = ["Supervisor Operaciones", "Jefe Operaciones", "Operativo", "Gerente Logistica"];
    public static readonly string[] AssignmentEligible =
        [.. BusinessRoles, Operator, OperatorLegacyTypo, OperatorWithoutAccent, Supervisor, "Gerencia Logística", "Jefe de Operaciones", "Supervisor Origen"];

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
