using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

/// <summary>
/// Tracks unmatched items from both sides of an unordered collection comparison,
/// recording each item's best-seen match quality so failure messages can distinguish
/// close misses from non-starters.
/// </summary>
/// <remarks>
/// <para>
/// Every item begins at <see cref="MatchState.NoComparison"/> and is upgraded toward
/// <see cref="MatchState.Found"/> as comparisons proceed. The state recorded for each
/// item is the <em>best</em> it achieved against any candidate on the other side:
/// </para>
/// <list type="table">
///   <listheader><term>State</term><description>Meaning</description></listheader>
///   <item><term>NoComparison</term><description>No constraint is registered for this type pair — the item cannot be inspected at all.</description></item>
///   <item><term>NameNotMatched</term><description>The type is comparable but no candidate with a matching name was found.</description></item>
///   <item><term>MatchNotFound</term><description>A name match was found but the full comparison failed — a close miss.</description></item>
///   <item><term>Found</term><description>A full match was found; the item is removed from both tallies.</description></item>
/// </list>
/// <para>
/// <see cref="WriteMessageTo"/> emits a separate message for each non-<c>Found</c>
/// category. A "name matched, wrong content" failure appears separately from "no name
/// match", which appears separately from "type not registered at all" — giving the test
/// author the most specific diagnosis available.
/// </para>
/// </remarks>
public class NamedCollectionTally
{
    /// <summary>
    /// Match quality between one item and its best candidate on the other side.
    /// Enum values are ordered: higher means closer to a full match. <see cref="Max"/>
    /// uses this ordering to accumulate the best quality seen across all comparisons.
    /// </summary>
    private enum MatchState
    {
        /// <summary>No registered constraint exists for this type pair; items are incomparable.</summary>
        NoComparison,
        /// <summary>A constraint exists but no candidate with a matching name was found.</summary>
        NameNotMatched,
        /// <summary>A name match was found but the full content comparison failed.</summary>
        MatchNotFound,
        /// <summary>A full match was found; item removed from both tallies.</summary>
        Found
    }

    private readonly Dictionary<MatchState, List<object>> _actualNotMatched;
    private readonly Dictionary<MatchState, List<object>> _expectedNotMatched;
    public int Level { get; set; }

    private readonly ConstraintComparer _comparer;

    public int Count
    {
        get { return _expectedNotMatched.Sum(pair => pair.Value.Count); }
    }

    public NamedCollectionTally(ConstraintComparer comparer, IEnumerable<object> actual)
    {
        _actualNotMatched = new Dictionary<MatchState, List<object>>();
        _expectedNotMatched = new Dictionary<MatchState, List<object>> {{MatchState.NoComparison, actual.ToList()}};

        _comparer = comparer;
    }

    /// <summary>
    /// Attempts to match <paramref name="actual"/> against a remaining expected item,
    /// removing the pair from both tallies on success.
    /// </summary>
    /// <remarks>
    /// Scans every unmatched expected item, comparing against <paramref name="actual"/>
    /// and accumulating the best <see cref="MatchState"/> reached. Scanning stops as
    /// soon as a <see cref="MatchState.Found"/> match is located — remaining expected
    /// items are carried forward at their existing state. Both the actual item and each
    /// expected item are upgraded to the maximum state seen, so an expected item whose
    /// name matched a different actual item retains <see cref="MatchState.NameNotMatched"/>
    /// even if no full match was ever found for it.
    /// </remarks>
    public bool TryRemove(object actual)
    {
        if (actual == null)
            return tryRemoveNull();

        var actualMatchState = MatchState.NoComparison;
        var allNotMatched =
            _expectedNotMatched.SelectMany(
                pair => pair.Value.Select(item => new KeyValuePair<MatchState, object>(pair.Key, item))).ToList();

        _expectedNotMatched.Clear();
        foreach (var expected in allNotMatched)
        {
            if (actualMatchState == MatchState.Found)
            {
                addToTally(_expectedNotMatched, expected.Key, expected.Value);
            }
            else
            {
                var thisMatchState = compare(actual, expected.Value);
                actualMatchState = Max(actualMatchState, thisMatchState);
                var expectedMatchState = Max(expected.Key, thisMatchState);
                addToTally(_expectedNotMatched, expectedMatchState, expected.Value);
            }
        }

        addToTally(_actualNotMatched, actualMatchState, actual);
        return actualMatchState == MatchState.Found;
    }

