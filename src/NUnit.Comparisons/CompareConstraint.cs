using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Base class for constraints that compare an instance of <typeparamref name="TActual"/>
/// against an instance of <typeparamref name="TExpected"/>, producing a detailed
/// nested failure message on mismatch.
/// </summary>
/// <typeparam name="TActual">The type being tested (e.g. <c>XElement</c>).</typeparam>
/// <typeparam name="TExpected">The reference type being compared against (e.g. <c>XmlElement</c>).</typeparam>
/// <remarks>
/// Subclass this and implement <see cref="AddCustomConstraints"/> and
/// <see cref="GetActualName"/>/<see cref="GetExpectedName"/>. Register the
/// assembly with <see cref="CompareConstraintFactory.AddAssembly"/> so the
/// factory can auto-dispatch to it. See the NUnit.Comparisons.Xml project
/// for a complete worked example.
/// </remarks>
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

    /// <summary>
    /// Adds the sub-constraints that define what "equal" means for this type pair.
    /// Called once per constraint instance, after <see cref="Expected"/> is set.
    /// </summary>
    /// <remarks>
    /// Use <c>Add(Has.Property(...).EqualTo(Expected.SomeField))</c> etc. to
    /// declare each field or property that must match. Each added constraint
    /// is evaluated in order; the first failure produces the detailed message.
    ///
    /// Override <see cref="AddConstraints"/> (not this method) if you need to
    /// suppress the automatic null-guard that wraps this call.
    /// </remarks>
    protected abstract void AddCustomConstraints();

    protected virtual void WriteActualName(MessageWriter writer)
    {
        writer.Write(GetActualName(Actual!));
    }

    protected virtual void WriteExpectedName(MessageWriter writer)
    {
        writer.Write(GetExpectedName(Expected!));
    }

    string? ICompareConstraint.GetActualName(object actual) => GetActualName((TActual)actual);
    string? ICompareConstraint.GetExpectedName(object expected) => GetExpectedName((TExpected)expected);

    /// <summary>
    /// Returns a short display name for <paramref name="actual"/> used in failure
    /// messages (e.g. an element name, a URI). Return <c>null</c> for types that
    /// have no meaningful identity (e.g. text nodes, comments).
    /// </summary>
    public abstract string? GetActualName(TActual actual);

    /// <summary>
    /// Returns a short display name for <paramref name="expected"/> used in failure
    /// messages. Return <c>null</c> for types with no meaningful identity.
    /// </summary>
    public abstract string? GetExpectedName(TExpected expected);

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

    /// <summary>
    /// Adds a null-guard then calls <see cref="AddCustomConstraints"/>.
    /// Override this (not <see cref="AddCustomConstraints"/>) only when the
    /// default null-guard behaviour is inappropriate for the type pair.
    /// </summary>
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
