using System;
using System.Diagnostics.Contracts;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public abstract class CompareConstraint<TActual, TExpected> : ComplexConstraint, ICompareConstraint 
        where TActual : class
        where TExpected : class
    {
        protected bool ConstraintsSet { get; private set; }
        public TExpected Expected { get; set; }
        public TActual Actual { get { return (TActual)actual; } }

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
            writer.Write(GetActualName(Actual));
        }

        protected virtual void WriteExpectedName(MessageWriter writer)
        {
            writer.Write(GetExpectedName(Expected));
        }

        string ICompareConstraint.GetActualName(object actual)
        {
            Contract.Assert(actual is TActual);
            return GetActualName((TActual) actual);
        }

        string ICompareConstraint.GetExpectedName(object expected)
        {
            Contract.Assert(expected is TExpected);
            return GetExpectedName((TExpected)expected);
        }

        public abstract String GetActualName(TActual actual);
        public abstract String GetExpectedName(TExpected expected);

        protected override bool InternalMatches(object actual)
        {
            if (ReferenceEquals(actual, Expected))
                return true;

            if (actual == null || Expected == null)
                return false;

            if (!(actual is TActual))
                throw new ArgumentException(GetTypeMistmatchMessage(actual));

            initializeConstraints();
            return base.InternalMatches(actual);
        }

        private static string GetTypeMistmatchMessage(object actual)
        {
                return String.Format(
                    "This constraint can only compare objects of type {0} to objects of type {1}, but actual type was {2}" ,
                    typeof (TActual).Name, typeof (TExpected).Name, actual.GetType());
            
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
            {
                Add(Is.Null);
            }
            else
            {
                Add(Is.Not.Null);
                AddCustomConstraints();
            }
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write("Comparison of actual type {0} to expected type {1}", typeof (TActual).Name,
                         typeof (TExpected).Name);
        }

        public override void WriteMessageTo(MessageWriter writer)
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
                writer.Write(typeof (TActual).Name);
                writer.Write(" {");
                WriteActualName(writer);
                writer.WriteLine("} should have matched the expected value, but did not.");
                base.WriteMessageTo(writer);
            }
        }
    }
}