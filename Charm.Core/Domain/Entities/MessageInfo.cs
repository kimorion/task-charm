using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Charm.Core.Domain.Entities
{
    public class MessageInfo : IWordSearchSource
    {
        public readonly string OriginalString;
        public readonly IReadOnlyList<Word> Words;
        public int Count => Words.Count;

        public MessageInfo(string message)
        {
            OriginalString = message;

            var splitString = message.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            Words = ConvertStringsToWords(splitString);
        }


        private static List<Word> ConvertStringsToWords(List<string> strings)
        {
            var words = strings.Select(_ => new Word(_)).ToList();

            for (var i = 1; i < words.Count; i++)
            {
                words[i].Prev = words[i - 1];
                words[i - 1].Next = words[i];
            }

            return words;
        }


        public IEnumerable<IWordSearchResult> Search(string word)
        {
            return Words.Where(_ => _ == word).Select(_ => new WordSearchResult(_, true));
        }

        public IWordSearchResult SearchSingle(Word word)
        {
            var list = Words.Where(_ => _ == word).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                1 => new WordSearchResult(list[0], true),
                _ => new WordSearchResult(list[0], false)
            };
        }

        public IWordSearchResult SearchFirst(Word word)
        {
            var list = Words.Where(_ => _ == word).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                _ => new WordSearchResult(list[0], false)
            };
        }

        public IWordSearchResult SearchAnySingle(IEnumerable<string> searchWords)
        {
            var list = Words.Where(_ => searchWords.Contains(_.Value)).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                1 => new WordSearchResult(list[0], true),
                _ => new WordSearchResult(list[0], false)
            };
        }

        public IWordSearchResult SearchAnySingle(IEnumerable<Word> word)
        {
            var list = Words.Where(word.Contains).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                1 => new WordSearchResult(list[0], true),
                _ => new WordSearchResult(list[0], false)
            };
        }

        public IWordSearchResult SearchAnyFirst(IEnumerable<string> searchWords)
        {
            var list = Words.Where(_ => searchWords.Contains(_.Value)).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                _ => new WordSearchResult(list[0], false)
            };
        }

        public IWordSearchResult SearchAnyFirst(IEnumerable<Word> searchWords)
        {
            var list = Words.Where(searchWords.Contains).ToList();
            return list.Count switch
            {
                0 => new WordSearchResult(null, false),
                _ => new WordSearchResult(list[0], false)
            };
        }
    }
}