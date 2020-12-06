using System;
using System.Collections.Generic;
using System.Linq;

namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        private class Level
        {
            private bool _innerStatus;

            public bool IsValid
            {
                get
                {
                    var result = _innerStatus && ParserCallQueue.All(call => call.Item2(call.Item1)) && !IsSkipping;
                    _innerStatus = result;
                    ParserCallQueue.Clear();
                    return result;
                }
            }

            public void Reset()
            {
                StringCaret = _initialStringCaret;
                ParserCallQueue.Clear();
                SetValid();
            }

            public void SetInvalid() => _innerStatus = false;
            public void SetValid() => _innerStatus = true;
            public void SetInnerStatus(bool status) => _innerStatus = status;

            public readonly Queue<Tuple<string, Func<string, bool>>>
                ParserCallQueue = new Queue<Tuple<string, Func<string, bool>>>();
            public int StringCaret { get; set; }
            private readonly int _initialStringCaret;
            public readonly bool IsOptional;
            public readonly bool IsSkipping;

            public Level(int stringCaret, bool isOptional, bool isSkipping)
            {
                _initialStringCaret = stringCaret;
                IsOptional = isOptional;
                IsSkipping = isSkipping;
                StringCaret = stringCaret;
                _innerStatus = true;
            }
        }
    }
}