using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Compares items from two collections using the registered
/// <see cref="ICompareConstraint"/> for each (expected, actual) type pair.
/// Used by <see cref="NamedCollectionTally"/> to drive unordered collection matching.
/// </summary>
/// <remarks>
/// <para>
/// Implements <see cref="IEqualityComparer"/> for use with NUnit's
/// <c>CollectionEquivalentConstraint.Using()</c>. Only <see cref="Equals"/> is
/// meaningful — <see cref="GetHashCode"/> throws <see cref="NotImplementedException"/>
/// because the comparer is never used for hashing.
/// </para>
/// <para>
/// <b>Cache design.</b> <c>_reusableConstraints</c> holds one constraint instance per
/// (expected type, actual type) pair. The cache is asymmetric by design:
/// </para>
/// <list type="bullet">
///   <item>
///     <see cref="tryGetConstraint"/> (used by <see cref="CanCompare"/> and
///     <see cref="NameEquals"/>) reads or creates a cached entry via
///     <c>GetOrAdd</c> and returns it without removing it. These callers only need
///     the constraint to check type compatibility or extract names —
///     <see cref="ICompareConstraint.GetExpectedName"/> and
///     <see cref="ICompareConstraint.GetActualName"/> are called with the current
///     expected/actual as parameters, so a stale <c>Expected</c> value on the cached
///     instance does not affect correctness.
///   </item>
///   <item>
///     <see cref="tryRemoveConstraint"/> (used by <see cref="Equals"/>) removes the
///     entry from the cache and calls <see cref="ICompareConstraint.Initialize"/> with
///     the current expected before invoking. The remove is necessary because invocation
///     sets <c>ConstraintsSet = true</c> on <see cref="CompareConstraint{TActual,TExpected}"/>;
///     subsequent comparisons of the same type pair need a fresh instance.
///     The re-initialize corrects any stale <c>Expected</c> that may have been left in
///     the cache by a prior <see cref="CanCompare"/> call where names did not match and
///     <see cref="Equals"/> was never reached.
///   </item>
/// </list>
/// </remarks>
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

    /// <summary>
    /// Returns true if a constraint is registered for the (expected, actual) type pair.
    /// </summary>
    public bool CanCompare(object expected, object actual)
    {
        return tryGetConstraint(expected, actual, out _);
    }

    /// <summary>
    /// Returns true if the registered constraint reports matching names for these two objects.
    /// Used by <see cref="NamedCollectionTally"/> to distinguish a name mismatch from a
    /// content mismatch before running the full comparison.
    /// </summary>
    public bool NameEquals(object expected, object actual)
    {
        if (!tryGetConstraint(expected, actual, out var constraint)) return false;
        return string.Equals(constraint.GetExpectedName(expected), constraint.GetActualName(actual));
    }

    /// <summary>
    /// Applies the registered constraint for the (expected, actual) type pair.
    /// Failed results are accumulated and written by <see cref="WriteMessageTo"/>.
    /// </summary>
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

    // Reads or creates the cached constraint for the type pair without consuming it.
    // Stale Expected on the cached instance is harmless here because GetExpectedName
    // and GetActualName receive the current values as parameters.
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

    // Removes and re-initializes the cached constraint before invoking it.
    // Remove: ensures the next comparison of this type pair gets a fresh instance
    //   (ConstraintsSet=true after invocation cannot be reset on the same instance).
    // Re-initialize: corrects a stale Expected left by a prior CanCompare where names
    //   did not match and Equals was never called to clear the cache slot.
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

    /// <summary>
    /// Not implemented. This comparer is used only for equality, never for hashing.
    /// </summary>
    public int GetHashCode(object obj) => throw new NotImplementedException();

    /// <summary>
    /// Writes the accumulated failure results from all failed <see cref="Equals"/> calls.
    /// </summary>
    public void WriteMessageTo(MessageWriter writer)
    {
        _failedResults.ForEach(r => r.WriteMessageTo(writer));
    }
}
