using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class MethodConstraint : PrefixConstraint, INestableConstraint
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
        private readonly object[] _arguments;
        private readonly Type[] _argumentTypes;
        private int _level;

        public MethodConstraint(IConstraint baseConstraint, string name, params object[] arguments)
            : base(baseConstraint, $"method {name}")
        {
            _name = name;
            _arguments = arguments;
            _argumentTypes = arguments == null ? Type.EmptyTypes : arguments.Select(a => a.GetType()).ToArray();
        }

        public override ConstraintResult ApplyTo<TActual>(TActual actual)
        {
            if (actual == null) throw new ArgumentNullException(nameof(actual));

            var method = actual.GetType().GetMethod(_name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty,
                null, _argumentTypes, null);
            if (method == null)
                throw new ArgumentException($"Method {_name} was not found", nameof(_name));

            var returnValue = method.Invoke(actual, _arguments);
            var innerResult = BaseConstraint.ApplyTo(returnValue);
            return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
                writer =>
                {
                    if (!SkipsNewLine) writer.WriteIndent(Level);
                    writer.Write("  method " + _name + " ");
                    innerResult.WriteMessageTo(writer);
                });
        }

        protected override string GetStringRepresentation() => $"<method {_name} {BaseConstraint}>";
    }
}
