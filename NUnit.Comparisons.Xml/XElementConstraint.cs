using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XElementConstraint : CompareConstraint<XElement, XmlElement>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Name").Property("LocalName").EqualTo(Expected.Name));
            Add(Has.Property("Name").Property("NamespaceName").EqualTo(Expected.NamespaceURI));
            Add(Has.Method(Actual.Attributes).ComparableTo(Expected.Attributes.Cast<XmlAttribute>()));
            Add(Has.Method(Actual.Nodes).ComparableTo(Expected.Cast<XmlNode>()));
        }

        public override string GetActualName(XElement actual)
        {
            return actual.Name.ToString();
        }

        public override string GetExpectedName(XmlElement expected)
        {
            return expected.Name;
        }
    }
}