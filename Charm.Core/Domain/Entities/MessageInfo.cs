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


        public IEnumerable<IWordSearchResult> Contains(string word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsSingle(Word word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsFirst(Word word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsSingle(string word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsFirst(string word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsAnySingle(IEnumerable<string> word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsAnyFirst(IEnumerable<string> word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsAnySingle(IEnumerable<Word> word)
        {
            throw new NotImplementedException();
        }

        public IWordSearchResult ContainsAnyFirst(IEnumerable<Word> word)
        {
            throw new NotImplementedException();
        }
    }
}