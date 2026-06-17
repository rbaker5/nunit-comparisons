using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public abstract class ComplexConstraint : Constraint, INestableConstraint
    {
        private int _level;
        protected List<Constraint> Constraints { get; private set; }
        protected Constraint FailedConstraint { get; private set; }
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                foreach (var constraint in Constraints.OfType<INestableConstraint>())
                {
                    constraint.Level = Level + 1;
                }
            }
        }

        public bool SkipsNewLine { get; set; }

        protected ComplexConstraint()
        {
            Constraints = new List<Constraint>();
        }

        protected void Add(IResolveConstraint constraint)
        {
            Constraint resolvedConstraint = constraint.Resolve();
            var nestableConstraint = resolvedConstraint as INestableConstraint;
            if (nestableConstraint != null)
            {
                nestableConstraint.Level = Level + 1;
            }
            Constraints.Add(resolvedConstraint);
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;
            return InternalMatches(actual);
        }

        protected virtual bool InternalMatches(object actual)
        {
            FailedConstraint =
                Constraints.FirstOrDefault(constraint => !(constraint.Matches(actual)));
            return (FailedConstraint == null);
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (!(SkipsNewLine || FailedConstraint is INestableConstraint)) writer.WriteIndent(Level + 1);
            FailedConstraint.WriteMessageTo(writer);
        }
    }
}