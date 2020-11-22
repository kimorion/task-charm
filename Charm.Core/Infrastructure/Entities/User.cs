using System;
using System.ComponentModel.DataAnnotations;
using Charm.Core.Infrastructure.Entities.Base;

namespace Charm.Core.Infrastructure.Entities
{
    public class User : IDbEntity<Guid>
    {
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        // todo Settings:
        // public TimeSpan MorningTime { get; set; }
        // public TimeSpan DayTime { get; set; }
        // public TimeSpan EveningTime { get; set; }

        //todo Context:
        // public LastAction LastAction { get; set; }
        // public DialogContext DialogContext { get; set; }
        // например, создание проекта
        // создание подзадачи
    }
}