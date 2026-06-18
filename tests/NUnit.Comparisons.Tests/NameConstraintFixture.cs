using System.Xml;
using System.Xml.Linq;
using NUnit.Comparisons.Xml;
using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class NameConstraintFixture
{
    // Note: the XmlQualifiedName.Empty (IsEmpty=true) path in AddCustomConstraints
    // adds OrConstraint(Is.Null, ...) to allow a null actual. In practice this path
    // is unreachable: CompareConstraint.InternalMatches returns false on null actual
    // before constraints are evaluated, and XName.Get("") throws ArgumentException
    // so an empty XName cannot be constructed. No test covers that branch.

    private static NameConstraint ConstraintFor(XmlQualifiedName expected)
    {
        var constraint = new NameConstraint();
        constraint.Initialize(expected);
        return constraint;
    }

    [Test]
    public void MatchingLocalName_NoNamespace_Passes()
    {
        Assert.That(XName.Get("local"), ConstraintFor(new XmlQualifiedName("local")));
    }

    [Test]
    public void MatchingLocalName_WithNamespace_Passes()
    {
        var xname = XName.Get("{http://example.com}local");
        Assert.That(xname, ConstraintFor(new XmlQualifiedName("local", "http://example.com")));
    }

    [Test]
    public void DifferentLocalName_Fails()
    {
        Assert.Throws<AssertionException>(() =>
            Assert.That(XName.Get("actual"), ConstraintFor(new XmlQualifiedName("expected"))));
    }

    [Test]
    public void DifferentNamespace_Fails()
    {
        var xname = XName.Get("{http://actual.com}local");
        Assert.Throws<AssertionException>(() =>
            Assert.That(xname, ConstraintFor(new XmlQualifiedName("local", "http://expected.com"))));
    }

    [Test]
    public void ActualIsNull_Fails()
    {
        Assert.Throws<AssertionException>(() =>
            Assert.That((XName?)null, ConstraintFor(new XmlQualifiedName("local"))));
    }
}
