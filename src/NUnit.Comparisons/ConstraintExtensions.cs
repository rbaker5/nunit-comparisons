using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Extension methods on NUnit's <see cref="ConstraintExpression"/> that add
/// the library's comparison operators to the standard Assert.That DSL.
/// </summary>
public static class ConstraintExtensions
{
    /// <summary>
    /// Applies the registered <see cref="ICompareConstraint"/> for the
    /// (actual, expected) type pair. This is the primary entry point for
    /// single-object deep comparison.
    /// </summary>
    public static Constraint ComparableTo(this ConstraintExpression baseExpression, object? expected)
    {
        return baseExpression.Append(new RegisteredCompareConstraint(expected));
    }

    /// <summary>
    /// Applies an unordered collection comparison, matching each actual item
    /// against the expected item with the same name (if any) using the
    /// registered constraint for that type pair.
    /// </summary>
    public static Constraint ComparableTo(this ConstraintExpression baseExpression, IEnumerable<object> expected)
    {
        return baseExpression.Append(new CollectionComparableConstraint(expected));
    }

    /// <summary>
    /// Invokes a named method on the actual object by reflection and applies
    /// the following constraint to the return value.
    /// </summary>
    /// <remarks>
    /// Used inside <c>AddCustomConstraints</c> to compare a method's return
    /// value, e.g. <c>Has.Method(Actual.Nodes).ComparableTo(...)</c>.
    /// </remarks>
    public static ResolvableConstraintExpression Method(this ConstraintExpression baseExpression, string name, params object[] arguments)
    {
        return baseExpression.Append(new MethodOperator(name, arguments));
    }

    /// <summary>
    /// Invokes the method named by <paramref name="func"/>'s <c>Method.Name</c>
    /// on the actual object and applies the following constraint to the return value.
    /// Provides a refactor-safe alternative to the string overload:
    /// <c>Has.Method(Actual.Nodes)</c> instead of <c>Has.Method("Nodes")</c>.
    /// </summary>
    public static ResolvableConstraintExpression Method(this ConstraintExpression baseExpression, Func<object> func, params object[] arguments)
    {
        return baseExpression.Append(new MethodOperator(func.Method.Name, arguments));
    }

    /// <summary>
    /// Accesses a named property on the actual object. Used inside
    /// <c>AddCustomConstraints</c> as a drop-in for NUnit's <c>Has.Property</c>,
    /// extended to propagate <see cref="INestableConstraint"/> indentation.
    /// </summary>
    public static ResolvableConstraintExpression PropertyExt(this ConstraintExpression baseExpression, string name)
    {
        return baseExpression.Append(new PropertyExtOperator(name));
    }

    /// <summary>
    /// Narrows the type of the actual value to <typeparamref name="T"/> before
    /// applying the next constraint in the chain. Throws
    /// <see cref="InvalidCastException"/> if the actual value is not castable
    /// to <typeparamref name="T"/> — this indicates an authoring error in the
    /// constraint expression, not a data mismatch.
    /// </summary>
    public static ResolvableConstraintExpression Cast<T>(this ConstraintExpression baseExpression)
    {
        return baseExpression.Append(new CastOperator<T>());
    }

    /// <summary>
    /// Replaces the constraint's description text in failure output. Used to
    /// provide a human-readable message when a simple null-check constraint
    /// fails (e.g. <c>Is.Null.WithMessage("Expected a null name.")</c>).
    /// </summary>
    public static MessageConstraint WithMessage(this Constraint baseConstraint, string message, params object[] args)
    {
        string fullMessage = args == null ? message : string.Format(message, args);
        var constraint = new MessageConstraint(((IResolveConstraint)baseConstraint).Resolve(), fullMessage);
        return constraint;
    }
}
