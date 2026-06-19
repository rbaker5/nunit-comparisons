using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class ConstraintComparer : IEqualityComparer
{
    private readonly ConcurrentDictionary<Tuple<Type, Type>, ICompareConstraint> _reusableConstraints;
    private readonly List<ConstraintResult> _failedResults;
    public int Level { get; set; }
    public bool SkipsNewLine { get; set; }

    public ConstraintComparer()
    {
        _reusableConstraints = new ConcurrentDictionary<Tuple<Type, Type>, ICompareConstraint>();
        _failedResults = [];
    }

    public bool CanCompare(object expected, object actual)
    {
        return tryGetConstraint(expected, actual, out _);
    }

    public bool NameEquals(object expected, object actual)
    {
        if (!tryGetConstraint(expected, actual, out var constraint)) return false;
        return string.Equals(constraint.GetExpectedName(expected), constraint.GetActualName(actual));
    }

    public new bool Equals(object? expected, object? actual)
    {
        if (expected == null || actual == null) return ReferenceEquals(expected, actual);
        if (!tryRemoveConstraint(expected, actual, out var constraint))
            return false;

        var result = constraint.Resolve().ApplyTo(actual);
        if (result.IsSuccess)
            return true;

        _failedResults.Add(result);
        return false;
    }

    private bool tryGetConstraint(object expected, object actual, out ICompareConstraint constraint)
    {
        var typeSignature = Tuple.Create(expected.GetType(), actual.GetType());
        constraint = _reusableConstraints.GetOrAdd(typeSignature, _ => {
            if (!CompareConstraintFactory.Instance.TryCreateConstraint(expected, actual, out var newConstraint))
                return null!;

            newConstraint.Level = Level;
            newConstraint.SkipsNewLine = SkipsNewLine;
            return newConstraint;
        });
        return constraint != null;
    }

    private bool tryRemoveConstraint(object expected, object actual, out ICompareConstraint constraint)
    {
        var typeSignature = Tuple.Create(expected.GetType(), actual.GetType());
        if (_reusableConstraints.TryRemove(typeSignature, out constraint!))
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

    public int GetHashCode(object obj) => throw new NotImplementedException();

    public void WriteMessageTo(MessageWriter writer)
    {
        _failedResults.ForEach(r => r.WriteMessageTo(writer));
    }
}
