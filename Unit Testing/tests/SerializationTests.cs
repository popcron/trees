using System;
using System.Numerics;

namespace Scripting.Tests;

public class SerializationTests : Tests
{
    [SetUp]
    public void SetUp()
    {
        ScriptingLibrary.RegisterTypeHandler(new Vector3Serializer());
    }

    [Test]
    public void CustomType()
    {
        Vector3 expected = new(DateTime.Now.Day, -DateTime.Now.Hour, DateTime.Now.Second * 23123.11f);
        SourceCode sourceCode = SourceCode.Create(expected);
        Assert.That(sourceCode.content, Has.Length.GreaterThan(0));
        Assert.That(Vector3Serializer.serialized, Is.True, sourceCode.ToString());
        Value actual = sourceCode.Evaluate(interpreter);
        bool success = actual.TryDeserialize(out Vector3 deserialized);
        Assert.That(success, Is.True);
        Assert.That(Vector3Serializer.deserialized, Is.True, sourceCode.ToString());
        Assert.That(expected, Is.EqualTo(deserialized));
    }

    public class Vector3Serializer : ITypeHandler<Vector3>
    {
        public static bool serialized;
        public static bool deserialized;

        Value ITypeHandler<Vector3>.Serialize(Vector3 value)
        {
            serialized = true;
            return Value.Serialize($"{value.X},{value.Y},{value.Z}");
        }

        Vector3 ITypeHandler<Vector3>.Deserialize(Value value)
        {
            deserialized = true;
            ReadOnlySpan<char> str = value.Deserialize();
            string[] parts = str.ToString().Split(',');
            return new Vector3(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }
    }
}