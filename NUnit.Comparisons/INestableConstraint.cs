using System;

namespace NUnit.Comparisons;

public interface INestableConstraint
{
    int Level { get; set; }
    bool SkipsNewLine { get; set; }
}
