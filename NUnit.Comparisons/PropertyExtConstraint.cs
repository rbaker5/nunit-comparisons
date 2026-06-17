using System.Reflection;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class PropertyExtConstraint : PropertyConstraint, INestableConstraint
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
    private readonly string _name;
    private int _level;

    public PropertyExtConstraint(string name, IConstraint baseConstraint) : base(name, baseConstraint)
    {
        _name = name;
    }

    public override ConstraintResult ApplyTo<TActual>(TActual actual)
    {
        if (actual == null)
            return new ConstraintResult(this, actual, false);

        var prop = actual.GetType().GetProperty(_name,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (prop == null)
            return new ConstraintResult(this, actual, false);

        var propValue = prop.GetValue(actual, null);
        var innerResult = BaseConstraint.ApplyTo(propValue);
        return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
            writer =>
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write("property " + _name + " ");
                innerResult.WriteMessageTo(writer);
            });
    }
}
