using NUnit.Framework.Constraints;

namespace NUnit.Comparisons
{
    internal sealed class DelegatingConstraintResult : ConstraintResult
    {
        private readonly Action<MessageWriter> _writeMessage;

        public DelegatingConstraintResult(IConstraint constraint, object? actual, bool isSuccess, Action<MessageWriter> writeMessage)
            : base(constraint, actual, isSuccess)
        {
            _writeMessage = writeMessage;
        }

        public override void WriteMessageTo(MessageWriter writer) => _writeMessage(writer);
    }
}
