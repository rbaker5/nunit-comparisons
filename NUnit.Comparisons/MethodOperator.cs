using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class MethodOperator : SelfResolvingOperator, INestableConstraint
    {
        public string Name { get; private set; }
        public object[] Arguments { get; set; }
        public int Level { get; set; }
        public bool SkipsNewLine { get; set; }

        public MethodOperator(string name, params object[] arguments)
        {
            Name = name;
            Arguments = arguments;
            left_precedence = right_precedence = 1;
        }

        public override void Reduce(ConstraintBuilder.ConstraintStack stack)
        {
            stack.Push(new MethodConstraint(stack.Pop(), Name, Arguments) { Level = Level, SkipsNewLine = SkipsNewLine });
        }
    }
}
