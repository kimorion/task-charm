namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private enum State
        {
            Reset,
            ReadPatternToken,
            ReadAndSavePatternToken,
            ReadAndSaveParserName,
            SaveParserCall,
            ExitCurrentLevel,
            SkipLevel,
            Stop,
            Error,
        }
    }
}