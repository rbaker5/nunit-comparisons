using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public interface ICompareConstraint : IResolveConstraint, INestableConstraint
    {
        String GetActualName(object actual);
        String GetExpectedName(object expected);
        void Initialize(object expected);
    }
}