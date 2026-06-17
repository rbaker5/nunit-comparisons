using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class RegisteredCompareConstraint : Constraint, INestableConstraint
{
    public object? Expected { get; private set; }
    public ConstraintComparer Comparer { get; private set; }

    public bool SkipsNewLine
    {
        get => _skipsNewLine;
        set
        {
            _skipsNewLine = value;
            Comparer.SkipsNewLine = value;
        }
    }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            Comparer.Level = value;
        }
    }

    private bool _canCompare;
    private int _level;
    private bool _skipsNewLine;

    public RegisteredCompareConstraint(object? expected)
    {
        Expected = expected;
        Comparer = new ConstraintComparer { Level = Level, SkipsNewLine = SkipsNewLine };
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (ReferenceEquals(actual, Expected))
            return new ConstraintResult(this, actual, true);

        if (actual == null || Expected == null)
            return new ConstraintResult(this, actual, false);

        _canCompare = Comparer.CanCompare(Expected, actual);
        if (!_canCompare)
        {
            return new DelegatingConstraintResult(this, actual, false,
                writer =>
                {
                    writer.Write("The expected type ");
                    writer.Write(Expected.GetType().Name);
                    writer.Write(" is not comparable to the actual type ");
                    writer.Write(actual!.GetType().Name);
                });
        }

        bool success = Comparer.Equals(Expected, actual);
        return new DelegatingConstraintResult(this, actual, success,
            writer => Comparer.WriteMessageTo(writer));
    }

    public override string Description => $"comparable to {Expected}";
}
