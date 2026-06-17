using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XDocumentConstraint : CompareConstraint<XDocument, XmlDocument>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Root").ComparableTo(Expected!.DocumentElement));
            Add(Has.Property("DocumentType").EqualTo(Expected.DocumentType));
            Add(Has.Property("Declaration").ComparableTo(
                Expected.Cast<XmlNode>().SingleOrDefault(node => node.NodeType == XmlNodeType.XmlDeclaration)));
            Add(Has.Method(Actual!.Nodes).ComparableTo(
                Expected.Cast<XmlNode>().Where(node => node.NodeType != XmlNodeType.XmlDeclaration)));
        }

        public override string GetActualName(XDocument actual) => actual.BaseUri;
        public override string GetExpectedName(XmlDocument expected) => expected.BaseURI;
    }
}
