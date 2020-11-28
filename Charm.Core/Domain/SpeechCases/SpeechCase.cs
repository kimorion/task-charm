using System.Collections.Generic;
using Charm.Core.Domain.Dto;
using Charm.Core.Domain.Services;

namespace Charm.Core.Domain.SpeechCases
{
    public abstract class SpeechCase
    {
        public abstract bool TryMatch(List<string> words);
        public abstract string ApplyAndRespond(CharmManager manager);
    }
}