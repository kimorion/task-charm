namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private enum State
        {
            Reset,
            ReadTemplate,
            ReadTemplateAndSave,
            ReadAndSaveParserName,
            SaveParserCall,
            ExitCurrentLevel,
            SkipLevel,
            Stop,
            Error,
        }
    }
}