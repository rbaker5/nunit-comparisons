using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class CastOperator<T> : SelfResolvingOperator, INestableConstraint
    {
        public int Level { get; set; }
        public bool SkipsNewLine { get; set; }

        public CastOperator()
        {
            left_precedence = right_precedence = 1;
        }

        public override void Reduce(ConstraintBuilder.ConstraintStack stack)
        {
            stack.Push(new CastConstraint<T>(stack.Pop()) { Level = Level, SkipsNewLine = SkipsNewLine });
        }
    }
}