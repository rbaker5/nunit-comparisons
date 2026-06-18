using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class CastConstraintFixture
{
    [Test]
    public void Cast_WhenActualIsAssignableToT_AppliesInnerConstraint()
    {
        object actual = "hello";
        Assert.That(actual, Has.Cast<string>().EqualTo("hello"));
    }

    [Test]
    public void Cast_WhenActualIsNotCastableToT_FailsWithIncompatibleTypeMessage()
    {
        var ex = Assert.Throws<AssertionException>(() =>
            Assert.That(42, Has.Cast<string>().EqualTo("hello")));

        Assert.That(ex!.Message, Does.Contain("cannot be cast to"));
        Assert.That(ex.Message, Does.Contain("Int32"));
        Assert.That(ex.Message, Does.Contain("String"));
    }

    [Test]
    public void Cast_WhenCastSucceedsButInnerConstraintFails_ReportsInnerFailure()
    {
        var ex = Assert.Throws<AssertionException>(() =>
            Assert.That((object)"world", Has.Cast<string>().EqualTo("hello")));

        Assert.That(ex!.Message, Does.Not.Contain("cannot be cast to"));
        Assert.That(ex.Message, Does.Contain("hello"));
    }
}
