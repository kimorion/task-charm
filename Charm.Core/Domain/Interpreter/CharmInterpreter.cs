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

        public void SetPattern(string pattern)
        {
            //todo check brackets count
            _originalPattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
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
        private string _originalPattern = "";

        private State _state = State.Reset;
        private readonly Stack<Level> _levels = new Stack<Level>();
        private string? _errorMessage;
        private string[] _patternWords = Array.Empty<string>();
        private string[] _stringWords = Array.Empty<string>();
        private uint _patternCaret;
        private readonly Queue<string> _patternQueue = new Queue<string>();

        private bool Start()
        {
            if (_state != State.Reset)
                throw new InvalidOperationException("Interpreter not reset");

            while (_state != State.Stop)
            {
                _state = _state switch
                {
                    State.Reset => ResetInterpreter(),
                    State.ReadPatternToken => ReadPatternToken(),
                    State.ReadAndSavePatternToken => ReadAndSavePatternToken(),
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

        private State ReadPatternToken()
        {
            if (_patternCaret == _patternWords.Length)
            {
                return State.Stop;
            }

            var nextWord = CurrentPatternToken;
            _patternCaret++;

            _logger.LogDebug($"reading next pattern token: {nextWord}");

            var result = nextWord switch
            {
                "[" => OpenLevel(isOptional: true),
                "]" => CloseLevel(),
                "(" => OpenLevel(isOptional: false),
                ")" => CloseLevel(),
                "{" => State.ReadAndSavePatternToken,
                ">" => State.ReadAndSaveParserName,
                "||" => TryAnotherGroup(),
                _ => CheckNextWordWithToken(nextWord),
            };

            return result;
        }

        private State ReadAndSavePatternToken()
        {
            if (_patternCaret == _patternWords.Length)
            {
                _errorMessage = "non closed capture group!";
                return State.Error;
            }

            var currentPatternToken = CurrentPatternToken;
            _patternCaret++;
            if (currentPatternToken == "}") return State.ReadPatternToken;

            _patternQueue.Enqueue(currentPatternToken);
            return State.ReadAndSavePatternToken;
        }

        private State ReadAndSaveParserName()
        {
            if (_patternCaret == _patternWords.Length)
            {
                _errorMessage = "parser name empty!";
                return State.Error;
            }

            _patternQueue.Enqueue(CurrentPatternToken);
            _patternCaret++;
            return State.SaveParserCall;
        }

        private State SaveParserCall()
        {
            var wordCountString = _patternQueue.Dequeue();
            var parserName = _patternQueue.Dequeue();

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
            return State.ReadPatternToken;
        }

        private State CheckNextWordWithToken(string token)
        {
            if (CurrentLevel.StringCaret == _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            var nextWord = CurrentStringWord;
            if (nextWord != token)
            {
                CurrentLevel.SetInvalid();
                return State.ExitCurrentLevel;
            }

            CurrentLevel.StringCaret++;
            return State.ReadPatternToken;
        }

        private State TryAnotherGroup()
        {
            if (CurrentLevel.IsValid)
            {
                return State.ExitCurrentLevel;
            }

            CurrentLevel.Reset();
            return State.ReadPatternToken;
        }

        private State OpenLevel(bool isOptional)
        {
            if (CurrentLevel.IsValid)
            {
                _levels.Push(new Level(CurrentLevel.StringCaret, isOptional: isOptional));
                return State.ReadPatternToken;
            }
            else
            {
                return State.SkipLevel;
            }
        }

        private State SkipCurrentLevel()
        {
            if (_patternCaret == _patternWords.Length)
            {
                _errorMessage = "non closed level!";
            }

            if (CurrentPatternToken != (CurrentLevel.IsOptional ? "]" : ")"))
            {
                _patternCaret++;
                return State.SkipLevel;
            }

            _patternCaret++;
            return State.ReadPatternToken;
        }

        private State CloseLevel()
        {
            if (_levels.Count == 1)
            {
                return State.ReadPatternToken;
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

            return State.ReadPatternToken;
        }

        private State ReturnError(string? message = null)
        {
            _logger.LogError(message ?? _errorMessage ?? "Unknown interpreter error");
            return State.Stop;
        }

        private State ExitCurrentLevel()
        {
            if (_patternCaret == _patternWords.Length)
            {
                return State.Stop;
            }

            var patternToken = CurrentPatternToken;
            if (patternToken == (CurrentLevel.IsOptional ? "]" : "}") || patternToken == "||")
            {
                return State.ReadPatternToken;
            }

            _patternCaret++;
            return State.ExitCurrentLevel;
        }

        private State ResetInterpreter()
        {
            var patternTokens =
                Regex.Split(_originalPattern, @"([()\[\]>]|\|\|)|\s+", RegexOptions.Compiled)
                    .Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            if (patternTokens.Length == 0)
            {
                _errorMessage = "pattern string was empty!";
                return State.Error;
            }

            var stringWords = _originalString.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (stringWords.Length == 0)
            {
                _errorMessage = "original string was empty!";
                return State.Error;
            }

            LogArray(stringWords);
            LogArray(patternTokens);

            _patternWords = patternTokens;
            _stringWords = stringWords;
            _levels.Clear();
            _levels.Push(new Level(0, isOptional: false));
            _patternCaret = 0;

            return State.ReadPatternToken;
        }

        private Level CurrentLevel
        {
            get => _levels.Peek();
        }

        private string CurrentStringWord
        {
            get => _stringWords[CurrentLevel.StringCaret];
        }

        private string CurrentPatternToken
        {
            get => _patternWords[_patternCaret];
        }

        private void LogArray(string[] array)
        {
            _logger.LogDebug($"{string.Join(", ", array)}");
        }
    }
}