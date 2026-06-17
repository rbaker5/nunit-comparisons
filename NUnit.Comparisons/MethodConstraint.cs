using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class MethodConstraint
        : PrefixConstraint, INestableConstraint
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
        private readonly object[] _arguments;
        private readonly Type[] _argumentTypes;
        private object _returnValue;
        private int _level;

        public MethodConstraint(Constraint baseConstraint, string name, params object[] arguments)
            : base(baseConstraint)
        {
            _name = name;
            _arguments = arguments;
            _argumentTypes = arguments == null ? Type.EmptyTypes : arguments.Select(argument => argument.GetType()).ToArray();
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;
            if (actual == null)
                throw new ArgumentNullException("actual");
            MethodInfo method = actual.GetType().GetMethod(_name,
                                                           BindingFlags.Instance |
                                                           BindingFlags.Public |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.GetProperty,
                                                           null, _argumentTypes, null);
            if (method == null)
                throw new ArgumentException(string.Format("Method {0} was not found", _name), "name");
            _returnValue = method.Invoke(actual, _arguments);
            return baseConstraint.Matches(_returnValue);
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WriteIndent(Level);
            writer.WritePredicate("method " + _name);
            baseConstraint.WriteMessageTo(writer);
        }

        public override void WriteDescriptionTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WritePredicate("method " + _name);
            if (baseConstraint == null)
                return;
            if (baseConstraint is EqualConstraint)
                writer.WritePredicate("equal to");
            baseConstraint.WriteDescriptionTo(writer);
        }

        public override void WriteActualValueTo(MessageWriter writer)
        {
            writer.WriteActualValue(_returnValue);
        }

        protected override string GetStringRepresentation()
        {
            return string.Format("<method {0} {1}>", _name, baseConstraint);
        }
    }
}
