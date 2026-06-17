using System;
using System.Xml;
using System.Xml.Linq;

namespace NUnit.Comparisons.Xml
{
    public class XDeclarationConstraint : CompareConstraint<XDeclaration, XmlDeclaration>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Version").EqualTo(Expected!.Version));
            if (string.IsNullOrEmpty(Expected.Standalone))
                Add(Has.Property("Standalone").Null);
            else
                Add(Has.Property("Standalone").EqualTo(Expected.Standalone));
            Add(Has.Property("Encoding").EqualTo(Expected.Encoding));
        }

        public override string? GetActualName(XDeclaration actual) => null;
        public override string? GetExpectedName(XmlDeclaration expected) => null;
    }
}
