using System.ComponentModel.DataAnnotations;

namespace Charm.Core.Infrastructure.Entities
{
    public class ChatContext
    {
        [Required]
        public long UserId { get; set; }
        public User User { get; set; } = null!;
        [Required]
        public string Context { get; set; } = null!;
    }
}