using System;
using System.Collections.Generic;
using System.Linq;
using Charm.Core.Domain.Entities;

namespace Charm.Core.Domain
{
    public class CharmInterpreter
    {
        private enum State
        {
            Reset,
            Read,
            Exit,
            Stop,
            Error,
        }

        private class Level
        {
            private bool _innerStatus;

            public bool IsValid
            {
                get { return _innerStatus && parserCallQueue.All(call => call.Item2(call.Item1)); }
            }

            public Word CurrentWord { get; set; }
            public Queue<Tuple<string, Func<string, bool>>>
                parserCallQueue = new Queue<Tuple<string, Func<string, bool>>>();

            public Level(Word currentWord)
            {
                CurrentWord = currentWord;
                _innerStatus = true;
            }
        }

        public CharmInterpreter()
        {
        }

        public void AddArray(string name, IEnumerable<string> array)
        {
            arrays.Add(name, array);
        }

        public void AddParser(string name, Func<string, bool> parser)
        {
            parsers.Add(name, parser);
        }

        public void SetTemplate(string template)
        {
            //todo check brackets count
            _template = template ?? throw new ArgumentNullException(nameof(template));
        }

        public void Reset()
        {
            _state = State.Reset;
            _caret = 0;
        }

        public void Interpret(string str)
        {
            _originalString = str ?? throw new ArgumentNullException(nameof(str));
            Start();
        }

        private Dictionary<string, IEnumerable<string>>
            arrays = new Dictionary<string, IEnumerable<string>>();
        private Dictionary<string, Func<string, bool>>
            parsers = new Dictionary<string, Func<string, bool>>();
        private string _originalString = "";

        private State _state = State.Reset;
        private Stack<Level> _levels = new Stack<Level>();
        private string _template = "";
        private string? _errorMessage;
        private uint _caret = 0;


        private void Start()
        {
            if (_state != State.Reset || _caret != 0) throw new InvalidOperationException("Interpreter not reset");

            while (_state != State.Stop)
            {
                _state = _state switch
                {
                    State.Reset => State.Read,
                    State.Read => Read(),
                    State.Exit => Exit(),
                    State.Stop => Error("interpreter was in stop state"),
                    State.Error => Error(),
                    _ => Error("Impossible interpreter state")
                };
            }
        }

        private State Read()
        {
            throw new NotImplementedException();
        }

        private State Error(string? message = null)
        {
            throw new Exception(message ?? _errorMessage ?? "Unknown interpreter error");
        }

        private State Exit()
        {
            throw new NotImplementedException();
        }
    }
}