using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Charm.Core.Domain.Entities
{
    public interface IWordGroupSearchResult : IEnumerable<Word>
    {
        public bool IsValid { get; }
        public Word? First { get; }
        public Word? Last { get; }
        public bool Build(out IWordGroupSearchResult result);

        public IWordGroupSearchResult SkipNext(Word word);
        public IWordGroupSearchResult SkipPrev(Word word);
        public IWordGroupSearchResult AtTheBeginning();
        public IWordGroupSearchResult NotAtTheBeginning();
        public IWordGroupSearchResult AtTheEnd();
        public IWordGroupSearchResult NotAtTheEnd();
        public IWordGroupSearchResult WithNext(Word word);
        public IWordGroupSearchResult WithNext(Func<Word, bool> parseChecker);
        public IWordGroupSearchResult WithNextOut<T>(Func<Word, T?> parser, out T? parseResult) where T : struct;
        public IWordGroupSearchResult WithNextOut<T>(Func<Word, T?> parser, out T? parseResult) where T : class;
        public IWordGroupSearchResult WithPrev(Word word);
        public IWordGroupSearchResult WithPrev(Func<Word, bool> parseChecker);
        public IWordGroupSearchResult WithPrevOut<T>(Func<Word, T?> parser, out T? parseResult) where T : struct;
        public IWordGroupSearchResult WithPrevOut<T>(Func<Word, T?> parser, out T? parseResult) where T : class;
        public string? GetBeginning();
        public string? GetEnding();
        public IWordGroupSearchResult SkipAnyNext(IEnumerable<string> searchWords);
        public IWordGroupSearchResult SkipAnyNext(IEnumerable<Word> searchWords);
        public IWordGroupSearchResult SkipAnyPrev(IEnumerable<string> searchWords);
        public IWordGroupSearchResult SkipAnyPrev(IEnumerable<Word> searchWords);
    }
}