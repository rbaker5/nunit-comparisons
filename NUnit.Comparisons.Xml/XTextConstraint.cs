using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XTextConstraint : CompareConstraint<XText, XmlText>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Value").EqualTo((Expected!.Value ?? "").Replace("\r\n", "\n")));
        }

        public override string? GetActualName(XText actual) => null;
        public override string? GetExpectedName(XmlText expected) => null;
    }
}
