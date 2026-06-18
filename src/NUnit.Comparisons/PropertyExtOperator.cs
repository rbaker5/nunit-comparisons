using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public class PropertyExtOperator : SelfResolvingOperator, INestableConstraint
{
    public string Name { get; private set; }
    public int Level { get; set; }
    public bool SkipsNewLine { get; set; }

    public PropertyExtOperator(string name)
    {
        Name = name;
        left_precedence = right_precedence = 1;
    }

    public override void Reduce(ConstraintBuilder.ConstraintStack stack)
    {
        stack.Push(new PropertyExtConstraint(Name, stack.Pop()) { Level = Level, SkipsNewLine = SkipsNewLine });
    }
}
