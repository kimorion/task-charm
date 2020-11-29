using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Charm.Core.Domain.Entities
{
    class WordGroupSearchResult : IWordGroupSearchResult
    {
        public bool IsValid { get; }
        public Word? First { get; }
        public Word? Last { get; }

        private WordGroupSearchResult Left => First?.Prev is null
            ? throw new InvalidOperationException()
            : new WordGroupSearchResult(First.Prev, Last, IsValid);


        private WordGroupSearchResult Right => Last?.Next is null
            ? throw new InvalidOperationException()
            : new WordGroupSearchResult(First, Last.Next, IsValid);

        private WordGroupSearchResult Invalid => new WordGroupSearchResult(First, Last, false);

        public WordGroupSearchResult(Word? first, Word? last, bool isValid)
        {
            First = first;
            Last = last;
            IsValid = isValid;
        }

        public bool Build(out IWordGroupSearchResult result)
        {
            result = this;
            return IsValid;
        }

        public IWordGroupSearchResult SkipNext(Word? word = null)
        {
            if (!IsValid || Last?.Next is null) return this;

            if (word is null)
            {
                return Right;
            }

            return Last.Next == word ? Right : this;
        }

        public IWordGroupSearchResult SkipPrev(Word? word = null)
        {
            if (!IsValid || First?.Prev is null) return this;

            if (word is null)
            {
                return Left;
            }

            return First.Prev == word ? Left : this;
        }

        public IWordGroupSearchResult AtTheBeginning()
        {
            if (!IsValid || First is null) return this;
            return new WordGroupSearchResult(First, Last, First.Prev == null);
        }

        public IWordGroupSearchResult NotAtTheBeginning()
        {
            if (!IsValid || First is null) return this;
            return new WordGroupSearchResult(First, Last, First.Prev != null);
        }

        public IWordGroupSearchResult AtTheEnd()
        {
            if (!IsValid || Last is null) return this;
            return new WordGroupSearchResult(First, Last, Last.Next == null);
        }

        public IWordGroupSearchResult NotAtTheEnd()
        {
            if (!IsValid || Last is null) return this;
            return new WordGroupSearchResult(First, Last, Last.Next != null);
        }

        public IWordGroupSearchResult WithNext(Word word)
        {
            if (!IsValid || Last?.Next is null) return Invalid;

            return Last.Next == word
                ? Right
                : Invalid;
        }

        public IWordGroupSearchResult WithNext(Func<Word, bool> parseChecker)
        {
            if (!IsValid || Last?.Next is null) return Invalid;

            return parseChecker(Last.Next)
                ? Right
                : Invalid;
        }

        public IWordGroupSearchResult WithNextOut<T>(Func<Word, T?> parser, out T? parseResult) where T : struct
        {
            parseResult = null;
            if (!IsValid || Last?.Next is null) return Invalid;

            parseResult = parser(Last.Next);
            return parseResult is null
                ? Invalid
                : Right;
        }

        public IWordGroupSearchResult WithNextOut<T>(Func<Word, T?> parser, out T? parseResult) where T : class
        {
            parseResult = null;
            if (!IsValid || Last?.Next is null) return Invalid;

            parseResult = parser(Last.Next);
            return parseResult is null
                ? Invalid
                : Right;
        }

        public IWordGroupSearchResult WithPrev(Word word)
        {
            if (!IsValid || First?.Prev is null) return Invalid;

            return First.Prev == word
                ? Left
                : Invalid;
        }

        public IWordGroupSearchResult WithPrev(Func<Word, bool> parseChecker)
        {
            if (!IsValid || First?.Prev is null) return Invalid;

            return parseChecker(First.Prev)
                ? Left
                : Invalid;
        }

        public IWordGroupSearchResult WithPrevOut<T>(Func<Word, T?> parser, out T? parseResult) where T : struct
        {
            parseResult = null;
            if (!IsValid || First?.Prev is null) return Invalid;

            parseResult = parser(First.Prev);
            return parseResult is null
                ? Invalid
                : Left;
        }

        public IWordGroupSearchResult WithPrevOut<T>(Func<Word, T?> parser, out T? parseResult) where T : class
        {
            parseResult = null;
            if (!IsValid || First?.Prev is null) return Invalid;

            parseResult = parser(First.Prev);
            return parseResult is null
                ? Invalid
                : Left;
        }

        public string? GetBeginning()
        {
            if (!IsValid || First?.Prev is null) return null;

            StringBuilder builder = new StringBuilder();
            var word = First;
            while (word.Prev is not null)
            {
                word = word.Prev;
                builder.Insert(0, ' ');
                builder.Insert(0, word.Value);
            }

            return builder.ToString();
        }

        public string? GetEnding()
        {
            if (!IsValid || Last?.Next is null) return null;

            StringBuilder builder = new StringBuilder();
            var word = Last;
            while (word.Next is not null)
            {
                word = word.Next;
                builder.Insert(0, ' ');
                builder.Insert(0, word.Value);
            }

            return builder.ToString();
        }

        public IEnumerator<Word> GetEnumerator()
        {
            return new WordGroupEnumerator(First);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class WordGroupEnumerator : IEnumerator<Word>
    {
        private readonly Word? _first;
        private Word? _current;

        public WordGroupEnumerator(Word? first)
        {
            _first = first;
        }

        public bool MoveNext()
        {
            _current = _current == null ? _first : _current.Next;

            return _current != null;
        }

        public void Reset()
        {
            _current = null;
        }

        private Word Current
        {
            get => _current!;
            set => _current = value;
        }

        Word IEnumerator<Word>.Current => this.Current;

        object? IEnumerator.Current => Current;

        public void Dispose()
        {
        }
    }
}