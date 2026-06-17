using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class RegisteredCompareConstraint : Constraint, INestableConstraint
    {
        public object Expected { get; private set; }
        public ConstraintComparer Comparer { get; private set; }

        public bool SkipsNewLine
        {
            get { return _skipsNewLine; }
            set
            {
                _skipsNewLine = value;
                Comparer.SkipsNewLine = value;
            }
        }

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                Comparer.Level = value;
            }
        }

        

        private bool _canCompare;
        private int _level;
        private bool _skipsNewLine;

        public RegisteredCompareConstraint(object expected)
        {
            Expected = expected;
            Comparer = new ConstraintComparer() { Level = Level, SkipsNewLine = SkipsNewLine };
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;

            if (ReferenceEquals(actual, Expected))
                return true;

            if (actual == null || Expected == null)
                return false;

            _canCompare = Comparer.CanCompare(Expected, actual);
            if (!_canCompare)
                return false;

            return Comparer.Equals(Expected, actual);
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (actual == null || Expected == null)
            {
                base.WriteMessageTo(writer);
            }
            else if (!_canCompare)
            {
                writer.Write("The expected type ");
                writer.Write(Expected.GetType().Name);
                writer.Write(" is not comparable to the actual type ");
                writer.Write(actual.GetType().Name);
            }
            else
            {
                Comparer.WriteMessageTo(writer);
            }
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write(" comparable to ");
            writer.WriteExpectedValue(Expected);
        }
    }
}