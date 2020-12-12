using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public void AddParser(string name, Func<List<string>, bool> parser)
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

            ResetInterpreter();
            return Start();
        }

        private readonly ILogger<CharmInterpreter> _logger;
        private readonly Stack<Level> _levelStack = new Stack<Level>();
        private readonly Stack<string> _tokenStack = new Stack<string>();
        private readonly Dictionary<string, Func<List<string>, bool>>
            _parsers = new Dictionary<string, Func<List<string>, bool>>();
        private string _originalString = "";
        private string _originalPattern = "";
        private string? _errorMessage;
        private string[] _tokens = Array.Empty<string>();
        private string[] _words = Array.Empty<string>();

        private Level CurrentLevel => _levelStack.Peek();

        private int TokenCaret
        {
            get => CurrentLevel.TokenCaret;
            set => CurrentLevel.TokenCaret = value;
        }

        private string CurrentWord => _words[CurrentLevel.StringCaret];
        private string CurrentToken => _tokens[TokenCaret];

        private ReaderState CurrentReaderState
        {
            get => CurrentLevel.ReaderState;
            set => CurrentLevel.ReaderState = value;
        }

        private ExpressionState CurrentExpressionState
        {
            get => CurrentLevel.ExpressionState;
            set => CurrentLevel.ExpressionState = value;
        }

        private bool CurrentExpressionResult
        {
            get => CurrentLevel.ExpressionResult;
            set => CurrentLevel.ExpressionResult = value;
        }

        private int CurrentWordCaret
        {
            get => CurrentLevel.StringCaret;
            set => CurrentLevel.StringCaret = value;
        }

        private int CurrentTokenCaret
        {
            get => CurrentLevel.TokenCaret;
            set => CurrentLevel.TokenCaret = value;
        }

        private bool IsSkipping()
        {
            return CurrentExpressionState == ExpressionState.And && CurrentExpressionResult == false ||
                   CurrentExpressionState == ExpressionState.Or && CurrentExpressionResult == true;
        }

        private void AppendExpressionResult(bool result)
        {
            if (CurrentExpressionState == ExpressionState.And)
            {
                CurrentExpressionResult = CurrentExpressionResult && result;
            }
            else if (CurrentExpressionState == ExpressionState.Or)
            {
                CurrentExpressionResult = CurrentExpressionResult || result;
            }
        }

        private void SetInvalid() => CurrentLevel.SetInvalid();
        private void SetValid() => CurrentLevel.SetValid();

        private bool Start()
        {
            if (CurrentReaderState != ReaderState.Reset)
                throw new InvalidOperationException("Interpreter not reset");

            while (CurrentReaderState != ReaderState.Stop)
            {
                CurrentReaderState = CurrentReaderState switch
                {
                    ReaderState.Reset => ReaderState.ReadToken,
                    ReaderState.ReadToken => ReadPatternToken(),
                    ReaderState.PushToken => ReadAndPushToken(),
                    ReaderState.CallParser => CallParser(),
                    ReaderState.Stop => ReaderState.Stop,
                    ReaderState.Error => ReturnError(),
                    _ => ReturnError("Impossible interpreter state")
                };
            }

            return CurrentExpressionResult;
        }

        private ReaderState ReadPatternToken()
        {
            if (TokenCaret >= _tokens.Length)
            {
                return ReaderState.Stop;
            }

            var nextWord = CurrentToken;

            _logger.LogDebug($"reading next pattern token: {nextWord}");

            ReaderState? result = nextWord switch
            {
                "[" => OpenLevel(),
                "]" => CloseLevel(),
                "(" => OpenLevel(),
                ")" => CloseLevel(),
                _ => null,
            };

            result ??= nextWord switch
            {
                "{" => ReaderState.PushToken,
                "}" => ReaderState.ReadToken,
                ">" => ReaderState.CallParser,
                "|" => ReaderState.ReadToken,
                "#" => MustBeAtTheEnd(),
                _ => CheckCurrentWordWithToken(),
            };

            CurrentExpressionState = CurrentToken == "|" ? ExpressionState.Or : ExpressionState.And;
            TokenCaret++;
            return result.Value;
        }

        private ReaderState MustBeAtTheEnd()
        {
            if (CurrentLevel.StringCaret != _words.Length)
            {
                CurrentLevel.SetInvalid();
                _logger.LogDebug("# => check failed, not at the end of the string");
                return ReaderState.ReadToken;
            }

            _logger.LogDebug("# => check success");
            return ReaderState.ReadToken;
        }

        private ReaderState ReadAndPushToken()
        {
            if (TokenCaret == _tokens.Length)
            {
                _errorMessage = "non closed capture group!";
                return ReaderState.Error;
            }

            if (CurrentToken == "}")
            {
                CurrentTokenCaret++;
                return ReaderState.ReadToken;
            }

            _tokenStack.Push(CurrentToken);
            _logger.LogDebug($"pushed token: {CurrentToken}");
            CurrentTokenCaret++;
            return ReaderState.PushToken;
        }

        private ReaderState CallParser()
        {
            if (IsSkipping())
            {
                _logger.LogDebug(
                    $"exp_state:{CurrentExpressionState}:{CurrentExpressionResult}  skipping parser");
                return ReaderState.ReadToken;
            }

            var wordCountString = _tokenStack.Pop();
            if (!_parsers.TryGetValue(CurrentToken, out var parser))
            {
                _errorMessage = $"parser \"{CurrentToken}\" not found";
                return ReaderState.Error;
            }

            IEnumerable<string> parserWords;
            if (wordCountString == "*")
            {
                if (CurrentWordCaret == _words.Length - 1)
                {
                    CurrentLevel.SetInvalid();
                    CurrentTokenCaret++;
                    return ReaderState.ReadToken;
                }

                parserWords = _words.Skip(CurrentWordCaret + 1);
                CurrentWordCaret = _words.Length;
            }
            else
            {
                if (!int.TryParse(wordCountString, out var wordCount) || wordCount < 0)
                {
                    _errorMessage = $"invalid parser parameter {wordCountString}";
                    return ReaderState.Error;
                }

                if (_words.Length - CurrentWordCaret < wordCount)
                {
                    _logger.LogDebug($"skipping parser {CurrentToken}: not enough words");
                    CurrentLevel.SetInvalid();
                    CurrentTokenCaret++;
                    return ReaderState.ReadToken;
                }

                parserWords = _words.Skip(CurrentWordCaret).Take(wordCount);
                CurrentWordCaret += wordCount;
            }

            var result = parser(parserWords.ToList());
            _logger.LogDebug($"called parser {CurrentToken}, result: {result}");
            AppendExpressionResult(result);
            CurrentTokenCaret++;
            return ReaderState.ReadToken;
        }

        private ReaderState CheckCurrentWordWithToken()
        {
            if (IsSkipping())
            {
                _logger.LogDebug(
                    $"exp_state:{CurrentExpressionState}:{CurrentExpressionResult}  skipping token: {CurrentToken}");
                return ReaderState.ReadToken;
            }

            if (CurrentWordCaret == _words.Length)
            {
                _logger.LogDebug($"Check failed: no more word left");
                AppendExpressionResult(false);
                return ReaderState.ReadToken;
            }

            var checkResult = CurrentWord == CurrentToken;
            if (!checkResult)
            {
                _logger.LogDebug($"Check failed: {CurrentWord} != {CurrentToken}");
                AppendExpressionResult(checkResult);
            }
            else
            {
                _logger.LogDebug($"Check succeeded: {CurrentWord} = {CurrentToken}");
                AppendExpressionResult(checkResult);
                CurrentWordCaret++;
            }

            return ReaderState.ReadToken;
        }

        private ReaderState OpenLevel()
        {
            _logger.LogDebug($"Opening new {CurrentToken} level with expr. result = {CurrentExpressionResult}");
            _levelStack.Push(new Level(
                tokenCaret: TokenCaret,
                expressionResult: CurrentExpressionState == ExpressionState.And ? CurrentExpressionResult : true,
                stringCaret: CurrentWordCaret));

            return ReaderState.ReadToken;
        }

        private ReaderState CloseLevel()
        {
            if (_levelStack.Count == 1)
            {
                _errorMessage = "attempt to close initial level";
                return ReaderState.Error;
            }

            var closingLevel = _levelStack.Pop();
            CurrentTokenCaret = closingLevel.TokenCaret;

            var isSkipping = IsSkipping();
            if (!isSkipping)
            {
                if (closingLevel.ExpressionResult)
                {
                    CurrentWordCaret = closingLevel.StringCaret;
                }

                if (CurrentToken != "]")
                {
                    AppendExpressionResult(closingLevel.ExpressionResult);
                }
            }

            var action = isSkipping ? "Skipping" : "Closing";
            _logger.LogDebug($"{action} {CurrentToken} level with expr. result = {closingLevel.ExpressionResult}");
            _logger.LogDebug($"Outer expr.result = {CurrentExpressionResult}");
            return ReaderState.ReadToken;
        }

        private ReaderState ReturnError(string? message = null)
        {
            _logger.LogError(message ?? _errorMessage ?? "Unknown interpreter error");
            return ReaderState.Stop;
        }

        private ReaderState ResetInterpreter()
        {
            var patternTokens =
                Regex.Split(_originalPattern, @"([()\[\]{}#>]|\|)|\s+", RegexOptions.Compiled)
                    .Where(w => !string.IsNullOrWhiteSpace(w)).ToArray();
            if (patternTokens.Length == 0)
            {
                _errorMessage = "pattern string was empty!";
                return ReaderState.Error;
            }

            var stringWords = _originalString.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (stringWords.Length == 0)
            {
                _errorMessage = "original string was empty!";
                return ReaderState.Error;
            }

            _logger.LogDebug($"Got tokens: {string.Join(", ", patternTokens)}");
            _logger.LogDebug($"Got words: {string.Join(", ", stringWords)}");

            _tokens = patternTokens;
            _words = stringWords;
            _levelStack.Clear();
            _levelStack.Push(new Level(stringCaret: 0, tokenCaret: 0, expressionResult: true));

            return ReaderState.ReadToken;
        }
    }
}