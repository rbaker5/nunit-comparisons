# NUnit.Comparisons

NUnit constraint extensions that produce **detailed nested diff output** when complex objects don't match — collections, XML documents, and custom type pairs.

Where NUnit's built-in constraints give a pass/fail for complex objects, this library gives you the full path to the mismatch:

```
XDocument {} should have matched the expected value, but did not.
    property Root XElement {letter} should have matched the expected value, but did not.
      method Nodes Collections did not match
        XElement {salutation} should have matched the expected value, but did not.
          method Nodes Collections did not match
            XText {} should have matched the expected value, but did not.
              property Value   String lengths are both 12. Strings differ at index 1.
                Expected: "Expected Text"
                But was:  "Actual Text"
```

The library is also not limited to comparing objects of the same type, which is useful when comparing a legacy API against a replacement that must behave identically.

## Requirements

- .NET 10 or later
- NUnit 4.x

## Getting started

Reference the projects you need in your test project:

- `NUnit.Comparisons` — core constraint framework
- `NUnit.Comparisons.Xml` — XML extension (compares `XDocument`/`XElement` against `XmlDocument`/`XmlElement`)

In your test setup, register the assembly containing your constraints with the factory:

```csharp
[OneTimeSetUp]
public void Setup()
{
    CompareConstraintFactory.AddAssembly(typeof(XElementConstraint).Assembly);
}
```

Then use `Is.ComparableTo` in assertions:

```csharp
Assert.That(actualXDocument, Is.ComparableTo(expectedXmlDocument));
```

## Adding a new comparable type pair

Create a class that extends `CompareConstraint<TActual, TExpected>`:

```csharp
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
```

`AddCustomConstraints` declares what "equal" means for this type pair using the same constraint DSL you would use in `Assert.That`. `GetActualName` and `GetExpectedName` return a short display name for each object used in failure messages (return `null` for types with no meaningful identity).

Register the assembly containing your constraint in test setup (see above). The factory discovers all `CompareConstraint` subclasses automatically — no attribute decoration required.

See `src/NUnit.Comparisons.Xml` for a complete example with eight constraint types covering the full XML object model.

## Architecture and extension points

See [CLAUDE.md](CLAUDE.md) for the full architecture, the constraint hierarchy, the MEF discovery mechanism, the `DelegatingConstraintResult` pattern, and a recipe for adding new type pairs.

## Build and test

```bash
dotnet build NUnit.Comparisons.slnx
dotnet test NUnit.Comparisons.slnx
```

## License

Apache License 2.0 — see [LICENSE](LICENSE).
