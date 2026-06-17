using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class NamedCollectionTally
    {
        private enum MatchState
        {
            NoComparison,
            NameNotMatched,
            MatchNotFound,
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

        private static void addToTally(Dictionary<MatchState, List<object>> tally, MatchState matchState, object value)
        {
            if (matchState == MatchState.Found)
                return;

            List<object> list;
            if (!tally.TryGetValue(matchState, out list))
            {
                list = new List<object>();
                tally.Add(matchState, list);
            }
            list.Add(value);
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
            List<object> list;
            if (!dictionary.TryGetValue(matchState, out list) || list.Count <= 0) return;

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
}
