using System;

namespace NUnit.Comparisons
{
    public interface ICompareConstraintFactory
    {
        ICompareConstraint CreateComparer(object expected);
    }
}