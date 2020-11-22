namespace Charm.Core.Domain.Services
{
    public class CharmInterpreter
    {
        private readonly CharmManager _manager;

        public CharmInterpreter(CharmManager manager)
        {
            _manager = manager;
        }

        // public async Task<Option<string, Exception>> TakeTextMessage(Guid userId, string message)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task<Option<string, Exception>> TakeAudioMessage()
        // {
        //     throw new NotImplementedException();
        // }
    }
}