using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons.Xml
{
    public class NameConstraint : CompareConstraint<XName, XmlQualifiedName>
    {
        protected override void AddConstraints()
        {
            AddCustomConstraints();
        }

        public override bool Matches(object actual)
        {
            this.actual = actual;           
            if ((Expected == null || Expected.IsEmpty) && actual == null)
                    return true;

            if (actual == null || Expected == null)
                    return false;

            return (Expected.Namespace == Actual.NamespaceName && Expected.Name == Actual.LocalName);
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (Expected == null || Expected.IsEmpty)
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write("XName {");
                WriteActualName(writer);
                writer.Write("} was expected to be null.");
            }
            else if (Actual == null)
            {
                if (!SkipsNewLine) writer.WriteIndent(Level);
                writer.Write("XName was null when ");
                writer.Write(" {");
                WriteExpectedName(writer);
                writer.WriteLine("} was expected.");
            }
            else
            {
                writer.DisplayDifferences(this);
            }
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
                {
                    Add(new OrConstraint(Is.Null, finalConstraint));
                }
                else
                {
                    Add(Is.Not.Null);
                    Add(finalConstraint);
                }
            }
        }

        public override string GetActualName(XName actual)
        {
            return actual.ToString();
        }

        public override string GetExpectedName(XmlQualifiedName expected)
        {
            return expected.ToString();
        }
    }
}