using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class Has : Framework.Has
    {
        public new static ResolvableConstraintExpression Property(string name)
        {
            return new ConstraintExpression().PropertyExt(name);
        }

        public static ResolvableConstraintExpression Method(string name, params object[] arguments)
        {
            return new ConstraintExpression().Method(name, arguments);
        }

        public static ResolvableConstraintExpression Method(Func<object> func, params object[] arguments)
        {
            return new ConstraintExpression().Method(func, arguments);
        }

        public static ResolvableConstraintExpression Cast<T>()
        {
            return new ConstraintExpression().Cast<T>();
        }
    }
}
