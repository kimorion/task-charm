using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class User : IDbEntity<long>
    {
        [Required]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; } = "";


        [Column(TypeName = "jsonb")]
        public DialogContext? DialogContext { get; set; }

        // public TimeSpan MorningTime { get; set; }
        // public TimeSpan DayTime { get; set; }
        // public TimeSpan EveningTime { get; set; }
    }
}