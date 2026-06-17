using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class ConstraintComparer : IEqualityComparer
    {
        private readonly ConcurrentDictionary<Tuple<Type, Type>, ICompareConstraint> _reusableConstraints;
        private readonly List<Constraint> _failedConstraints;
        public int Level { get; set; }
        public bool SkipsNewLine { get; set; }

        public ConstraintComparer()
        {
            _reusableConstraints = new ConcurrentDictionary<Tuple<Type, Type>, ICompareConstraint>();
            _failedConstraints = new List<Constraint>();
        }

        public bool CanCompare(object expected, object actual)
        {
            ICompareConstraint constraint;
            return tryGetConstraint(expected, actual, out constraint);
        }

        public bool NameEquals(object expected, object actual)
        {
            ICompareConstraint constraint;
            if (!tryGetConstraint(expected, actual, out constraint)) return false;
            return String.Equals(constraint.GetExpectedName(expected), constraint.GetActualName(actual));
        }

        public bool Equals(object expected, object actual)
        {
            var typeSignature = Tuple.Create(expected.GetType(), actual.GetType());
            ICompareConstraint constraint;
            if (!tryRemoveConstraint(expected, actual, out constraint))
                return false;

            var resolvedConstraint = constraint.Resolve();
            if (resolvedConstraint.Matches(actual))
            {
                return true;
            }
            else
            {
                _failedConstraints.Add(resolvedConstraint);
                return false;
            }
        }

        private bool tryGetConstraint(object expected, object actual, out ICompareConstraint constraint)
        {
            var typeSignature = Tuple.Create(expected.GetType(), actual.GetType());
            if (!_reusableConstraints.TryGetValue(typeSignature, out constraint))
            {
                if (!CompareConstraintFactory.Instance.TryCreateConstraint(expected, actual, out constraint))
                    return false;
                
                constraint.Level = Level;
                constraint.SkipsNewLine = SkipsNewLine;
                _reusableConstraints.TryAdd(typeSignature, constraint);
            }
            return true;
        }

        private bool tryRemoveConstraint(object expected, object actual, out ICompareConstraint constraint)
        {
            var typeSignature = Tuple.Create(expected.GetType(), actual.GetType());
            if (_reusableConstraints.TryRemove(typeSignature, out constraint))
            {
                constraint.Initialize(expected);
            }
            else
            {
                if (!CompareConstraintFactory.Instance.TryCreateConstraint(expected, actual, out constraint))
                    return false;

                constraint.Level = Level;
                constraint.SkipsNewLine = SkipsNewLine;
            }
            return true;
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }

        public void WriteMessageTo(MessageWriter writer)
        {
            _failedConstraints.ForEach(constraint => constraint.WriteMessageTo(writer));
        }
    }
}