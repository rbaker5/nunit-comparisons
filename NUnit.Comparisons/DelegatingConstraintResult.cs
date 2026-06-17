using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

internal sealed class DelegatingConstraintResult(
    IConstraint constraint,
    object? actual,
    bool isSuccess,
    Action<MessageWriter> writeMessage)
    : ConstraintResult(constraint, actual, isSuccess)
{
    public override void WriteMessageTo(MessageWriter writer) => writeMessage(writer);
}
