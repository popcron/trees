using System;

namespace Scripting.Tests;

public class BroadInterpreterTests : Tests
{
    [Test]
    public void UnaryOperators()
    {
        string code = """
            var a = 5
            var b = -a
            var c = !true
            var d = !!false
            var e = -(-7)
            return b + e
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(2));
    }

    [Test]
    public void NegateNonNumberThrows()
    {
        string code = """
            var s = "hi"
            return -s
            """;

        Assert.Throws<CannotNegateNonNumber>(() => Evaluate(code));
    }

    [Test]
    public void InvertNonBooleanThrows()
    {
        string code = """
            return !"hi"
            """;

        Assert.Throws<CannotInvertNonBoolean>(() => Evaluate(code));
    }

    [Test]
    public void LogicalAndOr()
    {
        string code = """
            var t = true
            var f = false
            var a = t && f
            var b = t || f
            var c = (t && t) && !f
            return a == false && b == true && c == true
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    public void ShortCircuitAndDoesNotEvaluateRight()
    {
        string code = """
            fn boom() {
                return 1 / 0
            }

            return false && boom()
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.False);
    }

    [Test]
    public void ShortCircuitOrDoesNotEvaluateRight()
    {
        string code = """
            fn boom() {
                return 1 / 0
            }

            return true || boom()
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    [TestCase(10, 3, 1)]
    [TestCase(20, 5, 0)]
    [TestCase(7, 4, 3)]
    public void Modulus(long a, long b, long expected)
    {
        string code = $"return {a} % {b}";
        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(expected));
    }

    [Test]
    public void ParenthesesOverridePrecedence()
    {
        string code = """
            var a = (1 + 2) * 3
            var b = 1 + 2 * 3
            return a + b
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(9 + 7));
    }

    [Test]
    public void EqualityBetweenStringsAndBooleans()
    {
        string code = """
            var sa = "hello"
            var sb = "hello"
            var sc = "world"
            var ba = true == true
            var bb = true != false
            var s1 = sa == sb
            var s2 = sa != sc
            return ba && bb && s1 && s2
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<bool>(), Is.True);
    }