    private MatchState compare(object actual, object expected)
    {
        var thisMatchState = MatchState.NoComparison;
        if (CanCompare(expected, actual))
        {
            thisMatchState = MatchState.NameNotMatched;
            if (NamesEqual(expected, actual))
            {
                thisMatchState = MatchState.MatchNotFound;
                if (ItemsEqual(expected, actual))
                    thisMatchState = MatchState.Found;
            }
        }
        return thisMatchState;
    }

    private static void addToTally(Dictionary<MatchState, List<object>> tally, MatchState matchState, object? value)
    {
        if (matchState == MatchState.Found)
            return;

        if (!tally.TryGetValue(matchState, out var list))
        {
            list = [];
            tally.Add(matchState, list);
        }
        list.Add(value!);
    }

    private MatchState Max(MatchState state1, MatchState state2)
    {
        return state1 > state2 ? state1 : state2;
    }

    private bool tryRemoveNull()
    {
        var possibleItems = _expectedNotMatched[MatchState.NoComparison];
        for (int index = 0; index < possibleItems.Count; ++index)
        {
            if (possibleItems[index] == null)
            {
                possibleItems.RemoveAt(index);
                return true;
            }
        }
        addToTally(_actualNotMatched, MatchState.NoComparison, null);
        return false;
    }

    private bool ItemsEqual(object expected, object actual)
    {
        return _comparer.Equals(expected, actual);
    }

    private bool NamesEqual(object expected, object actual)
    {
        return _comparer.NameEquals(expected, actual);
    }

    private bool CanCompare(object expected, object actual)
    {
        return _comparer.CanCompare(expected, actual);
    }

    public bool TryRemove(IEnumerable actual)
    {
        bool allRemoved = true;
        foreach (var actualItem in actual)
        {
            if (!TryRemove(actualItem))
                allRemoved = false;
        }
        return allRemoved;
    }

    /// <summary>
    /// Writes a failure message categorised by match quality, from most-specific to least.
    /// </summary>
    /// <remarks>
    /// Four message categories are emitted in order:
    /// <list type="number">
    ///   <item>Expected items with no comparable type registered.</item>
    ///   <item>Expected items whose type is comparable but whose name was never matched.</item>
    ///   <item>Unexpected actual items with no comparable type registered.</item>
    ///   <item>Unexpected actual items whose name was never matched.</item>
    /// </list>
    /// Items at <see cref="MatchState.MatchNotFound"/> (name matched, content differed)
    /// are not listed here — their failure detail is written by
    /// <see cref="ConstraintComparer.WriteMessageTo"/> which follows.
    /// </remarks>
    public void WriteMessageTo(MessageWriter writer)
    {
        writeNotFound(writer, _expectedNotMatched, MatchState.NoComparison, "Did not find any potential value for expected values: ");
        writeNotFound(writer, _expectedNotMatched, MatchState.NameNotMatched, "Did not find any name match for expected values: ");
        writeNotFound(writer, _actualNotMatched, MatchState.NoComparison, "Found unexpected values without any potential match : ");
        writeNotFound(writer, _actualNotMatched, MatchState.NameNotMatched, "Found unexpected values without any name match : ");
        _comparer.WriteMessageTo(writer);
    }

    private void writeNotFound(MessageWriter writer, Dictionary<MatchState, List<object>> dictionary, MatchState matchState, string message)
    {
        if (!dictionary.TryGetValue(matchState, out var list) || list.Count <= 0) return;

        writer.WriteIndent(Level);
        writer.Write(message);
        foreach (var value in list)
        {
            writer.Write(value.GetType().Name + ":");
            writer.WriteValue(value);
        }
        writer.WriteLine();
    }
}
