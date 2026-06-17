namespace NUnit.Comparisons;

public class Is : Framework.Is
{
    public static RegisteredCompareConstraint ComparableTo(object expected)
    {
        return new RegisteredCompareConstraint(expected);
    }
}
