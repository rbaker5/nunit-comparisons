using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Narrows the type of the actual value within a constraint chain by casting it
/// to <typeparamref name="T"/>, then applies the wrapped constraint to the
/// cast result.
/// </summary>
/// <typeparam name="T">The type to cast the actual value to.</typeparam>
/// <remarks>
/// Use this when a constraint chain receives an <c>object</c> (e.g. an item
/// from a collection or the return value of a method call) but a subsequent
/// constraint requires a specific type. For example:
/// <code>
/// Has.Method(actual.Nodes).Cast&lt;XmlElement&gt;().ComparableTo(expectedElement)
/// </code>
/// If the actual value cannot be cast to <typeparamref name="T"/> at runtime,
/// an <see cref="InvalidCastException"/> is thrown. This is intentional: an
/// incompatible cast indicates an error in the constraint expression itself,
/// not a data mismatch, so it surfaces as a test error rather than a failure.
///
/// The double cast <c>(T)(object)actual</c> is a C# idiom required because the
/// compiler cannot verify a direct cast between two unconstrained generic type
/// parameters. It has the same runtime semantics as a direct cast.
/// </remarks>
public class CastConstraint<T> : PrefixConstraint, INestableConstraint
{
    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            if (BaseConstraint is INestableConstraint nestable)
            {
                nestable.Level = value;
                nestable.SkipsNewLine = true;
            }
        }
    }

    public bool SkipsNewLine { get; set; }

    private T _returnValue = default!;
    private int _level;

    public CastConstraint(IConstraint baseConstraint) : base(baseConstraint, $"cast {typeof(T)}") { }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        _returnValue = (T)(object)actual!;
        var innerResult = BaseConstraint.ApplyTo(_returnValue);
        return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
            writer =>
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                innerResult.WriteMessageTo(writer);
            });
    }

    protected override string GetStringRepresentation() => $"<cast {BaseConstraint}>";
}
