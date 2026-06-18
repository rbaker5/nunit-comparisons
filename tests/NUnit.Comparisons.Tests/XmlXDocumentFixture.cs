using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using NUnit.Comparisons.Xml;
using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class XmlXDocumentFixture
{
    [OneTimeSetUp]
    public void SetupAssembly()
    {
        CompareConstraintFactory.AddAssembly(typeof(XElementConstraint).Assembly);
    }

    [Test]
    public void ReadAndCompareSame()
    {
        ReadAndCompare(Path.Combine("Samples", "Sample.xml"), Path.Combine("Samples", "Sample.xml"));
    }

    [Test]
    public void ReadAndCompareDiffSaluation()
    {
        Assert.Throws<AssertionException>(() =>
            ReadAndCompare(Path.Combine("Samples", "Sample.xml"), Path.Combine("Samples", "DiffSalutation.xml")));
    }

    private static void ReadAndCompare(string actualPath, string expectedPath)
    {
        var old = new XmlDocument();
        old.Load(expectedPath);
        var linqDoc = XDocument.Load(actualPath);
        Assert.That(linqDoc, Is.ComparableTo(old));
    }
}
