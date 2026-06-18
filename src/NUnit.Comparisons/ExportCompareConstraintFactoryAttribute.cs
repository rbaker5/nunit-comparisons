using System;
using System.ComponentModel.Composition;

namespace NUnit.Comparisons;

[MetadataAttribute]
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExportCompareConstraintFactoryAttribute : ExportAttribute, ICompareConstraintFactoryData
{
    public ExportCompareConstraintFactoryAttribute() : base(typeof(ICompareConstraintFactory)) { }
    public required Type ActualType { get; set; }
    public required Type ExpectedType { get; set; }
}
