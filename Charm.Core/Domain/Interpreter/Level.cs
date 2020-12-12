using System;
using System.Collections.Generic;
using System.Linq;

namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private class Level
        {
            public bool ExpressionResult { get; set; }
            public int TokenCaret { get; set; }
            public int StringCaret { get; set; }
            public void SetInvalid() => ExpressionResult = false;
            public void SetValid() => ExpressionResult = true;
            public readonly bool IsOptional;
            public ReaderState ReaderState = ReaderState.Reset;
            public ExpressionState ExpressionState = ExpressionState.And;

            public Level(int stringCaret, int tokenCaret, bool expressionResult)
            {
                _initialStringCaret = stringCaret;
                _initialTokenCaret = tokenCaret;
                StringCaret = stringCaret;
                TokenCaret = tokenCaret;
                ExpressionResult = expressionResult;
            }

            public void Reset()
            {
                StringCaret = _initialStringCaret;
                SetValid();
            }

            private readonly int _initialStringCaret;
            private readonly int _initialTokenCaret;
        }
    }
}