namespace Scripting.Tests;

public class ValueTests
{
    [Test]
    public void ConstructorsSetCorrectTypes()
    {
        Value boolean = Value.Serialize(true);
        Assert.That(boolean.type, Is.EqualTo(Value.Type.Boolean));
        Assert.That(boolean.Deserialize<bool>(), Is.EqualTo(true));

        Value character = Value.Serialize('a');
        Assert.That(character.type, Is.EqualTo(Value.Type.Character));
        Assert.That(character.Deserialize<char>(), Is.EqualTo('a'));

        Value text = Value.Serialize("hello");
        Assert.That(text.type, Is.EqualTo(Value.Type.String));
        Assert.That(text.Deserialize<string>(), Is.EqualTo("hello"));

        Value number = Value.Serialize(3.14);
        Assert.That(number.type, Is.EqualTo(Value.Type.Float));
        Assert.That(number.Deserialize<double>(), Is.EqualTo(3.14));

        Value integer = Value.Serialize(42L);
        Assert.That(integer.type, Is.EqualTo(Value.Type.Integer));
        Assert.That(integer.Deserialize<long>(), Is.EqualTo(42L));
    }

    [Test]
    public void DefaultNotTheSameAsNull()
    {
        Value defaultValue = default;
        Value nullValue = Value.Null;
        Assert.That(defaultValue, Is.Not.EqualTo(nullValue));

        Value missingObjectValue = Value.Serialize((ObjectInstance)null!);
        Assert.That(missingObjectValue, Is.EqualTo(nullValue));
        Assert.That(missingObjectValue, Is.Not.EqualTo(defaultValue));
    }

    [Test]
    public void BooleanStrings()
    {
        Value boolean = Value.Serialize(true);
        Assert.That(boolean.ToString(), Is.EqualTo(KeywordMap.True));

        boolean = Value.Serialize(false);
        Assert.That(boolean.ToString(), Is.EqualTo(KeywordMap.False));
    }

    [Test]
    public void NullStrings()
    {
        Value validString = Value.Serialize("hello");
        Assert.That(validString.ToString(), Is.EqualTo("hello"));

        Value nullString = Value.Serialize((string)null!);
        Assert.That(nullString.ToString(), Is.EqualTo(string.Empty));

        Value emptyString = Value.Serialize(string.Empty);
        Assert.That(emptyString.ToString(), Is.EqualTo(string.Empty));
    }
}
