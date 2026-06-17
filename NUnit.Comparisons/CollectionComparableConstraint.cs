using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    public class CollectionComparableConstraint : CollectionEquivalentConstraint, INestableConstraint
    {
        private NamedCollectionTally _collectionTally;
        private int _level;
        protected IEnumerable<object> Expected { get; private set; }
        public ConstraintComparer Comparer { get; private set; }
        public bool SkipsNewLine { get; set; }
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                Comparer.Level = value + 1;
            }
        }

        

        public CollectionComparableConstraint(IEnumerable<object> expected)
            : base(expected)
        {
            Expected = expected;
            Comparer = new ConstraintComparer {Level = Level + 1};
            Using(Comparer);
        }

        protected override bool doMatch(IEnumerable actual)
        {
            if (Expected is ICollection && actual is ICollection && ((ICollection)actual).Count != ((ICollection)Expected).Count)
                return false;
            _collectionTally = new NamedCollectionTally(Comparer, Expected) { Level = Level };
            return _collectionTally.TryRemove(actual) && _collectionTally.Count == 0;
        }

        public override void WriteMessageTo(MessageWriter writer)
        {
            if (!SkipsNewLine) writer.WriteIndent(Level);
            writer.WriteLine("Collections did not match");
            _collectionTally.WriteMessageTo(writer);
        }
    }
}