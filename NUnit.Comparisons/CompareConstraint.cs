using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public abstract class CompareConstraint<TActual, TExpected> : ComplexConstraint, ICompareConstraint
        where TActual : class
        where TExpected : class
    {
        private TActual? _storedActual;

        protected bool ConstraintsSet { get; private set; }
        public TExpected? Expected { get; set; }
        public TActual? Actual => _storedActual;

        public void Initialize(TExpected expected)
        {
            Expected = expected;
        }

        void ICompareConstraint.Initialize(object expected)
        {
            Initialize((TExpected)expected);
        }

        protected abstract void AddCustomConstraints();

        protected virtual void WriteActualName(MessageWriter writer)
        {
            writer.Write(GetActualName(Actual!));
        }

        protected virtual void WriteExpectedName(MessageWriter writer)
        {
            writer.Write(GetExpectedName(Expected!));
        }

        string ICompareConstraint.GetActualName(object actual) => GetActualName((TActual)actual);
        string ICompareConstraint.GetExpectedName(object expected) => GetExpectedName((TExpected)expected);

        public abstract string GetActualName(TActual actual);
        public abstract string GetExpectedName(TExpected expected);

        protected override bool InternalMatches(object actual)
        {
            if (ReferenceEquals(actual, Expected))
                return true;

            if (actual == null || Expected == null)
                return false;

            if (actual is not TActual typed)
                throw new ArgumentException(
                    $"This constraint can only compare objects of type {typeof(TActual).Name} to objects of type {typeof(TExpected).Name}, but actual type was {actual.GetType()}");

            _storedActual = typed;
            initializeConstraints();
            return base.InternalMatches(actual);
        }

        private void initializeConstraints()
        {
            if (!ConstraintsSet)
            {
                AddConstraints();
                ConstraintsSet = true;
            }
        }

        protected virtual void AddConstraints()
        {
            if (Expected == null)
                Add(Is.Null);
            else
            {
                Add(Is.Not.Null);
                AddCustomConstraints();
            }
        }

        public override string Description =>
            $"Comparison of actual type {typeof(TActual).Name} to expected type {typeof(TExpected).Name}";

        protected override void WriteFailure(MessageWriter writer)
        {
            if (Actual == null)
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write(typeof(TActual).Name);
                writer.Write(" was null when ");
                writer.Write(typeof(TExpected).Name);
                writer.Write(" {");
                WriteExpectedName(writer);
                writer.WriteLine("} was expected.");
            }
            else if (Expected == null)
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write(typeof(TActual).Name);
                writer.Write(" {");
                WriteActualName(writer);
                writer.Write("} was expected to be null.");
            }
            else
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write(typeof(TActual).Name);
                writer.Write(" {");
                WriteActualName(writer);
                writer.WriteLine("} should have matched the expected value, but did not.");
                base.WriteFailure(writer);
            }
        }
    }
}
