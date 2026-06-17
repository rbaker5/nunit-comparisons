using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using NUnit.Comparisons.Xml;
using NUnit.Framework;

namespace NUnit.Comparisons.Tests
{
    [TestFixture]
    public class XmlXDocumentFixture
    {
        [TestFixtureSetUp]
        public void SetupAssembly()
        {
            CompareConstraintFactory.AddAssembly(Assembly.GetAssembly(typeof (XElementConstraint)));
        }

        [Test]
        public void ReadAndCompareSame()
        {
            readAndCompare("Samples\\Sample.xml", "Samples\\Sample.xml");
        }

        [Test]
        [ExpectedException]
        public void ReadAndCompareDiffSaluation()
        {
            readAndCompare("Samples\\Sample.xml", "Samples\\DiffSalutation.xml");
        }

        private void readAndCompare(string actualPath, string expectedPath)
        {
            var old = new XmlDocument();
            old.Load(expectedPath);
            var linqDoc = XDocument.Load(actualPath);
            Assert.That(linqDoc, Is.ComparableTo(old));
        }
    }
}
