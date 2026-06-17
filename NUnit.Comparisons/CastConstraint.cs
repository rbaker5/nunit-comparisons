using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class CastConstraint<T>
        : PrefixConstraint, INestableConstraint
    {
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                var nestableConstraint = baseConstraint as INestableConstraint;
                if (nestableConstraint != null)
                {
                    nestableConstraint.Level = value;
                    nestableConstraint.SkipsNewLine = true;
                }
            }
        }

        public bool SkipsNewLine { get; set; }

        private T _returnValue;
        private int _level;

        public CastConstraint(Constraint baseConstraint)
            : base(baseConstraint)
        {
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;
            _returnValue = (T)actual;
            return baseConstraint.Matches(_returnValue);
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WriteIndent(Level);
            baseConstraint.WriteMessageTo(writer);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WritePredicate("cast " + typeof(T));
            if (baseConstraint == null)
                return;
            if (baseConstraint is EqualConstraint)
                writer.WritePredicate("equal to");
            baseConstraint.WriteDescriptionTo(writer);
        }

        public override void WriteActualValueTo(MessageWriter writer)
        {
            writer.WriteActualValue(_returnValue);
        }

        protected override string GetStringRepresentation()
        {
            return string.Format("<cast {0}>", baseConstraint);
        }
    }
}
