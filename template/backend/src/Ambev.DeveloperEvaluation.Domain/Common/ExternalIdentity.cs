namespace Ambev.DeveloperEvaluation.Domain.Common;

/// <summary>
/// Represents an external entity reference with denormalized description (External Identities pattern).
/// </summary>
public class ExternalIdentity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ExternalIdentity()
    {
    }

    public ExternalIdentity(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}
