using System;

namespace Charm.Core.Domain.Dto
{
    public class GistSearchCriteria
    {
        public bool? IsDone { get; set; }
        public DateTimeOffset? Date { get; set; }
    }
}