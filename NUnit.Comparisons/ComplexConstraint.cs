using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public abstract class ComplexConstraint : Constraint, INestableConstraint
{
    private int _level;
    private ConstraintResult? _failedResult;

    protected List<IConstraint> Constraints { get; private set; }
    protected IConstraint? FailedConstraint { get; private set; }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            foreach (var constraint in Constraints.OfType<INestableConstraint>())
            constraint.Level = Level + 1;
        }
    }

    public bool SkipsNewLine { get; set; }

    protected ComplexConstraint()
    {
        Constraints = [];
    }

    protected void Add(IResolveConstraint constraint)
    {
        var resolved = constraint.Resolve();
        if (resolved is INestableConstraint nestable)
            nestable.Level = Level + 1;
        Constraints.Add(resolved);
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        bool success = InternalMatches(actual!);
        return new DelegatingConstraintResult(this, actual, success, WriteFailure);
    }

    protected virtual bool InternalMatches(object actual)
    {
        foreach (var c in Constraints)
        {
            var result = c.ApplyTo(actual);
            if (!result.IsSuccess)
            {
                _failedResult = result;
                FailedConstraint = c;
                return false;
            }
        }
        _failedResult = null;
        FailedConstraint = null;
        return true;
    }

    protected virtual void WriteFailure(MessageWriter writer)
    {
        if (!(SkipsNewLine || FailedConstraint is INestableConstraint))
            writer.WriteIndent(Level + 1);
        _failedResult?.WriteMessageTo(writer);
    }
}
