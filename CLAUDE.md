# NUnit.Comparisons

NUnit constraint extensions that produce **detailed nested diff output** when complex objects don't match — collections, XML documents, and custom type pairs. The library is designed to be extended: adding a new comparable type pair requires implementing one class.

## Build and test

```bash
dotnet build NUnit.Comparisons.slnx
dotnet test NUnit.Comparisons.slnx
```

Build is clean at 0 warnings (`TreatWarningsAsErrors` is enforced via `Directory.Build.props`).

## Projects

| Project | Purpose |
|---|---|
| `src/NUnit.Comparisons` | Core constraint framework — base classes, MEF factory, operator/extension plumbing |
| `src/NUnit.Comparisons.Xml` | XML extension — compares `XDocument`/`XElement`/etc. against `XmlDocument`/`XmlElement`/etc. |
| `tests/NUnit.Comparisons.Tests` | Integration tests using the XML extension as the live exercise |

## Architecture

### Constraint hierarchy

```
NUnit Constraint (abstract, ApplyTo<TActual>)
  └── ComplexConstraint          — runs a list of sub-constraints, tracks the first failure
        └── CompareConstraint<TActual, TExpected>  — typed pair comparison; AddCustomConstraints()
              └── XDocumentConstraint, XElementConstraint, NameConstraint, ...
```

`CollectionComparableConstraint` extends NUnit's `CollectionEquivalentConstraint` directly (not `CompareConstraint`) and handles unordered collection matching with named-item tracking.

### Failure message design — DelegatingConstraintResult

NUnit 4 moved failure messages from `Constraint.WriteMessageTo()` to `ConstraintResult.WriteMessageTo()`. To preserve the nested indented diff output that is the library's core value, every `ApplyTo<TActual>()` implementation returns a `DelegatingConstraintResult` that captures a `writer =>` lambda at the time of evaluation:

```csharp
return new DelegatingConstraintResult(this, actual, innerResult.IsSuccess,
    writer => {
        writer.WriteIndent(Level);
        writer.Write("property " + _name + " ");
        innerResult.WriteMessageTo(writer);   // chains to inner constraint's message
    });
```

The lambda captures `innerResult` (another `DelegatingConstraintResult`), so failure messages compose recursively through the constraint tree.

### MEF constraint discovery

`CompareConstraintFactory` uses MEF 1 (`System.ComponentModel.Composition`) to discover `ICompareConstraint` implementations at runtime:

```csharp
// At test setup, register the assembly containing your constraints:
CompareConstraintFactory.AddAssembly(typeof(XElementConstraint).Assembly);
```

`AddAssembly` uses `RegistrationBuilder` to scan the assembly for concrete `ICompareConstraint` types and auto-reads the `Actual` and `Expected` properties to build a type-pair lookup. The factory picks the most-derived matching constraint when exact-type lookup fails.

## How to add a new comparable type pair

1. Create `MyTypeConstraint : CompareConstraint<MyActualType, MyExpectedType>` in your assembly.
2. Implement `AddCustomConstraints()` — call `Add(Has.Property(...).EqualTo(...))` etc.
3. Implement `GetActualName` / `GetExpectedName` — return a display string for error messages, or `null` for types with no meaningful identity (e.g. text nodes).
4. In test setup, call `CompareConstraintFactory.AddAssembly(typeof(MyTypeConstraint).Assembly)`.
5. Use `Assert.That(actual, Is.ComparableTo(expected))` — the factory resolves the right constraint.

See `src/NUnit.Comparisons.Xml` for a complete example with all eight constraint types.

## Key interfaces and extension points

| Type | Role |
|---|---|
| `ICompareConstraint` | Marker interface for auto-discovered constraints |
| `CompareConstraint<TA, TE>` | Base class to extend; override `AddCustomConstraints()` |
| `Has` / `Is` | Entry points for the constraint DSL inside `AddCustomConstraints()` |
| `INestableConstraint` | Implemented by constraints that carry `Level`/`SkipsNewLine` for indented output |
| `DelegatingConstraintResult` | Internal helper — wraps a `MessageWriter` lambda as a `ConstraintResult` |

## Non-obvious behaviours

### `Has.Cast<T>()` — error vs failure

`CastConstraint<T>` (used via `Has.Cast<T>()`) narrows the actual value's type within a chain:

```csharp
Has.Method(Actual.Nodes).Cast<XmlElement>().ComparableTo(expectedElement)
```

If the actual value is **not** castable to `T`, it throws `InvalidCastException` rather than returning a constraint failure. This is intentional: an incompatible cast means the constraint expression itself is wrong (a programming error), not that the data mismatched. It surfaces as a test error rather than a test failure.

The `(T)(object)actual` double-cast is a standard C# idiom for casting between two unconstrained generic type parameters — a direct `(T)actual` does not compile when neither type parameter is constrained relative to the other.

### `Has.Property` shadows NUnit's built-in

`Has.Property` in this library returns a `PropertyExtConstraint` (not NUnit's `PropertyConstraint`) so that indentation propagates through the `INestableConstraint` interface. Always use `NUnit.Comparisons.Has`, not `NUnit.Framework.Has`, inside `AddCustomConstraints`.

### Constraint initialisation is deferred

`AddCustomConstraints()` is called the **first time** a constraint instance is matched, not at construction. This allows the `Expected` value to be set via `Initialize()` before constraints are built. A consequence: adding constraints in a constructor will not work — they must go in `AddCustomConstraints()`.
