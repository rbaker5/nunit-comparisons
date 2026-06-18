namespace NUnit.Comparisons;

/// <summary>
/// Marks a constraint as participating in the indented failure-message tree.
/// </summary>
/// <remarks>
/// When constraints are nested (e.g. a property constraint wrapping a compare
/// constraint wrapping a collection constraint), each level writes its failure
/// message indented one step deeper than its parent. <see cref="Level"/> tracks
/// the current depth and <see cref="SkipsNewLine"/> suppresses the leading
/// newline when a parent has already positioned the cursor correctly.
///
/// Implementors must propagate both properties inward to any wrapped constraints
/// so the whole tree stays aligned.
/// </remarks>
public interface INestableConstraint
{
    /// <summary>Indentation depth for failure messages. 0 = top-level.</summary>
    int Level { get; set; }

    /// <summary>
    /// When true, the constraint omits its leading newline/indent because the
    /// parent constraint has already written a prefix on the current line.
    /// </summary>
    bool SkipsNewLine { get; set; }
}
