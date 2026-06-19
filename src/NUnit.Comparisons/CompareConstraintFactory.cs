using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;

namespace NUnit.Comparisons;

/// <summary>
/// MEF-backed factory that resolves the correct <see cref="ICompareConstraint"/> for a
/// given (actual type, expected type) pair at runtime.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Catalog"/>, <see cref="Container"/>, <see cref="_registeredAssemblies"/>,
/// and <see cref="_instance"/> are static — shared for the lifetime of the process.
/// Assemblies registered via <see cref="AddAssembly"/> cannot be removed, and calling
/// <see cref="AddAssembly"/> is not thread-safe. In practice the whole library is
/// single-threaded, so this is not an additional constraint.
/// </para>
/// <para>
/// <see cref="AddAssembly"/> extends the MEF <see cref="Catalog"/> at any point,
/// including after <see cref="Instance"/> has already been used. This works because
/// <c>[ImportMany(AllowRecomposition = true)]</c> tells MEF to update the
/// <see cref="_factories"/> field automatically when the catalog changes. The
/// <see cref="ReferenceEquals"/> check in <see cref="ConstraintsByType"/> and
/// <see cref="ConstraintsByActual"/> detects when recomposition has occurred: MEF
/// replaces the <see cref="_factories"/> reference with a new collection, so
/// <c>ReferenceEquals(_cachedFactories, _factories)</c> becomes false and
/// <see cref="resetCache"/> rebuilds the lookup dictionaries against the updated set.
/// </para>
/// </remarks>
[Export(typeof(CompareConstraintFactory))]
public class CompareConstraintFactory
{
    // AllowRecomposition=true: MEF replaces _factories with a new IEnumerable when
    // the catalog changes. The ReferenceEquals check below detects this and triggers
    // a cache rebuild.
    [ImportMany(AllowRecomposition = true)]
    private IEnumerable<ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> _factories = null!;
    private IEnumerable<ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> _cachedFactories = null!;

    private Lazy<Dictionary<Tuple<Type, Type>, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>
        _constraintsByType = null!;
    private Lazy<Dictionary<Type, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>
        _constraintsByActualType = null!;

    public CompareConstraintFactory()
    {
        resetCache();
    }

    private static readonly AggregateCatalog Catalog = new AggregateCatalog();
    private static readonly CompositionContainer Container = new CompositionContainer(Catalog);
    private static readonly HashSet<Assembly> _registeredAssemblies = [];
    private static CompareConstraintFactory? _instance;

    /// <summary>
    /// The singleton factory instance. Initialised lazily on first access;
    /// includes all constraints in this assembly automatically.
    /// </summary>
    public static CompareConstraintFactory Instance
    {
        get
        {
            if (_instance == null)
            {
                Catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                _instance = Container.GetExportedValue<CompareConstraintFactory>();
            }
            return _instance!;
        }
    }

    /// <summary>
    /// Registers all concrete <see cref="ICompareConstraint"/> types in
    /// <paramref name="assembly"/> with the factory.
    /// </summary>
    /// <remarks>
    /// Call this at test setup for each extension assembly; repeated calls with
    /// the same assembly are ignored. The factory reads the <c>Actual</c> and
    /// <c>Expected</c> property types from each constraint class via reflection to
    /// build a (TActual, TExpected) dispatch table — no attribute decoration is
    /// required on the constraint classes.
    ///
    /// Adding an assembly after <see cref="Instance"/> has already been used is safe:
    /// MEF recomposes <see cref="_factories"/> automatically (via
    /// <c>AllowRecomposition = true</c>), and the next lookup detects the change via
    /// <see cref="ReferenceEquals"/> and rebuilds the cache.
    /// </remarks>
    public static void AddAssembly(Assembly assembly)
    {
        if (!_registeredAssemblies.Add(assembly))
            return;
        var registration = new RegistrationBuilder();
        registration.ForTypesMatching<ICompareConstraint>(findConstraints).Export(createMetaData);
        Catalog.Catalogs.Add(new AssemblyCatalog(assembly, registration));
    }

