namespace Charm.Core.Infrastructure.Entities.Base
{
    public interface IDbEntity<T>
    {
        T Id { get; set; }
    }
}