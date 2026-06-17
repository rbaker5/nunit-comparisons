using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XAttributeConstraint : CompareConstraint<XAttribute, XmlAttribute>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Name").Property("LocalName").EqualTo(Expected!.Name));
            Add(Has.Property("Name").Property("NamespaceName").EqualTo(Expected.NamespaceURI));
            Add(Has.Property("Value").EqualTo(Expected.Value));
        }

        public override string GetActualName(XAttribute actual) => actual.Name.ToString();
        public override string GetExpectedName(XmlAttribute expected) => expected.Name;
    }
}