    private static bool findConstraints(Type type) =>
        type.IsClass && !type.IsAbstract && typeof(ICompareConstraint).IsAssignableFrom(type);

    private static void createMetaData(ExportBuilder exportBuilder)
    {
        exportBuilder.AsContractType<ICompareConstraint>();
        exportBuilder.AddMetadata("ActualType", getActualType);
        exportBuilder.AddMetadata("ExpectedType", getExpectedType);
    }

    // MEF guarantees ICompareConstraint implementations have Actual/Expected properties
    private static object getActualType(Type type) => type.GetProperty("Actual")!.PropertyType;
    private static object getExpectedType(Type type) => type.GetProperty("Expected")!.PropertyType;

    public bool TryCreateConstraint(object expected, object actual, out ICompareConstraint constraint)
    {
        if (TryGetFactory(expected.GetType(), actual.GetType(), out var factory))
        {
            constraint = factory!.CreateExport().Value;
            constraint.Initialize(expected);
            return true;
        }
        constraint = null!;
        return false;
    }

    /// <summary>
    /// Resolves the factory for a given (expected, actual) type pair, walking base types
    /// on both sides if no exact match is registered.
    /// </summary>
    /// <remarks>
    /// Lookup proceeds in two passes:
    /// <list type="number">
    ///   <item>Exact: looks up (expectedType, actualType) directly in the type-pair dictionary.</item>
    ///   <item>Supertype walk: climbs actualType's inheritance chain until a registered
    ///     actual type is found, then checks whether expectedType (or one of its base
    ///     types) matches that factory's registered expected type. This allows a constraint
    ///     registered for (XmlNode, XNode) to handle (XmlElement, XElement) when no more
    ///     specific constraint exists.</item>
    /// </list>
    /// </remarks>
    private bool TryGetFactory(Type expectedType, Type? actualType, out ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>? factory)
    {
        factory = null;
        if (actualType == null) return false;

        if (ConstraintsByType.TryGetValue(Tuple.Create(expectedType, actualType), out factory))
            return true;

        while (actualType != null && !ConstraintsByActual.ContainsKey(actualType))
            actualType = actualType.BaseType;

        if (actualType == null)
            return false;

        var possibleFactory = ConstraintsByActual[actualType];

        if (matchesExpectedTypeOrSuperTypes(expectedType, possibleFactory))
        {
            factory = possibleFactory;
            return true;
        }

        if (actualType == typeof(object))
            return false;

        return TryGetFactory(expectedType, actualType.BaseType, out factory);
    }

    private static bool matchesExpectedTypeOrSuperTypes(Type? expectedType, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData> possibleFactory)
    {
        while (expectedType != null)
        {
            if (expectedType == possibleFactory.Metadata.ExpectedType)
                return true;
            expectedType = expectedType.BaseType;
        }
        return false;
    }

    // ConstraintsByType and ConstraintsByActual check ReferenceEquals(_cachedFactories, _factories)
    // before each use. When MEF recomposes _factories (after AddAssembly), the reference changes
    // and resetCache() rebuilds the lazy dictionaries against the new factory set.
    private Dictionary<Tuple<Type, Type>, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> ConstraintsByType
    {
        get
        {
            if (!ReferenceEquals(_cachedFactories, _factories))
                resetCache();
            return _constraintsByType.Value;
        }
    }

    private Dictionary<Type, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> ConstraintsByActual
    {
        get
        {
            if (!ReferenceEquals(_cachedFactories, _factories))
                resetCache();
            return _constraintsByActualType.Value;
        }
    }

    private void resetCache()
    {
        _cachedFactories = _factories;
        _constraintsByType = new Lazy<Dictionary<Tuple<Type, Type>, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>(
            () => _factories.ToDictionary(item => Tuple.Create(item.Metadata.ExpectedType, item.Metadata.ActualType)));
        _constraintsByActualType = new Lazy<Dictionary<Type, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>(
            () => _factories.ToDictionary(item => item.Metadata.ActualType));
    }
}
