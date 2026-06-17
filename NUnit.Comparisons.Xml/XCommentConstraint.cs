using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XCommentConstraint : CompareConstraint<XComment, XmlComment>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Value").EqualTo((Expected!.Value ?? "").Replace("\r\n", "\n")));
        }

        public override string? GetActualName(XComment actual) => null;
        public override string? GetExpectedName(XmlComment expected) => null;
    }
}
