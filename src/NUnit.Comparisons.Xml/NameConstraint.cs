using System.Xml;
using System.Xml.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons.Xml;

public class NameConstraint : CompareConstraint<XName, XmlQualifiedName>
{
    protected override void AddConstraints()
    {
        AddCustomConstraints();
    }

    protected override void AddCustomConstraints()
    {
        if (Expected == null)
            Add(Is.Null.WithMessage("Expected a null name."));
        else
        {
            var finalConstraint =
                Has.Property("LocalName").EqualTo(Expected.Name).And.Property("NamespaceName").EqualTo(
                    Expected.Namespace);

            if (Expected.IsEmpty)
                Add(new OrConstraint(Is.Null, finalConstraint));
            else
            {
                Add(Is.Not.Null);
                Add(finalConstraint);
            }
        }
    }

    public override string GetActualName(XName actual) => actual.ToString();
    public override string GetExpectedName(XmlQualifiedName expected) => expected.ToString();
}
