using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnit.Comparisons
{
    public class Is : Framework.Is
    {
        public static RegisteredCompareConstraint ComparableTo(object expected)
        {
            return new RegisteredCompareConstraint(expected);
        }
    }
}
