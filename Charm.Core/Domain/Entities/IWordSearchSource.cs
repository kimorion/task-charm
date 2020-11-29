using System.Collections.Generic;

namespace Charm.Core.Domain.Entities
{
    public interface IWordSearchSource
    {
        public IEnumerable<IWordSearchResult> Search(string word);
        public IWordSearchResult SearchSingle(Word word);
        public IWordSearchResult SearchFirst(Word word);
        public IWordSearchResult SearchAnySingle(IEnumerable<string> searchWords);
        public IWordSearchResult SearchAnyFirst(IEnumerable<string> searchWords);
        public IWordSearchResult SearchAnySingle(IEnumerable<Word> word);
        public IWordSearchResult SearchAnyFirst(IEnumerable<Word> searchWords);
    }
}