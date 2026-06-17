using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Linq;
using System.Reflection;

namespace NUnit.Comparisons
{
    [Export(typeof (CompareConstraintFactory))]
    public class CompareConstraintFactory
    {
        [ImportMany(AllowRecomposition = true)]
        private IEnumerable<ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> _factories;
        private IEnumerable<ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>> _cachedFactories;

        private Lazy<Dictionary<Tuple<Type, Type>, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>
            _constraintsByType;
        private Lazy<Dictionary<Type, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData>>>
            _constraintsByActualType;

        //private static bool AllowSupertype(object actual)
        //{
        //    return actual is XNode || actual is XsFacet;
        //}

        public CompareConstraintFactory()
        {
           resetCache();
        }

        private static readonly AggregateCatalog Catalog = new AggregateCatalog();
        private static readonly CompositionContainer Container = new CompositionContainer(Catalog);
        private static CompareConstraintFactory _instance;

        public static CompareConstraintFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    Catalog.Catalogs.Add(new AssemblyCatalog(Assembly.GetExecutingAssembly()));
                    _instance = Container.GetExportedValue<CompareConstraintFactory>();
                }
                return _instance;
            }
        }

        public static void AddAssembly(Assembly assembly)
        {
            var registration = new RegistrationBuilder();
            registration.ForTypesMatching<ICompareConstraint>(findConstraints).Export(createMetaData);
            Catalog.Catalogs.Add(new AssemblyCatalog(assembly, registration));
        }

        private static bool findConstraints(Type type)
        {
            if (!type.IsClass)
                return false;

            if (type.IsAbstract)
                return false;

            if (!typeof(ICompareConstraint).IsAssignableFrom(type))
                return false;

            return true;
        }

        private static void createMetaData(ExportBuilder exportBuilder)
        {
            exportBuilder.AsContractType<ICompareConstraint>();
            exportBuilder.AddMetadata("ActualType", getActualType);
            exportBuilder.AddMetadata("ExpectedType", getExpectedType);
        }

        private static object getActualType(Type type)
        {
            var property = type.GetProperty("Actual");
            return property.PropertyType;
        }

        private static object getExpectedType(Type type)
        {
            return type.GetProperty("Expected").PropertyType;
        }

        public bool TryCreateConstraint(object expected, object actual, out ICompareConstraint constraint)
        {
            ExportFactory<ICompareConstraint, ICompareConstraintFactoryData> factory;
            if (TryGetFactory(expected.GetType(), actual.GetType(), out factory))
            {
                constraint = factory.CreateExport().Value;
                constraint.Initialize(expected);
                return true;
            }
            constraint = null;
            return false;
        }

        private bool TryGetFactory(Type expectedType, Type actualType, out ExportFactory<ICompareConstraint, ICompareConstraintFactoryData> factory)
        {
            bool allowSupertype = true;
            if (ConstraintsByType.TryGetValue(Tuple.Create(expectedType, actualType), out factory))
                return true;
            
            if (!allowSupertype) 
                return false;

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

        private static bool matchesExpectedTypeOrSuperTypes(Type expectedType, ExportFactory<ICompareConstraint, ICompareConstraintFactoryData> possibleFactory)
        {
            var possibleExpectedType = expectedType;
            while (possibleExpectedType != null)
            {
                if (possibleExpectedType == possibleFactory.Metadata.ExpectedType)
                    return true;

                possibleExpectedType = possibleExpectedType.BaseType;
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
            _constraintsByType =
               Lazy.Create(
                   () =>
                   _factories.ToDictionary(item => Tuple.Create(item.Metadata.ExpectedType, item.Metadata.ActualType)));
            _constraintsByActualType =
                Lazy.Create(
                    () =>
                    _factories.ToDictionary(item => item.Metadata.ActualType));
        }
    }
}