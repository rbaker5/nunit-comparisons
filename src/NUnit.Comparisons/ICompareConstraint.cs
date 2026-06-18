using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public interface ICompareConstraint : IResolveConstraint, INestableConstraint
{
    string? GetActualName(object actual);
    string? GetExpectedName(object expected);
    void Initialize(object expected);
}
