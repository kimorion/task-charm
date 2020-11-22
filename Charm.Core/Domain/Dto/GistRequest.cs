using System;

namespace Charm.Core.Domain.Dto
{
    public class GistRequest
    {
        public Guid UserId { get; set; }
        public string GistMessage { get; set; } = "";
    }
}