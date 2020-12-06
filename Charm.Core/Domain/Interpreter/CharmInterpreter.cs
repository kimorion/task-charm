using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Charm.Core.Domain.Interpreter
{
    public partial class CharmInterpreter
    {
        public CharmInterpreter(ILogger<CharmInterpreter> logger)
        {
            _logger = logger;
        }

        public void AddParser(string name, Func<string, bool> parser)
        {
            _parsers.Add(name, parser);
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

        private readonly ILogger<CharmInterpreter> _logger;
        private readonly Dictionary<string, Func<string, bool>>
            _parsers = new Dictionary<string, Func<string, bool>>();
        private string _originalString = "";
        private string _originalTemplate = "";

        private State _state = State.Reset;
        private readonly Stack<Level> _levels = new Stack<Level>();
        private string? _errorMessage;
        private string[] _templateWords = Array.Empty<string>();
        private string[] _stringWords = Array.Empty<string>();
        private uint _templateCaret;
        private readonly Queue<string> _templateQueue = new Queue<string>();

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
                    State.SkipLevel => SkipCurrentLevel(),
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
                "[" => OpenLevel(isOptional: true),
                "]" => CloseLevel(),
                "(" => OpenLevel(isOptional: false),
                ")" => CloseLevel(),
                "{" => State.ReadTemplateAndSave,
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
            if (currentTemplateWord == "}") return State.ReadTemplate;

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
            var wordCountString = _templateQueue.Dequeue();
            var parserName = _templateQueue.Dequeue();

            if (!_parsers.TryGetValue(parserName, out var parser))
            {
                _errorMessage = "parser not found";
                return State.Error;
            }

            if (CurrentLevel.StringCaret == _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            string? parserString;
            if (wordCountString == "*")
            {
                parserString = string.Join(" ", _stringWords.Skip(CurrentLevel.StringCaret));
                CurrentLevel.StringCaret = _stringWords.Length;
            }
            else
            {
                if (!int.TryParse(wordCountString, out var wordCount))
                {
                    _errorMessage = "invalid parser parameter";
                    return State.Error;
                }

                if (_stringWords.Length - CurrentLevel.StringCaret < wordCount)
                {
                    CurrentLevel.SetInvalid();
                    return State.ExitCurrentLevel;
                }

                parserString = string.Join(" ", _stringWords.Skip(CurrentLevel.StringCaret).Take(wordCount));
                CurrentLevel.StringCaret += wordCount;
            }

            CurrentLevel.ParserCallQueue.Enqueue(Tuple.Create(parserString, parser));
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

        private State OpenLevel(bool isOptional)
        {
            if (CurrentLevel.IsValid)
            {
                _levels.Push(new Level(CurrentLevel.StringCaret, isOptional: isOptional));
                return State.ReadTemplate;
            }
            else
            {
                return State.SkipLevel;
            }
        }

        private State SkipCurrentLevel()
        {
            if (_templateCaret == _templateWords.Length)
            {
                _errorMessage = "non closed level!";
            }

            if (CurrentTemplateWord != (CurrentLevel.IsOptional ? "]" : ")"))
            {
                _templateCaret++;
                return State.SkipLevel;
            }

            _templateCaret++;
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
            else if (!closingLevel.IsOptional)
            {
                CurrentLevel.SetInnerStatus(CurrentLevel.IsValid && closingLevel.IsValid);
            }

            return State.ReadTemplate;
        }

        private State ReturnError(string? message = null)
        {
            _logger.LogError(message ?? _errorMessage ?? "Unknown interpreter error");
            return State.Stop;
        }

        private State ExitCurrentLevel()
        {
            if (_templateCaret == _templateWords.Length)
            {
                return State.Stop;
            }

            var templateWord = CurrentTemplateWord;
            if (templateWord == (CurrentLevel.IsOptional ? "]" : "}") || templateWord == "||")
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
            _levels.Push(new Level(0, isOptional: false));
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
            _logger.LogDebug($"{string.Join(", ", array)}");
        }
    }
}