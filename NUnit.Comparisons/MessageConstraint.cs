using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class MessageConstraint : Constraint
{
    public IConstraint BaseConstraint { get; private set; }
    public string Message { get; private set; }

    public MessageConstraint(IConstraint baseConstraint, string message)
    {
        BaseConstraint = baseConstraint;
        Message = message;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var innerResult = BaseConstraint.ApplyTo(actual);
        return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
            writer => innerResult.WriteMessageTo(writer));
    }

    public override string Description => Message;
}
