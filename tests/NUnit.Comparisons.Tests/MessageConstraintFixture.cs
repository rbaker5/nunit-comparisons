using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class MessageConstraintFixture
{
    [Test]
    public void WhenInnerConstraintPasses_Passes()
    {
        Assert.That((object?)null, Is.Null.WithMessage("Should be null"));
    }

    [Test]
    public void WhenInnerConstraintFails_FailureMessageContainsCustomText()
    {
        var ex = Assert.Throws<AssertionException>(() =>
            Assert.That("hello", Is.Null.WithMessage("Should be null")));

        Assert.That(ex!.Message, Does.Contain("Should be null"));
    }

    [Test]
    public void WithMessage_FormatsArgsIntoMessage()
    {
        var ex = Assert.Throws<AssertionException>(() =>
            Assert.That(42, Is.Null.WithMessage("Expected {0} to be absent", "the value")));

        Assert.That(ex!.Message, Does.Contain("Expected the value to be absent"));
    }
}
