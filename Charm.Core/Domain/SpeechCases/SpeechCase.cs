using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Charm.Core.Domain.Entities;
using Charm.Core.Domain.Services;

namespace Charm.Core.Domain.SpeechCases
{
    public abstract class SpeechCase
    {
        public abstract bool TryParse(MessageInfo message);
        public abstract Task<string> ApplyAndRespond(long userId, CharmManager manager);
    }
}