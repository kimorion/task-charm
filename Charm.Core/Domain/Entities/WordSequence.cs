using System;
using System.Collections.Generic;

namespace Charm.Core.Domain.Entities
{
    public class WordSequence

    {
    public IWordSearchResult FirstWord { get; }
    public IWordSearchResult LastWord { get; }

    public WordSequence(IWordSearchResult first, IWordSearchResult last)
    {
        FirstWord = first ?? throw new ArgumentNullException(nameof(first));
        LastWord = last ?? throw new ArgumentNullException(nameof(last));
    }
    }
}