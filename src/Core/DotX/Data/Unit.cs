using System;

namespace DotX.Data
{
    public sealed class Unit
    {
        private static readonly Lazy<Unit> _valueProvider =
            new();

        public static Unit Value => _valueProvider.Value;

        private Unit()
        {}
    }
}