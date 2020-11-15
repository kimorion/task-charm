namespace Charm.Core.Infrastructure.Repositories.Base
{
    public interface IChangeTrackingContext
    {
        public void DetachAllEntities();
    }
}