    [Test]
    public void NullEqualityAndReturn()
    {
        string code = """
            var a = null
            var b = null
            var nope = "x"
            if (a == b && a != nope) {
                return "matched"
            }
            return "fallthrough"
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<string>(), Is.EqualTo("matched"));
    }

    [Test]
    public void IfElseIfChain()
    {
        string code = """
            fn classify(var n) {
                if (n < 0) {
                    return "neg"
                } else if (n == 0) {
                    return "zero"
                } else if (n < 10) {
                    return "small"
                } else {
                    return "big"
                }
            }

            return classify(-3) + "," + classify(0) + "," + classify(5) + "," + classify(100)
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<string>(), Is.EqualTo("neg,zero,small,big"));
    }

    [Test]
    public void VariableReassignment()
    {
        string code = """
            var x = 1
            x = x + 10
            x = x * 2
            return x
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(22));
    }

    [Test]
    public void MemberAssignmentFromOutside()
    {
        string code = """
            struct Point {
                var x
                var y
            }

            var p = new Point(x = 1, y = 2)
            p.x = 99
            p.y = p.x + 1
            return p.x + p.y
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(99 + 100));
    }

    [Test]
    public void ReadingFieldOfNullThrows()
    {
        string code = """
            struct Point {
                var x
            }

            var p = null
            return p.x
            """;

        Assert.Throws<ReadingFieldOfNullInstance>(() => Evaluate(code));
    }

    [Test]
    public void WritingFieldOfNullThrows()
    {
        string code = """
            struct Point {
                var x
            }

            var p = null
            p.x = 5
            return 0
            """;

        Assert.Throws<WritingFieldOfNullInstance>(() => Evaluate(code));
    }

    [Test]
    public void UnknownIdentifierThrows()
    {
        string code = """
            return doesNotExist + 1
            """;

        Assert.Throws<UnknownIdentifier>(() => Evaluate(code));
    }

    [Test]
    public void DuplicateLocalVariableInSameScopeThrows()
    {
        string code = """
            var x = 1
            var x = 2
            return x
            """;

        Assert.Throws<DuplicateLocalVariable>(() => Evaluate(code));
    }

    [Test]
    public void DuplicateTypeDefinitionThrows()
    {
        string code = """
            struct Point { var x }
            struct Point { var y }
            return 0
            """;

        Assert.Throws<DuplicateType>(() => Evaluate(code));
    }

    [Test]
    public void NestedStructInstancesAndFieldChain()
    {
        string code = """
            struct Inner {
                var v
            }

            struct Outer {
                var inner
                var label
            }

            var o = new Outer(inner = new Inner(v = 42), label = "x")
            o.inner.v = o.inner.v + 8
            return o.label + "=" + o.inner.v
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<string>(), Is.EqualTo("x=50"));
    }

    [Test]
    public void NativeFunctionReceivesArgsAndReturnsString()
    {
        string code = """
            return greet("world")
            """;

        interpreter.ClearBindings();
        interpreter.DeclareFunction("greet", (args) =>
        {
            string name = args[0].Deserialize<string>();
            return Value.Serialize("hi, " + name);
        });

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        Assert.That(result.Deserialize<string>(), Is.EqualTo("hi, world"));
        interpreter.ClearBindings();
    }

    [Test]
    public void DuplicateNativeBindingThrows()
    {
        interpreter.ClearBindings();
        interpreter.DeclareFunction("ping", (args) => Value.Serialize(1L));
        Assert.Throws<DuplicateBinding>(() =>
            interpreter.DeclareFunction("ping", (args) => Value.Serialize(2L)));
        interpreter.ClearBindings();
    }

    [Test]
    public void CharacterLiteralReturned()
    {
        string code = """
            var c = 'Z'
            return c
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Character));
        Assert.That(result.Deserialize<char>(), Is.EqualTo('Z'));
    }

    [Test]
    public void FloatArithmeticPreservesFloatType()
    {
        string code = """
            return 1.5 + 2.25
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Float));
        Assert.That(result.Deserialize<double>(), Is.EqualTo(3.75));
    }

    [Test]
    public void IntegerArithmeticPreservesIntegerType()
    {
        string code = """
            return 7 + 8
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.Integer));
        Assert.That(result.Deserialize<long>(), Is.EqualTo(15));
    }

    [Test]
    public void StructWithMethodReferencingOtherStruct()
    {
        string code = """
            struct Vec {
                var x
                var y

                fn lenSq() {
                    return self.x * self.x + self.y * self.y
                }
            }

            struct Box {
                var v

                fn area() {
                    return self.v.lenSq()
                }
            }

            var b = new Box(v = new Vec(x = 3, y = 4))
            return b.area()
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(25));
    }

    [Test]
    [TestCase("+=", 13)]
    [TestCase("-=", 7)]
    [TestCase("*=", 30)]
    [TestCase("/=", 3)]
    [TestCase("%=", 1)]
    public void CompoundAssignmentOnLocal(string op, long expected)
    {
        string code = $"""
            var x = 10
            x {op} 3
            return x
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(expected));
    }

    [Test]
    [TestCase("+=", 13)]
    [TestCase("-=", 7)]
    [TestCase("*=", 30)]
    [TestCase("/=", 3)]
    [TestCase("%=", 1)]
    public void CompoundAssignmentOnMember(string op, long expected)
    {
        string code = $$"""
            struct Box { var n }
            var b = new Box(n = 10)
            b.n {{op}} 3
            return b.n
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(expected));
    }

    [Test]
    public void CompoundPlusEqualConcatenatesString()
    {
        string code = """
            var s = "hi"
            s += ", world"
            return s
            """;

        Value result = Evaluate(code);
        Assert.That(result.type, Is.EqualTo(Value.Type.String));
        Assert.That(result.Deserialize<string>(), Is.EqualTo("hi, world"));
    }

    [Test]
    public void Vector2CompoundAssignmentMutatesByReference()
    {
        string code = """
            struct Vector2 {
                var x
                var y
            }

            var a = new Vector2(x = 1, y = 2)
            var b = a
            b.x += 10
            b.y += 20
            return a.x + "," + a.y + "|" + b.x + "," + b.y
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<string>(), Is.EqualTo("11,22|11,22"));
    }

    [Test]
    public void CompoundAssignmentChained()
    {
        string code = """
            var x = 1
            x += 2
            x *= 4
            x -= 3
            x %= 5
            return x
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(((1 + 2) * 4 - 3) % 5));
    }

    [Test]
    public void DeeplyRecursiveFibonacci()
    {
        string code = """
            fn fib(var n) {
                if (n < 2) {
                    return n
                }
                return fib(n - 1) + fib(n - 2)
            }

            return fib(12)
            """;

        Value result = Evaluate(code);
        Assert.That(result.Deserialize<long>(), Is.EqualTo(144));
    }
}
