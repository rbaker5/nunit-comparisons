using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class MessageConstraint(IConstraint baseConstraint, string message) : Constraint
{
    public IConstraint BaseConstraint { get; } = baseConstraint;
    public string Message { get; } = message;

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var innerResult = BaseConstraint.ApplyTo(actual);
        return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
            writer => innerResult.WriteMessageTo(writer));
    }

    public override string Description => Message;
}
