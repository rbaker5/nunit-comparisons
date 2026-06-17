**Project Description**
A set of libraries which extend the NUnit constraint framework to support deeper comparisons and report detailed differences useful to test debugging.  

In NUnit there are a set of classes that support the Assert.That(actual, ...) style of assertions.  This library builds and extends those classes.  Where the base classes do a very good job of letting you compare simple values, such as a string, or a number, they are less optimal for anything more complex.

If you want to compare non-ordered collections, you'll only get a pass/fail for the description, rather than a list of what items didn't match.  This library provides a way to get detailed, even nested detail about what failed.  For example:

{{XDocument {} should have matched the expected value, but did not.
    property Root XElement {letter} should have matched the expected value, but did not.
      method Nodes Collections did not match
        XElement {salutation} should have matched the expected value, but did not.
          method Nodes Collections did not match
            XText {} should have matched the expected value, but did not.
              property Value   String lengths are both 12. Strings differ at index 1.
                Expected: "Expected Text"
                But was:  "Actual Text"}} In addition, as you can see above, it's not limited to comparing the same type of object, which is useful when comparing a legacy interface or domain object to a replacement which must have similar behavior.

Using the library is fairly straightforward since it uses the existing Assert.That constructs to create additional constraints and then can evaluate these on demand.

For example:
{{
    public class XAttributeConstraint : CompareConstraint<XAttribute, XmlAttribute>
    {
        protected override void AddCustomConstraints()
        {
            Add(Has.Property("Name").Property("LocalName").EqualTo(Expected.Name));
            Add(Has.Property("Name").Property("NamespaceName")
                .EqualTo(Expected.NamespaceURI));
            Add(Has.Property("Value").EqualTo(Expected.Value));
        }

        public override string GetActualName(XAttribute actual)
        {
            return actual.Name.ToString();
        }

        public override string GetExpectedName(XmlAttribute expected)
        {
            return expected.Name;
        }
    } }} This class compares two Xml Attributes from two separate but Xml implementations.  As you can see defining the way to compare them involves defining a set of constraints, using the same classes you would have used with Assert.That and adding these to this custom classes constraint list.  Optionally as well you can provide "name" values for each object to improve descriptions of a mismatch and speed of comparisons.