using System;
using System.Diagnostics.CodeAnalysis;

namespace Charm.Core.Domain.Dto
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public struct Either<L, R>
        where L : class where R : class
    {
        private readonly L? _left;
        private readonly R? _right;

        public bool IsLeft => _left is not null;
        public bool IsRight => _right is not null;

        private Either(L? left)
        {
            _left = left ?? throw new ArgumentNullException(nameof(left));
            _right = null;
        }

        private Either(R? right)
        {
            _left = null;
            _right = right ?? throw new ArgumentNullException(nameof(right));
        }

        public void Match<TL, TR>(Func<R, TR> right, Func<L, TL> left)
        {
            if (_right is not null)
            {
                right(_right);
            }

            if (_left is not null)
            {
                left(_left);
            }

            throw new Exception("Either in the bottom state!");
        }

        public static Either<L, R> Left(L left)
        {
            return new Either<L, R>(left);
        }

        public static Either<L, R> Right(R right)
        {
            return new Either<L, R>(right);
        }

        public static implicit operator Either<L, R>(L left) => Left(left);

        public static implicit operator Either<L, R>(R right) => Right(right);
    }
}