using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public static class ConstraintExtensions
{
    public static Constraint ComparableTo(this ConstraintExpression baseExpression, object? expected)
    {
        return baseExpression.Append(new RegisteredCompareConstraint(expected));
    }

    public static Constraint ComparableTo(this ConstraintExpression baseExpression, IEnumerable<object> expected)
    {
        return baseExpression.Append(new CollectionComparableConstraint(expected));
    }

    public static ResolvableConstraintExpression Method(this ConstraintExpression baseExpression, string name, params object[] arguments)
    {
        return baseExpression.Append(new MethodOperator(name, arguments));
    }

    public static ResolvableConstraintExpression Method(this ConstraintExpression baseExpression, Func<object> func, params object[] arguments)
    {
        return baseExpression.Append(new MethodOperator(func.Method.Name, arguments));
    }

    public static ResolvableConstraintExpression PropertyExt(this ConstraintExpression baseExpression, string name)
    {
        return baseExpression.Append(new PropertyExtOperator(name));
    }

    public static ResolvableConstraintExpression Cast<T>(this ConstraintExpression baseExpression)
    {
        return baseExpression.Append(new CastOperator<T>());
    }

    public static MessageConstraint WithMessage(this Constraint baseConstraint, string message, params object[] args)
    {
        String fullMessage = args == null ? message : String.Format(message, args);
        var constraint = new MessageConstraint(((IResolveConstraint)baseConstraint).Resolve(), fullMessage);
        return constraint;
    }
}
