using System;
using NUnit.Framework.Constraints;

namespace NUnit.Comparisons;

public static class MessageWriterExtensions
{
    public static void WriteIndent(this MessageWriter writer, int level)
    {
        while (level-- >= 0)
            writer.Write("  ");
    }
}
