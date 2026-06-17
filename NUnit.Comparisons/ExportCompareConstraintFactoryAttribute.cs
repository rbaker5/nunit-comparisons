using System;
using System.ComponentModel.Composition;

namespace NUnit.Comparisons
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportCompareConstraintFactoryAttribute : ExportAttribute, ICompareConstraintFactoryData
    {
        public ExportCompareConstraintFactoryAttribute() : base(typeof(ICompareConstraintFactory)) { }
        public Type ActualType { get; set; }
        public Type ExpectedType { get; set; }
    }
}