namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private enum ReaderState
        {
            Reset, // starting state
            ReadToken, // reads and does the logical job
            // SkipToken, // reads and ignores token
            PushToken, // reads and save token to stack
            CallParser,
            // PushLevel, // enters 
            // PopLevel,  // 
            Stop,      // 
            Error,
        }
    }
}