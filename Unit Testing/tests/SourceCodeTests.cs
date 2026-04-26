using Scripting;

namespace Scripting.Tests;

public class SourceCodeTests : Tests
{
    [Test]
    public void Strings()
    {
        string original = "Hello, World!";
        SourceCode sourceCode = SourceCode.Create(original);
        Value actual = sourceCode.Evaluate(interpreter);
        Assert.That(actual, Is.EqualTo(Value.Serialize(original)));
    }

    [Test]
    public void Characters()
    {
        char original = 'A';
        SourceCode sourceCode = SourceCode.Create(original);
        Value actual = sourceCode.Evaluate(interpreter);
        Assert.That(actual, Is.EqualTo(Value.Serialize(original)));
    }

    [Test]
    public void Booleans()
    {
        bool original = true;
        SourceCode sourceCode = SourceCode.Create(original);
        Value actual = sourceCode.Evaluate(interpreter);
        Assert.That(actual, Is.EqualTo(Value.Serialize(original)));
    }

    [Test]
    public void LargeFloats()
    {
        double original = 1.234567890123456789;
        SourceCode sourceCode = SourceCode.Create(original);
        Value actual = sourceCode.Evaluate(interpreter);
        Assert.That(actual, Is.EqualTo(Value.Serialize(original)));
    }
}
