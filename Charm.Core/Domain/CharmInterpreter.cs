using System;
using System.Threading.Tasks;
using Optional;

namespace Charm.Core.Domain
{
    public class CharmInterpreter
    {
        private readonly CharmLibrarian _librarian;

        public CharmInterpreter(CharmLibrarian librarian)
        {
            _librarian = librarian;
        }

        public async Task<Option<string, Exception>> TakeTextMessage(Guid userId, string message)
        {
            throw new NotImplementedException();
        }

        public async Task<Option<string, Exception>> TakeAudioMessage()
        {
            throw new NotImplementedException();
        }
    }
}