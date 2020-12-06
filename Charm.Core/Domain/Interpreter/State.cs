namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private enum State
        {
            Reset,
            ReadToken,
            ReadAndSaveToken,
            ReadAndSaveParserName,
            SaveParserCall,
            Stop,
            Error,
        }
    }
}