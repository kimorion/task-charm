using System.Collections.Generic;

namespace Charm.Core.Domain.Entities
{
    public interface IWordSearchSource
    {
        public IEnumerable<IWordSearchResult> Contains(string word);
        public IWordSearchResult ContainsSingle(Word word);
        public IWordSearchResult ContainsFirst(Word word);
        public IWordSearchResult ContainsAnySingle(IEnumerable<string> word);
        public IWordSearchResult ContainsAnyFirst(IEnumerable<string> word);
        public IWordSearchResult ContainsAnySingle(IEnumerable<Word> word);
        public IWordSearchResult ContainsAnyFirst(IEnumerable<Word> word);
    }
}