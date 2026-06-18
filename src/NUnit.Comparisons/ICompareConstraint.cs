using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Marker interface for constraints that the factory can auto-discover and
/// dispatch to by (actual type, expected type) pair.
/// </summary>
/// <remarks>
/// Implement this interface (typically by subclassing
/// <see cref="CompareConstraint{TActual,TExpected}"/>) and register the
/// containing assembly with <see cref="CompareConstraintFactory.AddAssembly"/>.
/// The factory reads the <c>Actual</c> and <c>Expected</c> property types via
/// reflection to build its dispatch table — no attribute decoration required.
/// </remarks>
public interface ICompareConstraint : IResolveConstraint, INestableConstraint
{
    /// <summary>Returns a display name for <paramref name="actual"/> in failure messages, or null if the type has no meaningful identity.</summary>
    string? GetActualName(object actual);

    /// <summary>Returns a display name for <paramref name="expected"/> in failure messages, or null if the type has no meaningful identity.</summary>
    string? GetExpectedName(object expected);

    /// <summary>Sets the expected value before the constraint is applied.</summary>
    void Initialize(object expected);
}
