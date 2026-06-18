using System;

namespace NUnit.Comparisons;

public interface ICompareConstraintFactoryData
{
    Type ActualType { get; }
    Type ExpectedType { get; }
}
