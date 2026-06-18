using NUnit.Framework;

namespace NUnit.Comparisons.Tests;

[TestFixture]
public class MethodConstraintFixture
{
    private class Subject
    {
        public string GetName() => "hello";
        public string GetNameWithPrefix(string prefix) => $"{prefix}hello";
        private string PrivateResult() => "private";
    }

    [Test]
    public void Method_ByName_InvokesMethodAndAppliesInnerConstraint()
    {
        Assert.That(new Subject(), Has.Method("GetName").EqualTo("hello"));
    }

    [Test]
    public void Method_ByLambda_ExtractsNameFromDelegateAndInvokesOnActual()
    {
        // The delegate is only used to obtain the method name; it is not called.
        // Invocation happens on the actual object via reflection.
        var subject = new Subject();
        Assert.That(subject, Has.Method((Func<object>)subject.GetName).EqualTo("hello"));
    }

    [Test]
    public void Method_WithArguments_PassesArgumentsToInvocation()
    {
        Assert.That(new Subject(), Has.Method("GetNameWithPrefix", "pre_").EqualTo("pre_hello"));
    }

    [Test]
    public void Method_PrivateMethod_IsAccessibleViaReflection()
    {
        Assert.That(new Subject(), Has.Method("PrivateResult").EqualTo("private"));
    }

    [Test]
    public void Method_WhenMethodNotFound_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            Assert.That(new Subject(), Has.Method("NonExistent").EqualTo("anything")));

        Assert.That(ex!.Message, Does.Contain("NonExistent"));
    }

    [Test]
    public void Method_WhenActualIsNull_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Assert.That((Subject?)null, Has.Method("GetName").EqualTo("hello")));
    }

    [Test]
    public void Method_WhenInnerConstraintFails_FailureMessageIncludesMethodName()
    {
        var ex = Assert.Throws<AssertionException>(() =>
            Assert.That(new Subject(), Has.Method("GetName").EqualTo("wrong")));

        Assert.That(ex!.Message, Does.Contain("GetName"));
    }
}
