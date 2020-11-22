using System;

namespace Charm.Core.Domain.Dto
{
    public class GistRequest
    {
        public long ChatId { get; set; }
        public string GistMessage { get; set; } = "";
    }
}