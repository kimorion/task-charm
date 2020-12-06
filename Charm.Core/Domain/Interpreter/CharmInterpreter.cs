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
                    State.ReadToken => ReadPatternToken(),
                    State.ReadAndSaveToken => ReadAndSavePatternToken(),
                    State.ReadAndSaveParserName => ReadAndSaveParserName(),
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

            State? result = nextWord switch
            {
                "[" => OpenLevel(isOptional: true),
                "]" => CloseLevel(),
                "(" => OpenLevel(isOptional: false),
                ")" => CloseLevel(),
                _ => null,
            };

            if (result is not null) return result.Value;

            if (CurrentLevel.IsSkipping)
            {
                _logger.LogDebug($"skipping token: {nextWord}");
                return State.ReadToken;
            }

            return nextWord switch
            {
                "{" => State.ReadAndSaveToken,
                ">" => State.ReadAndSaveParserName,
                "||" => TryNextGroup(),
                "#" => MustBeAtTheEnd(),
                _ => CheckNextWordWithToken(nextWord),
            };
        }

        private State MustBeAtTheEnd()
        {
            if (CurrentLevel.StringCaret != _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                _logger.LogDebug("# => check failed, not at the end of the string");
                return State.ReadToken;
            }

            _logger.LogDebug("# => check success");
            return State.ReadToken;
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
            if (currentPatternToken == "}") return State.ReadToken;

            _patternQueue.Enqueue(currentPatternToken);
            return State.ReadAndSaveToken;
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
                return State.ReadToken;
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
                    return State.ReadToken;
                }

                parserString = string.Join(" ", _stringWords.Skip(CurrentLevel.StringCaret).Take(wordCount));
                CurrentLevel.StringCaret += wordCount;
            }

            CurrentLevel.ParserCallQueue.Enqueue(Tuple.Create(parserString, parser));
            return State.ReadToken;
        }

        private State CheckNextWordWithToken(string token)
        {
            if (CurrentLevel.StringCaret == _stringWords.Length)
            {
                CurrentLevel.SetInvalid();
                _logger.LogDebug("Check failed: string too short");
                return State.ReadToken;
            }

            var nextWord = CurrentStringWord;
            if (nextWord != token)
            {
                _logger.LogDebug($"Check failed: {nextWord} != {token}");
                CurrentLevel.SetInvalid();
                return State.ReadToken;
            }

            CurrentLevel.StringCaret++;
            _logger.LogDebug($"Check succeeded: {nextWord} = {token}");
            return State.ReadToken;
        }

        private State TryNextGroup()
        {
            throw new NotImplementedException("Нужно реализовать как-то пропуск ненужных проверок");
            if (CurrentLevel.IsValid)
            {
                _logger.LogDebug(" || > skipping next group...");
                return State.ReadToken;
            }

            CurrentLevel.Reset();
            _logger.LogDebug(" || > trying next group...");
            return State.ReadToken;
        }

        private State OpenLevel(bool isOptional)
        {
            var levelDescription = CurrentLevel.IsSkipping ? "skipping" :
                isOptional ? "optional" : "mandatory";

            _levels.Push(new Level(
                CurrentLevel.StringCaret,
                isOptional: isOptional,
                isSkipping: CurrentLevel.IsSkipping || !CurrentLevel.IsValid));

            _logger.LogDebug($"Opening new {levelDescription} level: {_levels.Count}");

            return State.ReadToken;
        }

        private State CloseLevel()
        {
            if (_levels.Count == 1)
            {
                return State.ReadToken;
            }

            var closingLevel = _levels.Pop();

            if (!closingLevel.IsSkipping)
            {
                if (closingLevel.IsValid)
                {
                    CurrentLevel.StringCaret = closingLevel.StringCaret;
                }
                else if (!closingLevel.IsOptional)
                {
                    CurrentLevel.SetInvalid();
                }
            }

            var closingLevelDescription = closingLevel.IsOptional ? "optional" : "mandatory";
            var closingLevelValidity = closingLevel.IsSkipping ? "skipped" : closingLevel.IsValid ? "valid" : "invalid";
            _logger.LogDebug(
                $"closing {closingLevelDescription} {closingLevelValidity} level, current level: {_levels.Count}");

            return State.ReadToken;
        }

        private State ReturnError(string? message = null)
        {
            _logger.LogError(message ?? _errorMessage ?? "Unknown interpreter error");
            return State.Stop;
        }

        private State ResetInterpreter()
        {
            var patternTokens =
                Regex.Split(_originalPattern, @"([()\[\]{}#>]|\|\|)|\s+", RegexOptions.Compiled)
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

            _logger.LogDebug($"Got tokens: {string.Join(", ", patternTokens)}");
            _logger.LogDebug($"Got words: {string.Join(", ", stringWords)}");

            _patternWords = patternTokens;
            _stringWords = stringWords;
            _levels.Clear();
            _levels.Push(new Level(0, isOptional: false, isSkipping: false));
            _patternCaret = 0;

            return State.ReadToken;
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
    }
}