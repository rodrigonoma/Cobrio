namespace Cobrio.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CriadoEm { get; protected set; }
    public DateTime AtualizadoEm { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    protected BaseEntity(Guid id)
    {
        Id = id;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AtualizarDataModificacao()
    {
        AtualizadoEm = DateTime.UtcNow;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
}
