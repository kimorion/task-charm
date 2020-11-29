using System;

namespace Charm.Core.Domain.Entities
{
    public class Word : IComparable<string>, IComparable<Word>
    {
        public readonly string Value;
        public Word? Next;
        public Word? Prev;

        public static implicit operator Word(string s) => new Word(s);
        public static implicit operator string(Word w) => w.Value;

        public Word(string s)
        {
            Value = s ?? throw new ArgumentNullException(nameof(s));
            Next = null;
            Prev = null;
        }

        public int CompareTo(string? other)
        {
            return string.Compare(Value, other, StringComparison.Ordinal);
        }

        public int CompareTo(Word? other)
        {
            if (other is null) return 1;
            return string.Compare(Value, other.Value, StringComparison.Ordinal);
        }
    }
}