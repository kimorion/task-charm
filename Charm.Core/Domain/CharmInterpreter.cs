using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Charm.Core.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Charm.Core.Domain
{
    public class CharmInterpreter
    {
        private enum State
        {
            Reset,
            ReadTemplate,
            ReadTemplateAndSave,
            ReadAndSaveParserName,
            SaveParserCall,
            ExitCurrentLevel,
            Stop,
            Error,
        }

        private class Level
        {
            private bool _innerStatus;

            public bool IsValid
            {
                get
                {
                    var result = _innerStatus && parserCallQueue.All(call => call.Item2(call.Item1));
                    parserCallQueue.Clear();
                    return result;
                }
            }

            public void Reset()
            {
                StringCaret = _initialStringCaret;
                parserCallQueue.Clear();
                SetValid();
            }

            public void SetInvalid() => _innerStatus = false;
            public void SetValid() => _innerStatus = true;
            public void SetInnerStatus(bool status) => _innerStatus = status;

            public Queue<Tuple<string, Func<string, bool>>>
                parserCallQueue = new Queue<Tuple<string, Func<string, bool>>>();
            public uint StringCaret { get; set; }
            private readonly uint _initialStringCaret;

            public Level(uint stringCaret)
            {
                _initialStringCaret = stringCaret;
                StringCaret = stringCaret;
                _innerStatus = true;
            }
        }

        public CharmInterpreter(string? template = null)
        {
            if (template is not null)
            {
                SetTemplate(template);
            }
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
            _originalTemplate = template ?? throw new ArgumentNullException(nameof(template));
        }

        public bool TryInterpret(string str)
        {
            _originalString = str ?? throw new ArgumentNullException(nameof(str));

            _state = State.Reset;
            return Start();
        }

        private Dictionary<string, IEnumerable<string>>
            arrays = new Dictionary<string, IEnumerable<string>>();
        private Dictionary<string, Func<string, bool>>
            parsers = new Dictionary<string, Func<string, bool>>();
        private string _originalString = "";
        private string _originalTemplate = "";

        private State _state = State.Reset;
        private Stack<Level> _levels = new Stack<Level>();
        private string? _errorMessage;
        private string[] _templateWords = Array.Empty<string>();
        private string[] _stringWords = Array.Empty<string>();
        private uint _templateCaret = 0;
        private Queue<string> _templateQueue = new Queue<string>();

        private bool Start()
        {
            if (_state != State.Reset)
                throw new InvalidOperationException("Interpreter not reset");

            while (_state != State.Stop)
            {
                _state = _state switch
                {
                    State.Reset => ResetInterpreter(),
                    State.ReadTemplate => ReadTemplate(),
                    State.ReadTemplateAndSave => ReadTemplateAndSave(),
                    State.ReadAndSaveParserName => ReadAndSaveParserName(),
                    State.ExitCurrentLevel => ExitCurrentLevel(),
                    State.SaveParserCall => SaveParserCall(),
                    State.Error => ReturnError(),
                    _ => ReturnError("Impossible interpreter state")
                };
            }

            return CurrentLevel.IsValid;
        }

        private State ReadTemplate()
        {
            if (_templateCaret == _templateWords.Length)
            {
                return State.Stop;
            }

            var nextWord = CurrentTemplateWord;
            _templateCaret++;

            var result = nextWord switch
            {
                "[" => OpenLevel(),
                "]" => CloseLevel(),
                "(" => State.ReadTemplateAndSave,
                ">" => State.ReadAndSaveParserName,
                "||" => TryAnotherGroup(),
                _ => CheckWord(nextWord),
            };

            return result;
        }

        private State ReadTemplateAndSave()
        {
            if (_templateCaret == _templateWords.Length)
            {
                _errorMessage = "non closed capture group!";
                return State.Error;
            }

            var currentTemplateWord = CurrentTemplateWord;
            _templateCaret++;
            if (currentTemplateWord == ")") return State.ReadTemplate;

            _templateQueue.Enqueue(currentTemplateWord);
            return State.ReadTemplateAndSave;
        }

        private State ReadAndSaveParserName()
        {
            if (_templateCaret == _templateWords.Length)
            {
                _errorMessage = "parser name empty!";
                return State.Error;
            }

            _templateQueue.Enqueue(CurrentTemplateWord);
            _templateCaret++;
            return State.SaveParserCall;
        }

        private State SaveParserCall()
        {
            var parserParameters = _templateQueue.Dequeue(); // for future
            var parserName = _templateQueue.Dequeue();

            if (!parsers.TryGetValue(parserName, out var parser))
            {
                _errorMessage = "parser not found";
                return State.Error;
            }

            if (CurrentLevel.StringCaret == _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            var stringWord = CurrentStringWord;
            CurrentLevel.StringCaret++;

            CurrentLevel.parserCallQueue.Enqueue(Tuple.Create(stringWord, parser));
            return State.ReadTemplate;
        }

        private State CheckWord(string templateWord)
        {
            if (CurrentLevel.StringCaret == _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            var nextWord = CurrentStringWord;
            if (nextWord != templateWord)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            CurrentLevel.StringCaret++;
            return State.ReadTemplate;
        }

        private State TryAnotherGroup()
        {
            if (CurrentLevel.IsValid)
            {
                return State.ExitCurrentLevel;
            }

            CurrentLevel.Reset();
            return State.ReadTemplate;
        }

        private State CloseLevel()
        {
            if (_levels.Count == 1)
            {
                return State.ReadTemplate;
            }

            var closingLevel = _levels.Pop();
            if (closingLevel.IsValid)
            {
                CurrentLevel.StringCaret = closingLevel.StringCaret;
            }

            return State.ReadTemplate;
        }

        private State OpenLevel()
        {
            if (CurrentLevel.IsValid)
            {
                _levels.Push(new Level(CurrentLevel.StringCaret));
            }

            return State.ReadTemplate;
        }

        private State ReturnError(string? message = null)
        {
            throw new Exception(message ?? _errorMessage ?? "Unknown interpreter error");
        }

        private State ExitCurrentLevel()
        {
            if (_templateCaret == _templateWords.Length)
            {
                return State.Stop;
            }

            var templateWord = CurrentTemplateWord;
            if (templateWord == "]" || templateWord == "||")
            {
                return State.ReadTemplate;
            }

            _templateCaret++;
            return State.ExitCurrentLevel;
        }

        private State ResetInterpreter()
        {
            var templateWords =
                Regex.Split(_originalTemplate, @"([()\[\]>]|\|\|)|\s+", RegexOptions.Compiled)
                    .Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            if (templateWords.Length == 0)
            {
                _errorMessage = "template string was empty!";
                return State.Error;
            }

            var stringWords = _originalString.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (stringWords.Length == 0)
            {
                _errorMessage = "original string was empty!";
                return State.Error;
            }

            LogArray(stringWords);
            LogArray(templateWords);

            _templateWords = templateWords;
            _stringWords = stringWords;
            _levels.Clear();
            _levels.Push(new Level(0));
            _templateCaret = 0;

            return State.ReadTemplate;
        }

        private Level CurrentLevel
        {
            get => _levels.Peek();
        }

        private string CurrentStringWord
        {
            get => _stringWords[CurrentLevel.StringCaret];
        }

        private string CurrentTemplateWord
        {
            get => _templateWords[_templateCaret];
        }

        private void LogArray(string[] array)
        {
            Console.WriteLine($"{string.Join(", ", array)}");
        }
    }
}