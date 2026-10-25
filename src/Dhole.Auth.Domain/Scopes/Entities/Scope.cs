using CustomCodeFramework.Core.Domain.Entities;

namespace Dhole.Auth.Domain.Scopes.Entities;

public sealed class Scope : Entity<Guid>
{
    private Scope() { }

    private Scope(Guid id, string code, string name, string? description)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        IsActive = true;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public bool IsActive { get; private set; }

    public static Scope Create(string code, string name, string? description)
    {
        return new Scope(
            Guid.NewGuid(),
            code.Trim().ToLowerInvariant(),
            name.Trim(),
            description?.Trim()
        );
    }

    public void UpdateDefinition(string name, string? description)
    {
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
