using System.Xml;
using System.Xml.Linq;
using NUnit.Comparisons.Xml;
using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class ConstraintComparerFixture
{
    [OneTimeSetUp]
    public void SetupAssembly()
    {
        CompareConstraintFactory.AddAssembly(typeof(XElementConstraint).Assembly);
    }

    // ---- Ordering tests: CanCompare and Equals in any order should work ----

    [Test]
    public void CanCompare_ThenEquals_Passes()
    {
        var comparer = new ConstraintComparer();
        var name = new XmlQualifiedName("Alice");
        var actual = XName.Get("Alice");

        Assert.That(comparer.CanCompare(name, actual), Is.True);
        Assert.That(comparer.Equals(name, actual), Is.True);
    }

    [Test]
    public void Equals_WithoutPriorCanCompare_Passes()
    {
        var comparer = new ConstraintComparer();

        Assert.That(comparer.Equals(new XmlQualifiedName("Alice"), XName.Get("Alice")), Is.True);
    }

    [Test]
    public void Equals_ThenCanCompare_ThenEquals_Passes()
    {
        var comparer = new ConstraintComparer();
        var name1 = new XmlQualifiedName("Alice");
        var name2 = new XmlQualifiedName("Bob");
        var actual = XName.Get("Bob");

        Assert.That(comparer.Equals(name1, actual), Is.False);
        Assert.That(comparer.CanCompare(name2, actual), Is.True);
        Assert.That(comparer.Equals(name2, actual), Is.True);
    }

    // ---- Multiple calls with different expected values of the same type ----

    [Test]
    public void Equals_TwiceWithDifferentExpected_EachComparesAgainstItsOwnExpected()
    {
        var comparer = new ConstraintComparer();
        var name1 = new XmlQualifiedName("Alice");
        var name2 = new XmlQualifiedName("Bob");
        var actual = XName.Get("Bob");

        Assert.That(comparer.Equals(name1, actual), Is.False); // Bob != Alice
        Assert.That(comparer.Equals(name2, actual), Is.True);  // Bob == Bob
    }

    [Test]
    public void CanCompare_MultipleTimes_ThenEquals_UsesCorrectExpected()
    {
        // After CanCompare(name1) populates the cache, CanCompare(name2) returns
        // the same cached constraint (stale Expected=name1). Equals(name2) must
        // re-initialize to name2 before invoking, otherwise it compares against name1.
        var comparer = new ConstraintComparer();
        var name1 = new XmlQualifiedName("Alice");
        var name2 = new XmlQualifiedName("Bob");
        var actual = XName.Get("Bob");

        comparer.CanCompare(name1, actual);
        comparer.CanCompare(name2, actual);
        Assert.That(comparer.Equals(name2, actual), Is.True);
    }

    // ---- Direct constraint reuse: exposes the ConstraintsSet limitation ----

    [Test, Explicit("Known bug: ConstraintsSet=true after first invocation prevents AddCustomConstraints " +
                    "from re-running on re-use. Fix requires resetting ConstraintsSet in Initialize().")]
    public void ReusedConstraintInstance_SecondInvocationComparesAgainstFirstExpected()
    {
        // AddCustomConstraints captures Expected.Name at build time (e.g. "Alice").
        // After Initialize(name2), Expected.Name is "Bob" but ConstraintsSet=true
        // prevents AddCustomConstraints from re-running. The second invocation silently
        // compares against the first expected's captured values.
        //
        // ConstraintComparer avoids this by removing the constraint from its cache after
        // each Equals call, so subsequent comparisons always get a fresh instance.
        // This test demonstrates what goes wrong when that protection is bypassed.
        var constraint = new NameConstraint();
        var name1 = new XmlQualifiedName("Alice");
        var name2 = new XmlQualifiedName("Bob");
        var actual = XName.Get("Bob");

        constraint.Initialize(name1);
        var firstResult = constraint.ApplyTo(actual);
        Assert.That(firstResult.IsSuccess, Is.False, "Bob should not match Alice");

        constraint.Initialize(name2);
        var secondResult = constraint.ApplyTo(actual);

        // This assertion FAILS: sub-constraints were built against "Alice" and ConstraintsSet
        // prevents them being rebuilt for "Bob", so the result is incorrectly false.
        Assert.That(secondResult.IsSuccess, Is.True,
            "Bob should match Bob — fails because ConstraintsSet prevents re-running AddCustomConstraints");
    }
}
