using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;

namespace NUnit.Comparisons;

[Export(typeof(CompareConstraintFactory))]
public class CompareConstraintFactory
{
    // Populated by MEF after construction
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
    /// Call this once at test setup for each extension assembly. The factory
    /// reads the <c>Actual</c> and <c>Expected</c> property types from each
    /// constraint class via reflection to build a (TActual, TExpected) dispatch
    /// table — no attribute decoration is required on the constraint classes.
    ///
    /// Assemblies are scanned lazily (on first use after registration) and the
    /// cache is invalidated automatically when new assemblies are added.
    /// </remarks>
    public static void AddAssembly(Assembly assembly)
    {
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
