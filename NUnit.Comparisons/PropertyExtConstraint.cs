using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class PropertyExtConstraint : PropertyConstraint, INestableConstraint
    {
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                var nestableConstraint = baseConstraint as INestableConstraint;
                if (nestableConstraint != null)
                {
                    nestableConstraint.Level = value;
                    nestableConstraint.SkipsNewLine = true;
                }
            }
        }

        public bool SkipsNewLine { get; set; }
        private readonly string _name;
        private int _level;

        public PropertyExtConstraint(string name, Constraint baseConstraint)
            : base(name, baseConstraint)
        {
            _name = name;
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WriteIndent(Level);
            writer.WritePredicate("property " + _name);
            baseConstraint.WriteMessageTo(writer);
        }
    }
}