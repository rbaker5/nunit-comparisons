using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Extends NUnit's <see cref="NUnit.Framework.Has"/> with operators specific
/// to this library. Import this class instead of (or alongside)
/// <c>NUnit.Framework.Has</c> inside <c>AddCustomConstraints</c>
/// implementations.
/// </summary>
public class Has : Framework.Has
{
    /// <summary>
    /// Accesses a named property, propagating <see cref="INestableConstraint"/>
    /// indentation. Shadows <c>NUnit.Framework.Has.Property</c>.
    /// </summary>
    public new static ResolvableConstraintExpression Property(string name)
    {
        return new ConstraintExpression().PropertyExt(name);
    }

    /// <summary>
    /// Invokes a named method on the actual object by reflection and applies
    /// the following constraint to its return value.
    /// </summary>
    public static ResolvableConstraintExpression Method(string name, params object[] arguments)
    {
        return new ConstraintExpression().Method(name, arguments);
    }

    /// <summary>
    /// Invokes the method named by <paramref name="func"/>'s <c>Method.Name</c>,
    /// providing a refactor-safe alternative to the string overload.
    /// </summary>
    public static ResolvableConstraintExpression Method(Func<object> func, params object[] arguments)
    {
        return new ConstraintExpression().Method(func, arguments);
    }

    /// <summary>
    /// Narrows the type of the actual value to <typeparamref name="T"/> before
    /// applying the next constraint. See <see cref="CastConstraint{T}"/> for
    /// details on when this throws vs. fails.
    /// </summary>
    public static ResolvableConstraintExpression Cast<T>()
    {
        return new ConstraintExpression().Cast<T>();
    }
}
