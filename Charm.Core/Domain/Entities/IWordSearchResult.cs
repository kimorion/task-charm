using System;
using System.Collections.Generic;

namespace Charm.Core.Domain.Entities
{
    public interface IWordSearchResult
    {
        public bool IsValid { get; }
        public Word? Word { get; }

        public bool Out<T>(Func<Word, T?> parser, out T? result) where T : struct;
        public bool Out<T>(Func<Word, T?> parser, out T? result) where T : class;
        public IWordSearchResult AtTheBeginning();
        public IWordSearchResult NotAtTheBeginning();
        public IWordSearchResult AtTheEnd();
        public IWordSearchResult NotAtTheEnd();
        public IWordGroupSearchResult ToGroup();
    }
}