namespace NUnit.Comparisons;

/// <summary>
/// Extends NUnit's <see cref="NUnit.Framework.Is"/> with <see cref="ComparableTo"/>,
/// the primary entry point for deep comparison assertions in test code.
/// </summary>
/// <example>
/// <code>
/// Assert.That(actualXDocument, Is.ComparableTo(expectedXmlDocument));
/// </code>
/// </example>
public class Is : Framework.Is
{
    /// <summary>
    /// Creates a constraint that compares <paramref name="expected"/> against
    /// the actual value using the registered <see cref="ICompareConstraint"/>
    /// for that type pair.
    /// </summary>
    public static RegisteredCompareConstraint ComparableTo(object expected)
    {
        return new RegisteredCompareConstraint(expected);
    }
}
