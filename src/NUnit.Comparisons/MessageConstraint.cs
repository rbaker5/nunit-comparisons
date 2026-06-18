using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Wraps another constraint and replaces its description text in failure output.
/// </summary>
/// <remarks>
/// NUnit formats a constraint failure as "Expected: {description}  But was: {actual}".
/// <c>MessageConstraint</c> substitutes <see cref="Message"/> for the description,
/// allowing callers to provide a human-readable explanation of why the constraint
/// was expected to pass (e.g. "Expected a null name." instead of "Expected: null").
///
/// Used via the <c>WithMessage</c> extension method:
/// <code>
/// Add(Is.Null.WithMessage("Expected a null name."));
/// </code>
/// </remarks>
public class MessageConstraint(IConstraint baseConstraint, string message) : Constraint
{
    public IConstraint BaseConstraint { get; } = baseConstraint;
    public string Message { get; } = message;

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        var innerResult = BaseConstraint.ApplyTo(actual);
        // Use DisplayDifferences so NUnit formats the output as:
        //   Expected: <Message>
        //   But was:  <actual>
        // Delegating to innerResult.WriteMessageTo would instead show the
        // inner constraint's own description, bypassing our custom message.
        return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
            writer => writer.DisplayDifferences(new ConstraintResult(this, actual, innerResult.IsSuccess)));
    }

    public override string Description => Message;
}
