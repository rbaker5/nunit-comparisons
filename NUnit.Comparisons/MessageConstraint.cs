using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class MessageConstraint : Constraint
    {
        public Constraint BaseConstraint { get; private set; }
        public String Message { get; private set; }

        public MessageConstraint(Constraint baseConstraint, String message)
        {
            BaseConstraint = baseConstraint;
            Message = message;
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;
            return BaseConstraint.Matches(actual);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            writer.Write(Message);
            //BaseConstraint.WriteDescriptionTo(writer);
        }

        public override void WriteActualValueTo(MessageWriter writer)
        {
            BaseConstraint.WriteActualValueTo(writer);
        }
    }
}