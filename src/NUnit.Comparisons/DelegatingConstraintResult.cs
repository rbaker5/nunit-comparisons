using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// A <see cref="ConstraintResult"/> whose failure message is provided by a
/// captured <see cref="Action{MessageWriter}"/> lambda rather than a per-class
/// override.
/// </summary>
/// <remarks>
/// <para>
/// NUnit 4 moved failure-message responsibility from <c>Constraint</c> to
/// <c>ConstraintResult</c>: NUnit 2 called <c>Constraint.WriteMessageTo</c>;
/// NUnit 4 calls <c>ConstraintResult.WriteMessageTo</c>. The standard NUnit
/// extension pattern is a per-constraint <c>ConstraintResult</c> subclass, but
/// this library has roughly fifteen constraint types each needing access to
/// already-evaluated inner results to compose their fragment of the nested
/// failure output. One subclass per constraint would produce a parallel class
/// hierarchy that duplicates the constraint tree.
/// </para>
/// <para>
/// <c>DelegatingConstraintResult</c> avoids that by capturing the
/// message-writing logic as a closure in the <c>writeMessage</c> parameter of
/// each <c>ApplyTo</c> call. Because the lambda is evaluated at match time, it
/// can close over the inner <c>ConstraintResult</c> already in hand:
/// </para>
/// <code>
/// var innerResult = BaseConstraint.ApplyTo(propValue);
/// return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
///     writer => {
///         writer.Write("property Name ");
///         innerResult.WriteMessageTo(writer);  // chains into the next lambda
///     });
/// </code>
/// <para>
/// If <c>innerResult</c> is itself a <c>DelegatingConstraintResult</c>, calling
/// <c>WriteMessageTo</c> on it invokes its own captured lambda, which may in
/// turn call <c>WriteMessageTo</c> on another captured result. This forms a
/// lazy chain that mirrors the constraint nesting and produces the library's
/// characteristic indented diff output without needing a class for each level.
/// </para>
/// </remarks>
internal sealed class DelegatingConstraintResult(
    IConstraint constraint,
    object? actual,
    bool isSuccess,
    Action<MessageWriter> writeMessage)
    : ConstraintResult(constraint, actual, isSuccess)
{
    public override void WriteMessageTo(MessageWriter writer) => writeMessage(writer);
}
