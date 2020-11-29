using System;
using System.Collections;
using System.Collections.Generic;

namespace Charm.Core.Domain.Entities
{
    class WordSearchResult : IWordSearchResult
    {
        public bool IsValid { get; }
        public Word? Word { get; }

        public IWordGroupSearchResult ToGroup()
        {
            return new WordGroupSearchResult(Word, Word, IsValid);
        }

        private WordSearchResult Invalid
        {
            get => new WordSearchResult(Word, false);
        }

        private WordSearchResult Valid
        {
            get => new WordSearchResult(Word, true);
        }

        private WordSearchResult Check(bool isValid) => new WordSearchResult(Word, isValid);

        public WordSearchResult(Word? word, bool isValid)
        {
            Word = word;
            IsValid = isValid;
        }

        public bool Out<T>(Func<Word, T?> parser, out T? result) where T : struct
        {
            result = null;
            if (!IsValid || Word is null) return false;

            result = parser(Word);
            return true;
        }

        public bool Out<T>(Func<Word, T?> parser, out T? result) where T : class
        {
            result = null;
            if (!IsValid || Word is null) return false;

            result = parser(Word);
            return true;
        }

        public IWordSearchResult AtTheBeginning()
        {
            if (!IsValid || Word is null) return Invalid;

            return Check(Word.Prev == null);
        }

        public IWordSearchResult NotAtTheBeginning()
        {
            if (!IsValid || Word is null) return Invalid;

            return Check(Word.Prev != null);
        }

        public IWordSearchResult AtTheEnd()
        {
            if (!IsValid || Word is null) return Invalid;

            return Check(Word.Next == null);
        }

        public IWordSearchResult NotAtTheEnd()
        {
            if (!IsValid || Word is null) return Invalid;

            return Check(Word.Next != null);
        }
    }
}