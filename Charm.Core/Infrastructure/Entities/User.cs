using System;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class User : IDbEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid DialogId { get; set; }
    }
}