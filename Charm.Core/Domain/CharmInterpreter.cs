using System;
using Optional;

namespace Charm.Core.Domain
{
    public class CharmInterpreter
    {
        public Option<string, Exception> TakeTextMessage()
        {
            throw new NotImplementedException();
        }

        public Option<string, Exception> TakeAudioMessage()
        {
            throw new NotImplementedException();
        }
    }
}