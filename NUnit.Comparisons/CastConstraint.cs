using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class CastConstraint<T> : PrefixConstraint, INestableConstraint
    {
        public int Level
        {
            get => _level;
            set
            {
                _level = value;
                if (BaseConstraint is INestableConstraint nestable)
                {
                    nestable.Level = value;
                    nestable.SkipsNewLine = true;
                }
            }
        }

        public bool SkipsNewLine { get; set; }

        private T _returnValue = default!;
        private int _level;

        public CastConstraint(IConstraint baseConstraint) : base(baseConstraint, $"cast {typeof(T)}") { }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            _returnValue = (T)(object)actual!;
            var innerResult = BaseConstraint.ApplyTo(_returnValue);
            return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
                writer =>
                {
                    if (!SkipsNewLine) writer.WriteIndent(Level);
                    innerResult.WriteMessageTo(writer);
                });
        }

        protected override string GetStringRepresentation() => $"<cast {BaseConstraint}>";
    }
}
