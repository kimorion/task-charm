using System;

namespace Charm.Core.Domain.Entities
{
    public class Word : IComparable<string>, IComparable<Word>, IEquatable<Word>, IEquatable<string>
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

        public static bool operator ==(Word? left, Word? right)
        {
            if (left is null && right is null) return true;
            if (left is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Word? left, Word? right)
        {
            return !(left == right);
        }

        public bool Equals(Word? other)
        {
            return other is not null && Value.Equals(other.Value);
        }

        public bool Equals(string? other)
        {
            return other is not null && Value.Equals(other);
        }
    }
}